namespace BeforeUs.Core
{
    /// <summary>
    /// Nguồn ngẫu nhiên duy nhất của Core (AGENTS.md). Có seed và lưu/khôi phục được trạng thái,
    /// để test lặp lại được và để lưu ván đúng tuyệt đối (C7).
    /// </summary>
    public interface IRandom
    {
        /// <summary>Số thực trong [0, 1).</summary>
        double NextDouble();

        /// <summary>Số nguyên trong [minInclusive, maxExclusive).</summary>
        int NextInt(int minInclusive, int maxExclusive);

        /// <summary>Chuỗi mô tả trọn vẹn trạng thái hiện tại. Đưa lại cho <see cref="LoadState"/> là tiếp đúng chỗ cũ.</summary>
        string SaveState();

        void LoadState(string state);
    }
}
