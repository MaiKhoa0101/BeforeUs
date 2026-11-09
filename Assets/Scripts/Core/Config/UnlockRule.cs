using System;
using System.Globalization;

namespace BeforeUs.Core.Config
{
    public enum UnlockKind
    {
        /// <summary>Mở từ đầu ván.</summary>
        Start,

        /// <summary>Mở khi đã mua một nút neuron (design.md §5, D1).</summary>
        Node,

        /// <summary>Mở từ một lượt trở đi.</summary>
        Turn
    }

    /// <summary>
    /// Điều kiện mở khóa hoạt động, phân tích từ chuỗi trong balance.json:
    /// "start", "node:&lt;id&gt;", "turn:&lt;n&gt;".
    /// </summary>
    public sealed class UnlockRule
    {
        private UnlockRule(UnlockKind kind, string nodeId, int fromTurn)
        {
            Kind = kind;
            NodeId = nodeId;
            FromTurn = fromTurn;
        }

        public UnlockKind Kind { get; }

        /// <summary>Chỉ có nghĩa khi <see cref="Kind"/> là <see cref="UnlockKind.Node"/>.</summary>
        public string NodeId { get; }

        /// <summary>Chỉ có nghĩa khi <see cref="Kind"/> là <see cref="UnlockKind.Turn"/>.</summary>
        public int FromTurn { get; }

        public static UnlockRule Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new BalanceConfigException("unlock rỗng");
            }

            string text = value.Trim();
            if (text == "start")
            {
                return new UnlockRule(UnlockKind.Start, null, 0);
            }

            int colon = text.IndexOf(':');
            if (colon > 0 && colon < text.Length - 1)
            {
                string head = text.Substring(0, colon);
                string tail = text.Substring(colon + 1);

                if (head == "node")
                {
                    return new UnlockRule(UnlockKind.Node, tail, 0);
                }

                if (head == "turn")
                {
                    int turn;
                    if (!int.TryParse(tail, NumberStyles.Integer, CultureInfo.InvariantCulture, out turn) || turn < 1)
                    {
                        throw new BalanceConfigException("unlock \"" + text + "\" có số lượt không hợp lệ");
                    }

                    return new UnlockRule(UnlockKind.Turn, null, turn);
                }
            }

            throw new BalanceConfigException("unlock không nhận dạng được: \"" + text + "\"");
        }

        public override string ToString()
        {
            switch (Kind)
            {
                case UnlockKind.Start: return "start";
                case UnlockKind.Node: return "node:" + NodeId;
                default: return "turn:" + FromTurn.ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}
