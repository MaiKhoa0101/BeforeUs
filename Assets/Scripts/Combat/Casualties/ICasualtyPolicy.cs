using BeforeUs.Core;

namespace BeforeUs.Combat
{
    /// <summary>
    /// Đổi một <see cref="BattleReport"/> (ai ngã trong sân) thành <see cref="CombatResult"/> của Core
    /// (ai chết thật, kể cả người ở nhà).
    ///
    /// Tồn tại vì design.md nói hai chuyện ở hai chỗ và chúng không tự khớp khi trận là mô phỏng thật:
    ///   • §5.2 "Thua: mất 2 người" và §8 "quái thú ăn đúng quota người" — tổn thất tính trên CẢ BỘ TỘC,
    ///     nên `CombatRequest.AllMemberIds` mới tồn tại;
    ///   • §9 "người chết trong trận là chết thật, kể cả khi thắng" — nói về trận THẮNG,
    ///     rằng thắng không hồi sinh ai.
    ///
    /// Bản dùng thật là <see cref="DesignTollCasualtyPolicy"/>.
    /// </summary>
    public interface ICasualtyPolicy
    {
        CombatResult Apply(CombatRequest request, BattleReport report);
    }
}
