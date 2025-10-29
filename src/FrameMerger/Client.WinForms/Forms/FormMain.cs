using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Tesseract;
using System.IO;

namespace Client.WinForms
{
    public partial class FormMain : Form
    {
        private List<(PictureBox Picture, Label Label, Button Delete)> screenshotItems = new List<(PictureBox, Label, Button)>();
        private int screenshotSpacing = 10;
        private int baseScreenshotWidth = 180;
        private int baseScreenshotHeight = 120;
        private Padding panelPadding = new Padding(10);
        private readonly int exportImageWidth = 1000;
        private readonly int exportPadding = 20;
        private readonly int exportFontSize = 14;
        private int? dragSourceIndex = null;
        private Panel? dropIndicatorLine = null; // линия-индикатор позиции вставки
        private int? dragInsertIndex = null; // индекс вставки во время DragOver
        private bool isEditEnabled = false;

        public FormMain()
        {
            InitializeComponent();
            InitializeScreenshotPanel();
            InitializeOcrUi();
        }

        private void InitializeScreenshotPanel()
        {
            // Настройка панели для скриншотов
            panelScreenshots.AutoScroll = true;
            panelScreenshots.BackColor = Color.LightGray;
            panelScreenshots.BorderStyle = BorderStyle.FixedSingle;
            panelScreenshots.Padding = panelPadding;

            // Перелайаут при изменении размеров, чтобы не появлялась горизонтальная прокрутка
            panelScreenshots.Resize += (_, __) => LayoutScreenshots();

            // Включаем Drag & Drop для перестановки
            panelScreenshots.AllowDrop = true;
            panelScreenshots.DragEnter += PanelScreenshots_DragEnter;
            panelScreenshots.DragOver += PanelScreenshots_DragOver;
            panelScreenshots.DragDrop += PanelScreenshots_DragDrop;
        }

        private void InitializeOcrUi()
        {
            // Ничего специального тут не требуется: обработчики кнопок ниже
        }

        public enum MessageType
        {
            Info,
            Success,
            Warning,
            Error
        }

        private void ShowStatusMessage(string message, MessageType messageType = MessageType.Info)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ShowStatusMessage(message, messageType)));
                return;
            }

            Color color;
            string iconSymbol;
            
            switch (messageType)
            {
                case MessageType.Error:
                    color = Color.Red;
                    iconSymbol = "✕"; // X mark
                    break;
                case MessageType.Warning:
                    color = Color.Orange;
                    iconSymbol = "⚠"; // Warning sign
                    break;
                case MessageType.Success:
                    color = Color.Green;
                    iconSymbol = "✓"; // Check mark
                    break;
                default: // Info
                    color = Color.Blue;
                    iconSymbol = "ℹ"; // Information sign
                    break;
            }

            // Обновляем иконку
            if (statusIconLabel.Image != null)
            {
                statusIconLabel.Image.Dispose();
            }
            statusIconLabel.Image = CreateStatusIcon(iconSymbol, color, 22);

            statusLabel.Text = message;
            statusLabel.ForeColor = color;
        }

        // Старый метод для обратной совместимости
        private void ShowStatusMessage(string message, Color color)
        {
            MessageType type = color == Color.Red ? MessageType.Error :
                              color == Color.Orange ? MessageType.Warning :
                              color == Color.Green ? MessageType.Success :
                              MessageType.Info;
            ShowStatusMessage(message, type);
        }

        private Bitmap CreateStatusIcon(string symbol, Color foreColor, int size = 16)
        {
            Bitmap bmp = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
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

        public void AddScreenshot(string name)
        {
            int contentWidth = GetContentWidth();
            int contentHeight = GetContentHeight(contentWidth);

            // Создаем PictureBox для скриншота
            PictureBox pictureBox = new PictureBox
            {
                Size = new Size(contentWidth, contentHeight),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Location = new Point(panelScreenshots.Padding.Left, panelScreenshots.Padding.Top + screenshotItems.Count * (contentHeight + screenshotSpacing))
            };

            // Создаем тестовое изображение (заглушка)
            Bitmap testImage = CreateTestImage(name, contentWidth, contentHeight);
            pictureBox.Image = testImage;

            // Добавляем подпись
            Label label = new Label
            {
                Text = name,
                Location = new Point(pictureBox.Left, pictureBox.Bottom + 2),
                Size = new Size(contentWidth, 15),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 8)
            };
            label.AutoSize = false;

            // Кнопка удаления
            Button btnDelete = new Button
            {
                Text = string.Empty,
                Font = new Font("Segoe UI", 13f, FontStyle.Bold, GraphicsUnit.Point),
                Size = new Size(22, 22),
                BackColor = Color.LightCoral,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                TabStop = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Tag = pictureBox; // для поиска элемента при удалении
            btnDelete.Click += OnDeleteItemClick;
            btnDelete.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using var f = new Font("Segoe UI", 13f, FontStyle.Bold, GraphicsUnit.Point);
                using var br = new SolidBrush(Color.White);
                pe.Graphics.DrawString("×", f, br, new RectangleF(0, 0, ((Control)s!).Width, ((Control)s!).Height), sf);
            };

            // Добавляем обработчик клика
            pictureBox.Click += (sender, e) => OnScreenshotClick(name);
            // Инициируем DnD по нажатию ЛКМ
            pictureBox.MouseDown += PictureBox_MouseDownStartDrag;

            // Добавляем элементы на панель
            panelScreenshots.Controls.Add(pictureBox);
            panelScreenshots.Controls.Add(label);
            // Крестик размещаем внутри превью, чтобы гарантировать позицию в углу
            btnDelete.Parent = pictureBox;
            btnDelete.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDelete.Location = new Point(pictureBox.Width - btnDelete.Width, 0);
            pictureBox.Controls.Add(btnDelete);
            screenshotItems.Add((pictureBox, label, btnDelete));

            // Обновляем размер панели для прокрутки
            UpdatePanelSize();
        }

        public void AddScreenshot(Bitmap image, string name)
        {
            int contentWidth = GetContentWidth();
            int contentHeight = GetContentHeight(contentWidth);

            PictureBox pictureBox = new PictureBox
            {
                Size = new Size(contentWidth, contentHeight),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Location = new Point(panelScreenshots.Padding.Left, panelScreenshots.Padding.Top + screenshotItems.Count * (contentHeight + screenshotSpacing))
            };

            pictureBox.Image = (Bitmap)image.Clone();

            Label label = new Label
            {
                Text = name,
                Location = new Point(pictureBox.Left, pictureBox.Bottom + 2),
                Size = new Size(contentWidth, 15),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 8),
                AutoSize = false
            };

            Button btnDelete = new Button
            {
                Text = string.Empty,
                Font = new Font("Segoe UI", 13f, FontStyle.Bold, GraphicsUnit.Point),
                Size = new Size(22, 22),
                BackColor = Color.LightCoral,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                TabStop = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Tag = pictureBox;
            btnDelete.Click += OnDeleteItemClick;
            btnDelete.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using var f = new Font("Segoe UI", 13f, FontStyle.Bold, GraphicsUnit.Point);
                using var br = new SolidBrush(Color.White);
                pe.Graphics.DrawString("×", f, br, new RectangleF(0, 0, ((Control)s!).Width, ((Control)s!).Height), sf);
            };

            pictureBox.Click += (sender, e) => OnScreenshotClick(name);
            pictureBox.MouseDown += PictureBox_MouseDownStartDrag;

            panelScreenshots.Controls.Add(pictureBox);
            panelScreenshots.Controls.Add(label);
            btnDelete.Parent = pictureBox;
            btnDelete.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDelete.Location = new Point(pictureBox.Width - btnDelete.Width, 0);
            pictureBox.Controls.Add(btnDelete);
            screenshotItems.Add((pictureBox, label, btnDelete));

            UpdatePanelSize();
            LayoutScreenshots();
        }

        private void OnDeleteItemClick(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is PictureBox pb)
            {
                int idx = screenshotItems.FindIndex(it => it.Picture == pb);
                if (idx >= 0)
                {
                    var item = screenshotItems[idx];
                    string screenshotName = item.Label.Text;
                    
                    ShowStatusMessage($"Удаление скриншота '{screenshotName}'...", MessageType.Info);
                    
                    panelScreenshots.Controls.Remove(item.Picture);
                    panelScreenshots.Controls.Remove(item.Label);
                    // delete button удаляется вместе с pictureBox, но очищаем на всякий случай
                    item.Picture.Controls.Remove(item.Delete);
                    item.Picture.Dispose();
                    item.Label.Dispose();
                    item.Delete.Dispose();
                    screenshotItems.RemoveAt(idx);
                    UpdatePanelSize();
                    LayoutScreenshots();
                    
                    ShowStatusMessage($"Скриншот '{screenshotName}' успешно удален", MessageType.Success);
                }
            }
        }

        private async void btnCreateFrame_Click(object? sender, EventArgs e)
        {
            ShowStatusMessage("Начало создания скриншота...", MessageType.Info);
            
            // Скрываем форму для захвата экрана
            this.WindowState = FormWindowState.Minimized;
            this.Hide();
            
            // Небольшая задержка для полного скрытия формы
            await Task.Delay(300);
            
            try
            {
                var screenshot = CaptureScreenArea();
                if (screenshot != null)
                {
                    string timestamp = DateTime.Now.ToString("HH-mm-ss");
                    AddScreenshot(screenshot, $"Скриншот {timestamp}");
                    ShowStatusMessage("Скриншот успешно создан", MessageType.Success);
                }
                else
                {
                    ShowStatusMessage("Создание скриншота отменено", MessageType.Info);
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Ошибка захвата экрана: {ex.Message}", MessageType.Error);
            }
            finally
            {
                // Показываем форму обратно
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.BringToFront();
            }
        }

        private Bitmap? CaptureScreenArea()
        {
            // Создаем форму для выбора области
            using var captureForm = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                WindowState = FormWindowState.Maximized,
                TopMost = true,
                BackColor = Color.Black,
                Opacity = 0.3,
                Cursor = Cursors.Cross
            };

            Rectangle selectedArea = Rectangle.Empty;
            bool isCapturing = false;
            Point startPoint = Point.Empty;

            // Обработчики событий мыши
            captureForm.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    isCapturing = true;
                    startPoint = e.Location;
                }
            };

            captureForm.MouseMove += (s, e) =>
            {
                if (isCapturing)
                {
                    int x = Math.Min(startPoint.X, e.X);
                    int y = Math.Min(startPoint.Y, e.Y);
                    int width = Math.Abs(e.X - startPoint.X);
                    int height = Math.Abs(e.Y - startPoint.Y);
                    selectedArea = new Rectangle(x, y, width, height);
                    captureForm.Invalidate();
                }
            };

            captureForm.MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtons.Left && isCapturing)
                {
                    isCapturing = false;
                    if (selectedArea.Width > 10 && selectedArea.Height > 10)
                    {
                        captureForm.DialogResult = DialogResult.OK;
                    }
                }
            };

            // Обработчик отрисовки выделенной области
            captureForm.Paint += (s, e) =>
            {
                if (selectedArea != Rectangle.Empty)
                {
                    using var brush = new SolidBrush(Color.FromArgb(100, 0, 120, 215));
                    e.Graphics.FillRectangle(brush, selectedArea);
                    using var pen = new Pen(Color.FromArgb(200, 0, 120, 215), 2);
                    e.Graphics.DrawRectangle(pen, selectedArea);
                }
            };

            // Обработчик клавиш
            captureForm.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    captureForm.DialogResult = DialogResult.Cancel;
                }
            };

            captureForm.KeyPreview = true;

            if (captureForm.ShowDialog() == DialogResult.OK && selectedArea.Width > 10 && selectedArea.Height > 10)
            {
                // Захватываем выбранную область экрана
                using var screenBitmap = new Bitmap(selectedArea.Width, selectedArea.Height);
                using var graphics = Graphics.FromImage(screenBitmap);
                graphics.CopyFromScreen(selectedArea.Location, Point.Empty, selectedArea.Size);
                return new Bitmap(screenBitmap);
            }

            return null;
        }

        // =========== OCR + TEXT PIPELINE ===========
        private async void btnFormText_Click(object? sender, EventArgs e)
        {
            ShowStatusMessage("Начало формирования текста...", MessageType.Info);
            
            try
            {
                // Проверяем наличие tessdata перед запуском
                string tessdataPath = Path.Combine(AppContext.BaseDirectory, "tessdata");
                if (!Directory.Exists(tessdataPath))
                {
                    ShowStatusMessage($"Не найдена папка tessdata. Разместите файлы языковых моделей (rus.traineddata, eng.traineddata) в папке: {tessdataPath}", MessageType.Error);
                    return;
                }

                string? text = await Task.Run(() => RunOcrAndBuildText());
                
                if (string.IsNullOrWhiteSpace(text))
                {
                    ShowStatusMessage("Текст не найден на изображениях.", MessageType.Warning);
                    return;
                }
                
                // Только при успешном считывании обновляем текст
                if (chkNormalize.Checked)
                {
                    text = NormalizeText(text);
                }
                richText.Text = text;
                ShowStatusMessage("Текст успешно сформирован", MessageType.Success);
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Ошибка OCR: {ex.Message}", MessageType.Error);
            }
        }

        private void btnFormImage_Click(object? sender, EventArgs e)
        {
            ShowStatusMessage("Начало формирования картинки...", MessageType.Info);
            
            try
            {
                string textToRender = chkNormalize.Checked ? NormalizeText(richText.Text) : richText.Text;
                if (string.IsNullOrWhiteSpace(textToRender))
                {
                    ShowStatusMessage("Нет текста для формирования картинки.", MessageType.Info);
                    return;
                }

                var img = RenderTextToBitmap(textToRender, exportImageWidth, exportPadding, exportFontSize);
                // Освобождаем старую картинку, если была
                if (picturePreview.Image != null)
                {
                    picturePreview.Image.Dispose();
                }
                picturePreview.Image = img;
                picturePreview.Size = img.Size;
                tabControl.SelectedTab = tabImage;
                ShowStatusMessage("Картинка успешно сформирована", MessageType.Success);
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Ошибка формирования картинки: {ex.Message}", MessageType.Error);
            }
        }

        private string? RunOcrAndBuildText()
        {
            StringBuilder sb = new StringBuilder();
            
            // Пробуем найти tessdata рядом с exe
            string tessdataPath = Path.Combine(AppContext.BaseDirectory, "tessdata");
            if (!Directory.Exists(tessdataPath))
            {
                // Возвращаем null, чтобы показать MessageBox
                return null;
            }

            try
            {
                using (var engine = new TesseractEngine(tessdataPath, "rus+eng", EngineMode.Default))
                {
                    foreach (var item in screenshotItems)
                    {
                        if (item.Picture.Image is Bitmap bmp)
                        {
                            using (var pix = ConvertBitmapToPix(bmp))
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
            }
            catch (Exception)
            {
                // При ошибке возвращаем null, чтобы показать MessageBox
                return null;
            }

            return sb.Length > 0 ? sb.ToString() : null;
        }

        private static Pix ConvertBitmapToPix(Bitmap bitmap)
        {
            using (var ms = new MemoryStream())
            {
                // PNG даёт без потерь и понятен Tesseract
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;
                return Pix.LoadFromMemory(ms.ToArray());
            }
        }

        private static string NormalizeText(string text)
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

        private static Bitmap RenderTextToBitmap(string text, int widthPx, int paddingPx, int fontSizePx)
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

        private void btnSaveTxt_Click(object? sender, EventArgs e)
        {
            ShowStatusMessage("Начало сохранения текста...", MessageType.Info);
            
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Frame Manager");
                Directory.CreateDirectory(dir);
                string file = Path.Combine(dir, $"{timestamp}.txt");
                File.WriteAllText(file, richText.Text, Encoding.UTF8);
                ShowStatusMessage($"Сохранено: {file}", MessageType.Success);
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Ошибка сохранения TXT: {ex.Message}", MessageType.Error);
            }
        }

        private void btnSavePng_Click(object? sender, EventArgs e)
        {
            ShowStatusMessage("Начало сохранения картинки...", MessageType.Info);
            
            try
            {
                if (picturePreview.Image == null)
                {
                    ShowStatusMessage("Нет картинки для сохранения. Сначала сформируйте картинку.", MessageType.Info);
                    return;
                }

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Frame Manager");
                Directory.CreateDirectory(dir);
                string file = Path.Combine(dir, $"{timestamp}.png");
                
                // Сохраняем картинку с вкладки "Картинка"
                picturePreview.Image.Save(file, System.Drawing.Imaging.ImageFormat.Png);
                ShowStatusMessage($"Сохранено: {file}", MessageType.Success);
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Ошибка сохранения PNG: {ex.Message}", MessageType.Error);
            }
        }

        private void btnAddFromFile_Click(object? sender, EventArgs e)
        {
            ShowStatusMessage("Начало добавления изображений...", MessageType.Info);
            
            string defaultFolder = @"C:\Users\gorba_6ku4vx\OneDrive\Документы\Frame Manager";
            if (!Directory.Exists(defaultFolder))
            {
                Directory.CreateDirectory(defaultFolder);
            }
            
            using OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "Выберите изображения",
                Filter = "Изображения|*.png;*.jpg;*.jpeg;*.bmp;*.gif",
                Multiselect = true,
                InitialDirectory = defaultFolder
            };
            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                int successCount = 0;
                foreach (var path in ofd.FileNames)
                {
                    try
                    {
                        using var img = new Bitmap(path);
                        AddScreenshot(img, Path.GetFileNameWithoutExtension(path));
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        ShowStatusMessage($"Не удалось загрузить {path}: {ex.Message}", MessageType.Error);
                    }
                }
                
                if (successCount > 0)
                {
                    ShowStatusMessage($"Успешно добавлено изображений: {successCount}", MessageType.Success);
                }
            }
            else
            {
                ShowStatusMessage("Добавление изображений отменено", MessageType.Info);
            }
        }

        private void btnOpenFolder_Click(object? sender, EventArgs e)
        {
            ShowStatusMessage("Открытие папки...", MessageType.Info);
            
            try
            {
                string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Frame Manager");
                Directory.CreateDirectory(dir);
                System.Diagnostics.Process.Start("explorer.exe", dir);
                ShowStatusMessage("Папка успешно открыта", MessageType.Success);
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Ошибка открытия папки: {ex.Message}", MessageType.Error);
            }
        }

        private void btnToggleEdit_Click(object? sender, EventArgs e)
        {
            ShowStatusMessage($"Переключение режима редактирования...", MessageType.Info);
            
            isEditEnabled = !isEditEnabled;
            richText.ReadOnly = !isEditEnabled;
            richText.BackColor = isEditEnabled ? Color.White : SystemColors.Control;
            btnToggleEdit.Text = isEditEnabled ? "Запретить редакт." : "Разрешить редакт.";
            
            ShowStatusMessage(isEditEnabled ? "Редактирование разрешено" : "Редактирование запрещено", MessageType.Success);
        }

        private void btnAddText_Click(object? sender, EventArgs e)
        {
            ShowStatusMessage("Начало добавления текста из файлов...", MessageType.Info);
            
            using OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "Выберите текстовые файлы",
                Filter = "Текстовые файлы (*.txt, *.log, *.md)|*.txt;*.log;*.md|" +
                         "Конфигурационные (*.ini, *.cfg, *.yml, *.yaml)|*.ini;*.cfg;*.yml;*.yaml|" +
                         "Веб-разметка (*.html, *.htm, *.css)|*.html;*.htm;*.css;*.scss;*.less|" +
                         "Данные (*.csv, *.json, *.xml)|*.csv;*.json;*.xml|" +
                         "Языки программирования (*.cs, *.js, *.py, *.java, и др.)|*.js;*.ts;*.tsx;*.jsx;*.cs;*.csx;*.vb;*.sql;*.py;*.rb;*.go;*.rs;*.kt;*.java;*.cpp;*.cc;*.c;*.hpp;*.h;*.sh;*.bat;*.ps1|" +
                         "Все файлы|*.*",
                Multiselect = true
            };
            if (ofd.ShowDialog(this) != DialogResult.OK)
            {
                ShowStatusMessage("Добавление текста отменено", MessageType.Info);
                return;
            }

            int successCount = 0;
            foreach (var path in ofd.FileNames)
            {
                try
                {
                    string content;
                    // Пытаемся UTF-8, затем системная кодировка как запасной вариант
                    try { content = File.ReadAllText(path, Encoding.UTF8); }
                    catch { content = File.ReadAllText(path, Encoding.Default); }

                    if (!string.IsNullOrEmpty(content))
                    {
                        if (!string.IsNullOrEmpty(richText.Text))
                            richText.AppendText("\n\n");
                        richText.AppendText(content);
                        successCount++;
                    }
                }
                catch (Exception ex)
                {
                    ShowStatusMessage($"Не удалось прочитать {path}: {ex.Message}", MessageType.Error);
                }
            }
            
            if (successCount > 0)
            {
                ShowStatusMessage($"Успешно добавлено файлов: {successCount}", MessageType.Success);
            }
        }

        private Bitmap CreateTestImage(string name, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                // Заливаем фон
                g.FillRectangle(Brushes.LightBlue, 0, 0, width, height);
                
                // Добавляем рамку
                g.DrawRectangle(Pens.Black, 0, 0, width - 1, height - 1);
                
                // Добавляем текст
                Font font = new Font("Arial", 12, FontStyle.Bold);
                StringFormat format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString(name, font, Brushes.Black,
                    new RectangleF(0, 0, width, height), format);
            }
            return bitmap;
        }

        private void UpdatePanelSize()
        {
            if (screenshotItems.Count > 0)
            {
                int contentWidth = GetContentWidth();
                int contentHeight = GetContentHeight(contentWidth);
                int totalHeight = panelScreenshots.Padding.Vertical + screenshotItems.Count * (contentHeight + screenshotSpacing);
                panelScreenshots.AutoScrollMinSize = new Size(0, totalHeight);
            }
        }

        private void OnScreenshotClick(string screenshotName)
        {
            ShowStatusMessage($"Выбран скриншот: {screenshotName}", MessageType.Info);
        }

        private int GetContentWidth()
        {
            // Делаем ширину контента на пару пикселей меньше клиентской ширины, чтобы исключить горизонтальную прокрутку
            int clientWidth = Math.Max(0, panelScreenshots.ClientSize.Width - panelScreenshots.Padding.Horizontal);
            return Math.Max(50, clientWidth - 2);
        }

        private int GetContentHeight(int contentWidth)
        {
            // Сохраняем базовое соотношение сторон
            float aspect = (float)baseScreenshotHeight / baseScreenshotWidth;
            return Math.Max(40, (int)(contentWidth * aspect));
        }

        private void LayoutScreenshots()
        {
            int contentWidth = GetContentWidth();
            int contentHeight = GetContentHeight(contentWidth);

            int y = panelScreenshots.Padding.Top;

            foreach (var item in screenshotItems)
            {
                var pictureBox = item.Picture;
                var label = item.Label;
                var btnDelete = item.Delete;

                pictureBox.Location = new Point(panelScreenshots.Padding.Left, y);
                pictureBox.Size = new Size(contentWidth, contentHeight);

                label.Location = new Point(pictureBox.Left, pictureBox.Bottom + 2);
                label.Size = new Size(contentWidth, 15);

                // Кнопка удаления позиционируется внутри превью, без отступов
                btnDelete.Location = new Point(pictureBox.Width - btnDelete.Width, 0);
                btnDelete.BringToFront();

                y = pictureBox.Bottom + 2 + 15 + screenshotSpacing;
            }

            panelScreenshots.AutoScrollMinSize = new Size(0, Math.Max(0, y + panelScreenshots.Padding.Bottom));
            // Индикатор линии убираем при релайауте
            HideDropIndicator();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            // Дополнительная инициализация при загрузке формы
        }

        // --- Drag & Drop перестановка миниатюр ---
        private void PictureBox_MouseDownStartDrag(object? sender, MouseEventArgs e)
        {
            if (sender is PictureBox pb && e.Button == MouseButtons.Left)
            {
                int idx = screenshotItems.FindIndex(it => it.Picture == pb);
                if (idx >= 0)
                {
                    dragSourceIndex = idx;
                    pb.DoDragDrop(pb, DragDropEffects.Move);
                }
            }
        }

        private void PanelScreenshots_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(typeof(PictureBox)))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void PanelScreenshots_DragOver(object? sender, DragEventArgs e)
        {
            if (e.Data == null || !e.Data.GetDataPresent(typeof(PictureBox))) return;

            Point clientPoint = panelScreenshots.PointToClient(new Point(e.X, e.Y));
            int indicatorY;
            int insertIndex = ComputeInsertIndexAndY(clientPoint, out indicatorY);
            if (insertIndex < 0) { HideDropIndicator(); dragInsertIndex = null; return; }

            dragInsertIndex = insertIndex;
            ShowDropIndicator(indicatorY);
            e.Effect = DragDropEffects.Move;
        }

        private void PanelScreenshots_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data == null || !e.Data.GetDataPresent(typeof(PictureBox))) return;
            if (dragSourceIndex is null) return;

            var pb = (PictureBox?)e.Data.GetData(typeof(PictureBox));
            if (pb == null) return;

            HideDropIndicator();

            Point clientPoint = panelScreenshots.PointToClient(new Point(e.X, e.Y));
            int indicatorY;
            int insertIndex = dragInsertIndex ?? ComputeInsertIndexAndY(clientPoint, out indicatorY);
            dragInsertIndex = null;
            
            if (insertIndex < 0) 
            { 
                dragSourceIndex = null;
                ShowStatusMessage("Перемещение отменено", MessageType.Info);
                return; 
            }

            int sourceIndex = dragSourceIndex.Value;
            string screenshotName = screenshotItems[sourceIndex].Label.Text;
            dragSourceIndex = null;

            var item = screenshotItems[sourceIndex];
            screenshotItems.RemoveAt(sourceIndex);
            if (insertIndex > sourceIndex) insertIndex--; // поправка после удаления исходного
            if (insertIndex < 0) insertIndex = 0;
            if (insertIndex > screenshotItems.Count) insertIndex = screenshotItems.Count;
            screenshotItems.Insert(insertIndex, item);

            LayoutScreenshots();
            
            int newPosition = insertIndex + 1;
            int oldPosition = sourceIndex + 1;
            ShowStatusMessage($"Скриншот '{screenshotName}' перемещен с позиции {oldPosition} на позицию {newPosition}", MessageType.Success);
        }

        private int ComputeInsertIndexAndY(Point p, out int indicatorY)
        {
            indicatorY = -1;
            if (screenshotItems.Count == 0)
            {
                indicatorY = panelScreenshots.Padding.Top;
                return 0;
            }

            for (int i = 0; i < screenshotItems.Count; i++)
            {
                var pic = screenshotItems[i].Picture;
                int blockTop = pic.Top;
                int blockBottom = pic.Bottom + 2 + 15; // включая подпись
                int mid = (blockTop + blockBottom) / 2;

                if (p.Y < blockTop)
                {
                    indicatorY = Math.Max(panelScreenshots.Padding.Top, blockTop - screenshotSpacing / 2);
                    return i;
                }
                if (p.Y >= blockTop && p.Y < mid)
                {
                    indicatorY = Math.Max(panelScreenshots.Padding.Top, blockTop - screenshotSpacing / 2);
                    return i;
                }
                if (p.Y >= mid && p.Y <= blockBottom)
                {
                    indicatorY = blockBottom + screenshotSpacing / 2;
                    return i + 1;
                }
            }

            // Ниже последнего
            var last = screenshotItems[^1].Picture;
            indicatorY = last.Bottom + 2 + 15 + screenshotSpacing / 2;
            return screenshotItems.Count;
        }

        private void ShowDropIndicator(int y)
        {
            if (dropIndicatorLine == null)
            {
                dropIndicatorLine = new Panel
                {
                    Height = 3,
                    BackColor = Color.Red,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right
                };
                panelScreenshots.Controls.Add(dropIndicatorLine);
            }
            dropIndicatorLine.Location = new Point(panelScreenshots.Padding.Left, y);
            dropIndicatorLine.Width = GetContentWidth();
            dropIndicatorLine.BringToFront();
        }

        private void HideDropIndicator()
        {
            if (dropIndicatorLine != null)
            {
                panelScreenshots.Controls.Remove(dropIndicatorLine);
                dropIndicatorLine.Dispose();
                dropIndicatorLine = null;
            }
        }
    }
}