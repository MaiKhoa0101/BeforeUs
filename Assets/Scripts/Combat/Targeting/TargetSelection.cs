using System;
using System.Collections.Generic;
using BeforeUs.Core;

namespace BeforeUs.Combat
{
    /// <summary>
    /// Ba kiểu tấn công của design.md §9 — thứ quyết định AI CHẾT.
    ///
    /// Hai ngữ cảnh khác nhau, cố ý tách làm hai hàm:
    ///   • <see cref="PickOnField"/> — chọn người để đánh TRONG trận, đọc máu CÒN LẠI.
    ///   • <see cref="PickFromTribe"/> — chọn thêm nạn nhân trong cả bộ tộc khi thua, đọc `hp` GỐC.
    /// Lý do xem ghi chú [ĐỀ XUẤT] trên từng hàm.
    ///
    /// Hoà nhau thì bốc ngẫu nhiên qua <see cref="IRandom"/>, không ưu tiên id nhỏ: thiên vị theo id
    /// là loại lệch không ai nhìn ra cho tới khi có người để ý nhân vật #1 chết nhiều bất thường.
    /// </summary>
    public static class TargetSelection
    {
        /// <summary>
        /// Chọn mục tiêu trong số những người đang đứng trên sân.
        ///
        /// [ĐỀ XUẤT] "Săn kẻ yếu" ở đây đọc MÁU CÒN LẠI, không phải `hp` gốc. design.md §3 viết
        /// `hp` là chỉ số cố định suốt đời, nên đọc theo nghĩa đen thì phải là chỉ số gốc; dùng máu
        /// còn lại là một DIỄN GIẢI cho hợp trận thời gian thực — thú dồn vào người đang bị thương.
        /// Khoa thấy và chốt; đổi lại chỉ là đổi một dòng ở đây.
        /// </summary>
        public static Combatant PickOnField(
            BeastAttackStyle style,
            IReadOnlyList<Combatant> candidates,
            IRandom random)
        {
            if (candidates == null) throw new ArgumentNullException(nameof(candidates));
            if (random == null) throw new ArgumentNullException(nameof(random));

            if (candidates.Count == 0)
            {
                return null;
            }

            if (candidates.Count == 1)
            {
                return candidates[0];
            }

            if (style == BeastAttackStyle.Random)
            {
                return candidates[random.NextInt(0, candidates.Count)];
            }

            bool weakest = style == BeastAttackStyle.HuntWeakest;
            double best = weakest ? double.MaxValue : double.MinValue;

            for (int i = 0; i < candidates.Count; i++)
            {
                double value = weakest ? candidates[i].Hp : candidates[i].Spec.Strength;
                if (weakest ? value < best : value > best)
                {
                    best = value;
                }
            }

            List<Combatant> tied = new List<Combatant>();
            for (int i = 0; i < candidates.Count; i++)
            {
                double value = weakest ? candidates[i].Hp : candidates[i].Spec.Strength;
                if (value.Equals(best))
                {
                    tied.Add(candidates[i]);
                }
            }

            return tied.Count == 1 ? tied[0] : tied[random.NextInt(0, tied.Count)];
        }

        /// <summary>
        /// Chọn một nạn nhân trong danh sách id cho trước, dùng khi phải lấy thêm người ngoài sân
        /// cho đủ mức tổn thất của luật (design.md §5.2, §8).
        ///
        /// Ở đây đọc `hp` GỐC của <see cref="Member"/>, vì người ở nhà chưa hề bị gì —
        /// máu còn lại của họ đúng bằng chỉ số gốc.
        ///
        /// Trả về id, hoặc null nếu danh sách rỗng. Id không tra được bị bỏ qua.
        /// </summary>
        public static int? PickFromTribe(
            BeastAttackStyle style,
            IReadOnlyList<int> memberIds,
            IMemberSource members,
            IRandom random)
        {
            if (memberIds == null) throw new ArgumentNullException(nameof(memberIds));
            if (members == null) throw new ArgumentNullException(nameof(members));
            if (random == null) throw new ArgumentNullException(nameof(random));

            if (memberIds.Count == 0)
            {
                return null;
            }

            // Chỉ có một lựa chọn thì không tiêu một lần rút: rút thừa làm chuỗi ngẫu nhiên
            // của cả ván lệch đi, và ván lưu lại (C7) sẽ không khôi phục đúng.
            if (memberIds.Count == 1)
            {
                return memberIds[0];
            }

            if (style == BeastAttackStyle.Random)
            {
                return memberIds[random.NextInt(0, memberIds.Count)];
            }

            bool weakest = style == BeastAttackStyle.HuntWeakest;
            double best = weakest ? double.MaxValue : double.MinValue;
            bool found = false;

            for (int i = 0; i < memberIds.Count; i++)
            {
                Member member = members.Find(memberIds[i]);
                if (member == null)
                {
                    continue;
                }

                double value = weakest ? member.Hp : member.Strength;
                if (!found || (weakest ? value < best : value > best))
                {
                    best = value;
                    found = true;
                }
            }

            // Không tra được ai thì không có cơ sở để so chỉ số; bốc ngẫu nhiên còn hơn im lặng
            // trả về người đầu danh sách, vốn là thiên vị theo id.
            if (!found)
            {
                return memberIds[random.NextInt(0, memberIds.Count)];
            }

            List<int> tied = new List<int>();
            for (int i = 0; i < memberIds.Count; i++)
            {
                Member member = members.Find(memberIds[i]);
                if (member == null)
                {
                    continue;
                }

                double value = weakest ? member.Hp : member.Strength;
                if (value.Equals(best))
                {
                    tied.Add(memberIds[i]);
                }
            }

            return tied.Count == 1 ? tied[0] : tied[random.NextInt(0, tied.Count)];
        }
    }
}
