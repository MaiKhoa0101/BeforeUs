"""Before Us — sinh các mục prompt của docs/prompt.md (v6, 09/11/2026).

Chạy:  python tools/art/gen_prompts.py <thư-mục-ra>
In ra các file: sec_1_1.md (quản Gemini), sec_2_4_to_2_6.md, sec_2_8.md, sec_3_4.md.
Dán đè đúng các mục tương ứng trong docs/prompt.md.

Nguyên tắc v6 (Khoa, 09/11):
- Hồ sơ đầy đủ theo phong cách tranh mẫu docs/art-reference/style_codex_leopard.png:
  vector phẳng kiểu áp phích, nền nhiều lớp bóng phẳng trong các tông của MỘT màu chủ đạo.
  Màu chủ đạo lấy theo VÙNG của loài, không lấy màu vàng của tranh mẫu.
- Đặc điểm giải phẫu chính xác, có câu "KHÔNG phải loài hiện đại" cho loài đã tuyệt chủng.
- Bộ mảnh ghi đúng số mảnh, tên từng mảnh, chỗ khớp; cấm chốt, đinh, viền, chữ.
- Mỗi prompt chỉ ra MỘT hình; nền màu hồng sen phẳng để xóa nền (Gemini không làm nền trong suốt thật).
"""
import os
import sys

ONE = "ONE IMAGE ONLY, one single asset, no panels."

STYLE_REF = "Use the attached style reference only for the drawing style, not for its colors, animal or setting"
STYLE = ("Flat vector game illustration: the animal is drawn with clean crisp shapes and hard-edged cel shading in three to four "
         "tones per color (a base tone, one shadow tone, one dark accent tone and a bright highlight along the top edges), thin "
         "dark brown contour lines, coat patterns such as spots, stripes and scales drawn as clean flat shapes, exaggerated "
         "powerful anatomy with clearly defined muscle masses, bold readable silhouette, no texture noise, no gradients on the "
         "animal, no painterly brush strokes")

BG_REMOVE = "plain flat solid magenta background (#FF00FF) filling the whole image, nothing else in the background"

TECH_RIG = ("single animal, full body, strict flat side view facing right, no perspective, no foreshortening, neutral relaxed "
            "pose exactly as described above, mouth closed, no snarl, " + BG_REMOVE + ", no ground, no grass, no cast shadow, no glow, no white "
            "outline or sticker border around the animal; this is a clean character reference that will be cut into parts for "
            "2D skeletal animation")

AVOID = ("no text, no letters, no labels, no titles, no numbers, no watermark, no panels, no collage, no frame, no white "
         "outline or sticker border, no drop shadow, no blood, no gore, no wounds, no modern humans, no modern objects, not "
         "photorealistic, not a 3D render, not pixel art, no cartoon or chibi proportions, no extra limbs, no duplicated parts")

MOOD_CODEX = "fierce, primal and dramatic mood, the animal caught in a moment of pure instinct"
AVOID_CODEX = "not cute, no calm, sleepy or stiff standing pose"

CODEX_BG = ("background in the same flat vector poster style: four to five flat overlapping silhouette layers (foreground "
            "plants, middle trees, hills, far mountains, sky) built only from tones of the region's dominant hue, darkest and "
            "most saturated in the foreground, lighter and paler toward the horizon, a large soft glowing light source in the "
            "sky, simplified iconic silhouettes of the region's plants and landforms")

COMPOSITION = ("wide 16:9 landscape illustration with the background reaching every edge, no border; the animal large and "
               "fully inside the right half of the frame in a dramatic three-quarter view, standing on a strong dark foreground "
               "element of its region (branch, rock, root, bank or ice ledge); the left half of the frame is calm and open "
               "(glowing sky and distant layers only) leaving clean empty space for text")

# ---------------------------------------------------------------------------
# Khung xương: dáng để rig, danh sách mảnh có khớp, thứ tự xếp
# ---------------------------------------------------------------------------
RIG = {
    "A": "standing in a neutral pose, all four legs clearly visible, near legs and far legs staggered so they do not overlap, head level with the shoulders, tail extended straight back and slightly down, far-side legs one shade darker than near-side legs",
    "B": "body stretched out almost straight along the ground with only a very gentle wave, head slightly raised at the front, the whole body visible with no part crossing over another",
    "C": "low sprawling stance with the belly close to the ground, legs bent out to the side and all four visible without overlapping, long tail extended straight back on the ground",
    "D": "side view with both wings raised and spread in a gliding position, near wing in front of the body and far wing behind, legs tucked under the body with talons visible, tail fanned slightly",
    "E": "standing in a neutral pose, all four pillar-like legs visible and staggered, trunk hanging straight down and slightly forward, tusks fully visible, tail hanging down",
    "F": "standing on all four limbs in a knuckle-walking pose, long arms and shorter legs all visible and staggered, head level",
}

LEG4 = lambda segs: [f"{side} {end} leg {seg}" for side in ("near", "far") for end in ("front", "hind") for seg in segs]

# (piece name, where it joins) — these are the ONLY pieces
PARTS = {
    "A": [("head (skull with upper jaw and ears)", "joins the neck behind the skull"),
          ("lower jaw", "hinges under the ear"),
          ("neck", "joins the torso in front of the shoulders"),
          ("torso, chest to hips in ONE piece", "carries the neck, legs and tail"),
          ("tail base", "joins the hips"), ("tail middle", "joins the tail base"), ("tail tip", "joins the tail middle"),
          ] + [(f"{side} front leg upper part (shoulder to elbow)", "joins the torso at the shoulder") for side in ("near", "far")] +
         [(f"{side} front leg lower part (elbow to wrist)", "joins the upper part at the elbow") for side in ("near", "far")] +
         [(f"{side} front paw", "joins at the wrist") for side in ("near", "far")] +
         [(f"{side} hind leg upper part (hip to knee)", "joins the torso at the hip") for side in ("near", "far")] +
         [(f"{side} hind leg lower part (knee to ankle)", "joins the upper part at the knee") for side in ("near", "far")] +
         [(f"{side} hind paw", "joins at the ankle") for side in ("near", "far")],
    "B": [("head (skull with upper jaw)", "joins the front end of the body"),
          ("lower jaw", "hinges at the back of the head"),
          ("body, from neck to tail tip in ONE long continuous piece, stretched straight", "bones are added inside it later")],
    "C": [("head (skull with upper jaw)", "joins the neck"), ("lower jaw", "hinges at the back of the head"),
          ("neck", "joins the torso"), ("torso in ONE piece", "carries neck, legs and tail"),
          ("tail piece 1", "joins the hips"), ("tail piece 2", "joins tail piece 1"), ("tail piece 3", "joins tail piece 2"),
          ("tail piece 4, the tip", "joins tail piece 3")] +
         [(f"{side} {end} leg upper part", "joins the torso") for side in ("near", "far") for end in ("front", "hind")] +
         [(f"{side} {end} leg lower part with foot and claws", "joins at the elbow or knee") for side in ("near", "far") for end in ("front", "hind")],
    "D": [("head with beak", "joins the body at the neck"), ("body in ONE piece", "carries everything"),
          ("near wing inner part (arm)", "joins the body at the shoulder"),
          ("near wing outer part (long flight feathers)", "joins the inner part at the wrist"),
          ("far wing in ONE piece, one shade darker", "joins the body behind the shoulder"),
          ("tail fan", "joins the rear of the body"),
          ("near leg with talons", "joins the belly"), ("far leg with talons, one shade darker", "joins the belly")],
    "E": [("head (skull without trunk, without tusks)", "joins the torso at the neck"),
          ("trunk piece 1, the base", "joins the face"), ("trunk piece 2", "joins trunk piece 1"),
          ("trunk piece 3, the tip", "joins trunk piece 2"),
          ("tusks as ONE piece", "fixed to the face"), ("ear", "fixed to the side of the head"),
          ("torso in ONE piece", "carries head, legs and tail"), ("tail", "joins the hips")] +
         [(f"{side} {end} leg upper part", "joins the torso") for side in ("near", "far") for end in ("front", "hind")] +
         [(f"{side} {end} leg lower part with foot", "joins at the elbow or knee") for side in ("near", "far") for end in ("front", "hind")],
    "F": [("head (skull with upper jaw)", "joins the torso"), ("lower jaw", "hinges under the ear"),
          ("torso in ONE piece", "carries head and limbs")] +
         [(f"{side} arm upper part", "joins at the shoulder") for side in ("near", "far")] +
         [(f"{side} forearm", "joins at the elbow") for side in ("near", "far")] +
         [(f"{side} hand", "joins at the wrist") for side in ("near", "far")] +
         [(f"{side} thigh", "joins at the hip") for side in ("near", "far")] +
         [(f"{side} shin", "joins at the knee") for side in ("near", "far")] +
         [(f"{side} foot", "joins at the ankle") for side in ("near", "far")],
}
FRAME_NAME = {"A": "thú bốn chân", "B": "rắn", "C": "bò sát thân dài", "D": "chim", "E": "voi", "F": "vượn lớn"}

# Per-species changes to the parts list: (remove names containing, add (name, join))
PARTS_EDIT = {
    "leopard": ([], [("near ear", "fixed to the top of the head"), ("far ear", "fixed to the top of the head, one shade darker")]),
    "hyena_pack": ([], [("near ear", "fixed to the top of the head"), ("far ear", "fixed to the top of the head, one shade darker")]),
    "hare": ([], [("near long ear", "fixed to the top of the head"), ("far long ear", "fixed to the top of the head, one shade darker")]),
    "sabertooth": (["tail middle"], []),
    "homotherium": (["tail middle"], []),
    "boar_piglet": (["tail middle", "tail tip"], []),
    "hippo_gorgops": (["tail middle", "tail tip"], []),
    "deinotherium": (["tusks as ONE piece"], [("lower jaw carrying both downward-curving tusks", "hinges under the head")]),
    "stegodon_dwarf": ([], []),
    "leptoptilos_robustus": (["near leg with talons", "far leg with talons"],
                             [("near long leg upper part", "joins the belly"), ("near long leg lower part with foot", "joins at the knee"),
                              ("far long leg upper part, one shade darker", "joins the belly"),
                              ("far long leg lower part with foot, one shade darker", "joins at the knee")]),
}


def parts_for(s):
    lst = list(PARTS[s["frame"]])
    rem, add = PARTS_EDIT.get(s["id"], ([], []))
    lst = [p for p in lst if not any(r in p[0] for r in rem)] + add
    return lst


def j(*xs):
    return ". ".join(x.strip().rstrip(".") for x in xs if x)


def parts_prompt(s):
    lst = parts_for(s)
    n = len(lst)
    items = "; ".join(f"({i}) {name}, {join}" for i, (name, join) in enumerate(lst, 1))
    return j(ONE,
             f"2D cut-out parts sheet of exactly the same {s['short']} as the attached rig picture, same side view, same colors, same line style and same scale",
             f"Draw exactly {n} separate pieces, no more and no fewer, and nothing else: {items}",
             "Each piece shows only that body part, cut cleanly along the joint line; the joint end of every piece is extended and rounded by about fifteen percent so it stays hidden under the neighbouring piece when it rotates",
             "Never slice a piece into extra segments; the torso stays one single piece",
             "Lay the pieces out in reading order following the numbers, left to right and top to bottom, with wide empty gaps between them, all pieces drawn flat in side view at the same scale as the rig picture",
             "No pins, rivets, bolts, hinge circles, dots or joint markers; no outlines, white borders, glows or shadows around the pieces; no labels or numbers drawn in the image",
             BG_REMOVE, STYLE_REF, STYLE, AVOID)
SPECIES = [{'sec': '3.1',
  'id': 'leopard',
  'codex': 'leopard',
  'vi': 'Báo hoa mai',
  'sci': 'Panthera pardus',
  'frame': 'A',
  'wide': False,
  'notes': ['Thân dài, chân ngắn hơn sư tử, đuôi dài; lông vàng, đốm hoa thị.', 'Thêm lớp và xương `ear` để cụp tai khi bỏ chạy.'],
  'subject': 'An adult African leopard, lean muscular body, long tail, rosette-spotted golden coat, intense eyes',
  'parts_extra': 'ears as separate small pieces',
  'anims': [('idle', 'thở, vẫy đuôi'),
            ('walk', ''),
            ('run', ''),
            ('stalk', 'hạ thấp người, bò sát đất'),
            ('attack', 'vồ'),
            ('stagger', 'lắc đầu, chân loạng choạng'),
            ('flee', 'quay đầu chạy, cụp tai'),
            ('grab_drag', 'lùi, cổ ghì xuống'),
            ('panic', 'chạy nước rút')],
  'instinct': 'ears flattened, eyes locked on prey, lips pulled back over long canines, body coiled like a spring about to pounce',
  'instinct_vi': 'Rình rồi vồ: tai cụp, mắt khóa mục tiêu, thân nén như lò xo.',
  'codex_scene': 'A leopard crouched along a thick mossy branch high in the rainforest, ears flat, staring straight down at its prey, one '
                 'paw reaching forward, about to drop',
  'why': 'Trên cành rêu giữa rừng mưa, sắp đổ xuống (vùng 0). Chuyện SK 54 kể bằng chữ, không vẽ.',
  'region': 'rainforest',
  'mvp': True},
 {'sec': '3.2',
  'id': 'python',
  'codex': 'python',
  'vi': 'Trăn đá châu Phi',
  'sci': 'Python sebae',
  'frame': 'B',
  'wide': True,
  'notes': ['Thân to, dài; đầu hình tam giác; vằn ô-liu và nâu hạt dẻ có viền sẫm, bụng nhạt.', 'Dùng khung 16:9.'],
  'subject': 'A large African rock python, thick heavy body, triangular head, irregular olive and chestnut blotches with dark edges, pale '
             'belly',
  'extra': [('beast_python_coil.png',
             'Mảnh vòng quấn cho trạng thái Siết (ngoại lệ của khung B)',
             'three tight coils of the same python body wrapped upright around an empty cylindrical space, seen from the side, the coils '
             'overlapping naturally, no head, cut cleanly at both ends so it can join the rest of the body, same scale and colors as the '
             'reference image')],
  'anims': [('idle', 'thân lượn rất nhẹ'),
            ('slither', ''),
            ('stalk', 'nằm sát đất, đầu đứng im'),
            ('attack', 'phóng thẳng nửa thân trước'),
            ('stagger', 'co giật, đầu rụt lại'),
            ('flee', 'trườn nhanh, sóng thân dài'),
            ('grab_squeeze', 'đổi sang mảnh vòng quấn, siết theo nhịp')],
  'instinct': 'cold unblinking yellow eyes, jaws open showing curved teeth, front of the body pulled back ready to strike',
  'instinct_vi': 'Phục kích: đầu rụt về sau hình chữ S, há miệng, sắp phóng.',
  'codex_scene': 'A huge African rock python rising from its coils among giant buttress roots on the rainforest floor, head drawn back in '
                 'a tight S, jaws wide open showing curved teeth, about to strike',
  'why': 'Rễ bạnh khổng lồ dưới tán rừng mưa, thế phóng.',
  'region': 'rainforest',
  'mvp': True},
 {'sec': '3.3',
  'id': 'hyena_pack',
  'codex': 'chasmaporthetes',
  'vi': 'Linh cẩu (bầy, theo *Chasmaporthetes*)',
  'sci': '',
  'frame': 'A',
  'wide': False,
  'notes': ['Đây **không** phải linh cẩu đốm ngày nay. *Chasmaporthetes* có chân dài, mảnh như báo săn.',
            'Bầy có 3 con dùng chung một bộ xương. Khác biệt chỉ là đổi lớp `torso` và `head` bằng Sprite Library (2 biến thể đốm, prompt '
            'bên dưới).'],
  'subject': 'An extinct hunting hyena (Chasmaporthetes), slender build with long thin legs like a cheetah, back only slightly sloping, '
             'narrow head, short sandy coat with sparse dark spots, small rounded ears',
  'parts_extra': 'ears as separate small pieces',
  'extra': [('beast_hyena_pack_var1.png / _var2.png',
             'Biến thể đốm (dùng `edit_asset_id` trên mảnh `torso` và mảnh `head`, chạy 2 lần)',
             'the same body part with a slightly different dark spot pattern and a slightly darker muzzle, identical shape, size and '
             'outline')],
  'anims': [('idle', ''), ('run', 'chạy sải dài'), ('attack', 'lao tới, há miệng'), ('stagger', ''), ('flee', 'cụp đuôi, cúi đầu')],
  'instinct': 'hackles raised along the spine, head low, lips curled, a relentless hunting stare',
  'instinct_vi': 'Săn theo bầy: lông gáy dựng, đầu thấp, ánh mắt không buông.',
  'codex_scene': 'Three long-legged hunting hyenas running flat out in a tight pack across the savanna grass, the leader in front with '
                 'jaws open, dust kicked up behind them',
  'why': 'Cả bầy đang lao qua đồng cỏ, bụi tung.',
  'region': 'savanna',
  'mvp': True},
 {'sec': '3.4',
  'id': 'sabertooth',
  'codex': 'megantereon',
  'vi': 'Hổ răng kiếm',
  'sci': 'Megantereon',
  'frame': 'A',
  'wide': False,
  'notes': ['Cỡ báo đốm, **không** to bằng sư tử. Chân trước rất khỏe; đuôi ngắn, chỉ cần 2 đoạn.',
            'Nanh trên dài, dẹt, chìa xuống dưới cằm. Nanh **gắn vào mảnh `head`**, không gắn vào `jaw`.',
            'Khoa đã vẽ một mẫu hổ răng kiếm: dùng mẫu đó làm `reference_asset_id` khi sinh hình gốc.'],
  'subject': 'A Megantereon sabertooth cat the size of a jaguar, very powerful muscular forelimbs and shoulders, short tail, long flat '
             'dagger-like upper canines reaching below the chin, tawny coat with faint pale markings',
  'parts_extra': 'tail as only two segments, the long upper canines kept attached to the head piece and not to the lower jaw',
  'anims': [('idle', ''),
            ('walk', ''),
            ('attack', 'chồm lên, hai chân trước vươn ra, há miệng'),
            ('stagger', ''),
            ('enrage', 'gầm, lưng cong, cụp tai'),
            ('panic', '')],
  'instinct': 'snarling with the long sabers fully bared, shoulders rolling, massive forelimbs ready to grab, menacing glare',
  'instinct_vi': 'Gầm, phô răng kiếm, chân trước khổng lồ sẵn sàng ghì.',
  'codex_scene': 'A Megantereon sabertooth bursting out of tall dry savanna grass beside an acacia, snarling with its long sabers bared, '
                 'massive forelimbs reaching forward',
  'why': 'Phóng ra từ cỏ khô cao cạnh cây keo.',
  'region': 'savanna',
  'mvp': True},
 {'sec': '3.5',
  'id': 'small_antelope',
  'codex': 'small_antelope',
  'vi': 'Linh dương nhỏ (con mồi)',
  'sci': '',
  'frame': 'A',
  'wide': False,
  'notes': ['Sừng gắn vào mảnh `head`.'],
  'subject': 'A small slender antelope like a duiker, short straight horns, reddish-brown coat, large dark eyes, thin legs',
  'parts_extra': 'short horns kept attached to the head piece',
  'anims': [('idle', ''), ('run', 'nhảy'), ('stagger', ''), ('panic', 'nhảy zig-zag')],
  'instinct': 'nostrils flared, eyes wide, ready to bolt at any sound',
  'instinct_vi': 'Cảnh giác: mũi phồng, tai dựng, sẵn sàng phóng đi.',
  'codex_scene': 'A small antelope in a high startled leap along the shore of a savanna lake at dawn, legs tucked, ears back',
  'why': 'Bờ hồ giữa savan lúc bình minh (chuyện Kanjera South).',
  'region': 'savanna',
  'mvp': True},
 {'sec': '3.6',
  'id': 'boar_piglet',
  'codex': 'boar_piglet',
  'vi': 'Lợn rừng con (con mồi)',
  'sci': '',
  'frame': 'A',
  'wide': False,
  'notes': ['Đuôi chỉ cần 1 đoạn. Không có tranh hồ sơ riêng có nguồn lịch sử (xem `bestiary.md` §2.3); tranh dưới đây chỉ là hình minh '
            'họa lớp 1.'],
  'subject': 'A young wild boar piglet, stocky round body, light brown coat with pale horizontal stripes, small snout',
  'parts_extra': 'tail as a single small piece',
  'anims': [('idle', ''), ('run', ''), ('stagger', ''), ('panic', '')],
  'instinct': 'bristles raised along the back, head lowered, stubborn and fearless',
  'instinct_vi': 'Liều lĩnh: lông gáy dựng, cúi đầu, không chịu lùi.',
  'codex_scene': 'A wild boar piglet squealing defiantly with bristles raised and front legs planted at the edge of a rainforest stream '
                 'among ferns',
  'why': 'Ven suối giữa dương xỉ rừng mưa, thế ương ngạnh.',
  'region': 'rainforest',
  'mvp': True},
 {'sec': '3.7',
  'id': 'hare',
  'codex': 'hare',
  'vi': 'Thỏ rừng (con mồi)',
  'sci': '',
  'frame': 'A',
  'wide': False,
  'notes': ['Thêm lớp và xương `ear`; chân sau dài. Không có nguồn lịch sử riêng; tranh chỉ minh họa lớp 1.'],
  'subject': 'A wild hare, long ears, long powerful hind legs, brown-grey coat',
  'parts_extra': 'long ears as separate pieces, short tail as a single piece',
  'anims': [('idle', ''), ('run', ''), ('stagger', ''), ('panic', 'nhảy ngang')],
  'instinct': 'eyes wide, hind legs coiled for an explosive jump',
  'instinct_vi': 'Bật nhảy: chân sau nén, tai dựng.',
  'codex_scene': 'A wild hare in an explosive sideways leap across a windy savanna hilltop, ears streaming back, fast clouds above',
  'why': 'Đồi savan lộng gió, cú bật ngang.',
  'region': 'savanna',
  'mvp': True},
 {'sec': '3.8',
  'id': 'eagle',
  'codex': 'taung_eagle',
  'vi': 'Đại bàng lớn (chuyện đứa trẻ Taung, MVP-2)',
  'sci': '',
  'frame': 'D',
  'wide': False,
  'notes': ['Lấy dáng đại bàng mào châu Phi: mào lông trên đầu, ngực sọc, móng rất to.',
            'Không biết chính xác loài chim nào đã bắt đứa trẻ Taung, nên không ghi tên loài cụ thể trong game.'],
  'subject': 'A large powerful African eagle, short crest on the head, dark grey-brown back, pale chest with dark bars, huge yellow feet '
             'and talons',
  'anims': [('glide', ''),
            ('flap', ''),
            ('stalk', 'lượn vòng, đầu nhìn xuống'),
            ('attack', 'bổ nhào, cụp cánh, móng đưa ra trước'),
            ('grab_drag', 'vỗ cánh mạnh bay lên, móng quặp'),
            ('flee', '')],
  'instinct': 'crest raised, hooked beak open in a scream, piercing yellow eyes, huge talons spread wide',
  'instinct_vi': 'Bổ nhào: mào dựng, mỏ há, móng xòe.',
  'codex_scene': 'A huge eagle diving out of the sky with wings half folded and enormous talons thrust forward, screaming, above a rocky '
                 'limestone kopje on the savanna, far below a tiny group of early hominins as small silhouettes near a lone acacia',
  'why': 'Từ trời lao xuống mỏm đá savan, người vượn chỉ là bóng nhỏ.',
  'region': 'savanna',
  'mvp': True},
 {'sec': '4.1',
  'id': 'machairodus',
  'codex': 'machairodus',
  'vi': 'Hổ răng kiếm khổng lồ',
  'sci': 'Machairodus kabir',
  'frame': 'A',
  'stage': 1,
  'must': 'To hơn hổ; nanh dài, dẹt, gắn vào mảnh `head`.',
  'subject': 'A huge Machairodus sabertooth cat bigger than a tiger, long flat blade-like upper canines, long legs, plain tawny coat',
  'parts_extra': 'the long upper canines kept attached to the head piece and not to the lower jaw',
  'anims': 'idle, walk, run, attack, stagger, enrage',
  'instinct': 'tiger-sized power, sabers bared, deep growl, heavy shoulders rolling forward',
  'instinct_vi': 'Bá chủ hồ nước: to hơn hổ, gầm trầm.',
  'codex_scene': 'A giant Machairodus sabertooth striding along a misty lakeshore at early morning in the Late Miocene Chad basin with its '
                 'head low and sabers bared, long-necked giraffe-like animals and small three-toed horses far away near the water',
  'why': 'Bờ hồ Chad có sương sớm, thú khác ở xa.',
  'region': 'lakeshore',
  'mvp': False},
 {'sec': '4.2',
  'id': 'dinofelis',
  'codex': 'dinofelis',
  'vi': 'Báo răng kiếm giả',
  'sci': 'Dinofelis',
  'frame': 'A',
  'stage': 2,
  'must': 'Nanh **ngắn** hơn răng kiếm thật.',
  'subject': 'A Dinofelis false sabertooth cat built like a large jaguar, short slightly flattened upper canines, heavy forelimbs, faint '
             'spotted coat',
  'anims': 'idle, walk, stalk, attack, stagger, flee',
  'instinct': 'slinking low, eyes glowing, short fangs bared',
  'instinct_vi': 'Kẻ rình đêm: trườn thấp, mắt sáng.',
  'codex_scene': 'A Dinofelis slinking out of a dark limestone cave mouth in a rocky savanna hill at night under a full moon, head low, '
                 'eyes catching the moonlight, fangs bared',
  'why': 'Ban đêm, trăng tròn, cửa hang trên đồi savan.',
  'region': 'savanna',
  'mvp': False},
 {'sec': '4.3',
  'id': 'deinotherium',
  'codex': 'deinotherium',
  'vi': 'Khủng tượng ngà cong',
  'sci': 'Deinotherium',
  'frame': 'E',
  'stage': 2,
  'must': 'Ngà mọc ở **hàm dưới**, cong xuống và ra sau; cao ~4 m. Khung E nhưng thêm mảnh `jaw` mang ngà (không có mảnh `tusks` riêng).',
  'subject': 'A gigantic Deinotherium proboscidean, very tall with long legs, short trunk, two tusks growing from the lower jaw and '
             'curving down and backward, grey wrinkled skin',
  'parts_extra': 'instead of a separate tusk piece, a lower jaw piece that carries both downward-curving tusks',
  'anims': 'idle, walk, charge, stagger',
  'instinct': 'towering, head raised, downward tusks swinging, trumpeting',
  'instinct_vi': 'Khổng lồ: ngẩng đầu, ngà hàm dưới vung xuống.',
  'codex_scene': 'A towering Deinotherium rearing its head and trumpeting among savanna woodland trees in midday heat haze, '
                 'downward-curving tusks swinging, one huge foot raised, seen from a low angle so it dwarfs the trees',
  'why': 'Góc thấp nhìn lên, rừng thưa savan ban trưa.',
  'region': 'savanna',
  'mvp': False},
 {'sec': '4.4',
  'id': 'crocodile_horned',
  'codex': 'crocodile_horned',
  'vi': 'Cá sấu sừng',
  'sci': 'Crocodylus anthropophagus',
  'frame': 'C',
  'wide': True,
  'stage': 3,
  'must': 'Gờ "sừng" trên mắt; dài ~5 m.',
  'subject': 'A large horned crocodile with bony horn-like ridges above each eye, long heavy body, armored back',
  'anims': 'idle, crawl, stalk, attack, grab_drag, stagger, flee',
  'instinct': 'jaws gaping, horn ridges above the eyes, armored body tense',
  'instinct_vi': 'Kẻ phục kích dưới nước: há hàm, sừng trên mắt.',
  'codex_scene': 'A horned crocodile lunging out of muddy lake water among reeds with jaws gaping wide, water splashing off its armored '
                 'head, under a stormy sky over the lakes of Olduvai',
  'why': 'Trời giông, lao ra khỏi hồ Olduvai.',
  'region': 'lakeshore',
  'mvp': False},
 {'sec': '4.5',
  'id': 'pelorovis',
  'codex': 'pelorovis',
  'vi': 'Trâu sừng cong',
  'sci': 'Pelorovis oldowayensis',
  'frame': 'A',
  'stage': 3,
  'must': 'Sừng cong nửa vòng tròn, rất dài; gắn vào mảnh `head`.',
  'subject': 'A Pelorovis giant buffalo with enormous horns forming a wide half circle, long legs, dark brown coat',
  'parts_extra': 'the huge curved horns kept attached to the head piece',
  'anims': 'idle, walk, charge, stagger, panic',
  'instinct': 'snorting, enormous horns aimed forward',
  'instinct_vi': 'Húc càn: cúi đầu, cặp sừng khổng lồ chĩa tới.',
  'codex_scene': 'A Pelorovis bull charging head-down across a green rainy-season lakeside plain at sunrise with its enormous half-circle '
                 'horns aimed forward, dust and spray exploding behind it, the herd behind',
  'why': 'Mùa mưa ven hồ, bình minh, đang lao tới.',
  'region': 'lakeshore',
  'mvp': False},
 {'sec': '4.6',
  'id': 'hippo_gorgops',
  'codex': 'hippo_gorgops',
  'vi': 'Hà mã khổng lồ',
  'sci': 'Hippopotamus gorgops',
  'frame': 'A',
  'stage': 3,
  'must': 'Mắt và mũi nằm cao trên sọ.',
  'subject': 'A very large extinct hippopotamus with eyes and nostrils set high on top of the head like periscopes, massive barrel body, '
             'short legs, grey-pink skin',
  'parts_extra': 'tail as a single small piece',
  'anims': 'idle, walk, charge, stagger',
  'instinct': 'mouth gaping in a threat yawn showing huge tusks',
  'instinct_vi': 'Đe dọa: há miệng khoe nanh.',
  'codex_scene': 'A giant hippo surging out of a shallow reedy lake in a threat yawn at evening, mouth gaping to show huge tusks, water '
                 'streaming off its body, white egrets scattering',
  'why': 'Chiều tối, trồi lên khỏi hồ, cò bay tán loạn.',
  'region': 'lakeshore',
  'mvp': False},
 {'sec': '4.7',
  'id': 'palaeoloxodon_recki',
  'codex': 'palaeoloxodon_recki',
  'vi': 'Voi ngà thẳng',
  'sci': 'Palaeoloxodon recki',
  'frame': 'E',
  'stage': 4,
  'must': 'Ngà gần thẳng.',
  'subject': 'A tall extinct straight-tusked elephant, long nearly straight tusks, high domed head, long legs',
  'anims': 'idle, walk, charge, stagger',
  'instinct': 'ears flared, trunk raised, long straight tusks forward',
  'instinct_vi': 'Uy nghi: tai xòe, vòi giơ cao.',
  'codex_scene': 'A lone straight-tusked elephant trumpeting with its trunk raised and ears flared on cracked dry mudflats beside a '
                 'shrinking lake in the dry season, dust swirling',
  'why': 'Bãi bùn khô nứt mùa khô ven hồ, một con đơn độc.',
  'region': 'lakeshore',
  'mvp': False},
 {'sec': '4.8',
  'id': 'theropithecus_oswaldi',
  'codex': 'theropithecus_oswaldi',
  'vi': 'Khỉ đầu chó khổng lồ',
  'sci': 'Theropithecus oswaldi',
  'frame': 'A',
  'stage': 4,
  'must': 'Lớn, dáng giống khỉ gelada; đi bốn chân nên dùng khung A.',
  'subject': 'A giant gelada-like baboon, heavy muscular body, long brown mane on the shoulders, bare chest patch, long face',
  'parts_extra': 'the shoulder mane kept on the torso piece',
  'anims': 'idle, walk, run, attack, stagger, flee',
  'instinct': 'lips flipped up to bare huge canines, mane bristling',
  'instinct_vi': 'Đầu đàn: lật môi khoe nanh, bờm dựng.',
  'codex_scene': 'A giant male baboon on a rock baring huge canines in a lip-flip threat, mane flared, the troop behind him on a foggy '
                 'grassy highland slope above the savanna',
  'why': 'Sườn cao nguyên sương mù phía trên savan.',
  'region': 'savanna',
  'mvp': False},
 {'sec': '4.9',
  'id': 'pachycrocuta',
  'codex': 'pachycrocuta',
  'vi': 'Linh cẩu khổng lồ',
  'sci': 'Pachycrocuta brevirostris',
  'frame': 'A',
  'stage': 4,
  'must': 'To, nặng, mặt ngắn. Trong tranh **không** có xương người.',
  'subject': 'A giant short-faced hyena, very bulky and heavy, massive broad head and jaws, strongly sloping back, short sturdy legs, '
             'coarse spotted coat',
  'anims': 'idle, walk, run, attack, stagger, flee',
  'instinct': 'massive jaws open, sloping back bristling, menacing heavy head',
  'instinct_vi': 'Kẻ nghiền xương: hàm khổng lồ, lưng dốc dựng lông.',
  'codex_scene': 'A giant short-faced hyena striding out of a snowy birch forest in northern China with a huge animal leg bone clamped in '
                 'its jaws, hackles raised, breath steaming',
  'why': 'Rừng bạch dương tuyết, ngậm xương thú.',
  'region': 'eurasia_temperate',
  'mvp': False},
 {'sec': '4.10',
  'id': 'gigantopithecus',
  'codex': 'gigantopithecus',
  'vi': 'Vượn khổng lồ',
  'sci': 'Gigantopithecus blacki',
  'frame': 'F',
  'stage': 4,
  'must': 'Màu lông là **phỏng đoán**. Cảnh núi đá vôi kiểu miền Bắc Việt Nam và Quảng Tây.',
  'subject': 'A colossal ape about three meters tall, very heavy body, thick reddish-brown fur, large heavy-browed face',
  'anims': 'idle, walk, eat, stagger',
  'instinct': 'colossal, chest puffed out, mouth open showing large teeth',
  'instinct_vi': 'Người khổng lồ của rừng đá vôi: to lớn, uy hiếp.',
  'codex_scene': 'A colossal ape rising up on its hind legs among tall bamboo in a misty karst valley, arms spread wide, mouth open in a '
                 'roar, steep limestone towers and drizzle behind',
  'why': 'Núi đá vôi, mưa phùn, đứng dậy gầm (dáng là tưởng tượng).',
  'region': 'southeast_asia',
  'mvp': False},
 {'sec': '4.11',
  'id': 'homotherium',
  'codex': 'homotherium',
  'vi': 'Hổ răng mã tấu',
  'sci': 'Homotherium latidens',
  'frame': 'A',
  'stage': 4,
  'must': 'Lưng dốc (chân trước dài hơn chân sau); nanh ngắn hơn *Smilodon*, gắn vào `head`.',
  'subject': 'A Homotherium scimitar-toothed cat, long front legs and shorter hind legs making a sloping back, short flat serrated '
             'canines, short tail, pale coat',
  'parts_extra': 'tail as only two segments, canines kept attached to the head piece',
  'anims': 'idle, walk, run, attack, stagger, enrage',
  'instinct': 'long front legs striding, sloping back, serrated fangs bared',
  'instinct_vi': 'Kẻ săn bền bỉ: lưng dốc, sải chân dài.',
  'codex_scene': 'A Homotherium prowling through tall golden autumn reeds on the shore of a cold shallow lake in Ice Age Germany, head '
                 'low, serrated fangs bared, stepping past a wooden spear lying in the reeds',
  'why': 'Lau sậy mùa thu, giáo gỗ bỏ lại (Schöningen).',
  'region': 'eurasia_temperate',
  'mvp': False},
 {'sec': '4.12',
  'id': 'leptoptilos_robustus',
  'codex': 'leptoptilos_robustus',
  'vi': 'Cò già khổng lồ',
  'sci': 'Leptoptilos robustus',
  'frame': 'D',
  'stage': 4,
  'must': 'Cao ~1,8 m; **bay được**. Khung D, nhưng hình gốc là dáng **đứng** cánh khép, chân dài 2 đoạn.',
  'subject': 'A giant marabou stork about 1.8 meters tall, bald pinkish head, huge heavy bill, long legs, dark wings and pale body',
  'parts_extra': 'each long leg as an upper and a lower piece with foot, wings as folded pieces that can open',
  'anims': 'idle, walk, peck, flap, stagger',
  'instinct': 'wings half spread, huge bill open, menacing glare',
  'instinct_vi': 'Kẻ tranh xác: xòe cánh, há mỏ.',
  'codex_scene': 'A giant marabou stork with wings flared wide and huge bill open, standing guard on a rock ledge in the mouth of a great '
                 'limestone cave on Flores, dark against the bright monsoon forest outside',
  'why': 'Nhìn từ trong hang ra, ngược sáng (Liang Bua).',
  'region': 'flores',
  'mvp': False},
 {'sec': '4.13',
  'id': 'komodo',
  'codex': 'komodo',
  'vi': 'Rồng Komodo',
  'sci': 'Varanus komodoensis',
  'frame': 'C',
  'wide': True,
  'stage': 4,
  'must': '',
  'subject': 'A Komodo dragon, heavy body, long thick tail, forked tongue, grey-brown scales',
  'anims': 'idle, crawl, attack, stagger, flee',
  'instinct': 'forked tongue out, head low, powerful and patient',
  'instinct_vi': 'Kiên nhẫn và chết người: lưỡi chẻ thè ra.',
  'codex_scene': 'A Komodo dragon crawling straight toward the viewer at ground level through dry gold island grassland with scattered '
                 'lontar palms, forked tongue flicking, heavy claws gripping dry earth',
  'why': 'Góc sát đất, bò thẳng về phía người xem.',
  'region': 'flores',
  'mvp': False},
 {'sec': '4.14',
  'id': 'stegodon_dwarf',
  'codex': 'stegodon_dwarf',
  'vi': 'Voi lùn Stegodon',
  'sci': 'Stegodon florensis insularis',
  'frame': 'E',
  'stage': 4,
  'must': 'Hai ngà sát nhau, gần chạm.',
  'subject': 'A dwarf Stegodon, a small elephant relative, low broad skull, long tusks set close together and nearly touching, short trunk',
  'parts_extra': 'both tusks as one piece because they nearly touch',
  'anims': 'idle, walk, stagger, panic',
  'instinct': 'ears flared, trunk raised, protective',
  'instinct_vi': 'Mẹ bảo vệ con: tai xòe, vòi giơ.',
  'codex_scene': 'A dwarf Stegodon mother trumpeting with ears flared, standing in front of her calf while wading through a clear shallow '
                 'river in an island monsoon forest',
  'why': 'Lội suối, mẹ che con.',
  'region': 'flores',
  'mvp': False},
 {'sec': '4.15',
  'id': 'cave_lion',
  'codex': 'cave_lion',
  'vi': 'Sư tử hang',
  'sci': 'Panthera spelaea',
  'frame': 'A',
  'stage': 5,
  'must': 'Không có bờm lớn.',
  'subject': 'An Ice Age cave lion, large and powerful, with no mane or only a very small one, plain pale coat',
  'anims': 'idle, walk, run, attack, stagger, enrage',
  'instinct': 'roaring, maneless and powerful',
  'instinct_vi': 'Vua băng giá: gầm vang.',
  'codex_scene': 'A cave lion roaring on the bank of a frozen river at sunrise, its mate behind it, breath steaming in the cold',
  'why': 'Sông băng lúc bình minh, một cặp.',
  'region': 'ice_age_eurasia',
  'mvp': False},
 {'sec': '4.16',
  'id': 'cave_hyena',
  'codex': 'cave_hyena',
  'vi': 'Linh cẩu hang',
  'sci': 'Crocuta crocuta spelaea',
  'frame': 'A',
  'stage': 5,
  'must': 'To hơn linh cẩu đốm.',
  'subject': 'A cave hyena, larger and heavier than a spotted hyena, sloping back, big head, thick spotted coat',
  'anims': 'idle, walk, run, attack, stagger, flee',
  'instinct': 'hunched, jaws open, hackles raised',
  'instinct_vi': 'Giữ hang: chồm tới, há hàm.',
  'codex_scene': 'Two cave hyenas defending the entrance of a rock shelter, one stepping forward with jaws open and hackles raised, seen '
                 'from inside the shelter looking out at a snowy valley',
  'why': 'Từ trong mái đá nhìn ra (chuyện tranh hang).',
  'region': 'ice_age_eurasia',
  'mvp': False},
 {'sec': '4.17',
  'id': 'mammoth',
  'codex': 'mammoth',
  'vi': 'Voi ma mút lông xoăn',
  'sci': 'Mammuthus primigenius',
  'frame': 'E',
  'stage': 5,
  'must': 'Tai nhỏ, đầu gồ cao, lưng dốc.',
  'subject': 'A woolly mammoth, high domed head, strongly sloping back, small ears, long shaggy reddish-brown fur, long curving tusks',
  'anims': 'idle, walk, charge, stagger',
  'instinct': 'trunk raised, long curved tusks forward, snow flying off its fur',
  'instinct_vi': 'Xé bão tuyết: vòi giơ, ngà chĩa tới.',
  'codex_scene': 'A woolly mammoth bull charging through a blizzard on the snowy steppe with its trunk raised and long curved tusks '
                 'forward, snow flying off its shaggy fur, the herd fading into the distance',
  'why': 'Bão tuyết, con đực đầu đàn lao tới.',
  'region': 'ice_age_eurasia',
  'mvp': False},
 {'sec': '4.18',
  'id': 'megalania',
  'codex': 'megalania',
  'vi': 'Thằn lằn khổng lồ',
  'sci': 'Varanus priscus',
  'frame': 'C',
  'wide': True,
  'stage': 5,
  'must': 'Dài ~6 m.',
  'subject': 'A gigantic monitor lizard about six meters long, heavy body, thick tail, scaly mottled brown skin',
  'anims': 'idle, crawl, attack, stagger, flee',
  'instinct': 'jaws open, forked tongue out, massive body',
  'instinct_vi': 'Rồng của nước Úc: hàm há, lưỡi chẻ.',
  'codex_scene': 'A gigantic monitor lizard rearing its head up with jaws open and forked tongue out on top of a red rocky outcrop in the '
                 'Australian bush at noon',
  'why': 'Mỏm đá đỏ giữa bụi rậm Úc, ngẩng đầu.',
  'region': 'australia',
  'mvp': False}]

# ---------------------------------------------------------------------------
# Giải phẫu: (tên ngắn tiếng Anh, đặc điểm phải đúng, câu "không phải loài hiện đại")
# ---------------------------------------------------------------------------
ANATOMY = {
 "leopard": ("leopard", "compact powerful body, relatively short legs, tail about as long as the body, broad head with small rounded ears, golden coat covered in rosettes that are rings with open centers, pale belly", "realistic wild leopard proportions, not a jaguar, not a cheetah, no lion features"),
 "python": ("python", "very thick heavy body, triangular head with a dark arrow-shaped mark on the crown and a dark stripe through the eye, irregular olive and chestnut blotches edged in black along the back, pale cream belly, round pupils", "not a cobra, no hood, no venom fangs"),
 "hyena_pack": ("hunting hyena", "Chasmaporthetes: long slender legs of nearly equal length so the back is almost level, light build like a cheetah, smaller narrow head with cat-like slicing teeth, short sandy coat with sparse small dark spots, small rounded ears, thin tail", "NOT a modern spotted hyena: no strongly sloping back, no heavy bone-crushing head, no shaggy mane"),
 "sabertooth": ("sabertooth cat", "Megantereon: jaguar-sized, very robust heavily muscled forelimbs and thick neck, short face, long flat blade-like upper canines reaching below the chin and resting against a bony flange on the front of the lower jaw, short tail, plain tawny coat with faint pale markings", "NOT Smilodon (smaller and lighter), NOT a modern tiger or lion: no stripes, no mane"),
 "small_antelope": ("small antelope", "small duiker-like antelope with an arched back, hindquarters higher than the shoulders, short straight spike horns, reddish-brown coat, large dark eyes, thin legs", "not an impala, no lyre-shaped horns"),
 "boar_piglet": ("wild piglet", "young wild pig, stocky round body, light brown coat with pale horizontal stripes along the body, short snout, tiny tusks just showing", "not a domestic pink pig"),
 "hare": ("hare", "wild hare with very long black-tipped ears, long powerful hind legs, brown-grey coat, large side eyes", "not a domestic rabbit"),
 "eagle": ("eagle", "large African forest eagle like a crowned eagle: short broad rounded wings, long barred tail, double crest on the head, dark grey-brown back, pale chest with heavy dark bars, massive yellow legs and huge talons", "not a bald eagle, no white head"),
 "machairodus": ("sabertooth cat", "Machairodus kabir: bigger than the largest tiger, long legs, long flat serrated saber canines, relatively short tail, plain tawny coat", "NOT a modern tiger: no stripes; NOT Smilodon"),
 "dinofelis": ("false sabertooth cat", "Dinofelis: jaguar-like build with robust forelimbs, short flattened canines only slightly longer than a leopard's, faint spots on a tawny coat", "NOT a leopard, NOT a true sabertooth with long sabers"),
 "deinotherium": ("Deinotherium", "elephant relative, much taller and longer-legged than an elephant, small head for the body, short trunk, NO upper tusks, one pair of tusks growing from the LOWER jaw and curving down and backward like hooks", "NOT a modern elephant"),
 "crocodile_horned": ("horned crocodile", "Crocodylus anthropophagus: heavy crocodile body with prominent horn-like bony bosses rising at the back of the skull above the eyes, deep strong snout, armored back", "NOT a modern Nile crocodile without horns"),
 "pelorovis": ("giant buffalo", "Pelorovis: long-legged wild cattle with enormous horns spreading sideways from the head and curving forward and up in a wide half circle, dark brown coat", "NOT a modern cape buffalo, NOT a water buffalo"),
 "hippo_gorgops": ("giant hippo", "Hippopotamus gorgops: larger than any modern hippo, eye sockets raised high on top of the skull like periscopes, nostrils high on the snout, massive barrel body, short legs", "NOT a modern hippo"),
 "palaeoloxodon_recki": ("straight-tusked elephant", "Palaeoloxodon recki: very tall elephant with a high domed head and a pronounced forehead ridge, long nearly straight tusks, long legs", "NOT a modern African elephant"),
 "theropithecus_oswaldi": ("giant baboon", "Theropithecus oswaldi: gelada-like giant monkey much bigger than any modern baboon, long heavy mane over the shoulders, bare hourglass-shaped chest patch, deep jaw, long face with upturned nostrils", "NOT a modern baboon or mandrill, no colorful face"),
 "pachycrocuta": ("giant hyena", "Pachycrocuta: hyena the size of a lion or bigger, short broad muzzle, massive bone-crushing jaws, short sturdy legs, strongly sloping back, coarse spotted coat", "NOT a modern spotted hyena: much heavier, shorter face"),
 "gigantopithecus": ("giant ape", "Gigantopithecus: only teeth and jaws are known, so this is a reconstruction: a giant orangutan-like ape about three meters tall, very heavy body, long arms, thick reddish-brown fur, large heavy-browed face", "NOT King Kong, NOT a gorilla, no human-like upright posture"),
 "homotherium": ("scimitar-toothed cat", "Homotherium: long front legs and shorter hind legs giving a sloping back like a hyena, short broad serrated scimitar canines that only just show below the lips, short bobbed tail, lean body, pale coat", "NOT Smilodon, NOT a lion, no mane"),
 "leptoptilos_robustus": ("giant stork", "Leptoptilos robustus: marabou-like stork about 1.8 m tall, bald pinkish head and neck, huge heavy wedge-shaped bill, long thick legs, dark wings and pale body", "NOT a modern marabou: much taller and heavier"),
 "komodo": ("Komodo dragon", "Komodo dragon: heavy low body, thick muscular tail, long forked yellow tongue, grey-brown beaded scales, strong curved claws", ""),
 "stegodon_dwarf": ("dwarf Stegodon", "dwarf Stegodon florensis insularis: small elephant relative, low flat skull, long tusks set very close together and nearly touching so the trunk hangs beside them, short trunk", "NOT a modern elephant"),
 "cave_lion": ("cave lion", "Panthera spelaea: bigger than a modern lion, no mane or only a very short ruff, thick pale winter coat, faint stripes on the flanks", "NOT a modern maned African lion"),
 "cave_hyena": ("cave hyena", "cave hyena: spotted hyena form but much larger and heavier, thick winter coat with dark spots, big head, sloping back", "NOT the size of a modern spotted hyena"),
 "mammoth": ("woolly mammoth", "Mammuthus primigenius: high domed head, small ears, strongly sloping back with a high shoulder hump, long shaggy brown hair over dense underfur, very long tusks curving out, up and inward in a spiral, short tail", "NOT a modern elephant"),
 "megalania": ("giant monitor lizard", "Varanus priscus (Megalania): monitor lizard about six meters long, far bulkier than a Komodo dragon, heavy limbs, deep powerful jaws, mottled brown beaded scales", "NOT a Komodo dragon: far larger and heavier"),
}

# ---------------------------------------------------------------------------
# Vùng: (tên, trong game, màu chủ đạo, bóng cây/địa hình, chặn)
# ---------------------------------------------------------------------------
REGIONS = {
 "rainforest": ("Rừng nhiệt đới", "vùng 0 (MVP)", "deep emerald green, from dark emerald in the foreground to pale jade mist at the back",
   "giant trunks with buttress roots, hanging lianas, broad leaves and ferns, layered canopy, shafts of light",
   "not savanna, no open grassland, no golden dry grass, no acacia trees, no snow, no desert"),
 "savanna": ("Savan", "vùng 1 (MVP)", "warm golden amber, from dark amber in the foreground to pale glowing gold at the horizon",
   "flat-topped acacia trees, tall dry grass, rocky kopjes, low rolling hills",
   "no rainforest, no dense jungle, no snow, no pine forest"),
 "lakeshore": ("Savan ven hồ", "bản đầy đủ [ĐỀ XUẤT]", "muted teal and olive, from dark olive in the foreground to pale silver-teal water and sky",
   "reed beds, still lake water with reflections, gallery trees along the shore, volcanic hills",
   "no rainforest, no snow, no desert dunes"),
 "eurasia_temperate": ("Âu-Á ôn đới", "bản đầy đủ [ĐỀ XUẤT]", "cool slate blue with rust-orange accents, from dark slate in the foreground to pale grey-blue sky",
   "birch and pine trunks, limestone hills with cave mouths, cold lakes with reeds",
   "no savanna, no acacia trees, no tropical plants, no palms"),
 "southeast_asia": ("Đông Nam Á núi đá vôi", "bản đầy đủ [ĐỀ XUẤT]", "misty jade green, from dark jade in the foreground to pale white-green mist",
   "tall steep karst limestone towers, bamboo clumps, bands of drifting cloud, a winding river",
   "no savanna, no acacia trees, no snow, no desert"),
 "flores": ("Đảo Flores", "bản đầy đủ [ĐỀ XUẤT]", "warm sand gold with turquoise sea accents, from dark ochre in the foreground to pale turquoise sky",
   "lontar palms, a large limestone cave mouth, dry island hills, the sea on the horizon",
   "no African savanna animals, no snow, no pine forest"),
 "ice_age_eurasia": ("Âu-Á băng hà", "bản đầy đủ [ĐỀ XUẤT]", "icy blue lit by pale pink-gold low sunlight, from dark steel blue in the foreground to almost white sky",
   "snowy steppe, frozen river, limestone cliffs and rock shelters, sparse wind-bent trees, falling snow",
   "no savanna, no acacia trees, no tropical plants, no green jungle"),
 "australia": ("Úc", "bản đầy đủ [ĐỀ XUẤT]", "red ochre, from dark rust in the foreground to pale hot orange sky",
   "red rock outcrops, grey-green eucalyptus trees, spinifex grass clumps",
   "no African savanna animals, no snow, no jungle"),
 "desert": ("Sa mạc", "bản đầy đủ [ĐỀ XUẤT] — chưa có loài", "terracotta and sand, from dark terracotta in the foreground to pale hot sky",
   "rock plateaus, wind-carved stones, sand dunes, rare thorn bushes", "no rainforest, no snow, no lush grass"),
 "nile": ("Đồng bằng sông Nile", "bản đầy đủ [ĐỀ XUẤT] — chưa có loài", "papyrus green with river blue, from dark green in the foreground to pale warm sky",
   "papyrus reeds, palm groves, a wide slow river, desert edge far away", "no snow, no pine forest, no jungle"),
}


def rig_prompt(s):
    name, ana, notm = ANATOMY[s["id"]]
    return j(ONE, s["subject"], "anatomy: " + ana, notm, RIG[s["frame"]], STYLE_REF, STYLE, TECH_RIG, AVOID)


def extra_prompt(s, pr):
    return j(ONE, pr, "same animal, same colors and same line style as the attached rig picture", BG_REMOVE, STYLE_REF, STYLE, AVOID)


def codex_prompt(s):
    name, ana, notm = ANATOMY[s["id"]]
    rv, rg, hue, sil, guard = REGIONS[s["region"]]
    return j(ONE, s["codex_scene"], "attitude: " + s["instinct"], "anatomy: " + ana, notm, STYLE_REF, STYLE, MOOD_CODEX,
             CODEX_BG, "dominant hue: " + hue, "region silhouettes: " + sil, COMPOSITION, AVOID, AVOID_CODEX, guard)


# ---------------------------------------------------------------------------
# v7 (09/11): lần thử Gemini thứ ba vẽ hình gốc thành một cảnh savan (con báo
# trên cành), vì (a) ảnh mẫu đính kèm cũng là con báo trên cành nên Gemini chép
# luôn nội dung ảnh mẫu, (b) prompt quá dài nên các ràng buộc cuối bị bỏ qua.
# Sửa: prompt NGẮN, ràng buộc quan trọng nhất đặt ĐẦU; hình gốc KHÔNG đính kèm
# ảnh mẫu phong cách mà đính kèm hồ sơ đầy đủ của chính loài đó; thứ tự sản xuất
# 4 → 1 → 2 để cả bộ cùng một thiết kế.
# ---------------------------------------------------------------------------
RIG_SHORT = {
    "A": "standing still, all four legs visible and spread apart so they do not overlap, head level, tail held straight back",
    "B": "body stretched out almost straight in a gentle wave, head slightly raised, no part of the body crossing another",
    "C": "low stance, all four legs visible and not overlapping, tail lying straight back",
    "D": "wings spread wide in a glide, near wing in front of the body, far wing behind, talons tucked under the body",
    "E": "standing still, all four legs visible and staggered, trunk hanging down, tusks fully visible",
    "F": "standing on all fours, long arms and legs visible and staggered, head level",
}
RIG_SHORT_OVERRIDE = {"leptoptilos_robustus": "standing still on two long legs slightly apart, wings folded, bill pointing forward"}
STYLE_SHORT = "Flat vector game art: clean flat colors, hard-edged cel shading in 3 to 4 tones, thin dark brown outlines, bright highlight along the top edges, no texture, no gradients"
NO_SCENE = "Background: solid flat magenta #FF00FF only. No ground, no grass, no tree, no branch, no scenery, no shadow, no white outline, no text"


def rig_prompt(s):
    name, ana, notm = ANATOMY[s["id"]]
    pose = RIG_SHORT_OVERRIDE.get(s["id"], RIG_SHORT[s["frame"]])
    return j(f"Game sprite: one {name} alone, full body, seen exactly from the side (profile view), facing right",
             f"Neutral pose: {pose}, mouth closed",
             "Keep the same animal design and colors as the attached picture, but change the pose and remove the whole background" ,
             f"Anatomy: {ana}", notm, STYLE_SHORT, NO_SCENE)


def parts_prompt(s):
    name = ANATOMY[s["id"]][0]
    lst = parts_for(s)
    items = ", ".join(f"({i}) {n.split(',')[0]}" for i, (n, _) in enumerate(lst, 1))
    return j(f"Cut-out parts sheet for 2D skeletal animation of the {name} in the attached picture",
             f"Draw exactly {len(lst)} separate pieces of this same {name}, same colors, same outlines, same scale: {items}",
             "Cut only at the joints (jaw, neck, shoulder, elbow, wrist, hip, knee, ankle, tail joints); the torso is ONE piece; extend and round each joint end a little so it overlaps when rotated",
             "Spread the pieces apart in rows in the numbered order, side view, with wide gaps",
             "No pins, no rivets, no joint circles or dots, no outlines or white borders around pieces, no labels, no numbers, no text",
             "Background: solid flat magenta #FF00FF only")


def extra_prompt(s, pr):
    return j(f"Using the {ANATOMY[s['id']][0]} in the attached picture: {pr}", "same colors and outlines", STYLE_SHORT, NO_SCENE)


def codex_prompt(s):
    name, ana, notm = ANATOMY[s["id"]]
    rv, rg, hue, sil, guard = REGIONS[s["region"]]
    return j(f"Wide 16:9 illustration. {s['codex_scene']}",
             f"Attitude: {s['instinct']}",
             f"Composition: the {name} is large and fully inside the RIGHT half of the image; the LEFT half is calm open sky and distant layers, empty for text",
             f"Anatomy: {ana}", notm,
             "Style: flat vector poster art like the attached image (copy only the drawing style, not its animal, colors or place): clean flat shapes, hard-edged cel shading, thin dark outlines, background of 4 to 5 flat layered silhouettes",
             f"Background colors: only shades of {hue}; the {name} keeps its own natural colors and stands out bright against it",
             f"Place: {sil}", guard.capitalize(),
             "No text, no border, no frame, no blood, not cute, not a calm pose")


def species_block(s):
    out = []
    sci = f" (*{s['sci']}*)" if s['sci'] else ""
    out += [f"### {s['sec']} `{s['id']}` — {s['vi']}{sci} — khung {s['frame']} ({FRAME_NAME[s['frame']]})", ""]
    rv, rg, *_ = REGIONS[s["region"]]
    out += [f"**Vùng:** {rv} (`{s['region']}`, {rg}). Nền hồ sơ theo vùng này (§2.8).", ""]
    out += [f"**Bản năng / dáng (chỉ cho hồ sơ):** {s['instinct_vi']}", ""]
    if not s["mvp"]:
        out.append(f"Giai đoạn {s['stage']}.")
        if s.get("must"):
            out.append(f"Phải đúng: {s['must']}")
        out.append("")
    for n in s.get("notes", []):
        out.append(f"- {n}")
    if s.get("notes"):
        out.append("")
    setting = '`aspect_ratio: "16:9"`' if s.get("wide") else '`aspect_ratio: "1:1"`'
    plist = parts_for(s)
    out += ["**Thứ tự làm:** hình 4 → hình 1 → hình 2 (hình 3 game tự cắt). Mỗi bước dùng hình vừa duyệt của bước trước, nên cả bộ cùng một thiết kế.", ""]
    out += [f"**1. Hình gốc** → `beast_{s['id']}_rigpose.png` · {setting} · làm **thứ hai** · đính kèm: **hồ sơ đầy đủ (hình 4) vừa duyệt của chính loài này**, KHÔNG đính kèm ảnh mẫu phong cách", "```", rig_prompt(s), "```", ""]
    out += [f"**2. Phân mảnh từ hình gốc** → `beast_{s['id']}_parts.png` · làm **thứ ba** · đính kèm: **chỉ hình 1** · **đúng {len(plist)} mảnh**", "```", parts_prompt(s), "```", ""]
    out += ["Kiểm mảnh (đếm trước khi nhận): " + "; ".join(f"{i}. {n}" for i, (n, _) in enumerate(plist, 1)) + ".", ""]
    for k, (fname, label, pr) in zip("bcd", s.get("extra", [])):
        out += [f"**2{k}. {label}** → `{fname}` · đính kèm: hình 1", "```", extra_prompt(s, pr), "```", ""]
    out += ["**3. Hình hồ sơ** → **không cần prompt.** Game cắt từ hình 4 (§2.7).", ""]
    out += [f"**4. Hồ sơ đầy đủ** → `codex_{s['codex']}.png` · `aspect_ratio: \"16:9\"` · làm **đầu tiên** · đính kèm: ảnh mẫu phong cách",
            f"Điểm riêng của tranh này: {s['why']}", "```", codex_prompt(s), "```", ""]
    if s["mvp"]:
        a = "; ".join(f"`{n}`" + (f" ({d})" if d else "") for n, d in s["anims"])
        out.append(f"**Hoạt ảnh làm bằng xương trong Unity** (không sinh ảnh): {a}.")
    else:
        out.append(f"**Hoạt ảnh dự kiến** (bằng xương, chốt khi loài vào combat): {s['anims']}.")
    out.append("")
    return out

for _s in SPECIES:
    _s["short"] = ANATOMY[_s["id"]][0]

FRAME_DOC = {
 "A": ("Dùng cho: mèo lớn, linh cẩu, con mồi nhỏ, trâu, hà mã, khỉ đầu chó (đi bốn chân).",
       "`leg_hind_far_upper, leg_hind_far_lower, foot_hind_far, leg_front_far_upper, leg_front_far_lower, foot_front_far, ear_far, tail_3, tail_2, tail_1, torso, neck, jaw, head, ear_near, leg_hind_near_upper, leg_hind_near_lower, foot_hind_near, leg_front_near_upper, leg_front_near_lower, foot_front_near`",
       ["Gốc `hips`. Từ đó: `spine` → `chest` → `neck` → `head` → `jaw`.", "`tail_1` → `tail_2` → `tail_3`, nối từ `hips`.",
        "Mỗi chân 3 xương (`upper`, `lower`, `foot`). Chân trước nối từ `chest`, chân sau nối từ `hips`.",
        "IK: Limb cho mỗi chân; CCD cho đuôi.", "Tai (`ear`) chỉ có ở loài cần cụp tai: báo, linh cẩu, thỏ."]),
 "B": ("Dùng cho: trăn.", "`body, jaw, head`",
       ["Chuỗi 12–16 xương dọc thân (`body_01` … `body_16`), rồi `head`, `jaw`.", "Gắn xương thẳng lên **một mảnh thân liền**.",
        "IK: FABRIK cho cả chuỗi.",
        "Ngoại lệ: động tác Siết dùng thêm **một mảnh vòng quấn** (§3.2), đổi bằng Sprite Library, vì một lưới hình không vẽ được thân tự đè lên chính nó."]),
 "C": ("Dùng cho: cá sấu, rồng Komodo, Megalania.", "Như khung A, nhưng đuôi 4 đoạn (`tail_1` … `tail_4`), chân 2 đoạn (`upper`, `lower`), không có tai.",
       ["Như khung A, đuôi 4 xương, mỗi chân 2 xương."]),
 "D": ("Dùng cho: đại bàng, cò khổng lồ (cò đứng: chân dài 2 đoạn).", "`wing_far, leg_far, tail, body, head, leg_near, wing_near_inner, wing_near_outer`",
       ["Gốc `body` → `neck` → `head`.", "`wing_near_inner` → `wing_near_outer`.", "`wing_far`, `tail`, `leg_near`, `leg_far`.", "IK: không cần."]),
 "E": ("Dùng cho: *Deinotherium*, voi *Palaeoloxodon*, *Stegodon*, ma mút.", "Như khung A, thêm `ear`, `tusks`, `trunk_1`, `trunk_2`, `trunk_3` nằm trên `head`. Không có `jaw` (trừ *Deinotherium*: `jaw` mang ngà).",
       ["Như khung A, thêm chuỗi `trunk_1` → `trunk_3` (IK: CCD) và `ear`."]),
 "F": ("Dùng cho: *Gigantopithecus*.", "Như khung A, không có đuôi; chân trước thành `arm_upper`, `forearm`, `hand`.", ["Như khung A, không có đuôi."]),
}


def sec_1_1():
    rules = ("You are generating game assets for me. Rules for this whole conversation, no exceptions: "
             "1) Each of my messages asks for exactly ONE image. Never combine several assets in one image, never make panels, "
             "sheets with titles, collages, before/after or comparison layouts. "
             "2) Never draw any text, letters, titles, labels, numbers, logos or UI frames. "
             "3) Never add a white outline, sticker border, glow or drop shadow around the subject. "
             "4) When I ask for a plain magenta background, fill the whole background with flat #FF00FF and nothing else; never draw a checkerboard. "
             "5) Follow the anatomy I describe exactly; do not replace an extinct animal with the modern animal it resembles. "
             "6) For a parts sheet, draw exactly the number and list of pieces I give, nothing more; no pins, rivets, bolts or joint markers. "
             "7) If you cannot follow a rule, tell me instead of drawing.")
    L = ["### 1.1 Quản Gemini (bắt buộc khi dùng Gemini hoặc chatbot tạo ảnh)", "",
         "Gemini đã mắc các lỗi sau trong lần thử ngày 09/11: gộp nhiều hình vào một ảnh có tiêu đề, thêm chữ, viền trắng quanh con vật, vẽ ô carô giả làm nền trong suốt, cắt mảnh tùy ý (thân cắt 5 khúc, có đinh chốt), và đổi loài tuyệt chủng thành loài hiện đại. Quy trình dưới đây để chặn các lỗi đó.", "",
         "**Bước 1. Mở một cuộc trò chuyện mới cho mỗi loài**, dán khung luật này làm tin nhắn đầu tiên (chưa xin hình):", "```", rules, "```", "",
         "**Bước 2. Làm theo thứ tự 4 → 1 → 2, mỗi tin nhắn đính kèm đúng ảnh ghi ở prompt và dán đúng MỘT prompt:**",
         "1. **Hồ sơ đầy đủ (hình 4)**: đính kèm ảnh mẫu phong cách `docs/art-reference/style_codex_leopard.png`. Riêng **báo hoa mai** thì đính kèm `sample_sabertooth.png` thay vào, vì ảnh mẫu cũng là con báo trên cành, Gemini sẽ chép nguyên cảnh.",
         "2. **Hình gốc (hình 1)**: đính kèm **hình 4 vừa duyệt**, KHÔNG đính kèm ảnh mẫu phong cách. Gemini giữ thiết kế con vật từ hình 4, chỉ đổi dáng và bỏ nền.",
         "3. **Bộ mảnh (hình 2)**: đính kèm **chỉ hình 1**.", "",
         "Lý do (lần thử thứ ba, 09/11): khi đính kèm tranh báo trên cành để xin hình gốc con báo, Gemini vẽ lại đúng cảnh savan trên cành, bỏ qua yêu cầu nhìn ngang và nền hồng sen. Ảnh đính kèm luôn mạnh hơn chữ, nên chỉ đính kèm ảnh có **đúng thứ mình muốn giữ**.", "",
         "**Prompt ngắn có chủ đích:** các ràng buộc quan trọng nhất (một con, nhìn ngang, nền hồng sen, không cảnh vật) nằm ở **đầu** prompt. Đừng thêm mô tả phong cách dài vào hình gốc: prompt càng dài, Gemini càng bỏ sót đoạn cuối.", "",
         "**Bước 3. Kiểm trước khi nhận.** Sai bất kỳ dòng nào là **loại**, không sửa tay:", "",
         "| Kiểm | Hình gốc | Bộ mảnh | Hồ sơ đầy đủ |", "| --- | --- | --- | --- |",
         "| Chỉ một hình, không ô, không tiêu đề | ✔ | ✔ | ✔ |",
         "| Không chữ, số, nhãn nào | ✔ | ✔ | ✔ |",
         "| Không viền trắng, không bóng đổ quanh con vật | ✔ | ✔ | ✔ |",
         "| Nền hồng sen phẳng (#FF00FF), không ô carô | ✔ | ✔ | — |",
         "| Nhìn ngang, dáng trung tính, miệng ngậm | ✔ | — | — |",
         "| Đúng số mảnh và đúng tên mảnh theo danh sách \"Kiểm mảnh\"; thân là MỘT mảnh; không đinh, chốt | — | ✔ | — |",
         "| Đúng giải phẫu, không giống loài hiện đại (đọc dòng `NOT ...` trong prompt) | ✔ | ✔ | ✔ |",
         "| Nền đúng vùng và đúng màu chủ đạo của vùng (§2.8), con vật trong nửa phải, nửa trái thoáng | — | — | ✔ |", "",
         "**Bước 4. Khi loại, nhắn đúng câu này** (thay `<lỗi>` bằng dòng sai), không giải thích thêm:", "```",
         "This image breaks the rule: <lỗi>. Generate it again from my previous prompt, following every rule. One image only.", "```", "",
         "**Bước 5. Sai 2 lần liên tiếp thì đổi cách:**",
         "- Hình gốc và hồ sơ: chuyển sang SpriteCook (`generate_game_art`, `style_asset_ids`).",
         "- Bộ mảnh: **cắt tay từ hình gốc** bằng Krita, GIMP hoặc Photoshop theo danh sách \"Kiểm mảnh\" (đường cắt đi qua khớp), rồi vẽ bù phần bị che ở đầu khớp. Đây là cách chắc chắn nhất; bộ mảnh do AI vẽ chỉ là đường tắt.", "",
         "**Xóa nền hồng sen:** SpriteCook `remove_background`, hoặc Magic Wand chọn màu #FF00FF trong Krita, GIMP, Photoshop. Gemini không xuất được nền trong suốt thật nên mọi prompt dùng nền hồng sen.", ""]
    return "\n".join(L)


def sec_2_4_to_2_6():
    L = ["### 2.4 Khối prompt dùng chung", "", "Mọi prompt ở §3–§4 **đã ghép sẵn**. Sửa thì sửa trong `tools/art/gen_prompts.py` rồi chạy lại.", "",
         "**v7 (đang dùng): prompt ngắn, ràng buộc đặt đầu.**", "```",
         "Hồ sơ đầy đủ (làm đầu tiên): Wide 16:9 + <cảnh> + Attitude + Composition (con vật nửa PHẢI) + Anatomy + NOT-modern + Style (chỉ chép cách vẽ của ảnh mẫu) + Colors (màu chủ đạo vùng) + Place (bóng vùng) + <chặn vùng> + No text…",
         "Hình gốc (thứ hai)        : Game sprite, một con, nhìn ngang hướng phải + Neutral pose + giữ thiết kế của hình 4 đính kèm, bỏ nền + Anatomy + NOT-modern + STYLE_SHORT + NO_SCENE",
         "Bộ mảnh (thứ ba)          : Parts sheet của con vật trong hình 1 đính kèm + đúng N mảnh có tên + chỉ cắt ở khớp, thân một mảnh + xếp theo số + cấm chốt/viền/chữ + nền hồng sen",
         "Hình hồ sơ                : không prompt, game cắt từ hồ sơ đầy đủ (§2.7)", "```", "",
         "**STYLE_SHORT**", "```", STYLE_SHORT, "```", "", "**NO_SCENE**", "```", NO_SCENE, "```", "",
         "Các khối dài bên dưới là bản v6, giữ lại để tham khảo cách tả phong cách; prompt v7 không ghép chúng nữa.", ""]
    for name, val, note in [("ONE", ONE, "mở đầu mọi prompt"), ("STYLE_REF", STYLE_REF, "đi kèm ảnh mẫu phong cách"),
                            ("STYLE", STYLE, "cách vẽ con vật, giống tranh mẫu báo nền vàng"),
                            ("BG_REMOVE", BG_REMOVE, "nền để xóa; không dùng 'transparent' vì Gemini vẽ ô carô giả"),
                            ("TECH_RIG", TECH_RIG, "hình gốc"), ("MOOD_CODEX", MOOD_CODEX, "chỉ cho hồ sơ"),
                            ("CODEX_BG", CODEX_BG, "nền hồ sơ: các lớp bóng phẳng trong tông của một màu chủ đạo, màu lấy theo vùng"),
                            ("COMPOSITION", COMPOSITION, "bố cục hồ sơ 16:9"), ("AVOID", AVOID, "mọi hình"), ("AVOID_CODEX", AVOID_CODEX, "chỉ cho hồ sơ")]:
        L += [f"**{name}** ({note})", "```", val, "```", ""]
    L += ["**Tranh mẫu phong cách hồ sơ:** `docs/art-reference/style_codex_leopard.png`. Đặc điểm cần giữ:",
          "- Con vật: khối phẳng sắc nét, đổ bóng cắt cạnh 3–4 tông, viền nâu sẫm mảnh, điểm sáng dọc mép trên, đốm vẽ thành mảng phẳng gọn.",
          "- Nền: 4–5 lớp bóng phẳng chồng nhau (cây gần, cây giữa, đồi, núi xa, trời), **tất cả trong các tông của một màu**, đậm ở gần, nhạt dần về chân trời, một vầng sáng lớn trên trời.",
          "- Cành cây hoặc mỏm đá đậm màu ở tiền cảnh làm chỗ đứng cho con vật.",
          "- Khác tranh mẫu ở hai điểm: **màu chủ đạo theo vùng** (rừng xanh ngọc, băng hà xanh băng…), và **khổ 16:9, con vật ở nửa phải** để đặt chữ bên trái.", "",
          "**Thiết lập SpriteCook (nếu dùng):** `pixel: false`; hình gốc và bộ mảnh `resolution: \"2K\"`, `smart_crop: false` cho bộ mảnh, rồi `remove_background`; hồ sơ `aspect_ratio: \"16:9\"`, `bg_mode: \"include\"`.", "",
          "### 2.5 Ví dụ đã ghép đủ (báo hoa mai, hình gốc)", "", "```", rig_prompt(SPECIES[0]), "```", "",
          "### 2.6 Khung xương mẫu và danh sách mảnh", "",
          "Mỗi khung có: dáng để rig, **danh sách mảnh cố định** (mỗi mảnh là một đoạn nằm giữa hai khớp), tên lớp trong file `.psb`, và xương.",
          "Chỉ cắt ở **khớp thật**: hàm, cổ, vai, khuỷu, cổ tay, hông, gối, cổ chân, gốc đuôi và các đốt đuôi, gốc cánh, cổ tay cánh, các đốt vòi. Không cắt giữa một khối cơ (ví dụ cắt ngang thân).",
          "Mỗi mảnh kéo dài và bo tròn thêm khoảng 15% ở đầu khớp, để khi xoay không hở. Chi phía xa tối hơn một bậc màu.", ""]
    for f in "ABCDEF":
        use, layers, bones = FRAME_DOC[f]
        L += [f"#### Khung {f} — {FRAME_NAME[f]}", "", use, "", "Dáng để rig:", "```", RIG[f], "```", "",
              f"Mảnh ({len(PARTS[f])} mảnh mặc định; loài có thể thêm hoặc bớt, xem dòng \"Kiểm mảnh\" của từng loài):", ""]
        L += [f"{i}. {n} — {jn}" for i, (n, jn) in enumerate(PARTS[f], 1)]
        L += ["", f"Tên lớp (sau → trước): {layers}", "", "Xương:"] + [f"- {b}" for b in bones] + [""]
    return "\n".join(L)


def sec_2_8():
    L = ["### 2.8 Vùng đất và nền hồ sơ", "",
         "Khoa chốt 09/11: *\"game là ở các vùng khác nhau, mỗi vùng sẽ có các con vật đặc trưng của vùng đó\"*. Nền hồ sơ dùng **màu chủ đạo và bóng cây, địa hình của vùng**, vẽ theo phong cách tranh mẫu (các lớp bóng phẳng trong tông của một màu). Không bao giờ dùng chung nền savan.", "",
         "Vùng 0 và 1 là hai vùng của MVP (`balance.json` `regions`). Các vùng còn lại là đề xuất cho bản đầy đủ (`bestiary.md` §8.3).", "",
         "| Mã vùng | Tên | Trong game | Màu chủ đạo | Loài |", "| --- | --- | --- | --- | --- |"]
    for k, (vi, g, hue, sil, guard) in REGIONS.items():
        sp = ", ".join(s["vi"] for s in SPECIES if s["region"] == k) or "— (chưa có loài, cần nghiên cứu)"
        L.append(f"| `{k}` | {vi} | {g} | {hue.split(',')[0]} | {sp} |")
    L.append("")
    for k, (vi, g, hue, sil, guard) in REGIONS.items():
        L += [f"#### `{k}` — {vi}", "", "Màu chủ đạo:", "```", hue, "```", "Bóng cây, địa hình:", "```", sil, "```", "Chặn:", "```", guard, "```", ""]
    L += ["**Thêm loài:** gán `region` trong dữ liệu loài. **Thêm vùng:** thêm một mục vào `REGIONS`. Chạy lại script.", ""]
    return "\n".join(L)


def sec_3_4():
    L = ["## 3. Sinh vật trong MVP", "",
         "Mỗi loài có **4 hình**: (1) hình gốc dáng trung tính, (2) phân mảnh từ hình gốc, (3) hình hồ sơ — game tự cắt từ hình 4, không cần prompt, (4) hồ sơ đầy đủ — tranh ngang, nền theo vùng (§2.8), con vật nằm nửa phải.",
         "Mỗi prompt đã ghép đủ, dán thẳng được. **Thứ tự làm: hình 4 → hình 1 → hình 2.** **Dùng Gemini thì làm đúng §1.1.**", "",
         "Thứ tự làm: báo hoa mai (mẫu để duyệt cả quy trình) → trăn → linh cẩu → hổ răng kiếm → 3 con mồi → đại bàng (MVP-2).", ""]
    for s in SPECIES:
        if s["mvp"]:
            L += species_block(s)
    L += ["---", "", "## 4. Sinh vật cho bản đầy đủ", "", "Cấu trúc giống §3. Hoạt ảnh chỉ là dự kiến.",
          "Mỗi tranh hồ sơ phải khác các tranh khác ở ít nhất hai điểm: nơi chốn, thời điểm hoặc thời tiết, góc nhìn, số con hoặc hành động (dòng \"Điểm riêng\").", ""]
    for s in SPECIES:
        if not s["mvp"]:
            L += species_block(s)
    L += ["---", "", "_§1.1, §2.4–§2.6, §2.8, §3 và §4 được sinh bằng `tools/art/gen_prompts.py`. Sửa script rồi chạy lại, dán đè các mục đó._", "", ""]
    return "\n".join(L)


if __name__ == "__main__":
    out = sys.argv[1] if len(sys.argv) > 1 else "."
    for fn, f in [("sec_1_1.md", sec_1_1), ("sec_2_4_to_2_6.md", sec_2_4_to_2_6), ("sec_2_8.md", sec_2_8), ("sec_3_4.md", sec_3_4)]:
        with open(os.path.join(out, fn), "w", encoding="utf-8") as fh:
            fh.write(f())
    print("written to", out)
