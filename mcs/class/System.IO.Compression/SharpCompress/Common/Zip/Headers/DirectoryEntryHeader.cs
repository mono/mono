using System.IO;

namespace SharpCompress.Common.Zip.Headers
{
    internal class DirectoryEntryHeader : ZipFileEntry
    {
        public DirectoryEntryHeader()
            : base(ZipHeaderType.DirectoryEntry)
        {
        }

        internal override void Read(BinaryReader reader)
        {
            Version = reader.ReadUInt16();
            VersionNeededToExtract = reader.ReadUInt16();
            Flags = (HeaderFlags) reader.ReadUInt16();
            CompressionMethod = (ZipCompressionMethod) reader.ReadUInt16();
            LastModifiedTime = reader.ReadUInt16();
            LastModifiedDate = reader.ReadUInt16();
            Crc = reader.ReadUInt32();
            CompressedSize = reader.ReadUInt32();
            UncompressedSize = reader.ReadUInt32();
            ushort nameLength = reader.ReadUInt16();
            ushort extraLength = reader.ReadUInt16();
            ushort commentLength = reader.ReadUInt16();
            DiskNumberStart = reader.ReadUInt16();
            InternalFileAttributes = reader.ReadUInt16();
            ExternalFileAttributes = reader.ReadUInt32();
            RelativeOffsetOfEntryHeader = reader.ReadUInt32();

            byte[] name = reader.ReadBytes(nameLength);
            Name = DecodeString(name);
            byte[] extra = reader.ReadBytes(extraLength);
            byte[] comment = reader.ReadBytes(commentLength);
            Comment = DecodeString(comment);
            LoadExtra(extra);
        }

        internal override void Write(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.Write(VersionNeededToExtract);
            writer.Write((ushort) Flags);
            writer.Write((ushort) CompressionMethod);
            writer.Write(LastModifiedTime);
            writer.Write(LastModifiedDate);
            writer.Write(Crc);
            writer.Write(CompressedSize);
            writer.Write(UncompressedSize);

            byte[] nameBytes = EncodeString(Name);
            writer.Write((ushort) nameBytes.Length);
            //writer.Write((ushort)Extra.Length);
            writer.Write((ushort) 0);
            writer.Write((ushort) Comment.Length);

            writer.Write(DiskNumberStart);
            writer.Write(InternalFileAttributes);
            writer.Write(ExternalFileAttributes);
            writer.Write(RelativeOffsetOfEntryHeader);

            writer.Write(nameBytes);
            // writer.Write(Extra);
            writer.Write(Comment);
        }

        internal ushort Version { get; private set; }

        public ushort VersionNeededToExtract { get; set; }

        public uint RelativeOffsetOfEntryHeader { get; set; }

        public uint ExternalFileAttributes { get; set; }

        public ushort InternalFileAttributes { get; set; }

        public ushort DiskNumberStart { get; set; }

        public string Comment { get; private set; }
    }
}