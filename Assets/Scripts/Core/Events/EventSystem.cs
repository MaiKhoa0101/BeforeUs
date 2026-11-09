using System;
using System.Collections.Generic;
using BeforeUs.Core.Config;

namespace BeforeUs.Core
{
    public enum TurnEventKind
    {
        None,

        /// <summary>Trận hướng dẫn ở lượt 3 (design.md §8).</summary>
        Tutorial,

        /// <summary>Sự kiện không combat, kết quả có lợi.</summary>
        NonCombatGood,

        /// <summary>Sự kiện không combat, kết quả có hại.</summary>
        NonCombatBad,

        /// <summary>Thú tấn công hang: phòng thủ bắt buộc.</summary>
        Raid,

        /// <summary>Cơ hội combat tùy chọn.</summary>
        Opportunity
    }

    /// <summary>Chuyện gì đã xảy ra ở lượt máy. A2 đọc để hiện hộp sự kiện; I3 ghi vào telemetry.</summary>
    public sealed class TurnEventRecord
    {
        public static readonly TurnEventRecord None = new TurnEventRecord(TurnEventKind.None, 0);

        internal TurnEventRecord(TurnEventKind kind, int turn)
        {
            Kind = kind;
            Turn = turn;
            DeadMemberIds = new List<int>();
        }

        public TurnEventKind Kind { get; }
        public int Turn { get; }

        /// <summary>Chỉ có nghĩa với Raid và Opportunity đã nhận đánh.</summary>
        public bool? CombatWon { get; internal set; }

        /// <summary>Cơ hội combat mà người chơi rút lui: không gì xảy ra (design.md §8).</summary>
        public bool Declined { get; internal set; }

        public int Food { get; internal set; }
        public int Morale { get; internal set; }
        public int Neuron { get; internal set; }

        public List<int> DeadMemberIds { get; }
    }

    /// <summary>
    /// Hệ sự kiện (design.md §8).
    ///
    /// Thứ tự rút số ngẫu nhiên, cố định để ván tái lập được:
    ///   1) một lần rút quyết "có sự kiện không" (70%);
    ///   2) một lần rút quyết loại sự kiện;
    ///   3) phần còn lại tùy loại.
    ///
    /// Lượt 3 là trận hướng dẫn và KHÔNG rút sự kiện (before_us_sim.py:97-99 dùng if/elif).
    /// </summary>
    public sealed class EventSystem : ITurnEvents
    {
        private readonly IEventDecisions _decisions;
        private readonly ICombatOdds _odds;

        public EventSystem(IEventDecisions decisions, ICombatOdds odds = null)
        {
            if (decisions == null)
            {
                throw new ArgumentNullException(nameof(decisions));
            }

            _decisions = decisions;
            _odds = odds;
            LastEvent = TurnEventRecord.None;
        }

        /// <summary>Sự kiện gần nhất đã xảy ra. Tầng Game đọc sau EndTurn để hiện lên UI.</summary>
        public TurnEventRecord LastEvent { get; private set; }

        /// <summary>
        /// design.md §2 bước 1.
        ///
        /// CHƯA CÓ GÌ Ở ĐÂY — xem câu hỏi 11 trong báo cáo C5–C6. design.md §8 nói sự kiện không combat
        /// loại "ngẫu nhiên, không cần chọn" phải hiện và áp dụng ở đầu lượt người chơi kế tiếp, còn loại
        /// "lựa chọn" thì hiện ngay ở lượt máy. Nhưng §8 không cho biết trong 40% sự kiện không combat
        /// thì bao nhiêu phần là loại nào, nên không thể cài mà không tự bịa một con số.
        /// Tạm thời mọi sự kiện không combat đều giải quyết ngay ở lượt máy, đúng như before_us_sim.py:103.
        /// Cơ chế hoãn đã sẵn ở đây; I2 có events.json sẽ biết từng sự kiện thuộc loại nào.
        /// </summary>
        public void ApplyPendingAtTurnStart(TurnEventContext context)
        {
        }

        /// <summary>design.md §2 bước 3: lượt máy, rút sự kiện.</summary>
        public void RunMachineTurn(TurnEventContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            EventsData events = context.Config.Data.Events;
            int turn = context.State.Turn;

            if (turn == events.TutorialTurn)
            {
                LastEvent = ResolveTutorial(context);
                return;
            }

            // design.md §8: lượt 1–5 không có sự kiện ngẫu nhiên.
            if (turn <= events.GraceTurns)
            {
                LastEvent = TurnEventRecord.None;
                return;
            }

            if (context.Random.NextDouble() >= events.ChancePerTurn)
            {
                LastEvent = TurnEventRecord.None;
                return;
            }

            double roll = context.Random.NextDouble();
            double raidShare = context.Phase == ClimatePhase.Dry
                ? events.ShareRaid.Dry
                : events.ShareRaid.Wet;

            if (roll < events.ShareNoncombat)
            {
                LastEvent = ResolveNonCombat(context);
            }
            else if (roll < events.ShareNoncombat + raidShare)
            {
                LastEvent = ResolveRaid(context);
            }
            else
            {
                LastEvent = ResolveOpportunity(context);
            }
        }

        private TurnEventRecord ResolveTutorial(TurnEventContext context)
        {
            TutorialRewardData reward = context.Config.Data.Events.TutorialReward;
            TurnEventRecord record = new TurnEventRecord(TurnEventKind.Tutorial, context.State.Turn);

            // design.md §8: trận combat hướng dẫn với một con vật dễ, luôn thắng, không ai chết.
            // Vẫn đi qua ICombatResolver để I1 có thể cho nó chạy trong scene combat thật.
            CombatResult result = context.CombatResolver.Resolve(
                BuildRequest(context, CombatKind.Tutorial, AllMemberIds(context)));

            record.CombatWon = result.Won;
            Award(context, record, 0, reward.Morale, reward.Neuron);
            for (int i = 0; i < reward.CombatWins; i++)
            {
                context.AddCombatWin();
            }

            Kill(context, record, result.DeadMemberIds);
            return record;
        }

        private TurnEventRecord ResolveNonCombat(TurnEventContext context)
        {
            EventsData events = context.Config.Data.Events;
            Difficulty difficulty = new Difficulty(context.Config);
            int level = context.DifficultyLevel;

            bool good = _decisions.ChooseGoodOutcome(Decision(context, 0.0));

            if (good)
            {
                TurnEventRecord record = new TurnEventRecord(TurnEventKind.NonCombatGood, context.State.Turn);
                Award(context, record, events.NoncombatGood.Food, events.NoncombatGood.Morale, 0);
                return record;
            }

            // design.md §8 + §11: xấu thì −(3 + level) thức ăn và −(2 + level) tinh thần.
            // Hệ số độ khó trong balance.json đã mang sẵn dấu âm nên CỘNG.
            TurnEventRecord bad = new TurnEventRecord(TurnEventKind.NonCombatBad, context.State.Turn);
            Award(
                context,
                bad,
                events.NoncombatBad.Food + difficulty.BadEventFoodDelta(level),
                events.NoncombatBad.Morale + difficulty.BadEventMoraleDelta(level),
                0);
            return bad;
        }

        private TurnEventRecord ResolveRaid(TurnEventContext context)
        {
            // design.md §8: phòng thủ BẮT BUỘC, cả bộ tộc tham gia.
            CombatRequest request = BuildRequest(context, CombatKind.Defence, AllMemberIds(context));
            CombatResult result = context.CombatResolver.Resolve(request);

            TurnEventRecord record = new TurnEventRecord(TurnEventKind.Raid, context.State.Turn);
            record.CombatWon = result.Won;

            if (result.Won)
            {
                RaidWinData win = context.Config.Data.Events.RaidWin;
                Award(context, record, 0, win.Morale, win.Neuron);
                context.AddCombatWin();
            }

            // Thua thì quái thú ăn quota rồi rút — số người do resolver quyết (D11 chừa lại ít nhất 1).
            Kill(context, record, result.DeadMemberIds);
            return record;
        }

        private TurnEventRecord ResolveOpportunity(TurnEventContext context)
        {
            CombatRequest request = BuildRequest(context, CombatKind.Opportunity, AllMemberIds(context));
            double estimate = _odds != null ? _odds.WinChance(request) : 0.0;

            TurnEventRecord record = new TurnEventRecord(TurnEventKind.Opportunity, context.State.Turn);

            if (!_decisions.AttackOpportunity(Decision(context, estimate)))
            {
                // design.md §8: rút lui thì không gì xảy ra.
                record.Declined = true;
                return record;
            }

            CombatResult result = context.CombatResolver.Resolve(request);
            record.CombatWon = result.Won;

            if (result.Won)
            {
                OpportunityWinData win = context.Config.Data.Events.OpportunityWin;
                Award(context, record, win.Food, win.Morale, win.Neuron);
                context.AddCombatWin();
            }

            Kill(context, record, result.DeadMemberIds);
            return record;
        }

        private EventDecisionContext Decision(TurnEventContext context, double estimatedWinChance)
        {
            return new EventDecisionContext(
                context.Config, context.State, context.Random,
                context.Phase, context.DifficultyLevel, estimatedWinChance);
        }

        private static CombatRequest BuildRequest(TurnEventContext context, CombatKind kind, int[] memberIds)
        {
            GameState state = context.State;
            return new CombatRequest(
                kind, memberIds, state.Stage, state.Region, state.BuildLevel,
                context.DifficultyLevel, state.Crew, memberIds);
        }

        private static int[] AllMemberIds(TurnEventContext context)
        {
            List<Member> members = context.State.Members;
            int[] ids = new int[members.Count];
            for (int i = 0; i < ids.Length; i++)
            {
                ids[i] = members[i].Id;
            }

            return ids;
        }

        private static void Award(TurnEventContext context, TurnEventRecord record, int food, int morale, int neuron)
        {
            record.Food += food;
            record.Morale += morale;
            record.Neuron += neuron;

            context.AddFood(food);
            context.AddMorale(morale);
            context.AddNeuron(neuron);
        }

        private static void Kill(TurnEventContext context, TurnEventRecord record, IReadOnlyList<int> deadMemberIds)
        {
            for (int i = 0; i < deadMemberIds.Count; i++)
            {
                record.DeadMemberIds.Add(deadMemberIds[i]);
            }

            context.KillMembers(deadMemberIds);
        }
    }
}
