using System.IO;

namespace Loader
{
    /// <summary>
    /// компаратор для сравднения двух FileInfo
    /// </summary>
    interface IComparer
    {
        bool Equals(string aOldFile, string aNewFile);
        bool Equals(FileInfo aOldFile, FileInfo aNewFile);
    }

    class Comparer : IComparer
    {
        public bool Equals(string aOldFile, string aNewFile)
        {
            return Equals(
                new FileInfo(aOldFile), 
                new FileInfo(aNewFile));
        }

        public bool Equals(FileInfo aOldFile, FileInfo aNewFile)
        {
            return aOldFile.CreationTime == aNewFile.CreationTime
                && aOldFile.LastWriteTime == aNewFile.LastWriteTime
                && aOldFile.Length == aNewFile.Length;
        }
    }
}
