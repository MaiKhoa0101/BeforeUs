namespace BeforeUs.Combat
{
    /// <summary>Hai kiểu trận của design.md §9.</summary>
    public enum BattleKind
    {
        /// <summary>
        /// Dụ và hạ. Mục tiêu: hạ con thú trước khi nó chạy thoát.
        /// Thua khi thú ra khỏi khu vực. Dùng cho Săn bắt và Cơ hội combat.
        /// </summary>
        Lure,

        /// <summary>
        /// Phòng thủ. Mục tiêu: chặn kẻ địch trước khi tới hang hoặc mục tiêu.
        /// Thua khi kẻ địch chạm mục tiêu. Dùng cho Thú tấn công hang và Khám phá.
        /// </summary>
        Prevent
    }

    /// <summary>Bên nào của sân.</summary>
    public enum CombatSide
    {
        Tribe,
        Beast
    }

    public enum BattleStatus
    {
        /// <summary>Đã dựng xong, đồng hồ chưa chạy. Lớp hiển thị muốn giữ ở đây bao lâu cũng được.</summary>
        Ready,

        Running,

        Won,

        Lost
    }

    /// <summary>
    /// Vì sao trận kết thúc. Tồn tại để B3 báo được phân bố cách kết thúc, không chỉ thắng/thua:
    /// một tỉ lệ <see cref="TimeLimit"/> cao là dấu hiệu mô phỏng có vấn đề, không phải luật có vấn đề.
    /// </summary>
    public enum BattleEndReason
    {
        None,

        /// <summary>Hạ hết thú. Thắng ở cả hai kiểu trận.</summary>
        AllBeastsDown,

        /// <summary>Thú vượt vạch thoát (Dụ và hạ). Thua.</summary>
        BeastEscaped,

        /// <summary>Kẻ địch chạm hang hoặc mục tiêu (Phòng thủ). Thua.</summary>
        TargetReached,

        /// <summary>
        /// Hết giờ. Dụ và hạ: THUA (thú đã thoát). Phòng thủ: THẮNG — design.md §9 chỉ cho Phòng thủ
        /// đúng một điều kiện thua là "kẻ địch chạm mục tiêu", nên chặn được tới hết giờ chính là
        /// đạt mục tiêu "chặn kẻ địch trước khi tới hang".
        /// </summary>
        TimeLimit,

        /// <summary>
        /// Không còn người nào trên sân và cũng không còn ai để thả. Thua ở cả hai kiểu trận.
        /// Ca suy biến: không còn ai chặn thì thú tới đích là chuyện chắc chắn, nên tuyên thua ngay
        /// thay vì để đồng hồ chạy hết rồi tuyên Phòng thủ THẮNG — đó mới là kết quả vô lý.
        /// </summary>
        TribeDown
    }

    /// <summary>
    /// Kiểu tấn công của quái thú (design.md §9). Thứ quyết định AI CHẾT.
    /// Danh sách loài và kiểu tương ứng nằm ở Assets/Data/beasts.json (B2).
    /// </summary>
    public enum BeastAttackStyle
    {
        /// <summary>Săn kẻ yếu: nhắm thành viên hp thấp nhất.</summary>
        HuntWeakest,

        /// <summary>Ngẫu nhiên: bất kỳ ai.</summary>
        Random,

        /// <summary>Thách đấu kẻ mạnh: nhắm thành viên strength cao nhất.</summary>
        ChallengeStrongest
    }

    /// <summary>Kết quả của một lần người chơi thả quân.</summary>
    public enum DeployResult
    {
        Ok,
        UnknownMember,
        AlreadyDeployed,
        OutsideDeployZone,
        BattleNotRunning
    }
}
