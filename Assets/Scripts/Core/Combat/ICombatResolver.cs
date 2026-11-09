using System;
using System.Collections.Generic;

namespace BeforeUs.Core
{
    /// <summary>Loại trận (design.md §9). Quyết định kiểu trận và phần thưởng.</summary>
    public enum CombatKind
    {
        /// <summary>Săn bắt: kiểu Dụ và hạ (design.md §5, §9).</summary>
        Hunt,

        /// <summary>Khám phá: kiểu Phòng thủ để chiếm vùng mới (design.md §5, §9).</summary>
        Explore,

        /// <summary>Thú tấn công hang, bắt buộc đánh (design.md §8).</summary>
        Defence,

        /// <summary>Cơ hội combat tùy chọn (design.md §8).</summary>
        Opportunity,

        /// <summary>Trận hướng dẫn lượt 3: luôn thắng, không ai chết (design.md §8).</summary>
        Tutorial
    }

    /// <summary>Mọi thứ bộ giải quyết combat cần biết để tính tỉ lệ thắng (design.md §5.2, §8).</summary>
    public sealed class CombatRequest
    {
        private static readonly int[] NoMembers = new int[0];

        public CombatRequest(
            CombatKind kind,
            IReadOnlyList<int> memberIds,
            int stage,
            int region,
            int buildLevel,
            int difficultyLevel,
            int crew,
            IReadOnlyList<int> allMemberIds = null)
        {
            Kind = kind;
            MemberIds = memberIds ?? NoMembers;
            AllMemberIds = allMemberIds ?? MemberIds;
            Stage = stage;
            Region = region;
            BuildLevel = buildLevel;
            DifficultyLevel = difficultyLevel;
            Crew = crew;
        }

        public CombatKind Kind { get; }

        /// <summary>Những người được cử đi. Với Phòng thủ bắt buộc thì cả bộ tộc tham gia.</summary>
        public IReadOnlyList<int> MemberIds { get; }

        public int Stage { get; }
        public int Region { get; }
        public int BuildLevel { get; }
        public int DifficultyLevel { get; }

        /// <summary>Tổng số thành viên còn sống, dùng cho công thức phòng thủ (design.md §8).</summary>
        public int Crew { get; }

        /// <summary>
        /// Id toàn bộ thành viên còn sống, kể cả người ở nhà.
        /// Cần vì thua một trận có thể giết người không tham gia (design.md §5.2: "Thua: mất 2 người"),
        /// và phòng thủ thua thì quái thú ăn `quota` người của cả bầy (design.md §8).
        /// </summary>
        public IReadOnlyList<int> AllMemberIds { get; }

        /// <summary>k = số người được cử (design.md §5.2).</summary>
        public int PartySize
        {
            get { return MemberIds.Count; }
        }
    }

    /// <summary>
    /// Đầu ra của combat cho Core (AGENTS.md, design.md §9).
    /// Người chết trong trận là chết thật, kể cả khi thắng — nên danh sách này áp dụng cho cả hai kết quả.
    /// </summary>
    public sealed class CombatResult
    {
        private static readonly int[] NoDeaths = new int[0];

        public CombatResult(bool won, IReadOnlyList<int> deadMemberIds)
        {
            Won = won;
            DeadMemberIds = deadMemberIds ?? NoDeaths;
        }

        public bool Won { get; }

        public IReadOnlyList<int> DeadMemberIds { get; }

        public static CombatResult Win()
        {
            return new CombatResult(true, null);
        }

        public static CombatResult Loss(IReadOnlyList<int> deadMemberIds)
        {
            return new CombatResult(false, deadMemberIds);
        }
    }

    /// <summary>
    /// Nguồn kết quả combat. Trước khi có scene combat thật (nhánh B), Core dùng bộ giải quyết
    /// xác suất theo công thức design.md §5.2 và §8 (C6).
    ///
    /// Bản MVP đồng bộ. I1 sẽ bọc thêm lớp bất đồng bộ khi nối scene combat thật vào vòng lượt;
    /// chữ ký này giữ nguyên để C4–C8 không phải sửa.
    /// </summary>
    public interface ICombatResolver
    {
        CombatResult Resolve(CombatRequest request);
    }
}
