// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>
using System.IO;

namespace HtmlAgilityPack
{
    internal struct IOLibrary
    {
        #region Internal Methods

        internal static void CopyAlways(string source, string target)
        {
            if (!File.Exists(source))
                return;
            Directory.CreateDirectory(Path.GetDirectoryName(target));
            MakeWritable(target);
            File.Copy(source, target, true);
        }

        internal static void MakeWritable(string path)
        {
            if (!File.Exists(path))
                return;
            File.SetAttributes(path, File.GetAttributes(path) & ~FileAttributes.ReadOnly);
        }

        #endregion
    }
}