"""Bộ vẽ tối thiểu: ảnh RGBA trong bộ nhớ, vài hình cơ bản, ghi ra PNG.

Chỉ dùng thư viện chuẩn (zlib, struct) — máy không cài được gói ngoài.

Cách khử răng cưa: vẽ ở độ phân giải gấp SS lần với cạnh cứng, rồi thu nhỏ bằng
trung bình ô vuông. Đơn giản hơn nhiều so với tính phủ biên từng điểm ảnh, mà
với phong cách bóng phẳng thì kết quả đủ sạch.
"""

import struct
import zlib

SS = 4  # hệ số vẽ phóng to trước khi thu nhỏ


class Canvas(object):
    """Ảnh RGBA, gốc toạ độ ở góc trên trái, trục y hướng xuống."""

    def __init__(self, width, height):
        self.width = width
        self.height = height
        self.px = bytearray(width * height * 4)  # trong suốt hoàn toàn

    # --- vẽ -------------------------------------------------------------

    def set(self, x, y, color):
        if x < 0 or y < 0 or x >= self.width or y >= self.height:
            return
        i = (y * self.width + x) * 4
        r, g, b = color[0], color[1], color[2]
        a = color[3] if len(color) > 3 else 255
        if a == 255:
            self.px[i] = r
            self.px[i + 1] = g
            self.px[i + 2] = b
            self.px[i + 3] = 255
            return
        # pha alpha lên nền có sẵn
        dst_a = self.px[i + 3]
        out_a = a + dst_a * (255 - a) // 255
        if out_a == 0:
            return
        for k, src in enumerate((r, g, b)):
            dst = self.px[i + k]
            self.px[i + k] = (src * a + dst * dst_a * (255 - a) // 255) // out_a
        self.px[i + 3] = out_a

    def rect(self, x0, y0, x1, y1, color):
        for y in range(int(y0), int(y1)):
            for x in range(int(x0), int(x1)):
                self.set(x, y, color)

    def polygon(self, points, color):
        """Tô đa giác đặc bằng quét dòng, quy tắc chẵn-lẻ."""
        if len(points) < 3:
            return
        ys = [p[1] for p in points]
        y_min = max(0, int(min(ys)))
        y_max = min(self.height - 1, int(max(ys)) + 1)

        for y in range(y_min, y_max + 1):
            cy = y + 0.5
            xs = []
            n = len(points)
            for i in range(n):
                x1, y1 = points[i]
                x2, y2 = points[(i + 1) % n]
                if (y1 <= cy < y2) or (y2 <= cy < y1):
                    t = (cy - y1) / (y2 - y1)
                    xs.append(x1 + t * (x2 - x1))
            xs.sort()
            for i in range(0, len(xs) - 1, 2):
                for x in range(int(round(xs[i])), int(round(xs[i + 1]))):
                    self.set(x, y, color)

    def ellipse(self, cx, cy, rx, ry, color, steps=64):
        import math
        pts = []
        for i in range(steps):
            a = 2.0 * math.pi * i / steps
            pts.append((cx + rx * math.cos(a), cy + ry * math.sin(a)))
        self.polygon(pts, color)

    def circle(self, cx, cy, r, color):
        self.ellipse(cx, cy, r, r, color)

    def capsule(self, x1, y1, x2, y2, r, color):
        """Đoạn thẳng dày hai đầu bo tròn — dùng cho tay, chân, cành cây."""
        import math
        dx, dy = x2 - x1, y2 - y1
        length = math.hypot(dx, dy)
        if length < 1e-6:
            self.circle(x1, y1, r, color)
            return
        nx, ny = -dy / length * r, dx / length * r
        self.polygon(
            [(x1 + nx, y1 + ny), (x2 + nx, y2 + ny), (x2 - nx, y2 - ny), (x1 - nx, y1 - ny)],
            color)
        self.circle(x1, y1, r, color)
        self.circle(x2, y2, r, color)

    # --- thu nhỏ và ghi -------------------------------------------------

    def downsample(self, factor=SS):
        out = Canvas(self.width // factor, self.height // factor)
        f2 = factor * factor
        for y in range(out.height):
            for x in range(out.width):
                r = g = b = a = 0
                for sy in range(factor):
                    for sx in range(factor):
                        i = ((y * factor + sy) * self.width + (x * factor + sx)) * 4
                        sa = self.px[i + 3]
                        r += self.px[i] * sa
                        g += self.px[i + 1] * sa
                        b += self.px[i + 2] * sa
                        a += sa
                o = (y * out.width + x) * 4
                if a > 0:
                    out.px[o] = r // a
                    out.px[o + 1] = g // a
                    out.px[o + 2] = b // a
                out.px[o + 3] = a // f2
        return out

    def to_png_bytes(self):
        raw = bytearray()
        stride = self.width * 4
        for y in range(self.height):
            raw.append(0)  # filter 0: None
            raw.extend(self.px[y * stride:(y + 1) * stride])

        def chunk(tag, data):
            out = struct.pack(">I", len(data)) + tag + data
            return out + struct.pack(">I", zlib.crc32(tag + data) & 0xFFFFFFFF)

        header = struct.pack(">IIBBBBB", self.width, self.height, 8, 6, 0, 0, 0)
        return (b"\x89PNG\r\n\x1a\n"
                + chunk(b"IHDR", header)
                + chunk(b"IDAT", zlib.compress(bytes(raw), 9))
                + chunk(b"IEND", b""))

    def save(self, path):
        with open(path, "wb") as f:
            f.write(self.to_png_bytes())


def new_canvas(width, height):
    """Khung vẽ phóng to SS lần; nhớ gọi downsample() trước khi lưu."""
    return Canvas(width * SS, height * SS)
