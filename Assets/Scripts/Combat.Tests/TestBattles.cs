using System;
using System.Collections.Generic;
using System.IO;
using BeforeUs.Core;
using BeforeUs.Core.Config;

namespace BeforeUs.Combat.Tests
{
    /// <summary>
    /// Dựng sẵn sân và đơn vị cho test. Mọi con số ở đây là DỮ LIỆU TEST, không phải hằng số sản phẩm:
    /// chỉ số thật của bộ tộc và của thú sống trong Assets/Data/beasts.json (B2).
    /// </summary>
    public static class TestBattles
    {
        public static TribeCombatProfile Profile(
            double moveSpeed = 3.0,
            double damage = 1.0,
            double interval = 0.8,
            double range = 1.2)
        {
            return new TribeCombatProfile(moveSpeed, damage, interval, range);
        }

        public static BattleField LureField()
        {
            // Thú chạy từ x nhỏ về vạch thoát ở x = 20.
            return new BattleField(20.0, 6.0, 20.0, 0.5, 2.0, 16.0);
        }

        public static BattleField PreventField()
        {
            // Kẻ địch tiến từ x lớn về cửa hang ở x = 0.
            return new BattleField(20.0, 6.0, 0.0, 0.5, 2.0, 16.0);
        }

        public static Member Member(int id, double speed = 1.0, double strength = 1.0, int hp = 5)
        {
            return new Member(id, speed, strength, hp, null);
        }

        public static List<CombatantSpec> Tribe(TribeCombatProfile profile, params Member[] members)
        {
            List<CombatantSpec> specs = new List<CombatantSpec>();
            for (int i = 0; i < members.Length; i++)
            {
                specs.Add(CombatantSpec.ForMember(members[i], profile));
            }

            return specs;
        }

        public static CombatantSpec Beast(
            int id,
            double x,
            double y = 3.0,
            double hp = 8.0,
            double moveSpeed = 2.0,
            double damage = 1.0,
            double interval = 1.0,
            double range = 1.2,
            BeastAttackStyle style = BeastAttackStyle.Random,
            string speciesId = "test_beast")
        {
            return new CombatantSpec(
                id,
                CombatSide.Beast,
                1.0,
                1.0,
                hp,
                moveSpeed,
                damage,
                interval,
                range,
                style,
                speciesId,
                new CombatVector(x, y));
        }

        public static IRandom Random(int seed = 12345)
        {
            return new XorShift128Random(seed);
        }

        public static BalanceConfig Config()
        {
            return BalanceConfig.FromJson(File.ReadAllText(BalanceJsonPath));
        }

        public static CombatRequest Request(
            CombatKind kind,
            IReadOnlyList<int> memberIds,
            IReadOnlyList<int> allMemberIds = null,
            int stage = 2,
            int region = 0,
            int buildLevel = 0,
            int difficultyLevel = 0)
        {
            IReadOnlyList<int> all = allMemberIds ?? memberIds;
            return new CombatRequest(kind, memberIds, stage, region, buildLevel, difficultyLevel, all.Count, all);
        }

        /// <summary>Gốc dự án, tìm bằng cách leo ngược như Core.Tests.TestPaths.</summary>
        public static string RepoRoot
        {
            get
            {
                if (_repoRoot == null)
                {
                    _repoRoot = FindRepoRoot();
                }

                return _repoRoot;
            }
        }

        public static string BalanceJsonPath
        {
            get { return Path.Combine(RepoRoot, "Assets", "Data", "balance.json"); }
        }

        public static string BeastsJsonPath
        {
            get { return Path.Combine(RepoRoot, "Assets", "Data", "beasts.json"); }
        }

        public static string CombatSourceRoot
        {
            get { return Path.Combine(RepoRoot, "Assets", "Scripts", "Combat"); }
        }

        private const string Marker = "Assets/Data/balance.json";

        private static string _repoRoot;

        private static string FindRepoRoot()
        {
            string[] starts =
            {
                Directory.GetCurrentDirectory(),
                AppContext.BaseDirectory
            };

            foreach (string start in starts)
            {
                DirectoryInfo dir = new DirectoryInfo(start);
                while (dir != null)
                {
                    if (File.Exists(Path.Combine(dir.FullName, Marker.Replace('/', Path.DirectorySeparatorChar))))
                    {
                        return dir.FullName;
                    }

                    dir = dir.Parent;
                }
            }

            throw new InvalidOperationException(
                "Không tìm thấy gốc dự án (mốc: " + Marker + "). Đã leo từ: " + string.Join(" và ", starts));
        }
    }
}
