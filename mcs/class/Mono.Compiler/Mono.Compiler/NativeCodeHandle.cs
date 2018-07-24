using System.Runtime.InteropServices;

namespace Mono.Compiler
{
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct NativeCodeHandle
	{
		byte* blob;
		long length;
		MethodInfo mi;

		public byte* Blob {
			get { return blob; }
		}

		public MethodInfo MethodInfo {
			get { return mi; }
		}

		public NativeCodeHandle (byte *codeBlob, long codeLength, MethodInfo methodInfo) {
			blob = codeBlob;
			length = codeLength;
			mi = methodInfo;
		}
	}
}
