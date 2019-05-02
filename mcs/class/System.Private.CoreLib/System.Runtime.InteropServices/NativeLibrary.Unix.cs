using System.IO;
using System.Reflection;

namespace System.Runtime.InteropServices
{
	partial class NativeLibrary
	{
		[DllImport ("libdl")]
		static extern IntPtr dlopen (string libName, int flags);
				
		static IntPtr LoadLibraryByName (string libraryName, Assembly assembly, DllImportSearchPath? searchPath, bool throwOnError) => throw new NotImplementedException ();

		static IntPtr LoadFromPath (string libraryName, bool throwOnError)
		{
			const int RTLD_LAZY = 0x001;
			
			IntPtr ptr = dlopen (libraryName, RTLD_LAZY);
			if (ptr == IntPtr.Zero && throwOnError) {
				throw new DllNotFoundException();
			}
			return ptr;
		}

		static IntPtr LoadByName (string libraryName, RuntimeAssembly callingAssembly, bool hasDllImportSearchPathFlag, uint dllImportSearchPathFlag, bool throwOnError) => throw new NotImplementedException ();

		static void FreeLib (IntPtr handle) => throw new NotImplementedException ();

		static IntPtr GetSymbol (IntPtr handle, string symbolName, bool throwOnError) => throw new NotImplementedException ();
	}
}
