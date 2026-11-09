namespace BeforeUs.Core
{
    /// <summary>
    /// Hậu quả của việc THẮNG trận Khám phá, tính trước khi bấm kết thúc lượt (design-v7 §F.2).
    /// Dữ liệu thô. Core cố ý không quy ra lời cảnh báo — cách diễn đạt là quyền của UI và của Khoa.
    /// </summary>
    public struct MigrationPreview
    {
        /// <summary>Không còn vùng nào để đi: thắng Khám phá thì chỉ là một trận thắng bình thường.</summary>
        public static readonly MigrationPreview None = new MigrationPreview(-1, 0, 0, 0, 0);

        public MigrationPreview(int nextRegion, int capacityOpen, int crew, int chosen, int leftBehind)
        {
            NextRegion = nextRegion;
            CapacityOpen = capacityOpen;
            Crew = crew;
            Chosen = chosen;
            LeftBehind = leftBehind;
        }

        public bool HasNextRegion
        {
            get { return NextRegion >= 0; }
        }

        public int NextRegion { get; }

        /// <summary>Số chỗ vùng mới mở sẵn, tức trần số người mang theo được.</summary>
        public int CapacityOpen { get; }

        public int Crew { get; }

        /// <summary>Số người người chơi đã chọn cho đoàn di cư. 0 nghĩa là chưa chọn.</summary>
        public int Chosen { get; }

        /// <summary>
        /// Số người sẽ MẤT HẲN nếu thắng. Khoa: "người ở lại mất luôn, đây là cơ chế giảm sức mạnh
        /// người chơi ở khu vực cũ". Không phải người chết — không trừ tinh thần, không có tang.
        /// </summary>
        public int LeftBehind { get; }
    }
}
