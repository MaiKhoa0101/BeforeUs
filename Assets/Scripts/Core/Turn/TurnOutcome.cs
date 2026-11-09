using System.Collections.Generic;

using BeforeUs.Core.Sites;

namespace BeforeUs.Core
{
    /// <summary>Kết quả một hoạt động sau khi giải quyết.</summary>
    public sealed class ActivityOutcome
    {
        public ActivityOutcome(ActivityKind kind, int partySize)
        {
            Kind = kind;
            PartySize = partySize;
            DeadMemberIds = new List<int>();
        }

        public ActivityKind Kind { get; }

        /// <summary>k = số người được cử (design.md §5).</summary>
        public int PartySize { get; }

        public int Food { get; internal set; }
        public int Morale { get; internal set; }
        public int Neuron { get; internal set; }

        /// <summary>Chỉ có nghĩa với Săn bắt và Khám phá.</summary>
        public bool? CombatWon { get; internal set; }

        public List<int> DeadMemberIds { get; }
    }

    /// <summary>
    /// Mọi thứ đã xảy ra trong một lượt. Dùng cho UI (A1, A2), cho telemetry (I3) và cho bot ở C8.
    /// Chỉ là báo cáo — thay đổi thật đã nằm trong GameState.
    /// </summary>
    public sealed class TurnOutcome
    {
        public TurnOutcome()
        {
            Activities = new List<ActivityOutcome>();
            DeadMemberIds = new List<int>();
            BornMemberIds = new List<int>();
            LeftBehindMemberIds = new List<int>();
        }

        /// <summary>Lượt vừa kết thúc.</summary>
        public int Turn { get; internal set; }

        public ClimatePhase Phase { get; internal set; }
        public int DifficultyLevel { get; internal set; }

        /// <summary>
        /// Chi tiết lần Kiếm ăn: ai tìm được loài gì, có khám phá được loài mới không
        /// (design-v7 §A.3). <see cref="GatherOutcome.Empty"/> nếu lượt này không ai đi kiếm ăn.
        /// </summary>
        public GatherOutcome Gather { get; internal set; } = GatherOutcome.Empty;

        /// <summary>
        /// Chuyện gì xảy ra với slot công trình lượt này (design-v7 §B.1).
        /// <see cref="SiteResolution.None"/> nếu không ai đi Xây dựng.
        /// </summary>
        public SiteResolution Site { get; internal set; } = SiteResolution.None;

        /// <summary>
        /// Những người BỊ BỎ LẠI khi di cư. Khoa: "người ở lại mất luôn".
        /// KHÔNG nằm trong <see cref="DeadMemberIds"/> — họ không chết, nên không trừ tinh thần
        /// theo morale.on_member_death. UI nên nói về họ bằng giọng khác với giọng nói về người chết.
        /// </summary>
        public List<int> LeftBehindMemberIds { get; }

        public List<ActivityOutcome> Activities { get; }

        /// <summary>Thức ăn từ hoạt động.</summary>
        public int FoodFromActivities { get; internal set; }

        /// <summary>Thức ăn từ sự kiện, cả đầu lượt lẫn lượt máy (design.md §8).</summary>
        public int FoodFromEvents { get; internal set; }

        /// <summary>Tổng thức ăn của lượt. Không để dành sang lượt sau (design.md §4).</summary>
        public int FoodTotal { get; internal set; }

        /// <summary>Tổng khẩu phần cần, tính trên số người còn sống sau combat (design.md §4).</summary>
        public int FoodNeeded { get; internal set; }

        public bool Starved { get; internal set; }
        public int StarvationDeaths { get; internal set; }

        public int NeuronGained { get; internal set; }

        /// <summary>Thay đổi tinh thần ròng của lượt, sau khi đã chặn ở 0.</summary>
        public int MoraleDelta { get; internal set; }

        public int BuildLevelBefore { get; internal set; }
        public int BuildLevelAfter { get; internal set; }

        public int RegionBefore { get; internal set; }
        public int RegionAfter { get; internal set; }

        public int StageBefore { get; internal set; }
        public int StageAfter { get; internal set; }

        /// <summary>Tất cả người chết trong lượt, mọi nguyên nhân.</summary>
        public List<int> DeadMemberIds { get; }

        /// <summary>Thành viên sinh ra cuối lượt (design.md §6). Tối đa 1 mỗi lượt.</summary>
        public List<int> BornMemberIds { get; }

        /// <summary>Số thành viên còn sống sau khi lượt đã giải quyết xong.</summary>
        public int CrewAfter { get; internal set; }

        /// <summary>
        /// Thua ván (design.md §1). Theo góp ý 8 của Khoa ở playtest 01, ngưỡng là
        /// `lose_when_crew_at_or_below` trong balance.json, không còn là "hết sạch người".
        /// </summary>
        public bool Lost { get; internal set; }

        /// <summary>
        /// Bộ tộc không còn ai. Hẹp hơn <see cref="Lost"/> — giữ lại vì vẫn là thông tin có thật,
        /// nhưng điều kiện THUA là <see cref="Lost"/>.
        /// </summary>
        public bool TribeWipedOut
        {
            get { return CrewAfter == 0; }
        }

        /// <summary>Phần tinh thần mất đi vì có người chết trong lượt (playtest 01, góp ý 1). Luôn &lt;= 0.</summary>
        public int MoraleFromDeaths { get; internal set; }

        /// <summary>Đã đạt giai đoạn thắng của chế độ đang chạy (design.md §1, §7).</summary>
        public bool Won { get; internal set; }

        /// <summary>Đã tiến hóa trong lượt này (mở nút Tiến hóa).</summary>
        public bool Evolved
        {
            get { return StageAfter > StageBefore; }
        }

        public bool BuildLevelIncreased
        {
            get { return BuildLevelAfter > BuildLevelBefore; }
        }

        public bool Migrated
        {
            get { return RegionAfter > RegionBefore; }
        }

        public ActivityOutcome OutcomeOf(ActivityKind kind)
        {
            for (int i = 0; i < Activities.Count; i++)
            {
                if (Activities[i].Kind == kind)
                {
                    return Activities[i];
                }
            }

            return null;
        }
    }
}
