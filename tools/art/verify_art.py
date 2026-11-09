"""Giải mã lại các PNG vừa sinh và báo cáo chúng chứa gì.

Chạy:  python tools/art/verify_art.py

Mục đích: agent không nhìn được ảnh, nên thay vì tin rằng mã vẽ đúng, ta đọc ngược
file PNG ra và đo. Kiểm được: file hợp lệ, đúng kích thước, có thật sự vẽ gì không,
mọi hình cùng nhóm có chung đường chân không, và bóng dáng (silhouette) ra sao.
"""

import os
import struct
import sys
import zlib


def read_png(path):
    with open(path, "rb") as f:
        data = f.read()

    if data[:8] != b"\x89PNG\r\n\x1a\n":
        raise ValueError(path + ": không phải PNG")

    pos = 8
    width = height = None
    idat = bytearray()
    saw_end = False
    while pos < len(data):
        (length,) = struct.unpack(">I", data[pos:pos + 4])
        tag = data[pos + 4:pos + 8]
        payload = data[pos + 8:pos + 8 + length]
        (crc,) = struct.unpack(">I", data[pos + 8 + length:pos + 12 + length])
        if crc != (zlib.crc32(tag + payload) & 0xFFFFFFFF):
            raise ValueError(path + ": CRC sai ở chunk " + tag.decode())
        if tag == b"IHDR":
            width, height, depth, color_type = struct.unpack(">IIBB", payload[:10])
            if depth != 8 or color_type != 6:
                raise ValueError(path + ": chờ RGBA 8 bit")
        elif tag == b"IDAT":
            idat.extend(payload)
        elif tag == b"IEND":
            saw_end = True
        pos += 12 + length

    if not saw_end:
        raise ValueError(path + ": thiếu IEND")

    raw = zlib.decompress(bytes(idat))
    stride = width * 4
    px = bytearray(width * height * 4)
    prev = bytearray(stride)
    i = 0
    for y in range(height):
        f = raw[i]
        i += 1
        line = bytearray(raw[i:i + stride])
        i += stride
        if f == 1:
            for x in range(4, stride):
                line[x] = (line[x] + line[x - 4]) & 0xFF
        elif f == 2:
            for x in range(stride):
                line[x] = (line[x] + prev[x]) & 0xFF
        elif f != 0:
            raise ValueError(path + ": chưa đỡ filter " + str(f))
        px[y * stride:(y + 1) * stride] = line
        prev = line

    return width, height, px


def describe(path, expect_w, expect_h):
    w, h, px = read_png(path)
    assert (w, h) == (expect_w, expect_h), path + ": kích thước " + str((w, h))

    opaque = 0
    min_x, max_x, min_y, max_y = w, -1, h, -1
    colors = {}
    for y in range(h):
        for x in range(w):
            i = (y * w + x) * 4
            if px[i + 3] > 128:
                opaque += 1
                min_x = min(min_x, x)
                max_x = max(max_x, x)
                min_y = min(min_y, y)
                max_y = max(max_y, y)
                key = (px[i], px[i + 1], px[i + 2])
                colors[key] = colors.get(key, 0) + 1

    return {
        "w": w, "h": h, "opaque": opaque,
        "coverage": round(100.0 * opaque / (w * h), 1),
        "bbox": (min_x, min_y, max_x, max_y),
        "bottom": max_y,
        "colors": len(colors),
        "px": px,
    }


def ascii_preview(info, cols=36):
    """Bóng dáng ảnh bằng chữ, để mắt người (và agent) kiểm được hình thù."""
    w, h, px = info["w"], info["h"], info["px"]
    rows = max(1, int(cols * h / w / 2))
    ramp = " .:-=+*#%@"
    out = []
    for ry in range(rows):
        line = []
        for rx in range(cols):
            x0, x1 = rx * w // cols, max(rx * w // cols + 1, (rx + 1) * w // cols)
            y0, y1 = ry * h // rows, max(ry * h // rows + 1, (ry + 1) * h // rows)
            total = count = 0
            for y in range(y0, y1):
                for x in range(x0, x1):
                    i = (y * w + x) * 4
                    a = px[i + 3]
                    lum = (px[i] * 30 + px[i + 1] * 59 + px[i + 2] * 11) // 100
                    total += (255 - lum) * a // 255
                    count += 1
            v = total // max(1, count)
            line.append(ramp[min(len(ramp) - 1, v * len(ramp) // 256)])
        out.append("".join(line))
    return out


def main():
    here = os.path.dirname(os.path.abspath(__file__))
    art = os.path.abspath(os.path.join(
        here, "..", "..", "Assets", "Art", "Placeholder", "Resources", "BeforeUsPlaceholder"))

    groups = [
        ("THÀNH VIÊN  (96x128, đường chân 122)", 96, 128, 122, [
            "member_stage1_male_stand", "member_stage1_male_sit",
            "member_stage1_female_stand", "member_stage1_female_sit"]),
        ("VẬT THỂ  (96x96, đường chân 88)", 96, 96, 88, [
            "object_food_nest", "object_club", "object_rock_pile", "object_branch"]),
    ]

    failures = 0
    for title, ew, eh, ground, names in groups:
        print("")
        print("=" * 70)
        print(" " + title)
        print("=" * 70)
        bottoms = []
        for name in names:
            info = describe(os.path.join(art, name + ".png"), ew, eh)
            bottoms.append(info["bottom"])
            print("")
            print(name + ".png")
            print("  phủ %.1f%% khung · khung bao %s · đáy y=%d · %d màu"
                  % (info["coverage"], info["bbox"], info["bottom"], info["colors"]))
            if info["coverage"] < 8:
                failures += 1
                print("  HỎNG: gần như trống")
            for line in ascii_preview(info):
                print("    |" + line + "|")

        spread = max(bottoms) - min(bottoms)
        # Thứ thật sự quan trọng khi thay ảnh: ĐÁY phải nằm đúng trên đường chân khai báo,
        # không chỉ là các hình bằng nhau. Bằng nhau mà cùng lệch thì vẫn phải canh lại.
        off = [abs(b - (ground - 1)) for b in bottoms]
        ok = spread == 0 and max(off) <= 1
        if not ok:
            failures += 1
        print("")
        print("  %s đường chân: lệch nhau %d px, xa đường chân khai báo nhiều nhất %d px"
              % ("ĐẠT " if ok else "HỎNG", spread, max(off)))

    print("")
    print("KẾT LUẬN: " + ("mọi hình hợp lệ." if failures == 0 else str(failures) + " vấn đề."))
    return 1 if failures else 0


if __name__ == "__main__":
    sys.exit(main())
