using System;
using System.IO;

namespace BeforeUs.Core.Tests
{
    /// <summary>
    /// Tìm gốc dự án để test đọc được Assets/Data/*.json.
    /// Phải chạy được ở cả hai nơi: dotnet test (cwd là thư mục bin) và Unity Test Runner (cwd là gốc dự án),
    /// nên leo ngược từ cả thư mục hiện tại lẫn thư mục chứa assembly.
    /// </summary>
    public static class TestPaths
    {
        private const string Marker = "Assets/Data/balance.json";

        private static string _repoRoot;

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

        public static string NeuronsJsonPath
        {
            get { return Path.Combine(RepoRoot, "Assets", "Data", "neurons.json"); }
        }

        public static string ReadBalanceJson()
        {
            return File.ReadAllText(BalanceJsonPath);
        }

        public static string ReadNeuronsJson()
        {
            return File.ReadAllText(NeuronsJsonPath);
        }

        public static string PlantsJsonPath
        {
            get { return Path.Combine(RepoRoot, "Assets", "Data", "plants.json"); }
        }

        public static string ReadPlantsJson()
        {
            return File.ReadAllText(PlantsJsonPath);
        }

        public static string SitesJsonPath
        {
            get { return Path.Combine(RepoRoot, "Assets", "Data", "sites.json"); }
        }

        public static string ReadSitesJson()
        {
            return File.ReadAllText(SitesJsonPath);
        }

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
                "Không tìm thấy gốc dự án (mốc: " + Marker + "). Đã leo từ: "
                + string.Join(" và ", starts));
        }
    }
}
