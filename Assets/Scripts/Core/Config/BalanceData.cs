using Newtonsoft.Json;

namespace BeforeUs.Core.Config
{
    // Ánh xạ 1-1 với Assets/Data/balance.json.
    // Nạp bằng MissingMemberHandling.Error: thêm khóa mới vào JSON mà quên khai báo ở đây thì nạp hỏng ngay,
    // thay vì âm thầm bỏ qua một con số cân bằng. Required.Always: thiếu khóa cũng hỏng ngay.
    // Setter để private nên sau khi nạp xong, số liệu không ai sửa được nữa.

    public sealed class BalanceData
    {
        [JsonProperty("_comment")] public string Comment { get; private set; }
        [JsonProperty("mode", Required = Required.Always)] public string Mode { get; private set; }
        [JsonProperty("start", Required = Required.Always)] public StartData Start { get; private set; }
        [JsonProperty("member_stats", Required = Required.Always)] public MemberStatsData MemberStats { get; private set; }
        [JsonProperty("win_stage", Required = Required.Always)] public ModeIntData WinStage { get; private set; }
        [JsonProperty("lose_when_crew_at_or_below", Required = Required.Always)] public int LoseWhenCrewAtOrBelow { get; private set; }
        [JsonProperty("energy_max", Required = Required.Always)] public int[] EnergyMax { get; private set; }
        [JsonProperty("upkeep_per_member", Required = Required.Always)] public int[] UpkeepPerMember { get; private set; }
        [JsonProperty("starvation_deaths", Required = Required.Always)] public int StarvationDeaths { get; private set; }
        [JsonProperty("food_carries_over", Required = Required.Always)] public bool FoodCarriesOver { get; private set; }
        [JsonProperty("activities", Required = Required.Always)] public ActivitiesData Activities { get; private set; }
        [JsonProperty("gather", Required = Required.Always)] public GatherData Gather { get; private set; }
        [JsonProperty("combat_odds", Required = Required.Always)] public CombatOddsData CombatOdds { get; private set; }
        [JsonProperty("combat_results", Required = Required.Always)] public CombatResultsData CombatResults { get; private set; }
        [JsonProperty("birth", Required = Required.Always)] public BirthData Birth { get; private set; }
        [JsonProperty("morale", Required = Required.Always)] public MoraleData Morale { get; private set; }
        [JsonProperty("evolution", Required = Required.Always)] public EvolutionData Evolution { get; private set; }
        [JsonProperty("events", Required = Required.Always)] public EventsData Events { get; private set; }
        [JsonProperty("defence_odds", Required = Required.Always)] public DefenceOddsData DefenceOdds { get; private set; }
        [JsonProperty("raid_quota_by_region", Required = Required.Always)] public int[] RaidQuotaByRegion { get; private set; }
        [JsonProperty("climate", Required = Required.Always)] public ClimateData Climate { get; private set; }
        [JsonProperty("difficulty", Required = Required.Always)] public DifficultyData Difficulty { get; private set; }
        [JsonProperty("regions", Required = Required.Always)] public RegionsData Regions { get; private set; }
    }

    public sealed class StartData
    {
        [JsonProperty("crew", Required = Required.Always)] public int Crew { get; private set; }
        [JsonProperty("region", Required = Required.Always)] public int Region { get; private set; }
        [JsonProperty("stage", Required = Required.Always)] public int Stage { get; private set; }
    }

    /// <summary>
    /// Dải chỉ số khi sinh thành viên (design.md §3). Mỗi dải là [cận dưới, cận trên], cận trên tính cả chính nó.
    /// </summary>
    public sealed class MemberStatsData
    {
        [JsonProperty("speed", Required = Required.Always)] public double[] Speed { get; private set; }
        [JsonProperty("strength", Required = Required.Always)] public double[] Strength { get; private set; }
        [JsonProperty("hp", Required = Required.Always)] public int[] Hp { get; private set; }
    }

    /// <summary>Một số tách theo chế độ.</summary>
    public sealed class ModeIntData
    {
        [JsonProperty("full", Required = Required.Always)] public int Full { get; private set; }
        [JsonProperty("mvp", Required = Required.Always)] public int Mvp { get; private set; }

        public int For(GameMode mode)
        {
            return mode == GameMode.Mvp ? Mvp : Full;
        }
    }

    /// <summary>Một mảng số tách theo chế độ.</summary>
    public sealed class ModeIntArrayData
    {
        [JsonProperty("full", Required = Required.Always)] public int[] Full { get; private set; }
        [JsonProperty("mvp", Required = Required.Always)] public int[] Mvp { get; private set; }

        public int[] For(GameMode mode)
        {
            return mode == GameMode.Mvp ? Mvp : Full;
        }
    }

    public sealed class ActivitiesData
    {
        [JsonProperty("gather", Required = Required.Always)] public GatherActivityData Gather { get; private set; }
        [JsonProperty("morale", Required = Required.Always)] public MoraleActivityData Morale { get; private set; }
        [JsonProperty("build", Required = Required.Always)] public BuildActivityData Build { get; private set; }
        [JsonProperty("hunt", Required = Required.Always)] public CombatActivityData Hunt { get; private set; }
        [JsonProperty("explore", Required = Required.Always)] public CombatActivityData Explore { get; private set; }
    }

    public abstract class ActivityData
    {
        [JsonProperty("energy", Required = Required.Always)] public int Energy { get; private set; }
        [JsonProperty("neuron_per_member", Required = Required.Always)] public int NeuronPerMember { get; private set; }
        [JsonProperty("unlock", Required = Required.Always)] public string Unlock { get; private set; }
    }

    public sealed class GatherActivityData : ActivityData
    {
        [JsonProperty("morale_on_success", Required = Required.Always)] public int MoraleOnSuccess { get; private set; }
    }

    public sealed class MoraleActivityData : ActivityData
    {
        [JsonProperty("morale_per_member", Required = Required.Always)] public int MoralePerMember { get; private set; }
    }

    public sealed class BuildActivityData : ActivityData
    {
        [JsonProperty("morale_on_success", Required = Required.Always)] public int MoraleOnSuccess { get; private set; }
    }

    public sealed class CombatActivityData : ActivityData
    {
        [JsonProperty("base_win", Required = Required.Always)] public double BaseWin { get; private set; }
    }

    /// <summary>
    /// Tham số hái lượm v7 (design-v7-draft.md §A). Công thức gather_yield của v6 đã bị xoá:
    /// base, per_region, dry_penalty, dry_min, floor nay nằm trong plants.json dưới dạng
    /// find_chance / nutrition / dry_multiplier theo từng loài.
    /// </summary>
    public sealed class GatherData
    {
        /// <summary>Giữ chỗ cho bếp lửa giai đoạn 4 (nấu chín). Ngoài MVP, chưa dùng.</summary>
        [JsonProperty("fire_bonus_from_stage", Required = Required.Always)] public int FireBonusFromStage { get; private set; }
        [JsonProperty("fire_bonus", Required = Required.Always)] public int FireBonus { get; private set; }

        /// <summary>Sàn bắt buộc đi Kiếm ăn, dạng phân số (D6 của Khoa: ít nhất 1/3 bộ tộc).</summary>
        [JsonProperty("floor_numerator", Required = Required.Always)] public int FloorNumerator { get; private set; }
        [JsonProperty("floor_denominator", Required = Required.Always)] public int FloorDenominator { get; private set; }

        [JsonProperty("discover_base", Required = Required.Always)] public double DiscoverBase { get; private set; }
        [JsonProperty("discover_per_member", Required = Required.Always)] public double DiscoverPerMember { get; private set; }
        [JsonProperty("discover_max", Required = Required.Always)] public double DiscoverMax { get; private set; }

        /// <summary>Hệ số speed tăng cơ hội tìm loài hiếm (D4 của Khoa).</summary>
        [JsonProperty("speed_rare_bonus", Required = Required.Always)] public double SpeedRareBonus { get; private set; }
    }

    public sealed class CombatOddsData
    {
        [JsonProperty("per_member", Required = Required.Always)] public double PerMember { get; private set; }
        [JsonProperty("per_build_level", Required = Required.Always)] public double PerBuildLevel { get; private set; }
        [JsonProperty("min", Required = Required.Always)] public double Min { get; private set; }
        [JsonProperty("max", Required = Required.Always)] public double Max { get; private set; }
    }

    public sealed class CombatResultsData
    {
        [JsonProperty("hunt_food_per_member", Required = Required.Always)] public int[] HuntFoodPerMember { get; private set; }
        [JsonProperty("morale_on_win", Required = Required.Always)] public int MoraleOnWin { get; private set; }
        [JsonProperty("morale_on_loss", Required = Required.Always)] public int MoraleOnLoss { get; private set; }
        [JsonProperty("win_death_chance_avg", Required = Required.Always)] public double WinDeathChanceAvg { get; private set; }
        [JsonProperty("loss_deaths", Required = Required.Always)] public int LossDeaths { get; private set; }
    }

    /// <summary>Tinh thần ngoài phần đã nằm trong từng hoạt động (design.md §4).</summary>
    public sealed class MoraleData
    {
        /// <summary>
        /// Cộng vào tinh thần MỖI KHI mất một thành viên, mọi nguyên nhân: đói, thua combat,
        /// quái thú ăn. Mang sẵn dấu âm, nơi gọi chỉ việc cộng.
        /// Góp ý 1 của Khoa ở playtest 01 (2026-11-08). Giá trị là [ĐỀ XUẤT] của Supervisor.
        /// </summary>
        [JsonProperty("on_member_death", Required = Required.Always)] public int OnMemberDeath { get; private set; }
    }

    public sealed class BirthData
    {
        [JsonProperty("morale_cost", Required = Required.Always)] public int MoraleCost { get; private set; }
        [JsonProperty("max_per_turn", Required = Required.Always)] public int MaxPerTurn { get; private set; }
    }

    public sealed class EvolutionData
    {
        [JsonProperty("required_neuron", Required = Required.Always)] public ModeIntArrayData RequiredNeuron { get; private set; }
        [JsonProperty("feats", Required = Required.Always)] public FeatData[] Feats { get; private set; }
    }

    public sealed class FeatData
    {
        [JsonProperty("from_stage", Required = Required.Always)] public int FromStage { get; private set; }
        [JsonProperty("combat_wins", Required = Required.Always)] public int CombatWins { get; private set; }
        [JsonProperty("hunts", Required = Required.Always)] public int Hunts { get; private set; }
        [JsonProperty("migrated_ever", Required = Required.Always)] public int MigratedEver { get; private set; }
    }

    public sealed class EventsData
    {
        [JsonProperty("grace_turns", Required = Required.Always)] public int GraceTurns { get; private set; }
        [JsonProperty("tutorial_turn", Required = Required.Always)] public int TutorialTurn { get; private set; }
        [JsonProperty("tutorial_reward", Required = Required.Always)] public TutorialRewardData TutorialReward { get; private set; }
        [JsonProperty("chance_per_turn", Required = Required.Always)] public double ChancePerTurn { get; private set; }
        [JsonProperty("share_noncombat", Required = Required.Always)] public double ShareNoncombat { get; private set; }
        [JsonProperty("share_raid", Required = Required.Always)] public ShareRaidData ShareRaid { get; private set; }
        [JsonProperty("noncombat_good", Required = Required.Always)] public FoodMoraleData NoncombatGood { get; private set; }
        [JsonProperty("noncombat_bad", Required = Required.Always)] public FoodMoraleData NoncombatBad { get; private set; }
        [JsonProperty("opportunity_win", Required = Required.Always)] public OpportunityWinData OpportunityWin { get; private set; }
        [JsonProperty("raid_win", Required = Required.Always)] public RaidWinData RaidWin { get; private set; }
    }

    public sealed class TutorialRewardData
    {
        [JsonProperty("morale", Required = Required.Always)] public int Morale { get; private set; }
        [JsonProperty("neuron", Required = Required.Always)] public int Neuron { get; private set; }
        [JsonProperty("combat_wins", Required = Required.Always)] public int CombatWins { get; private set; }
    }

    public sealed class ShareRaidData
    {
        [JsonProperty("wet", Required = Required.Always)] public double Wet { get; private set; }
        [JsonProperty("dry", Required = Required.Always)] public double Dry { get; private set; }
    }

    public sealed class FoodMoraleData
    {
        [JsonProperty("food", Required = Required.Always)] public int Food { get; private set; }
        [JsonProperty("morale", Required = Required.Always)] public int Morale { get; private set; }
    }

    public sealed class OpportunityWinData
    {
        [JsonProperty("food", Required = Required.Always)] public int Food { get; private set; }
        [JsonProperty("morale", Required = Required.Always)] public int Morale { get; private set; }
        [JsonProperty("neuron", Required = Required.Always)] public int Neuron { get; private set; }
    }

    public sealed class RaidWinData
    {
        [JsonProperty("morale", Required = Required.Always)] public int Morale { get; private set; }
        [JsonProperty("neuron", Required = Required.Always)] public int Neuron { get; private set; }
    }

    public sealed class DefenceOddsData
    {
        [JsonProperty("base", Required = Required.Always)] public double Base { get; private set; }
        [JsonProperty("per_member", Required = Required.Always)] public double PerMember { get; private set; }
        [JsonProperty("per_build_level", Required = Required.Always)] public double PerBuildLevel { get; private set; }
        [JsonProperty("per_stage", Required = Required.Always)] public double PerStage { get; private set; }
        [JsonProperty("per_region", Required = Required.Always)] public double PerRegion { get; private set; }
        [JsonProperty("min", Required = Required.Always)] public double Min { get; private set; }
        [JsonProperty("max", Required = Required.Always)] public double Max { get; private set; }
    }

    public sealed class ClimateData
    {
        [JsonProperty("phase_length", Required = Required.Always)] public int PhaseLength { get; private set; }
        [JsonProperty("first_phase", Required = Required.Always)] public string FirstPhase { get; private set; }
    }

    public sealed class DifficultyData
    {
        [JsonProperty("period_turns", Required = Required.Always)] public ModeIntData PeriodTurns { get; private set; }
        [JsonProperty("per_level", Required = Required.Always)] public DifficultyPerLevelData PerLevel { get; private set; }
    }

    public sealed class DifficultyPerLevelData
    {
        [JsonProperty("combat_odds", Required = Required.Always)] public double CombatOdds { get; private set; }
        [JsonProperty("raid_quota", Required = Required.Always)] public int RaidQuota { get; private set; }
        [JsonProperty("bad_event_food", Required = Required.Always)] public int BadEventFood { get; private set; }
        [JsonProperty("bad_event_morale", Required = Required.Always)] public int BadEventMorale { get; private set; }
        /// <summary>
        /// Cộng vào HỆ SỐ NHÂN của mọi tỉ lệ tìm thấy, mỗi bậc độ khó (design-v7 §A.7).
        /// Mang sẵn dấu âm. Thay cho `gather: -1` của v6, vốn trừ thẳng vào gather_yield đã bị xoá.
        /// ⚠ Đây là cơ chế DUY NHẤT kết thúc ván ở lì (design.md §11) — không được bỏ không.
        /// </summary>
        [JsonProperty("find_chance_multiplier", Required = Required.Always)] public double FindChanceMultiplier { get; private set; }
    }

    public sealed class RegionsData
    {
        [JsonProperty("max_index", Required = Required.Always)] public ModeIntData MaxIndex { get; private set; }
        [JsonProperty("names", Required = Required.Always)] public string[] Names { get; private set; }
    }
}
