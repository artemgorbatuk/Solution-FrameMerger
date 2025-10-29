using System.Text.RegularExpressions;

namespace Client.WinForms.Services
{
    public interface ITextProcessingService
    {
        string NormalizeText(string text);
    }

    public class TextProcessingService : ITextProcessingService
    {
        public string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            // Trim построчно
            string[] lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].Trim();
            }

            string joined = string.Join("\n", lines);

            // Схлопываем более одного пустого до одного
            joined = Regex.Replace(joined, "\n{2,}", "\n");

            return joined.Trim();
        }
    }
}
