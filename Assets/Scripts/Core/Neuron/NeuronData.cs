using BeforeUs.Core.Config;
using Newtonsoft.Json;

namespace BeforeUs.Core.Neuron
{
    /// <summary>Loại nút (design.md §7): mỗi nhánh có 1 gốc → 1 cổng (bắt buộc) + 1 tùy chọn nối từ gốc.</summary>
    public enum NodeType
    {
        Root,

        /// <summary>Nút cổng. Phải đủ cả 3 cổng của giai đoạn mới mở được nút Tiến hóa.</summary>
        Gate,

        Optional,

        /// <summary>Nút Tiến hóa: mở được thì lên giai đoạn kế.</summary>
        Evolve
    }

    /// <summary>Tên các loại hiệu ứng hợp lệ trong neurons.json. Gặp tên lạ thì nạp hỏng ngay.</summary>
    public static class NodeEffectKind
    {
        public const string GatherFlatPerMember = "gather_flat_per_member";
        public const string GatherYieldBonus = "gather_yield_bonus";
        public const string GatherYieldBonusWet = "gather_yield_bonus_wet";
        public const string DryPenaltyReduction = "dry_penalty_reduction";
        public const string SpeedBonus = "speed_bonus";
        public const string StrengthBonus = "strength_bonus";
        public const string MoraleActivityBonus = "morale_activity_bonus";
        public const string BirthMoraleCost = "birth_morale_cost";
        public const string StarvationForgiveOne = "starvation_forgive_one";
        public const string NewbornStatBonus = "newborn_stat_bonus";
        public const string DefenceOddsBonus = "defence_odds_bonus";
        public const string ExploreOddsBonus = "explore_odds_bonus";
        public const string BadEventFoodReduction = "bad_event_food_reduction";
        public const string RaidWarning = "raid_warning";
        public const string UnlockActivity = "unlock_activity";
    }

    // Ánh xạ 1-1 với Assets/Data/neurons.json. Cùng kỷ luật như BalanceData:
    // MissingMemberHandling.Error + Required.Always, setter private.

    public sealed class NeuronData
    {
        [JsonProperty("_comment")] public string Comment { get; private set; }
        [JsonProperty("stages", Required = Required.Always)] public NeuronStageData[] Stages { get; private set; }
    }

    public sealed class NeuronStageData
    {
        [JsonProperty("stage", Required = Required.Always)] public int Stage { get; private set; }
        [JsonProperty("name", Required = Required.Always)] public string Name { get; private set; }
        [JsonProperty("branches", Required = Required.Always)] public NeuronBranchData[] Branches { get; private set; }
        [JsonProperty("nodes", Required = Required.Always)] public NeuronNodeData[] Nodes { get; private set; }
    }

    public sealed class NeuronBranchData
    {
        [JsonProperty("id", Required = Required.Always)] public string Id { get; private set; }
        [JsonProperty("name", Required = Required.Always)] public string Name { get; private set; }
    }

    public sealed class NeuronNodeData
    {
        [JsonProperty("id", Required = Required.Always)] public string Id { get; private set; }

        /// <summary>null với nút Tiến hóa: nó không thuộc nhánh nào.</summary>
        [JsonProperty("branch")] public string Branch { get; private set; }

        [JsonProperty("type", Required = Required.Always)] public string Type { get; private set; }
        [JsonProperty("name", Required = Required.Always)] public string Name { get; private set; }

        /// <summary>Nút phải mua trước. null nếu không cần.</summary>
        [JsonProperty("requires")] public string Requires { get; private set; }

        [JsonProperty("cost", Required = Required.Always)] public ModeIntData Cost { get; private set; }

        /// <summary>Mô tả hiệu ứng, nguyên văn design.md §7.3–7.4.</summary>
        [JsonProperty("effect_text", Required = Required.Always)] public string EffectText { get; private set; }

        [JsonProperty("effects", Required = Required.Always)] public NodeEffectData[] Effects { get; private set; }
    }

    public sealed class NodeEffectData
    {
        [JsonProperty("kind", Required = Required.Always)] public string Kind { get; private set; }
        [JsonProperty("value")] public double Value { get; private set; }

        /// <summary>Chỉ dùng với kind = unlock_activity.</summary>
        [JsonProperty("activity")] public string Activity { get; private set; }
    }
}
