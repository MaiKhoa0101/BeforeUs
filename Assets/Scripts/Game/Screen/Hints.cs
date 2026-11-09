using System;
using System.Collections.Generic;
using BeforeUs.Core;

namespace BeforeUs.Game
{
    /// <summary>Chỗ một gợi ý được hiện: "cạnh thứ nó nói tới" (design.md §13).</summary>
    public enum HintAnchor
    {
        Gather,
        Morale,
        Explore,

        /// <summary>Ngay dưới banner đổi pha khí hậu.</summary>
        Climate,

        /// <summary>Thanh gợi ý chung dưới thanh thông tin: những gợi ý không neo vào một thẻ nào.</summary>
        Top
    }

    /// <summary>Một gợi ý đang hiện. Tối đa 2 câu (design.md §13).</summary>
    public sealed class HintModel
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string DismissText { get; set; }
        public HintAnchor Anchor { get; set; }
    }

    /// <summary>Một dòng trong mục tra cứu: lúc nào hiện, và nói gì.</summary>
    public sealed class HintCatalogEntry
    {
        public string Id { get; set; }
        public string When { get; set; }
        public string Text { get; set; }
        public bool Dismissed { get; set; }
    }

    /// <summary>
    /// Người chơi đã tắt gợi ý nào. Sống theo ván; lưu lại giữa các ván là việc của C7 (lưu ván),
    /// chưa có nên chưa nối.
    /// </summary>
    public sealed class HintState
    {
        private readonly HashSet<string> _dismissed = new HashSet<string>(StringComparer.Ordinal);

        /// <summary>Công tắc chung: tắt là không gợi ý nào hiện nữa (design.md §13 "tắt được").</summary>
        public bool Enabled { get; set; } = true;

        public bool IsDismissed(string id)
        {
            return _dismissed.Contains(id);
        }

        public void Dismiss(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _dismissed.Add(id);
            }
        }

        /// <summary>Hiện lại tất cả, từ mục tra cứu.</summary>
        public void ShowAllAgain()
        {
            _dismissed.Clear();
            Enabled = true;
        }
    }

    /// <summary>
    /// Bảng 7 mốc gợi ý của design.md §13. Mỗi mốc là một điều kiện đọc từ Core — không mốc nào
    /// tự tính luật: "Khám phá đã mở" hỏi TurnPlan.IsUnlocked, "pha Khô" hỏi TurnSystem.Phase,
    /// "đủ neuron" so hai con số mà BalanceConfig và GameState đưa ra.
    ///
    /// CHƯA NỐI, chờ nhánh khác:
    ///   - "Lần đầu gặp một loài": cần beasts.json của B2, chưa có dữ liệu loài nào để nói về.
    /// </summary>
    public static class HintRules
    {
        private const string Turn1 = "turn1";
        private const string Turn2 = "turn2";
        private const string ExploreOpen = "explore";
        private const string FirstDry = "dry";
        private const string EnoughNeuron = "neuron";
        private const string TutorialTurn = "tutorial";

        /// <summary>Những gợi ý nên hiện ngay lúc này.</summary>
        public static List<HintModel> ActiveHints(GameSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            List<HintModel> hints = new List<HintModel>();
            if (!session.Hints.Enabled)
            {
                return hints;
            }

            GameState state = session.State;

            Add(hints, session, Turn1, HintAnchor.Gather, state.Turn == 1);
            Add(hints, session, Turn2, HintAnchor.Morale, state.Turn == 2);
            Add(hints, session, TutorialTurn, HintAnchor.Top, state.Turn == session.Config.Data.Events.TutorialTurn);
            Add(hints, session, ExploreOpen, HintAnchor.Explore, session.Turn.Plan.IsUnlocked(ActivityKind.Explore));
            Add(hints, session, FirstDry, HintAnchor.Climate, session.Turn.Phase == ClimatePhase.Dry);
            Add(hints, session, EnoughNeuron, HintAnchor.Top, HasEnoughNeuronToSpend(session));

            return hints;
        }

        /// <summary>Mục tra cứu: xem lại mọi gợi ý, kể cả cái đã tắt (design.md §13).</summary>
        public static List<HintCatalogEntry> Catalog(GameSession session)
        {
            StringTable s = session.Strings;
            string[] ids = { Turn1, Turn2, TutorialTurn, ExploreOpen, FirstDry, EnoughNeuron };

            List<HintCatalogEntry> entries = new List<HintCatalogEntry>();
            for (int i = 0; i < ids.Length; i++)
            {
                HintCatalogEntry entry = new HintCatalogEntry();
                entry.Id = ids[i];
                entry.When = s.Get("hint." + ids[i] + ".when");
                entry.Text = s.Get("hint." + ids[i] + ".text");
                entry.Dismissed = session.Hints.IsDismissed(ids[i]);
                entries.Add(entry);
            }

            return entries;
        }

        /// <summary>
        /// "Lần đầu đủ neuron" của design.md §13: trong túi đã có đủ số neuron mà giai đoạn này
        /// đòi. Hai con số đều do Core đưa ra, chỗ này chỉ so sánh.
        /// </summary>
        private static bool HasEnoughNeuronToSpend(GameSession session)
        {
            return session.State.Neuron >= session.Config.RequiredNeuronForStage(session.State.Stage);
        }

        private static void Add(List<HintModel> hints, GameSession session, string id, HintAnchor anchor, bool condition)
        {
            if (!condition || session.Hints.IsDismissed(id))
            {
                return;
            }

            StringTable s = session.Strings;
            HintModel hint = new HintModel();
            hint.Id = id;
            hint.Anchor = anchor;
            hint.Text = s.Get("hint." + id + ".text");
            hint.DismissText = s.Get("hint.dismiss");
            hints.Add(hint);
        }
    }
}
