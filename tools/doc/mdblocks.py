# -*- coding: utf-8 -*-
"""Mô hình khối Markdown dùng chung cho docx_write.py và docx_read.py.

Cả hai chiều phải dùng ĐÚNG MỘT định nghĩa khối. Nếu bên ghi hiểu "bảng" một kiểu
và bên đọc hiểu một kiểu khác thì khứ hồi sẽ hỏng âm thầm — và thứ hỏng ở đây là
nguồn sự thật số 1 của dự án (AGENTS.md), nên nó phải hỏng ồn ào hoặc không hỏng.

Chỉ dùng thư viện chuẩn. Không có mạng để cài gì thêm.

Khối (dict):
    {'t': 'heading', 'level': 1..6, 'spans': [...]}
    {'t': 'para',    'spans': [...]}
    {'t': 'quote',   'lines': [[span, ...], ...]}       # các dòng '> ' liền nhau
    {'t': 'code',    'lang': str, 'lines': [str, ...]}
    {'t': 'list',    'items': [{'level': int, 'ordered': bool, 'spans': [...]}, ...]}
    {'t': 'table',   'header': [[span, ...], ...],
                     'align':  ['left'|'center'|'right', ...],
                     'rows':   [[[span, ...], ...], ...]}
    {'t': 'hr'}

Span: {'text': str, 'b': bool, 'i': bool, 'c': bool}   # đậm / nghiêng / mã
"""

import re

MAX_LIST_LEVEL = 2          # ilvl 0..2, tức 3 mức lồng
INDENT_PER_LEVEL = 2        # Markdown: 2 dấu cách mỗi mức


# --------------------------------------------------------------------------- inline

def span(text, b=False, i=False, c=False):
    return {'text': text, 'b': b, 'i': i, 'c': c}


def parse_inline(text):
    """Tách một dòng thành các span.

    Thứ tự ưu tiên: `mã` trước, vì bên trong mã không có định dạng nào khác —
    đó là quy tắc của chính Markdown, không phải lựa chọn tiện tay.
    """
    spans = []
    pos = 0
    pattern = re.compile(
        r'`([^`]+)`'                      # 1: mã
        r'|\*\*(.+?)\*\*'                 # 2: đậm
        r'|(?<!\*)\*(?!\*)([^*\n]+?)\*(?!\*)'   # 3: nghiêng, không nuốt nhầm **
        r'|(?<![A-Za-z0-9_])_([^_\n]+?)_(?![A-Za-z0-9_])'   # 4: nghiêng kiểu _
    )

    for m in pattern.finditer(text):
        if m.start() > pos:
            spans.append(span(text[pos:m.start()]))
        if m.group(1) is not None:
            spans.append(span(m.group(1), c=True))
        elif m.group(2) is not None:
            spans.extend(_nested(m.group(2), b=True))
        elif m.group(3) is not None:
            spans.append(span(m.group(3), i=True))
        else:
            spans.append(span(m.group(4), i=True))
        pos = m.end()

    if pos < len(text):
        spans.append(span(text[pos:]))

    return spans or [span('')]


def _nested(text, b=False, i=False):
    """Định dạng lồng một mức: **chữ đậm có `mã` bên trong**."""
    out = []
    for s in parse_inline(text):
        out.append(span(s['text'], b=b or s['b'], i=i or s['i'], c=s['c']))
    return out


def render_inline(spans):
    """Dựng lại chuỗi Markdown từ các span.

    Phải dựng theo VÙNG chứ không theo từng span: một đoạn đậm có `mã` bên trong bị tách
    thành ba span (đậm / đậm+mã / đậm), và nếu bọc dấu cho từng span thì ra
    `**a**` + `**`b`**` + `**c**` — đúng về định dạng nhưng khác chữ gốc.

    Bản đầu giải chuyện đó bằng cách dựng thô rồi xoá các cặp dấu dính nhau. Cách đó SAI
    và sai âm thầm: vòng xoá chạy tới dấu nghiêng `*` thì `'*' + '*'` chính là `'**'`,
    nên nó xoá sạch MỌI dấu đậm trong tài liệu. 64 dấu đậm của docs/design.md, gồm mọi
    nhãn **[ĐỀ XUẤT]** và **[MVP]**, biến mất khỏi bản chiếu — mà cả hai phép kiểm đều
    mù với nó, vì một bên bóc dấu cú pháp còn bên kia chạy cùng hàm này lên cả hai phía.

    Cách dưới đây gom các span liền nhau cùng cờ thành một vùng rồi bọc dấu quanh vùng,
    lồng dần theo thứ tự đậm → nghiêng → mã. Không có dấu thừa nào sinh ra, nên không
    cần xoá gì.
    """
    spans = [s for s in spans if s['text'] != '']
    if not spans:
        return ''
    return _render_group(spans, 0)


_LEVELS = (('b', '**'), ('i', '*'), ('c', '`'))


def _render_group(spans, depth):
    if depth >= len(_LEVELS):
        return ''.join(s['text'] for s in spans)

    key, marker = _LEVELS[depth]
    out = []
    i = 0
    n = len(spans)

    while i < n:
        flag = spans[i][key]
        j = i
        while j < n and spans[j][key] == flag:
            j += 1

        inner = _render_group(spans[i:j], depth + 1)
        out.append(marker + inner + marker if flag and inner else inner)
        i = j

    return ''.join(out)


def spans_text(spans):
    return ''.join(s['text'] for s in spans)


# --------------------------------------------------------------------------- parse

_HEADING = re.compile(r'^(#{1,6})\s+(.*)$')
_BULLET = re.compile(r'^(\s*)[-*]\s+(.*)$')
_ORDERED = re.compile(r'^(\s*)(\d+)[.)]\s+(.*)$')
_FENCE = re.compile(r'^```(.*)$')
_HR = re.compile(r'^(?:---+|\*\*\*+|___+)\s*$')


def parse_markdown(text):
    lines = text.replace('\r\n', '\n').replace('\r', '\n').split('\n')
    blocks = []
    i = 0
    n = len(lines)

    while i < n:
        line = lines[i]

        if not line.strip():
            i += 1
            continue

        m = _FENCE.match(line)
        if m:
            lang = m.group(1).strip()
            body = []
            i += 1
            while i < n and not lines[i].startswith('```'):
                body.append(lines[i])
                i += 1
            i += 1                       # bỏ hàng rào đóng
            blocks.append({'t': 'code', 'lang': lang, 'lines': body})
            continue

        m = _HEADING.match(line)
        if m:
            blocks.append({'t': 'heading',
                           'level': len(m.group(1)),
                           'spans': parse_inline(m.group(2).strip())})
            i += 1
            continue

        if _HR.match(line) and not line.startswith('|'):
            blocks.append({'t': 'hr'})
            i += 1
            continue

        if line.startswith('|'):
            table, i = _parse_table(lines, i)
            if table is not None:
                blocks.append(table)
                continue

        if line.startswith('>'):
            quote_lines = []
            while i < n and lines[i].startswith('>'):
                quote_lines.append(parse_inline(lines[i][1:].lstrip(' ')))
                i += 1
            blocks.append({'t': 'quote', 'lines': quote_lines})
            continue

        if _BULLET.match(line) or _ORDERED.match(line):
            items = []
            while i < n:
                mb = _BULLET.match(lines[i])
                mo = _ORDERED.match(lines[i])
                if mb:
                    level = min(len(mb.group(1)) // INDENT_PER_LEVEL, MAX_LIST_LEVEL)
                    items.append({'level': level, 'ordered': False,
                                  'spans': parse_inline(mb.group(2))})
                elif mo:
                    level = min(len(mo.group(1)) // INDENT_PER_LEVEL, MAX_LIST_LEVEL)
                    items.append({'level': level, 'ordered': True,
                                  'spans': parse_inline(mo.group(3))})
                else:
                    break
                i += 1
            blocks.append({'t': 'list', 'items': items})
            continue

        # Đoạn văn: gom các dòng liền nhau cho tới dòng trắng hoặc một khối khác.
        para = [line]
        i += 1
        while i < n and lines[i].strip() and not _starts_block(lines[i]):
            para.append(lines[i])
            i += 1
        blocks.append({'t': 'para', 'spans': parse_inline(' '.join(x.strip() for x in para))})

    return blocks


def _starts_block(line):
    return bool(
        _HEADING.match(line) or _FENCE.match(line) or _BULLET.match(line)
        or _ORDERED.match(line) or line.startswith('|') or line.startswith('>')
        or _HR.match(line))


def _split_row(line):
    s = line.strip()
    if s.startswith('|'):
        s = s[1:]
    if s.endswith('|'):
        s = s[:-1]
    return [c.strip() for c in s.split('|')]


def _parse_table(lines, i):
    """Bảng cần ít nhất hàng tiêu đề + hàng ngăn cách, nếu không thì không phải bảng."""
    if i + 1 >= len(lines):
        return None, i

    header = _split_row(lines[i])
    sep = _split_row(lines[i + 1])

    if not sep or not all(re.match(r'^:?-{1,}:?$', c) for c in sep if c != ''):
        return None, i

    align = []
    for c in sep:
        left = c.startswith(':')
        right = c.endswith(':')
        align.append('center' if left and right else 'right' if right else 'left')

    rows = []
    j = i + 2
    while j < len(lines) and lines[j].startswith('|'):
        rows.append([parse_inline(c) for c in _split_row(lines[j])])
        j += 1

    return ({'t': 'table',
             'header': [parse_inline(c) for c in header],
             'align': align,
             'rows': rows}, j)


# --------------------------------------------------------------------------- render

_ALIGN_MARK = {'left': '---', 'center': ':---:', 'right': '---:'}


def render_markdown(blocks):
    out = []

    for b in blocks:
        t = b['t']

        if t == 'heading':
            out.append('#' * b['level'] + ' ' + render_inline(b['spans']))
        elif t == 'para':
            out.append(render_inline(b['spans']))
        elif t == 'hr':
            out.append('---')
        elif t == 'quote':
            for line in b['lines']:
                out.append('> ' + render_inline(line))
        elif t == 'code':
            out.append('```' + (b['lang'] or ''))
            out.extend(b['lines'])
            out.append('```')
        elif t == 'list':
            counters = {}
            for item in b['items']:
                level = item['level']
                pad = ' ' * (level * INDENT_PER_LEVEL)
                if item['ordered']:
                    counters[level] = counters.get(level, 0) + 1
                    out.append('%s%d. %s' % (pad, counters[level], render_inline(item['spans'])))
                else:
                    counters.pop(level, None)
                    out.append('%s- %s' % (pad, render_inline(item['spans'])))
        elif t == 'table':
            out.append('| ' + ' | '.join(render_inline(c) for c in b['header']) + ' |')
            out.append('|' + '|'.join(_ALIGN_MARK[a] for a in b['align']) + '|')
            for row in b['rows']:
                out.append('| ' + ' | '.join(render_inline(c) for c in row) + ' |')
        else:
            raise ValueError('Khối lạ: %r' % (t,))

        out.append('')

    while out and out[-1] == '':
        out.pop()

    return '\n'.join(out) + '\n'


# --------------------------------------------------------------------------- so sánh

def normalise(text):
    """Dạng chuẩn để so hai bản Markdown là "tương đương về NỘI DUNG".

    Chỉ bỏ khác biệt về khoảng trắng và cách căn bảng. KHÔNG bao giờ bỏ chữ —
    nếu một chữ rụng mất thì bản chuẩn phải khác nhau, đó là toàn bộ công dụng của nó.
    """
    return render_markdown(parse_markdown(text))
