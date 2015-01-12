using System.Text;

namespace System.Data.Common {

	internal static class SafeNativeMethods {

		static internal int GetComputerNameEx (int nameType, StringBuilder nameBuffer, ref int bufferSize)
		{
			throw new NotImplementedException ();
		}
	}
}