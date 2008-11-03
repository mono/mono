// ZipArchive.cs created with MonoDevelop
// User: alan at 16:31 20/10/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.IO;

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
			Handle = NativeZip.OpenArchive (Stream.IOFunctions, append);
		}

		public Stream GetStream (string filename)
		{
			if (FileActive)
				throw new InvalidOperationException ("A file is already open");
			
			NativeZip.OpenFile (Handle, filename);
			return new ZipWriteStream (this);
		}

		public void Dispose ()
		{
			NativeZip.CloseArchive (Handle);
			Stream.Close ();
		}
	}
}
