using System;
using System.Collections.Generic;
using BeforeUs.Core;

namespace BeforeUs.Combat
{
    /// <summary>
    /// Tra `Member` theo id. Cần vì `CombatRequest` của Core chỉ mang id, không mang chỉ số —
    /// mà kiểu tấn công "săn kẻ yếu" và "thách đấu kẻ mạnh" (design.md §9) phải đọc `hp` và
    /// `strength` của từng người, kể cả người ở nhà.
    ///
    /// Khai ở nhánh combat chứ không phải trong Core: đây là nhu cầu của nhánh này,
    /// không phải một mảnh luật chơi còn thiếu. Core không phải đổi gì.
    /// </summary>
    public interface IMemberSource
    {
        /// <summary>Trả về null nếu không có id đó.</summary>
        Member Find(int memberId);
    }

    /// <summary>Bản tra theo danh sách, đủ dùng cho test và cho B3.</summary>
    public sealed class MemberList : IMemberSource
    {
        private readonly Dictionary<int, Member> _byId = new Dictionary<int, Member>();

        public MemberList(IReadOnlyList<Member> members)
        {
            if (members == null) throw new ArgumentNullException(nameof(members));

            for (int i = 0; i < members.Count; i++)
            {
                Member member = members[i];
                if (member != null)
                {
                    _byId[member.Id] = member;
                }
            }
        }

        public Member Find(int memberId)
        {
            return _byId.TryGetValue(memberId, out Member member) ? member : null;
        }
    }
}
