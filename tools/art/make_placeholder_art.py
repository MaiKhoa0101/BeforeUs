"""Sinh bộ hình tạm cho Before Us.

Chạy:  python tools/art/make_placeholder_art.py

Hình do agent vẽ bằng code, để Khoa có cái nhìn thấy ngay trong lúc chờ art thật.
Muốn đổi bảng màu hay tỉ lệ thì sửa file này rồi chạy lại — đừng sửa PNG bằng tay,
lần chạy sau sẽ ghi đè.

Phong cách: bóng phẳng, bảng màu hạn chế, không viền nét, không chuyển sắc.
Mọi hình trong cùng một nhóm dùng CHUNG khung và CHUNG đường chân, nên thay bằng
ảnh khác cùng kích thước là xong, không phải canh lại bố cục.
"""

import json
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from pnglib import SS, new_canvas  # noqa: E402

# --- bảng màu -----------------------------------------------------------

FUR_DARK = (72, 54, 42)
FUR_MID = (104, 78, 60)
FUR_LIGHT = (142, 110, 84)
SKIN = (198, 160, 126)
DARK = (34, 28, 24)

WOOD_DARK = (92, 64, 40)
WOOD_MID = (134, 98, 62)
WOOD_LIGHT = (168, 130, 86)

STONE_DARK = (84, 84, 92)
STONE_MID = (122, 122, 132)
STONE_LIGHT = (158, 158, 168)

LEAF_DARK = (72, 104, 56)
LEAF_MID = (106, 144, 76)

BERRY = (176, 62, 58)
BERRY_DARK = (132, 44, 42)

PALETTE = {
    "FUR_DARK": FUR_DARK, "FUR_MID": FUR_MID, "FUR_LIGHT": FUR_LIGHT,
    "SKIN": SKIN, "DARK": DARK,
    "WOOD_DARK": WOOD_DARK, "WOOD_MID": WOOD_MID, "WOOD_LIGHT": WOOD_LIGHT,
    "STONE_DARK": STONE_DARK, "STONE_MID": STONE_MID, "STONE_LIGHT": STONE_LIGHT,
    "LEAF_DARK": LEAF_DARK, "LEAF_MID": LEAF_MID,
    "BERRY": BERRY, "BERRY_DARK": BERRY_DARK,
}

# --- khung chuẩn --------------------------------------------------------

MEMBER_W, MEMBER_H = 96, 128
MEMBER_GROUND = 122          # đường chân: mọi thành viên đứng/ngồi trên cao độ này

OBJECT_W, OBJECT_H = 96, 96
OBJECT_GROUND = 88


def S(v):
    """Đổi toạ độ khung sang toạ độ khung phóng to."""
    return v * SS


# --- thành viên giai đoạn 1 (vượn người) --------------------------------

def draw_member(canvas, sitting, body, shade, tuft):
    """Một vượn người nhìn ngang-chéo, tay dài, lưng hơi gù.

    body:  hệ số to nhỏ (1.0 = vóc lớn)
    shade: (màu thân, màu sáng hơn cho ngực/mặt)
    tuft:  có túm lông trên đầu không
    """
    fur, fur_lit = shade
    cx = MEMBER_W / 2.0
    ground = MEMBER_GROUND

    if sitting:
        hip_y = ground - 16 * body
        chest_y = ground - 40 * body
        head_y = ground - 62 * body
        head_r = 15 * body
    else:
        hip_y = ground - 34 * body
        chest_y = ground - 58 * body
        head_y = ground - 84 * body
        head_r = 15 * body

    # chân
    if sitting:
        # ngồi xếp chân: hai ống chân nằm ngang về phía trước
        # bán kính 7 nên tâm phải nằm ở ground-7, nếu không hình thò xuống dưới đường chân
        canvas.capsule(S(cx - 4), S(ground - 14), S(cx + 20 * body), S(ground - 7 * body),
                       S(7 * body), fur)
        canvas.capsule(S(cx - 10), S(ground - 16), S(cx + 12 * body), S(ground - 7 * body),
                       S(7 * body), fur)
        canvas.ellipse(S(cx + 21 * body), S(ground - 7 * body), S(6 * body), S(5 * body), fur_lit)
    else:
        canvas.capsule(S(cx - 9 * body), S(hip_y), S(cx - 10 * body), S(ground - 7 * body),
                       S(7 * body), fur)
        canvas.capsule(S(cx + 9 * body), S(hip_y), S(cx + 10 * body), S(ground - 7 * body),
                       S(7 * body), fur)
        canvas.ellipse(S(cx - 10 * body), S(ground - 4 * body), S(8 * body), S(4 * body), fur_lit)
        canvas.ellipse(S(cx + 10 * body), S(ground - 4 * body), S(8 * body), S(4 * body), fur_lit)

    # thân, hơi gù về trước
    canvas.ellipse(S(cx), S((hip_y + chest_y) / 2.0), S(18 * body), S(20 * body), fur)
    canvas.ellipse(S(cx + 2 * body), S((hip_y + chest_y) / 2.0 + 3 * body),
                   S(11 * body), S(12 * body), fur_lit)

    # Tay dài, chạm gần đất khi đứng. Dùng tông SẪM hơn thân: cùng màu thì tay chìm vào
    # thân và bóng dáng mất đặc trưng vượn — kiểm bằng bản xem chữ của verify_art.py.
    arm = FUR_DARK if fur is not FUR_DARK else (52, 38, 30)
    shoulder_y = chest_y + 2 * body
    if sitting:
        canvas.capsule(S(cx - 19 * body), S(shoulder_y), S(cx - 13 * body), S(ground - 16),
                       S(5.5 * body), arm)
        canvas.capsule(S(cx + 19 * body), S(shoulder_y), S(cx + 26 * body), S(ground - 11),
                       S(5.5 * body), arm)
    else:
        canvas.capsule(S(cx - 21 * body), S(shoulder_y), S(cx - 25 * body), S(ground - 20),
                       S(5.5 * body), arm)
        canvas.capsule(S(cx + 21 * body), S(shoulder_y), S(cx + 25 * body), S(ground - 20),
                       S(5.5 * body), arm)

    # đầu
    canvas.circle(S(cx), S(head_y), S(head_r), fur)
    if tuft:
        canvas.ellipse(S(cx), S(head_y - head_r * 0.9), S(head_r * 0.55), S(head_r * 0.35), fur)

    # mõm nhô ra trước, màu sáng
    canvas.ellipse(S(cx + 3 * body), S(head_y + head_r * 0.42),
                   S(head_r * 0.72), S(head_r * 0.50), SKIN)

    # gờ lông mày
    canvas.ellipse(S(cx), S(head_y - head_r * 0.30), S(head_r * 0.86), S(head_r * 0.22), fur_lit)

    # mắt
    eye_y = head_y - head_r * 0.12
    canvas.circle(S(cx - head_r * 0.38), S(eye_y), S(head_r * 0.14), DARK)
    canvas.circle(S(cx + head_r * 0.30), S(eye_y), S(head_r * 0.14), DARK)

    # lỗ mũi
    canvas.circle(S(cx + 2 * body), S(head_y + head_r * 0.36), S(head_r * 0.08), DARK)


# --- vật thể ------------------------------------------------------------

def draw_food_nest(canvas):
    """Ổ đựng thức ăn: rổ đan bằng cành, bên trong có quả."""
    cx = OBJECT_W / 2.0
    top = OBJECT_GROUND - 30

    # lòng ổ (tối) rồi vành ổ (sáng) đè lên, tạo cảm giác lõm
    canvas.ellipse(S(cx), S(top + 6), S(30), S(12), WOOD_DARK)

    # quả bên trong
    canvas.circle(S(cx - 10), S(top + 2), S(7), BERRY)
    canvas.circle(S(cx + 6), S(top), S(8), BERRY)
    canvas.circle(S(cx + 16), S(top + 6), S(6), BERRY_DARK)
    canvas.ellipse(S(cx - 20), S(top + 4), S(9), S(5), LEAF_MID)

    # thành ổ
    canvas.polygon([(S(cx - 32), S(top + 6)), (S(cx + 32), S(top + 6)),
                    (S(cx + 24), S(OBJECT_GROUND)), (S(cx - 24), S(OBJECT_GROUND))], WOOD_MID)
    canvas.ellipse(S(cx), S(top + 6), S(32), S(11), WOOD_LIGHT)
    canvas.ellipse(S(cx), S(top + 6), S(25), S(7), WOOD_DARK)

    # quả nhô lên khỏi miệng ổ
    canvas.circle(S(cx + 6), S(top - 1), S(8), BERRY)
    canvas.circle(S(cx - 9), S(top + 1), S(7), BERRY)
    canvas.circle(S(cx + 3), S(top - 4), S(3), BERRY_DARK)

    # vài sợi đan ngang cho đỡ phẳng
    canvas.capsule(S(cx - 22), S(top + 14), S(cx + 22), S(top + 14), S(2), WOOD_DARK)
    canvas.capsule(S(cx - 19), S(top + 21), S(cx + 19), S(top + 21), S(2), WOOD_DARK)


def draw_club(canvas):
    """Cây gậy to: khúc gỗ nặng, đầu phình, dựng chéo."""
    x1, y1 = 26, OBJECT_GROUND - 8
    x2, y2 = 70, OBJECT_GROUND - 64

    canvas.capsule(S(x1), S(y1), S(x2), S(y2), S(8), WOOD_MID)
    canvas.circle(S(x2), S(y2), S(14), WOOD_MID)
    canvas.circle(S(x2 - 3), S(y2 + 2), S(9), WOOD_LIGHT)

    # vân và mấu
    canvas.capsule(S(x1 + 6), S(y1 - 8), S(x2 - 8), S(y2 + 12), S(2), WOOD_DARK)
    canvas.circle(S(x2 + 8), S(y2 - 6), S(5), WOOD_DARK)
    canvas.circle(S(x1 + 2), S(y1 - 2), S(7), WOOD_DARK)

    # bóng đổ trên đất
    canvas.ellipse(S(x1 + 4), S(OBJECT_GROUND - 4), S(16), S(4), (0, 0, 0, 60))


def draw_rock_pile(canvas):
    """Bãi đá chưa dọn: ba tảng chồng nhau, ba sắc xám."""
    g = OBJECT_GROUND
    canvas.ellipse(S(48), S(g - 6), S(34), S(6), (0, 0, 0, 55))

    canvas.polygon([(S(16), S(g)), (S(26), S(g - 20)), (S(42), S(g - 24)),
                    (S(52), S(g - 8)), (S(46), S(g))], STONE_MID)
    canvas.polygon([(S(44), S(g)), (S(52), S(g - 18)), (S(70), S(g - 22)),
                    (S(80), S(g - 6)), (S(74), S(g))], STONE_DARK)
    canvas.polygon([(S(34), S(g - 22)), (S(44), S(g - 40)), (S(60), S(g - 38)),
                    (S(64), S(g - 22))], STONE_LIGHT)

    # mặt vát sáng hơn, cho ra khối
    canvas.polygon([(S(26), S(g - 20)), (S(42), S(g - 24)), (S(36), S(g - 12))], STONE_LIGHT)
    canvas.polygon([(S(52), S(g - 18)), (S(70), S(g - 22)), (S(64), S(g - 10))], STONE_MID)


def draw_branch(canvas):
    """Nhánh cây gãy: cành chính nằm chéo, hai nhánh con, ba lá."""
    g = OBJECT_GROUND
    canvas.ellipse(S(48), S(g - 5), S(32), S(5), (0, 0, 0, 55))

    canvas.capsule(S(14), S(g - 5), S(82), S(g - 25), S(5), WOOD_MID)
    canvas.capsule(S(36), S(g - 13), S(30), S(g - 34), S(3), WOOD_DARK)
    canvas.capsule(S(60), S(g - 20), S(70), S(g - 42), S(3), WOOD_DARK)

    canvas.ellipse(S(28), S(g - 40), S(10), S(6), LEAF_MID)
    canvas.ellipse(S(72), S(g - 48), S(11), S(6), LEAF_DARK)
    canvas.ellipse(S(52), S(g - 28), S(9), S(5), LEAF_MID)

    # gân lá
    canvas.capsule(S(20), S(g - 40), S(36), S(g - 40), S(1), LEAF_DARK)
    canvas.capsule(S(62), S(g - 48), S(82), S(g - 48), S(1), LEAF_MID)


# --- danh mục -----------------------------------------------------------

MEMBERS = [
    ("member_stage1_male_stand", False, 1.00, (FUR_DARK, FUR_MID), False,
     "Vượn người vóc lớn, lông nâu sẫm, đứng hơi gù, hai tay dài buông gần đất."),
    ("member_stage1_male_sit", True, 1.00, (FUR_DARK, FUR_MID), False,
     "Cùng nhân vật trên, ngồi xếp chân về phía trước, một tay chống đất."),
    ("member_stage1_female_stand", False, 0.90, (FUR_MID, FUR_LIGHT), True,
     "Vượn người nhỏ hơn, lông nâu sáng, có túm lông trên đỉnh đầu, đứng."),
    ("member_stage1_female_sit", True, 0.90, (FUR_MID, FUR_LIGHT), True,
     "Cùng nhân vật trên, ngồi xếp chân."),
]

OBJECTS = [
    ("object_food_nest", draw_food_nest,
     "Ổ đựng thức ăn: rổ đan bằng cành nâu, miệng rộng, bên trong có mấy quả đỏ và một chiếc lá."),
    ("object_club", draw_club,
     "Cây gậy to: khúc gỗ nặng dựng chéo từ dưới trái lên trên phải, đầu trên phình to, có mấu."),
    ("object_rock_pile", draw_rock_pile,
     "Bãi đá chưa dọn: ba tảng đá xám chồng lệch nhau, ba sắc xám khác nhau, có bóng đổ."),
    ("object_branch", draw_branch,
     "Nhánh cây gãy nằm chéo trên mặt đất, hai nhánh con chĩa lên, ba chiếc lá xanh."),
]


def main():
    here = os.path.dirname(os.path.abspath(__file__))
    out_dir = os.path.abspath(os.path.join(
        here, "..", "..", "Assets", "Art", "Placeholder", "Resources", "BeforeUsPlaceholder"))
    if not os.path.isdir(out_dir):
        os.makedirs(out_dir)

    manifest = []

    for name, sitting, body, shade, tuft, note in MEMBERS:
        canvas = new_canvas(MEMBER_W, MEMBER_H)
        draw_member(canvas, sitting, body, shade, tuft)
        canvas.downsample().save(os.path.join(out_dir, name + ".png"))
        manifest.append({
            "file": name + ".png", "group": "member",
            "width": MEMBER_W, "height": MEMBER_H, "ground_line": MEMBER_GROUND,
            "mo_ta": note,
        })
        print("  " + name + ".png")

    for name, fn, note in OBJECTS:
        canvas = new_canvas(OBJECT_W, OBJECT_H)
        fn(canvas)
        canvas.downsample().save(os.path.join(out_dir, name + ".png"))
        manifest.append({
            "file": name + ".png", "group": "object",
            "width": OBJECT_W, "height": OBJECT_H, "ground_line": OBJECT_GROUND,
            "mo_ta": note,
        })
        print("  " + name + ".png")

    with open(os.path.join(out_dir, "..", "..", "manifest.json"), "w", encoding="utf-8") as f:
        json.dump({
            "generated_by": "tools/art/make_placeholder_art.py",
            "palette": {k: list(v) for k, v in PALETTE.items()},
            "files": manifest,
        }, f, ensure_ascii=False, indent=2)

    print("Xong: " + str(len(manifest)) + " hình.")


if __name__ == "__main__":
    main()
