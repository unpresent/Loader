using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Loader
{
    /// <summary>
    /// компаратор для сравднения двух FileInfo
    /// </summary>
    interface IComparer
    {
        bool Equals(string oldFile, string newFile);
        bool Equals(FileInfo oldFile, FileInfo newFile);
    }

    class Comparer : IComparer
    {
        public bool Equals(string oldFile, string newFile)
        {
            return Equals(
                new FileInfo(oldFile), 
                new FileInfo(newFile));
        }

        public bool Equals(FileInfo oldFile, FileInfo newFile)
        {
            return oldFile.CreationTime == newFile.CreationTime
                && oldFile.LastWriteTime == newFile.LastWriteTime
                && oldFile.Length == newFile.Length;
        }
    }
}
