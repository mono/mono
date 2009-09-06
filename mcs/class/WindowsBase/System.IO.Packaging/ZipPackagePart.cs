// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//

using System;
using System.IO;
using zipsharp;

namespace System.IO.Packaging {

	public sealed class ZipPackagePart : PackagePart
	{
		new ZipPackage Package {
			get { return (ZipPackage)base.Package; }
		}
		
		internal ZipPackagePart (Package package, Uri partUri)
			: base (package, partUri)
		{
			
		}

		internal ZipPackagePart (Package package, Uri partUri, string contentType)
			: base (package, partUri, contentType)
		{
			
		}

		internal ZipPackagePart (Package package, Uri partUri, string contentType, CompressionOption compressionOption )
			: base (package, partUri, contentType, compressionOption)
		{
			
		}

		protected override Stream GetStreamCore (FileMode mode, FileAccess access)
		{
			ZipPartStream zps;
			MemoryStream stream;
			if (Package.PartStreams.TryGetValue (Uri, out stream)) {
				zps = new ZipPartStream (Package, stream, access);
				if (mode == FileMode.Create)
					stream.SetLength (0);
				return new ZipPartStream (Package, stream, access);
			}
			
			stream = new MemoryStream ();
			try
			{
				using (UnzipArchive archive = new UnzipArchive (Package.PackageStream)) {
					foreach (string file in archive.GetFiles ()) {
						if (file != Uri.ToString ().Substring (1))
							continue;
						
						using (Stream archiveStream = archive.GetStream (file)) {
							int read = 0;
							byte[] buffer = new byte [Math.Min (archiveStream.Length, 2 * 1024)];
							while ((read = archiveStream.Read (buffer, 0, buffer.Length)) != 0)
								stream.Write (buffer, 0, read);
						}
					}
				}
			}
			catch
			{
				// The zipfile is invalid, so just create the file
				// as if it didn't exist
				stream.SetLength (0);
			}

			Package.PartStreams.Add (Uri, stream);
			if (mode == FileMode.Create)
				stream.SetLength (0);
			return new ZipPartStream (Package, stream, access);
		}
	}

}
