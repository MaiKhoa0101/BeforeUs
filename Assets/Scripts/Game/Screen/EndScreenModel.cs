using System;
using System.Collections.Generic;
using BeforeUs.Core;
using BeforeUs.Core.Neuron;

namespace BeforeUs.Game
{
    /// <summary>
    /// Màn menu đầu ván, màn thắng, màn thua, và màn ăn mừng tiến hóa (A4).
    ///
    /// Giữ đúng nguyên tắc của A3: phán quyết "thắng chưa" là của Core (TurnOutcome.Won),
    /// chỗ này chỉ kể lại. Không có chỗ nào so state.Stage với config.WinStage.
    /// </summary>
    public sealed class EndScreenModel
    {
        private readonly List<string> _lines = new List<string>();

        public string Title { get; set; }
        public string Body { get; set; }
        public string ButtonText { get; set; }

        /// <summary>Thống kê cuối ván, mỗi dòng một ý.</summary>
        public List<string> Lines
        {
            get { return _lines; }
        }

        /// <summary>Menu đầu ván. Nút "Tiếp tục" chưa dựng: lưu ván là C7.</summary>
        public static EndScreenModel Menu(StringTable s)
        {
            EndScreenModel model = new EndScreenModel();
            model.Title = s.Get("menu.title");
            model.Body = s.Get("menu.subtitle");
            model.ButtonText = s.Get("menu.newgame");
            return model;
        }

        public static EndScreenModel Win(GameSession session)
        {
            EndScreenModel model = Outcome(session, "win.title", "win.body", "button.newgame");
            return model;
        }

        public static EndScreenModel Lose(GameSession session)
        {
            return Outcome(session, "lose.title", "lose.body", "button.restart");
        }

        /// <summary>
        /// Màn ăn mừng tiến hóa (design.md §7). Dựng ở mức chữ: art đổi ngoại hình bộ tộc thuộc
        /// F1–F2 và Traits hiện còn rỗng, nên không mô tả ngoại hình — chỉ nói là đã đổi.
        /// </summary>
        public static EndScreenModel Evolution(GameSession session, string newStageName)
        {
            StringTable s = session.Strings;
            GameState state = session.State;

            EndScreenModel model = new EndScreenModel();
            model.Title = s.Get("evolve.title");
            model.Body = s.Format("evolve.body", state.Stage, newStageName);
            model.ButtonText = s.Get("button.continue");

            model.Lines.Add(s.Get("evolve.appearance"));
            model.Lines.Add(s.Format("evolve.energy", session.Config.EnergyMaxForStage(state.Stage)));
            model.Lines.Add(s.Format("evolve.upkeep", session.Config.UpkeepForStage(state.Stage)));
            model.Lines.Add(s.Get("evolve.feats.reset"));

            return model;
        }

        private static EndScreenModel Outcome(GameSession session, string titleKey, string bodyKey, string buttonKey)
        {
            StringTable s = session.Strings;
            GameState state = session.State;

            EndScreenModel model = new EndScreenModel();
            model.Title = s.Get(titleKey);
            model.Body = s.Get(bodyKey);
            model.ButtonText = s.Get(buttonKey);

            model.Lines.Add(s.Format("summary.turns", state.Turn));
            model.Lines.Add(s.Format("summary.stage", state.Stage, StageName(session, state.Stage)));
            model.Lines.Add(s.Format("summary.region", TurnScreenModel.RegionName(session.Config, state.Region)));
            model.Lines.Add(s.Format("summary.crew", state.Crew));
            model.Lines.Add(s.Format("summary.neuron", state.Neuron));
            model.Lines.Add(s.Format("summary.nodes", session.Network.PurchasedIds.Count));
            model.Lines.Add(s.Format("summary.feats", state.CombatWins, state.Hunts, state.MigratedEver));

            return model;
        }

        /// <summary>Tên giai đoạn lấy từ neurons.json; giai đoạn ngoài MVP thì hỏi bảng chuỗi.</summary>
        public static string StageName(GameSession session, int stage)
        {
            NeuronData data = session.NeuronData;
            if (data != null)
            {
                for (int i = 0; i < data.Stages.Length; i++)
                {
                    if (data.Stages[i].Stage == stage)
                    {
                        return data.Stages[i].Name;
                    }
                }
            }

            return TurnScreenModel.StageName(session.Strings, stage);
        }
    }
}
