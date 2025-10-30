using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Client.WinForms.Services
{

    public interface IStatusMessageService
    {
        (Color color, string iconSymbol) GetStatusConfig(MessageType messageType);
        Bitmap CreateStatusIcon(string symbol, Color foreColor, int size = 16);
    }

    public class StatusMessageService : IStatusMessageService
    {
        public (Color color, string iconSymbol) GetStatusConfig(MessageType messageType)
        {
            return messageType switch
            {
                MessageType.Error => (Color.Red, "✕"),
                MessageType.Warning => (Color.Orange, "⚠"),
                MessageType.Success => (Color.Green, "✓"),
                _ => (Color.Blue, "ℹ") // Info by default
            };
        }

        public Bitmap CreateStatusIcon(string symbol, Color foreColor, int size = 16)
        {
            Bitmap bmp = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                // Прозрачный фон
                g.Clear(Color.Transparent);

                // Пробуем несколько шрифтов для лучшей поддержки символов
                string[] fonts = { "Segoe UI Symbol", "Segoe UI", "Arial Unicode MS", "Arial" };
                Font? font = null;
                foreach (string fontName in fonts)
                {
                    try
                    {
                        font = new Font(fontName, size * 0.85f, FontStyle.Regular, GraphicsUnit.Pixel);
                        break;
                    }
                    catch { }
                }

                if (font == null)
                    font = SystemFonts.DefaultFont;

                using (font)
                using (SolidBrush brush = new SolidBrush(foreColor))
                {
                    StringFormat format = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString(symbol, font, brush, new RectangleF(0, 0, size, size), format);
                }
            }
            return bmp;
        }
    }
}
