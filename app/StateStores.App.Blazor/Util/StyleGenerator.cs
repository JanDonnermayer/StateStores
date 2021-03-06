using System.Linq;

namespace StateStores.App.Blazor
{
    static class StyleGenerator
    {
        /// <summary>
        /// Returns a color based on the content of the specified string
        /// </summary>
        public static string GetStyle(string text)
        {
            var code = (text ?? string.Empty).Select(c => (int)c).Sum() ^ 358;
            return $"color: rgb({(code ^ 126) % 255}, " +
            $"{(code ^ 226) % 255}, {(code ^ 656) % 255 });";
        }
    }
}