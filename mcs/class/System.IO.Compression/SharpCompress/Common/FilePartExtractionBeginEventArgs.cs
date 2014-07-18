using System;

namespace SharpCompress.Common
{
    internal class FilePartExtractionBeginEventArgs : EventArgs
    {
        /// <summary>
        /// File name for the part for the current entry
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Uncompressed size of the current entry in the part
        /// </summary>
        public long Size { get; internal set; }

        /// <summary>
        /// Compressed size of the current entry in the part
        /// </summary>
        public long CompressedSize { get; internal set; }
    }
}