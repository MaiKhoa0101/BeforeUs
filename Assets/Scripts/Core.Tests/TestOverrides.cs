using System;
using System.Collections.Generic;
using System.Globalization;
using BeforeUs.Core.Config;
using BeforeUs.Core.Neuron;
using BeforeUs.Core.Plants;
using BeforeUs.Core.Sites;

namespace BeforeUs.Core.Tests
{
    /// <summary>
    /// Ghi đè số liệu CHỈ TRONG BỘ NHỚ để đo các phương án.
    ///
    /// Không file nào trên đĩa bị sửa: ta đọc chuỗi JSON gốc, thay đúng một mẩu chuỗi, rồi nạp
    /// chuỗi đã sửa. Assets/Data/balance.json, Assets/Data/neurons.json, docs/design.md và
    /// tools/sim/before_us_sim.py giữ nguyên từng byte. Đây là ĐO, không phải ĐỔI.
    /// </summary>
    public static class TestOverrides
    {
        private static NeuronData _standardNeurons;
        private static PlantCatalog _standardPlants;
        private static PlantCatalog _singlePlant;
        private static SiteCatalog _standardSites;

        /// <summary>Dữ liệu nút gốc, nạp một lần rồi dùng lại — 2.000 ván mỗi cấu hình.</summary>
        public static NeuronData StandardNeurons
        {
            get
            {
                if (_standardNeurons == null)
                {
                    _standardNeurons = NeuronNetwork.LoadData(TestPaths.ReadNeuronsJson());
                }

                return _standardNeurons;
            }
        }

        /// <summary>Danh mục slot công trình thật, nạp một lần rồi dùng lại.</summary>
        public static SiteCatalog StandardSites
        {
            get
            {
                if (_standardSites == null)
                {
                    _standardSites = SiteCatalog.Load(TestPaths.ReadSitesJson());
                }

                return _standardSites;
            }
        }

        /// <summary>Danh mục thực vật thật, nạp một lần rồi dùng lại.</summary>
        public static PlantCatalog StandardPlants
        {
            get
            {
                if (_standardPlants == null)
                {
                    _standardPlants = PlantCatalog.Load(TestPaths.ReadPlantsJson());
                }

                return _standardPlants;
            }
        }

        /// <summary>
        /// Danh mục TỐI GIẢN cho test luật lượt: đúng một loài, biết sẵn, dinh dưỡng 3,
        /// không có loài nào để khám phá.
        ///
        /// Vì sao cần: với danh mục thật, mỗi lần Kiếm ăn là một chùm phép rút và việc khám phá
        /// có thể thêm loài giữa lượt — thức ăn thành ngẫu nhiên. Test luật lượt (đói, sinh sản,
        /// thứ tự giải quyết) cần một con số tất định, không cần sự ngẫu nhiên đó.
        /// Loài này có find_chance = 1.0 và dry_multiplier = 1.0, nên thức ăn hái lượm bằng đúng
        /// 3 lần số người đi kiếm ăn — cùng con số với gather_yield của v6, nên số học của test giữ nguyên.
        ///
        /// ⚠ Tính tất định này chỉ đúng ở BẬC ĐỘ KHÓ 0. Từ bậc 1 trở đi, find_chance_multiplier
        /// kéo tỉ lệ xuống dưới 1 và kết quả thành ngẫu nhiên. Trước E2 (khung đảm bảo theo TỪNG
        /// người) danh mục này miễn nhiễm với độ khó; sau E2 (khung đảm bảo theo TỔNG cả nhóm,
        /// tối đa một lần tìm thấy cho cả nhóm) thì không còn miễn nhiễm. Test luật lượt nào cần
        /// con số cố định thì phải nằm trong lượt 1..11 của chế độ MVP.
        /// Test về bản thân cơ chế hái lượm thì dùng StandardPlants.
        /// </summary>
        public static PlantCatalog SinglePlant
        {
            get
            {
                if (_singlePlant == null)
                {
                    _singlePlant = PlantCatalog.Load(
                        "{\"plants\":[{\"id\":\"la_non\",\"name\":\"Lá non\",\"description\":\"test\","
                        + "\"find_chance\":1.0,\"nutrition\":3,\"regions\":[0,1],"
                        + "\"dry_multiplier\":1.0,\"guaranteed_total_speed\":1.0,"
                        + "\"discover_weight\":0}]}");
                }

                return _singlePlant;
            }
        }

        public static BalanceConfig StandardConfig()
        {
            return BalanceConfig.FromJson(TestPaths.ReadBalanceJson(), GameMode.Mvp);
        }

        /// <summary>Thay đúng một cặp khóa–giá trị trong balance.json, trong bộ nhớ.</summary>
        public static BalanceConfig ConfigWith(params KeyValuePair<string, string>[] replacements)
        {
            string json = TestPaths.ReadBalanceJson();
            foreach (KeyValuePair<string, string> pair in replacements)
            {
                if (!json.Contains(pair.Key))
                {
                    throw new InvalidOperationException("Không tìm thấy mẩu cần thay: " + pair.Key);
                }

                json = json.Replace(pair.Key, pair.Value);
            }

            return BalanceConfig.FromJson(json, GameMode.Mvp);
        }

        public static KeyValuePair<string, string> Replace(string from, string to)
        {
            return new KeyValuePair<string, string>(from, to);
        }

        /// <summary>C1: sức chứa hang cơ bản 6 → n.</summary>
        public static BalanceConfig WithLairCapacity(int capacity)
        {
            return ConfigWith(Replace(
                "\"capacity_base\": 6",
                "\"capacity_base\": " + capacity.ToString(CultureInfo.InvariantCulture)));
        }

        /// <summary>C3: chu kỳ độ khó MVP 12 → n.</summary>
        public static BalanceConfig WithDifficultyPeriod(int turns)
        {
            return ConfigWith(Replace(
                "\"period_turns\": { \"full\": 20, \"mvp\": 12 }",
                "\"period_turns\": { \"full\": 20, \"mvp\": " + turns.ToString(CultureInfo.InvariantCulture) + " }"));
        }

        /// <summary>C2: hạ giá hai nút cổng của giai đoạn 2, trong bộ nhớ.</summary>
        public static NeuronData NeuronsWithCheaperGates(int stoneTools, int scavengeMeat)
        {
            string json = TestPaths.ReadNeuronsJson();
            json = ReplaceCostOf(json, "s2_stone_tools", 28, stoneTools);
            json = ReplaceCostOf(json, "s2_scavenge_meat", 42, scavengeMeat);
            return NeuronNetwork.LoadData(json);
        }

        /// <summary>Bỏ toàn bộ hiệu ứng của đúng một nút, để đo đóng góp riêng của nó (câu 13).</summary>
        public static NeuronData NeuronsWithoutEffectsOf(string nodeId)
        {
            string json = TestPaths.ReadNeuronsJson();
            int anchor = json.IndexOf("\"id\": \"" + nodeId + "\"", StringComparison.Ordinal);
            if (anchor < 0)
            {
                throw new InvalidOperationException("Không có nút " + nodeId);
            }

            int effectsAt = json.IndexOf("\"effects\":", anchor, StringComparison.Ordinal);
            int open = json.IndexOf('[', effectsAt);
            int close = json.IndexOf(']', open);

            return NeuronNetwork.LoadData(json.Substring(0, open) + "[]" + json.Substring(close + 1));
        }

        private static string ReplaceCostOf(string json, string nodeId, int oldMvp, int newMvp)
        {
            int anchor = json.IndexOf("\"id\": \"" + nodeId + "\"", StringComparison.Ordinal);
            if (anchor < 0)
            {
                throw new InvalidOperationException("Không có nút " + nodeId);
            }

            string needle = "\"mvp\": " + oldMvp.ToString(CultureInfo.InvariantCulture) + " }";
            int at = json.IndexOf(needle, anchor, StringComparison.Ordinal);
            if (at < 0)
            {
                throw new InvalidOperationException("Giá nút " + nodeId + " không còn là " + oldMvp);
            }

            return json.Substring(0, at)
                   + "\"mvp\": " + newMvp.ToString(CultureInfo.InvariantCulture) + " }"
                   + json.Substring(at + needle.Length);
        }
    }
}
