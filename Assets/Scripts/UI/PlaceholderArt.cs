using System.Collections.Generic;
using BeforeUs.Game;
using UnityEngine;
using UnityEngine.UIElements;

namespace BeforeUs.UI
{
    /// <summary>
    /// Nạp hình tạm và gắn vào phần tử UI Toolkit.
    ///
    /// Dùng Resources.Load chứ không dùng url() trong USS: đường dẫn trong USS phân giải theo
    /// vị trí file USS và tôi chưa kiểm được điều đó ngoài Editor, còn Resources.Load thì tra
    /// theo tên, không phụ thuộc GUID hay đường dẫn tương đối, và chạy đúng trên bản build WebGL.
    ///
    /// Thiếu ảnh KHÔNG được làm sập màn hình: trả về null, phần tử chỉ là một ô trống.
    /// Một ô trống trung thực tốt hơn một màn hình lỗi.
    /// </summary>
    public static class PlaceholderArt
    {
        private static readonly Dictionary<string, Texture2D> Cache = new Dictionary<string, Texture2D>();
        private static bool _warned;

        public static Texture2D Load(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            Texture2D texture;
            if (Cache.TryGetValue(key, out texture))
            {
                return texture;
            }

            texture = Resources.Load<Texture2D>(PlaceholderArtKeys.Folder + key);
            Cache[key] = texture;

            if (texture == null && !_warned)
            {
                _warned = true;
                Debug.LogWarning(
                    "PlaceholderArt: không nạp được hình tạm (" + key + "). "
                    + "Màn hình vẫn chạy, chỉ thiếu hình. "
                    + "Kiểm tra Assets/Art/Placeholder/Resources/BeforeUsPlaceholder/ có PNG không.");
            }

            return texture;
        }

        /// <summary>Một ô vuông chứa hình, hoặc null nếu không có hình để khỏi chiếm chỗ.</summary>
        public static VisualElement Thumbnail(string key, string className)
        {
            Texture2D texture = Load(key);
            if (texture == null)
            {
                return null;
            }

            VisualElement element = new VisualElement();
            element.AddToClassList(className);
            element.style.backgroundImage = new StyleBackground(texture);
            return element;
        }
    }
}
