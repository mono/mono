
using System.Runtime.InteropServices;

namespace Mono.Compiler
{
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct NativeCodeHandle
	{
		byte* blob;
		long length;

		public byte* Blob {
			get { return blob; }
		}

		internal NativeCodeHandle (byte *codeBlob, long codeLength) {
			blob = codeBlob;
			length = codeLength;
		}
	}
}
