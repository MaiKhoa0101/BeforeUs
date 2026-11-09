# -*- coding: utf-8 -*-
""".docx -> Markdown, và kiểm băm chống trôi.

    python tools/doc/docx_read.py docs/design.docx docs/design.md
    python tools/doc/docx_read.py --check docs/design.docx

Đây là thứ giữ cho agent còn đọc được nguồn sự thật số 1 (AGENTS.md) sau khi tài liệu
chuyển sang docx: docx là nhị phân, không grep được, và dự án không có git nên cũng
không diff được.

LỆNH --check LÀ PHẦN QUAN TRỌNG NHẤT. Nó so sha256 của docx hiện tại với con số đã ghi
trong file đồng bộ. Lệch nghĩa là docx đã được sửa trong Word mà bản chiếu .md chưa sinh
lại — và một bản chiếu cũ trông vẫn hợp lệ là cái bẫy nguy hiểm nhất ở đây, vì nó là
luật chơi chứ không phải ghi chú.
"""

import argparse
import hashlib
import io
import json
import os
import re
import sys
import zipfile
import xml.etree.ElementTree as ET

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import docxparts as P
import mdblocks as M

W = '{%s}' % P.W

_LANG_MARKER = re.compile(r'^⟦lang:([^⟧]*)⟧')


def q(tag):
    return W + tag


# --------------------------------------------------------------------------- run -> span

def _run_spans(r):
    """Một <w:r> thành các span. <w:br/> thành '\\n' để bên gọi tách dòng."""
    rpr = r.find(q('rPr'))
    bold = italic = code = False
    hidden = False

    if rpr is not None:
        bold = rpr.find(q('b')) is not None
        italic = rpr.find(q('i')) is not None
        hidden = rpr.find(q('vanish')) is not None
        style = rpr.find(q('rStyle'))
        code = style is not None and style.get(q('val')) == 'InlineCode'

    out = []
    for child in r:
        if child.tag == q('t'):
            out.append(M.span(child.text or '', b=bold, i=italic, c=code))
        elif child.tag in (q('br'), q('cr')):
            out.append(M.span('\n'))
        elif child.tag == q('tab'):
            out.append(M.span('\t'))

    if hidden:
        for s in out:
            s['hidden'] = True

    return out


def _paragraph_spans(p):
    out = []
    for child in p:
        if child.tag == q('r'):
            out.extend(_run_spans(child))
        elif child.tag in (q('hyperlink'), q('smartTag'), q('ins')):
            for r in child.iter(q('r')):
                out.extend(_run_spans(r))
        elif child.tag == q('fldSimple'):
            # Trường của Word (mục lục chẳng hạn) không phải nội dung tài liệu.
            continue
    return out


def _split_lines(spans):
    """Tách danh sách span theo '\\n' thành danh sách dòng."""
    lines = [[]]
    for s in spans:
        if '\n' in s['text']:
            pieces = s['text'].split('\n')
            for idx, piece in enumerate(pieces):
                if idx:
                    lines.append([])
                if piece:
                    lines[-1].append(M.span(piece, s['b'], s['i'], s['c']))
        else:
            lines[-1].append(s)
    return lines


def _clean(spans):
    return [s for s in spans if s['text'] != '' and not s.get('hidden')]


# --------------------------------------------------------------------------- docx -> khối

def _para_info(p):
    ppr = p.find(q('pPr'))
    style = None
    numid = None
    ilvl = 0
    jc = None

    if ppr is not None:
        pstyle = ppr.find(q('pStyle'))
        if pstyle is not None:
            style = pstyle.get(q('val'))

        numpr = ppr.find(q('numPr'))
        if numpr is not None:
            nid = numpr.find(q('numId'))
            lvl = numpr.find(q('ilvl'))
            if nid is not None:
                numid = int(nid.get(q('val')))
            if lvl is not None:
                ilvl = int(lvl.get(q('val')))

        j = ppr.find(q('jc'))
        if j is not None:
            jc = j.get(q('val'))

    return style, numid, ilvl, jc


def _table_blocks(tbl):
    rows = []
    aligns = None

    for tr in tbl.findall(q('tr')):
        cells = []
        row_align = []
        for tc in tr.findall(q('tc')):
            spans = []
            for p in tc.findall(q('p')):
                spans.extend(_paragraph_spans(p))
                _, _, _, jc = _para_info(p)
            row_align.append(jc)
            cells.append(_clean(spans))
        rows.append(cells)
        if aligns is None:
            aligns = row_align

    if not rows:
        return None

    # Không bóc đậm ở đây: đậm trang trí của ô tiêu đề nằm trong style đoạn
    # TableHeaderCell, mà _run_spans chỉ đọc w:b trực tiếp trên run. Nên thứ còn lại
    # đúng là đậm do tác giả viết, và phải giữ.
    header = rows[0]
    align = ['center' if a == 'center' else 'right' if a == 'right' else 'left'
             for a in (aligns or [])]

    return {'t': 'table', 'header': header, 'align': align, 'rows': rows[1:]}


def docx_to_blocks(path):
    with zipfile.ZipFile(path) as z:
        xml = z.read('word/document.xml')

    root = ET.fromstring(xml)
    body = root.find(q('body'))
    blocks = []
    pending_list = None

    for node in body:
        if node.tag == q('tbl'):
            pending_list = None
            table = _table_blocks(node)
            if table is not None:
                blocks.append(table)
            continue

        if node.tag != q('p'):
            continue

        style, numid, ilvl, _ = _para_info(node)
        spans = _paragraph_spans(node)

        if numid is not None:
            item = {'level': ilvl,
                    'ordered': numid == P.NUM_ID_ORDERED,
                    'spans': _clean(spans)}
            if pending_list is None:
                pending_list = {'t': 'list', 'items': []}
                blocks.append(pending_list)
            pending_list['items'].append(item)
            continue

        pending_list = None

        if style == 'SourceCode':
            lang = ''
            for s in spans:
                if s.get('hidden'):
                    m = _LANG_MARKER.match(s['text'])
                    if m:
                        lang = m.group(1)
            lines = [M.spans_text(_clean(line)) for line in _split_lines(spans)]
            blocks.append({'t': 'code', 'lang': lang, 'lines': lines})
            continue

        if style == 'Quote':
            blocks.append({'t': 'quote',
                           'lines': [_clean(line) for line in _split_lines(spans)]})
            continue

        cleaned = _clean(spans)

        if style and style.startswith('Heading') and style[7:].isdigit():
            if cleaned:
                blocks.append({'t': 'heading', 'level': int(style[7:]), 'spans': cleaned})
            continue

        if style == 'TOCHeading':
            continue                      # "Mục lục" do công cụ chèn, không phải nội dung

        if style == 'Title':
            if cleaned:
                blocks.append({'t': 'heading', 'level': 1, 'spans': cleaned})
            continue

        if not cleaned:
            # Đoạn rỗng: đoạn kê sau bảng, hoặc đường kẻ ngang.
            ppr = node.find(q('pPr'))
            if ppr is not None and ppr.find(q('pBdr')) is not None:
                blocks.append({'t': 'hr'})
            continue

        blocks.append({'t': 'para', 'spans': cleaned})

    return blocks


def docx_to_markdown(path):
    return M.render_markdown(docx_to_blocks(path))


# --------------------------------------------------------------------------- chống trôi

def sha256_of(path):
    h = hashlib.sha256()
    with open(path, 'rb') as f:
        for chunk in iter(lambda: f.read(65536), b''):
            h.update(chunk)
    return h.hexdigest()


def default_sync_path(docx_path):
    base, _ = os.path.splitext(docx_path)
    return base + '.sync.json'


def check(docx_path, sync_path=None):
    """0 = khớp, 2 = lệch hoặc thiếu. Trả mã thoát để gọi được từ script."""
    sync_path = sync_path or default_sync_path(docx_path)

    if not os.path.exists(docx_path):
        print('LỖI: không thấy %s' % docx_path)
        return 2

    if not os.path.exists(sync_path):
        print('LỖI: không thấy file đồng bộ %s' % sync_path)
        print('      Chưa có gì để so. Sinh lại docx kèm --sync, hoặc tạo file đồng bộ.')
        return 2

    with io.open(sync_path, encoding='utf-8') as f:
        payload = json.load(f)

    actual = sha256_of(docx_path)
    expected = payload.get('sha256')

    if actual == expected:
        print('KHỚP. %s đúng bản đã ghi trong %s.' % (
            os.path.basename(docx_path), os.path.basename(sync_path)))
        print('Bản chiếu Markdown cạnh nó tin được.')
        return 0

    print('LỆCH — ĐỪNG TIN BẢN CHIẾU MARKDOWN.')
    print('  docx hiện tại : %s' % actual)
    print('  đã ghi        : %s' % expected)
    print('  ghi lúc       : %s' % payload.get('generated_at'))
    print('')
    print('Ai đó đã sửa %s trong Word mà chưa sinh lại bản chiếu.' % os.path.basename(docx_path))
    print('Bản .md cạnh nó đang mô tả một tài liệu CŨ HƠN. Sinh lại trước khi đọc:')
    print('  python tools/doc/docx_read.py %s <đường-dẫn>.md --sync %s'
          % (docx_path, sync_path))
    return 2


def main(argv=None):
    ap = argparse.ArgumentParser(description='.docx -> Markdown, và kiểm băm chống trôi')
    ap.add_argument('docx', help='file .docx')
    ap.add_argument('output', nargs='?', help='file Markdown sinh ra')
    ap.add_argument('--check', action='store_true',
                    help='chỉ so băm với file đồng bộ, không sinh gì')
    ap.add_argument('--sync', help='file đồng bộ; mặc định <tên-docx>.sync.json')
    args = ap.parse_args(argv)

    if args.check:
        return check(args.docx, args.sync)

    if not args.output:
        ap.error('thiếu đường dẫn file Markdown sinh ra (hoặc dùng --check)')

    markdown = docx_to_markdown(args.docx)

    with io.open(args.output, 'w', encoding='utf-8', newline='\n') as f:
        f.write(markdown)

    print('Đã ghi %s (%d dòng)' % (args.output, markdown.count('\n')))

    sync_path = args.sync or default_sync_path(args.docx)
    if os.path.exists(sync_path):
        with io.open(sync_path, encoding='utf-8') as f:
            payload = json.load(f)
        payload['sha256'] = sha256_of(args.docx)
        payload['projection'] = os.path.basename(args.output)
        with io.open(sync_path, 'w', encoding='utf-8', newline='\n') as f:
            json.dump(payload, f, ensure_ascii=False, indent=2)
            f.write('\n')
        print('Đã cập nhật băm trong %s' % sync_path)
    else:
        print('Không thấy %s — bản chiếu này CHƯA có chống trôi.' % sync_path)

    return 0


if __name__ == '__main__':
    sys.exit(main())
