# -*- coding: utf-8 -*-
"""Các phần XML cố định của một gói .docx.

Một file .docx chỉ là một file ZIP chứa XML. Không cần thư viện ngoài — mạng bị chặn,
và dù không bị chặn thì thêm một phụ thuộc cho việc này cũng không đáng.

Tham chiếu: ECMA-376 (Office Open XML), phần WordprocessingML.
"""

W = 'http://schemas.openxmlformats.org/wordprocessingml/2006/main'
R = 'http://schemas.openxmlformats.org/officeDocument/2006/relationships'

CONTENT_TYPES = '''<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
  <Default Extension="xml" ContentType="application/xml"/>
  <Override PartName="/word/document.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml"/>
  <Override PartName="/word/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.styles+xml"/>
  <Override PartName="/word/numbering.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.numbering+xml"/>
  <Override PartName="/word/settings.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.settings+xml"/>
  <Override PartName="/docProps/core.xml" ContentType="application/vnd.openxmlformats-package.core-properties+xml"/>
</Types>
'''

ROOT_RELS = '''<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="word/document.xml"/>
  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties" Target="docProps/core.xml"/>
</Relationships>
'''

DOCUMENT_RELS = '''<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>
  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/numbering" Target="numbering.xml"/>
  <Relationship Id="rId3" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/settings" Target="settings.xml"/>
</Relationships>
'''

# updateFields: chính là thứ làm Word hỏi "cập nhật mục lục?" khi mở file.
# Không có nó thì mục lục hiện trống cho tới khi ai đó biết bấm F9.
SETTINGS = '''<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<w:settings xmlns:w="%s">
  <w:updateFields w:val="true"/>
</w:settings>
''' % W


def _heading_style(level, size_half_points, color):
    """outlineLvl là thứ trường TOC của Word dùng để gom mục lục.

    Thiếu nó thì mục lục dựng ra rỗng dù tiêu đề trông vẫn đúng cỡ chữ — một lỗi
    im lặng rất khó đoán, nên nó nằm đây chứ không phải ở chỗ sinh đoạn văn.
    """
    return '''  <w:style w:type="paragraph" w:styleId="Heading%(n)d">
    <w:name w:val="heading %(n)d"/>
    <w:basedOn w:val="Normal"/>
    <w:next w:val="Normal"/>
    <w:qFormat/>
    <w:pPr>
      <w:keepNext/>
      <w:spacing w:before="%(before)d" w:after="120"/>
      <w:outlineLvl w:val="%(outline)d"/>
    </w:pPr>
    <w:rPr>
      <w:b/>
      <w:color w:val="%(color)s"/>
      <w:sz w:val="%(size)d"/>
    </w:rPr>
  </w:style>
''' % {'n': level, 'outline': level - 1, 'size': size_half_points,
       'color': color, 'before': 360 if level == 1 else 240}


STYLES = '''<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<w:styles xmlns:w="%(w)s">
  <w:docDefaults>
    <w:rPrDefault>
      <w:rPr>
        <w:rFonts w:ascii="Calibri" w:hAnsi="Calibri" w:cs="Calibri"/>
        <w:sz w:val="22"/>
        <w:szCs w:val="22"/>
        <w:lang w:val="vi-VN"/>
      </w:rPr>
    </w:rPrDefault>
    <w:pPrDefault>
      <w:pPr><w:spacing w:after="120" w:line="276" w:lineRule="auto"/></w:pPr>
    </w:pPrDefault>
  </w:docDefaults>

  <w:style w:type="paragraph" w:default="1" w:styleId="Normal">
    <w:name w:val="Normal"/>
    <w:qFormat/>
  </w:style>

%(headings)s
  <w:style w:type="paragraph" w:styleId="Title">
    <w:name w:val="Title"/>
    <w:basedOn w:val="Normal"/>
    <w:qFormat/>
    <w:pPr><w:spacing w:after="240"/></w:pPr>
    <w:rPr><w:b/><w:sz w:val="56"/></w:rPr>
  </w:style>

  <w:style w:type="paragraph" w:styleId="Quote">
    <w:name w:val="Quote"/>
    <w:basedOn w:val="Normal"/>
    <w:qFormat/>
    <w:pPr>
      <w:ind w:left="567"/>
      <w:pBdr><w:left w:val="single" w:sz="12" w:space="8" w:color="B0B0B0"/></w:pBdr>
    </w:pPr>
    <w:rPr><w:i/><w:color w:val="404040"/></w:rPr>
  </w:style>

  <w:style w:type="paragraph" w:styleId="SourceCode">
    <w:name w:val="Source Code"/>
    <w:basedOn w:val="Normal"/>
    <w:qFormat/>
    <w:pPr>
      <w:shd w:val="clear" w:color="auto" w:fill="F4F4F4"/>
      <w:spacing w:before="120" w:after="120" w:line="240" w:lineRule="auto"/>
      <w:ind w:left="284" w:right="284"/>
    </w:pPr>
    <w:rPr><w:rFonts w:ascii="Consolas" w:hAnsi="Consolas" w:cs="Consolas"/><w:sz w:val="19"/></w:rPr>
  </w:style>

  <w:style w:type="paragraph" w:styleId="ListParagraph">
    <w:name w:val="List Paragraph"/>
    <w:basedOn w:val="Normal"/>
    <w:qFormat/>
    <w:pPr><w:contextualSpacing/><w:spacing w:after="60"/></w:pPr>
  </w:style>

  <w:style w:type="paragraph" w:styleId="TableHeaderCell">
    <w:name w:val="Table Header Cell"/>
    <w:basedOn w:val="Normal"/>
    <w:qFormat/>
    <w:pPr><w:spacing w:after="0"/></w:pPr>
    <!-- Đậm nằm ở STYLE ĐOẠN, không phải ở từng run.
         Nếu ép w:b lên từng run thì bên đọc không phân biệt được "đậm vì là ô tiêu đề"
         với "đậm vì tác giả viết **đậm**", và phải bóc đậm khỏi mọi ô tiêu đề — làm mất
         chữ đậm thật. docs/design.md có đúng một ô như thế: | Giai đoạn | ... | **[MVP]** |. -->
    <w:rPr><w:b/></w:rPr>
  </w:style>

  <w:style w:type="character" w:styleId="InlineCode">
    <w:name w:val="Inline Code"/>
    <w:qFormat/>
    <w:rPr>
      <w:rFonts w:ascii="Consolas" w:hAnsi="Consolas" w:cs="Consolas"/>
      <w:sz w:val="19"/>
      <w:shd w:val="clear" w:color="auto" w:fill="F0F0F0"/>
    </w:rPr>
  </w:style>

  <w:style w:type="table" w:styleId="TableGrid">
    <w:name w:val="Table Grid"/>
    <w:tblPr>
      <w:tblBorders>
        <w:top w:val="single" w:sz="4" w:space="0" w:color="A0A0A0"/>
        <w:left w:val="single" w:sz="4" w:space="0" w:color="A0A0A0"/>
        <w:bottom w:val="single" w:sz="4" w:space="0" w:color="A0A0A0"/>
        <w:right w:val="single" w:sz="4" w:space="0" w:color="A0A0A0"/>
        <w:insideH w:val="single" w:sz="4" w:space="0" w:color="A0A0A0"/>
        <w:insideV w:val="single" w:sz="4" w:space="0" w:color="A0A0A0"/>
      </w:tblBorders>
    </w:tblPr>
  </w:style>

  <w:style w:type="paragraph" w:styleId="TOCHeading">
    <w:name w:val="TOC Heading"/>
    <w:basedOn w:val="Heading1"/>
    <w:qFormat/>
    <w:pPr><w:outlineLvl w:val="9"/></w:pPr>
  </w:style>
</w:styles>
''' % {'w': W,
       'headings': ''.join(_heading_style(lv, sz, col) for lv, sz, col in (
           (1, 36, '1F3864'), (2, 30, '2E5496'), (3, 26, '2E5496'),
           (4, 24, '404040'), (5, 22, '404040'), (6, 22, '606060')))}


# numId 1 = chấm đầu dòng, numId 2 = đánh số. Ba mức mỗi loại.
def _levels(fmt, text_fn, font=None):
    out = []
    for ilvl in range(3):
        rpr = ('<w:rPr><w:rFonts w:ascii="%s" w:hAnsi="%s" w:hint="default"/></w:rPr>' % (font, font)) if font else ''
        out.append(
            '<w:lvl w:ilvl="%d"><w:start w:val="1"/><w:numFmt w:val="%s"/>'
            '<w:lvlText w:val="%s"/><w:lvlJc w:val="left"/>'
            '<w:pPr><w:ind w:left="%d" w:hanging="360"/></w:pPr>%s</w:lvl>'
            % (ilvl, fmt, text_fn(ilvl), 720 + ilvl * 360, rpr))
    return ''.join(out)


NUMBERING = '''<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<w:numbering xmlns:w="%(w)s">
  <w:abstractNum w:abstractNumId="0">
    <w:multiLevelType w:val="hybridMultilevel"/>
    %(bullet)s
  </w:abstractNum>
  <w:abstractNum w:abstractNumId="1">
    <w:multiLevelType w:val="hybridMultilevel"/>
    %(decimal)s
  </w:abstractNum>
  <w:num w:numId="1"><w:abstractNumId w:val="0"/></w:num>
  <w:num w:numId="2"><w:abstractNumId w:val="1"/></w:num>
</w:numbering>
''' % {'w': W,
       'bullet': _levels('bullet', lambda i: ['', 'o', ''][i], font='Symbol'),
       'decimal': _levels('decimal', lambda i: '%%%d.' % (i + 1))}

NUM_ID_BULLET = 1
NUM_ID_ORDERED = 2
