using System;
using System.Collections.Generic;
using BeforeUs.Core;
using BeforeUs.Core.Neuron;
using BeforeUs.Core.Sites;

namespace BeforeUs.Game
{
    /// <summary>Ba việc làm được từ trung tâm cụm cộng đồng (góp ý 7 của Khoa).</summary>
    public enum HubAction
    {
        /// <summary>Mở bảng dọn một slot công trình.</summary>
        OpenSite,

        /// <summary>Mở màn mạng lưới neuron (mua nút và tiến hóa).</summary>
        OpenNetwork,

        /// <summary>Mở bảng cử người Nâng tinh thần.</summary>
        OpenMorale
    }

    /// <summary>Một mục bấm được trong menu trung tâm.</summary>
    public sealed class HubItemModel
    {
        public HubAction Action { get; set; }

        /// <summary>Chỉ có nghĩa với <see cref="HubAction.OpenSite"/>.</summary>
        public string SiteId { get; set; }

        public string Name { get; set; }
        public string StatusText { get; set; }
        public string ArtKey { get; set; }

        public bool Enabled { get; set; }
        public string DisabledReason { get; set; }

        /// <summary>Có gì đáng làm ngay: đủ điều kiện tiến hóa, đủ sức dọn slot…</summary>
        public bool Highlight { get; set; }
    }

    /// <summary>Một nhóm mục trong menu trung tâm.</summary>
    public sealed class HubSectionModel
    {
        private readonly List<HubItemModel> _items = new List<HubItemModel>();

        public string Title { get; set; }
        public string EmptyText { get; set; }

        public List<HubItemModel> Items
        {
            get { return _items; }
        }
    }

    /// <summary>
    /// Menu trung tâm: nâng cấp công trình, tiến hóa, nâng nhuệ khí.
    ///
    /// Nguyên văn Khoa (playtest-01 góp ý 7): *"Bấm trung tâm = nâng cấp công trình / tiến hóa /
    /// nâng nhuệ khí"*. Menu này là đường đi ĐỦ THẤY: slot công trình vẫn bấm thẳng được quanh
    /// hang, nhưng người chơi mới sẽ tìm thấy chúng ở đây.
    ///
    /// Mọi phán quyết vẫn của Core hoặc của ISiteSystem; chỗ này chỉ gom lại và diễn đạt.
    /// </summary>
    public sealed class HubMenuModel
    {
        private readonly List<HubSectionModel> _sections = new List<HubSectionModel>();

        public string Title { get; set; }
        public string CloseText { get; set; }

        public List<HubSectionModel> Sections
        {
            get { return _sections; }
        }

        public static HubMenuModel Build(GameSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            StringTable s = session.Strings;

            HubMenuModel model = new HubMenuModel();
            model.Title = s.Get("hub.title");
            model.CloseText = s.Get("hub.close");

            model.Sections.Add(BuildSites(session));
            model.Sections.Add(BuildEvolve(session));
            model.Sections.Add(BuildMorale(session));

            return model;
        }

        private static HubSectionModel BuildSites(GameSession session)
        {
            StringTable s = session.Strings;
            HubSectionModel section = new HubSectionModel();
            section.Title = s.Get("hub.sites.title");
            section.EmptyText = s.Get("hub.sites.empty");

            bool unlocked = session.Turn.Plan.IsUnlocked(ActivityKind.Build);
            IReadOnlyList<int> builders = session.Turn.Plan.MembersOn(ActivityKind.Build);
            IReadOnlyList<SiteView> sites = session.Sites.VisibleSites(session.State.Stage, session.Network);

            for (int i = 0; i < sites.Count; i++)
            {
                SiteView site = sites[i];

                HubItemModel item = new HubItemModel();
                item.Action = HubAction.OpenSite;
                item.SiteId = site.Id;
                item.Name = s.Get("site." + site.Id + ".name");
                item.ArtKey = site.ArtKey;
                item.Enabled = unlocked;

                double needed = site.StrengthRequired;
                if (site.Kind == SiteKind.FoodNest && site.Tier >= site.MaxTier)
                {
                    item.StatusText = s.Get("site.nest.max");
                    item.Enabled = false;
                }
                else
                {
                    item.StatusText = s.Format("site.strength", Formatting.Amount(needed));
                    item.Highlight = unlocked && session.Sites.WouldClear(site.Id, builders);
                }

                if (!unlocked)
                {
                    item.DisabledReason = TurnScreenModel.ExplainLockOf(session, ActivityKind.Build);
                }

                section.Items.Add(item);
            }

            return section;
        }

        /// <summary>
        /// Tiến hóa. Ba điều kiện của §7 nói RIÊNG từng cái, giống hệt màn mạng lưới —
        /// người chơi thiếu cổng thì đi mua nút, thiếu chiến tích thì đi đánh trận.
        /// Phán quyết "tiến hóa được chưa" lấy NeuronNetwork.CanEvolve, không tự suy.
        /// </summary>
        private static HubSectionModel BuildEvolve(GameSession session)
        {
            StringTable s = session.Strings;
            HubSectionModel section = new HubSectionModel();
            section.Title = s.Get("hub.evolve.title");
            section.EmptyText = s.Get("hub.evolve.none");

            NeuronScreenModel neuron = NeuronScreenModel.Build(session, null);
            EvolveModel evolve = neuron.Evolve;
            if (!evolve.Exists)
            {
                return section;
            }

            HubItemModel item = new HubItemModel();
            item.Action = HubAction.OpenNetwork;
            item.Name = evolve.Name;
            item.Enabled = true;
            item.Highlight = evolve.CanEvolve;
            item.StatusText = string.Join(" · ", new string[]
            {
                Mark(evolve.GatesDone) + evolve.GatesText,
                Mark(evolve.FeatsDone) + evolve.FeatsText,
                Mark(evolve.NeuronEnough) + evolve.NeuronText
            });

            section.Items.Add(item);
            return section;
        }

        private static HubSectionModel BuildMorale(GameSession session)
        {
            StringTable s = session.Strings;
            TurnPlan plan = session.Turn.Plan;

            HubSectionModel section = new HubSectionModel();
            section.Title = s.Get("hub.morale.title");

            HubItemModel item = new HubItemModel();
            item.Action = HubAction.OpenMorale;
            item.Name = TurnScreenModel.ActivityName(s, ActivityKind.Morale);
            item.Enabled = plan.IsUnlocked(ActivityKind.Morale);

            int party = plan.PartySizeOn(ActivityKind.Morale);
            item.StatusText = party == 0
                ? s.Get("activity.party.none")
                : s.Format("activity.party.some", party);

            if (!item.Enabled)
            {
                item.DisabledReason = TurnScreenModel.ExplainLockOf(session, ActivityKind.Morale);
            }
            else if (!plan.CanAfford(ActivityKind.Morale))
            {
                item.DisabledReason = s.Format(
                    "activity.unaffordable",
                    plan.EnergyCostToAdd(ActivityKind.Morale),
                    plan.EnergyRemaining);
            }

            section.Items.Add(item);
            return section;
        }

        private static string Mark(bool done)
        {
            return done ? "✓ " : "✗ ";
        }
    }
}
