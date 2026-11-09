using System;
using System.Collections.Generic;

namespace BeforeUs.Core
{
    /// <summary>
    /// Những nút mạng lưới neuron đã mua. C5 (NeuronNetwork) hiện thực đầy đủ;
    /// C4 chỉ cần biết một nút đã mua hay chưa, để xử lý mở khóa Xây dựng và Săn bắt (D1).
    /// </summary>
    public interface IPurchasedNodes
    {
        bool Has(string nodeId);

        /// <summary>
        /// Tổng hiệu ứng số của các nút đã mua (design.md §7.3–7.4).
        /// Trả <see cref="NodeEffects.None"/> khi applyNodeEffects = false hoặc khi chưa có mạng lưới thật.
        /// </summary>
        NodeEffects Effects { get; }
    }

    /// <summary>Chưa mua nút nào. Dùng khi dựng ván mới và trong test của C4.</summary>
    public sealed class NoPurchasedNodes : IPurchasedNodes
    {
        public static readonly NoPurchasedNodes Instance = new NoPurchasedNodes();

        private NoPurchasedNodes()
        {
        }

        public bool Has(string nodeId)
        {
            return false;
        }

        public NodeEffects Effects
        {
            get { return NodeEffects.None; }
        }
    }

    /// <summary>Tập nút cố định. Dùng trong test và cho tầng Game trước khi C5 xong.</summary>
    public sealed class PurchasedNodeSet : IPurchasedNodes
    {
        private readonly HashSet<string> _nodes;

        public PurchasedNodeSet(params string[] nodeIds)
        {
            _nodes = new HashSet<string>(StringComparer.Ordinal);
            if (nodeIds != null)
            {
                for (int i = 0; i < nodeIds.Length; i++)
                {
                    _nodes.Add(nodeIds[i]);
                }
            }
        }

        public bool Has(string nodeId)
        {
            return nodeId != null && _nodes.Contains(nodeId);
        }

        /// <summary>Tập nút cố định không mang hiệu ứng số: nó chỉ dùng để test mở khóa.</summary>
        public NodeEffects Effects
        {
            get { return NodeEffects.None; }
        }

        public void Add(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId))
            {
                throw new ArgumentException("nodeId rỗng", nameof(nodeId));
            }

            _nodes.Add(nodeId);
        }
    }
}
