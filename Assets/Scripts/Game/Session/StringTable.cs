using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;

namespace BeforeUs.Game
{
    /// <summary>
    /// Bảng chuỗi hiện cho người chơi, nạp từ Assets/Data/strings.vi.json (AGENTS.md: không viết cứng
    /// tiếng Việt trong code hay UXML). Phẳng một tầng: khóa → mẫu chuỗi, chỗ thay dùng {0}, {1}.
    ///
    /// Thuần C#, không tham chiếu UnityEngine: tầng Unity chỉ việc đưa nội dung file vào đây.
    /// </summary>
    public sealed class StringTable
    {
        private readonly Dictionary<string, string> _entries;
        private readonly List<string> _missing = new List<string>();

        private StringTable(Dictionary<string, string> entries)
        {
            _entries = entries;
        }

        /// <summary>Những khóa đã bị hỏi mà không có. Dùng để bắt lỗi sớm, không hiện cho người chơi.</summary>
        public IReadOnlyList<string> MissingKeys
        {
            get { return _missing; }
        }

        public static StringTable FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new ArgumentException("strings json rỗng", nameof(json));
            }

            Dictionary<string, string> raw = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            if (raw == null)
            {
                throw new FormatException("strings json không đọc được thành bảng khóa → chuỗi");
            }

            Dictionary<string, string> entries = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, string> pair in raw)
            {
                // Khóa bắt đầu bằng "_" là ghi chú cho người đọc file, không phải chuỗi hiển thị.
                if (pair.Key.Length > 0 && pair.Key[0] == '_')
                {
                    continue;
                }

                entries[pair.Key] = pair.Value;
            }

            return new StringTable(entries);
        }

        public bool Has(string key)
        {
            return key != null && _entries.ContainsKey(key);
        }

        /// <summary>
        /// Chuỗi theo khóa. Thiếu khóa thì trả về "⟨khóa⟩" chứ không ném: thiếu chữ không đáng làm sập
        /// màn hình, và dấu ngoặc nhọn làm chỗ thiếu đập vào mắt ngay khi chạy.
        /// </summary>
        public string Get(string key)
        {
            string value;
            if (key != null && _entries.TryGetValue(key, out value))
            {
                return value;
            }

            Miss(key);
            return "⟨" + key + "⟩";
        }

        public string Format(string key, params object[] args)
        {
            string template = Get(key);
            if (args == null || args.Length == 0)
            {
                return template;
            }

            try
            {
                return string.Format(CultureInfo.InvariantCulture, template, args);
            }
            catch (FormatException)
            {
                Miss(key);
                return template;
            }
        }

        private void Miss(string key)
        {
            string name = key ?? "(null)";
            if (!_missing.Contains(name))
            {
                _missing.Add(name);
            }
        }
    }
}
