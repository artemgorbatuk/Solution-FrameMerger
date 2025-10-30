using Client.WinForms.Services;
using System.Drawing.Drawing2D;
using static Client.WinForms.Services.StatusMessages;
using MessageType = Client.WinForms.Services.MessageType;

namespace Client.WinForms
{
    public partial class FormMain : Form
    {
        private readonly List<(PictureBox Picture, Label Label, Button Delete)> frameControls = [];
        private readonly Dictionary<Guid, (Bitmap Original, Bitmap Thumbnail)> frameIdToBitmaps = new();
        private readonly int frameSpacing = 10;
        private readonly int baseThumbnailWidth = 180;
        private readonly int baseThumbnailHeight = 120;
        private Padding framesPanelPadding = new(10);
        private readonly int formExportImageTargetWidth = 1000;
        private readonly int formExportImagePadding = 20;
        private readonly int formExportImageFontSize = 14;
        private int? dragSourceFrameIndex = null;
        private Panel? dropInsertIndicatorLine = null; // линия-индикатор позиции вставки
        private int? dragInsertFrameIndex = null; // индекс вставки во время DragOver
        private bool isTextEditEnabled = false;
        private readonly string defaultFramesFolderPath = @"C:\Users\gorba_6ku4vx\OneDrive\Документы\Frame Manager";

        // Сервисы
        private readonly ITextProcessingService _textProcessingService = new TextProcessingService();
        private readonly IImageRenderingService _imageRenderingService = new ImageRenderingService();
        private readonly IOcrService _ocrService = new TesseractOcrService();
        private readonly IFileService _fileService = new FileService();
        private readonly IStatusMessageService _statusMessageService = new StatusMessageService();

        public FormMain()
        {
            InitializeComponent();
            InitializeFramePanel();
        }

        private void InitializeFramePanel()
        {
            // Настройка панели для кадров
            panelFrames.AutoScroll = true;
            panelFrames.BackColor = Color.LightGray;
            panelFrames.BorderStyle = BorderStyle.FixedSingle;
            panelFrames.Padding = framesPanelPadding;

            // Перелайаут при изменении размеров, чтобы не появлялась горизонтальная прокрутка
            panelFrames.Resize += (_, __) => LayoutFrames();

            // Включаем Drag & Drop для перестановки
            panelFrames.AllowDrop = true;
            panelFrames.DragEnter += PanelFrames_DragEnter;
            panelFrames.DragOver += PanelFrames_DragOver;
            panelFrames.DragDrop += PanelFrames_DragDrop;
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
                Padding = Padding.Empty,
                Tag = pictureBox,
                Parent = pictureBox,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(pictureBox.Width - 22, 0)
            };
            
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += OnDeleteItemClick;
            btnDelete.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using var f = new Font("Segoe UI", 13f, FontStyle.Bold, GraphicsUnit.Point);
                using var br = new SolidBrush(Color.White);
                pe.Graphics.DrawString("×", f, br, new RectangleF(0, 0, ((Control)s!).Width, ((Control)s!).Height), sf);
            };

            pictureBox.Controls.Add(btnDelete);
            return btnDelete;
        }

        private (PictureBox Picture, Label Label, Button Delete) CreateFrameControls(string name, int thumbnailWidth, int thumbnailHeight)
        {
            // Создаем PictureBox для кадра
            var pictureBox = new PictureBox
            {
                Size = new Size(thumbnailWidth, thumbnailHeight),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Location = new Point(panelFrames.Padding.Left, panelFrames.Padding.Top + frameControls.Count * (thumbnailHeight + frameSpacing))
            };

            // Добавляем подпись
            var label = new Label
            {
                Text = name,
                Location = new Point(pictureBox.Left, pictureBox.Bottom + 2),
                Size = new Size(thumbnailWidth, 15),
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
            var thumbnailWidth = GetThumbnailWidth();
            var thumbnailHeight = GetThumbnailHeight(thumbnailWidth);

            var (pictureBox, label, btnDelete) = CreateFrameControls(name, thumbnailWidth, thumbnailHeight);

            // Подготавливаем данные кадра: оригинал + миниатюра
            var frameId = Guid.NewGuid();
            var frameOriginal = (Bitmap)image.Clone();
            var frameThumbnail = CreateThumbnail(frameOriginal, thumbnailWidth, thumbnailHeight);

            frameIdToBitmaps[frameId] = (frameOriginal, frameThumbnail);

            pictureBox.Tag = frameId;
            pictureBox.Image = (Bitmap)frameThumbnail.Clone();

            // Добавляем элементы на панель
            panelFrames.Controls.Add(pictureBox);
            panelFrames.Controls.Add(label);
            frameControls.Add((pictureBox, label, btnDelete));

            UpdatePanelSize();
            LayoutFrames();
        }

        private void OnDeleteItemClick(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is PictureBox pb)
            {
                var frameIndex = frameControls.FindIndex(it => it.Picture == pb);
                if (frameIndex >= 0)
                {
                    var frameControl = frameControls[frameIndex];
                    var frameName = frameControl.Label.Text;
                    var frameId = (Guid?)frameControl.Picture.Tag;
                    
                    ShowStatusMessage(string.Format(FrameDeletionStart, frameName), MessageType.Info);
                    
                    panelFrames.Controls.Remove(frameControl.Picture);
                    panelFrames.Controls.Remove(frameControl.Label);
                    // delete button удаляется вместе с pictureBox, но очищаем на всякий случай
                    frameControl.Picture.Controls.Remove(frameControl.Delete);
                    // Освобождаем ресурсы превью внутри PictureBox
                    if (frameControl.Picture.Image != null)
                    {
                        frameControl.Picture.Image.Dispose();
                        frameControl.Picture.Image = null;
                    }
                    frameControl.Picture.Dispose();
                    frameControl.Label.Dispose();
                    frameControl.Delete.Dispose();
                    frameControls.RemoveAt(frameIndex);

                    // Удаляем из словаря и освобождаем оригинал и миниатюру
                    if (frameId.HasValue && frameIdToBitmaps.TryGetValue(frameId.Value, out var data))
                    {
                        data.Original.Dispose();
                        data.Thumbnail.Dispose();
                        frameIdToBitmaps.Remove(frameId.Value);
                    }
                    UpdatePanelSize();
                    LayoutFrames();
                    
                    ShowStatusMessage(string.Format(FrameDeletedSuccessfully, frameName), MessageType.Success);
                }
            }
        }

        private async void btnCreateFrame_Click(object? sender, EventArgs e)
        {
            ShowStatusMessage(FrameCreationStart, MessageType.Info);
            
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
                    ShowStatusMessage(FrameCreatedSuccessfully, MessageType.Success);
                }
                else
                {
                    ShowStatusMessage(FrameCreationCancelled, MessageType.Info);
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage(string.Format(ScreenCaptureError, ex.Message), MessageType.Error);
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
                var screenFrame = new Bitmap(selectedArea.Width, selectedArea.Height);
                using (var screenFrameGraphics = Graphics.FromImage(screenFrame))
                {
                    screenFrameGraphics.CopyFromScreen(selectedArea.Location, Point.Empty, selectedArea.Size);
                }
                return screenFrame;
            }

            return null;
        }

        // =========== OCR + TEXT PIPELINE ===========
        private async void btnFormText_Click(object? sender, EventArgs e)
        {
            ShowStatusMessage(TextFormationStart, MessageType.Info);
            
            try
            {
                // Проверяем наличие tessdata перед запуском
                var tessdataPath = Path.Combine(AppContext.BaseDirectory, "tessdata");
                if (!Directory.Exists(tessdataPath))
                {
                    ShowStatusMessage(string.Format(TesseractNotFound, tessdataPath), MessageType.Error);
                    return;
                }

                // Формируем коллекцию оригиналов в порядке отображения
                var originalFrames = frameControls
                    .Select(item => item.Picture.Tag)
                    .OfType<Guid>()
                    .Select(id => frameIdToBitmaps.TryGetValue(id, out var data) ? data.Original : null)
                    .Where(bmp => bmp != null)
                    .ToList();

                var text = await Task.Run(() => _ocrService.RunOcrAndBuildText(originalFrames, tessdataPath));
                
                if (string.IsNullOrWhiteSpace(text))
                {
                    ShowStatusMessage(TextNotFoundOnImages, MessageType.Warning);
                    return;
                }
                
                // Только при успешном считывании обновляем текст
                if (chkNormalize.Checked)
                {
                    text = _textProcessingService.NormalizeText(text);
                }
                richText.Text = text;
                ShowStatusMessage(TextFormedSuccessfully, MessageType.Success);
            }
            catch (Exception ex)
            {
                ShowStatusMessage(string.Format(OcrError, ex.Message), MessageType.Error);
            }
        }

        private void btnFormImage_Click(object? sender, EventArgs e)
        {
            ShowStatusMessage(ImageFormationStart, MessageType.Info);
            
            try
            {
                var textToRender = chkNormalize.Checked ? _textProcessingService.NormalizeText(richText.Text) : richText.Text;
                if (string.IsNullOrWhiteSpace(textToRender))
                {
                    ShowStatusMessage(NoTextForImageGeneration, MessageType.Info);
                    return;
                }

                var finalFrame = _imageRenderingService.RenderTextToBitmap(textToRender, formExportImageTargetWidth, formExportImagePadding, formExportImageFontSize);
                // Освобождаем старую картинку, если была
                if (picturePreview.Image != null)
                {
                    picturePreview.Image.Dispose();
                }
                picturePreview.Image = finalFrame;
                picturePreview.Size = finalFrame.Size;
                tabControl.SelectedTab = tabImage;
                ShowStatusMessage(ImageFormedSuccessfully, MessageType.Success);
            }
            catch (Exception ex)
            {
                ShowStatusMessage(string.Format(ImageGenerationError, ex.Message), MessageType.Error);
            }
        }


        private void btnSaveTxt_Click(object? sender, EventArgs e)
        {
            ShowStatusMessage(TextSaveStart, MessageType.Info);
            
            try
            {
                var filePath = _fileService.SaveTextWithTimestamp(richText.Text);
                ShowStatusMessage(string.Format(FileSaved, filePath), MessageType.Success);
            }
            catch (Exception ex)
            {
                ShowStatusMessage(string.Format(TextSaveError, ex.Message), MessageType.Error);
            }
        }

        private void btnSavePng_Click(object? sender, EventArgs e)
        {
            ShowStatusMessage(ImageSaveStart, MessageType.Info);
            
            try
            {
                if (picturePreview.Image == null)
                {
                    ShowStatusMessage(NoImageToSave, MessageType.Info);
                    return;
                }

                var filePath = _fileService.SaveImageWithTimestamp((Bitmap)picturePreview.Image);
                ShowStatusMessage(string.Format(FileSaved, filePath), MessageType.Success);
            }
            catch (Exception ex)
            {
                ShowStatusMessage(string.Format(ImageSaveError, ex.Message), MessageType.Error);
            }
        }

        private void btnAddFromFile_Click(object? sender, EventArgs e)
        {
            ShowStatusMessage(ImageLoadStart, MessageType.Info);
            
            if (!Directory.Exists(defaultFramesFolderPath))
            {
                Directory.CreateDirectory(defaultFramesFolderPath);
            }
            
            using OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "Выберите изображения",
                Filter = "Изображения|*.png;*.jpg;*.jpeg;*.bmp;*.gif",
                Multiselect = true,
                InitialDirectory = defaultFramesFolderPath
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
                        ShowStatusMessage(string.Format(ImageLoadError, path, ex.Message), MessageType.Error);
                    }
                }
                
                if (successCount > 0)
                {
                    ShowStatusMessage(string.Format(ImagesLoadedSuccessfully, successCount), MessageType.Success);
                }
            }
            else
            {
                ShowStatusMessage(ImageLoadCancelled, MessageType.Info);
            }
        }

        private void btnOpenFolder_Click(object? sender, EventArgs e)
        {
            ShowStatusMessage(FolderOpenStart, MessageType.Info);
            
            try
            {
                _fileService.OpenFrameManagerFolder();
                ShowStatusMessage(FolderOpenedSuccessfully, MessageType.Success);
            }
            catch (Exception ex)
            {
                ShowStatusMessage(string.Format(FolderOpenError, ex.Message), MessageType.Error);
            }
        }

        private void btnToggleEdit_Click(object? sender, EventArgs e)
        {
            ShowStatusMessage(EditModeToggle, MessageType.Info);
            
            isTextEditEnabled = !isTextEditEnabled;
            richText.ReadOnly = !isTextEditEnabled;
            richText.BackColor = isTextEditEnabled ? Color.White : SystemColors.Control;
            btnToggleEdit.Text = isTextEditEnabled ? "Запретить редакт." : "Разрешить редакт.";
            
            ShowStatusMessage(isTextEditEnabled ? EditEnabled : EditDisabled, MessageType.Success);
        }

        private void btnAddText_Click(object? sender, EventArgs e)
        {
            ShowStatusMessage(TextLoadStart, MessageType.Info);
            
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
                ShowStatusMessage(TextLoadCancelled, MessageType.Info);
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
                    ShowStatusMessage(string.Format(TextReadError, path, ex.Message), MessageType.Error);
                }
            }
            
            if (successCount > 0)
            {
                ShowStatusMessage(string.Format(TextFilesLoadedSuccessfully, successCount), MessageType.Success);
            }
        }

        private void UpdatePanelSize()
        {
            if (frameControls.Count > 0)
            {
                var thumbnailWidth = GetThumbnailWidth();
                var thumbnailHeight = GetThumbnailHeight(thumbnailWidth);
                var totalHeight = panelFrames.Padding.Vertical + frameControls.Count * (thumbnailHeight + frameSpacing);
                panelFrames.AutoScrollMinSize = new Size(0, totalHeight);
            }
        }

        private void OnFrameClick(string frameName) => 
            ShowStatusMessage(string.Format(FrameSelected, frameName), MessageType.Info);

        private int GetThumbnailWidth()
        {
            // Делаем ширину контента на пару пикселей меньше клиентской ширины, чтобы исключить горизонтальную прокрутку
            var clientWidth = Math.Max(0, panelFrames.ClientSize.Width - panelFrames.Padding.Horizontal);
            return Math.Max(50, clientWidth - 2);
        }

        private int GetThumbnailHeight(int thumbnailWidth)
        {
            // Сохраняем базовое соотношение сторон
            var aspect = (float)baseThumbnailHeight / baseThumbnailWidth;
            return Math.Max(40, (int)(thumbnailWidth * aspect));
        }

        private void LayoutFrames()
        {
            var thumbnailWidth = GetThumbnailWidth();
            var thumbnailHeight = GetThumbnailHeight(thumbnailWidth);

            var y = panelFrames.Padding.Top;

            foreach (var frameControl in frameControls)
            {
                var pictureBox = frameControl.Picture;
                var label = frameControl.Label;
                var btnDelete = frameControl.Delete;

                pictureBox.Location = new Point(panelFrames.Padding.Left, y);
                pictureBox.Size = new Size(thumbnailWidth, thumbnailHeight);

                label.Location = new Point(pictureBox.Left, pictureBox.Bottom + 2);
                label.Size = new Size(thumbnailWidth, 15);

                // Кнопка удаления позиционируется внутри превью, без отступов
                btnDelete.Location = new Point(pictureBox.Width - btnDelete.Width, 0);
                btnDelete.BringToFront();

                y = pictureBox.Bottom + 2 + 15 + frameSpacing;
            }

            panelFrames.AutoScrollMinSize = new Size(0, Math.Max(0, y + panelFrames.Padding.Bottom));
            // Индикатор линии убираем при релайауте
            HideDropIndicator();
        }

        private Bitmap CreateThumbnail(Bitmap originalFrame, int targetWidth, int targetHeight)
        {
            var srcWidth = originalFrame.Width;
            var srcHeight = originalFrame.Height;
            if (srcWidth <= 0 || srcHeight <= 0) return new Bitmap(targetWidth, targetHeight);

            var scale = Math.Min((float)targetWidth / srcWidth, (float)targetHeight / srcHeight);
            var drawWidth = (int)Math.Max(1, srcWidth * scale);
            var drawHeight = (int)Math.Max(1, srcHeight * scale);

            var thumbnailFrame = new Bitmap(drawWidth, drawHeight);
            using (var graphics = Graphics.FromImage(thumbnailFrame))
            {
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.Clear(Color.White);
                graphics.DrawImage(originalFrame, new Rectangle(0, 0, drawWidth, drawHeight));
            }
            return thumbnailFrame;
        }

        // --- Drag & Drop перестановка миниатюр ---
        private void PictureBox_MouseDownStartDrag(object? sender, MouseEventArgs e)
        {
            if (sender is PictureBox pb && e.Button == MouseButtons.Left)
            {
                var frameIndex = frameControls.FindIndex(it => it.Picture == pb);
                if (frameIndex >= 0)
                {
                    dragSourceFrameIndex = frameIndex;
                    pb.DoDragDrop(pb, DragDropEffects.Move);
                }
            }
        }

private void PanelFrames_DragEnter(object? sender, DragEventArgs e)
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

private void PanelFrames_DragOver(object? sender, DragEventArgs e)
        {
            if (e.Data == null || !e.Data.GetDataPresent(typeof(PictureBox))) return;

            var clientPoint = panelFrames.PointToClient(new Point(e.X, e.Y));
            int indicatorY;
            var insertIndex = ComputeInsertIndexAndY(clientPoint, out indicatorY);
            if (insertIndex < 0) { HideDropIndicator(); dragInsertFrameIndex = null; return; }

            dragInsertFrameIndex = insertIndex;
            ShowDropIndicator(indicatorY);
            e.Effect = DragDropEffects.Move;
        }

private void PanelFrames_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data == null || !e.Data.GetDataPresent(typeof(PictureBox))) return;
            if (dragSourceFrameIndex is null) return;

            var framePictureBox = (PictureBox?)e.Data.GetData(typeof(PictureBox));
            if (framePictureBox == null) return;

            HideDropIndicator();

            var clientPoint = panelFrames.PointToClient(new Point(e.X, e.Y));
            int indicatorY;
            var insertIndex = dragInsertFrameIndex ?? ComputeInsertIndexAndY(clientPoint, out indicatorY);
            dragInsertFrameIndex = null;
            
            if (insertIndex < 0) 
            { 
                dragSourceFrameIndex = null;
                ShowStatusMessage(MoveCancelled, MessageType.Info);
                return; 
            }

            var sourceIndex = dragSourceFrameIndex.Value;
            var frameName = frameControls[sourceIndex].Label.Text;
            dragSourceFrameIndex = null;

            var item = frameControls[sourceIndex];
            frameControls.RemoveAt(sourceIndex);
            if (insertIndex > sourceIndex) insertIndex--; // поправка после удаления исходного
            if (insertIndex < 0) insertIndex = 0;
            if (insertIndex > frameControls.Count) insertIndex = frameControls.Count;
            frameControls.Insert(insertIndex, item);

            LayoutFrames();
            
            var newPosition = insertIndex + 1;
            var oldPosition = sourceIndex + 1;
            ShowStatusMessage(string.Format(FrameMoved, frameName, oldPosition, newPosition), MessageType.Success);
        }

        private int ComputeInsertIndexAndY(Point p, out int indicatorY)
        {
            indicatorY = -1;
            if (frameControls.Count == 0)
            {
                indicatorY = panelFrames.Padding.Top;
                return 0;
            }

            for (var i = 0; i < frameControls.Count; i++)
            {
                var pic = frameControls[i].Picture;
                var blockTop = pic.Top;
                var blockBottom = pic.Bottom + 2 + 15; // включая подпись
                var mid = (blockTop + blockBottom) / 2;

                if (p.Y < blockTop)
                {
                    indicatorY = Math.Max(panelFrames.Padding.Top, blockTop - frameSpacing / 2);
                    return i;
                }
                if (p.Y >= blockTop && p.Y < mid)
                {
                    indicatorY = Math.Max(panelFrames.Padding.Top, blockTop - frameSpacing / 2);
                    return i;
                }
                if (p.Y >= mid && p.Y <= blockBottom)
                {
                    indicatorY = blockBottom + frameSpacing / 2;
                    return i + 1;
                }
            }

            // Ниже последнего
            var last = frameControls[^1].Picture;
            indicatorY = last.Bottom + 2 + 15 + frameSpacing / 2;
            return frameControls.Count;
        }

        private void ShowDropIndicator(int y)
        {
            if (dropInsertIndicatorLine == null)
            {
                dropInsertIndicatorLine = new Panel
                {
                    Height = 3,
                    BackColor = Color.Red,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right
                };
                panelFrames.Controls.Add(dropInsertIndicatorLine);
            }
            dropInsertIndicatorLine.Location = new Point(panelFrames.Padding.Left, y);
            dropInsertIndicatorLine.Width = GetThumbnailWidth();
            dropInsertIndicatorLine.BringToFront();
        }

        private void HideDropIndicator()
        {
            if (dropInsertIndicatorLine != null)
            {
                panelFrames.Controls.Remove(dropInsertIndicatorLine);
                dropInsertIndicatorLine.Dispose();
                dropInsertIndicatorLine = null;
            }
        }
    }
}