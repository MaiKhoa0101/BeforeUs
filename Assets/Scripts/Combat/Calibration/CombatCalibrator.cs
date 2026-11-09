using System;
using System.Collections.Generic;
using BeforeUs.Core;
using BeforeUs.Core.Config;

namespace BeforeUs.Combat.Calibration
{
    /// <summary>
    /// Công cụ hiệu chỉnh combat (HANDOFF.md B3): tự chơi N trận với cách thả ngẫu nhiên, đo tỉ lệ
    /// thắng thật và đặt cạnh con số công thức của Core.
    ///
    /// Đây là nơi DUY NHẤT trong nhánh combat được phép gọi <c>ICombatOdds.WinChance</c> —
    /// <c>CombatSourceDisciplineTests</c> miễn trừ đúng thư mục này và chặn mọi nơi khác.
    /// Và kể cả ở đây, nó chỉ GỌI, không bao giờ tự tính.
    ///
    /// Mỗi trận dựng một bộ tộc MỚI từ <see cref="MemberFactory"/> với đúng dải chỉ số của
    /// balance.json: phương sai giữa các thành viên là một phần thật của trò chơi, không phải
    /// nhiễu cần khử.
    /// </summary>
    public sealed class CombatCalibrator
    {
        private readonly BalanceConfig _config;
        private readonly BeastCatalog _catalog;
        private readonly MemberStatRanges _ranges;

        public CombatCalibrator(BalanceConfig config, BeastCatalog catalog)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _ranges = MemberStatRanges.FromConfig(config);
        }

        /// <summary>
        /// Đo một ô. <paramref name="timeLimitOverride"/> khác 0 thì thay thời lượng trận,
        /// dùng cho bảng độ nhạy 20/30/40 giây.
        /// </summary>
        public CalibrationCell Measure(
            CombatKind kind,
            int partySize,
            int stage,
            int region,
            int buildLevel,
            int difficultyLevel,
            int battles,
            int seed,
            double timeLimitOverride = 0.0,
            BattleOptions optionsOverride = null)
        {
            if (battles <= 0) throw new ArgumentOutOfRangeException(nameof(battles));
            if (partySize <= 0) throw new ArgumentOutOfRangeException(nameof(partySize));

            IRandom random = new XorShift128Random(seed);
            MemberFactory factory = new MemberFactory(_ranges);
            ProbabilisticCombatResolver odds = new ProbabilisticCombatResolver(_config, random);

            CalibrationCell cell = new CalibrationCell
            {
                Kind = kind,
                PartySize = partySize,
                Stage = stage,
                Region = region,
                BuildLevel = buildLevel,
                DifficultyLevel = difficultyLevel,
                Battles = battles,
                TimeLimitSeconds = timeLimitOverride > 0.0
                    ? timeLimitOverride
                    : _catalog.TemplateFor(kind).TimeLimitSeconds
            };

            double secondsTotal = 0.0;
            bool formulaCaptured = false;

            for (int i = 0; i < battles; i++)
            {
                List<Member> tribe = new List<Member>(partySize);
                List<int> ids = new List<int>(partySize);
                for (int m = 0; m < partySize; m++)
                {
                    Member member = factory.Create(random);
                    tribe.Add(member);
                    ids.Add(member.Id);
                }

                CombatRequest request = new CombatRequest(
                    kind, ids, stage, region, buildLevel, difficultyLevel, ids.Count, ids);

                if (!formulaCaptured)
                {
                    // Con số của Core, không phải của tôi.
                    cell.FormulaWinChance = odds.WinChance(request);
                    formulaCaptured = true;
                }

                MemberList members = new MemberList(tribe);
                BattleSetupFactory setups = new BattleSetupFactory(_catalog, members);
                DesignTollCasualtyPolicy casualties =
                    new DesignTollCasualtyPolicy(_config, odds, members, random);

                BattleSetup setup = Reshape(setups.Build(request, random), timeLimitOverride, optionsOverride);

                BattleSimulation battle = new BattleSimulation(setup, random);
                RandomDeploymentAutoPlayer player = new RandomDeploymentAutoPlayer(random);

                battle.Start();
                int maxSteps = (int)Math.Ceiling(setup.TimeLimitSeconds / BattleSimulation.FixedStepSeconds) + 2;
                int steps = 0;
                while (!battle.IsFinished && steps < maxSteps)
                {
                    player.OnStep(battle);
                    battle.StepFixed();
                    steps++;
                }

                BattleReport report = battle.Report;
                CombatResult result = casualties.Apply(request, report);

                Record(cell, report, result, request, odds);
                secondsTotal += report.ElapsedSeconds;
            }

            cell.AverageSeconds = secondsTotal / battles;
            return cell;
        }

        private static void Record(
            CalibrationCell cell,
            BattleReport report,
            CombatResult result,
            CombatRequest request,
            ProbabilisticCombatResolver odds)
        {
            if (result.Won)
            {
                cell.Wins++;
                if (result.DeadMemberIds.Count > 0)
                {
                    cell.WinsWithDeaths++;
                }
            }

            cell.TotalDeaths += result.DeadMemberIds.Count;

            switch (report.Reason)
            {
                case BattleEndReason.AllBeastsDown: cell.EndedAllBeastsDown++; break;
                case BattleEndReason.BeastEscaped: cell.EndedBeastEscaped++; break;
                case BattleEndReason.TargetReached: cell.EndedTargetReached++; break;
                case BattleEndReason.TimeLimit: cell.EndedTimeLimit++; break;
                case BattleEndReason.TribeDown: cell.EndedTribeDown++; break;
            }

            // Kẹp D11 có phải ra tay không: so số người lẽ ra phải chết với số thật sự chết.
            if (!result.Won && request.Kind == CombatKind.Defence)
            {
                int wanted = Math.Max(report.DeadMemberIds.Count, odds.RaidQuota(request));
                if (result.DeadMemberIds.Count < Math.Min(wanted, request.AllMemberIds.Count))
                {
                    cell.LastSurvivorFloorHits++;
                }
            }
        }

        /// <summary>
        /// Dựng lại cùng một trận với thời lượng hoặc đòn bẩy khác, giữ nguyên mọi thứ còn lại.
        /// Dùng cho bảng độ nhạy và bảng đòn bẩy.
        /// </summary>
        private static BattleSetup Reshape(
            BattleSetup setup, double timeLimitOverride, BattleOptions optionsOverride)
        {
            if (timeLimitOverride <= 0.0 && optionsOverride == null)
            {
                return setup;
            }

            return new BattleSetup(
                setup.Kind,
                setup.Field,
                setup.Tribe,
                setup.Beasts,
                timeLimitOverride > 0.0 ? timeLimitOverride : setup.TimeLimitSeconds,
                optionsOverride ?? setup.Options);
        }
    }
}
