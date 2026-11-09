using System;
using System.Collections.Generic;

namespace BeforeUs.Core
{
    /// <summary>Một nhóm đặc điểm ngoại hình và các lựa chọn của nó, ví dụ "toc" với "thang", "xoan".</summary>
    public sealed class TraitCategory
    {
        public TraitCategory(string name, IReadOnlyList<string> options)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Nhóm đặc điểm phải có tên", nameof(name));
            }

            if (options == null || options.Count == 0)
            {
                throw new ArgumentException("Nhóm đặc điểm \"" + name + "\" không có lựa chọn nào", nameof(options));
            }

            Name = name;
            Options = options;
        }

        public string Name { get; }
        public IReadOnlyList<string> Options { get; }
    }

    /// <summary>
    /// Sinh thành viên mới (design.md §3). Giữ bộ đếm id, nên id không bao giờ trùng trong một ván
    /// kể cả sau khi có người chết — combat trả về deadMemberIds nên id phải là duy nhất.
    /// </summary>
    public sealed class MemberFactory
    {
        private static readonly TraitCategory[] NoTraits = new TraitCategory[0];

        private readonly MemberStatRanges _ranges;
        private readonly IReadOnlyList<TraitCategory> _traitPool;
        private int _nextId;

        public MemberFactory(MemberStatRanges ranges, IReadOnlyList<TraitCategory> traitPool = null, int firstId = 1)
        {
            if (ranges == null)
            {
                throw new ArgumentNullException(nameof(ranges));
            }

            if (firstId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(firstId), "id thành viên đếm từ 1");
            }

            _ranges = ranges;
            _traitPool = traitPool ?? NoTraits;
            _nextId = firstId;
        }

        /// <summary>Id sẽ cấp cho thành viên tiếp theo. Cần lưu lại ở C7 để tải ván không cấp trùng id.</summary>
        public int NextId
        {
            get { return _nextId; }
        }

        public void ResumeFrom(int nextId)
        {
            if (nextId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(nextId), "id thành viên đếm từ 1");
            }

            _nextId = nextId;
        }

        /// <param name="statBonus">
        /// Cộng thẳng vào speed và strength của riêng người vừa sinh.
        /// Dùng cho nút "Dạy trẻ" (design.md §7.4): chỉ thành viên MỚI SINH được cộng,
        /// người đã có từ trước không đổi — nên nó là tham số của Create chứ không phải của dải chỉ số.
        /// </param>
        public Member Create(IRandom random, double statBonus = 0.0)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            double speed = Lerp(_ranges.SpeedMin, _ranges.SpeedMax, random) + statBonus;
            double strength = Lerp(_ranges.StrengthMin, _ranges.StrengthMax, random) + statBonus;
            int hp = _ranges.HpMin == _ranges.HpMax
                ? _ranges.HpMin
                : random.NextInt(_ranges.HpMin, _ranges.HpMax + 1);

            string[] traits = DrawTraits(random);
            return new Member(_nextId++, speed, strength, hp, traits);
        }

        private string[] DrawTraits(IRandom random)
        {
            if (_traitPool.Count == 0)
            {
                return null;
            }

            string[] traits = new string[_traitPool.Count];
            for (int i = 0; i < _traitPool.Count; i++)
            {
                TraitCategory category = _traitPool[i];
                string option = category.Options[random.NextInt(0, category.Options.Count)];
                traits[i] = category.Name + ":" + option;
            }

            return traits;
        }

        private static double Lerp(double min, double max, IRandom random)
        {
            // Dải một điểm thì không rút số: rút ngẫu nhiên để lấy về một hằng số là việc thừa.
            // (Không phải để "khớp chuỗi" với before_us_sim.py — sim dùng Mersenne Twister của Python,
            //  Core dùng xorshift128, hai chuỗi không bao giờ trùng nhau. C8 so thống kê, không so chuỗi.)
            if (min == max)
            {
                return min;
            }

            return min + (max - min) * random.NextDouble();
        }
    }
}
