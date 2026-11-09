using System;
using System.Collections.Generic;
using BeforeUs.Core.Config;
using Newtonsoft.Json;

namespace BeforeUs.Core.Save
{
    /// <summary>Ván đã tải xong, sẵn sàng dựng lại TurnSystem.</summary>
    public sealed class LoadedGame
    {
        internal LoadedGame(GameState state, XorShift128Random random, GameMode mode)
        {
            State = state;
            Random = random;
            Mode = mode;
        }

        public GameState State { get; }

        /// <summary>Bộ sinh số đã khôi phục đúng chỗ cũ. Dùng CHUNG một thể hiện này cho
        /// TurnSystem, EventSystem và ProbabilisticCombatResolver.</summary>
        public XorShift128Random Random { get; }

        public GameMode Mode { get; }
    }

    public class SaveGameException : Exception
    {
        public SaveGameException(string message) : base(message)
        {
        }

        public SaveGameException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    /// <summary>
    /// Lưu và tải ván (design.md §14, HANDOFF C7). C# thuần, không chạm hệ thống file:
    /// lớp này chỉ đổi giữa trạng thái ván và một chuỗi JSON. Tầng Unity quyết định cất chuỗi đó
    /// ở đâu — cần thế để chạy được cả trên WebGL, nơi System.IO gần như vô dụng.
    ///
    /// CHỈ LƯU GIỮA HAI LƯỢT, sau khi <see cref="TurnSystem.EndTurn"/> trả về.
    /// Dự định trong lượt (TurnPlan) là trạng thái tạm và cố ý KHÔNG được lưu: theo D2 nó chưa
    /// phải là gì cả cho tới khi người chơi bấm kết thúc lượt.
    /// </summary>
    public static class SaveGame
    {
        /// <summary>
        /// Tăng khi hình dạng file lưu đổi theo cách không đọc ngược được.
        /// 1 → 2 (2026-11-08, v7): thêm `discovered_plants`. Ván lưu ở bản 1 không đọc được nữa.
        /// </summary>
        public const int CurrentVersion = 3;

        private static readonly string[] NoTraits = new string[0];

        public static string Write(GameState state, IRandom random, GameMode mode, bool indented = true)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (random == null) throw new ArgumentNullException(nameof(random));

            SaveData data = new SaveData
            {
                Version = CurrentVersion,
                Mode = mode == GameMode.Mvp ? "mvp" : "full",
                RandomState = random.SaveState(),
                Turn = state.Turn,
                Stage = state.Stage,
                Region = state.Region,
                Neuron = state.Neuron,
                Morale = state.Morale,
                RegionCapacityOpen = state.RegionCapacityOpen,
                RegionCapacityMax = state.RegionCapacityMax,
                ClearedSiteIds = state.ClearedSiteIds.ToArray(),
                FoodNestTier = state.FoodNestTier,
                CombatWins = state.CombatWins,
                Hunts = state.Hunts,
                MigratedEver = state.MigratedEver,
                NextMemberId = state.NextMemberId,
                Members = WriteMembers(state.Members),
                PurchasedNodeIds = state.PurchasedNodeIds.ToArray(),
                DiscoveredPlantIds = state.DiscoveredPlantIds.ToArray()
            };

            return JsonConvert.SerializeObject(data, indented ? Formatting.Indented : Formatting.None);
        }

        /// <summary>
        /// Tải ván. <paramref name="config"/> chỉ dùng để kiểm chế độ có khớp không —
        /// nạp ván MVP bằng số liệu bản đầy đủ sẽ ra một ván khác hẳn, nên chặn ngay chứ không để trôi.
        /// </summary>
        public static LoadedGame Read(string json, BalanceConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            SaveData data = ReadData(json);

            if (data.Version != CurrentVersion)
            {
                throw new SaveGameException(
                    "Ván đã lưu ở phiên bản " + data.Version + ", bản hiện tại là " + CurrentVersion);
            }

            GameMode mode = GameModeParser.Parse(data.Mode);
            if (mode != config.Mode)
            {
                throw new SaveGameException(
                    "Ván đã lưu ở chế độ " + mode + " nhưng số liệu đang nạp là " + config.Mode);
            }

            XorShift128Random random = new XorShift128Random(0);
            try
            {
                random.LoadState(data.RandomState);
            }
            catch (ArgumentException ex)
            {
                throw new SaveGameException("Trạng thái ngẫu nhiên trong ván đã lưu không đọc được: " + ex.Message, ex);
            }

            GameState state = new GameState
            {
                Turn = data.Turn,
                Stage = data.Stage,
                Region = data.Region,
                Neuron = data.Neuron,
                Morale = data.Morale,
                RegionCapacityOpen = data.RegionCapacityOpen,
                RegionCapacityMax = data.RegionCapacityMax,
                FoodNestTier = data.FoodNestTier,
                CombatWins = data.CombatWins,
                Hunts = data.Hunts,
                MigratedEver = data.MigratedEver,
                NextMemberId = data.NextMemberId
            };

            ReadMembers(data, state);

            for (int i = 0; i < data.PurchasedNodeIds.Length; i++)
            {
                state.PurchasedNodeIds.Add(data.PurchasedNodeIds[i]);
            }

            for (int i = 0; i < data.ClearedSiteIds.Length; i++)
            {
                state.ClearedSiteIds.Add(data.ClearedSiteIds[i]);
            }

            for (int i = 0; i < data.DiscoveredPlantIds.Length; i++)
            {
                state.DiscoveredPlantIds.Add(data.DiscoveredPlantIds[i]);
            }

            Validate(state);
            return new LoadedGame(state, random, mode);
        }

        private static SaveData ReadData(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new SaveGameException("Ván đã lưu rỗng");
            }

            SaveData data;
            try
            {
                data = JsonConvert.DeserializeObject<SaveData>(json, new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Error,
                    FloatParseHandling = FloatParseHandling.Double
                });
            }
            catch (JsonException ex)
            {
                throw new SaveGameException("Ván đã lưu không đọc được: " + ex.Message, ex);
            }

            if (data == null)
            {
                throw new SaveGameException("Ván đã lưu không đọc được: kết quả rỗng");
            }

            return data;
        }

        private static MemberSaveData[] WriteMembers(List<Member> members)
        {
            MemberSaveData[] saved = new MemberSaveData[members.Count];
            for (int i = 0; i < members.Count; i++)
            {
                Member member = members[i];
                string[] traits = new string[member.Traits.Count];
                for (int t = 0; t < traits.Length; t++)
                {
                    traits[t] = member.Traits[t];
                }

                saved[i] = new MemberSaveData
                {
                    Id = member.Id,
                    Speed = member.Speed,
                    Strength = member.Strength,
                    Hp = member.Hp,
                    Traits = traits
                };
            }

            return saved;
        }

        private static void ReadMembers(SaveData data, GameState state)
        {
            for (int i = 0; i < data.Members.Length; i++)
            {
                MemberSaveData saved = data.Members[i];
                try
                {
                    state.Members.Add(new Member(
                        saved.Id, saved.Speed, saved.Strength, saved.Hp, saved.Traits ?? NoTraits));
                }
                catch (ArgumentException ex)
                {
                    throw new SaveGameException("Thành viên trong ván đã lưu không hợp lệ: " + ex.Message, ex);
                }
            }
        }

        private static void Validate(GameState state)
        {
            if (state.Turn < 1)
            {
                throw new SaveGameException("Lượt trong ván đã lưu phải >= 1, nhận được " + state.Turn);
            }

            if (state.Stage < 1)
            {
                throw new SaveGameException("Giai đoạn trong ván đã lưu phải >= 1, nhận được " + state.Stage);
            }

            HashSet<int> ids = new HashSet<int>();
            for (int i = 0; i < state.Members.Count; i++)
            {
                int id = state.Members[i].Id;
                if (!ids.Add(id))
                {
                    throw new SaveGameException("Ván đã lưu có hai thành viên cùng id " + id);
                }

                if (id >= state.NextMemberId)
                {
                    // Nếu lọt qua, người sinh tiếp theo sẽ trùng id với người đang sống,
                    // và combat trả về deadMemberIds sẽ giết nhầm người.
                    throw new SaveGameException(
                        "Ván đã lưu có thành viên id " + id + " >= next_member_id " + state.NextMemberId);
                }
            }
        }
    }
}
