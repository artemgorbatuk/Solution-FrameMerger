namespace Client.WinForms.Services
{
    public static class StatusMessages
    {
        // Создание кадра
        public const string FrameCreationStart = "Начало создания кадра...";
        public const string FrameCreatedSuccessfully = "Кадр успешно создан";
        public const string FrameCreationCancelled = "Создание кадра отменено";
        public const string FrameSelected = "Выбран кадр: {0}";

        // Удаление кадра
        public const string FrameDeletionStart = "Удаление кадра '{0}'...";
        public const string FrameDeletedSuccessfully = "Кадр '{0}' успешно удален";

        // Ошибки
        public const string ScreenCaptureError = "Ошибка захвата экрана: {0}";
        public const string OcrError = "Ошибка OCR: {0}";
        public const string ImageGenerationError = "Ошибка формирования картинки: {0}";
        public const string TextSaveError = "Ошибка сохранения TXT: {0}";
        public const string ImageSaveError = "Ошибка сохранения PNG: {0}";
        public const string FolderOpenError = "Ошибка открытия папки: {0}";
        public const string ImageLoadError = "Не удалось загрузить {0}: {1}";
        public const string TextReadError = "Не удалось прочитать {0}: {1}";

        // Загрузка файлов
        public const string ImageLoadStart = "Начало добавления изображений...";
        public const string ImagesLoadedSuccessfully = "Успешно добавлено изображений: {0}";
        public const string ImageLoadCancelled = "Добавление изображений отменено";

        // Чтение текста
        public const string TextLoadStart = "Начало добавления текста из файлов...";
        public const string TextFilesLoadedSuccessfully = "Успешно добавлено файлов: {0}";
        public const string TextLoadCancelled = "Добавление текста отменено";

        // OCR
        public const string TextFormationStart = "Начало формирования текста...";
        public const string TesseractNotFound = "Не найдена папка tessdata. Разместите файлы языковых моделей (rus.traineddata, eng.traineddata) в папке: {0}";
        public const string TextNotFoundOnImages = "Текст не найден на изображениях.";
        public const string TextFormedSuccessfully = "Текст успешно сформирован";

        // Генерация изображения
        public const string ImageFormationStart = "Начало формирования картинки...";
        public const string NoTextForImageGeneration = "Нет текста для формирования картинки.";
        public const string ImageFormedSuccessfully = "Картинка успешно сформирована";

        // Сохранение
        public const string TextSaveStart = "Начало сохранения текста...";
        public const string ImageSaveStart = "Начало сохранения картинки...";
        public const string NoImageToSave = "Нет картинки для сохранения. Сначала сформируйте картинку.";
        public const string FileSaved = "Сохранено: {0}";

        // Папка
        public const string FolderOpenStart = "Открытие папки...";
        public const string FolderOpenedSuccessfully = "Папка успешно открыта";

        // Редактирование
        public const string EditModeToggle = "Переключение режима редактирования...";
        public const string EditEnabled = "Редактирование разрешено";
        public const string EditDisabled = "Редактирование запрещено";

        // Drag & Drop
        public const string MoveCancelled = "Перемещение отменено";
        public const string FrameMoved = "Кадр '{0}' перемещен с позиции {1} на позицию {2}";
    }
}

