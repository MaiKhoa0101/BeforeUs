using System;
using System.Collections.Generic;
using BeforeUs.Core;
using BeforeUs.Core.Config;
using BeforeUs.Core.Sites;

namespace BeforeUs.Game
{
    /// <summary>
    /// Bảng kết quả hiện sau khi bấm Kết thúc lượt, dựng từ TurnOutcome của Core.
    /// Chỉ đọc và diễn đạt lại; không cộng trừ gì ngoài việc đổi dấu để hiện cho đẹp.
    /// </summary>
    public sealed class TurnResultModel
    {
        private readonly List<string> _lines = new List<string>();

        public string Title { get; set; }

        public List<string> Lines
        {
            get { return _lines; }
        }

        public bool TribeWipedOut { get; set; }
        public string ContinueText { get; set; }

        public static TurnResultModel Build(GameSession session, TurnOutcome outcome)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (outcome == null)
            {
                throw new ArgumentNullException(nameof(outcome));
            }

            StringTable s = session.Strings;
            BalanceConfig config = session.Config;

            TurnResultModel model = new TurnResultModel();
            model.Title = s.Format("result.title", outcome.Turn);
            model.TribeWipedOut = outcome.TribeWipedOut;
            model.ContinueText = s.Get("button.continue");

            if (outcome.Activities.Count == 0)
            {
                model.Lines.Add(s.Get("result.nobody"));
            }
            else
            {
                for (int i = 0; i < outcome.Activities.Count; i++)
                {
                    model.Lines.Add(ActivityLine(s, outcome.Activities[i]));
                }
            }

            SiteResolution site = outcome.Site;
            if (site != null && site.Attempted)
            {
                // Tên slot là NỘI DUNG, nằm trong strings.vi.json — hỏi theo id, đừng đọc
                // SiteView.Name vì stub không giữ tên (và bản thật của Core cũng không nên giữ).
                string name = s.Get("site." + site.SiteId + ".name");
                model.Lines.Add(site.Cleared
                    ? s.Format("site.result.cleared", name)
                    : s.Format("site.result.failed", name,
                        Formatting.Amount(site.Required), Formatting.Amount(site.Strength)));
            }

            string eventLine = EventLine(session, outcome.Turn);
            if (eventLine != null)
            {
                model.Lines.Add(eventLine);
            }

            model.Lines.Add(s.Format("result.food", outcome.FoodTotal, outcome.FoodNeeded)
                + " (" + s.Format("result.food.detail", outcome.FoodFromActivities, outcome.FoodFromEvents) + ")");

            model.Lines.Add(outcome.Starved
                ? s.Format("result.starved", outcome.StarvationDeaths)
                : s.Get("result.fed"));

            if (outcome.NeuronGained > 0)
            {
                model.Lines.Add(s.Format("result.neuron", outcome.NeuronGained));
            }

            if (outcome.MoraleDelta > 0)
            {
                model.Lines.Add(s.Format("result.morale.up", outcome.MoraleDelta));
            }
            else if (outcome.MoraleDelta < 0)
            {
                model.Lines.Add(s.Format("result.morale.down", outcome.MoraleDelta));
            }
            else
            {
                model.Lines.Add(s.Get("result.morale.same"));
            }

            if (outcome.BuildLevelIncreased)
            {
                model.Lines.Add(s.Format("result.build", outcome.BuildLevelAfter));
            }

            if (outcome.Migrated)
            {
                model.Lines.Add(s.Format("result.migrate", TurnScreenModel.RegionName(config, outcome.RegionAfter)));
            }

            if (outcome.DeadMemberIds.Count > 0)
            {
                model.Lines.Add(s.Format("result.dead", MemberNames(s, outcome.DeadMemberIds)));
            }

            if (outcome.BornMemberIds.Count > 0)
            {
                model.Lines.Add(s.Format("result.born", MemberNames(s, outcome.BornMemberIds)));
            }

            return model;
        }

        /// <summary>
        /// Hộp sự kiện của A2: KỂ LẠI chuyện vừa xảy ra, không hỏi người chơi.
        ///
        /// Vì sao không hỏi: Core gọi IEventDecisions đồng bộ bên trong EndTurn(), nên không có chỗ
        /// nào chờ được một hộp thoại. Cơ chế bất đồng bộ quanh vòng lượt thuộc I1; dựng một vòng
        /// lặp riêng ở đây là tạo control flow thứ hai mà I1 sẽ phải gỡ.
        ///
        /// D12: hiện mọi sự kiện không combat đều giải quyết ngay trong lượt máy, nên chúng xuất
        /// hiện ở bảng kết quả lượt chứ không ở đầu lượt sau. Không dựng banner "sự kiện đầu lượt".
        /// </summary>
        private static string EventLine(GameSession session, int turn)
        {
            TurnEventRecord record = session.Events == null ? null : session.Events.LastEvent;
            if (record == null || record.Kind == TurnEventKind.None || record.Turn != turn)
            {
                return null;
            }

            StringTable s = session.Strings;
            string headline = Headline(s, record);

            List<string> parts = new List<string>();
            if (record.Food != 0)
            {
                parts.Add(s.Format("result.activity.food", record.Food));
            }

            if (record.Morale != 0)
            {
                parts.Add(s.Format("result.activity.morale", Formatting.Signed(record.Morale)));
            }

            if (record.Neuron != 0)
            {
                parts.Add(s.Format("result.activity.neuron", record.Neuron));
            }

            return parts.Count == 0
                ? headline
                : headline + " " + string.Join(" · ", parts.ToArray());
        }

        private static string Headline(StringTable s, TurnEventRecord record)
        {
            switch (record.Kind)
            {
                case TurnEventKind.Tutorial:
                    return s.Get("event.tutorial");

                case TurnEventKind.NonCombatGood:
                    return s.Get("event.noncombat.good");

                case TurnEventKind.NonCombatBad:
                    return s.Get("event.noncombat.bad");

                case TurnEventKind.Raid:
                    return s.Get(record.CombatWon == true ? "event.raid.won" : "event.raid.lost");

                default:
                    if (record.Declined)
                    {
                        // GỠ Ở I1: tới I1 người chơi mới thật sự được chọn đánh hay rút lui.
                        // Để dòng này hiện ra cho thấy quyền năng đang thiếu; thiếu mà vô hình
                        // thì sẽ lên tới bản phát hành.
                        return s.Get("event.opportunity.declined") + " " + s.Get("event.opportunity.nochoice");
                    }

                    return s.Get(record.CombatWon == true ? "event.opportunity.won" : "event.opportunity.lost");
            }
        }

        private static string ActivityLine(StringTable s, ActivityOutcome activity)
        {
            List<string> parts = new List<string>();

            if (activity.Food != 0)
            {
                parts.Add(s.Format("result.activity.food", activity.Food));
            }

            if (activity.Morale != 0)
            {
                parts.Add(s.Format("result.activity.morale", Formatting.Signed(activity.Morale)));
            }

            if (activity.Neuron != 0)
            {
                parts.Add(s.Format("result.activity.neuron", activity.Neuron));
            }

            if (activity.CombatWon != null)
            {
                parts.Add(s.Get(activity.CombatWon.Value ? "result.activity.won" : "result.activity.lost"));
            }

            string detail = parts.Count == 0 ? s.Get("result.activity.nothing") : string.Join(" · ", parts.ToArray());
            return s.Format(
                "result.activity.line",
                TurnScreenModel.ActivityName(s, activity.Kind),
                activity.PartySize,
                detail);
        }

        private static string MemberNames(StringTable s, IReadOnlyList<int> ids)
        {
            string[] names = new string[ids.Count];
            for (int i = 0; i < ids.Count; i++)
            {
                names[i] = s.Format("member.name", ids[i]);
            }

            return string.Join(", ", names);
        }
    }
}
