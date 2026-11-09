using System;
using System.Globalization;

namespace BeforeUs.Core
{
    /// <summary>
    /// xorshift128 với trạng thái 4 × 32 bit.
    /// Tự cài chứ không dùng System.Random có chủ đích: System.Random cho chuỗi khác nhau giữa Mono (Unity)
    /// và CoreCLR (dotnet test), nên cùng một seed sẽ ra hai ván khác nhau ở hai nơi. Thuật toán ở đây
    /// chỉ gồm phép dịch và XOR trên uint, nên mọi runtime cho cùng một chuỗi.
    /// </summary>
    public sealed class XorShift128Random : IRandom
    {
        private const string StatePrefix = "xs128:";

        private uint _x;
        private uint _y;
        private uint _z;
        private uint _w;

        public XorShift128Random(int seed)
        {
            Seed(seed);
        }

        /// <summary>Gieo lại từ đầu. Cùng seed cho cùng chuỗi.</summary>
        public void Seed(int seed)
        {
            // SplitMix32 để trải một seed 32 bit thành 4 từ trạng thái; tránh trạng thái toàn 0.
            uint s = unchecked((uint)seed);
            _x = SplitMix32(ref s);
            _y = SplitMix32(ref s);
            _z = SplitMix32(ref s);
            _w = SplitMix32(ref s);

            if ((_x | _y | _z | _w) == 0u)
            {
                _x = 0x9E3779B9u;
            }
        }

        public double NextDouble()
        {
            // 53 bit ý nghĩa, đúng độ chính xác của double: 27 bit cao + 26 bit thấp.
            ulong high = NextUInt() >> 5;
            ulong low = NextUInt() >> 6;
            return (high * 67108864.0 + low) / 9007199254740992.0;
        }

        public int NextInt(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
            {
                throw new ArgumentOutOfRangeException(nameof(maxExclusive), "maxExclusive phải > minInclusive");
            }

            uint range = (uint)((long)maxExclusive - minInclusive);

            // Lemire: nhân rồi lấy 32 bit cao, có vòng loại bỏ để không lệch về phía giá trị nhỏ.
            uint candidate = NextUInt();
            ulong product = (ulong)candidate * range;
            uint low = (uint)product;

            if (low < range)
            {
                uint threshold = (uint)((0x100000000UL - range) % range);
                while (low < threshold)
                {
                    candidate = NextUInt();
                    product = (ulong)candidate * range;
                    low = (uint)product;
                }
            }

            return (int)(minInclusive + (long)(product >> 32));
        }

        public string SaveState()
        {
            return StatePrefix
                   + _x.ToString("x8", CultureInfo.InvariantCulture) + ","
                   + _y.ToString("x8", CultureInfo.InvariantCulture) + ","
                   + _z.ToString("x8", CultureInfo.InvariantCulture) + ","
                   + _w.ToString("x8", CultureInfo.InvariantCulture);
        }

        public void LoadState(string state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (!state.StartsWith(StatePrefix, StringComparison.Ordinal))
            {
                throw new ArgumentException("Trạng thái không phải của xorshift128: " + state, nameof(state));
            }

            string[] parts = state.Substring(StatePrefix.Length).Split(',');
            if (parts.Length != 4)
            {
                throw new ArgumentException("Trạng thái xorshift128 phải có 4 từ, có " + parts.Length, nameof(state));
            }

            uint x = ParseWord(parts[0], state);
            uint y = ParseWord(parts[1], state);
            uint z = ParseWord(parts[2], state);
            uint w = ParseWord(parts[3], state);

            if ((x | y | z | w) == 0u)
            {
                throw new ArgumentException("Trạng thái xorshift128 toàn 0 là không hợp lệ", nameof(state));
            }

            _x = x;
            _y = y;
            _z = z;
            _w = w;
        }

        private static uint ParseWord(string text, string whole)
        {
            uint value;
            if (!uint.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value))
            {
                throw new ArgumentException("Trạng thái xorshift128 có từ không đọc được: " + whole, nameof(whole));
            }

            return value;
        }

        private uint NextUInt()
        {
            unchecked
            {
                uint t = _x ^ (_x << 11);
                _x = _y;
                _y = _z;
                _z = _w;
                _w = _w ^ (_w >> 19) ^ (t ^ (t >> 8));
                return _w;
            }
        }

        private static uint SplitMix32(ref uint state)
        {
            unchecked
            {
                state += 0x9E3779B9u;
                uint z = state;
                z = (z ^ (z >> 16)) * 0x85EBCA6Bu;
                z = (z ^ (z >> 13)) * 0xC2B2AE35u;
                return z ^ (z >> 16);
            }
        }
    }
}
