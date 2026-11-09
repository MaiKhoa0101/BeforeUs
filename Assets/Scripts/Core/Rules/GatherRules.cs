using System;
using System.Collections.Generic;
using BeforeUs.Core.Config;
using BeforeUs.Core.Plants;

namespace BeforeUs.Core
{
    /// <summary>Một lần tìm thấy: ai tìm được loài gì, được bao nhiêu thức ăn.</summary>
    public struct PlantFind
    {
        public PlantFind(int memberId, string plantId, int nutrition, bool fromGuarantee)
        {
            MemberId = memberId;
            PlantId = plantId;
            Nutrition = nutrition;
            FromGuarantee = fromGuarantee;
        }

        public int MemberId { get; }
        public string PlantId { get; }
        public int Nutrition { get; }

        /// <summary>Tìm được nhờ khung đảm bảo theo speed, không phải nhờ rút trúng.</summary>
        public bool FromGuarantee { get; }
    }

    /// <summary>Kết quả một lần Kiếm ăn (design-v7-draft.md §A.3).</summary>
    public sealed class GatherOutcome
    {
        public static readonly GatherOutcome Empty = new GatherOutcome();

        internal GatherOutcome()
        {
            Finds = new List<PlantFind>();
        }

        public int Food { get; internal set; }

        /// <summary>Loài vừa khám phá được trong lượt này, null nếu không khám phá được gì.</summary>
        public string DiscoveredPlantId { get; internal set; }

        public List<PlantFind> Finds { get; }
    }

    /// <summary>Dự báo cho UI: sàn chắc chắn và kỳ vọng.</summary>
    public struct GatherForecast
    {
        public GatherForecast(int guaranteed, double expected)
        {
            Guaranteed = guaranteed;
            Expected = expected;
        }

        /// <summary>Thức ăn CHẮC CHẮN có, chỉ từ khung đảm bảo theo speed. Có thể là 0.</summary>
        public int Guaranteed { get; }

        /// <summary>Kỳ vọng toán học. Không phải con số sẽ xảy ra, chỉ là trung bình dài hạn.</summary>
        public double Expected { get; }
    }

    /// <summary>
    /// Hái lượm v7 (design-v7-draft.md §A). Thay hẳn công thức `gather_yield` của design.md §5.1.
    ///
    /// ⚠ KHÁC v6 Ở CHỖ CƠ BẢN NHẤT: thức ăn KHÔNG suy ra được từ số người bằng bất kỳ phép nhân nào.
    /// Nó là tổng của những lần rút trúng, mỗi người rút riêng với mỗi loài đã biết.
    /// `speed` không còn nhân vào lượng thức ăn (D4 của Khoa, playtest 01). Nó làm hai việc:
    ///   - THEO TỪNG NGƯỜI: tăng cơ hội rút trúng loài HIẾM (D4);
    ///   - THEO TỔNG CẢ NHÓM: đạt `guaranteed_total_speed` của một loài thì CHẮC CHẮN tìm thấy
    ///     loài đó ít nhất một lần (E2 của Khoa). Bản đầu cài nhầm thành ngưỡng của từng người.
    /// </summary>
    public static class GatherRules
    {
        /// <summary>
        /// Tỉ lệ một người tìm thấy một loài, sau mọi hệ số (design-v7 §A.3 bước 2).
        /// Công khai để UI hiện được "khả năng tìm thấy" từng loài mà không phải tự tính.
        /// </summary>
        public static double FindChance(
            PlantData plant, Member member, BalanceConfig config, ClimatePhase phase, double difficultyMultiplier)
        {
            if (plant == null) throw new ArgumentNullException(nameof(plant));
            if (member == null) throw new ArgumentNullException(nameof(member));
            if (config == null) throw new ArgumentNullException(nameof(config));

            double p = plant.FindChance
                       * (phase == ClimatePhase.Dry ? plant.DryMultiplier : 1.0)
                       * difficultyMultiplier;

            // D4: speed tăng khả năng tìm thấy loài HIẾM. Càng hiếm (find_chance càng thấp)
            // thì phần cộng càng lớn — nên speed là chỉ số của người đi tìm giỏi,
            // không phải người mang vác khỏe.
            p += config.Data.Gather.SpeedRareBonus * (member.Speed - 1.0) * (1.0 - plant.FindChance);

            return MathUtil.Clamp(p, 0.0, 1.0);
        }

        /// <summary>design-v7 §A.6: số người tối thiểu phải đi Kiếm ăn mỗi lượt.</summary>
        public static int FloorForCrew(BalanceConfig config, int crew, int denominatorOverride = 0)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            if (crew <= 0)
            {
                return 0;
            }

            GatherData g = config.Data.Gather;
            int numerator = g.FloorNumerator;
            int denominator = denominatorOverride > 0 ? denominatorOverride : g.FloorDenominator;

            // ceil(crew × num / den) bằng số nguyên, không qua double.
            return (crew * numerator + denominator - 1) / denominator;
        }

        /// <summary>design-v7 §A.3 bước 1: tỉ lệ khám phá được một loài mới, một lần cho cả nhóm.</summary>
        public static double DiscoverChance(BalanceConfig config, int partySize)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            GatherData g = config.Data.Gather;
            return Math.Min(g.DiscoverMax, g.DiscoverBase + g.DiscoverPerMember * partySize);
        }

        /// <summary>
        /// Giải quyết một lần Kiếm ăn. Thứ tự rút số ngẫu nhiên cố định để ván tái lập được:
        ///   1) một lần rút quyết có khám phá được loài mới không;
        ///   2) nếu có: một lần rút chọn loài;
        ///   3) với mỗi người, với mỗi loài đã biết theo thứ tự trong plants.json: một lần rút.
        /// Khung đảm bảo KHÔNG tiêu lần rút nào.
        /// </summary>
        public static GatherOutcome Resolve(
            BalanceConfig config,
            PlantCatalog catalog,
            GameState state,
            IReadOnlyList<Member> party,
            ClimatePhase phase,
            double difficultyMultiplier,
            IRandom random,
            double speedThresholdRelief = 0.0)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (party == null) throw new ArgumentNullException(nameof(party));
            if (random == null) throw new ArgumentNullException(nameof(random));

            GatherOutcome outcome = new GatherOutcome();
            if (party.Count == 0)
            {
                return outcome;
            }

            // --- bước 1: khám phá, MỘT lần cho cả nhóm ---
            if (random.NextDouble() < DiscoverChance(config, party.Count))
            {
                PlantData discovered = catalog.DrawUndiscovered(state.DiscoveredPlantIds, state.Region, random);
                if (discovered != null)
                {
                    state.DiscoveredPlantIds.Add(discovered.Id);
                    outcome.DiscoveredPlantId = discovered.Id;
                }
            }

            // Loài vừa khám phá tham gia luôn bước 2 của chính lượt này.
            List<PlantData> known = catalog.KnownInRegion(state.DiscoveredPlantIds, state.Region);
            if (known.Count == 0)
            {
                return outcome;
            }

            double partySpeed = TotalSpeed(party, speedThresholdRelief);
            bool[] found = new bool[known.Count];

            // --- bước 2: tìm kiếm, mỗi người mỗi loài một lần rút ---
            for (int i = 0; i < party.Count; i++)
            {
                Member member = party[i];

                for (int p = 0; p < known.Count; p++)
                {
                    PlantData plant = known[p];
                    if (random.NextDouble() < FindChance(plant, member, config, phase, difficultyMultiplier))
                    {
                        outcome.Finds.Add(new PlantFind(member.Id, plant.Id, plant.Nutrition, false));
                        outcome.Food += plant.Nutrition;
                        found[p] = true;
                    }
                }
            }

            // --- bước 3: khung đảm bảo, theo TỔNG nhanh nhẹn của cả nhóm, cho TỪNG loài (E2) ---
            // Không tiêu lần rút nào, nên nó không làm lệch chuỗi ngẫu nhiên.
            for (int p = 0; p < known.Count; p++)
            {
                PlantData plant = known[p];
                if (!found[p] && partySpeed >= plant.GuaranteedTotalSpeed)
                {
                    outcome.Finds.Add(new PlantFind(party[0].Id, plant.Id, plant.Nutrition, true));
                    outcome.Food += plant.Nutrition;
                }
            }

            return outcome;
        }

        /// <summary>
        /// Dự báo cho UI. KHÔNG rút số ngẫu nhiên, không đổi gì — gọi bao nhiêu lần cũng được.
        /// `Guaranteed` là thứ duy nhất chắc chắn có; `Expected` chỉ là trung bình dài hạn.
        /// </summary>
        public static GatherForecast Forecast(
            BalanceConfig config,
            PlantCatalog catalog,
            GameState state,
            IReadOnlyList<Member> party,
            ClimatePhase phase,
            double difficultyMultiplier,
            double speedThresholdRelief = 0.0)
        {
            if (party == null || party.Count == 0)
            {
                return new GatherForecast(0, 0);
            }

            List<PlantData> known = catalog.KnownInRegion(state.DiscoveredPlantIds, state.Region);
            if (known.Count == 0)
            {
                return new GatherForecast(0, 0);
            }

            double partySpeed = TotalSpeed(party, speedThresholdRelief);
            int guaranteed = 0;
            double expected = 0;

            for (int p = 0; p < known.Count; p++)
            {
                PlantData plant = known[p];
                double nobodyFinds = 1.0;
                int certainFinders = 0;

                for (int i = 0; i < party.Count; i++)
                {
                    double chance = FindChance(plant, party[i], config, phase, difficultyMultiplier);
                    expected += chance * plant.Nutrition;
                    nobodyFinds *= 1.0 - chance;

                    if (chance >= 1.0)
                    {
                        certainFinders++;
                    }
                }

                // Sàn chắc chắn có hai nguồn, và lấy cái LỚN HƠN chứ không cộng dồn:
                //   - những người có tỉ lệ đã chạm 1.0 thì chắc chắn tìm thấy, mỗi người một lần;
                //   - khung đảm bảo theo tổng nhanh nhẹn (E2) chỉ bảo đảm MỘT lần cho cả nhóm.
                // Với plants.json hiện tại không loài nào chạm 1.0, nên nhánh đầu nằm im;
                // nó có ở đây để dự báo không nói dối khi Khoa chỉnh số lên.
                int floorFinds = certainFinders;
                if (partySpeed >= plant.GuaranteedTotalSpeed)
                {
                    if (floorFinds == 0)
                    {
                        floorFinds = 1;
                    }

                    expected += nobodyFinds * plant.Nutrition;
                }

                guaranteed += floorFinds * plant.Nutrition;
            }

            return new GatherForecast(guaranteed, expected);
        }

        /// <summary>
        /// Tổng nhanh nhẹn của nhóm, cộng phần "nới ngưỡng" mà Ổ đựng thức ăn cho (E2 của Khoa:
        /// nâng cấp ổ thì cây cần tổng 6 xuống còn 5.8 rồi 5.5). Cài bằng cách CỘNG vào tổng của
        /// nhóm thay vì trừ vào từng ngưỡng — cùng một hiệu lực, nhưng chỉ một chỗ để sai.
        /// </summary>
        private static double TotalSpeed(IReadOnlyList<Member> party, double relief)
        {
            double total = relief;
            for (int i = 0; i < party.Count; i++)
            {
                total += party[i].Speed;
            }

            return total;
        }
    }
}
