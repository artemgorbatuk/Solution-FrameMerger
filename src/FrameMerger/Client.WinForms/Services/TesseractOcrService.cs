using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using Tesseract;

namespace Client.WinForms.Services
{
    public class TesseractOcrService : IOcrService
    {
        public string? RunOcrAndBuildText(IEnumerable<Bitmap?>? images = null, string? tessdataPath = null)
        {
            // Если tessdataPath не передан, пытаемся найти рядом с exe
            if (string.IsNullOrEmpty(tessdataPath))
            {
                tessdataPath = Path.Combine(AppContext.BaseDirectory, "tessdata");
            }
            
            if (!Directory.Exists(tessdataPath))
            {
                return null;
            }

            // Если images не передан, возвращаем null (без изображений нечего обрабатывать)
            if (images == null)
            {
                return null;
            }

            StringBuilder sb = new StringBuilder();

            try
            {
                using (var engine = new TesseractEngine(tessdataPath, "rus+eng", EngineMode.Default))
                {
                    foreach (var bitmap in images)
                    {
                        if (bitmap == null)
                        {
                            continue;
                        }

                        using (var pix = ConvertBitmapToPix(bitmap))
                        using (var page = engine.Process(pix))
                        {
                            string text = page.GetText();
                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                sb.AppendLine(text);
                            }
                        }
                    }
                }
            }
            catch
            {
                // При ошибке возвращаем null
                return null;
            }

            return sb.Length > 0 ? sb.ToString() : null;
        }

        private Pix ConvertBitmapToPix(Bitmap bitmap)
        {
            using (var ms = new MemoryStream())
            {
                // PNG даёт без потерь и понятен Tesseract
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;
                return Pix.LoadFromMemory(ms.ToArray());
            }
        }
    }
}
