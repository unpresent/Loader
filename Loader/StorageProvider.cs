using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Loader
{
    interface IStorageProvider
    {
        string Root { get; }
        string[] GetAllDirectories();
        string[] GetAllFiles();
        void CreateDirectory(string aDirName);
        bool FileExists(string aFileName);
        void SaveFile(string aSourceName, string aFileName);
        FileInfo GetFileInfo(string aFileName);
        void Delete(string aFileName);
    }

    class StorageProvider : IStorageProvider
    {
        // -----------------------------------------------------------
        #region Инициализация
        // -----------------------------------------------------------
        public StorageProvider(string aDestFolder)
        {
            Root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + aDestFolder;
        }
        // -----------------------------------------------------------
        #endregion
        // -----------------------------------------------------------
        #region Свойства
        // -----------------------------------------------------------
        public string Root
        { get; }
        // -----------------------------------------------------------
        #endregion
        // -----------------------------------------------------------
        #region Реализация
        // -----------------------------------------------------------
        /// <summary>
        /// Список всех папок (пути относительные)
        /// </summary>
        /// <returns></returns>
        public string[] GetAllDirectories()
        {
            var vDirectories = new List<string>(Directory.GetDirectories(Root));
            var i = 0;
            while (i < vDirectories.Count)
            {
                // Добавляем субпапки
                vDirectories.AddRange( Directory.GetDirectories(vDirectories[i]).ToArray() );

                // Заменяем на субпуть
                vDirectories[i] = vDirectories[i].Replace(Root, string.Empty);

                i++;
            }

            vDirectories.Add(string.Empty);

            return vDirectories.ToArray();
        }

        /// <summary>
        /// Список всех файлов. Пути относительные
        /// </summary>
        /// <returns></returns>
        public string[] GetAllFiles()
        {
            var vFiles = new List<string>();
            foreach (var vDir in GetAllDirectories())
            {
                vFiles.AddRange(Directory.GetFiles(Root + vDir).ToArray());
            }

            for (var i = 0; i < vFiles.Count; i++)
            {
                vFiles[i] = vFiles[i].Replace(Root, string.Empty);
            }

            return vFiles.ToArray();
        }

        public void CreateDirectory(string aName)
        {
            Directory.CreateDirectory(Root + aName);
        }

        public bool FileExists(string aName)
        {
            return File.Exists(Root + aName);
        }

        public void SaveFile(string aSourceName, string aFileName)
        {
            File.Copy(aSourceName, Root + aFileName, true);
            var vDestination = new FileInfo(Root + aFileName);
            var vSource = new FileInfo(aSourceName);
            vDestination.CreationTime = vSource.CreationTime;
            vDestination.LastWriteTime = vSource.LastWriteTime;
        }

        public FileInfo GetFileInfo(string aFileName)
        {
            return new FileInfo(Root + aFileName);
        }

        public void Delete(string aFileName)
        {
            File.Delete(Root + aFileName);
        }
        // -----------------------------------------------------------
        #endregion
        // -----------------------------------------------------------
    }
}
