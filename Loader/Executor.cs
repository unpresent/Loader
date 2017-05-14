using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Loader
{

    [SuppressMessage("ReSharper", "LocalizableElement")]
    [SuppressMessage("ReSharper", "ConvertPropertyToExpressionBody")]
    [SuppressMessage("ReSharper", "ConvertToAutoProperty")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    class Executor
    {
        // -----------------------------------------------------------
        #region Константы
        // -----------------------------------------------------------
        // private const string CStorageProviderFilterAll = "*";
        // -----------------------------------------------------------
        #endregion
        // -----------------------------------------------------------
        #region Поля
        // -----------------------------------------------------------
        private bool mFormActivated;
        private readonly List<string> mCopyFrom = new List<string>();
        private readonly List<string> mCopyTo = new List<string>();
        private readonly List<string> mFilesForDelete = new List<string>();
        private string[] mDestinationFolders;
        private string[] mDestinationFiles;
        // -----------------------------------------------------------
        #endregion
        // -----------------------------------------------------------
        #region Инициализация
        // -----------------------------------------------------------
        public Executor(string aPathToSource, string aPathToDestination, string aAppName, string aOtherParams, ProgressForm aForm)
        {
            ProgressForm = aForm;
            SynchronizationContext = SynchronizationContext.Current;

            PathToSource = aPathToSource;
            PathToDestination = aPathToDestination;
            AppName = aAppName;
            OtherParams = aOtherParams;

            StorageProvider = new StorageProvider(PathToDestination);

            FileSystemProvider = new FileSystemProvider();
            Comparer = new Comparer();
        }
        // -----------------------------------------------------------
        #endregion
        // -----------------------------------------------------------
        #region Свойства
        // -----------------------------------------------------------
        private ProgressForm ProgressForm
        { get; }

        private SynchronizationContext SynchronizationContext
        { get; }

        /// <summary>
        /// Путь к папке, из которой копируем компоненты приложения
        /// </summary>
        private string PathToSource
        { get; }

        /// <summary>
        /// Путь к папке, в которую копируем компоненты приложения
        /// </summary>
        private string PathToDestination
        { get; }

        /// <summary>
        /// Приложение, которое надо запустить (относительно пути Path)
        /// </summary>
        private string AppName
        { get; }

        /// <summary>
        /// Параметры, которые надо передать приложению
        /// </summary>
        private string OtherParams
        { get; }

        private IStorageProvider StorageProvider
        { get; }

        private IFileSystemProvider FileSystemProvider
        { get; }

        private Comparer Comparer
        { get; }

        private List<string> CopyFrom
        { get { return mCopyFrom; } }

        private List<string> CopyTo
        { get { return mCopyTo; } }

        private List<string> FilesForDelete
        { get { return mFilesForDelete; } }
        // -----------------------------------------------------------
        #endregion
        // -----------------------------------------------------------
        #region Реализация
        // -----------------------------------------------------------
        /// <summary>
        /// Основаная работа:
        /// Копировение файлов и запуск приложения
        /// </summary>
        public void Run()
        {
            // Вешаем событие на то, что форма отобразилась
            ProgressForm.Activated -= FormActivated;
            ProgressForm.Activated += FormActivated;

            // Запуск формы для отображения прогресса
            Application.Run(ProgressForm);
        }

        private async void FormActivated(object aSender, EventArgs aEventArgs)
        {
            if (mFormActivated)
            {
                return;
            }
            mFormActivated = true;

            // Запуск копировальщика
            await Task.Run(() => InternalRun());
        }

        private void InternalRun()
        {
            try
            {
                Notify(string.Empty, -1);
                NotifyCaption("Подготовка списка файлов");

                // Анализ файлов в получателе
                NotifyCaption("Анализ получателя...");
                InternalPrepareDestinationFilesList();

                // Подготовка списка файлов к копированию
                NotifyCaption("Сканирование источника...");
                InternalPrepareSourceFilesList();

                // Копирование файлов
                NotifyCaption("Копирование файлов:");
                InternalFilesCopy();

                // Удаление лишних
                NotifyCaption("Удаление лишних:");
                InternalCleanup();

                // Запуск приложения
                NotifyCaption("Запуск приложения.");
                InternalLaunchApplication();
            }
            catch (IOException ex)
            {
                SynchronizationContext.Post
                (
                    aPost =>
                    {
                        var e = (IOException)aPost;
                        MessageBox.Show("Ошибка", e.Message + "\n" + e.StackTrace, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    },
                    ex
                );
            }
            catch (Exception ex)
            {
                SynchronizationContext.Post
                (
                    aPost =>
                    {
                        var e = (Exception)aPost;
                        MessageBox.Show("Ошибка", e.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    },
                    ex
                );
            }

            Application.Exit();
        }

        /// <summary>
        /// Формируем список файлов. Пока это все файлы папки
        /// </summary>
        private void InternalPrepareDestinationFilesList()
        {
            if (!Directory.Exists(StorageProvider.Root))
            {
                Directory.CreateDirectory(StorageProvider.Root);
            }

            // Получаем список всех папок в получателе
            mDestinationFolders = StorageProvider.GetAllDirectories();

            // Получаем список всех файлов в получателе
            mDestinationFiles = StorageProvider.GetAllFiles();

            // Пока кандидатами на удаление являются все файлы в получателе
            FilesForDelete.AddRange(mDestinationFiles);
        }

        /// <summary>
        /// Формируем список файлов. Пока это все файлы папки
        /// </summary>
        private void InternalPrepareSourceFilesList()
        {
            var vFiles = FileSystemProvider.ListFiles(PathToSource);
            CopyFrom.Clear();
            CopyTo.Clear();

            var vDirectory = Path.GetDirectoryName(PathToSource);
            if (vDirectory == null)
            {
                return;
            }

            var vFilesCount = vFiles.Length;
            var i = -1;

            // TODO: Добавить сравнение с получателем
            foreach (var vFile in vFiles)
            {
                i++;
                if (vFile.DirectoryName == null)
                {
                    continue;
                }

                // Получаем относительный (внутри папки PathToSource) путь к файлу
                var vLocalPathToFile = vFile.DirectoryName.Replace(vDirectory, string.Empty);

                Notify(vLocalPathToFile + "\\" + vFile.Name, i * 100 / vFilesCount);

                // Формируем путь в получателе
                var vDestPathToFile = (!string.IsNullOrEmpty(vLocalPathToFile)) ? (vLocalPathToFile.Substring(1) + "\\") : string.Empty;

                // При необходимости создаем новую папку в папке получателя
                if (!string.IsNullOrEmpty(vDestPathToFile) && (!mDestinationFolders.Contains(vDestPathToFile)))
                {
                    StorageProvider.CreateDirectory(vDestPathToFile);
                }

                var vFileDest = vDestPathToFile + vFile.Name;

                // Удаляем файл из списка кандидатов на удаление
                FilesForDelete.Remove(vFileDest);

                if (StorageProvider.FileExists(vFileDest))
                {
                    var vFI = StorageProvider.GetFileInfo(vFileDest);
                    if
                    (
                           (vFI.Name == vFile.Name)
                        && (vFI.LastWriteTime == vFile.LastWriteTime)
                        && (vFI.CreationTime == vFile.CreationTime)
                    )
                    {
                        continue;
                    }
                }

                CopyFrom.Add(vFile.FullName);
                CopyTo.Add(vFileDest);
            }
        }

        private void InternalFilesCopy()
        {
            var vDirectory = Path.GetDirectoryName(PathToSource);
            if (vDirectory == null)
            {
                return;
            }

            var vFilesCount = CopyFrom.Count;

            for (var i = 0; i < vFilesCount; i++)
            {
                var vFileFrom = CopyFrom[i];
                var vFileTo = CopyTo[i];

                Notify(vFileFrom.Replace(PathToSource, string.Empty), i * 100 / vFilesCount);

                // Копирование файла
                StorageProvider.SaveFile(vFileFrom, vFileTo);
            }
        }

        /// <summary>
        /// Запуск приложения из папки получателя
        /// </summary>
        private void InternalLaunchApplication()
        {
            var vAppFile = StorageProvider.Root + AppName;
            if (string.IsNullOrEmpty(vAppFile))
            {
                throw new Exception($"Не удалось найти файл приложения {AppName}");
            }

            var vProccess = new ProcessStartInfo()
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                WorkingDirectory = Path.GetDirectoryName(vAppFile),
                FileName = Path.GetFileName(vAppFile)
            };

            // Если у нас есть выходящие параметры - добавим их в инфо
            if (!string.IsNullOrEmpty(OtherParams))
            {
                vProccess.Arguments = string.Join(" ", OtherParams);
            }

            // запустим приложение из хранилища
            Process.Start(vProccess);
        }

        /// <summary>
        /// Удаление лишних файлов в получателе
        /// </summary>
        private void InternalCleanup()
        {
            foreach (var vItem in mFilesForDelete)
            {
                StorageProvider.Delete(vItem);
            }

            FileSystemProvider.DeleteEmptyDirectories(StorageProvider.Root);
        }

        private void NotifyCaption(string aCaption)
        {
            SynchronizationContext.Post
            (
                aPost =>
                {
                    ProgressForm.ProgressCaption = (string)aPost;
                    ProgressForm.ProgressDescription = string.Empty;
                    ProgressForm.ProgressValue = -1;
                },
                aCaption
            );
        }
        private void Notify(string aDescription, int aPercent)
        {
            SynchronizationContext.Post
            (
                aPost =>
                {
                    var vState = (ExecutorShortState)aPost;
                    ProgressForm.ProgressDescription = vState.Description;
                    ProgressForm.ProgressValue = vState.Percent;
                },
                new ExecutorShortState(aDescription, aPercent)
            );
        }
        // -----------------------------------------------------------
        #endregion
        // -----------------------------------------------------------

        private class ExecutorShortState
        {
            public ExecutorShortState(string aDescription, int aPercent)
            {
                Percent = aPercent;
                Description = aDescription;
            }
            public string Description { get; }
            public int Percent { get; }
        }
    }
    
}
