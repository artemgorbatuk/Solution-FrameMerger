using System.Drawing;
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
        // Можно заменить на другую модель, например: LocalFullModels.MultilingualV3 (если доступна в вашей версии пакета)
        private static readonly FullOcrModel _ocrModel = LocalFullModels.EnglishV3;

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
                        Action<PaddleConfig> device = PaddleDevice.Mkldnn();

                        _ocrInstance = new PaddleOcrAll(_ocrModel, device)
                        {
                            AllowRotateDetection = true,
                            Enable180Classification = false
                        };
                    }
                }
            }
            return _ocrInstance;
        }

        public string? RunOcrAndBuildText(IEnumerable<Bitmap?>? images = null, string? tessdataPath = null)
        {
            if (images == null || !images.Any())
                return null;

            try
            {
                var ocr = GetOcrInstance();
                var sb = new StringBuilder();

                foreach (var bitmap in images)
                {
                    if (bitmap == null)
                        continue;

                    using var mat = BitmapToMat(bitmap);
                    if (mat.Empty())
                        continue;

                    var result = ocr.Run(mat);
                    if (result?.Regions is { } regions)
                    {
                        foreach (var region in regions)
                        {
                            if (!string.IsNullOrWhiteSpace(region.Text))
                            {
                                sb.AppendLine(region.Text);
                            }
                        }
                    }
                }

                return sb.Length > 0 ? sb.ToString().TrimEnd() : null;
            }
            catch
            {
                return null;
            }
        }

        private static Mat BitmapToMat(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            Rectangle rect = new(0, 0, bitmap.Width, bitmap.Height);
            BitmapData? bmpData = null;

            try
            {
                // Поддерживаем только распространённые форматы
                PixelFormat format = bitmap.PixelFormat;

                switch (format)
                {
                    case PixelFormat.Format24bppRgb:
                    {
                        // В GDI+ порядок каналов фактически BGR, поэтому конверсии не требуется
                        bmpData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, format);
                        using var rawMat = Mat.FromPixelData(bitmap.Height, bitmap.Width, MatType.CV_8UC3, bmpData.Scan0, bmpData.Stride);
                        var cloned = rawMat.Clone();
                        return cloned;
                    }
                    case PixelFormat.Format32bppArgb:
                    case PixelFormat.Format32bppRgb:
                    {
                        // В GDI+ порядок каналов BGRA
                        bmpData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, format);
                        using var rawMat = Mat.FromPixelData(bitmap.Height, bitmap.Width, MatType.CV_8UC4, bmpData.Scan0, bmpData.Stride);
                        var bgrMat = new Mat();
                        Cv2.CvtColor(rawMat, bgrMat, ColorConversionCodes.BGRA2BGR);
                        return bgrMat;
                    }
                    default:
                    {
                        // Если формат не поддерживается — конвертируем в 24bpp RGB и повторим
                        using var converted = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format24bppRgb);
                        using (var g = Graphics.FromImage(converted))
                        {
                            g.DrawImage(bitmap, 0, 0, bitmap.Width, bitmap.Height);
                        }
                        return BitmapToMat(converted);
                    }
                }
            }
            finally
            {
                if (bmpData != null)
                    bitmap.UnlockBits(bmpData);
            }
        }
    }
}