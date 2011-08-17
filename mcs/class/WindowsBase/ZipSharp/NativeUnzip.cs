// NativeUnzip.cs created with MonoDevelop
// User: alan at 13:11 20/10/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Runtime.InteropServices;
using System.Text;

namespace zipsharp
{
	static class NativeUnzip
	{
		enum ZipStringComparison
		{
			OSDefault = 0,
			CaseSensitive = 1,
			CaseInsensitive = 2
		}
		
		public static void CloseArchive (UnzipHandle handle)
		{
			unzClose (handle);
			handle.SetHandleAsInvalid ();
		}

		public static void CloseCurrentFile (UnzipHandle handle)
		{
			if (unzCloseCurrentFile (handle) != 0)
				throw new Exception ("Could not close the active file");
		}

		static CompressionOption ConvertCompression (int compressionLevel)
		{
			switch (compressionLevel)
			{
			case 1:
			case 2:
				return CompressionOption.SuperFast;
				
			case 3:
			case 4:
				return CompressionOption.Fast;
				
			case 5:
			case 6:
				return CompressionOption.Normal;
				
			case 7:
			case 8:
			case 9:
				return CompressionOption.Maximum;

			default:
				return CompressionOption.NotCompressed;
			}
		}

		public static long CurrentFilePosition (UnzipHandle handle)
		{
			return unztell(handle).ToInt64 ();
		}

		public static long CurrentFileLength (UnzipHandle handle)
		{
			UnzipFileInfo info;
			StringBuilder sbName = new StringBuilder (128);
			int result = unzGetCurrentFileInfo (handle, out info, sbName, new IntPtr (sbName.Capacity), IntPtr.Zero, IntPtr.Zero, null,  IntPtr.Zero);
			
			if (result != 0)
				return -1;
			else
				return (long)info.UncompressedSize;
		}
		
		static string GetCurrentFileName (UnzipHandle handle)
		{
			UnzipFileInfo info;
			StringBuilder sbName = new StringBuilder (128);
			int result = unzGetCurrentFileInfo (handle, out info, sbName, new IntPtr (sbName.Capacity), IntPtr.Zero, new IntPtr (0), null,  IntPtr.Zero);
			
			if (result != 0)
				return null;
			else
				return sbName.ToString ();
		}

		public static string[] GetFiles (UnzipHandle handle)
		{
			List<string> files = new List<string> ();

			GoToFirstFile (handle);

			string name;
			while ((name = GetCurrentFileName(handle)) != null)
			{
				files.Add (name);
				if (!NativeUnzip.GoToNextFile (handle))
					break;
			}
			
			return files.ToArray ();
		}

		static void GoToFirstFile (UnzipHandle handle)
		{
			if (NativeUnzip.unzGoToFirstFile (handle) != 0)
				throw new Exception ("Zip file is invalid");
		}

		static bool GoToNextFile (UnzipHandle handle)
		{
			return unzGoToNextFile(handle) == 0;
		}
		
		public static UnzipHandle OpenArchive (ZlibFileFuncDef fileFuncs)
		{
			UnzipHandle handle = unzOpen2 ("", ref fileFuncs);
			if (handle.IsInvalid)
				throw new Exception ("Could not open unzip archive");
			return handle;
		}

		public static void OpenFile (UnzipHandle handle, string name, out CompressionOption level)
		{
			if (unzLocateFile (handle, name, (int) ZipStringComparison.CaseInsensitive) != 0)
				throw new Exception ("The file doesn't exist");
			
			int method, compression;
			// '0' means do not open in raw mode (raw == do not decompress)
			if (unzOpenCurrentFile2 (handle, out method, out compression, 0) != 0)
				throw new Exception ("The file could not be opened");

			level = ConvertCompression (method == 0 ? 0 : compression);
		}

		public static unsafe int Read (UnzipHandle handle, byte[] buffer, int offset, int count)
		{
			if ((buffer.Length - offset) > count)
				throw new ArgumentOutOfRangeException ("count", "Buffer is too small to read that amount of data");
			
			fixed (byte * b = &buffer[offset])
				return unzReadCurrentFile (handle, b, (uint)count);
		}

		[DllImport ("MonoPosixHelper", CallingConvention=CallingConvention.Cdecl)]
		static extern int unzCloseCurrentFile (UnzipHandle handle);

		[DllImport ("MonoPosixHelper", CallingConvention=CallingConvention.Cdecl)]
		static extern IntPtr unztell (UnzipHandle handle);
		
		[DllImport ("MonoPosixHelper", CallingConvention=CallingConvention.Cdecl)]
		static extern int unzGoToFirstFile (UnzipHandle handle);

		[DllImport ("MonoPosixHelper", CallingConvention=CallingConvention.Cdecl)]
		static extern UnzipHandle unzOpen2 (string path,
		                                            ref ZlibFileFuncDef pzlib_filefunc_def);

		[DllImport ("MonoPosixHelper", CallingConvention=CallingConvention.Cdecl)]
		static extern int unzGoToNextFile (UnzipHandle handle);

		[DllImport ("MonoPosixHelper", CallingConvention=CallingConvention.Cdecl)]
		static extern int unzLocateFile (UnzipHandle handle,
		                                         string szFileName,
		                                         int iCaseSensitivity);

		[DllImport ("MonoPosixHelper", CallingConvention=CallingConvention.Cdecl)]
		static extern int unzOpenCurrentFile2 (UnzipHandle handle,
		                                       out int method,
		                                       out int level,
		                                       int raw);

		[DllImport ("MonoPosixHelper", CallingConvention=CallingConvention.Cdecl)]
		static extern int unzGetCurrentFileInfo (UnzipHandle handle,
		                                                 out UnzipFileInfo pfile_info,
		                                                 StringBuilder szFileName,
		                                                 IntPtr fileNameBufferSize,   // uLong
		                                                 IntPtr extraField,           // void *
		                                                 IntPtr extraFieldBufferSize, // uLong
		                                                 StringBuilder szComment,
		                                                 IntPtr commentBufferSize);   // uLong

		[DllImport ("MonoPosixHelper", CallingConvention=CallingConvention.Cdecl)]
		static unsafe extern int unzReadCurrentFile (UnzipHandle handle,
		                                              byte* buf, // voidp
		                                              uint len);

		//[DllImport ("MonoPosixHelper", CallingConvention=CallingConvention.Cdecl)]
		//static extern int unzSetOffset (UnzipHandle handle, IntPtr pos); // uLong
		
		[DllImport ("MonoPosixHelper", CallingConvention=CallingConvention.Cdecl)]
		static extern int unzClose (UnzipHandle handle);
	}
}
