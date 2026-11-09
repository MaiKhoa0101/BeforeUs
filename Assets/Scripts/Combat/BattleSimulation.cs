using System;
using System.Collections.Generic;
using BeforeUs.Core;

namespace BeforeUs.Combat
{
    /// <summary>
    /// Mô phỏng một trận combat mini (design.md §9). C# thuần, không chạm UnityEngine.
    ///
    /// Cách dùng từ lớp hiển thị (Khoa, B1 phần hình và B4):
    ///     var battle = new BattleSimulation(setup, random);
    ///     battle.Start();
    ///     // mỗi khung hình:
    ///     battle.Step(Time.deltaTime);
    ///     battle.TryDeploy(memberId, new CombatVector(x, y));   // khi người chơi chạm vào sân
    ///     // rồi đọc battle.Tribe / battle.Beasts mà vẽ
    ///
    /// Tiến theo BƯỚC THỜI GIAN CỐ ĐỊNH <see cref="FixedStepSeconds"/>, không theo khung hình:
    /// cùng một seed cho cùng một trận dù máy chạy 30 hay 144 fps. Nếu không thế thì B3 đo một đằng,
    /// Khoa gặp một nẻo, và không ai hiểu vì sao.
    ///
    /// Thứ tự rút số ngẫu nhiên, cố định để trận tái lập được:
    ///   mỗi bước, theo thứ tự thú trong danh sách, mỗi con rút tối đa một lần để phá hoà khi chọn
    ///   mục tiêu. Bên bộ tộc không rút gì (luôn nhắm con thú gần nhất).
    /// </summary>
    public sealed class BattleSimulation
    {
        /// <summary>30 bước mỗi giây. Một con số duy nhất, cố ý không cho chỉnh.</summary>
        public const double FixedStepSeconds = 1.0 / 30.0;

        /// <summary>
        /// Dung sai khi so bộ dồn với một bước. Cộng dồn nhiều số thực nhỏ thì tổng hụt đi vài ulp,
        /// nên nếu so thẳng `>=` thì 60 lần Step(1/30) chỉ chạy 59 bước, còn một lần Step(2.0) chạy đủ 60 —
        /// cùng một quãng thời gian mà ra hai trận khác nhau, đúng cái bước cố định sinh ra để tránh.
        /// </summary>
        private const double StepEpsilon = 1e-9;

        private readonly IRandom _random;
        private readonly List<Combatant> _tribe = new List<Combatant>();
        private readonly List<Combatant> _beasts = new List<Combatant>();
        private readonly List<int> _deadMemberIds = new List<int>();
        private readonly List<Combatant> _reachable = new List<Combatant>();

        // Đòn bẩy (A). Chỉ được cấp phát và dùng khi BattleOptions bật giới hạn,
        // nên đường chạy mặc định không tốn thêm gì.
        private readonly HashSet<int> _mayStrike = new HashSet<int>();
        private readonly List<KeyValuePair<double, Combatant>> _queue = new List<KeyValuePair<double, Combatant>>();

        private double _accumulator;
        private int _undeployedCount;

        public BattleSimulation(BattleSetup setup, IRandom random)
        {
            if (setup == null) throw new ArgumentNullException(nameof(setup));
            if (random == null) throw new ArgumentNullException(nameof(random));

            Setup = setup;
            _random = random;

            for (int i = 0; i < setup.Tribe.Count; i++)
            {
                _tribe.Add(new Combatant(setup.Tribe[i]));
            }

            for (int i = 0; i < setup.Beasts.Count; i++)
            {
                _beasts.Add(new Combatant(setup.Beasts[i]));
            }

            _undeployedCount = _tribe.Count;
            Status = BattleStatus.Ready;
            EndReason = BattleEndReason.None;
        }

        public BattleSetup Setup { get; }

        public BattleStatus Status { get; private set; }

        /// <summary>Chỉ có nghĩa khi trận đã kết thúc.</summary>
        public BattleEndReason EndReason { get; private set; }

        public double ElapsedSeconds { get; private set; }

        public double TimeRemainingSeconds
        {
            get { return Math.Max(0.0, Setup.TimeLimitSeconds - ElapsedSeconds); }
        }

        /// <summary>Những người được cử đi, kể cả người chưa thả và người đã ngã. Lớp hiển thị đọc để vẽ.</summary>
        public IReadOnlyList<Combatant> Tribe
        {
            get { return _tribe; }
        }

        public IReadOnlyList<Combatant> Beasts
        {
            get { return _beasts; }
        }

        public bool IsFinished
        {
            get { return Status == BattleStatus.Won || Status == BattleStatus.Lost; }
        }

        /// <summary>Null cho tới khi trận kết thúc.</summary>
        public BattleReport Report { get; private set; }

        /// <summary>Còn bao nhiêu người chưa được thả xuống sân.</summary>
        public int UndeployedCount
        {
            get { return _undeployedCount; }
        }

        /// <summary>Cho đồng hồ chạy. Trước lúc này lớp hiển thị muốn giữ màn chờ bao lâu cũng được.</summary>
        public void Start()
        {
            if (Status == BattleStatus.Ready)
            {
                Status = BattleStatus.Running;
            }
        }

        /// <summary>
        /// Thả một người xuống sân (design.md §9: chạm hoặc nhấp vào bản đồ trận).
        /// Thất bại thì KHÔNG đổi gì — gọi thẳng rồi hiện lỗi, không cần kiểm tra trước.
        /// </summary>
        public DeployResult TryDeploy(int memberId, CombatVector position)
        {
            if (Status != BattleStatus.Running && Status != BattleStatus.Ready)
            {
                return DeployResult.BattleNotRunning;
            }

            Combatant member = FindTribeMember(memberId);
            if (member == null)
            {
                return DeployResult.UnknownMember;
            }

            if (member.IsDeployed)
            {
                return DeployResult.AlreadyDeployed;
            }

            if (!Setup.Field.IsInsideDeployZone(position))
            {
                return DeployResult.OutsideDeployZone;
            }

            member.IsDeployed = true;
            member.Position = position;
            _undeployedCount--;
            return DeployResult.Ok;
        }

        public Combatant FindTribeMember(int memberId)
        {
            for (int i = 0; i < _tribe.Count; i++)
            {
                if (_tribe[i].Id == memberId)
                {
                    return _tribe[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Tiến <paramref name="deltaSeconds"/> giây thời gian thật, dồn vào các bước cố định.
        /// Phần dư được giữ lại cho lần gọi sau, nên tổng thời gian không trôi.
        /// </summary>
        public void Step(double deltaSeconds)
        {
            if (deltaSeconds <= 0.0)
            {
                return;
            }

            Start();

            _accumulator += deltaSeconds;
            while (_accumulator >= FixedStepSeconds - StepEpsilon && Status == BattleStatus.Running)
            {
                _accumulator -= FixedStepSeconds;
                StepFixed();
            }
        }

        /// <summary>Đúng một bước cố định. Dùng cho test và cho B3 chạy headless.</summary>
        public void StepFixed()
        {
            Start();

            if (Status != BattleStatus.Running)
            {
                return;
            }

            ElapsedSeconds += FixedStepSeconds;

            AssignAttackSlots();

            // 1. Bộ tộc: nhắm con thú gần nhất, tới nơi thì đánh.
            for (int i = 0; i < _tribe.Count; i++)
            {
                Combatant member = _tribe[i];
                if (!member.IsOnField)
                {
                    continue;
                }

                Combatant target = NearestLivingBeast(member);
                member.TargetId = target == null ? (int?)null : target.Id;
                if (target == null)
                {
                    continue;
                }

                Advance(member, target);
            }

            // 2. Thú: LUÔN đi về đích, đánh người trong tầm trên đường đi. Cùng một đoạn mã cho cả
            //    hai kiểu trận, chỉ khác nhau ở chỗ đích nằm đầu nào của sân. Vì thú không dừng lại,
            //    thế trận người chơi dàn ra quyết định mỗi người có được BAO NHIÊU GIÂY trong tầm —
            //    đó mới là chỗ "cảm giác thả quân" (B4) có ý nghĩa, chứ không phải đứng chắn cứng.
            for (int i = 0; i < _beasts.Count; i++)
            {
                Combatant beast = _beasts[i];
                if (!beast.IsAlive)
                {
                    continue;
                }

                beast.TimeUntilNextAttack -= FixedStepSeconds;

                CollectReachableMembers(beast);
                if (_reachable.Count > 0)
                {
                    Combatant victim = TargetSelection.PickOnField(beast.Spec.AttackStyle, _reachable, _random);
                    beast.TargetId = victim == null ? (int?)null : victim.Id;
                    if (victim != null)
                    {
                        TryStrike(beast, victim);
                    }
                }
                else
                {
                    beast.TargetId = null;
                }

                // Đánh xong vẫn đi tiếp. Thú KHÔNG bao giờ bị đứng lại vì có người chắn:
                // cách duy nhất chặn nó là hạ nó. Nếu cho nó dừng lại đánh thì chỉ cần thả một người
                // ra chắn là mọi trận Phòng thủ đều thắng (hết giờ = thắng), và tỉ lệ thắng thật
                // sẽ dính trần 100% bất kể k — không còn gì để hiệu chỉnh ở B3 nữa.
                MoveTowardGoal(beast);
            }

            EvaluateEnd();
        }

        /// <summary>
        /// Chạy tới khi trận kết thúc. Dùng cho B3; lớp hiển thị dùng <see cref="Step"/>.
        /// Có trần số bước để một trận dựng sai không treo cả bộ chạy 500 trận.
        /// </summary>
        public BattleReport RunToEnd()
        {
            Start();

            int maxSteps = (int)Math.Ceiling(Setup.TimeLimitSeconds / FixedStepSeconds) + 2;
            int steps = 0;

            while (Status == BattleStatus.Running && steps < maxSteps)
            {
                StepFixed();
                steps++;
            }

            if (Status == BattleStatus.Running)
            {
                // Không nên tới được đây: EvaluateEnd luôn chốt khi hết giờ.
                throw new InvalidOperationException(
                    "Trận không kết thúc sau " + maxSteps + " bước — kiểm tra lại điều kiện kết thúc.");
            }

            return Report;
        }

        private void Advance(Combatant attacker, Combatant target)
        {
            attacker.TimeUntilNextAttack -= FixedStepSeconds;

            double distance = attacker.Position.DistanceTo(target.Position);
            if (distance > attacker.Spec.AttackRange)
            {
                attacker.Position = attacker.Position.MovedToward(
                    target.Position,
                    attacker.Spec.EffectiveMoveSpeed * FixedStepSeconds);
                return;
            }

            // Đòn bẩy (A): tới nơi rồi nhưng hết chỗ thì đứng chờ, không đánh và cũng không đi.
            if (Setup.Options.LimitsAttackers && !_mayStrike.Contains(attacker.Id))
            {
                return;
            }

            TryStrike(attacker, target);
        }

        /// <summary>
        /// Đòn bẩy (A). Mỗi con thú chỉ cho <c>MaxAttackersPerBeast</c> người GẦN NHẤT ra đòn.
        /// Chọn theo khoảng cách chứ không theo thứ tự trong danh sách: xếp theo chỉ số thì người
        /// id nhỏ luôn được ưu tiên, một thiên vị không ai nhìn ra.
        ///
        /// Tắt (mặc định) thì hàm này thoát ngay và không đụng gì.
        /// </summary>
        private void AssignAttackSlots()
        {
            if (!Setup.Options.LimitsAttackers)
            {
                return;
            }

            _mayStrike.Clear();
            int cap = Setup.Options.MaxAttackersPerBeast;

            for (int b = 0; b < _beasts.Count; b++)
            {
                Combatant beast = _beasts[b];
                if (!beast.IsAlive)
                {
                    continue;
                }

                _queue.Clear();
                for (int i = 0; i < _tribe.Count; i++)
                {
                    Combatant member = _tribe[i];
                    if (!member.IsOnField)
                    {
                        continue;
                    }

                    if (!ReferenceEquals(NearestLivingBeast(member), beast))
                    {
                        continue;
                    }

                    double distance = member.Position.DistanceTo(beast.Position);
                    if (distance <= member.Spec.AttackRange)
                    {
                        _queue.Add(new KeyValuePair<double, Combatant>(distance, member));
                    }
                }

                _queue.Sort(CompareByDistanceThenId);

                int slots = Math.Min(cap, _queue.Count);
                for (int i = 0; i < slots; i++)
                {
                    _mayStrike.Add(_queue[i].Value.Id);
                }
            }
        }

        private static int CompareByDistanceThenId(
            KeyValuePair<double, Combatant> left, KeyValuePair<double, Combatant> right)
        {
            int byDistance = left.Key.CompareTo(right.Key);
            return byDistance != 0 ? byDistance : left.Value.Id.CompareTo(right.Value.Id);
        }

        private void TryStrike(Combatant attacker, Combatant target)
        {
            if (attacker.Position.DistanceTo(target.Position) > attacker.Spec.AttackRange)
            {
                return;
            }

            if (attacker.TimeUntilNextAttack > 0.0)
            {
                return;
            }

            attacker.TimeUntilNextAttack += attacker.Spec.EffectiveAttackInterval;
            attacker.LastAttackAtSeconds = ElapsedSeconds;

            target.Hp -= attacker.Spec.EffectiveAttackDamage;
            if (target.Hp > 0.0)
            {
                return;
            }

            target.Hp = 0.0;
            target.DiedAtSeconds = ElapsedSeconds;

            // design.md §9: người chết trong trận là chết thật, kể cả khi thắng.
            if (target.Side == CombatSide.Tribe)
            {
                _deadMemberIds.Add(target.Id);
            }
        }

        private void MoveTowardGoal(Combatant beast)
        {
            // Thú chỉ chạy dọc trục x về đích; trục y giữ nguyên, nên thế trận do người chơi dàn ra
            // mới là thứ quyết định ai chắn được đường.
            CombatVector goal = new CombatVector(Setup.Field.GoalX, beast.Position.Y);
            beast.Position = beast.Position.MovedToward(goal, beast.Spec.EffectiveMoveSpeed * FixedStepSeconds);
        }

        private void CollectReachableMembers(Combatant beast)
        {
            _reachable.Clear();

            for (int i = 0; i < _tribe.Count; i++)
            {
                Combatant member = _tribe[i];
                if (member.IsOnField && beast.Position.DistanceTo(member.Position) <= beast.Spec.AttackRange)
                {
                    _reachable.Add(member);
                }
            }
        }

        private Combatant NearestLivingBeast(Combatant member)
        {
            Combatant nearest = null;
            double best = double.MaxValue;

            for (int i = 0; i < _beasts.Count; i++)
            {
                Combatant beast = _beasts[i];
                if (!beast.IsAlive)
                {
                    continue;
                }

                double distance = member.Position.DistanceTo(beast.Position);
                if (distance < best)
                {
                    best = distance;
                    nearest = beast;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Thứ tự kiểm tra có chủ đích: hạ hết thú (thắng) → thú tới đích (thua) → hết người (thua)
        /// → hết giờ. Đảo hai mục đầu sẽ biến đòn kết liễu đúng lúc thú chạm vạch thành một trận thua.
        /// </summary>
        private void EvaluateEnd()
        {
            int beastsDown = 0;
            for (int i = 0; i < _beasts.Count; i++)
            {
                if (!_beasts[i].IsAlive)
                {
                    beastsDown++;
                }
            }

            if (beastsDown == _beasts.Count)
            {
                Finish(true, BattleEndReason.AllBeastsDown, beastsDown);
                return;
            }

            for (int i = 0; i < _beasts.Count; i++)
            {
                Combatant beast = _beasts[i];
                if (beast.IsAlive && BattleSetup.ReachedGoal(Setup.Kind, Setup.Field, beast.Position.X))
                {
                    Finish(
                        false,
                        Setup.Kind == BattleKind.Lure ? BattleEndReason.BeastEscaped : BattleEndReason.TargetReached,
                        beastsDown);
                    return;
                }
            }

            if (_undeployedCount == 0 && CountDeployedAlive() == 0)
            {
                Finish(false, BattleEndReason.TribeDown, beastsDown);
                return;
            }

            if (ElapsedSeconds >= Setup.TimeLimitSeconds)
            {
                // Dụ và hạ: hết giờ nghĩa là con thú đã chạy khỏi khu vực → thua (design.md §9).
                // Phòng thủ: §9 chỉ cho đúng MỘT điều kiện thua là "kẻ địch chạm mục tiêu".
                // Chặn được tới hết giờ chính là đạt mục tiêu "chặn kẻ địch trước khi tới hang" → thắng.
                bool won = Setup.Kind == BattleKind.Prevent;
                Finish(won, BattleEndReason.TimeLimit, beastsDown);
            }
        }

        private int CountDeployedAlive()
        {
            int alive = 0;
            for (int i = 0; i < _tribe.Count; i++)
            {
                if (_tribe[i].IsOnField)
                {
                    alive++;
                }
            }

            return alive;
        }

        private void Finish(bool won, BattleEndReason reason, int beastsDown)
        {
            Status = won ? BattleStatus.Won : BattleStatus.Lost;
            EndReason = reason;

            Report = new BattleReport(
                won,
                reason,
                ElapsedSeconds,
                _deadMemberIds.ToArray(),
                beastsDown,
                _beasts.Count,
                _tribe.Count - _undeployedCount,
                _tribe.Count,
                Setup.AttackStyle,
                Setup.Kind);
        }
    }
}
