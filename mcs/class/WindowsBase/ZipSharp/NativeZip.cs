// Native.cs created with MonoDevelop
// User: alan at 12:18 13/10/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//


using System;
using System.Runtime.InteropServices;

namespace zipsharp
{
	static class NativeZip
	{
		const int DEFAULT_COMPRESSION = 0;
		const int Z_DEFLATED = 8;

		public static void CloseArchive (ZipHandle handle)
		{
			CloseArchive (handle, null);
		}

		public static void CloseArchive (ZipHandle handle, string comment)
		{
			zipClose (handle, comment);
			handle.SetHandleAsInvalid ();
		}

		public static void CloseFile (ZipHandle handle)
		{
			zipCloseFileInZip (handle);
		}

		public static ZipHandle OpenArchive (ZlibFileFuncDef funcDef, Append append)
		{
			ZipHandle h = zipOpen2 ("", (int) append, IntPtr.Zero, ref funcDef);
			if (h.IsInvalid)
				throw new Exception ("Could not open the zip archive");
			return h;
		}
		
		public static int OpenFile (ZipHandle handle, string filename)
		{
			return OpenFile (handle, filename, DEFAULT_COMPRESSION);
		}

		public static int OpenFile (ZipHandle handle, string filename, int compressionLevel)
		{
			ZipFileInfo fileInfo = new ZipFileInfo (DateTime.Now);
			int method = compressionLevel == 0 ? 0 : Z_DEFLATED;
			return zipOpenNewFileInZip (handle, filename, ref fileInfo, IntPtr.Zero, 0, IntPtr.Zero, 0, "", method, compressionLevel);
		}

		public static unsafe void Write (ZipHandle handle, byte[] buffer, int offset, uint count)
		{
			fixed (byte* b = &buffer[offset])
				zipWriteInFileInZip (handle, b, count);
		}

		[DllImport ("MonoPosixHelper")]
		static extern unsafe int zipWriteInFileInZip (ZipHandle handle,
		                                               byte* buffer,
		                                               uint len);

		[DllImport ("MonoPosixHelper")]
		static extern int zipCloseFileInZip (ZipHandle handle);

		[DllImport ("MonoPosixHelper")]
		static extern ZipHandle zipOpen2 (string pathname,
		                                  int append,
		                                  IntPtr globalcomment, // zipcharpc*
		                                  ref ZlibFileFuncDef pzlib_filefunc_def); // zlib_filefunc_def*

		
		[DllImport ("MonoPosixHelper")]
		static extern int zipClose (ZipHandle handle, string globalComment);

		[DllImport ("MonoPosixHelper")]
		static extern int zipOpenNewFileInZip (ZipHandle handle,
		                                       string filename,
		                                       ref ZipFileInfo zipfi,
		                                       IntPtr extrafield_local,
		                                       uint size_extrafield_local,
		                                       IntPtr extrafield_global,
		                                       uint size_extrafield_global,
		                                       string comment,
		                                       int method,
		                                       int level);
	}
}
