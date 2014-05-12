using System.IO;

namespace SharpCompress.Common.Zip.Headers
{
    internal enum ExtraDataType : ushort
    {
        WinZipAes = 0x9901,
    }

    internal class ExtraData
    {
        internal ExtraDataType Type { get; set; }
        internal ushort Length { get; set; }
        internal byte[] DataBytes { get; set; }
    }

    internal class LocalEntryHeader : ZipFileEntry
    {
        public LocalEntryHeader()
            : base(ZipHeaderType.LocalEntry)
        {
        }

        internal override void Read(BinaryReader reader)
        {
            Version = reader.ReadUInt16();
            Flags = (HeaderFlags) reader.ReadUInt16();
            CompressionMethod = (ZipCompressionMethod) reader.ReadUInt16();
            LastModifiedTime = reader.ReadUInt16();
            LastModifiedDate = reader.ReadUInt16();
            Crc = reader.ReadUInt32();
            CompressedSize = reader.ReadUInt32();
            UncompressedSize = reader.ReadUInt32();
            ushort nameLength = reader.ReadUInt16();
            ushort extraLength = reader.ReadUInt16();
            byte[] name = reader.ReadBytes(nameLength);
            byte[] extra = reader.ReadBytes(extraLength);
            Name = DecodeString(name);
            LoadExtra(extra);
        }

        internal override void Write(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.Write((ushort) Flags);
            writer.Write((ushort) CompressionMethod);
            writer.Write(LastModifiedTime);
            writer.Write(LastModifiedDate);
            writer.Write(Crc);
            writer.Write(CompressedSize);
            writer.Write(UncompressedSize);

            byte[] nameBytes = EncodeString(Name);

            writer.Write((ushort) nameBytes.Length);
            writer.Write((ushort) 0);
            //if (Extra != null)
            //{
            //    writer.Write(Extra);
            //}
            writer.Write(nameBytes);
        }

        internal ushort Version { get; private set; }
    }
}