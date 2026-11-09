using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BeforeUs.Core.Plants
{
    /// <summary>Ánh xạ 1-1 với Assets/Data/plants.json. Cùng kỷ luật nạp như BalanceData.</summary>
    public sealed class PlantData
    {
        [JsonProperty("id", Required = Required.Always)] public string Id { get; private set; }
        [JsonProperty("name", Required = Required.Always)] public string Name { get; private set; }
        [JsonProperty("description", Required = Required.Always)] public string Description { get; private set; }

        /// <summary>Tỉ lệ MỘT người tìm thấy loài này trong một lần Kiếm ăn, trước mọi hệ số.</summary>
        [JsonProperty("find_chance", Required = Required.Always)] public double FindChance { get; private set; }

        /// <summary>Thức ăn nhận được MỖI LẦN tìm thấy.</summary>
        [JsonProperty("nutrition", Required = Required.Always)] public int Nutrition { get; private set; }

        [JsonProperty("regions", Required = Required.Always)] public int[] Regions { get; private set; }

        /// <summary>Nhân vào find_chance trong pha Khô (design-v7 A.8).</summary>
        [JsonProperty("dry_multiplier", Required = Required.Always)] public double DryMultiplier { get; private set; }

        /// <summary>
        /// TỔNG nhanh nhẹn của cả nhóm được cử đi, đạt ngưỡng thì CHẮC CHẮN tìm thấy loài này
        /// ít nhất một lần (E2 của Khoa, playtest 01: "cây yêu cầu có tổng nhanh nhẹn là 6
        /// thì mới chắc chắn tìm thấy"). Loài càng hiếm thì ngưỡng càng cao.
        /// KHÔNG phải chỉ số của từng người — đó là cách đọc sai mà bản đầu đã cài nhầm.
        /// </summary>
        [JsonProperty("guaranteed_total_speed", Required = Required.Always)] public double GuaranteedTotalSpeed { get; private set; }

        /// <summary>Trọng số khi rút loài mới. 0 nghĩa là biết sẵn từ đầu ván.</summary>
        [JsonProperty("discover_weight", Required = Required.Always)] public int DiscoverWeight { get; private set; }

        public bool KnownFromStart
        {
            get { return DiscoverWeight == 0; }
        }

        public bool GrowsIn(int region)
        {
            for (int i = 0; i < Regions.Length; i++)
            {
                if (Regions[i] == region)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class PlantDataException : Exception
    {
        public PlantDataException(string message) : base(message)
        {
        }

        public PlantDataException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    /// <summary>
    /// Danh mục thực vật (design-v7-draft.md §A). Chỉ tra cứu, không giữ trạng thái ván —
    /// loài nào ĐÃ KHÁM PHÁ nằm trong GameState.DiscoveredPlantIds, không nằm ở đây.
    /// </summary>
    public sealed class PlantCatalog
    {
        private readonly List<PlantData> _all;
        private readonly Dictionary<string, PlantData> _byId;

        private PlantCatalog(List<PlantData> all)
        {
            _all = all;
            _byId = new Dictionary<string, PlantData>(StringComparer.Ordinal);
            for (int i = 0; i < all.Count; i++)
            {
                _byId[all[i].Id] = all[i];
            }
        }

        public IReadOnlyList<PlantData> All
        {
            get { return _all; }
        }

        public PlantData Get(string id)
        {
            PlantData plant;
            return id != null && _byId.TryGetValue(id, out plant) ? plant : null;
        }

        /// <summary>Những loài biết sẵn từ đầu ván (discover_weight = 0).</summary>
        public List<string> KnownFromStartIds()
        {
            List<string> ids = new List<string>();
            for (int i = 0; i < _all.Count; i++)
            {
                if (_all[i].KnownFromStart)
                {
                    ids.Add(_all[i].Id);
                }
            }

            return ids;
        }

        /// <summary>Loài ĐÃ BIẾT và mọc được ở vùng này. Thứ tự theo file, nên tất định.</summary>
        public List<PlantData> KnownInRegion(IReadOnlyList<string> discoveredIds, int region)
        {
            HashSet<string> known = new HashSet<string>(discoveredIds, StringComparer.Ordinal);
            List<PlantData> result = new List<PlantData>();

            for (int i = 0; i < _all.Count; i++)
            {
                if (_all[i].GrowsIn(region) && known.Contains(_all[i].Id))
                {
                    result.Add(_all[i]);
                }
            }

            return result;
        }

        /// <summary>Loài CHƯA BIẾT mà mọc được ở vùng này — tức còn khám phá được.</summary>
        public List<PlantData> UndiscoveredInRegion(IReadOnlyList<string> discoveredIds, int region)
        {
            HashSet<string> known = new HashSet<string>(discoveredIds, StringComparer.Ordinal);
            List<PlantData> result = new List<PlantData>();

            for (int i = 0; i < _all.Count; i++)
            {
                if (_all[i].GrowsIn(region) && !known.Contains(_all[i].Id) && !_all[i].KnownFromStart)
                {
                    result.Add(_all[i]);
                }
            }

            return result;
        }

        /// <summary>
        /// Rút một loài chưa biết theo discover_weight. Tất định với cùng IRandom.
        /// Trả null nếu không còn gì để khám phá ở vùng này.
        /// </summary>
        public PlantData DrawUndiscovered(IReadOnlyList<string> discoveredIds, int region, IRandom random)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            List<PlantData> pool = UndiscoveredInRegion(discoveredIds, region);
            if (pool.Count == 0)
            {
                return null;
            }

            int total = 0;
            for (int i = 0; i < pool.Count; i++)
            {
                total += pool[i].DiscoverWeight;
            }

            if (total <= 0)
            {
                return null;
            }

            int roll = random.NextInt(0, total);
            for (int i = 0; i < pool.Count; i++)
            {
                roll -= pool[i].DiscoverWeight;
                if (roll < 0)
                {
                    return pool[i];
                }
            }

            return pool[pool.Count - 1];
        }

        public static PlantCatalog Load(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new PlantDataException("plants.json rỗng");
            }

            PlantFile file;
            try
            {
                file = JsonConvert.DeserializeObject<PlantFile>(json, new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Error
                });
            }
            catch (JsonException ex)
            {
                throw new PlantDataException("plants.json không đọc được: " + ex.Message, ex);
            }

            if (file == null || file.Plants == null)
            {
                throw new PlantDataException("plants.json không đọc được: kết quả rỗng");
            }

            Validate(file.Plants);
            return new PlantCatalog(new List<PlantData>(file.Plants));
        }

        private static void Validate(PlantData[] plants)
        {
            if (plants.Length == 0)
            {
                throw new PlantDataException("plants.json không có loài nào");
            }

            HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
            bool anyKnownFromStart = false;

            for (int i = 0; i < plants.Length; i++)
            {
                PlantData p = plants[i];

                if (!ids.Add(p.Id))
                {
                    throw new PlantDataException("Trùng id loài: " + p.Id);
                }

                if (p.FindChance <= 0 || p.FindChance > 1)
                {
                    throw new PlantDataException(p.Id + ": find_chance phải nằm trong (0, 1], nhận được " + p.FindChance);
                }

                if (p.Nutrition <= 0)
                {
                    throw new PlantDataException(p.Id + ": nutrition phải > 0");
                }

                if (p.Regions.Length == 0)
                {
                    throw new PlantDataException(p.Id + ": không thuộc vùng nào thì không bao giờ gặp được");
                }

                if (p.DryMultiplier < 0)
                {
                    throw new PlantDataException(p.Id + ": dry_multiplier không được âm");
                }

                if (p.GuaranteedTotalSpeed <= 0)
                {
                    throw new PlantDataException(p.Id + ": guaranteed_total_speed phải > 0");
                }

                if (p.DiscoverWeight < 0)
                {
                    throw new PlantDataException(p.Id + ": discover_weight không được âm");
                }

                if (p.KnownFromStart)
                {
                    anyKnownFromStart = true;
                }
            }

            // Không có loài nào biết sẵn thì lượt 1 người chơi không biết hái gì, và khung đảm bảo
            // theo speed cũng không có loài nào để bảo đảm. Nổ ngay còn hơn để ván chết câm.
            if (!anyKnownFromStart)
            {
                throw new PlantDataException("plants.json phải có ít nhất một loài discover_weight = 0 (biết sẵn từ đầu ván)");
            }
        }

        private sealed class PlantFile
        {
            [JsonProperty("_comment")] public string Comment { get; private set; }
            [JsonProperty("plants", Required = Required.Always)] public PlantData[] Plants { get; private set; }
        }
    }
}
