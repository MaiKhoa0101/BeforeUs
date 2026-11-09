using Newtonsoft.Json;

namespace BeforeUs.Combat
{
    /// <summary>Tên kiểu tấn công hợp lệ trong beasts.json. Gặp tên lạ thì nạp hỏng ngay.</summary>
    public static class BeastAttackStyleName
    {
        public const string HuntWeakest = "hunt_weakest";
        public const string Random = "random";
        public const string ChallengeStrongest = "challenge_strongest";
    }

    /// <summary>Tên kiểu trận hợp lệ trong beasts.json (design.md §9).</summary>
    public static class BattleKindName
    {
        public const string Lure = "lure";
        public const string Prevent = "prevent";
    }

    /// <summary>Tên vai trò hợp lệ: khớp một-một với CombatKind của Core, trừ Tutorial.</summary>
    public static class BeastRoleName
    {
        public const string Hunt = "hunt";
        public const string Explore = "explore";
        public const string Defence = "defence";
        public const string Opportunity = "opportunity";
    }

    // Ánh xạ 1-1 với Assets/Data/beasts.json. Cùng kỷ luật như BalanceData và NeuronData của Core:
    // MissingMemberHandling.Error + Required.Always, setter private. Thừa một trường hay thiếu một
    // trường đều nổ lúc nạp, chứ không im lặng chạy bằng số 0.

    public sealed class BeastData
    {
        [JsonProperty("_comment")] public string Comment { get; private set; }
        [JsonProperty("tribe_in_battle", Required = Required.Always)] public TribeInBattleData TribeInBattle { get; private set; }
        [JsonProperty("scaling", Required = Required.Always)] public ScalingData Scaling { get; private set; }
        [JsonProperty("levers", Required = Required.Always)] public LeversData Levers { get; private set; }
        [JsonProperty("battles", Required = Required.Always)] public BattlesData Battles { get; private set; }
        [JsonProperty("species", Required = Required.Always)] public SpeciesData[] Species { get; private set; }
    }

    public sealed class TribeInBattleData
    {
        [JsonProperty("_comment")] public string Comment { get; private set; }
        [JsonProperty("move_speed", Required = Required.Always)] public double MoveSpeed { get; private set; }
        [JsonProperty("attack_damage", Required = Required.Always)] public double AttackDamage { get; private set; }
        [JsonProperty("attack_interval", Required = Required.Always)] public double AttackInterval { get; private set; }
        [JsonProperty("attack_range", Required = Required.Always)] public double AttackRange { get; private set; }
    }

    public sealed class ScalingData
    {
        [JsonProperty("_comment")] public string Comment { get; private set; }
        [JsonProperty("beast_hp_per_difficulty_level", Required = Required.Always)] public double BeastHpPerDifficultyLevel { get; private set; }
        [JsonProperty("beast_damage_per_difficulty_level", Required = Required.Always)] public double BeastDamagePerDifficultyLevel { get; private set; }
        [JsonProperty("beast_hp_per_region", Required = Required.Always)] public double BeastHpPerRegion { get; private set; }
        [JsonProperty("tribe_damage_per_build_level", Required = Required.Always)] public double TribeDamagePerBuildLevel { get; private set; }
        [JsonProperty("tribe_damage_per_stage", Required = Required.Always)] public double TribeDamagePerStage { get; private set; }
    }

    /// <summary>
    /// ⚠ ĐÒN BẨY THỬ NGHIỆM, MẶC ĐỊNH TẮT (mọi giá trị 0). Xem <see cref="BattleOptions"/>.
    /// Chúng tồn tại để B3 ĐO xem làm thoải được đường cong theo k tới đâu và phải trả giá gì.
    /// Cả hai đều chạm vào cảm giác trận đánh, nên chỉ Khoa mới được bật ở B4.
    /// </summary>
    public sealed class LeversData
    {
        [JsonProperty("_comment")] public string Comment { get; private set; }

        /// <summary>(A) 0 = không giới hạn.</summary>
        [JsonProperty("max_attackers_per_beast", Required = Required.Always)] public int MaxAttackersPerBeast { get; private set; }

        /// <summary>(B) Biên độ bốc ngẫu nhiên máu và sát thương thú mỗi trận. 0 = tắt. 0.3 = ±30%.</summary>
        [JsonProperty("beast_stat_variance", Required = Required.Always)] public double BeastStatVariance { get; private set; }

        /// <summary>(B) Biên độ xê dịch vị trí xuất phát của thú theo trục x. 0 = tắt.</summary>
        [JsonProperty("beast_spawn_x_jitter", Required = Required.Always)] public double BeastSpawnXJitter { get; private set; }
    }

    public sealed class BattlesData
    {
        [JsonProperty("_comment")] public string Comment { get; private set; }
        [JsonProperty("hunt", Required = Required.Always)] public BattleTemplateData Hunt { get; private set; }
        [JsonProperty("explore", Required = Required.Always)] public BattleTemplateData Explore { get; private set; }
        [JsonProperty("defence", Required = Required.Always)] public BattleTemplateData Defence { get; private set; }
        [JsonProperty("opportunity", Required = Required.Always)] public BattleTemplateData Opportunity { get; private set; }
    }

    public sealed class BattleTemplateData
    {
        [JsonProperty("battle_kind", Required = Required.Always)] public string BattleKind { get; private set; }
        [JsonProperty("time_limit_seconds", Required = Required.Always)] public double TimeLimitSeconds { get; private set; }
        [JsonProperty("field", Required = Required.Always)] public FieldData Field { get; private set; }
        [JsonProperty("spawn_x", Required = Required.Always)] public double SpawnX { get; private set; }

        /// <summary>Bầy sinh ra trải trên trục y quanh giữa sân, rộng chừng này.</summary>
        [JsonProperty("spawn_spread_y", Required = Required.Always)] public double SpawnSpreadY { get; private set; }
    }

    public sealed class FieldData
    {
        [JsonProperty("width", Required = Required.Always)] public double Width { get; private set; }
        [JsonProperty("height", Required = Required.Always)] public double Height { get; private set; }
        [JsonProperty("goal_x", Required = Required.Always)] public double GoalX { get; private set; }
        [JsonProperty("goal_radius", Required = Required.Always)] public double GoalRadius { get; private set; }
        [JsonProperty("deploy_min_x", Required = Required.Always)] public double DeployMinX { get; private set; }
        [JsonProperty("deploy_max_x", Required = Required.Always)] public double DeployMaxX { get; private set; }
    }

    public sealed class SpeciesData
    {
        [JsonProperty("id", Required = Required.Always)] public string Id { get; private set; }

        /// <summary>Chữ hiện cho người chơi.</summary>
        [JsonProperty("name", Required = Required.Always)] public string Name { get; private set; }

        /// <summary>Chữ hiện cho người chơi.</summary>
        [JsonProperty("description", Required = Required.Always)] public string Description { get; private set; }

        [JsonProperty("attack_style", Required = Required.Always)] public string AttackStyle { get; private set; }

        /// <summary>
        /// Chữ hiện cho người chơi, dùng ở gợi ý design.md §13 "lần đầu gặp một loài".
        /// §9: người chơi chỉ biết kiểu của một loài sau lần đầu gặp nó.
        /// </summary>
        [JsonProperty("attack_style_text", Required = Required.Always)] public string AttackStyleText { get; private set; }

        [JsonProperty("regions", Required = Required.Always)] public int[] Regions { get; private set; }
        [JsonProperty("roles", Required = Required.Always)] public string[] Roles { get; private set; }
        [JsonProperty("pack_size", Required = Required.Always)] public int PackSize { get; private set; }
        [JsonProperty("stats", Required = Required.Always)] public BeastStatsData Stats { get; private set; }
    }

    public sealed class BeastStatsData
    {
        [JsonProperty("hp", Required = Required.Always)] public double Hp { get; private set; }
        [JsonProperty("move_speed", Required = Required.Always)] public double MoveSpeed { get; private set; }
        [JsonProperty("attack_damage", Required = Required.Always)] public double AttackDamage { get; private set; }
        [JsonProperty("attack_interval", Required = Required.Always)] public double AttackInterval { get; private set; }
        [JsonProperty("attack_range", Required = Required.Always)] public double AttackRange { get; private set; }
    }
}
