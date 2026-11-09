using System.Collections.Generic;

namespace BeforeUs.Core
{
    /// <summary>
    /// Tổng hiệu ứng của các nút đã mua (design.md §7.3–7.4).
    /// Mọi số ở đây là [ĐỀ XUẤT] trong design.md và mô phỏng tham chiếu KHÔNG tính chúng,
    /// nên Cổng 2 chạy hai lần: applyNodeEffects = false để so với mô phỏng, rồi = true để báo số mới.
    ///
    /// Khi tắt cờ, dùng <see cref="None"/>: mọi trường bằng 0 / false, nên các luật gọi vào đây
    /// không cần biết cờ bật hay tắt.
    /// </summary>
    public sealed class NodeEffects
    {
        /// <summary>Không hiệu ứng nào. Dùng khi applyNodeEffects = false.</summary>
        public static readonly NodeEffects None = new NodeEffects();

        /// <summary>"Leo trèo khéo": +0.5 thức ăn mỗi người, làm tròn xuống trên TỔNG.</summary>
        public double GatherFlatPerMember { get; internal set; }

        /// <summary>"Đào rễ củ": gather_yield +1, mọi pha.</summary>
        public int GatherYieldBonus { get; internal set; }

        /// <summary>"Gậy đào": gather_yield +1, chỉ pha Ẩm.</summary>
        public int GatherYieldBonusWet { get; internal set; }

        /// <summary>"Ăn côn trùng, trứng": pha Khô trừ hái lượm ít hơn 1.</summary>
        public int DryPenaltyReduction { get; internal set; }

        /// <summary>"Đứng thẳng ngắn", "Đi bằng hai chân": speed mọi thành viên +0.05 mỗi nút.</summary>
        public double SpeedBonus { get; internal set; }

        /// <summary>"Hàm khỏe", "Ném đá": strength mọi thành viên +0.1 mỗi nút.</summary>
        public double StrengthBonus { get; internal set; }

        /// <summary>"Chải lông cho nhau": Nâng tinh thần +1 mỗi lần làm (một lần, không theo đầu người).</summary>
        public int MoraleActivityBonus { get; internal set; }

        /// <summary>"Cõng con trên lưng": sinh cần 3 tinh thần thay vì 4. 0 nghĩa là dùng số trong balance.json.</summary>
        public int BirthMoraleCostOverride { get; internal set; }

        /// <summary>"Chia sẻ thức ăn": cuối lượt thiếu đúng 1 phần ăn thì không mất người.</summary>
        public bool StarvationForgiveOne { get; internal set; }

        /// <summary>"Dạy trẻ": thành viên mới sinh có speed, strength +0.05.</summary>
        public double NewbornStatBonus { get; internal set; }

        /// <summary>"Tiếng gọi báo động": tỉ lệ thắng phòng thủ +0.05.</summary>
        public double DefenceOddsBonus { get; internal set; }

        /// <summary>"Bàn chân vòm": tỉ lệ thắng Khám phá +0.05.</summary>
        public double ExploreOddsBonus { get; internal set; }

        /// <summary>
        /// "Nhai lá non": sự kiện "mất mùa trái cây" mất ít hơn 1 thức ăn.
        /// CHƯA ÁP DỤNG: sự kiện có tên cụ thể chỉ xuất hiện ở events.json (I2), ngoài phạm vi MVP Core.
        /// </summary>
        public int BadEventFoodReduction { get; internal set; }

        /// <summary>
        /// "Canh gác luân phiên": báo trước thú tấn công hang 1 lượt.
        /// CHƯA ÁP DỤNG trong Core: đây là thông tin cho UI, không đổi con số nào.
        /// </summary>
        public bool RaidWarning { get; internal set; }

        /// <summary>
        /// Những hiệu ứng đã mua nhưng Core chưa áp dụng được. Dùng để báo cáo ở C8,
        /// để không ai tưởng mọi hiệu ứng đều đã vào số liệu.
        /// </summary>
        public IReadOnlyList<string> NotYetApplied
        {
            get
            {
                List<string> pending = new List<string>();
                if (BadEventFoodReduction > 0)
                {
                    pending.Add(Neuron.NodeEffectKind.BadEventFoodReduction + " (cần events.json ở I2)");
                }

                if (RaidWarning)
                {
                    pending.Add(Neuron.NodeEffectKind.RaidWarning + " (chỉ là thông tin cho UI)");
                }

                // v7 đã xoá công thức gather_yield, nên bốn hiệu ứng dưới đây mất chỗ bám.
                // Chúng VẪN được nạp và cộng dồn để không âm thầm biến mất khỏi dữ liệu,
                // nhưng KHÔNG còn tác dụng nào. Chờ Khoa thiết kế lại theo thang của v7
                // (find_chance, discover_chance, dry_multiplier). Xem design-v7-draft.md §C.4.
                if (GatherFlatPerMember > 0)
                {
                    pending.Add(Neuron.NodeEffectKind.GatherFlatPerMember + " (v7 đã xoá công thức gather_yield)");
                }

                if (GatherYieldBonus > 0)
                {
                    pending.Add(Neuron.NodeEffectKind.GatherYieldBonus + " (v7 đã xoá công thức gather_yield)");
                }

                if (GatherYieldBonusWet > 0)
                {
                    pending.Add(Neuron.NodeEffectKind.GatherYieldBonusWet + " (v7 đã xoá công thức gather_yield)");
                }

                if (DryPenaltyReduction > 0)
                {
                    pending.Add(Neuron.NodeEffectKind.DryPenaltyReduction + " (v7 thay dry_penalty bằng dry_multiplier theo loài)");
                }

                return pending;
            }
        }

        /// <summary>Chi phí sinh thành viên sau hiệu ứng (design.md §6, §7.3).</summary>
        public int BirthMoraleCost(int fromBalance)
        {
            return BirthMoraleCostOverride > 0 ? BirthMoraleCostOverride : fromBalance;
        }
    }
}
