
namespace System.Net.Mime
{
    internal class Base64WriteStateInfo : WriteStateInfoBase
    {
        internal Base64WriteStateInfo() : base(){
        }

        internal Base64WriteStateInfo(int bufferSize, byte[] header, byte[] footer, int maxLineLength, int mimeHeaderLength)
            : base(bufferSize, header, footer, maxLineLength, mimeHeaderLength) {
        }

        internal int Padding { get; set; }

        internal byte LastBits { get; set; }
    }
}
