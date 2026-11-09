using System;
using BeforeUs.Core;

namespace BeforeUs.Combat
{
    /// <summary>
    /// Người tự chơi một trận thay cho người chơi. Được gọi mỗi bước mô phỏng.
    /// Dùng cho B3 (500 trận với cách thả ngẫu nhiên) và cho bot ở C8 nếu cần.
    /// </summary>
    public interface IBattleAutoPlayer
    {
        void OnStep(BattleSimulation battle);
    }

    /// <summary>
    /// Chạy trọn một trận HEADLESS rồi trả <see cref="CombatResult"/> cho Core.
    ///
    /// ĐÂY KHÔNG PHẢI đường đi của game thật. Trong game thật, scene của Khoa cầm
    /// <see cref="BattleSimulation"/> và cho nó chạy theo khung hình, rồi đưa
    /// <see cref="BattleReport"/> qua <see cref="ICasualtyPolicy"/> — người chơi tự thả quân,
    /// không có bot nào thay. Lớp này tồn tại cho B3 và làm chỗ nối tạm cho I1 khi cần một
    /// `ICombatResolver` đồng bộ.
    ///
    /// Trận hướng dẫn (design.md §8) không vào sân: luôn thắng, không ai chết.
    /// </summary>
    public sealed class SimulatedCombatResolver : ICombatResolver
    {
        private readonly BattleSetupFactory _factory;
        private readonly ICasualtyPolicy _casualties;
        private readonly IBattleAutoPlayer _player;
        private readonly IRandom _random;

        public SimulatedCombatResolver(
            BattleSetupFactory factory,
            ICasualtyPolicy casualties,
            IBattleAutoPlayer player,
            IRandom random)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _casualties = casualties ?? throw new ArgumentNullException(nameof(casualties));
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        /// <summary>Trận gần nhất đã chạy. Dùng cho B3 để lấy lý do kết thúc và thời lượng.</summary>
        public BattleReport LastReport { get; private set; }

        public CombatResult Resolve(CombatRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            if (request.Kind == CombatKind.Tutorial)
            {
                LastReport = null;
                return CombatResult.Win();
            }

            LastReport = Play(_factory.Build(request, _random));
            return _casualties.Apply(request, LastReport);
        }

        /// <summary>Chạy một trận đã dựng sẵn. Tách ra để B3 đo riêng từng loài và từng thời lượng.</summary>
        public BattleReport Play(BattleSetup setup)
        {
            BattleSimulation battle = new BattleSimulation(setup, _random);
            battle.Start();

            int maxSteps = (int)Math.Ceiling(setup.TimeLimitSeconds / BattleSimulation.FixedStepSeconds) + 2;
            int steps = 0;

            while (!battle.IsFinished && steps < maxSteps)
            {
                _player.OnStep(battle);
                battle.StepFixed();
                steps++;
            }

            if (!battle.IsFinished)
            {
                throw new InvalidOperationException("Trận không kết thúc sau " + maxSteps + " bước.");
            }

            return battle.Report;
        }
    }

    /// <summary>
    /// Thả hết người xuống những chỗ ngẫu nhiên trong vùng thả, ngay ở bước đầu.
    /// Đúng cái HANDOFF.md §5 gọi là "tự chơi combat 500 trận với cách thả ngẫu nhiên".
    ///
    /// Cố ý ngốc: nó là ĐƯỜNG CƠ SỞ để đo, không phải bản mô phỏng người chơi giỏi. Người chơi
    /// thật sẽ thả tốt hơn, nên tỉ lệ thắng thật của người sẽ cao hơn bảng B3 — ghi nhớ điều đó
    /// khi đọc số.
    /// </summary>
    public sealed class RandomDeploymentAutoPlayer : IBattleAutoPlayer
    {
        private readonly IRandom _random;

        public RandomDeploymentAutoPlayer(IRandom random)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public void OnStep(BattleSimulation battle)
        {
            if (battle == null) throw new ArgumentNullException(nameof(battle));

            if (battle.UndeployedCount == 0)
            {
                return;
            }

            BattleField field = battle.Setup.Field;

            for (int i = 0; i < battle.Tribe.Count; i++)
            {
                Combatant member = battle.Tribe[i];
                if (member.IsDeployed)
                {
                    continue;
                }

                double x = field.DeployMinX + (_random.NextDouble() * (field.DeployMaxX - field.DeployMinX));
                double y = _random.NextDouble() * field.Height;
                battle.TryDeploy(member.Id, new CombatVector(x, y));
            }
        }
    }
}
