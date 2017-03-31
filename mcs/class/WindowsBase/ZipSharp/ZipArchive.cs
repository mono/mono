// ZipArchive.cs created with MonoDevelop
// User: alan at 16:31 20/10/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.IO;
using System.IO.Packaging;

namespace zipsharp
{
	class ZipArchive : IDisposable
	{
		internal bool FileActive { get; set; }
		internal ZipHandle Handle { get; private set; }
		ZipStream Stream { get; set; }
	
		public ZipArchive (string filename, Append append)
			: this (File.Open (filename, FileMode.OpenOrCreate), append)
		{

		}

		public ZipArchive (Stream stream, Append append)
			: this (stream, append, false)
		{
			
		}

		public ZipArchive (Stream stream, Append append, bool ownsStream)
		{
			Stream = new ZipStream (stream, ownsStream);
			Handle = NativeVersion.Use32Bit ? NativeZip.OpenArchive32 (Stream.IOFunctions32, append) : NativeZip.OpenArchive64 (Stream.IOFunctions64, append);
		}

		
		static int ConvertCompression (System.IO.Packaging.CompressionOption option)
		{
			switch (option)
			{
			case CompressionOption.SuperFast:
				return 2;
				
			case CompressionOption.Fast:
				return 4;
				
			case CompressionOption.Normal:
				return 6;
				
			case CompressionOption.Maximum:
				return 9;

			default:
				return 0;
			}
		}


		public Stream GetStream (string filename, System.IO.Packaging.CompressionOption option)
		{
			if (FileActive)
				throw new InvalidOperationException ("A file is already open");

			if (NativeVersion.Use32Bit)
				NativeZip.OpenFile32 (Handle, filename, ConvertCompression (option));
			else
				NativeZip.OpenFile64 (Handle, filename, ConvertCompression (option));
			return new ZipWriteStream (this);
		}

		public void Dispose ()
		{
			NativeZip.CloseArchive (Handle);
			Stream.Close ();
		}
	}
}
