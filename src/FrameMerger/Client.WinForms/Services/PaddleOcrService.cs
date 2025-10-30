using System.Drawing.Imaging;
using System.Text;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models;
using Sdcb.PaddleOCR.Models.Local;
using Sdcb.PaddleInference;
using OpenCvSharp;

namespace Client.WinForms.Services
{
    public class PaddleOcrService : IOcrService
    {
        private static PaddleOcrAll? _ocrInstance = null;
        private static readonly object _lock = new object();

        private static PaddleOcrAll GetOcrInstance()
        {
            if (_ocrInstance == null)
            {
                lock (_lock)
                {
                    if (_ocrInstance == null)
                    {
                        // Используем английскую модель V3 из Local models
                        FullOcrModel model = LocalFullModels.EnglishV3;
                        _ocrInstance = new PaddleOcrAll(model, PaddleDevice.Mkldnn())
                        {
                            AllowRotateDetection = true,  // разрешить распознавание под углом
                            Enable180Classification = false // не распознавать повёрнутый на 180 градусов
                        };
                    }
                }
            }
            return _ocrInstance;
        }

        public string? RunOcrAndBuildText(IEnumerable<Bitmap?>? images = null, string? tessdataPath = null)
        {
            if (images == null || !images.Any())
            {
                return null;
            }

            try
            {
                var ocr = GetOcrInstance();
                StringBuilder sb = new StringBuilder();

                foreach (var bitmap in images)
                {
                    if (bitmap == null) continue;

                    // Конвертируем Bitmap в Mat (OpenCV формат)
                    using (var mat = BitmapToMat(bitmap))
                    {
                        var result = ocr.Run(mat);
                        if (result != null && result.Regions.Any())
                        {
                            // Объединяем все регионы в текст
                            foreach (var region in result.Regions)
                            {
                                if (!string.IsNullOrWhiteSpace(region.Text))
                                {
                                    sb.AppendLine(region.Text);
                                }
                            }
                        }
                    }
                }

                return sb.Length > 0 ? sb.ToString() : null;
            }
            catch
            {
                return null;
            }
        }

        private static Mat BitmapToMat(Bitmap bitmap)
        {
            // Сохраняем Bitmap во временный поток для преобразования
            using (var ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Png);
                ms.Position = 0;
                return Cv2.ImDecode(ms.ToArray(), ImreadModes.Color);
            }
        }
    }
}

