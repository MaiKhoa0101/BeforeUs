using Newtonsoft.Json;

namespace BeforeUs.Core.Save
{
    /// <summary>
    /// Hình dạng file lưu ván trên đĩa. Cố ý là một DTO riêng chứ không tuần tự hóa thẳng
    /// <see cref="GameState"/>: file lưu là mặt tương thích với người chơi, nên nó phải đổi được
    /// độc lập với cách Core tổ chức dữ liệu bên trong. Đổi tên một thuộc tính trong Core
    /// không được phép làm hỏng ván đã lưu của ai đó.
    ///
    /// Tên khóa dùng snake_case cho khớp với các file JSON còn lại của dự án.
    /// </summary>
    public sealed class SaveData
    {
        [JsonProperty("version", Required = Required.Always)] public int Version { get; set; }

        /// <summary>"full" hay "mvp". Nạp ván bằng chế độ khác là sai, vì mọi con số đều khác.</summary>
        [JsonProperty("mode", Required = Required.Always)] public string Mode { get; set; }

        /// <summary>Trạng thái bộ sinh số ngẫu nhiên, dạng "xs128:…" (design.md §14).</summary>
        [JsonProperty("random_state", Required = Required.Always)] public string RandomState { get; set; }

        [JsonProperty("turn", Required = Required.Always)] public int Turn { get; set; }
        [JsonProperty("stage", Required = Required.Always)] public int Stage { get; set; }
        [JsonProperty("region", Required = Required.Always)] public int Region { get; set; }
        [JsonProperty("neuron", Required = Required.Always)] public int Neuron { get; set; }
        [JsonProperty("morale", Required = Required.Always)] public int Morale { get; set; }

        /// <summary>
        /// Số chỗ ở đã mở sẵn và trần chỗ ở của vùng hiện tại (v7 §F.2).
        /// build_level và lair capacity KHÔNG còn được lưu: cả hai là số suy ra từ các trường
        /// dưới đây, và lưu một số suy ra là cách chắc chắn nhất để hai nguồn lệch nhau.
        /// </summary>
        [JsonProperty("region_capacity_open", Required = Required.Always)] public int RegionCapacityOpen { get; set; }

        [JsonProperty("region_capacity_max", Required = Required.Always)] public int RegionCapacityMax { get; set; }

        /// <summary>Id các slot công trình đã dọn trong ván này (v7 §B).</summary>
        [JsonProperty("cleared_sites", Required = Required.Always)] public string[] ClearedSiteIds { get; set; }

        /// <summary>Bậc Ổ đựng thức ăn, 0 = ổ đất trống (v7 §B.4).</summary>
        [JsonProperty("food_nest_tier", Required = Required.Always)] public int FoodNestTier { get; set; }
        [JsonProperty("combat_wins", Required = Required.Always)] public int CombatWins { get; set; }
        [JsonProperty("hunts", Required = Required.Always)] public int Hunts { get; set; }
        [JsonProperty("migrated_ever", Required = Required.Always)] public int MigratedEver { get; set; }
        [JsonProperty("next_member_id", Required = Required.Always)] public int NextMemberId { get; set; }

        [JsonProperty("members", Required = Required.Always)] public MemberSaveData[] Members { get; set; }
        [JsonProperty("purchased_nodes", Required = Required.Always)] public string[] PurchasedNodeIds { get; set; }

        /// <summary>Loài thực vật đã khám phá TRONG VÁN NÀY (v7 §A.4). Catalog xuyên ván nằm ở file khác.</summary>
        [JsonProperty("discovered_plants", Required = Required.Always)] public string[] DiscoveredPlantIds { get; set; }
    }

    public sealed class MemberSaveData
    {
        [JsonProperty("id", Required = Required.Always)] public int Id { get; set; }
        [JsonProperty("speed", Required = Required.Always)] public double Speed { get; set; }
        [JsonProperty("strength", Required = Required.Always)] public double Strength { get; set; }
        [JsonProperty("hp", Required = Required.Always)] public int Hp { get; set; }
        [JsonProperty("traits", Required = Required.Always)] public string[] Traits { get; set; }
    }
}
