using System;
using System.Collections.Generic;
using BeforeUs.Core;

namespace BeforeUs.Combat
{
    /// <summary>
    /// Dựng <see cref="BattleSetup"/> từ `CombatRequest` của Core cộng số liệu beasts.json.
    /// Đây là cây cầu duy nhất giữa vòng lượt và sân trận.
    ///
    /// Không chứa một mẩu công thức §5.2/§8 nào: nó chỉ đổi trạng thái ván thành chỉ số đơn vị.
    /// Công trình và giai đoạn làm bộ tộc mạnh lên, độ khó và vùng xa làm thú mạnh lên — đó là
    /// cách trận THẬT thể hiện những thứ mà công thức tính bằng cộng điểm phần trăm. Hai bên có
    /// gặp nhau hay không là câu hỏi của B3, và phải được ĐO chứ không được giả định.
    /// </summary>
    public sealed class BattleSetupFactory
    {
        private readonly BeastCatalog _catalog;
        private readonly IMemberSource _members;

        public BattleSetupFactory(BeastCatalog catalog, IMemberSource members)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _members = members ?? throw new ArgumentNullException(nameof(members));
        }

        public BattleSetup Build(CombatRequest request, IRandom random)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (random == null) throw new ArgumentNullException(nameof(random));

            if (request.Kind == CombatKind.Tutorial)
            {
                throw new ArgumentException(
                    "Trận hướng dẫn không vào sân: design.md §8 cho nó luôn thắng, không ai chết.",
                    nameof(request));
            }

            BeastSpecies species = _catalog.Choose(request.Kind, request.Region, random);
            return Build(request, species, random);
        }

        /// <summary>Dựng với một loài chỉ định sẵn. Dùng cho test và cho B3 khi cần đo riêng từng loài.</summary>
        public BattleSetup Build(CombatRequest request, BeastSpecies species, IRandom random)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (species == null) throw new ArgumentNullException(nameof(species));
            if (random == null) throw new ArgumentNullException(nameof(random));

            BattleTemplateData template = _catalog.TemplateFor(request.Kind);
            BattleField field = ToField(template.Field);
            LeversData levers = _catalog.Data.Levers;

            return new BattleSetup(
                ParseBattleKind(template.BattleKind),
                field,
                BuildTribe(request),
                BuildBeasts(request, species, template, field, levers, random),
                template.TimeLimitSeconds,
                new BattleOptions(levers.MaxAttackersPerBeast));
        }

        private IReadOnlyList<CombatantSpec> BuildTribe(CombatRequest request)
        {
            ScalingData scaling = _catalog.Data.Scaling;

            // Giai đoạn đếm từ 1, nên giai đoạn 1 không được cộng gì.
            double damageMultiplier = 1.0
                                      + (scaling.TribeDamagePerBuildLevel * request.BuildLevel)
                                      + (scaling.TribeDamagePerStage * Math.Max(0, request.Stage - 1));

            TribeCombatProfile baseProfile = _catalog.TribeProfile;
            TribeCombatProfile profile = new TribeCombatProfile(
                baseProfile.MoveSpeed,
                baseProfile.AttackDamage * damageMultiplier,
                baseProfile.AttackInterval,
                baseProfile.AttackRange);

            List<CombatantSpec> tribe = new List<CombatantSpec>();
            IReadOnlyList<int> ids = request.MemberIds;

            for (int i = 0; i < ids.Count; i++)
            {
                Member member = _members.Find(ids[i]);
                if (member == null)
                {
                    throw new InvalidOperationException(
                        "Không tra được thành viên #" + ids[i] + " khi dựng trận. "
                        + "IMemberSource phải phủ hết CombatRequest.MemberIds.");
                }

                tribe.Add(CombatantSpec.ForMember(member, profile));
            }

            return tribe;
        }

        private IReadOnlyList<CombatantSpec> BuildBeasts(
            CombatRequest request,
            BeastSpecies species,
            BattleTemplateData template,
            BattleField field,
            LeversData levers,
            IRandom random)
        {
            ScalingData scaling = _catalog.Data.Scaling;
            BeastStatsData stats = species.Data.Stats;

            double hpMultiplier = 1.0
                                  + (scaling.BeastHpPerDifficultyLevel * request.DifficultyLevel)
                                  + (scaling.BeastHpPerRegion * request.Region);
            double damageMultiplier = 1.0 + (scaling.BeastDamagePerDifficultyLevel * request.DifficultyLevel);

            int packSize = species.Data.PackSize;
            List<CombatantSpec> beasts = new List<CombatantSpec>();

            for (int i = 0; i < packSize; i++)
            {
                // ĐÒN BẨY (B), mặc định tắt (biên độ 0 ⇒ hệ số đúng bằng 1 và không rút số nào).
                // Phương sai lớn hơn làm vùng chuyển tiếp của đường cong rộng ra, tức thoải hơn,
                // mà KHÔNG đổi giá trị trung bình.
                double hpJitter = Jitter(levers.BeastStatVariance, random);
                double damageJitter = Jitter(levers.BeastStatVariance, random);
                double spawnX = template.SpawnX + Offset(levers.BeastSpawnXJitter, random);

                beasts.Add(new CombatantSpec(
                    i,
                    CombatSide.Beast,
                    1.0,
                    1.0,
                    stats.Hp * hpMultiplier * hpJitter,
                    stats.MoveSpeed,
                    stats.AttackDamage * damageMultiplier * damageJitter,
                    stats.AttackInterval,
                    stats.AttackRange,
                    species.AttackStyle,
                    species.Id,
                    new CombatVector(spawnX, SpawnY(field, template, i, packSize))));
            }

            return beasts;
        }

        /// <summary>Hệ số nhân trong [1 − biên độ, 1 + biên độ]. Biên độ 0 thì không rút số nào.</summary>
        private static double Jitter(double amplitude, IRandom random)
        {
            return amplitude <= 0.0 ? 1.0 : 1.0 + (((random.NextDouble() * 2.0) - 1.0) * amplitude);
        }

        /// <summary>Lượng cộng trong [−biên độ, +biên độ]. Biên độ 0 thì không rút số nào.</summary>
        private static double Offset(double amplitude, IRandom random)
        {
            return amplitude <= 0.0 ? 0.0 : ((random.NextDouble() * 2.0) - 1.0) * amplitude;
        }

        /// <summary>
        /// Bầy trải đều trên trục y quanh giữa sân. Cố ý KHÔNG ngẫu nhiên: vị trí sinh của thú là
        /// thứ người chơi nhìn thấy và phản ứng lại, nên nó phải đọc được, không phải đoán.
        /// </summary>
        private static double SpawnY(BattleField field, BattleTemplateData template, int index, int packSize)
        {
            double middle = field.Height / 2.0;
            if (packSize <= 1)
            {
                return middle;
            }

            double step = template.SpawnSpreadY / (packSize - 1);
            double y = middle - (template.SpawnSpreadY / 2.0) + (step * index);
            return Math.Max(0.0, Math.Min(field.Height, y));
        }

        private static BattleField ToField(FieldData data)
        {
            return new BattleField(
                data.Width, data.Height, data.GoalX, data.GoalRadius, data.DeployMinX, data.DeployMaxX);
        }

        private static BattleKind ParseBattleKind(string name)
        {
            switch (name)
            {
                case BattleKindName.Lure: return BattleKind.Lure;
                case BattleKindName.Prevent: return BattleKind.Prevent;
                default:
                    throw new BeastDataException(
                        "battle_kind lạ: \"" + name + "\". design.md §9 chỉ có hai kiểu trận: lure, prevent.");
            }
        }
    }
}
