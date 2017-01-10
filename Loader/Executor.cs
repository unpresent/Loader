using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Loader
{

    class Executor
    {
        // -----------------------------------------------------------
        #region Константы
        // -----------------------------------------------------------
        private const string CStorageProviderFilterAll = "*";
        // -----------------------------------------------------------
        #endregion
        // -----------------------------------------------------------
        #region Поля
        // -----------------------------------------------------------
        private bool mFormActivated;
        private readonly List<FileInfo> mSourceFiles = new List<FileInfo>();
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

        private string PathInStorage
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

        private List<FileInfo> SourceFiles
        { get { return mSourceFiles; } }
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
            ProgressForm.Activated -= this.FormActivated;
            ProgressForm.Activated += this.FormActivated;

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
            await Task.Run(() => this.InternalRun());
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
                        IOException e = (IOException)aPost;
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
                        Exception e = (Exception)aPost;
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
            mFilesForDelete.AddRange(mDestinationFiles);
        }

        /// <summary>
        /// Формируем список файлов. Пока это все файлы папки
        /// </summary>
        private void InternalPrepareSourceFilesList()
        {
            FileInfo[] vFiles = FileSystemProvider.ListFiles(PathToSource);
            mSourceFiles.Clear();

            // TODO: Добавить сравнение с получателем
            foreach (var vItem in vFiles)
            {
                mSourceFiles.Add(vItem);
            }
        }

        private void InternalFilesCopy()
        {
            string vDirectory = Path.GetDirectoryName(PathToSource);
            int vFilesCount = SourceFiles.Count;
            int i = -1;

            foreach (FileInfo vFile in SourceFiles)
            {
                i++;

                // Получаем относительный (внутри папки PathToSource) путь к файлу
                string vLocalPathToFile = vFile.DirectoryName.Replace(vDirectory, string.Empty);

                Notify(vLocalPathToFile + vFile.Name, i * 100 / vFilesCount);

                // Формируем путь в получателе
                string vDestPathToFile = (!string.IsNullOrEmpty(vLocalPathToFile)) ? (vLocalPathToFile.Substring(1) + "/") : string.Empty;

                // При необходимости создаем новую папку в папке получателя
                if (!string.IsNullOrEmpty(vDestPathToFile) && (!mDestinationFolders.Contains(vDestPathToFile)))
                {
                    StorageProvider.CreateDirectory(vDestPathToFile);
                }

                string vFileDest = vDestPathToFile + vFile.Name;

                // Удаляем файл из списка кандидатов на удаление
                mFilesForDelete.Remove(vFileDest);

                if (StorageProvider.FileExists(vFileDest))
                {
                    FileInfo vFI = StorageProvider.GetFileInfo(vFileDest);
                    if (Comparer.Equals(vFI, vFile))
                    {
                        continue;
                    }
                }

                // Копирование файла
                StorageProvider.SaveFile(vFile.FullName, vFileDest);
            }
        }

        /// <summary>
        /// Запуск приложения из папки получателя
        /// </summary>
        private void InternalLaunchApplication()
        {
            string vAppFile = StorageProvider.Root + AppName;
            if (vAppFile == null)
            {
                throw new Exception($"Не удалось найти файл приложения {AppName}");
            }

            ProcessStartInfo vProccess = new ProcessStartInfo()
            {
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
            //*
            SynchronizationContext.Post
            (
                aPost =>
                {
                    ProgressForm.ProgressCaption = (string)aPost;
                    ProgressForm.ProgressDescription = string.Empty;
                    ProgressForm.ProgressValue = -1;
                },
                (object)aCaption
            );
            //*/
        }
        private void Notify(string aDescription, int aPercent)
        {
            //*
            SynchronizationContext.Post
            (
                aPost =>
                {
                    ExecutorShortState vState = (ExecutorShortState)aPost;
                    ProgressForm.ProgressDescription = vState.Description;
                    ProgressForm.ProgressValue = vState.Percent;
                },
                new ExecutorShortState(aDescription, aPercent)
            );
            //*/
        }
        // -----------------------------------------------------------
        #endregion
        // -----------------------------------------------------------

        class ExecutorShortState
        {
            public ExecutorShortState(string aDescription, int aPercent)
            {
                this.Percent = aPercent;
                this.Description = aDescription;
            }
            public string Description { get; }
            public int Percent { get; }
        }
    }
    
}


/*
        public FileInfo GetStorageFileInfo(string file)
        {
            using (IsolatedStorageFileStream stream = isoStore.OpenFile(file, FileMode.Open))
            {
                return GetFileInfoFromStream(stream);
            }   
        }
  
  private FileInfo GetFileInfoFromStream(FileStream stream)
        {
            return new FileInfo(
                stream.GetType()
                    .GetField("m_FullPath", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(stream)
                    .ToString());
        }
*/
