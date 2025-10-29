using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace Client.WinForms.Services
{
    public interface IFileService
    {
        string GetFrameManagerDirectory();
        string SaveTextWithTimestamp(string text, string extension = "txt");
        string SaveImageWithTimestamp(Bitmap image, string extension = "png");
        void SaveText(string text, string filePath);
        void SaveImage(Bitmap image, string filePath);
        string? ReadTextFile(string filePath);
        void OpenFrameManagerFolder();
        void OpenFolder(string path);
    }

    public class FileService : IFileService
    {
        public string GetFrameManagerDirectory()
        {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Frame Manager");
            Directory.CreateDirectory(dir);
            return dir;
        }

        public string SaveTextWithTimestamp(string text, string extension = "txt")
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string dir = GetFrameManagerDirectory();
            string fileName = $"{timestamp}.{extension}";
            string filePath = Path.Combine(dir, fileName);
            
            SaveText(text, filePath);
            return filePath;
        }

        public string SaveImageWithTimestamp(Bitmap image, string extension = "png")
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string dir = GetFrameManagerDirectory();
            string fileName = $"{timestamp}.{extension}";
            string filePath = Path.Combine(dir, fileName);
            
            SaveImage(image, filePath);
            return filePath;
        }

        public void SaveText(string text, string filePath)
        {
            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(filePath, text, Encoding.UTF8);
        }

        public void SaveImage(Bitmap image, string filePath)
        {
            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            image.Save(filePath, ImageFormat.Png);
        }

        public string? ReadTextFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            try
            {
                return File.ReadAllText(filePath, Encoding.UTF8);
            }
            catch
            {
                // Если UTF-8 не работает, пробуем системную кодировку
                return File.ReadAllText(filePath, Encoding.Default);
            }
        }

        public void OpenFrameManagerFolder()
        {
            string dir = GetFrameManagerDirectory();
            OpenFolder(dir);
        }

        public void OpenFolder(string path)
        {
            // Создаем папку, если не существует
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            System.Diagnostics.Process.Start("explorer.exe", path);
        }
    }
}
