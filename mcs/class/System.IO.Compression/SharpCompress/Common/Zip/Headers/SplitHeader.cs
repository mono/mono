using System;

namespace SharpCompress.Common.Zip.Headers
{
    internal class SplitHeader : ZipHeader
    {
        public SplitHeader()
            : base(ZipHeaderType.Split)
        {
        }

        internal override void Read(System.IO.BinaryReader reader)
        {
            throw new NotImplementedException();
        }

        internal override void Write(System.IO.BinaryWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}