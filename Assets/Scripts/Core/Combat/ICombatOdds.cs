namespace BeforeUs.Core
{
    /// <summary>
    /// Nguồn ước lượng tỉ lệ thắng theo công thức (design.md §5.2, §8).
    /// Tách khỏi <see cref="ICombatResolver"/> vì hai việc khác nhau: resolver QUYẾT kết quả,
    /// còn cái này chỉ ƯỚC LƯỢNG để người chơi hoặc bot cân nhắc có đánh hay không.
    ///
    /// Khi I1 thay resolver bằng scene combat thật, scene đó không tính được xác suất —
    /// lúc ấy vẫn giữ một ProbabilisticCombatResolver làm nguồn ước lượng. B3 cũng dùng
    /// đúng interface này để đo độ lệch của combat thật so với công thức.
    /// </summary>
    public interface ICombatOdds
    {
        double WinChance(CombatRequest request);
    }
}
