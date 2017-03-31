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

		public static ZipHandle OpenArchive32 (ZlibFileFuncDef32 funcDef, Append append)
		{
			ZipHandle h = zipOpen2_32 ("", (int) append, IntPtr.Zero, ref funcDef);
			if (h.IsInvalid)
				throw new Exception ("Could not open the zip archive");
			return h;
		}

		public static ZipHandle OpenArchive64 (ZlibFileFuncDef64 funcDef, Append append)
		{
			ZipHandle h = zipOpen2_64 ("", (int) append, IntPtr.Zero, ref funcDef);
			if (h.IsInvalid)
				throw new Exception ("Could not open the zip archive");
			return h;
		}

		public static int OpenFile32 (ZipHandle handle, string filename)
		{
			return OpenFile32 (handle, filename, DEFAULT_COMPRESSION);
		}

		public static int OpenFile32 (ZipHandle handle, string filename, int compressionLevel)
		{
			ZipFileInfo32 fileInfo = new ZipFileInfo32 (DateTime.Now);
			int method = compressionLevel == 0 ? 0 : Z_DEFLATED;
			return zipOpenNewFileInZip_32 (handle, filename, ref fileInfo, IntPtr.Zero, 0, IntPtr.Zero, 0, "", method, compressionLevel);
		}

		public static int OpenFile64 (ZipHandle handle, string filename)
		{
			return OpenFile64 (handle, filename, DEFAULT_COMPRESSION);
		}

		public static int OpenFile64 (ZipHandle handle, string filename, int compressionLevel)
		{
			ZipFileInfo64 fileInfo = new ZipFileInfo64 (DateTime.Now);
			int method = compressionLevel == 0 ? 0 : Z_DEFLATED;
			return zipOpenNewFileInZip_64 (handle, filename, ref fileInfo, IntPtr.Zero, 0, IntPtr.Zero, 0, "", method, compressionLevel);
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

		[DllImport ("MonoPosixHelper", EntryPoint = "zipOpen2")]
		static extern ZipHandle zipOpen2_32 (string pathname,
		                                     int append,
		                                     IntPtr globalcomment, // zipcharpc*
		                                     ref ZlibFileFuncDef32 pzlib_filefunc_def); // zlib_filefunc_def*

		[DllImport ("MonoPosixHelper", EntryPoint = "zipOpen2")]
		static extern ZipHandle zipOpen2_64 (string pathname,
		                                     int append,
		                                     IntPtr globalcomment, // zipcharpc*
		                                     ref ZlibFileFuncDef64 pzlib_filefunc_def); // zlib_filefunc_def*

		[DllImport ("MonoPosixHelper")]
		static extern int zipClose (ZipHandle handle, string globalComment);

		[DllImport ("MonoPosixHelper", EntryPoint = "zipOpenNewFileInZip")]
		static extern int zipOpenNewFileInZip_32 (ZipHandle handle,
		                                          string filename,
		                                          ref ZipFileInfo32 zipfi,
		                                          IntPtr extrafield_local,
		                                          uint size_extrafield_local,
		                                          IntPtr extrafield_global,
		                                          uint size_extrafield_global,
		                                          string comment,
		                                          int method,
		                                          int level);

		[DllImport ("MonoPosixHelper", EntryPoint = "zipOpenNewFileInZip")]
		static extern int zipOpenNewFileInZip_64 (ZipHandle handle,
		                                          string filename,
		                                          ref ZipFileInfo64 zipfi,
		                                          IntPtr extrafield_local,
		                                          uint size_extrafield_local,
		                                          IntPtr extrafield_global,
		                                          uint size_extrafield_global,
		                                          string comment,
		                                          int method,
		                                          int level);
	}
}
