using Client.WinForms.Services;
using System.Drawing.Drawing2D;
using MessageType = Client.WinForms.Services.MessageType;

namespace Client.WinForms
{
    public partial class FormMain : Form
    {
        private static readonly List<(PictureBox Picture, Label Label, Button Delete)> value = [];
        private readonly List<(PictureBox Picture, Label Label, Button Delete)> frameItems = value;
        private readonly int frameSpacing = 10;
        private readonly int baseFrameWidth = 180;
        private readonly int baseFrameHeight = 120;
        private Padding panelPadding = new(10);
        private readonly int exportImageWidth = 1000;
        private readonly int exportPadding = 20;
        private readonly int exportFontSize = 14;
        private int? dragSourceIndex = null;
        private Panel? dropIndicatorLine = null; // линия-индикатор позиции вставки
        private int? dragInsertIndex = null; // индекс вставки во время DragOver
        private bool isEditEnabled = false;

        // Сервисы
        private readonly ITextProcessingService _textProcessingService;
        private readonly IImageRenderingService _imageRenderingService;
        private readonly IOcrService _ocrService;
        private readonly IFileService _fileService;
        private readonly IStatusMessageService _statusMessageService;

        public FormMain()
        {
            InitializeComponent();
            
            // Инициализация сервисов
            _textProcessingService = new TextProcessingService();
            _imageRenderingService = new ImageRenderingService();
            _ocrService = new OcrService();
            _fileService = new FileService();
            _statusMessageService = new StatusMessageService();
            
            InitializeFramePanel();
        }

        private void InitializeFramePanel()
        {
            // Настройка панели для кадров
            panelScreenshots.AutoScroll = true;
            panelScreenshots.BackColor = Color.LightGray;
            panelScreenshots.BorderStyle = BorderStyle.FixedSingle;
            panelScreenshots.Padding = panelPadding;

            // Перелайаут при изменении размеров, чтобы не появлялась горизонтальная прокрутка
            panelScreenshots.Resize += (_, __) => LayoutFrames();

            // Включаем Drag & Drop для перестановки
            panelScreenshots.AllowDrop = true;
            panelScreenshots.DragEnter += PanelScreenshots_DragEnter;
            panelScreenshots.DragOver += PanelScreenshots_DragOver;
            panelScreenshots.DragDrop += PanelScreenshots_DragDrop;
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            // Дополнительная инициализация при загрузке формы
        }

        private void ShowStatusMessage(string message, MessageType messageType = MessageType.Info)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ShowStatusMessage(message, messageType)));
                return;
            }

            var (color, iconSymbol) = _statusMessageService.GetStatusConfig(messageType);

            // Обновляем иконку
            if (statusIconLabel.Image != null)
            {
                statusIconLabel.Image.Dispose();
            }
            statusIconLabel.Image = _statusMessageService.CreateStatusIcon(iconSymbol, color, 22);

            statusLabel.Text = message;
            statusLabel.ForeColor = color;
        }

        private Button CreateDeleteButton(PictureBox pictureBox)
        {
            var btnDelete = new Button
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

            // Крестик размещаем внутри превью
            btnDelete.Parent = pictureBox;
            btnDelete.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDelete.Location = new Point(pictureBox.Width - btnDelete.Width, 0);
            pictureBox.Controls.Add(btnDelete);

            return btnDelete;
        }

        private (PictureBox Picture, Label Label, Button Delete) CreateFrameControls(string name, int contentWidth, int contentHeight)
        {
            // Создаем PictureBox для кадра
            var pictureBox = new PictureBox
            {
                Size = new Size(contentWidth, contentHeight),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Location = new Point(panelScreenshots.Padding.Left, panelScreenshots.Padding.Top + frameItems.Count * (contentHeight + frameSpacing))
            };

            // Добавляем подпись
            var label = new Label
            {
                Text = name,
                Location = new Point(pictureBox.Left, pictureBox.Bottom + 2),
                Size = new Size(contentWidth, 15),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 8),
                AutoSize = false
            };

            // Создаем кнопку удаления
            var btnDelete = CreateDeleteButton(pictureBox);

            // Добавляем обработчики событий
            pictureBox.Click += (sender, e) => OnFrameClick(name);
            pictureBox.MouseDown += PictureBox_MouseDownStartDrag;

            return (pictureBox, label, btnDelete);
        }

        public void AddFrame(Bitmap image, string name)
        {
            var contentWidth = GetContentWidth();
            var contentHeight = GetContentHeight(contentWidth);

            var (pictureBox, label, btnDelete) = CreateFrameControls(name, contentWidth, contentHeight);

            pictureBox.Image = (Bitmap)image.Clone();

            // Добавляем элементы на панель
            panelScreenshots.Controls.Add(pictureBox);
            panelScreenshots.Controls.Add(label);
            frameItems.Add((pictureBox, label, btnDelete));

            UpdatePanelSize();
            LayoutFrames();
        }

        private void OnDeleteItemClick(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is PictureBox pb)
            {
                var idx = frameItems.FindIndex(it => it.Picture == pb);
                if (idx >= 0)
                {
                    var item = frameItems[idx];
                    var frameName = item.Label.Text;
                    
                    ShowStatusMessage($"Удаление кадра '{frameName}'...", MessageType.Info);
                    
                    panelScreenshots.Controls.Remove(item.Picture);
                    panelScreenshots.Controls.Remove(item.Label);
                    // delete button удаляется вместе с pictureBox, но очищаем на всякий случай
                    item.Picture.Controls.Remove(item.Delete);
                    item.Picture.Dispose();
                    item.Label.Dispose();
                    item.Delete.Dispose();
                    frameItems.RemoveAt(idx);
                    UpdatePanelSize();
                    LayoutFrames();
                    
                    ShowStatusMessage($"Кадр '{frameName}' успешно удален", MessageType.Success);
                }
            }
        }

        private async void btnCreateFrame_Click(object? sender, EventArgs e)
        {
            ShowStatusMessage("Начало создания кадра...", MessageType.Info);
            
            // Скрываем форму для захвата экрана
            WindowState = FormWindowState.Minimized;
            Hide();
            
            // Небольшая задержка для полного скрытия формы
            await Task.Delay(300);
            
            try
            {
                var frame = CaptureScreenArea();
                if (frame != null)
                {
                    var timestamp = DateTime.Now.ToString("HH-mm-ss");
                    AddFrame(frame, $"Кадр {timestamp}");
                    ShowStatusMessage("Кадр успешно создан", MessageType.Success);
                }
                else
                {
                    ShowStatusMessage("Создание кадра отменено", MessageType.Info);
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Ошибка захвата экрана: {ex.Message}", MessageType.Error);
            }
            finally
            {
                // Показываем форму обратно
                Show();
                WindowState = FormWindowState.Normal;
                BringToFront();
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

            var selectedArea = Rectangle.Empty;
            var isCapturing = false;
            var startPoint = Point.Empty;

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
                    var x = Math.Min(startPoint.X, e.X);
                    var y = Math.Min(startPoint.Y, e.Y);
                    var width = Math.Abs(e.X - startPoint.X);
                    var height = Math.Abs(e.Y - startPoint.Y);
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
                var tessdataPath = Path.Combine(AppContext.BaseDirectory, "tessdata");
                if (!Directory.Exists(tessdataPath))
                {
                    ShowStatusMessage($"Не найдена папка tessdata. Разместите файлы языковых моделей (rus.traineddata, eng.traineddata) в папке: {tessdataPath}", MessageType.Error);
                    return;
                }

                // Подготавливаем коллекцию Bitmap из frameItems
                var bitmaps = frameItems
                    .Where(item => item.Picture.Image is Bitmap)
                    .Select(item => item.Picture.Image as Bitmap)
                    .Where(bmp => bmp != null)
                    .ToList();

                var text = await Task.Run(() => _ocrService.RunOcrAndBuildText(bitmaps, tessdataPath));
                
                if (string.IsNullOrWhiteSpace(text))
                {
                    ShowStatusMessage("Текст не найден на изображениях.", MessageType.Warning);
                    return;
                }
                
                // Только при успешном считывании обновляем текст
                if (chkNormalize.Checked)
                {
                    text = _textProcessingService.NormalizeText(text);
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
                var textToRender = chkNormalize.Checked ? _textProcessingService.NormalizeText(richText.Text) : richText.Text;
                if (string.IsNullOrWhiteSpace(textToRender))
                {
                    ShowStatusMessage("Нет текста для формирования картинки.", MessageType.Info);
                    return;
                }

                var img = _imageRenderingService.RenderTextToBitmap(textToRender, exportImageWidth, exportPadding, exportFontSize);
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


        private void btnSaveTxt_Click(object? sender, EventArgs e)
        {
            ShowStatusMessage("Начало сохранения текста...", MessageType.Info);
            
            try
            {
                var filePath = _fileService.SaveTextWithTimestamp(richText.Text);
                ShowStatusMessage($"Сохранено: {filePath}", MessageType.Success);
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

                var filePath = _fileService.SaveImageWithTimestamp((Bitmap)picturePreview.Image);
                ShowStatusMessage($"Сохранено: {filePath}", MessageType.Success);
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Ошибка сохранения PNG: {ex.Message}", MessageType.Error);
            }
        }

        private void btnAddFromFile_Click(object? sender, EventArgs e)
        {
            ShowStatusMessage("Начало добавления изображений...", MessageType.Info);
            
            var defaultFolder = @"C:\Users\gorba_6ku4vx\OneDrive\Документы\Frame Manager";
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
                var successCount = 0;
                foreach (var path in ofd.FileNames)
                {
                    try
                    {
                        using var img = new Bitmap(path);
                        AddFrame(img, Path.GetFileNameWithoutExtension(path));
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
                _fileService.OpenFrameManagerFolder();
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

            var successCount = 0;
            foreach (var path in ofd.FileNames)
            {
                try
                {
                    var content = _fileService.ReadTextFile(path);

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

        private void UpdatePanelSize()
        {
            if (frameItems.Count > 0)
            {
                var contentWidth = GetContentWidth();
                var contentHeight = GetContentHeight(contentWidth);
                var totalHeight = panelScreenshots.Padding.Vertical + frameItems.Count * (contentHeight + frameSpacing);
                panelScreenshots.AutoScrollMinSize = new Size(0, totalHeight);
            }
        }

        private void OnFrameClick(string frameName)
        {
            ShowStatusMessage($"Выбран кадр: {frameName}", MessageType.Info);
        }

        private int GetContentWidth()
        {
            // Делаем ширину контента на пару пикселей меньше клиентской ширины, чтобы исключить горизонтальную прокрутку
            var clientWidth = Math.Max(0, panelScreenshots.ClientSize.Width - panelScreenshots.Padding.Horizontal);
            return Math.Max(50, clientWidth - 2);
        }

        private int GetContentHeight(int contentWidth)
        {
            // Сохраняем базовое соотношение сторон
            var aspect = (float)baseFrameHeight / baseFrameWidth;
            return Math.Max(40, (int)(contentWidth * aspect));
        }

        private void LayoutFrames()
        {
            var contentWidth = GetContentWidth();
            var contentHeight = GetContentHeight(contentWidth);

            var y = panelScreenshots.Padding.Top;

            foreach (var item in frameItems)
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

                y = pictureBox.Bottom + 2 + 15 + frameSpacing;
            }

            panelScreenshots.AutoScrollMinSize = new Size(0, Math.Max(0, y + panelScreenshots.Padding.Bottom));
            // Индикатор линии убираем при релайауте
            HideDropIndicator();
        }

        // --- Drag & Drop перестановка миниатюр ---
        private void PictureBox_MouseDownStartDrag(object? sender, MouseEventArgs e)
        {
            if (sender is PictureBox pb && e.Button == MouseButtons.Left)
            {
                var idx = frameItems.FindIndex(it => it.Picture == pb);
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

            var clientPoint = panelScreenshots.PointToClient(new Point(e.X, e.Y));
            int indicatorY;
            var insertIndex = ComputeInsertIndexAndY(clientPoint, out indicatorY);
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

            var clientPoint = panelScreenshots.PointToClient(new Point(e.X, e.Y));
            int indicatorY;
            var insertIndex = dragInsertIndex ?? ComputeInsertIndexAndY(clientPoint, out indicatorY);
            dragInsertIndex = null;
            
            if (insertIndex < 0) 
            { 
                dragSourceIndex = null;
                ShowStatusMessage("Перемещение отменено", MessageType.Info);
                return; 
            }

            var sourceIndex = dragSourceIndex.Value;
            var frameName = frameItems[sourceIndex].Label.Text;
            dragSourceIndex = null;

            var item = frameItems[sourceIndex];
            frameItems.RemoveAt(sourceIndex);
            if (insertIndex > sourceIndex) insertIndex--; // поправка после удаления исходного
            if (insertIndex < 0) insertIndex = 0;
            if (insertIndex > frameItems.Count) insertIndex = frameItems.Count;
            frameItems.Insert(insertIndex, item);

            LayoutFrames();
            
            var newPosition = insertIndex + 1;
            var oldPosition = sourceIndex + 1;
            ShowStatusMessage($"Кадр '{frameName}' перемещен с позиции {oldPosition} на позицию {newPosition}", MessageType.Success);
        }

        private int ComputeInsertIndexAndY(Point p, out int indicatorY)
        {
            indicatorY = -1;
            if (frameItems.Count == 0)
            {
                indicatorY = panelScreenshots.Padding.Top;
                return 0;
            }

            for (var i = 0; i < frameItems.Count; i++)
            {
                var pic = frameItems[i].Picture;
                var blockTop = pic.Top;
                var blockBottom = pic.Bottom + 2 + 15; // включая подпись
                var mid = (blockTop + blockBottom) / 2;

                if (p.Y < blockTop)
                {
                    indicatorY = Math.Max(panelScreenshots.Padding.Top, blockTop - frameSpacing / 2);
                    return i;
                }
                if (p.Y >= blockTop && p.Y < mid)
                {
                    indicatorY = Math.Max(panelScreenshots.Padding.Top, blockTop - frameSpacing / 2);
                    return i;
                }
                if (p.Y >= mid && p.Y <= blockBottom)
                {
                    indicatorY = blockBottom + frameSpacing / 2;
                    return i + 1;
                }
            }

            // Ниже последнего
            var last = frameItems[^1].Picture;
            indicatorY = last.Bottom + 2 + 15 + frameSpacing / 2;
            return frameItems.Count;
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