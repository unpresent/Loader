using System.Collections.Generic;
using System.IO;

namespace Loader
{
    interface IFileSystemProvider
    {
        void DeleteEmptyDirectories(string aDir, string aRoot = null);
        FileInfo[] ListFiles(string aPath);
    }

    class FileSystemProvider : IFileSystemProvider
    {
        /// <summary>
        /// рекурсивно удаляет пустые папки
        /// </summary>
        /// <param name="aDir"></param>
        /// <param name="aRoot">корень</param>
        public void DeleteEmptyDirectories(string aDir, string aRoot)
        {
            if (Directory.GetDirectories(aDir).Length + Directory.GetFiles(aDir).Length < 1)
            {
                Directory.Delete(aDir);
                // после удаления папки вернёмся в корень - проверить ещё раз не освободилась ли какая-нибудь папка
                DeleteEmptyDirectories(aRoot ?? aDir, aRoot ?? aDir);
            }
            else
            {
                foreach (string vDir in Directory.GetDirectories(aDir))
                {
                    DeleteEmptyDirectories(vDir, aRoot ?? aDir);
                }
            }
        }

        /// <summary>
        /// извлекает массив FileInfo из дирректории
        /// </summary>
        public FileInfo[] ListFiles(string aPath)
        {
            List<FileInfo> vFiles = new List<FileInfo>();
            aPath = Path.GetDirectoryName(aPath);
            return GetFiles(aPath, vFiles);
        }

        private FileInfo[] GetFiles(string aPath, List<FileInfo> aFiles)
        {
            foreach (string vFile in Directory.GetFiles(aPath))
            {
                aFiles.Add(new FileInfo(vFile));
            }
            GoDeeper(aPath, aFiles);
            return aFiles.ToArray();
        }

        private void GoDeeper(string aPath, List<FileInfo> aFiles)
        {
            foreach (var vDir in Directory.GetDirectories(aPath))
            {
                foreach (var vFile in Directory.GetFiles(vDir))
                {
                    aFiles.Add(new FileInfo(vFile));
                }
                GoDeeper(vDir, aFiles);
            }
        }
    }
}
