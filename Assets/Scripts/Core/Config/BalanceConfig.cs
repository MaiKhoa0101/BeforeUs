using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BeforeUs.Core.Config
{
    /// <summary>
    /// Số liệu cân bằng đã nạp và đã chốt chế độ. Mọi con số luật chơi đi qua đây;
    /// không chỗ nào trong Core được viết cứng số (AGENTS.md).
    /// </summary>
    public sealed class BalanceConfig
    {
        private readonly Dictionary<ActivityKind, UnlockRule> _unlocks;

        private BalanceConfig(BalanceData data, GameMode mode)
        {
            Data = data;
            Mode = mode;

            WinStage = data.WinStage.For(mode);
            RequiredNeuron = Array.AsReadOnly(data.Evolution.RequiredNeuron.For(mode));
            DifficultyPeriodTurns = data.Difficulty.PeriodTurns.For(mode);
            RegionMaxIndex = data.Regions.MaxIndex.For(mode);

            _unlocks = new Dictionary<ActivityKind, UnlockRule>
            {
                { ActivityKind.Gather, UnlockRule.Parse(data.Activities.Gather.Unlock) },
                { ActivityKind.Morale, UnlockRule.Parse(data.Activities.Morale.Unlock) },
                { ActivityKind.Build, UnlockRule.Parse(data.Activities.Build.Unlock) },
                { ActivityKind.Hunt, UnlockRule.Parse(data.Activities.Hunt.Unlock) },
                { ActivityKind.Explore, UnlockRule.Parse(data.Activities.Explore.Unlock) }
            };
        }

        /// <summary>Số liệu thô, đúng hình dạng balance.json.</summary>
        public BalanceData Data { get; }

        public GameMode Mode { get; }

        /// <summary>Giai đoạn thắng ván: 5 ở bản đầy đủ, 3 ở MVP (design.md §1).</summary>
        public int WinStage { get; }

        /// <summary>Tổng neuron bắt buộc để tiến hóa khỏi giai đoạn 1..4 (design.md §7.2).</summary>
        public IReadOnlyList<int> RequiredNeuron { get; }

        /// <summary>Số lượt cho mỗi bậc độ khó (design.md §11).</summary>
        public int DifficultyPeriodTurns { get; }

        /// <summary>Chỉ số vùng lớn nhất: 5 ở bản đầy đủ, 1 ở MVP (design.md §12).</summary>
        public int RegionMaxIndex { get; }

        /// <summary>
        /// Thua khi số thành viên còn lại <= số này (design.md §1).
        /// Góp ý 8 của Khoa ở playtest 01: "khi cộng đồng chỉ còn 1 người thì kết thúc game".
        /// </summary>
        public int LoseWhenCrewAtOrBelow
        {
            get { return Data.LoseWhenCrewAtOrBelow; }
        }

        /// <summary>
        /// Số người tối thiểu một trận BẮT BUỘC (Phòng thủ) phải chừa lại, suy ra từ ngưỡng thua.
        /// D11: một trận không né được không bao giờ được thành cú thua tức thì — nên phải chừa lại
        /// nhiều hơn ngưỡng thua đúng một người. Tính ra chứ không viết cứng: đổi ngưỡng thua thì
        /// con số này tự theo.
        /// </summary>
        public int MinSurvivorsAfterForcedCombat
        {
            get { return LoseWhenCrewAtOrBelow + 1; }
        }

        public UnlockRule UnlockOf(ActivityKind activity)
        {
            return _unlocks[activity];
        }

        /// <summary>Năng lượng của một hoạt động (design.md §5). Trả đúng một lần mỗi lượt, không nhân theo số người (D2).</summary>
        public int EnergyCostOf(ActivityKind activity)
        {
            return ActivityOf(activity).Energy;
        }

        /// <summary>Neuron mỗi người tham gia (design.md §5). Nhận cả khi thất bại.</summary>
        public int NeuronPerMemberOf(ActivityKind activity)
        {
            return ActivityOf(activity).NeuronPerMember;
        }

        public ActivityData ActivityOf(ActivityKind activity)
        {
            switch (activity)
            {
                case ActivityKind.Gather: return Data.Activities.Gather;
                case ActivityKind.Morale: return Data.Activities.Morale;
                case ActivityKind.Build: return Data.Activities.Build;
                case ActivityKind.Hunt: return Data.Activities.Hunt;
                case ActivityKind.Explore: return Data.Activities.Explore;
                default: throw new ArgumentOutOfRangeException(nameof(activity));
            }
        }

        /// <summary>Năng lượng tối đa của giai đoạn (design.md §4). Giai đoạn đếm từ 1.</summary>
        public int EnergyMaxForStage(int stage)
        {
            return ByStage(Data.EnergyMax, stage, "energy_max");
        }

        /// <summary>Khẩu phần mỗi thành viên mỗi lượt (design.md §4).</summary>
        public int UpkeepForStage(int stage)
        {
            return ByStage(Data.UpkeepPerMember, stage, "upkeep_per_member");
        }

        /// <summary>Thức ăn mỗi người khi săn thắng (design.md §5.2).</summary>
        public int HuntFoodForStage(int stage)
        {
            return ByStage(Data.CombatResults.HuntFoodPerMember, stage, "combat_results.hunt_food_per_member");
        }

        /// <summary>Tổng neuron bắt buộc để rời giai đoạn này (design.md §7.2).</summary>
        public int RequiredNeuronForStage(int stage)
        {
            if (stage < 1 || stage > RequiredNeuron.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(stage), "evolution.required_neuron không có giai đoạn " + stage);
            }

            return RequiredNeuron[stage - 1];
        }

        /// <summary>Chiến tích cần để rời giai đoạn này (design.md §7.1).</summary>
        public FeatData FeatsForStage(int stage)
        {
            FeatData[] feats = Data.Evolution.Feats;
            for (int i = 0; i < feats.Length; i++)
            {
                if (feats[i].FromStage == stage)
                {
                    return feats[i];
                }
            }

            throw new ArgumentOutOfRangeException(nameof(stage), "evolution.feats không có from_stage = " + stage);
        }

        /// <summary>Số người quái thú ăn khi phòng thủ thua, trước khi cộng độ khó (design.md §8).</summary>
        public int RaidQuotaForRegion(int region)
        {
            int[] quota = Data.RaidQuotaByRegion;
            if (region < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(region));
            }

            // design.md §8: quota = [...][region]; vùng vượt bảng thì dùng giá trị cuối.
            return region < quota.Length ? quota[region] : quota[quota.Length - 1];
        }

        private static int ByStage(int[] values, int stage, string field)
        {
            if (stage < 1 || stage > values.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(stage), field + " không có giai đoạn " + stage);
            }

            return values[stage - 1];
        }

        /// <summary>Nạp theo chế độ ghi trong chính JSON (trường "mode").</summary>
        public static BalanceConfig FromJson(string json)
        {
            BalanceData data = Deserialize(json);
            return new BalanceConfig(data, GameModeParser.Parse(data.Mode));
        }

        /// <summary>Nạp và ép chế độ, bỏ qua trường "mode" trong JSON.</summary>
        public static BalanceConfig FromJson(string json, GameMode mode)
        {
            return new BalanceConfig(Deserialize(json), mode);
        }

        private static BalanceData Deserialize(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new BalanceConfigException("balance.json rỗng");
            }

            BalanceData data;
            try
            {
                data = JsonConvert.DeserializeObject<BalanceData>(json, new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Error,
                    NullValueHandling = NullValueHandling.Include
                });
            }
            catch (JsonException ex)
            {
                throw new BalanceConfigException("balance.json không đọc được: " + ex.Message, ex);
            }

            if (data == null)
            {
                throw new BalanceConfigException("balance.json không đọc được: kết quả rỗng");
            }

            Validate(data);
            return data;
        }

        private static void Validate(BalanceData data)
        {
            RequireRange(data.MemberStats.Speed, "member_stats.speed");
            RequireRange(data.MemberStats.Strength, "member_stats.strength");
            if (data.MemberStats.Hp.Length != 2 || data.MemberStats.Hp[0] > data.MemberStats.Hp[1])
            {
                throw new BalanceConfigException("member_stats.hp phải là [cận dưới, cận trên] với cận dưới <= cận trên");
            }

            if (data.MemberStats.Hp[0] < 1)
            {
                throw new BalanceConfigException("member_stats.hp cận dưới phải >= 1");
            }

            RequireLength(data.EnergyMax, 4, "energy_max");
            RequireLength(data.UpkeepPerMember, 4, "upkeep_per_member");
            RequireLength(data.CombatResults.HuntFoodPerMember, 4, "combat_results.hunt_food_per_member");
            RequireLength(data.Evolution.RequiredNeuron.Full, 4, "evolution.required_neuron.full");
            RequireLength(data.Evolution.RequiredNeuron.Mvp, 4, "evolution.required_neuron.mvp");

            if (data.Evolution.Feats.Length != 4)
            {
                throw new BalanceConfigException("evolution.feats phải có 4 mục, có " + data.Evolution.Feats.Length);
            }

            for (int stage = 1; stage <= 4; stage++)
            {
                bool found = false;
                for (int i = 0; i < data.Evolution.Feats.Length; i++)
                {
                    if (data.Evolution.Feats[i].FromStage == stage)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    throw new BalanceConfigException("evolution.feats thiếu from_stage = " + stage);
                }
            }

            if (data.RaidQuotaByRegion.Length == 0)
            {
                throw new BalanceConfigException("raid_quota_by_region rỗng");
            }

            if (data.Gather.FloorNumerator < 0 || data.Gather.FloorDenominator < 1
                || data.Gather.FloorNumerator > data.Gather.FloorDenominator)
            {
                throw new BalanceConfigException(
                    "gather.floor_numerator / floor_denominator phải là một phân số hợp lệ trong [0, 1]");
            }

            if (data.Climate.PhaseLength < 1)
            {
                throw new BalanceConfigException("climate.phase_length phải >= 1");
            }

            if (data.Climate.FirstPhase != "wet" && data.Climate.FirstPhase != "dry")
            {
                throw new BalanceConfigException("climate.first_phase phải là \"wet\" hoặc \"dry\", nhận được: " + data.Climate.FirstPhase);
            }

            if (data.Difficulty.PeriodTurns.Full < 1 || data.Difficulty.PeriodTurns.Mvp < 1)
            {
                throw new BalanceConfigException("difficulty.period_turns phải >= 1");
            }

            // design.md §4: thức ăn không để dành. Nếu ai đó bật lên, luật cuối lượt của C4 sẽ sai.
            if (data.FoodCarriesOver)
            {
                throw new BalanceConfigException("food_carries_over = true chưa có luật tương ứng trong Core (design.md §4 nói không để dành)");
            }
        }

        private static void RequireRange(double[] values, string field)
        {
            if (values.Length != 2 || values[0] > values[1])
            {
                throw new BalanceConfigException(field + " phải là [cận dưới, cận trên] với cận dưới <= cận trên");
            }
        }

        private static void RequireLength(int[] values, int expected, string field)
        {
            if (values.Length != expected)
            {
                throw new BalanceConfigException(field + " phải có " + expected + " phần tử, có " + values.Length);
            }
        }
    }
}
