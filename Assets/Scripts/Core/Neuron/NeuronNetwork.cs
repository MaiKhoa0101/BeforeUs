using System;
using System.Collections.Generic;
using BeforeUs.Core.Config;
using Newtonsoft.Json;

namespace BeforeUs.Core.Neuron
{
    /// <summary>Một nút đã chốt giá theo chế độ.</summary>
    public sealed class NeuronNode
    {
        internal NeuronNode(NeuronNodeData data, int stage, NodeType type, int cost)
        {
            Data = data;
            Stage = stage;
            Type = type;
            Cost = cost;
        }

        public NeuronNodeData Data { get; }
        public int Stage { get; }
        public NodeType Type { get; }
        public int Cost { get; }

        public string Id
        {
            get { return Data.Id; }
        }

        public string Name
        {
            get { return Data.Name; }
        }

        public string Branch
        {
            get { return Data.Branch; }
        }

        public string Requires
        {
            get { return Data.Requires; }
        }
    }

    public enum PurchaseError
    {
        None,
        UnknownNode,
        AlreadyPurchased,

        /// <summary>Nút của giai đoạn khác giai đoạn hiện tại.</summary>
        WrongStage,

        /// <summary>Chưa mua nút gốc mà nút này nối vào.</summary>
        MissingPrerequisite,

        NotEnoughNeuron,

        /// <summary>Nút Tiến hóa: chưa đủ 3 nút cổng của giai đoạn (design.md §7).</summary>
        MissingGates,

        /// <summary>Nút Tiến hóa: chưa đủ chiến tích (design.md §7.1).</summary>
        MissingFeats
    }

    public sealed class PurchaseResult
    {
        public static readonly PurchaseResult Ok = new PurchaseResult(PurchaseError.None, null);

        private PurchaseResult(PurchaseError error, string message)
        {
            Error = error;
            Message = message;
        }

        public PurchaseError Error { get; }
        public string Message { get; }

        public bool Succeeded
        {
            get { return Error == PurchaseError.None; }
        }

        public static PurchaseResult Fail(PurchaseError error, string message)
        {
            return new PurchaseResult(error, message);
        }
    }

    /// <summary>
    /// Mạng lưới neuron (design.md §7). Mua nút, nút cổng, chiến tích, tiến hóa.
    ///
    /// Tổng giá nút bắt buộc mỗi giai đoạn (3 gốc + 3 cổng + Tiến hóa) đúng bằng
    /// balance.json evolution.required_neuron, nên mô hình mua-từng-nút ở đây tái tạo
    /// chi phí phẳng của before_us_sim.py mà không cần giả định thêm.
    /// BalanceConfigTests.TongGiaNutBatBuoc_KhopRequiredNeuron khóa đẳng thức đó.
    /// </summary>
    public sealed class NeuronNetwork : IPurchasedNodes
    {
        private readonly BalanceConfig _config;
        private readonly GameState _state;
        private readonly Dictionary<string, NeuronNode> _byId;
        private readonly Dictionary<int, List<NeuronNode>> _byStage;
        private readonly HashSet<string> _purchased;

        private NodeEffects _effects;

        public NeuronNetwork(NeuronData data, BalanceConfig config, GameState state, bool applyNodeEffects = false)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (state == null) throw new ArgumentNullException(nameof(state));

            _config = config;
            _state = state;
            ApplyNodeEffects = applyNodeEffects;

            _byId = new Dictionary<string, NeuronNode>(StringComparer.Ordinal);
            _byStage = new Dictionary<int, List<NeuronNode>>();

            for (int s = 0; s < data.Stages.Length; s++)
            {
                NeuronStageData stage = data.Stages[s];
                List<NeuronNode> nodes = new List<NeuronNode>();
                _byStage[stage.Stage] = nodes;

                for (int n = 0; n < stage.Nodes.Length; n++)
                {
                    NeuronNodeData nodeData = stage.Nodes[n];
                    NeuronNode node = new NeuronNode(
                        nodeData, stage.Stage, ParseType(nodeData), nodeData.Cost.For(config.Mode));

                    if (_byId.ContainsKey(node.Id))
                    {
                        throw new NeuronDataException("Trùng id nút: " + node.Id);
                    }

                    _byId[node.Id] = node;
                    nodes.Add(node);
                }
            }

            // Trạng thái đã mua sống trong GameState, nên C7 chỉ việc tuần tự hóa GameState.
            _purchased = new HashSet<string>(StringComparer.Ordinal);
            if (_state.PurchasedNodeIds != null)
            {
                for (int i = 0; i < _state.PurchasedNodeIds.Count; i++)
                {
                    string id = _state.PurchasedNodeIds[i];
                    if (!_byId.ContainsKey(id))
                    {
                        throw new NeuronDataException("Ván đã lưu có nút không tồn tại: " + id);
                    }

                    _purchased.Add(id);
                }
            }

            RecomputeEffects();
        }

        /// <summary>Cờ của Cổng 2. Tắt: mọi hiệu ứng số của nút bị bỏ qua, chỉ còn tác dụng mở khóa.</summary>
        public bool ApplyNodeEffects { get; }

        public NodeEffects Effects
        {
            get { return _effects; }
        }

        public IReadOnlyList<NeuronNode> NodesOfStage(int stage)
        {
            List<NeuronNode> nodes;
            return _byStage.TryGetValue(stage, out nodes) ? nodes : (IReadOnlyList<NeuronNode>)new NeuronNode[0];
        }

        public bool HasStage(int stage)
        {
            return _byStage.ContainsKey(stage);
        }

        public NeuronNode Node(string nodeId)
        {
            NeuronNode node;
            return nodeId != null && _byId.TryGetValue(nodeId, out node) ? node : null;
        }

        public bool Has(string nodeId)
        {
            return nodeId != null && _purchased.Contains(nodeId);
        }

        public IReadOnlyCollection<string> PurchasedIds
        {
            get { return _purchased; }
        }

        /// <summary>Đủ cả 3 nút cổng của giai đoạn chưa (design.md §7).</summary>
        public bool HasAllGates(int stage)
        {
            IReadOnlyList<NeuronNode> nodes = NodesOfStage(stage);
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].Type == NodeType.Gate && !Has(nodes[i].Id))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Đủ chiến tích của giai đoạn chưa (design.md §7.1).</summary>
        public bool HasFeats(int stage)
        {
            FeatData feats = _config.FeatsForStage(stage);
            return _state.CombatWins >= feats.CombatWins
                   && _state.Hunts >= feats.Hunts
                   && _state.MigratedEver >= feats.MigratedEver;
        }

        /// <summary>Nút Tiến hóa của giai đoạn hiện tại, hoặc null nếu giai đoạn đó chưa thiết kế.</summary>
        public NeuronNode EvolveNodeOfCurrentStage()
        {
            IReadOnlyList<NeuronNode> nodes = NodesOfStage(_state.Stage);
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].Type == NodeType.Evolve)
                {
                    return nodes[i];
                }
            }

            return null;
        }

        /// <summary>Có mở được nút Tiến hóa của giai đoạn hiện tại ngay bây giờ không.</summary>
        public bool CanEvolve
        {
            get
            {
                NeuronNode evolve = EvolveNodeOfCurrentStage();
                return evolve != null && CanBuy(evolve.Id).Succeeded;
            }
        }

        public PurchaseResult CanBuy(string nodeId)
        {
            NeuronNode node = Node(nodeId);
            if (node == null)
            {
                return PurchaseResult.Fail(PurchaseError.UnknownNode, "Không có nút " + nodeId);
            }

            if (Has(nodeId))
            {
                return PurchaseResult.Fail(PurchaseError.AlreadyPurchased, "Đã mở nút " + node.Name);
            }

            if (node.Stage != _state.Stage)
            {
                return PurchaseResult.Fail(
                    PurchaseError.WrongStage,
                    "Nút " + node.Name + " thuộc giai đoạn " + node.Stage + ", đang ở giai đoạn " + _state.Stage);
            }

            if (node.Requires != null && !Has(node.Requires))
            {
                return PurchaseResult.Fail(
                    PurchaseError.MissingPrerequisite,
                    "Phải mở " + Node(node.Requires).Name + " trước");
            }

            if (node.Type == NodeType.Evolve)
            {
                if (!HasAllGates(node.Stage))
                {
                    return PurchaseResult.Fail(PurchaseError.MissingGates, "Chưa đủ 3 nút cổng của giai đoạn");
                }

                if (!HasFeats(node.Stage))
                {
                    return PurchaseResult.Fail(PurchaseError.MissingFeats, DescribeMissingFeats(node.Stage));
                }
            }

            if (_state.Neuron < node.Cost)
            {
                return PurchaseResult.Fail(
                    PurchaseError.NotEnoughNeuron,
                    "Cần " + node.Cost + " neuron, đang có " + _state.Neuron);
            }

            return PurchaseResult.Ok;
        }

        /// <summary>
        /// Mở một nút. design.md §2: làm được bất cứ lúc nào trong lượt.
        /// Mở nút Tiến hóa thì lên giai đoạn kế ngay và chiến tích săn/thắng trận về 0
        /// (riêng "đã từng di cư" giữ nguyên, design.md §7).
        /// </summary>
        public PurchaseResult Buy(string nodeId)
        {
            PurchaseResult check = CanBuy(nodeId);
            if (!check.Succeeded)
            {
                return check;
            }

            NeuronNode node = _byId[nodeId];
            _state.Neuron -= node.Cost;
            _purchased.Add(nodeId);
            _state.PurchasedNodeIds.Add(nodeId);

            if (node.Type == NodeType.Evolve)
            {
                _state.Stage++;
                _state.CombatWins = 0;
                _state.Hunts = 0;
            }

            RecomputeEffects();
            return PurchaseResult.Ok;
        }

        private string DescribeMissingFeats(int stage)
        {
            FeatData feats = _config.FeatsForStage(stage);
            List<string> missing = new List<string>();

            if (_state.CombatWins < feats.CombatWins)
            {
                missing.Add("thắng trận " + _state.CombatWins + "/" + feats.CombatWins);
            }

            if (_state.Hunts < feats.Hunts)
            {
                missing.Add("săn thành công " + _state.Hunts + "/" + feats.Hunts);
            }

            if (_state.MigratedEver < feats.MigratedEver)
            {
                missing.Add("đã từng di cư " + _state.MigratedEver + "/" + feats.MigratedEver);
            }

            return "Chưa đủ chiến tích: " + string.Join(", ", missing.ToArray());
        }

        private void RecomputeEffects()
        {
            if (!ApplyNodeEffects)
            {
                _effects = NodeEffects.None;
                return;
            }

            // Duyệt theo THỨ TỰ MUA (List trong GameState), không duyệt HashSet: thứ tự lặp của
            // HashSet không được bảo đảm, mà có hiệu ứng dùng phép gán chứ không phải cộng dồn
            // (BirthMoraleCostOverride). Nếu sau này có hai nút cùng đặt một hiệu ứng kiểu gán,
            // duyệt HashSet sẽ cho kết quả không tất định và phá vỡ cả C7 lẫn C8.
            NodeEffects effects = new NodeEffects();
            List<string> inPurchaseOrder = _state.PurchasedNodeIds;
            for (int n = 0; n < inPurchaseOrder.Count; n++)
            {
                string id = inPurchaseOrder[n];
                NeuronNodeData data = _byId[id].Data;
                for (int i = 0; i < data.Effects.Length; i++)
                {
                    Apply(effects, data.Effects[i], id);
                }
            }

            _effects = effects;
        }

        private static void Apply(NodeEffects effects, NodeEffectData effect, string nodeId)
        {
            switch (effect.Kind)
            {
                case NodeEffectKind.GatherFlatPerMember:
                    effects.GatherFlatPerMember += effect.Value;
                    break;
                case NodeEffectKind.GatherYieldBonus:
                    effects.GatherYieldBonus += (int)effect.Value;
                    break;
                case NodeEffectKind.GatherYieldBonusWet:
                    effects.GatherYieldBonusWet += (int)effect.Value;
                    break;
                case NodeEffectKind.DryPenaltyReduction:
                    effects.DryPenaltyReduction += (int)effect.Value;
                    break;
                case NodeEffectKind.SpeedBonus:
                    effects.SpeedBonus += effect.Value;
                    break;
                case NodeEffectKind.StrengthBonus:
                    effects.StrengthBonus += effect.Value;
                    break;
                case NodeEffectKind.MoraleActivityBonus:
                    effects.MoraleActivityBonus += (int)effect.Value;
                    break;
                case NodeEffectKind.BirthMoraleCost:
                    effects.BirthMoraleCostOverride = (int)effect.Value;
                    break;
                case NodeEffectKind.StarvationForgiveOne:
                    effects.StarvationForgiveOne = true;
                    break;
                case NodeEffectKind.NewbornStatBonus:
                    effects.NewbornStatBonus += effect.Value;
                    break;
                case NodeEffectKind.DefenceOddsBonus:
                    effects.DefenceOddsBonus += effect.Value;
                    break;
                case NodeEffectKind.ExploreOddsBonus:
                    effects.ExploreOddsBonus += effect.Value;
                    break;
                case NodeEffectKind.BadEventFoodReduction:
                    effects.BadEventFoodReduction += (int)effect.Value;
                    break;
                case NodeEffectKind.RaidWarning:
                    effects.RaidWarning = true;
                    break;
                case NodeEffectKind.UnlockActivity:
                    // Mở khóa đi qua IPurchasedNodes.Has, không qua NodeEffects — xem ActivityRules.
                    break;
                default:
                    throw new NeuronDataException(
                        "Nút " + nodeId + " có hiệu ứng không nhận dạng được: " + effect.Kind);
            }
        }

        private static NodeType ParseType(NeuronNodeData data)
        {
            switch (data.Type)
            {
                case "root": return NodeType.Root;
                case "gate": return NodeType.Gate;
                case "optional": return NodeType.Optional;
                case "evolve": return NodeType.Evolve;
                default:
                    throw new NeuronDataException("Nút " + data.Id + " có type không hợp lệ: " + data.Type);
            }
        }

        public static NeuronData LoadData(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new NeuronDataException("neurons.json rỗng");
            }

            NeuronData data;
            try
            {
                data = JsonConvert.DeserializeObject<NeuronData>(json, new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Error
                });
            }
            catch (JsonException ex)
            {
                throw new NeuronDataException("neurons.json không đọc được: " + ex.Message, ex);
            }

            if (data == null)
            {
                throw new NeuronDataException("neurons.json không đọc được: kết quả rỗng");
            }

            Validate(data);
            return data;
        }

        private static readonly HashSet<string> KnownEffectKinds = new HashSet<string>(StringComparer.Ordinal)
        {
            NodeEffectKind.GatherFlatPerMember, NodeEffectKind.GatherYieldBonus,
            NodeEffectKind.GatherYieldBonusWet, NodeEffectKind.DryPenaltyReduction,
            NodeEffectKind.SpeedBonus, NodeEffectKind.StrengthBonus,
            NodeEffectKind.MoraleActivityBonus, NodeEffectKind.BirthMoraleCost,
            NodeEffectKind.StarvationForgiveOne, NodeEffectKind.NewbornStatBonus,
            NodeEffectKind.DefenceOddsBonus, NodeEffectKind.ExploreOddsBonus,
            NodeEffectKind.BadEventFoodReduction, NodeEffectKind.RaidWarning,
            NodeEffectKind.UnlockActivity
        };

        private static void Validate(NeuronData data)
        {
            HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);

            for (int s = 0; s < data.Stages.Length; s++)
            {
                NeuronStageData stage = data.Stages[s];
                int roots = 0, gates = 0, evolves = 0;

                for (int n = 0; n < stage.Nodes.Length; n++)
                {
                    NeuronNodeData node = stage.Nodes[n];
                    if (!ids.Add(node.Id))
                    {
                        throw new NeuronDataException("Trùng id nút: " + node.Id);
                    }

                    switch (ParseType(node))
                    {
                        case NodeType.Root: roots++; break;
                        case NodeType.Gate: gates++; break;
                        case NodeType.Evolve: evolves++; break;
                    }

                    // Phải kiểm ngay lúc nạp, KHÔNG đợi tới lúc áp dụng: RecomputeEffects chỉ chạy khi
                    // applyNodeEffects = true, nên nếu để đó thì một "kind" gõ sai sẽ trôi im lặng qua
                    // đúng cấu hình của Cổng 2 (cờ tắt) và chỉ lộ ra khi bật cờ.
                    for (int e = 0; e < node.Effects.Length; e++)
                    {
                        string kind = node.Effects[e].Kind;
                        if (!KnownEffectKinds.Contains(kind))
                        {
                            throw new NeuronDataException(
                                "Nút " + node.Id + " có hiệu ứng không nhận dạng được: " + kind);
                        }
                    }
                }

                // design.md §7: mỗi giai đoạn 3 nhánh, mỗi nhánh 1 gốc + 1 cổng + 1 tùy chọn, cộng 1 nút Tiến hóa.
                if (roots != 3 || gates != 3 || evolves != 1)
                {
                    throw new NeuronDataException(
                        "Giai đoạn " + stage.Stage + " phải có 3 gốc, 3 cổng, 1 Tiến hóa; có "
                        + roots + "/" + gates + "/" + evolves);
                }
            }

            // Kiểm tra requires trỏ tới nút có thật, và trỏ trong cùng giai đoạn.
            for (int s = 0; s < data.Stages.Length; s++)
            {
                NeuronStageData stage = data.Stages[s];
                HashSet<string> inStage = new HashSet<string>(StringComparer.Ordinal);
                for (int n = 0; n < stage.Nodes.Length; n++)
                {
                    inStage.Add(stage.Nodes[n].Id);
                }

                for (int n = 0; n < stage.Nodes.Length; n++)
                {
                    NeuronNodeData node = stage.Nodes[n];
                    if (node.Requires != null && !inStage.Contains(node.Requires))
                    {
                        throw new NeuronDataException(
                            "Nút " + node.Id + " yêu cầu " + node.Requires + " không có trong giai đoạn " + stage.Stage);
                    }
                }
            }
        }
    }

    public class NeuronDataException : Exception
    {
        public NeuronDataException(string message) : base(message)
        {
        }

        public NeuronDataException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
