# -*- coding: utf-8 -*-
"""Bài kiểm khứ hồi: Markdown -> docx -> Markdown phải tương đương về NỘI DUNG.

    python tools/doc/test_roundtrip.py                 # chạy cả bộ, gồm docs/design.md
    python tools/doc/test_roundtrip.py <file.md>       # kiểm một file bất kỳ

Công cụ này tồn tại để giữ nguồn sự thật số 1 của dự án đọc được. Nếu khứ hồi làm rụng
một chữ thì công cụ chưa xong — và một công cụ tài liệu làm rụng chữ trong LUẬT CHƠI
còn tệ hơn là không có công cụ nào.

So sánh theo dạng CHUẨN HOÁ (mdblocks.normalise): bỏ khác biệt về khoảng trắng và cách
căn bảng, không bao giờ bỏ chữ.
"""

import collections
import difflib
import io
import os
import re
import sys
import tempfile

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import docx_read
import docx_write
import mdblocks as M

REPO = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))


CASES = {
    'tiêu đề mọi cấp': '# Một\n\n## Hai\n\n### Ba\n\n#### Bốn\n\n##### Năm\n\n###### Sáu\n',

    'đoạn văn và định dạng trong dòng':
        'Chữ thường, **đậm**, *nghiêng*, `mã`, và **đậm có `mã` bên trong**.\n',

    'bảng đơn giản':
        '| Cột A | Cột B |\n|---|---|\n| 1 | 2 |\n| 3 | 4 |\n',

    'bảng có căn lề':
        '| Trái | Giữa | Phải |\n|---|:---:|---:|\n| a | b | c |\n',

    'bảng có định dạng trong ô':
        '| Mục | Ghi chú |\n|---|---|\n| **Đậm** | `mã` và *nghiêng* |\n',

    'bảng có ô rỗng':
        '| A | B | C |\n|---|---|---|\n| 1 |  | 3 |\n',

    'danh sách chấm đầu dòng':
        '- Một\n- Hai\n- Ba\n',

    'danh sách lồng hai mức':
        '- Mức một\n  - Mức hai\n  - Mức hai nữa\n- Mức một lại\n',

    'danh sách đánh số':
        '1. Một\n2. Hai\n3. Ba\n',

    'khối mã không khai ngôn ngữ':
        '```\nmot dong\nhai dong\n```\n',

    'khối mã có khai ngôn ngữ':
        '```python\nprint("xin chao")\n```\n',

    'trích dẫn nhiều dòng':
        '> Dòng một.\n> Dòng hai có **đậm**.\n> Dòng ba.\n',

    'đường kẻ ngang':
        'Trên\n\n---\n\nDưới\n',

    'tiếng Việt có dấu đầy đủ':
        '# Tiến hóa\n\nQuái thú ăn `quota` người rồi rút — không bao giờ xóa sổ cả bầy.\n',

    # Liên kết chưa dựng thành siêu liên kết Word, nhưng CHỮ và ĐỊNH DẠNG không được mất.
    # Chính ca thứ hai đã bắt được lỗi xoá sạch dấu đậm.
    'liên kết giữ nguyên chữ':
        'Xem [mô phỏng](tools/sim/before_us_sim.py) để biết thêm.\n',

    'liên kết có chữ đậm bên trong':
        'Xem [**công thức**](docs/design.md) và [`mã`](a.md).\n',

    'liên kết trong bảng':
        '| Mục | Nguồn |\n|---|---|\n| D13 | [quyết định](docs/decisions.md) |\n',

    'ký tự phải thoát trong XML':
        'Dấu & và < và > phải sống sót. Cả "nháy kép" nữa.\n',
}


def roundtrip(markdown_text):
    """Markdown -> docx -> Markdown. Trả bản chiếu thu được."""
    tmpdir = tempfile.mkdtemp(prefix='beforeus_doc_')
    try:
        docx_path = os.path.join(tmpdir, 'tmp.docx')
        docx_write.write_docx(markdown_text, docx_path)
        return docx_read.docx_to_markdown(docx_path)
    finally:
        for name in os.listdir(tmpdir):
            os.remove(os.path.join(tmpdir, name))
        os.rmdir(tmpdir)


_SYNTAX = re.compile(r'[\s|#*`>_\-:]+', re.UNICODE)
_SINGLE_STAR = re.compile(r'(?<!\*)\*(?!\*)')


def bare_text(markdown_text):
    """Bỏ hết ký tự cú pháp Markdown và khoảng trắng, chỉ còn chữ.

    Đây là phép kiểm KHÔNG đi qua parser của chính công cụ này, và đó là cả vấn đề:
    normalise() chạy parse->render trên CẢ HAI bên, nên nếu parser nuốt mất một chữ
    thì hai bên nuốt như nhau và bài kiểm vẫn xanh. Một bài kiểm tự lừa mình thì tệ
    hơn không có bài kiểm, vì nó cấp cho ta sự yên tâm sai.
    """
    return _SYNTAX.sub('', markdown_text)


def compare(name, original):
    back = roundtrip(original)

    # 1. Kiểm độc lập trước: không được rụng một chữ nào.
    a, b = bare_text(original), bare_text(back)
    if a != b:
        print('ĐỎ  %s  — RỤNG CHỮ' % name)
        print('     ký tự gốc %d, sau khứ hồi %d' % (len(a), len(b)))
        ca, cb = collections.Counter(a), collections.Counter(b)
        if ca - cb:
            print('     thiếu: %r' % dict((ca - cb).most_common(10)))
        if cb - ca:
            print('     thừa : %r' % dict((cb - ca).most_common(10)))
        sm = difflib.SequenceMatcher(None, a, b, autojunk=False)
        for tag, i1, i2, j1, j2 in sm.get_opcodes():
            if tag != 'equal':
                print('     quanh chỗ lệch, gốc: ...%s...' % a[max(0, i1 - 50):i2 + 50])
                print('     quanh chỗ lệch, mới: ...%s...' % b[max(0, j1 - 50):j2 + 50])
                break
        print('')
        return False

    # 2. Dấu ĐỊNH DẠNG cũng không được rụng.
    #
    #    Phép (1) bóc hết ký tự cú pháp nên nó mù với chuyện này, còn phép (3) chạy
    #    normalise() lên CẢ HAI bên nên một lỗi trong hàm dựng sẽ tự che chính nó.
    #    Đã xảy ra thật: hàm gộp dấu xoá sạch 64 dấu đậm của docs/design.md — gồm mọi
    #    nhãn [ĐỀ XUẤT] và [MVP] — mà cả hai phép kia vẫn xanh.
    #
    #    Đếm trên chuỗi THÔ, không qua parser.
    for label, counter in (('đậm **', lambda t: t.count('**')),
                           ('mã `', lambda t: t.count('`')),
                           ('nghiêng *', lambda t: len(_SINGLE_STAR.findall(t)))):
        if counter(original) != counter(back):
            print('ĐỎ  %s  — RỤNG DẤU ĐỊNH DẠNG (%s)' % (name, label))
            print('     gốc %d, sau khứ hồi %d' % (counter(original), counter(back)))
            print('     gốc : %r' % original.strip()[:200])
            print('     mới : %r' % back.strip()[:200])
            print('')
            return False

    # 3. Cuối cùng mới kiểm cấu trúc.
    want = M.normalise(original)
    got = M.normalise(back)

    if want == got:
        return True

    print('ĐỎ  %s' % name)
    diff = difflib.unified_diff(
        want.split('\n'), got.split('\n'),
        fromfile='gốc (chuẩn hoá)', tofile='sau khứ hồi', lineterm='', n=1)
    for line in list(diff)[:40]:
        print('     ' + line)
    print('')
    return False


def check_file(path):
    with io.open(path, encoding='utf-8') as f:
        text = f.read()

    blocks = M.parse_markdown(text)
    counts = {}
    for b in blocks:
        counts[b['t']] = counts.get(b['t'], 0) + 1

    print('--- %s' % os.path.relpath(path, REPO))
    print('    %d khối: %s' % (
        len(blocks), ', '.join('%s %d' % (k, v) for k, v in sorted(counts.items()))))

    ok = compare(os.path.relpath(path, REPO), text)
    if ok:
        print('    XANH — %d ký tự chữ khớp từng ký tự, cấu trúc khớp'
              % len(bare_text(text)))
    print('')
    return ok


def main(argv):
    if argv:
        return 0 if all(check_file(p) for p in argv) else 1

    print('Bài kiểm khứ hồi: Markdown -> docx -> Markdown\n')

    passed = 0
    failed = 0
    for name, text in CASES.items():
        if compare(name, text):
            passed += 1
        else:
            failed += 1

    print('Trường hợp dựng sẵn: %d xanh, %d đỏ\n' % (passed, failed))

    design = os.path.join(REPO, 'docs', 'design.md')
    if os.path.exists(design):
        if not check_file(design):
            failed += 1
    else:
        print('Không thấy %s — bỏ qua.\n' % design)

    if failed:
        print('ĐỎ: còn %d chỗ chưa khứ hồi được. Công cụ chưa xong.' % failed)
        return 1

    print('XANH: khứ hồi giữ nguyên nội dung ở mọi trường hợp.')
    return 0


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
