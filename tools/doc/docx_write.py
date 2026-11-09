# -*- coding: utf-8 -*-
"""Markdown -> .docx (có mục lục Word tự dựng).

    python tools/doc/docx_write.py docs/design.md docs/design.docx
    python tools/doc/docx_write.py docs/design.md docs/design.docx --sync docs/design.sync.json

Chỉ dùng thư viện chuẩn: .docx là ZIP chứa XML, nên zipfile + chuỗi là đủ.

Mục lục dùng trường TOC của Word (`TOC \\o "1-3" \\h \\z \\u`). Word dựng nó khi mở file,
không phải khi sinh file — nên lúc mở Word sẽ hỏi "cập nhật mục lục?", bấm **Có**.
"""

import argparse
import datetime
import re
import hashlib
import io
import json
import os
import sys
import zipfile

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import docxparts as P
import mdblocks as M


# --------------------------------------------------------------------------- tiện ích

def xml_escape(text):
    return (text.replace('&', '&amp;').replace('<', '&lt;').replace('>', '&gt;'))


def attr_escape(text):
    return xml_escape(text).replace('"', '&quot;')


def run(text, bold=False, italic=False, code=False):
    """Một đoạn chữ có định dạng. xml:space="preserve" là bắt buộc, nếu không Word
    nuốt khoảng trắng đầu/cuối và chữ dính vào nhau sau khi khứ hồi."""
    props = []
    if code:
        props.append('<w:rStyle w:val="InlineCode"/>')
    if bold:
        props.append('<w:b/>')
    if italic:
        props.append('<w:i/>')

    rpr = '<w:rPr>%s</w:rPr>' % ''.join(props) if props else ''
    return '<w:r>%s<w:t xml:space="preserve">%s</w:t></w:r>' % (rpr, xml_escape(text))


def runs_from_spans(spans):
    return ''.join(run(s['text'], s['b'], s['i'], s['c']) for s in spans if s['text'] != '')


def paragraph(content, style=None, numid=None, ilvl=0, jc=None):
    props = []
    if style:
        props.append('<w:pStyle w:val="%s"/>' % style)
    if numid is not None:
        props.append('<w:numPr><w:ilvl w:val="%d"/><w:numId w:val="%d"/></w:numPr>' % (ilvl, numid))
    if jc:
        props.append('<w:jc w:val="%s"/>' % jc)

    ppr = '<w:pPr>%s</w:pPr>' % ''.join(props) if props else ''
    return '<w:p>%s%s</w:p>' % (ppr, content)


# --------------------------------------------------------------------------- khối

_JC = {'left': None, 'center': 'center', 'right': 'right'}


def code_language_marker(lang):
    """Ngôn ngữ của khối mã cất trong một run ẨN (w:vanish).

    Word giữ nguyên chữ ẩn qua các lần lưu nhưng không hiện nó, nên khứ hồi không
    mất ```python. Chỉ phát sinh khi khối mã CÓ khai ngôn ngữ — docs/design.md hiện
    không khối nào khai, nên với file đó phần này không bao giờ chạy.
    """
    if not lang:
        return ''
    return ('<w:r><w:rPr><w:vanish/></w:rPr>'
            '<w:t xml:space="preserve">⟦lang:%s⟧</w:t></w:r>' % xml_escape(lang))


def block_xml(block):
    t = block['t']

    if t == 'heading':
        return paragraph(runs_from_spans(block['spans']), style='Heading%d' % block['level'])

    if t == 'para':
        return paragraph(runs_from_spans(block['spans']))

    if t == 'hr':
        return ('<w:p><w:pPr><w:pBdr>'
                '<w:bottom w:val="single" w:sz="6" w:space="1" w:color="A0A0A0"/>'
                '</w:pBdr></w:pPr></w:p>')

    if t == 'quote':
        # Các dòng '>' liền nhau thành MỘT đoạn có ngắt dòng mềm, để khứ hồi ra
        # đúng số dòng như bản gốc.
        parts = []
        for idx, line in enumerate(block['lines']):
            if idx:
                parts.append('<w:r><w:br/></w:r>')
            parts.append(runs_from_spans(line))
        return paragraph(''.join(parts), style='Quote')

    if t == 'code':
        parts = [code_language_marker(block['lang'])]
        for idx, line in enumerate(block['lines']):
            if idx:
                parts.append('<w:r><w:br/></w:r>')
            parts.append('<w:r><w:t xml:space="preserve">%s</w:t></w:r>' % xml_escape(line))
        return paragraph(''.join(parts), style='SourceCode')

    if t == 'list':
        out = []
        for item in block['items']:
            numid = P.NUM_ID_ORDERED if item['ordered'] else P.NUM_ID_BULLET
            out.append(paragraph(runs_from_spans(item['spans']),
                                 style='ListParagraph', numid=numid, ilvl=item['level']))
        return ''.join(out)

    if t == 'table':
        return table_xml(block)

    raise ValueError('Khối lạ: %r' % (t,))


def table_xml(block):
    columns = len(block['header'])
    align = list(block['align']) + ['left'] * (columns - len(block['align']))

    # Chiều rộng tổng cố định (khổ A4 trừ lề), chia đều rồi để Word tự co theo nội dung.
    total = 9360
    width = max(1, total // max(1, columns))

    out = ['<w:tbl><w:tblPr>'
           '<w:tblStyle w:val="TableGrid"/>'
           '<w:tblW w:w="%d" w:type="dxa"/>'
           '<w:tblLayout w:type="autofit"/>'
           '</w:tblPr><w:tblGrid>%s</w:tblGrid>'
           % (total, ''.join('<w:gridCol w:w="%d"/>' % width for _ in range(columns)))]

    out.append(_row(block['header'], align, header=True))
    for row in block['rows']:
        out.append(_row(row, align, header=False))

    out.append('</w:tbl>')

    # Word cần một đoạn sau bảng, nếu không hai bảng liền nhau sẽ bị gộp làm một.
    out.append('<w:p/>')
    return ''.join(out)


def _row(cells, align, header):
    parts = ['<w:tr>']
    if header:
        parts.append('<w:trPr><w:tblHeader/></w:trPr>')   # lặp lại hàng tiêu đề khi sang trang

    for idx in range(len(align)):
        spans = cells[idx] if idx < len(cells) else [M.span('')]

        # Đậm của ô tiêu đề do STYLE ĐOẠN lo, không ép lên từng run — xem ghi chú ở
        # style TableHeaderCell. Ép lên run thì chữ đậm thật trong ô tiêu đề sẽ bị
        # bên đọc bóc nhầm.
        shading = '<w:shd w:val="clear" w:color="auto" w:fill="EDEDED"/>' if header else ''
        parts.append(
            '<w:tc><w:tcPr><w:tcW w:w="0" w:type="auto"/>%s</w:tcPr>%s</w:tc>'
            % (shading,
               paragraph(runs_from_spans(spans),
                         style='TableHeaderCell' if header else None,
                         jc=_JC.get(align[idx]))))

    parts.append('</w:tr>')
    return ''.join(parts)


# --------------------------------------------------------------------------- tài liệu

TOC_FIELD = (
    '<w:p><w:pPr><w:pStyle w:val="TOCHeading"/></w:pPr>'
    '<w:r><w:t xml:space="preserve">Mục lục</w:t></w:r></w:p>'
    '<w:p>'
    '<w:fldSimple w:instr=" TOC \\o &quot;1-3&quot; \\h \\z \\u ">'
    '<w:r><w:rPr><w:i/><w:color w:val="808080"/></w:rPr>'
    '<w:t xml:space="preserve">Word sẽ dựng mục lục ở đây. '
    'Nếu được hỏi "cập nhật mục lục?", bấm Có.'
    '</w:t></w:r>'
    '</w:fldSimple>'
    '</w:p>'
    '<w:p><w:r><w:br w:type="page"/></w:r></w:p>'
)


def document_xml(blocks, with_toc=True):
    body = []

    # Tiêu đề h1 đầu tiên (nếu có) lên trước mục lục, phần còn lại đi sau.
    rest = blocks
    if blocks and blocks[0]['t'] == 'heading' and blocks[0]['level'] == 1:
        body.append(paragraph(runs_from_spans(blocks[0]['spans']), style='Title'))
        rest = blocks[1:]

    if with_toc:
        body.append(TOC_FIELD)

    for b in rest:
        body.append(block_xml(b))

    return (
        '<?xml version="1.0" encoding="UTF-8" standalone="yes"?>\n'
        '<w:document xmlns:w="%s" xmlns:r="%s"><w:body>%s'
        '<w:sectPr>'
        '<w:pgSz w:w="11906" w:h="16838"/>'
        '<w:pgMar w:top="1134" w:right="1134" w:bottom="1134" w:left="1418" w:header="709" w:footer="709"/>'
        '</w:sectPr>'
        '</w:body></w:document>' % (P.W, P.R, ''.join(body)))


def core_xml(title):
    now = datetime.datetime.now(datetime.timezone.utc).strftime('%Y-%m-%dT%H:%M:%SZ')
    return (
        '<?xml version="1.0" encoding="UTF-8" standalone="yes"?>\n'
        '<cp:coreProperties '
        'xmlns:cp="http://schemas.openxmlformats.org/package/2006/metadata/core-properties" '
        'xmlns:dc="http://purl.org/dc/elements/1.1/" '
        'xmlns:dcterms="http://purl.org/dc/terms/" '
        'xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">'
        '<dc:title>%s</dc:title>'
        '<dcterms:created xsi:type="dcterms:W3CDTF">%s</dcterms:created>'
        '<dcterms:modified xsi:type="dcterms:W3CDTF">%s</dcterms:modified>'
        '</cp:coreProperties>' % (xml_escape(title), now, now))


_LINK = re.compile(r'\[([^\]]+)\]\(([^)\s]+)\)')


def find_links(markdown_text):
    """Tìm liên kết Markdown, trả về danh sách (số dòng, chữ, địa chỉ)."""
    found = []
    for lineno, line in enumerate(markdown_text.split('\n'), start=1):
        for m in _LINK.finditer(line):
            found.append((lineno, m.group(1), m.group(2)))
    return found


def warn_about_links(markdown_text, stream=None):
    """Liên kết KHÔNG bị nuốt: chữ và định dạng đi qua nguyên vẹn, và bài kiểm khứ hồi
    có ba trường hợp khoá điều đó (kể cả liên kết có chữ đậm bên trong, chính là ca đã
    bắt được lỗi xoá sạch dấu đậm).

    Nhưng trong Word chúng hiện ra dạng cú pháp thô `[chữ](địa-chỉ)`, không bấm được.
    Nói to chuyện đó ngay lúc sinh file, vì im lặng ở đây nghĩa là Khoa phát hiện bằng
    cách mở tài liệu và thấy nó trông cẩu thả.

    Trả về số liên kết tìm thấy.
    """
    stream = stream or sys.stderr
    links = find_links(markdown_text)
    if not links:
        return 0

    w = stream.write
    w('\nCẢNH BÁO: tìm thấy %d liên kết Markdown.\n' % len(links))
    w('  Chữ và định dạng ĐƯỢC GIỮ NGUYÊN (bài kiểm khứ hồi khoá điều này),\n')
    w('  nhưng trong Word chúng hiện ra dạng cú pháp thô, không bấm được.\n')
    w('  Siêu liên kết thật chưa được dựng — xem mục Giới hạn đã biết trong\n')
    w('  tools/doc/README.md. Cần thì báo để làm thêm.\n')
    for lineno, text, target in links[:10]:
        w('    dòng %-5d [%s](%s)\n' % (lineno, text, target))
    if len(links) > 10:
        w('    ... và %d liên kết nữa\n' % (len(links) - 10))
    w('\n')
    return len(links)


def write_docx(markdown_text, out_path, title=None, with_toc=True):
    blocks = M.parse_markdown(markdown_text)

    if title is None:
        title = next((M.spans_text(b['spans']) for b in blocks
                      if b['t'] == 'heading' and b['level'] == 1), 'Tài liệu')

    parts = {
        '[Content_Types].xml': P.CONTENT_TYPES,
        '_rels/.rels': P.ROOT_RELS,
        'word/_rels/document.xml.rels': P.DOCUMENT_RELS,
        'word/document.xml': document_xml(blocks, with_toc=with_toc),
        'word/styles.xml': P.STYLES,
        'word/numbering.xml': P.NUMBERING,
        'word/settings.xml': P.SETTINGS,
        'docProps/core.xml': core_xml(title),
    }

    buffer = io.BytesIO()
    # Thứ tự ghi cố định và ZIP_DEFLATED để hai lần sinh từ cùng một nguồn cho cùng
    # một chuỗi byte — băm chống trôi chỉ có nghĩa nếu điều đó đúng.
    with zipfile.ZipFile(buffer, 'w', zipfile.ZIP_DEFLATED) as z:
        for name in ('[Content_Types].xml', '_rels/.rels', 'word/document.xml',
                     'word/_rels/document.xml.rels', 'word/styles.xml',
                     'word/numbering.xml', 'word/settings.xml', 'docProps/core.xml'):
            info = zipfile.ZipInfo(name, date_time=(1980, 1, 1, 0, 0, 0))
            info.compress_type = zipfile.ZIP_DEFLATED
            info.external_attr = 0o600 << 16
            z.writestr(info, parts[name].encode('utf-8'))

    data = buffer.getvalue()
    with open(out_path, 'wb') as f:
        f.write(data)

    return hashlib.sha256(data).hexdigest()


def write_sync(sync_path, docx_path, digest, source_path, block_count):
    payload = {
        '_comment': ('Chong troi cho ban chieu Markdown cua tai lieu docx. '
                     'docx la BAN CHINH; file .md canh no chi la ban chieu sinh tu dong. '
                     'Truoc khi tin ban chieu, chay: python tools/doc/docx_read.py --check <docx>. '
                     'Bam lech nghia la ai do da sua docx trong Word ma chua sinh lai ban chieu — '
                     'doc ban chieu cu luc do la doc mot tai lieu da loi thoi.'),
        'docx': os.path.basename(docx_path),
        'sha256': digest,
        'generated_from': os.path.basename(source_path) if source_path else None,
        'blocks': block_count,
        'generated_at': datetime.datetime.now(datetime.timezone.utc).strftime('%Y-%m-%dT%H:%M:%SZ'),
        'tool': 'tools/doc/docx_write.py',
    }

    with io.open(sync_path, 'w', encoding='utf-8', newline='\n') as f:
        json.dump(payload, f, ensure_ascii=False, indent=2)
        f.write('\n')


def main(argv=None):
    ap = argparse.ArgumentParser(description='Markdown -> .docx có mục lục')
    ap.add_argument('source', help='file Markdown nguồn')
    ap.add_argument('output', help='file .docx sinh ra')
    ap.add_argument('--sync', help='ghi file băm chống trôi (JSON)')
    ap.add_argument('--title', help='tiêu đề tài liệu; mặc định lấy từ tiêu đề cấp 1')
    ap.add_argument('--no-toc', action='store_true', help='không chèn mục lục')
    ap.add_argument('--strict-links', action='store_true',
                    help='dừng hẳn (mã thoát 3) nếu tài liệu có liên kết Markdown')
    args = ap.parse_args(argv)

    with io.open(args.source, encoding='utf-8') as f:
        text = f.read()

    blocks = M.parse_markdown(text)
    link_count = warn_about_links(text)

    if link_count and args.strict_links:
        print('DỪNG: --strict-links được bật và tài liệu có liên kết.', file=sys.stderr)
        return 3

    digest = write_docx(text, args.output, title=args.title, with_toc=not args.no_toc)

    print('Đã ghi %s' % args.output)
    print('  khối   : %d' % len(blocks))
    print('  sha256 : %s' % digest)

    if args.sync:
        write_sync(args.sync, args.output, digest, args.source, len(blocks))
        print('  đồng bộ: %s' % args.sync)

    print('\nMở trong Word: nếu được hỏi "cập nhật mục lục?" thì bấm Có.')
    return 0


if __name__ == '__main__':
    sys.exit(main())
