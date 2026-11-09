using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BeforeUs.Core.Sites
{
    /// <summary>
    /// Loại slot công trình (design-v7 §B.5). Quyết định nút neuron nào mở nó ra.
    /// Tên thành viên khớp từng chữ với BeforeUs.Game.SiteKind của nhánh UI, để lớp nối
    /// giữa hai bên chỉ là một phép đổi tên chứ không phải một bảng tra.
    /// </summary>
    public enum SiteKind
    {
        Wood,
        Stone,
        Earth,

        /// <summary>Ổ đựng thức ăn: công trình NÂNG CẤP nhiều bậc, không phải slot dọn một lần (§B.4).</summary>
        FoodNest
    }

    /// <summary>
    /// Một slot công trình. Ánh xạ 1-1 với một phần tử trong Assets/Data/sites.json.
    ///
    /// KHÔNG có trường tên và mô tả. Nội dung hiện cho người chơi nằm ở strings.vi.json
    /// của nhánh UI theo khóa "site.&lt;id&gt;.name" — cố ý để đúng một nơi giữ nó.
    /// </summary>
    public sealed class SiteData
    {
        [JsonProperty("id", Required = Required.Always)] public string Id { get; private set; }

        [JsonProperty("kind", Required = Required.Always)] public string KindText { get; private set; }

        /// <summary>Giai đoạn tiến hóa slot này bắt đầu hiện ra.</summary>
        [JsonProperty("stage", Required = Required.Always)] public int Stage { get; private set; }

        /// <summary>TỔNG sức mạnh của nhóm được cử phải đạt mức này mới dọn nổi (§B.1).</summary>
        [JsonProperty("strength_required", Required = Required.Always)] public double StrengthRequired { get; private set; }

        /// <summary>Nút neuron phải mua trước, hoặc null (§B.3). Khóa phải có mặt, giá trị được phép null.</summary>
        [JsonProperty("requires_node", Required = Required.AllowNull)] public string RequiresNodeId { get; private set; }

        [JsonProperty("art_key", Required = Required.Always)] public string ArtKey { get; private set; }

        /// <summary>Vùng chứa slot này. Catalog điền lúc nạp, không có trong file.</summary>
        [JsonIgnore] public int Region { get; internal set; }

        [JsonIgnore] public SiteKind Kind { get; internal set; }
    }

    /// <summary>
    /// Một vùng: trần chỗ ở, số chỗ mở sẵn, và danh sách công trình dọn được.
    ///
    /// ⚠ "Chỗ" ở đây là CHỖ CHO THÀNH VIÊN, tức sức chứa hang — không phải ô đất đặt công trình.
    /// Khoa, 2026-11-09: "vùng 1 có tối đa 8 chỗ, khởi đầu là 4 chỗ, cần phải thực hiện dọn chỗ
    /// cho 4 chỗ còn lại". Dọn một công trình thì mở thêm một chỗ, tới trần của vùng.
    /// </summary>
    public sealed class RegionSites
    {
        [JsonProperty("region", Required = Required.Always)] public int Region { get; private set; }

        /// <summary>Trần chỗ ở của vùng (vùng 0: 8, vùng 1: 14).</summary>
        [JsonProperty("capacity_max", Required = Required.Always)] public int CapacityMax { get; private set; }

        /// <summary>Số chỗ đã mở sẵn khi bộ tộc vừa tới vùng này (vùng 0: 4).</summary>
        [JsonProperty("capacity_open_at_start", Required = Required.Always)] public int CapacityOpenAtStart { get; private set; }

        [JsonProperty("sites", Required = Required.Always)] public SiteData[] Sites { get; private set; }
    }

    /// <summary>Một bậc của Ổ đựng thức ăn (design-v7 §B.4, theo cách đọc E2 của Khoa).</summary>
    public sealed class FoodNestTierData
    {
        [JsonProperty("tier", Required = Required.Always)] public int Tier { get; private set; }

        [JsonProperty("strength_required", Required = Required.Always)] public double StrengthRequired { get; private set; }

        /// <summary>
        /// Số CỘNG vào tổng nhanh nhẹn của nhóm khi xét khung đảm bảo của từng loài thực vật.
        ///
        /// E2 của Khoa, nguyên văn: "cây yêu cầu có tổng nhanh nhẹn là 6 thì mới chắc chắn tìm thấy
        /// thì giảm còn 5.8 hoặc 5.5". Hạ ngưỡng của mọi loài đi 0.2 và cộng 0.2 vào tổng của nhóm
        /// là cùng một hiệu lực; cài theo cách thứ hai vì chỉ có MỘT chỗ để sai.
        /// </summary>
        [JsonProperty("speed_threshold_relief", Required = Required.Always)] public double SpeedThresholdRelief { get; private set; }
    }

    public sealed class FoodNestData
    {
        [JsonProperty("id", Required = Required.Always)] public string Id { get; private set; }
        [JsonProperty("art_key", Required = Required.Always)] public string ArtKey { get; private set; }
        [JsonProperty("tiers", Required = Required.Always)] public FoodNestTierData[] Tiers { get; private set; }
    }

    public class SiteDataException : Exception
    {
        public SiteDataException(string message) : base(message)
        {
        }

        public SiteDataException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    /// <summary>
    /// Danh mục slot công trình (design-v7 §B). Chỉ tra cứu, KHÔNG giữ trạng thái ván —
    /// slot nào đã dọn nằm trong GameState.ClearedSiteIds, bậc ổ nằm ở GameState.FoodNestTier.
    ///
    /// Cùng kỷ luật nạp như BalanceData và PlantCatalog: thiếu khóa thì nổ, thừa khóa cũng nổ.
    /// </summary>
    public sealed class SiteCatalog
    {
        private readonly List<RegionSites> _regions;
        private readonly Dictionary<string, SiteData> _byId;
        private readonly FoodNestData _nest;

        private SiteCatalog(List<RegionSites> regions, FoodNestData nest)
        {
            _regions = regions;
            _nest = nest;
            _byId = new Dictionary<string, SiteData>(StringComparer.Ordinal);

            for (int r = 0; r < regions.Count; r++)
            {
                SiteData[] sites = regions[r].Sites;
                for (int i = 0; i < sites.Length; i++)
                {
                    _byId[sites[i].Id] = sites[i];
                }
            }
        }

        /// <summary>Id của Ổ đựng thức ăn. Không nằm trong danh sách slot vì nó nâng cấp nhiều bậc.</summary>
        public string FoodNestId
        {
            get { return _nest.Id; }
        }

        public string FoodNestArtKey
        {
            get { return _nest.ArtKey; }
        }

        /// <summary>Bậc cao nhất của Ổ đựng thức ăn.</summary>
        public int FoodNestMaxTier
        {
            get { return _nest.Tiers.Length; }
        }

        /// <summary>Thông số của một bậc ổ, đếm từ 1. Trả null nếu ngoài thang.</summary>
        public FoodNestTierData FoodNestTier(int tier)
        {
            if (tier < 1 || tier > _nest.Tiers.Length)
            {
                return null;
            }

            return _nest.Tiers[tier - 1];
        }

        /// <summary>
        /// Số cộng vào tổng nhanh nhẹn của nhóm hái lượm khi ổ đang ở bậc này (§B.4, E2).
        /// Bậc 0 cho 0 — ổ đất trống chỉ là lối tắt giao diện, không có hiệu ứng số.
        /// </summary>
        public double SpeedThresholdRelief(int tier)
        {
            FoodNestTierData data = FoodNestTier(tier);
            return data == null ? 0.0 : data.SpeedThresholdRelief;
        }

        public SiteData Get(string id)
        {
            SiteData site;
            return id != null && _byId.TryGetValue(id, out site) ? site : null;
        }

        /// <summary>Mọi slot của một vùng, kể cả slot chưa tới giai đoạn. Thứ tự theo file nên tất định.</summary>
        public IReadOnlyList<SiteData> InRegion(int region)
        {
            for (int r = 0; r < _regions.Count; r++)
            {
                if (_regions[r].Region == region)
                {
                    return _regions[r].Sites;
                }
            }

            return new SiteData[0];
        }

        /// <summary>Trần chỗ ở của vùng. 0 nếu vùng không có trong file.</summary>
        public int CapacityMax(int region)
        {
            RegionSites found = FindRegion(region);
            return found == null ? 0 : found.CapacityMax;
        }

        /// <summary>
        /// Số chỗ mở sẵn khi vừa tới vùng này. Đây cũng là TRẦN SỐ NGƯỜI được mang theo khi di cư
        /// sang vùng đó (Khoa: "miễn khu vực mới có đủ slot cho số người di cư").
        /// 0 nếu vùng không có trong file — và 0 nghĩa là KHÔNG di cư sang đó được.
        /// </summary>
        public int CapacityOpenAtStart(int region)
        {
            RegionSites found = FindRegion(region);
            return found == null ? 0 : found.CapacityOpenAtStart;
        }

        public bool HasRegion(int region)
        {
            return FindRegion(region) != null;
        }

        private RegionSites FindRegion(int region)
        {
            for (int r = 0; r < _regions.Count; r++)
            {
                if (_regions[r].Region == region)
                {
                    return _regions[r];
                }
            }

            return null;
        }

        public static SiteCatalog Load(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new SiteDataException("sites.json rỗng");
            }

            SiteFile file;
            try
            {
                file = JsonConvert.DeserializeObject<SiteFile>(json, new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Error
                });
            }
            catch (JsonException ex)
            {
                throw new SiteDataException("sites.json không đọc được: " + ex.Message, ex);
            }

            if (file == null || file.Regions == null || file.FoodNest == null)
            {
                throw new SiteDataException("sites.json không đọc được: kết quả rỗng");
            }

            Validate(file);

            return new SiteCatalog(new List<RegionSites>(file.Regions), file.FoodNest);
        }

        private static void Validate(SiteFile file)
        {
            if (file.Regions.Length == 0)
            {
                throw new SiteDataException("sites.json không có vùng nào");
            }

            HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
            HashSet<int> regions = new HashSet<int>();

            for (int r = 0; r < file.Regions.Length; r++)
            {
                RegionSites region = file.Regions[r];

                if (!regions.Add(region.Region))
                {
                    throw new SiteDataException("Vùng " + region.Region + " xuất hiện hai lần");
                }

                if (region.CapacityMax <= 0)
                {
                    throw new SiteDataException("Vùng " + region.Region + ": capacity_max phải > 0");
                }

                if (region.CapacityOpenAtStart <= 0 || region.CapacityOpenAtStart > region.CapacityMax)
                {
                    // 0 chỗ mở sẵn nghĩa là tới nơi không ai ở được — không di cư sang nổi.
                    throw new SiteDataException(
                        "Vùng " + region.Region + ": capacity_open_at_start phải nằm trong [1, "
                        + region.CapacityMax + "]");
                }

                if (region.Sites == null || region.Sites.Length == 0)
                {
                    throw new SiteDataException("Vùng " + region.Region + " không có công trình nào, Xây dựng sẽ thành nút chết");
                }

                // Mỗi công trình mở đúng một chỗ ([ĐỀ XUẤT]), nên số công trình phải bằng đúng
                // phần còn thiếu. Thừa thì có công trình dọn xong mà không mở được gì — một nút
                // bấm không làm gì cả; thiếu thì vùng không bao giờ đạt trần của chính nó.
                int openable = region.CapacityMax - region.CapacityOpenAtStart;
                if (region.Sites.Length != openable)
                {
                    throw new SiteDataException(
                        "Vùng " + region.Region + ": có " + region.Sites.Length
                        + " công trình nhưng còn " + openable + " chỗ phải mở (trần "
                        + region.CapacityMax + " − mở sẵn " + region.CapacityOpenAtStart + ")");
                }

                for (int i = 0; i < region.Sites.Length; i++)
                {
                    SiteData site = region.Sites[i];

                    if (!ids.Add(site.Id))
                    {
                        throw new SiteDataException("Trùng id slot: " + site.Id);
                    }

                    if (site.Stage < 1)
                    {
                        throw new SiteDataException(site.Id + ": stage đếm từ 1");
                    }

                    if (site.StrengthRequired <= 0)
                    {
                        throw new SiteDataException(site.Id + ": strength_required phải > 0");
                    }

                    site.Kind = ParseKind(site.Id, site.KindText);
                    site.Region = region.Region;

                    if (site.Kind == SiteKind.FoodNest)
                    {
                        throw new SiteDataException(
                            site.Id + ": Ổ đựng thức ăn không phải slot dọn một lần, khai ở khối food_nest");
                    }
                }
            }

            ValidateNest(file.FoodNest, ids);
        }

        private static void ValidateNest(FoodNestData nest, HashSet<string> ids)
        {
            if (!ids.Add(nest.Id))
            {
                throw new SiteDataException("Id Ổ đựng thức ăn trùng với một slot: " + nest.Id);
            }

            if (nest.Tiers == null || nest.Tiers.Length == 0)
            {
                throw new SiteDataException("food_nest không có bậc nào");
            }

            double previousStrength = 0;
            double previousRelief = 0;

            for (int i = 0; i < nest.Tiers.Length; i++)
            {
                FoodNestTierData tier = nest.Tiers[i];

                if (tier.Tier != i + 1)
                {
                    throw new SiteDataException(
                        "food_nest: bậc phải liên tiếp từ 1, gặp " + tier.Tier + " ở vị trí " + (i + 1));
                }

                if (tier.StrengthRequired <= previousStrength)
                {
                    throw new SiteDataException(
                        "food_nest bậc " + tier.Tier + ": strength_required phải tăng dần");
                }

                // Bậc sau mà nới ngưỡng ít hơn bậc trước thì nâng cấp xong lại yếu đi —
                // người chơi trả giá để nhận một thứ tệ hơn. Nổ ngay còn hơn để lọt vào ván.
                if (tier.SpeedThresholdRelief <= previousRelief)
                {
                    throw new SiteDataException(
                        "food_nest bậc " + tier.Tier + ": speed_threshold_relief phải tăng dần");
                }

                previousStrength = tier.StrengthRequired;
                previousRelief = tier.SpeedThresholdRelief;
            }
        }

        private static SiteKind ParseKind(string id, string text)
        {
            switch (text)
            {
                case "wood": return SiteKind.Wood;
                case "stone": return SiteKind.Stone;
                case "earth": return SiteKind.Earth;
                case "food_nest": return SiteKind.FoodNest;
                default:
                    throw new SiteDataException(
                        id + ": kind không hiểu được: '" + text + "'. Chỉ nhận wood, stone, earth, food_nest");
            }
        }

        private sealed class SiteFile
        {
            [JsonProperty("_comment")] public string Comment { get; private set; }
            [JsonProperty("regions", Required = Required.Always)] public RegionSites[] Regions { get; private set; }
            [JsonProperty("food_nest", Required = Required.Always)] public FoodNestData FoodNest { get; private set; }
        }
    }
}
