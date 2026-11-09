using System;
using System.Collections.Generic;
using BeforeUs.Core;

namespace BeforeUs.Game
{
    /// <summary>
    /// Thao tác chuột của người chơi lên dự định của lượt: chọn một hoạt động, rồi nhấp từng
    /// thành viên để thêm hoặc bớt. Sửa bao nhiêu lần cũng được, không gì được tính cho tới
    /// khi kết thúc lượt (D2).
    ///
    /// Lớp này không có luật riêng: mọi thay đổi đều gọi TurnPlan.Assign của Core và nhận phán
    /// quyết từ đó. Việc duy nhất nó tự làm là giữ nguyên hiện trạng khi một thao tác thất bại.
    /// </summary>
    public sealed class TurnPlanEditor
    {
        private readonly TurnSystem _turn;

        public TurnPlanEditor(TurnSystem turn)
        {
            if (turn == null)
            {
                throw new ArgumentNullException(nameof(turn));
            }

            _turn = turn;
        }

        private TurnPlan Plan
        {
            get { return _turn.Plan; }
        }

        /// <summary>Hoạt động đang được xếp người, hoặc null khi người chơi chưa chọn gì.</summary>
        public ActivityKind? SelectedActivity { get; private set; }

        /// <summary>Kết quả của thao tác gần nhất. Null khi chưa có thao tác nào từ lúc mở lượt.</summary>
        public PlanResult LastResult { get; private set; }

        /// <summary>Hoạt động mà thao tác gần nhất nhắm tới, dùng để hiện lỗi đúng chỗ.</summary>
        public ActivityKind? LastResultActivity { get; private set; }

        /// <summary>
        /// Slot mà nhóm Xây dựng đang nhắm tới. Nay là của Core (TurnPlan.BuildTargetSiteId) —
        /// trước đây nhánh giao diện tự giữ, và chính việc tự giữ đã sinh ra lỗi "mở bảng khác
        /// làm mất slot đang dọn". Core giữ thì không còn hai bản trạng thái để lệch nhau.
        /// </summary>
        public string SelectedSiteId
        {
            get { return Plan.BuildTargetSiteId; }
        }

        /// <summary>Chọn slot để dọn. Phán quyết "slot này chọn được không" là của Core.</summary>
        public PlanResult SelectSite(string siteId)
        {
            PlanResult result = Plan.ChooseSite(siteId);
            if (result.Succeeded)
            {
                SelectActivity(ActivityKind.Build);
            }

            // Đặt SAU SelectActivity: SelectActivity xoá kết quả cũ, nên đặt trước là mất ngay.
            LastResult = result;
            LastResultActivity = ActivityKind.Build;
            return result;
        }

        /// <summary>
        /// Thành viên đang được soi, hoặc null. MỘT nguồn duy nhất: lưới slot và thanh thẻ cùng
        /// đọc nó, không ai giữ bản riêng rồi đồng bộ hai chiều — đó đúng là hình dạng của lỗi
        /// "mở bảng khác làm mất slot đang dọn" đã phải gỡ bằng cách bỏ bản trạng thái thứ hai.
        /// </summary>
        public int? FocusedMemberId { get; private set; }

        public void FocusMember(int memberId)
        {
            FocusedMemberId = FocusedMemberId == memberId ? (int?)null : memberId;
        }

        public void SelectActivity(ActivityKind activity)
        {
            SelectedActivity = activity;
            ClearLastResult();
        }

        /// <summary>Đóng bảng đang mở. KHÔNG bỏ mục tiêu slot — nhóm Xây dựng vẫn đang làm việc đó.</summary>
        public void Deselect()
        {
            SelectedActivity = null;
            ClearLastResult();
        }

        public void ClearLastResult()
        {
            LastResult = null;
            LastResultActivity = null;
        }

        /// <summary>Quên hết dự định đang xếp và bỏ chọn. Gọi khi sang lượt mới.</summary>
        public void Reset()
        {
            SelectedActivity = null;
            FocusedMemberId = null;
            ClearLastResult();
        }

        /// <summary>Bỏ toàn bộ người khỏi một hoạt động. Luôn thành công (D2: dự định sửa tự do).</summary>
        public void ClearActivity(ActivityKind activity)
        {
            if (activity == ActivityKind.Build)
            {
                Plan.ChooseSite(null);
            }

            Plan.Clear(activity);
            LastResult = PlanResult.Ok;
            LastResultActivity = activity;
        }

        /// <summary>
        /// Nhấp vào một thành viên khi đang chọn một hoạt động: đang ở trong thì bỏ ra,
        /// chưa ở trong thì đưa vào — kể cả khi người đó đang bận việc khác (đổi việc).
        /// Thất bại thì dự định giữ nguyên y như trước khi nhấp.
        /// </summary>
        public bool ToggleMember(int memberId)
        {
            if (SelectedActivity == null)
            {
                return false;
            }

            ActivityKind target = SelectedActivity.Value;
            List<int> targetBefore = Snapshot(target);
            LastResultActivity = target;

            if (targetBefore.Contains(memberId))
            {
                targetBefore.Remove(memberId);
                LastResult = Plan.Assign(target, targetBefore);
                return LastResult.Succeeded;
            }

            ActivityKind? other = FindActivityOf(memberId);
            List<int> otherBefore = null;
            if (other != null)
            {
                // Bỏ khỏi việc cũ TRƯỚC: nếu việc cũ thành rỗng thì năng lượng được trả lại,
                // và chính chỗ năng lượng đó có thể là chỗ việc mới cần.
                otherBefore = Snapshot(other.Value);
                List<int> otherAfter = new List<int>(otherBefore);
                otherAfter.Remove(memberId);

                PlanResult removal = Plan.Assign(other.Value, otherAfter);
                if (!removal.Succeeded)
                {
                    LastResult = removal;
                    LastResultActivity = other.Value;
                    return false;
                }
            }

            targetBefore.Add(memberId);
            PlanResult result = Plan.Assign(target, targetBefore);
            if (!result.Succeeded && other != null)
            {
                // Trả người về việc cũ: trạng thái này vừa hợp lệ một khoảnh khắc trước nên khôi phục được.
                Plan.Assign(other.Value, otherBefore);
            }

            LastResult = result;
            return result.Succeeded;
        }

        /// <summary>
        /// Thêm/bớt một người khỏi đoàn di cư (design-v7 §F.2). Core quyết sức chứa và từ chối
        /// nếu chọn quá; thất bại thì danh sách cũ giữ nguyên, cùng kỷ luật với ToggleMember.
        /// </summary>
        public bool ToggleMigrationMember(int memberId)
        {
            List<int> next = new List<int>(Plan.MigrationParty);
            if (!next.Remove(memberId))
            {
                next.Add(memberId);
            }

            PlanResult result = Plan.ChooseMigrationParty(next);
            LastResult = result;
            LastResultActivity = ActivityKind.Explore;
            return result.Succeeded;
        }

        /// <summary>Chọn cả bộ tộc theo sang vùng mới. Core từ chối nếu không đủ chỗ.</summary>
        public bool ChooseWholeTribeForMigration(IReadOnlyList<Member> members)
        {
            List<int> all = new List<int>();
            for (int i = 0; i < members.Count; i++)
            {
                all.Add(members[i].Id);
            }

            PlanResult result = Plan.ChooseMigrationParty(all);
            LastResult = result;
            LastResultActivity = ActivityKind.Explore;
            return result.Succeeded;
        }

        public ActivityKind? FindActivityOf(int memberId)
        {
            for (int i = 0; i < ActivityRules.All.Length; i++)
            {
                ActivityKind activity = ActivityRules.All[i];
                IReadOnlyList<int> members = Plan.MembersOn(activity);
                for (int j = 0; j < members.Count; j++)
                {
                    if (members[j] == memberId)
                    {
                        return activity;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Bản sao danh sách người của một hoạt động. Phải sao: TurnPlan.MembersOn trả về chính
        /// danh sách bên trong nó, sửa thẳng lên đó là sửa trộm dự định mà không qua xác thực.
        /// </summary>
        private List<int> Snapshot(ActivityKind activity)
        {
            return new List<int>(Plan.MembersOn(activity));
        }
    }
}
