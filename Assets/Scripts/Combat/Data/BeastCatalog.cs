using System;
using System.Collections.Generic;
using BeforeUs.Core;
using Newtonsoft.Json;

namespace BeforeUs.Combat
{
    /// <summary>Một loài đã được kiểm và dịch sang kiểu C#, sẵn sàng dựng trận.</summary>
    public sealed class BeastSpecies
    {
        internal BeastSpecies(SpeciesData data, BeastAttackStyle style, IReadOnlyList<CombatKind> roles)
        {
            Data = data;
            AttackStyle = style;
            Roles = roles;
        }

        public SpeciesData Data { get; }

        public string Id
        {
            get { return Data.Id; }
        }

        /// <summary>Chữ hiện cho người chơi.</summary>
        public string Name
        {
            get { return Data.Name; }
        }

        public BeastAttackStyle AttackStyle { get; }

        /// <summary>Loài này xuất hiện ở những loại trận nào.</summary>
        public IReadOnlyList<CombatKind> Roles { get; }

        public bool LivesIn(int region)
        {
            for (int i = 0; i < Data.Regions.Length; i++)
            {
                if (Data.Regions[i] == region)
                {
                    return true;
                }
            }

            return false;
        }

        public bool PlaysRole(CombatKind kind)
        {
            for (int i = 0; i < Roles.Count; i++)
            {
                if (Roles[i] == kind)
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Nạp và tra Assets/Data/beasts.json. Cùng lối với <c>NeuronNetwork.LoadData</c> của Core:
    /// JSON hỏng hay thiếu trường thì nổ lúc nạp, không im lặng chạy bằng số 0.
    ///
    /// Lớp này KHÔNG chứa một mẩu luật chơi nào — nó chỉ đọc dữ liệu và chọn loài theo vùng và
    /// loại trận. Tỉ lệ thắng vẫn là chuyện riêng của Core.
    /// </summary>
    public sealed class BeastCatalog
    {
        private readonly List<BeastSpecies> _species = new List<BeastSpecies>();

        public BeastCatalog(BeastData data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));

            TribeProfile = new TribeCombatProfile(
                data.TribeInBattle.MoveSpeed,
                data.TribeInBattle.AttackDamage,
                data.TribeInBattle.AttackInterval,
                data.TribeInBattle.AttackRange);

            HashSet<string> ids = new HashSet<string>();
            for (int i = 0; i < data.Species.Length; i++)
            {
                SpeciesData entry = data.Species[i];

                if (!ids.Add(entry.Id))
                {
                    throw new BeastDataException("beasts.json có hai loài cùng id: " + entry.Id);
                }

                if (entry.PackSize < 1)
                {
                    throw new BeastDataException("Loài " + entry.Id + " có pack_size < 1");
                }

                _species.Add(new BeastSpecies(entry, ParseStyle(entry), ParseRoles(entry)));
            }
        }

        public BeastData Data { get; }

        /// <summary>Chỉ số nền của thành viên trong trận, từ khối "tribe_in_battle".</summary>
        public TribeCombatProfile TribeProfile { get; }

        public IReadOnlyList<BeastSpecies> Species
        {
            get { return _species; }
        }

        public BeastSpecies Find(string speciesId)
        {
            for (int i = 0; i < _species.Count; i++)
            {
                if (string.Equals(_species[i].Id, speciesId, StringComparison.Ordinal))
                {
                    return _species[i];
                }
            }

            return null;
        }

        public BattleTemplateData TemplateFor(CombatKind kind)
        {
            switch (kind)
            {
                case CombatKind.Hunt: return Data.Battles.Hunt;
                case CombatKind.Explore: return Data.Battles.Explore;
                case CombatKind.Defence: return Data.Battles.Defence;
                case CombatKind.Opportunity: return Data.Battles.Opportunity;
                default:
                    throw new BeastDataException(
                        "Không có mẫu trận cho " + kind + ". Trận hướng dẫn (Tutorial) luôn thắng "
                        + "và không vào sân (design.md §8).");
            }
        }

        /// <summary>
        /// Chọn loài cho một trận. Ứng viên là những loài sống ở <paramref name="region"/> và
        /// nhận vai <paramref name="kind"/>; nhiều ứng viên thì bốc qua <see cref="IRandom"/>.
        ///
        /// [ĐỀ XUẤT] Khám phá đối mặt thú của VÙNG SẮP TỚI, không phải vùng đang ở: design.md §5
        /// gọi Khám phá là "chiếm vùng mới", nên kẻ cản đường hợp lý là chủ của vùng mới.
        /// Vùng kế không có loài nào nhận vai thì lùi về vùng hiện tại.
        /// </summary>
        public BeastSpecies Choose(CombatKind kind, int region, IRandom random)
        {
            if (random == null) throw new ArgumentNullException(nameof(random));

            int lookupRegion = kind == CombatKind.Explore ? region + 1 : region;

            BeastSpecies picked = ChooseIn(kind, lookupRegion, random);
            if (picked == null && kind == CombatKind.Explore)
            {
                picked = ChooseIn(kind, region, random);
            }

            if (picked == null)
            {
                throw new BeastDataException(
                    "beasts.json không có loài nào nhận vai " + kind + " ở vùng " + lookupRegion);
            }

            return picked;
        }

        private BeastSpecies ChooseIn(CombatKind kind, int region, IRandom random)
        {
            List<BeastSpecies> candidates = new List<BeastSpecies>();
            for (int i = 0; i < _species.Count; i++)
            {
                if (_species[i].LivesIn(region) && _species[i].PlaysRole(kind))
                {
                    candidates.Add(_species[i]);
                }
            }

            if (candidates.Count == 0)
            {
                return null;
            }

            // Một ứng viên thì không tiêu một lần rút — rút thừa làm chuỗi ngẫu nhiên của cả ván
            // lệch đi và ván lưu lại (C7) sẽ không khôi phục đúng.
            return candidates.Count == 1 ? candidates[0] : candidates[random.NextInt(0, candidates.Count)];
        }

        public static BeastData LoadData(string json)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));

            BeastData data;
            try
            {
                data = JsonConvert.DeserializeObject<BeastData>(json, new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Error
                });
            }
            catch (JsonException ex)
            {
                throw new BeastDataException("Không đọc được beasts.json: " + ex.Message, ex);
            }

            if (data == null)
            {
                throw new BeastDataException("beasts.json rỗng");
            }

            if (data.Species.Length == 0)
            {
                throw new BeastDataException("beasts.json không có loài nào");
            }

            return data;
        }

        private static BeastAttackStyle ParseStyle(SpeciesData entry)
        {
            switch (entry.AttackStyle)
            {
                case BeastAttackStyleName.HuntWeakest: return BeastAttackStyle.HuntWeakest;
                case BeastAttackStyleName.Random: return BeastAttackStyle.Random;
                case BeastAttackStyleName.ChallengeStrongest: return BeastAttackStyle.ChallengeStrongest;
                default:
                    throw new BeastDataException(
                        "Loài " + entry.Id + " có attack_style lạ: \"" + entry.AttackStyle
                        + "\". design.md §9 chỉ có ba kiểu: hunt_weakest, random, challenge_strongest.");
            }
        }

        private static IReadOnlyList<CombatKind> ParseRoles(SpeciesData entry)
        {
            List<CombatKind> roles = new List<CombatKind>();
            for (int i = 0; i < entry.Roles.Length; i++)
            {
                switch (entry.Roles[i])
                {
                    case BeastRoleName.Hunt: roles.Add(CombatKind.Hunt); break;
                    case BeastRoleName.Explore: roles.Add(CombatKind.Explore); break;
                    case BeastRoleName.Defence: roles.Add(CombatKind.Defence); break;
                    case BeastRoleName.Opportunity: roles.Add(CombatKind.Opportunity); break;
                    default:
                        throw new BeastDataException(
                            "Loài " + entry.Id + " có role lạ: \"" + entry.Roles[i] + "\"");
                }
            }

            if (roles.Count == 0)
            {
                throw new BeastDataException("Loài " + entry.Id + " không nhận vai trận nào");
            }

            return roles;
        }
    }

    public sealed class BeastDataException : Exception
    {
        public BeastDataException(string message) : base(message)
        {
        }

        public BeastDataException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
