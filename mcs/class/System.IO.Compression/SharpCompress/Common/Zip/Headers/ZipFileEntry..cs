using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SharpCompress.Common.Zip.Headers
{
    internal abstract class ZipFileEntry : ZipHeader
    {
        protected ZipFileEntry(ZipHeaderType type)
            : base(type)
        {
            Extra = new List<ExtraData>();
        }


        internal bool IsDirectory
        {
            get { return Name.EndsWith("/"); }
        }

        protected string DecodeString(byte[] str)
        {
            if (FlagUtility.HasFlag(Flags, HeaderFlags.UTF8))
            {
                return Encoding.UTF8.GetString(str, 0, str.Length);
            }
            return ArchiveEncoding.Default.GetString(str, 0, str.Length);
        }

        protected byte[] EncodeString(string str)
        {
            if (FlagUtility.HasFlag(Flags, HeaderFlags.UTF8))
            {
                return Encoding.UTF8.GetBytes(str);
            }
            return ArchiveEncoding.Default.GetBytes(str);
        }

        internal Stream PackedStream { get; set; }

        internal string Name { get; set; }

        internal HeaderFlags Flags { get; set; }

        internal ZipCompressionMethod CompressionMethod { get; set; }

        internal uint CompressedSize { get; set; }

        internal long? DataStartPosition { get; set; }

        internal uint UncompressedSize { get; set; }

        internal List<ExtraData> Extra { get; set; }

        internal PkwareTraditionalEncryptionData PkwareTraditionalEncryptionData { get; set; }
#if !PORTABLE && !NETFX_CORE
        internal WinzipAesEncryptionData WinzipAesEncryptionData { get; set; }
#endif

        internal ushort LastModifiedDate { get; set; }

        internal ushort LastModifiedTime { get; set; }

        internal uint Crc { get; set; }

        protected void LoadExtra(byte[] extra)
        {
            if (extra.Length % 2 != 0)
                return;
			
            for (int i = 0; i < extra.Length;)
            {
                ExtraDataType type = (ExtraDataType) BitConverter.ToUInt16(extra, i);
                if (!Enum.IsDefined(typeof (ExtraDataType), type))
                {
                    return;
                }
                ushort length = BitConverter.ToUInt16(extra, i + 2);
                byte[] data = new byte[length];
                Buffer.BlockCopy(extra, i + 4, data, 0, length);
                Extra.Add(new ExtraData
                              {
                                  Type = type,
                                  Length = length,
                                  DataBytes = data
                              });
                i += length + 4;
            }
        }

        internal ZipFilePart Part { get; set; }
    }
}