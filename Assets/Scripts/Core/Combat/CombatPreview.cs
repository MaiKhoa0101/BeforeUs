namespace BeforeUs.Core
{
    /// <summary>
    /// Thông tin về một trận SẮP xảy ra, để người chơi cân nhắc trước khi kết thúc lượt
    /// (góp ý 3 của Khoa ở playtest 01).
    ///
    /// Cố ý chỉ mang dữ liệu thô. Core không quy ra "dễ / vừa / khó" vì cách diễn đạt là
    /// quyết định thiết kế của Khoa, không phải của luật.
    /// </summary>
    public sealed class CombatPreview
    {
        /// <summary>Chưa cử ai, hoặc hoạt động này không mở combat.</summary>
        public static readonly CombatPreview None = new CombatPreview(0, 0, null);

        internal CombatPreview(int partySize, int foodIfWon, double? winChance)
        {
            PartySize = partySize;
            FoodIfWon = foodIfWon;
            WinChance = winChance;
        }

        public bool HasParty
        {
            get { return PartySize > 0; }
        }

        /// <summary>k = số người đang được cử đi.</summary>
        public int PartySize { get; }

        /// <summary>
        /// Thức ăn mang về NẾU thắng: k × hunt_food_per_member[stage] (design.md §5.2).
        /// Khám phá không ra thức ăn nên luôn 0.
        /// Lưu ý: ở giai đoạn 1 hiện là 0 — xem docs/playtest-01.md, việc còn treo.
        /// </summary>
        public int FoodIfWon { get; }

        /// <summary>
        /// Tỉ lệ thắng theo công thức design.md §5.2, đã clamp.
        /// null khi không có nguồn ước lượng (<see cref="ICombatOdds"/>) — lúc đó UI phải
        /// hiện "chưa rõ" chứ KHÔNG được tự tính lấy.
        /// </summary>
        public double? WinChance { get; }
    }
}
