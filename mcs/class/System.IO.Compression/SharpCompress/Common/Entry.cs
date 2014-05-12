using System;
using System.Collections.Generic;

namespace SharpCompress.Common
{
    internal abstract class Entry : IEntry
    {
        /// <summary>
        /// The File's 32 bit CRC Hash
        /// </summary>
        public abstract uint Crc { get; }

        /// <summary>
        /// The string key of the file internal to the Archive.
        /// </summary>
        public abstract string Key { get; }

        /// <summary>
        /// The compressed file size
        /// </summary>
        public abstract long CompressedSize { get; }

        /// <summary>
        /// The compression type
        /// </summary>
        public abstract CompressionType CompressionType { get; }

        /// <summary>
        /// The uncompressed file size
        /// </summary>
        public abstract long Size { get; }

        /// <summary>
        /// The entry last modified time in the archive, if recorded
        /// </summary>
        public abstract DateTime? LastModifiedTime { get; set; }

        /// <summary>
        /// The entry create time in the archive, if recorded
        /// </summary>
        public abstract DateTime? CreatedTime { get; }

        /// <summary>
        /// The entry last accessed time in the archive, if recorded
        /// </summary>
        public abstract DateTime? LastAccessedTime { get; }

        /// <summary>
        /// The entry time whend archived, if recorded
        /// </summary>
        public abstract DateTime? ArchivedTime { get; }

        /// <summary>
        /// Entry is password protected and encrypted and cannot be extracted.
        /// </summary>
        public abstract bool IsEncrypted { get; }

        /// <summary>
        /// Entry is password protected and encrypted and cannot be extracted.
        /// </summary>
        public abstract bool IsDirectory { get; }

        /// <summary>
        /// Entry is split among multiple volumes
        /// </summary>
        public abstract bool IsSplit { get; }

        internal abstract IEnumerable<FilePart> Parts { get; }
        internal bool IsSolid { get; set; }

        internal virtual void Close()
        {
            
        }
    }
}