using System;
using System.Collections.Generic;
using BeforeUs.Core.Config;
using BeforeUs.Core.Sites;

namespace BeforeUs.Core
{
    public enum PlanError
    {
        None,

        /// <summary>Hoạt động chưa mở khóa ở lượt này (design.md §5, D1).</summary>
        ActivityLocked,

        /// <summary>Id không có trong bộ tộc.</summary>
        UnknownMember,

        /// <summary>Thành viên đã được cử cho hoạt động khác (D2: mỗi người tối đa 1 hoạt động mỗi lượt).</summary>
        MemberAlreadyAssigned,

        /// <summary>Cùng một người xuất hiện hai lần trong danh sách.</summary>
        DuplicateMember,

        /// <summary>Không đủ năng lượng (design.md §4, §5).</summary>
        NotEnoughEnergy,

        /// <summary>
        /// Chưa đủ người đi Kiếm ăn (design-v7 §A.6, D6 của Khoa: ít nhất 1/3 bộ tộc).
        /// Chỉ xuất hiện khi kết thúc lượt, không phải khi phân công.
        /// </summary>
        GatherFloorNotMet,

        /// <summary>Cử người đi Xây dựng mà chưa chọn slot nào (design-v7 §B.1).</summary>
        SiteNotChosen,

        /// <summary>Slot không tồn tại, đã dọn xong, hoặc chưa hiện ra ở giai đoạn/vùng này.</summary>
        SiteNotAvailable,

        /// <summary>
        /// Cử người đi Khám phá mà chưa chọn ai sẽ theo sang vùng mới nếu thắng (design-v7 §F.2).
        /// Thắng trận Khám phá là DI CƯ, và người không được chọn thì mất hẳn — nên Core bắt
        /// người chơi quyết trước khi bấm, không để họ biết luật bằng cách mất ván.
        /// </summary>
        MigrationPartyNotChosen,

        /// <summary>Chọn nhiều người hơn số chỗ vùng mới mở sẵn.</summary>
        MigrationPartyTooLarge
    }

    public sealed class PlanResult
    {
        public static readonly PlanResult Ok = new PlanResult(PlanError.None, null);

        private PlanResult(PlanError error, string message)
        {
            Error = error;
            Message = message;
        }

        public PlanError Error { get; }
        public string Message { get; }

        public bool Succeeded
        {
            get { return Error == PlanError.None; }
        }

        public static PlanResult Fail(PlanError error, string message)
        {
            return new PlanResult(error, message);
        }
    }

    /// <summary>
    /// Phân công trong lượt. Theo D2 đây chỉ là DỰ ĐỊNH: người chơi sửa tự do bao nhiêu lần tùy ý,
    /// không gì được tính cho tới khi bấm kết thúc lượt. Lớp này chỉ giữ dự định và xác thực
    /// ngân sách năng lượng tại chỗ; nó không bao giờ thay đổi GameState.
    ///
    /// Hai bất biến D2 mà lớp này canh:
    ///   - một hoạt động xuất hiện tối đa một lần mỗi lượt (map hoạt động → tập memberId);
    ///   - mỗi thành viên tối đa một hoạt động mỗi lượt.
    /// </summary>
    public sealed class TurnPlan
    {
        private static readonly int[] NoMembers = new int[0];

        private readonly BalanceConfig _config;
        private readonly GameState _state;
        private readonly IPurchasedNodes _nodes;
        private readonly Dictionary<ActivityKind, List<int>> _assignments;
        private readonly SiteSystem _sites;
        private readonly List<int> _migrationParty = new List<int>();

        internal TurnPlan(BalanceConfig config, GameState state, IPurchasedNodes nodes, SiteSystem sites = null)
        {
            _config = config;
            _state = state;
            _nodes = nodes;
            _sites = sites;
            _assignments = new Dictionary<ActivityKind, List<int>>();
        }

        /// <summary>
        /// Slot công trình đang nhắm tới lượt này (design-v7 §B.1). null nghĩa là chưa chọn.
        /// Chọn slot KHÔNG tiêu năng lượng và không đổi gì — như mọi thứ khác trong dự định (D2).
        /// </summary>
        public string BuildTargetSiteId { get; private set; }

        /// <summary>
        /// Chọn slot cho hoạt động Xây dựng. Truyền null để bỏ chọn.
        /// Trả lỗi nếu slot không có, đã dọn xong, hoặc chưa hiện ra — chứ không âm thầm bỏ qua.
        /// </summary>
        public PlanResult ChooseSite(string siteId)
        {
            if (siteId == null)
            {
                BuildTargetSiteId = null;
                return PlanResult.Ok;
            }

            if (_sites == null)
            {
                return PlanResult.Fail(
                    PlanError.SiteNotAvailable,
                    "Ván này chưa có SiteCatalog, không chọn được slot công trình");
            }

            SiteView site = _sites.Find(siteId);
            if (site == null)
            {
                return PlanResult.Fail(PlanError.SiteNotAvailable, "Không có slot công trình " + siteId);
            }

            if (site.Cleared)
            {
                return PlanResult.Fail(PlanError.SiteNotAvailable, "Slot " + siteId + " đã dọn xong rồi");
            }

            if (!IsVisible(siteId))
            {
                return PlanResult.Fail(
                    PlanError.SiteNotAvailable,
                    "Slot " + siteId + " chưa hiện ra ở giai đoạn hoặc vùng này");
            }

            BuildTargetSiteId = siteId;
            return PlanResult.Ok;
        }

        /// <summary>
        /// Những người sẽ theo sang vùng mới nếu trận Khám phá lượt này THẮNG (design-v7 §F.2).
        /// Rỗng nghĩa là chưa chọn. Người không có trong danh sách này **mất hẳn** khi di cư.
        /// </summary>
        public IReadOnlyList<int> MigrationParty
        {
            get { return _migrationParty; }
        }

        /// <summary>
        /// Chọn đoàn di cư. Danh sách rỗng hoặc null = bỏ chọn.
        /// Không tiêu gì, không đổi gì — như mọi thứ khác trong dự định (D2).
        ///
        /// Core CỐ Ý không bắt đoàn di cư phải chứa nhóm đi Khám phá. Ai đi và ai ở là quyết định
        /// của người chơi; Core chỉ canh trần số chỗ. [ĐỀ XUẤT], chờ Khoa bác nếu muốn chặt hơn.
        /// </summary>
        public PlanResult ChooseMigrationParty(IReadOnlyList<int> memberIds)
        {
            IReadOnlyList<int> ids = memberIds ?? NoMembers;

            if (ids.Count == 0)
            {
                _migrationParty.Clear();
                return PlanResult.Ok;
            }

            int capacity = MigrationCapacity;
            if (capacity <= 0)
            {
                return PlanResult.Fail(
                    PlanError.MigrationPartyTooLarge,
                    "Không còn vùng nào để di cư sang");
            }

            if (ids.Count > capacity)
            {
                return PlanResult.Fail(
                    PlanError.MigrationPartyTooLarge,
                    "Vùng mới chỉ mở sẵn " + capacity + " chỗ, đang chọn " + ids.Count + " người");
            }

            HashSet<int> seen = new HashSet<int>();
            for (int i = 0; i < ids.Count; i++)
            {
                if (!seen.Add(ids[i]))
                {
                    return PlanResult.Fail(PlanError.DuplicateMember, "Thành viên " + ids[i] + " xuất hiện hai lần");
                }

                if (_state.FindMember(ids[i]) == null)
                {
                    return PlanResult.Fail(PlanError.UnknownMember, "Không có thành viên " + ids[i]);
                }
            }

            _migrationParty.Clear();
            _migrationParty.AddRange(ids);
            return PlanResult.Ok;
        }

        public PlanResult ChooseMigrationParty(params int[] memberIds)
        {
            return ChooseMigrationParty((IReadOnlyList<int>)(memberIds ?? NoMembers));
        }

        /// <summary>Số chỗ vùng kế tiếp mở sẵn, tức trần số người mang theo được. 0 = hết vùng.</summary>
        public int MigrationCapacity
        {
            get { return _sites == null ? 0 : _sites.NextRegionCapacityOpen(_config.RegionMaxIndex); }
        }

        /// <summary>
        /// Nếu thắng trận Khám phá lượt này thì phải bỏ lại bao nhiêu người.
        /// Truy vấn thuần — UI hiện con số này TRƯỚC khi người chơi bấm kết thúc lượt.
        /// </summary>
        public int MembersLeftBehindIfMigrating
        {
            get
            {
                if (MigrationCapacity <= 0)
                {
                    return 0;
                }

                int taken = _migrationParty.Count > 0
                    ? _migrationParty.Count
                    : Math.Min(_state.Crew, MigrationCapacity);

                return Math.Max(0, _state.Crew - taken);
            }
        }

        /// <summary>Nhóm đang được cử đi Xây dựng có dọn nổi slot đã chọn không, nếu kết thúc lượt bây giờ.</summary>
        public bool WouldClearSite
        {
            get
            {
                if (_sites == null || BuildTargetSiteId == null)
                {
                    return false;
                }

                return _sites.WouldClear(BuildTargetSiteId, MembersOn(ActivityKind.Build));
            }
        }

        private bool IsVisible(string siteId)
        {
            List<SiteView> visible = _sites.VisibleSites(_state.Stage, _nodes);
            for (int i = 0; i < visible.Count; i++)
            {
                if (visible[i].Id == siteId)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Năng lượng đã đặt: tổng chi phí của những hoạt động có ít nhất một người.
        /// Một lần cho mỗi hoạt động, không phụ thuộc số người (design.md §2, D2).</summary>
        public int EnergySpent
        {
            get
            {
                int spent = 0;
                foreach (KeyValuePair<ActivityKind, List<int>> pair in _assignments)
                {
                    if (pair.Value.Count > 0)
                    {
                        spent += _config.EnergyCostOf(pair.Key);
                    }
                }

                return spent;
            }
        }

        public int EnergyMax
        {
            get { return _state.EnergyMax(_config); }
        }

        public int EnergyRemaining
        {
            get { return EnergyMax - EnergySpent; }
        }

        /// <summary>Số người chưa được cử đi đâu.</summary>
        public int IdleMemberCount
        {
            get { return _state.Crew - AssignedMemberCount; }
        }

        public int AssignedMemberCount
        {
            get
            {
                int count = 0;
                foreach (KeyValuePair<ActivityKind, List<int>> pair in _assignments)
                {
                    count += pair.Value.Count;
                }

                return count;
            }
        }

        public IReadOnlyList<int> MembersOn(ActivityKind activity)
        {
            List<int> members;
            return _assignments.TryGetValue(activity, out members) ? members : (IReadOnlyList<int>)NoMembers;
        }

        public int PartySizeOn(ActivityKind activity)
        {
            return MembersOn(activity).Count;
        }

        public bool IsBusy(int memberId)
        {
            foreach (KeyValuePair<ActivityKind, List<int>> pair in _assignments)
            {
                if (pair.Value.Contains(memberId))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsUnlocked(ActivityKind activity)
        {
            return ActivityRules.IsUnlocked(_config, _state, _nodes, activity);
        }

        /// <summary>
        /// Số người TỐI THIỂU phải đi Kiếm ăn lượt này (design-v7 §A.6).
        /// D6 của Khoa: "ít nhất 1/3 số người phải đi tìm thức ăn để hạn chế việc người chơi
        /// phát triển quá nhanh".
        /// </summary>
        public int GatherFloor
        {
            get { return GatherRules.FloorForCrew(_config, _state.Crew); }
        }

        public bool MeetsGatherFloor
        {
            get { return PartySizeOn(ActivityKind.Gather) >= GatherFloor; }
        }

        /// <summary>
        /// Bộ tộc có CÓ THỂ đạt sàn không, nếu người chơi muốn.
        ///
        /// Sàn chặn khi người chơi CÓ THỂ đạt mà CHỌN không. Nó KHÔNG được chặn khi họ
        /// KHÔNG THỂ — một luật giới hạn mà khoá người chơi vào ngõ cụt thì tệ hơn là không có luật.
        /// </summary>
        public bool CanPossiblyMeetGatherFloor
        {
            get
            {
                if (GatherFloor <= 0)
                {
                    return true;
                }

                return IsUnlocked(ActivityKind.Gather)
                       && _state.Crew >= GatherFloor
                       && EnergyMax >= _config.EnergyCostOf(ActivityKind.Gather);
            }
        }

        /// <summary>
        /// Dự định này có kết thúc lượt được không. UI gọi trước khi bật nút "Kết thúc lượt";
        /// <see cref="TurnSystem.EndTurn"/> cũng tự kiểm và ném nếu chưa đạt.
        /// Truy vấn thuần, không tác dụng phụ.
        /// </summary>
        public PlanResult ValidateForEndTurn()
        {
            // Ca suy biến: không thể đạt thì không chặn. Ví dụ Kiếm ăn bị khoá, hoặc năng lượng
            // tối đa còn ít hơn chi phí của chính hoạt động Kiếm ăn.
            if (!CanPossiblyMeetGatherFloor)
            {
                return PlanResult.Ok;
            }

            int floor = GatherFloor;
            if (PartySizeOn(ActivityKind.Gather) < floor)
            {
                return PlanResult.Fail(
                    PlanError.GatherFloorNotMet,
                    "Phải cử ít nhất " + floor + " người đi Kiếm ăn, đang có "
                    + PartySizeOn(ActivityKind.Gather));
            }

            // design-v7 §B.1: Xây dựng là dọn MỘT slot cụ thể. Cử người mà không chọn slot thì
            // không có gì để giải quyết — chặn ở đây chứ không lặng lẽ đốt 2 năng lượng.
            if (PartySizeOn(ActivityKind.Build) > 0 && BuildTargetSiteId == null)
            {
                return PlanResult.Fail(
                    PlanError.SiteNotChosen,
                    "Đã cử người đi Xây dựng nhưng chưa chọn slot công trình nào");
            }

            // design-v7 §F.2: thắng trận Khám phá LÀ di cư, và di cư bỏ người lại vĩnh viễn.
            // Bắt chọn trước khi bấm, để người chơi không học luật này bằng cách mất ván.
            if (PartySizeOn(ActivityKind.Explore) > 0 && MigrationCapacity > 0 && _migrationParty.Count == 0)
            {
                return PlanResult.Fail(
                    PlanError.MigrationPartyNotChosen,
                    "Thắng trận Khám phá là di cư. Phải chọn tối đa " + MigrationCapacity
                    + " người theo sang vùng mới; " + MembersLeftBehindIfMigrating + " người còn lại sẽ mất hẳn");
            }

            return PlanResult.Ok;
        }

        /// <summary>
        /// Năng lượng phải trả THÊM để đưa hoạt động này vào dự định hiện tại.
        /// Bằng 0 nếu hoạt động đã có người: một hoạt động trả năng lượng đúng một lần mỗi lượt,
        /// không phụ thuộc số người (design.md §2, D2) — nên cử thêm người thì không tốn thêm gì.
        /// </summary>
        public int EnergyCostToAdd(ActivityKind activity)
        {
            return PartySizeOn(activity) > 0 ? 0 : _config.EnergyCostOf(activity);
        }

        /// <summary>
        /// Còn đủ năng lượng để thêm hoạt động này vào dự định hiện tại không.
        /// Truy vấn thuần, KHÔNG có tác dụng phụ — gọi bao nhiêu lần cũng không đổi gì.
        ///
        /// Chỉ nói về năng lượng. Muốn biết thẻ có bấm được không thì cần cả
        /// <see cref="IsUnlocked"/>; tách hai thứ để UI nói được lý do làm mờ là "chưa mở khóa"
        /// hay "hết năng lượng".
        /// </summary>
        public bool CanAfford(ActivityKind activity)
        {
            return EnergyCostToAdd(activity) <= EnergyRemaining;
        }

        /// <summary>
        /// Đặt lại toàn bộ danh sách người cho một hoạt động. Danh sách rỗng tương đương bỏ hoạt động đó.
        /// Thất bại thì KHÔNG đổi gì — dự định cũ giữ nguyên.
        /// </summary>
        public PlanResult Assign(ActivityKind activity, params int[] memberIds)
        {
            return Assign(activity, (IReadOnlyList<int>)(memberIds ?? NoMembers));
        }

        public PlanResult Assign(ActivityKind activity, IReadOnlyList<int> memberIds)
        {
            IReadOnlyList<int> ids = memberIds ?? NoMembers;

            if (ids.Count == 0)
            {
                Clear(activity);
                return PlanResult.Ok;
            }

            if (!IsUnlocked(activity))
            {
                return PlanResult.Fail(PlanError.ActivityLocked, "Hoạt động " + activity + " chưa mở khóa");
            }

            HashSet<int> wanted = new HashSet<int>();
            for (int i = 0; i < ids.Count; i++)
            {
                int id = ids[i];

                if (!wanted.Add(id))
                {
                    return PlanResult.Fail(PlanError.DuplicateMember, "Thành viên " + id + " xuất hiện hai lần");
                }

                if (_state.FindMember(id) == null)
                {
                    return PlanResult.Fail(PlanError.UnknownMember, "Không có thành viên " + id);
                }

                ActivityKind other;
                if (TryGetActivityOf(id, activity, out other))
                {
                    return PlanResult.Fail(
                        PlanError.MemberAlreadyAssigned,
                        "Thành viên " + id + " đã được cử đi " + other);
                }
            }

            int energyAfter = EnergySpentIfActivityHasMembers(activity);
            if (energyAfter > EnergyMax)
            {
                return PlanResult.Fail(
                    PlanError.NotEnoughEnergy,
                    "Cần " + energyAfter + " năng lượng, chỉ có " + EnergyMax);
            }

            List<int> slot;
            if (!_assignments.TryGetValue(activity, out slot))
            {
                slot = new List<int>();
                _assignments[activity] = slot;
            }

            slot.Clear();
            slot.AddRange(ids);
            return PlanResult.Ok;
        }

        public void Clear(ActivityKind activity)
        {
            _assignments.Remove(activity);

            if (activity == ActivityKind.Build)
            {
                BuildTargetSiteId = null;
            }

            if (activity == ActivityKind.Explore)
            {
                _migrationParty.Clear();
            }
        }

        public void ClearAll()
        {
            _assignments.Clear();
            BuildTargetSiteId = null;
            _migrationParty.Clear();
        }

        /// <summary>Những hoạt động có ít nhất một người, theo thứ tự giải quyết cố định của TurnSystem.</summary>
        internal List<ActivityKind> ActiveActivitiesInResolveOrder()
        {
            List<ActivityKind> active = new List<ActivityKind>();
            for (int i = 0; i < TurnSystem.ResolveOrder.Length; i++)
            {
                ActivityKind activity = TurnSystem.ResolveOrder[i];
                if (PartySizeOn(activity) > 0)
                {
                    active.Add(activity);
                }
            }

            return active;
        }

        private bool TryGetActivityOf(int memberId, ActivityKind ignoring, out ActivityKind found)
        {
            foreach (KeyValuePair<ActivityKind, List<int>> pair in _assignments)
            {
                if (pair.Key == ignoring)
                {
                    continue;
                }

                if (pair.Value.Contains(memberId))
                {
                    found = pair.Key;
                    return true;
                }
            }

            found = default(ActivityKind);
            return false;
        }

        private int EnergySpentIfActivityHasMembers(ActivityKind activity)
        {
            int spent = _config.EnergyCostOf(activity);
            foreach (KeyValuePair<ActivityKind, List<int>> pair in _assignments)
            {
                if (pair.Key != activity && pair.Value.Count > 0)
                {
                    spent += _config.EnergyCostOf(pair.Key);
                }
            }

            return spent;
        }
    }
}
