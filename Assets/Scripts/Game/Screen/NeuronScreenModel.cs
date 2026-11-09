using System;
using System.Collections.Generic;
using BeforeUs.Core;
using BeforeUs.Core.Config;
using BeforeUs.Core.Neuron;

namespace BeforeUs.Game
{
    /// <summary>Một nút trên màn mạng lưới neuron.</summary>
    public sealed class NeuronNodeModel
    {
        public string Id { get; set; }

        /// <summary>Tên nút, lấy nguyên văn từ neurons.json (design.md §7.3–7.4). Không đặt lại tên ở đây.</summary>
        public string Name { get; set; }

        public string TypeText { get; set; }
        public string CostText { get; set; }
        public string EffectText { get; set; }

        public bool Purchased { get; set; }

        /// <summary>NeuronNetwork.CanBuy nói được hay không. Tầng này không so neuron với giá nút.</summary>
        public bool Buyable { get; set; }

        /// <summary>Vì sao chưa mua được. Null khi mua được hoặc đã mua.</summary>
        public string BlockReason { get; set; }
    }

    public sealed class NeuronBranchModel
    {
        private readonly List<NeuronNodeModel> _nodes = new List<NeuronNodeModel>();

        public string Id { get; set; }
        public string Name { get; set; }

        public List<NeuronNodeModel> Nodes
        {
            get { return _nodes; }
        }
    }

    /// <summary>
    /// Nút Tiến hóa. design.md §7 có HAI điều kiện độc lập — đủ 3 nút cổng VÀ đủ chiến tích —
    /// nên màn hình phải nói riêng từng cái. Gộp thành "chưa tiến hóa được" là bỏ rơi người chơi
    /// giữa hai con đường khác hẳn nhau: một bên đi mua nút, một bên đi đánh trận.
    /// </summary>
    public sealed class EvolveModel
    {
        private readonly List<string> _missingGates = new List<string>();
        private readonly List<string> _missingFeats = new List<string>();

        public bool Exists { get; set; }
        public string Name { get; set; }
        public string CostText { get; set; }

        /// <summary>NeuronNetwork.CanEvolve. Không tự suy từ ba điều kiện bên dưới.</summary>
        public bool CanEvolve { get; set; }

        public bool GatesDone { get; set; }
        public string GatesText { get; set; }

        public List<string> MissingGates
        {
            get { return _missingGates; }
        }

        public bool FeatsDone { get; set; }
        public string FeatsText { get; set; }

        public List<string> MissingFeats
        {
            get { return _missingFeats; }
        }

        public bool NeuronEnough { get; set; }
        public string NeuronText { get; set; }
    }

    /// <summary>
    /// Màn mạng lưới neuron (A3, design.md §7). Ba nhánh, trạng thái nút, chiến tích, nút Tiến hóa.
    ///
    /// Mọi phán quyết "mua được chưa / tiến hóa được chưa" đều hỏi NeuronNetwork của Core.
    /// Chỗ này chỉ dựng câu chữ giải thích, và chỉ đọc những con số Core đã đưa ra.
    /// </summary>
    public sealed class NeuronScreenModel
    {
        private readonly List<NeuronBranchModel> _branches = new List<NeuronBranchModel>();

        public string Title { get; set; }
        public string StageText { get; set; }
        public string NeuronText { get; set; }
        public string FeatsSummary { get; set; }
        public string CloseText { get; set; }

        /// <summary>Giai đoạn hiện tại chưa được thiết kế nút (giai đoạn 3–4, ngoài MVP).</summary>
        public bool StageHasNoNodes { get; set; }
        public string StageHasNoNodesText { get; set; }

        public List<NeuronBranchModel> Branches
        {
            get { return _branches; }
        }

        public EvolveModel Evolve { get; set; }

        /// <summary>Lỗi của lần bấm mua gần nhất, hoặc null.</summary>
        public string Message { get; set; }

        public static NeuronScreenModel Build(GameSession session, PurchaseResult lastResult)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            StringTable s = session.Strings;
            NeuronNetwork network = session.Network;
            GameState state = session.State;
            int stage = state.Stage;

            NeuronScreenModel model = new NeuronScreenModel();
            model.Title = s.Get("neuron.title");
            model.StageText = s.Format("neuron.stage", stage, StageName(session, stage));
            model.NeuronText = s.Format("neuron.have", state.Neuron);
            model.FeatsSummary = s.Format(
                "neuron.feats.summary",
                state.CombatWins,
                state.Hunts,
                state.MigratedEver);
            model.CloseText = s.Get("neuron.close");
            model.StageHasNoNodesText = s.Get("neuron.stage.nonodes");

            if (lastResult != null && !lastResult.Succeeded)
            {
                model.Message = ExplainPurchaseError(s, lastResult.Error);
            }

            IReadOnlyList<NeuronNode> nodes = network.NodesOfStage(stage);
            model.StageHasNoNodes = nodes.Count == 0;

            BuildBranches(model, session, nodes);
            model.Evolve = BuildEvolve(session, stage);

            return model;
        }

        private static void BuildBranches(NeuronScreenModel model, GameSession session, IReadOnlyList<NeuronNode> nodes)
        {
            StringTable s = session.Strings;
            NeuronNetwork network = session.Network;
            Dictionary<string, NeuronBranchModel> byBranch = new Dictionary<string, NeuronBranchModel>(StringComparer.Ordinal);

            for (int i = 0; i < nodes.Count; i++)
            {
                NeuronNode node = nodes[i];
                if (node.Type == NodeType.Evolve)
                {
                    continue; // Nút Tiến hóa có ô riêng, không nằm trong nhánh nào.
                }

                NeuronBranchModel branch;
                if (!byBranch.TryGetValue(node.Branch ?? string.Empty, out branch))
                {
                    branch = new NeuronBranchModel();
                    branch.Id = node.Branch;
                    branch.Name = BranchName(session, node.Branch);
                    byBranch[node.Branch ?? string.Empty] = branch;
                    model.Branches.Add(branch);
                }

                PurchaseResult check = network.CanBuy(node.Id);

                NeuronNodeModel item = new NeuronNodeModel();
                item.Id = node.Id;
                item.Name = node.Name;
                item.TypeText = TypeText(s, node.Type);
                item.CostText = s.Format("neuron.cost", node.Cost);
                item.EffectText = node.Data.EffectText;
                item.Purchased = network.Has(node.Id);
                item.Buyable = check.Succeeded;
                item.BlockReason = item.Purchased || item.Buyable
                    ? null
                    : BlockReason(session, node, check);

                branch.Nodes.Add(item);
            }
        }

        private static EvolveModel BuildEvolve(GameSession session, int stage)
        {
            StringTable s = session.Strings;
            NeuronNetwork network = session.Network;
            GameState state = session.State;

            EvolveModel evolve = new EvolveModel();
            NeuronNode node = network.EvolveNodeOfCurrentStage();
            if (node == null)
            {
                evolve.Exists = false;
                return evolve;
            }

            evolve.Exists = true;
            evolve.Name = node.Name;
            evolve.CostText = s.Format("neuron.cost", node.Cost);
            evolve.CanEvolve = network.CanEvolve;

            // Điều kiện 1: ba nút cổng (design.md §7).
            evolve.GatesDone = network.HasAllGates(stage);
            IReadOnlyList<NeuronNode> nodes = network.NodesOfStage(stage);
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].Type == NodeType.Gate && !network.Has(nodes[i].Id))
                {
                    evolve.MissingGates.Add(nodes[i].Name);
                }
            }

            evolve.GatesText = evolve.GatesDone
                ? s.Get("neuron.evolve.gates.done")
                : s.Format("neuron.evolve.gates.missing", string.Join(", ", evolve.MissingGates.ToArray()));

            // Điều kiện 2: chiến tích (design.md §7.1). Độc lập hẳn với điều kiện 1.
            evolve.FeatsDone = network.HasFeats(stage);
            FeatData feats = session.Config.FeatsForStage(stage);
            AddMissingFeat(evolve, s, "neuron.feat.wins", state.CombatWins, feats.CombatWins);
            AddMissingFeat(evolve, s, "neuron.feat.hunts", state.Hunts, feats.Hunts);
            AddMissingFeat(evolve, s, "neuron.feat.migrated", state.MigratedEver, feats.MigratedEver);

            evolve.FeatsText = evolve.FeatsDone
                ? s.Get("neuron.evolve.feats.done")
                : s.Format("neuron.evolve.feats.missing", string.Join(", ", evolve.MissingFeats.ToArray()));

            // Điều kiện 3: đủ neuron để trả giá nút Tiến hóa.
            evolve.NeuronEnough = state.Neuron >= node.Cost;
            evolve.NeuronText = evolve.NeuronEnough
                ? s.Get("neuron.evolve.neuron.done")
                : s.Format("neuron.evolve.neuron.missing", node.Cost, state.Neuron);

            return evolve;
        }

        private static void AddMissingFeat(EvolveModel evolve, StringTable s, string key, int have, int need)
        {
            if (have < need)
            {
                evolve.MissingFeats.Add(s.Format(key, have, need));
            }
        }

        /// <summary>
        /// Vì sao một nút chưa mua được. Phán quyết là của Core (CanBuy); chỗ này chỉ diễn đạt lại,
        /// và với điều kiện tiên quyết thì nói luôn TÊN nút phải mở trước.
        /// </summary>
        private static string BlockReason(GameSession session, NeuronNode node, PurchaseResult check)
        {
            StringTable s = session.Strings;

            if (check.Error == PurchaseError.MissingPrerequisite)
            {
                NeuronNode required = session.Network.Node(node.Requires);
                return s.Format("neuron.block.prereq", required == null ? node.Requires : required.Name);
            }

            if (check.Error == PurchaseError.NotEnoughNeuron)
            {
                return s.Format("neuron.block.neuron", node.Cost, session.State.Neuron);
            }

            return ExplainPurchaseError(s, check.Error);
        }

        public static string ExplainPurchaseError(StringTable s, PurchaseError error)
        {
            string key = "neuron.error." + error;
            return s.Has(key) ? s.Get(key) : s.Get("neuron.error.unknown");
        }

        private static string TypeText(StringTable s, NodeType type)
        {
            switch (type)
            {
                case NodeType.Root: return s.Get("neuron.type.root");
                case NodeType.Gate: return s.Get("neuron.type.gate");
                case NodeType.Optional: return s.Get("neuron.type.optional");
                default: return s.Get("neuron.type.evolve");
            }
        }

        /// <summary>Tên giai đoạn lấy từ neurons.json; thiếu thì mới hỏi bảng chuỗi.</summary>
        private static string StageName(GameSession session, int stage)
        {
            NeuronData data = session.NeuronData;
            for (int i = 0; i < data.Stages.Length; i++)
            {
                if (data.Stages[i].Stage == stage)
                {
                    return data.Stages[i].Name;
                }
            }

            return TurnScreenModel.StageName(session.Strings, stage);
        }

        /// <summary>Tên nhánh lấy từ neurons.json (nội dung của Khoa), không đặt lại trong code.</summary>
        private static string BranchName(GameSession session, string branchId)
        {
            NeuronData data = session.NeuronData;
            for (int i = 0; i < data.Stages.Length; i++)
            {
                NeuronBranchData[] branches = data.Stages[i].Branches;
                for (int j = 0; j < branches.Length; j++)
                {
                    if (string.Equals(branches[j].Id, branchId, StringComparison.Ordinal))
                    {
                        return branches[j].Name;
                    }
                }
            }

            return branchId;
        }
    }
}
