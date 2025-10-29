using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Client.WinForms.Services
{
    public interface IImageRenderingService
    {
        Bitmap RenderTextToBitmap(string text, int widthPx, int paddingPx, int fontSizePx);
    }

    public class ImageRenderingService : IImageRenderingService
    {
        public Bitmap RenderTextToBitmap(string text, int widthPx, int paddingPx, int fontSizePx)
        {
            if (string.IsNullOrEmpty(text))
            {
                text = "(пусто)";
            }

            using Font font = new Font("Segoe UI", fontSizePx, FontStyle.Regular, GraphicsUnit.Pixel);
            int contentWidth = Math.Max(50, widthPx - paddingPx * 2);

            // Оценка высоты через MeasureString (даёт менее «жирный» рендер при DrawString)
            int textHeight;
            using (Bitmap tmp = new Bitmap(1, 1))
            using (Graphics mg = Graphics.FromImage(tmp))
            {
                StringFormat measureFormat = new StringFormat(StringFormatFlags.LineLimit)
                {
                    Trimming = StringTrimming.Word
                };
                SizeF measuredF = mg.MeasureString(text, font, contentWidth, measureFormat);
                textHeight = (int)Math.Ceiling(measuredF.Height);
            }

            int totalHeight = paddingPx * 2 + textHeight;

            Bitmap bmp = new Bitmap(widthPx, Math.Max(50, totalHeight));
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                g.SmoothingMode = SmoothingMode.None;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                StringFormat drawFormat = new StringFormat(StringFormatFlags.LineLimit)
                {
                    Trimming = StringTrimming.Word
                };
                RectangleF rect = new RectangleF(paddingPx, paddingPx, contentWidth, textHeight);
                using (Brush brush = new SolidBrush(Color.Black))
                {
                    g.DrawString(text, font, brush, rect, drawFormat);
                }
            }
            return bmp;
        }
    }
}
