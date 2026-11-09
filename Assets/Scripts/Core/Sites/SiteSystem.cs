using System;
using System.Collections.Generic;

namespace BeforeUs.Core.Sites
{
    /// <summary>
    /// Một slot đã chốt mọi thứ tầng trên cần biết để vẽ. Dữ liệu thuần, không có luật.
    /// Tên thành viên khớp từng chữ với BeforeUs.Game.SiteView của nhánh UI.
    /// </summary>
    public sealed class SiteView
    {
        /// <summary>
        /// Id là thứ DUY NHẤT định danh slot. Tên và mô tả nằm ở strings.vi.json của nhánh UI,
        /// cố ý không có ở đây — một trường tên bỏ trống sẽ lặng lẽ in ra "Đã dọn xong ."
        /// trước mặt người chơi.
        /// </summary>
        public string Id { get; internal set; }

        public SiteKind Kind { get; internal set; }

        /// <summary>Tổng sức mạnh nhóm phải đạt để dọn, hoặc để lên bậc tiếp theo nếu là Ổ (§B.1).</summary>
        public double StrengthRequired { get; internal set; }

        /// <summary>Nút neuron phải mua trước, hoặc null (§B.3).</summary>
        public string RequiresNodeId { get; internal set; }

        public int Stage { get; internal set; }

        public bool Cleared { get; internal set; }

        /// <summary>Bậc hiện tại — chỉ có nghĩa với Ổ đựng thức ăn.</summary>
        public int Tier { get; internal set; }

        public int MaxTier { get; internal set; }

        public string ArtKey { get; internal set; }
    }

    /// <summary>Chuyện gì xảy ra với một slot ở cuối lượt (§B.1: ăn cả ngã về không).</summary>
    public sealed class SiteResolution
    {
        public static readonly SiteResolution None = new SiteResolution();

        public bool Attempted { get; internal set; }
        public bool Cleared { get; internal set; }
        public string SiteId { get; internal set; }
        public double Strength { get; internal set; }
        public double Required { get; internal set; }

        /// <summary>Bậc Ổ sau khi giải quyết; 0 với mọi slot thường.</summary>
        public int TierAfter { get; internal set; }
    }

    /// <summary>
    /// Hệ slot công trình (design-v7 §B). Thay hẳn cơ chế điểm xây của design.md §5.
    ///
    /// Luật trung tâm (D7 của Khoa): cử người vào một slot, ĐỦ TỔNG SỨC MẠNH thì dọn xong ngay
    /// trong lần đó; thiếu thì lượt đó phí, KHÔNG tích lũy gì. Không có thanh tiến trình.
    ///
    /// Công trình THUỘC VỀ VÙNG (§F.2). Dọn một công trình thì mở thêm một chỗ ở, tới trần của
    /// vùng. Di cư xoá sạch danh sách đã dọn và nhận bộ công trình của vùng mới — đó là lý do
    /// Khoa gọi di cư là "cơ chế giảm sức mạnh người chơi ở khu vực cũ".
    /// </summary>
    public sealed class SiteSystem
    {
        private readonly SiteCatalog _catalog;
        private readonly GameState _state;

        public SiteSystem(SiteCatalog catalog, GameState state)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            if (state == null) throw new ArgumentNullException(nameof(state));

            _catalog = catalog;
            _state = state;
        }

        public SiteCatalog Catalog
        {
            get { return _catalog; }
        }

        /// <summary>
        /// Số công trình đã dọn ở vùng hiện tại, cộng từng bậc Ổ đựng thức ăn.
        /// Đây chính là build_level của §B.2 — <see cref="GameState.BuildLevel"/> tính cùng công thức.
        /// </summary>
        public int ClearedCount
        {
            get { return _state.BuildLevel; }
        }

        /// <summary>
        /// Đặt bộ tộc vào một vùng: chép trần và số chỗ mở sẵn của vùng đó vào GameState,
        /// và xoá danh sách công trình đã dọn (công trình thuộc về vùng cũ).
        ///
        /// Gọi lúc dựng ván mới và lúc di cư. KHÔNG gọi khi tải ván — ván đã lưu giữ sẵn
        /// cả ba thứ đó, tra lại từ catalog sẽ ghi đè mất tiến độ của người chơi.
        /// </summary>
        public void EnterRegion(int region)
        {
            if (!_catalog.HasRegion(region))
            {
                throw new SiteDataException("sites.json không có vùng " + region);
            }

            _state.Region = region;
            _state.RegionCapacityOpen = _catalog.CapacityOpenAtStart(region);
            _state.RegionCapacityMax = _catalog.CapacityMax(region);
            _state.ClearedSiteIds.Clear();
        }

        /// <summary>
        /// Vùng kế tiếp có tồn tại không. Di cư là ĐƯỜNG MỘT CHIỀU (Khoa 2026-11-09:
        /// "nó là lối đi 1 chiều không có đường trở lại"), nên chỉ có khái niệm vùng KẾ TIẾP.
        /// </summary>
        public bool HasNextRegion(int regionMaxIndex)
        {
            int next = _state.Region + 1;
            return next <= regionMaxIndex && _catalog.HasRegion(next);
        }

        /// <summary>
        /// Số người tối đa mang theo được sang vùng kế tiếp = số chỗ mở sẵn của vùng đó.
        /// 0 nghĩa là không còn vùng nào để đi.
        /// </summary>
        public int NextRegionCapacityOpen(int regionMaxIndex)
        {
            return HasNextRegion(regionMaxIndex) ? _catalog.CapacityOpenAtStart(_state.Region + 1) : 0;
        }

        /// <summary>
        /// Những slot còn hiện ra: chưa dọn, đã tới giai đoạn, đã đủ điều kiện nút neuron,
        /// và thuộc vùng bộ tộc đang ở. Ổ đựng thức ăn luôn đứng đầu nếu còn bậc để lên.
        /// </summary>
        public List<SiteView> VisibleSites(int stage, IPurchasedNodes nodes)
        {
            IPurchasedNodes purchased = nodes ?? NoPurchasedNodes.Instance;
            List<SiteView> visible = new List<SiteView>();

            SiteView nest = FoodNestView();
            if (nest != null && !nest.Cleared)
            {
                visible.Add(nest);
            }

            IReadOnlyList<SiteData> sites = _catalog.InRegion(_state.Region);
            for (int i = 0; i < sites.Count; i++)
            {
                SiteData site = sites[i];

                if (site.Stage > stage || IsCleared(site.Id))
                {
                    continue;
                }

                if (site.RequiresNodeId != null && !purchased.Has(site.RequiresNodeId))
                {
                    continue;
                }

                visible.Add(ViewOf(site));
            }

            return visible;
        }

        /// <summary>Tra một slot theo id, kể cả slot đã dọn hoặc ở vùng khác. Null nếu không có.</summary>
        public SiteView Find(string siteId)
        {
            if (siteId != null && siteId == _catalog.FoodNestId)
            {
                return FoodNestView();
            }

            SiteData site = _catalog.Get(siteId);
            return site == null ? null : ViewOf(site);
        }

        /// <summary>
        /// Tổng sức mạnh của một nhóm. Đọc Member.Strength của Core; người đã chết không tính.
        /// </summary>
        public double StrengthOf(IReadOnlyList<int> memberIds)
        {
            if (memberIds == null)
            {
                return 0;
            }

            double total = 0;
            for (int i = 0; i < memberIds.Count; i++)
            {
                Member member = _state.FindMember(memberIds[i]);
                if (member != null)
                {
                    total += member.Strength;
                }
            }

            return total;
        }

        /// <summary>
        /// Nhóm này có dọn nổi slot đó không, nếu kết thúc lượt ngay bây giờ.
        /// Truy vấn thuần, KHÔNG tác dụng phụ — cùng kỷ luật với TurnPlan.CanAfford.
        /// </summary>
        public bool WouldClear(string siteId, IReadOnlyList<int> memberIds)
        {
            SiteView view = Find(siteId);
            if (view == null || view.Cleared)
            {
                return false;
            }

            return StrengthOf(memberIds) >= view.StrengthRequired;
        }

        /// <summary>
        /// Giải quyết ở cuối lượt. Đây là nơi DUY NHẤT slot đổi trạng thái.
        /// Ăn cả ngã về không: thiếu sức mạnh thì lượt đó phí, không tích lũy gì (§B.1).
        /// </summary>
        public SiteResolution Resolve(string siteId, IReadOnlyList<int> memberIds)
        {
            SiteView view = Find(siteId);
            if (view == null || view.Cleared)
            {
                return SiteResolution.None;
            }

            double strength = StrengthOf(memberIds);
            SiteResolution resolution = new SiteResolution
            {
                Attempted = true,
                SiteId = view.Id,
                Strength = strength,
                Required = view.StrengthRequired,
                TierAfter = view.Tier
            };

            if (strength < view.StrengthRequired)
            {
                return resolution;
            }

            resolution.Cleared = true;

            if (view.Kind == SiteKind.FoodNest)
            {
                _state.FoodNestTier++;
                resolution.TierAfter = _state.FoodNestTier;
            }
            else
            {
                _state.ClearedSiteIds.Add(view.Id);
            }

            return resolution;
        }

        /// <summary>
        /// Số cộng vào tổng nhanh nhẹn của nhóm hái lượm, do bậc Ổ đựng thức ăn hiện tại (§B.4, E2).
        /// </summary>
        public double SpeedThresholdRelief
        {
            get { return _catalog.SpeedThresholdRelief(_state.FoodNestTier); }
        }

        public bool IsCleared(string siteId)
        {
            if (siteId == _catalog.FoodNestId)
            {
                return _state.FoodNestTier >= _catalog.FoodNestMaxTier;
            }

            return _state.ClearedSiteIds.Contains(siteId);
        }

        private SiteView ViewOf(SiteData site)
        {
            return new SiteView
            {
                Id = site.Id,
                Kind = site.Kind,
                StrengthRequired = site.StrengthRequired,
                RequiresNodeId = site.RequiresNodeId,
                Stage = site.Stage,
                Cleared = IsCleared(site.Id),
                Tier = 0,
                MaxTier = 0,
                ArtKey = site.ArtKey
            };
        }

        /// <summary>
        /// Ổ đựng thức ăn dưới dạng một slot. StrengthRequired là của BẬC TIẾP THEO —
        /// hết bậc thì coi như đã dọn xong và không hiện ra nữa.
        /// </summary>
        private SiteView FoodNestView()
        {
            int tier = _state.FoodNestTier;
            bool maxed = tier >= _catalog.FoodNestMaxTier;
            FoodNestTierData next = _catalog.FoodNestTier(tier + 1);

            return new SiteView
            {
                Id = _catalog.FoodNestId,
                Kind = SiteKind.FoodNest,
                StrengthRequired = next == null ? 0 : next.StrengthRequired,
                RequiresNodeId = null,
                Stage = 1,
                Cleared = maxed,
                Tier = tier,
                MaxTier = _catalog.FoodNestMaxTier,
                ArtKey = _catalog.FoodNestArtKey
            };
        }
    }
}
