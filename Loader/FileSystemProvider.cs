using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loader
{
    interface IFileSystemProvider
    {
        void DeleteEmptyDirectories(string dir, string root = null);
        FileInfo[] ListFiles(string v);
    }

    class FileSystemProvider : IFileSystemProvider
    {
        /// <summary>
        /// рекурсивно удаляет пустые папки
        /// </summary>
        /// <param name="root">корень</param>
        public void DeleteEmptyDirectories(string dir, string root)
        {
            if (Directory.GetDirectories(dir).Length + Directory.GetFiles(dir).Length < 1)
            {
                Directory.Delete(dir);
                // после удаления папки вернёмся в корень - проверить ещё раз не освободилась ли какая-нибудь папка
                DeleteEmptyDirectories(root ?? dir, root ?? dir);
            }
            else
            {
                foreach (string d in Directory.GetDirectories(dir))
                {
                    DeleteEmptyDirectories(d, root ?? dir);
                }
            }
        }

        /// <summary>
        /// извлекает массив FileInfo из дирректории
        /// </summary>
        public FileInfo[] ListFiles(string path)
        {
            List<FileInfo> files = new List<FileInfo>();
            path = Path.GetDirectoryName(path);
            return GetFiles(path, files);
        }

        private FileInfo[] GetFiles(string path, List<FileInfo> files)
        {
            foreach (string f in Directory.GetFiles(path))
            {
                files.Add(new FileInfo(f));
            }
            GoDeeper(path, files);
            return files.ToArray();
        }

        private void GoDeeper(string s, List<FileInfo> files)
        {
            foreach (string d in Directory.GetDirectories(s))
            {
                foreach (string f in Directory.GetFiles(d))
                {
                    files.Add(new FileInfo(f));
                }
                GoDeeper(d, files);
            }
        }
    }
}
