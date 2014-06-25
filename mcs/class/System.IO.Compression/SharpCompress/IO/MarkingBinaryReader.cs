using System;
using System.IO;
using System.Linq;

namespace SharpCompress.IO
{
    internal class MarkingBinaryReader : BinaryReader
    {

        public MarkingBinaryReader(Stream stream)
            : base(stream)
        {
        }

        public long CurrentReadByteCount { get; private set; }

        public void Mark()
        {
            CurrentReadByteCount = 0;
        }

        public override int Read()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int index, int count)
        {
            throw new NotImplementedException();
        }

        public override int Read(char[] buffer, int index, int count)
        {
            throw new NotImplementedException();
        }

        public override bool ReadBoolean()
        {
            return BitConverter.ToBoolean(ReadBytes(1), 0);
        }

        public override byte ReadByte()
        {
            return ReadBytes(1).Single();
        }

        public override byte[] ReadBytes(int count)
        {
            CurrentReadByteCount += count;
            var bytes = base.ReadBytes(count);
            if (bytes.Length != count)
            {
                throw new EndOfStreamException(string.Format("Could not read the requested amount of bytes.  End of stream reached. Requested: {0} Read: {1}", count, bytes.Length));
            }
            return bytes;
        }

        public override char ReadChar()
        {
            throw new NotImplementedException();
        }

        public override char[] ReadChars(int count)
        {
            throw new NotImplementedException();
        }

#if !PORTABLE
        public override decimal ReadDecimal()
        {
            return ByteArrayToDecimal(ReadBytes(16), 0);
        }

        private decimal ByteArrayToDecimal(byte[] src, int offset)
        {
            //http://stackoverflow.com/a/16984356/385387
            var i1 = BitConverter.ToInt32(src, offset);
            var i2 = BitConverter.ToInt32(src, offset + 4);
            var i3 = BitConverter.ToInt32(src, offset + 8);
            var i4 = BitConverter.ToInt32(src, offset + 12);

            return new decimal(new[] { i1, i2, i3, i4 });
        }
#endif

        public override double ReadDouble()
        {
            return BitConverter.ToDouble(ReadBytes(8), 0);
        }

        public override short ReadInt16()
        {
            return BitConverter.ToInt16(ReadBytes(2), 0);
        }

        public override int ReadInt32()
        {
            return BitConverter.ToInt32(ReadBytes(4), 0);
        }

        public override long ReadInt64()
        {
            return BitConverter.ToInt64(ReadBytes(8), 0);
        }

        public override sbyte ReadSByte()
        {
            return (sbyte)ReadByte();
        }

        public override float ReadSingle()
        {
            return BitConverter.ToSingle(ReadBytes(4), 0);
        }

        public override string ReadString()
        {
            throw new NotImplementedException();
        }

        public override ushort ReadUInt16()
        {
            return BitConverter.ToUInt16(ReadBytes(2), 0);
        }

        public override uint ReadUInt32()
        {
            return BitConverter.ToUInt32(ReadBytes(4), 0);
        }

        public override ulong ReadUInt64()
        {
            return BitConverter.ToUInt64(ReadBytes(8), 0);
        }
    }
}