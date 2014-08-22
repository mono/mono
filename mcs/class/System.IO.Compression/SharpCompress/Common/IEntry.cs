using System;

namespace SharpCompress.Common
{
    internal interface IEntry
    {
        CompressionType CompressionType { get; }
        DateTime? ArchivedTime { get; }
        long CompressedSize { get; }
        uint Crc { get; }
        DateTime? CreatedTime { get; }
        string Key { get; }
        bool IsDirectory { get; }
        bool IsEncrypted { get; }
        bool IsSplit { get; }
        DateTime? LastAccessedTime { get; }
        DateTime? LastModifiedTime { get; set; }
        long Size { get; }
    }
}