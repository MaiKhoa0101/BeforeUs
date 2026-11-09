using System;
using System.Collections.Generic;
using BeforeUs.Core.Config;

namespace BeforeUs.Core
{
    /// <summary>
    /// Toàn bộ trạng thái một ván, trừ số liệu cân bằng. Dữ liệu thuần, không chứa luật:
    /// luật nằm ở Climate, Difficulty, TurnSystem, NeuronNetwork… và nhận GameState làm tham số.
    /// C7 tuần tự hóa đúng lớp này cộng trạng thái IRandom.
    /// </summary>
    public sealed class GameState
    {
        public GameState()
        {
            Members = new List<Member>();
            PurchasedNodeIds = new List<string>();
            DiscoveredPlantIds = new List<string>();
            ClearedSiteIds = new List<string>();
        }

        /// <summary>Lượt hiện tại, đếm từ 1 (design.md §2).</summary>
        public int Turn { get; set; }

        /// <summary>Giai đoạn tiến hóa, đếm từ 1 (design.md §7).</summary>
        public int Stage { get; set; }

        /// <summary>Chỉ số vùng, bắt đầu 0 (design.md §4, §12).</summary>
        public int Region { get; set; }

        /// <summary>Neuron chưa tiêu. Dư thì giữ sang giai đoạn sau (design.md §7).</summary>
        public int Neuron { get; set; }

        /// <summary>Tinh thần tích lũy, không âm (design.md §4).</summary>
        public int Morale { get; set; }

        /// <summary>
        /// Id các slot công trình đã dọn trong ván này (design-v7 §B).
        /// Thay hẳn cơ chế điểm xây của design.md §5 — không còn build_points.
        /// </summary>
        public List<string> ClearedSiteIds { get; set; }

        /// <summary>Bậc Ổ đựng thức ăn, bắt đầu 0 = ổ đất trống (design-v7 §B.4).</summary>
        public int FoodNestTier { get; set; }

        /// <summary>
        /// ⚠ CHẾT TỪ v7 — LUÔN TRẢ 0. Cơ chế điểm xây của design.md §5 bị §B thay hẳn:
        /// dọn slot là ăn cả ngã về không, không có gì tích lũy giữa các lượt.
        ///
        /// Giữ lại đúng một thuộc tính chỉ-đọc để nhánh UI (TurnScreenModel) còn biên dịch được
        /// trong lúc họ gỡ chỗ hiển thị "build=cấp/điểm". Xóa khi họ gỡ xong.
        /// </summary>
        public int BuildPoints
        {
            get { return 0; }
        }

        /// <summary>
        /// Số chỗ ở đã mở sẵn của vùng hiện tại, và trần chỗ ở của vùng đó (design-v7 §F.2).
        /// Chép từ sites.json lúc vào vùng, giữ ở đây để GameState không phụ thuộc SiteCatalog
        /// và để ván tải lại không phải tra lại.
        /// </summary>
        public int RegionCapacityOpen { get; set; }

        public int RegionCapacityMax { get; set; }

        /// <summary>
        /// Cấp công trình (design.md §5.2, §8). Từ v7 đây là số SUY RA, không phải số lưu:
        /// = số công trình đã dọn Ở VÙNG NÀY + bậc Ổ đựng thức ăn (§B.2).
        ///
        /// Không có setter là cố ý. Trước v7 nó là trường tự do và cả hai nơi — điểm xây lẫn
        /// cấp — phải tự giữ cho khớp nhau. Nay chỉ còn một nguồn sự thật.
        ///
        /// ⚠ Di cư làm nó TỤT VỀ 0 (trừ bậc ổ), vì công trình thuộc về vùng cũ. Đó là chủ ý:
        /// Khoa gọi di cư là "cơ chế giảm sức mạnh người chơi ở khu vực cũ".
        /// </summary>
        public int BuildLevel
        {
            get { return ClearedSiteIds.Count + FoodNestTier; }
        }

        public List<Member> Members { get; set; }

        /// <summary>Id các nút mạng lưới neuron đã mua, theo thứ tự mua (design.md §7).</summary>
        public List<string> PurchasedNodeIds { get; set; }

        /// <summary>
        /// Id các loài thực vật đã khám phá TRONG VÁN NÀY (design-v7 §A.4).
        /// Ván mới thì xoá sạch, phải tìm lại — D5 của Khoa. Catalog xuyên ván là thứ KHÁC,
        /// nằm ngoài file lưu ván và không được ảnh hưởng lối chơi.
        /// </summary>
        public List<string> DiscoveredPlantIds { get; set; }

        /// <summary>Chiến tích: số trận thắng bất kỳ trong giai đoạn này. Về 0 khi tiến hóa (design.md §7).</summary>
        public int CombatWins { get; set; }

        /// <summary>Chiến tích: số lần săn thành công trong giai đoạn này. Về 0 khi tiến hóa (design.md §7).</summary>
        public int Hunts { get; set; }

        /// <summary>Chiến tích: số lần đã từng di cư. KHÔNG về 0 khi tiến hóa (design.md §7).</summary>
        public int MigratedEver { get; set; }

        /// <summary>Id sẽ cấp cho thành viên sinh tiếp theo; giữ ở đây để tải ván không cấp trùng id.</summary>
        public int NextMemberId { get; set; }

        /// <summary>Số thành viên còn sống.</summary>
        public int Crew
        {
            get { return Members.Count; }
        }

        /// <summary>
        /// Sức chứa hang. ⚠ ĐÃ THAY CÔNG THỨC của design.md §4.
        ///
        /// Cũ: `capacity_base + capacity_per_build_level × build_level`, một công thức cho cả ván.
        /// Nay: theo VÙNG (Khoa 2026-11-09) — số chỗ mở sẵn của vùng, cộng mỗi công trình đã dọn
        /// một chỗ, chặn ở trần của vùng. Ổ đựng thức ăn KHÔNG mở chỗ ([ĐỀ XUẤT]).
        ///
        /// Tham số <paramref name="config"/> giữ lại để nhánh UI không phải sửa lời gọi, nhưng
        /// công thức mới không còn đọc gì từ nó.
        /// </summary>
        public int LairCapacity(BalanceConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            int opened = RegionCapacityOpen + ClearedSiteIds.Count;
            return opened < RegionCapacityMax ? opened : RegionCapacityMax;
        }

        /// <summary>Tổng khẩu phần cả bộ tộc cần trong lượt này (design.md §4).</summary>
        public int FoodNeeded(BalanceConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            return Crew * config.UpkeepForStage(Stage);
        }

        /// <summary>Năng lượng tối đa của lượt, hồi đầy đầu mỗi lượt (design.md §4).</summary>
        public int EnergyMax(BalanceConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            return config.EnergyMaxForStage(Stage);
        }

        public Member FindMember(int id)
        {
            for (int i = 0; i < Members.Count; i++)
            {
                if (Members[i].Id == id)
                {
                    return Members[i];
                }
            }

            return null;
        }

        /// <summary>Ván mới theo balance.json (design.md §3, §4).</summary>
        public static GameState NewGame(BalanceConfig config, IRandom random, MemberFactory memberFactory)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            if (memberFactory == null)
            {
                throw new ArgumentNullException(nameof(memberFactory));
            }

            GameState state = new GameState
            {
                Turn = 1,
                Stage = config.Data.Start.Stage,
                Region = config.Data.Start.Region,
                Neuron = 0,
                Morale = 0,
                FoodNestTier = 0,
                CombatWins = 0,
                Hunts = 0,
                MigratedEver = 0
            };

            for (int i = 0; i < config.Data.Start.Crew; i++)
            {
                state.Members.Add(memberFactory.Create(random));
            }

            state.NextMemberId = memberFactory.NextId;
            return state;
        }
    }
}
