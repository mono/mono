// ZipReadStream.cs
//
// Copyright (c) 2008 [copyright holders]
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//

using System;
using System.IO;
using System.IO.Packaging;

namespace zipsharp
{
	class UnzipReadStream : Stream
	{
		long length;
		
		UnzipArchive Archive { get; set; }

		public override bool CanRead {
			get { return true; }
		}
		
		public override bool CanSeek {
			get { return false; }
		}

		public override bool CanWrite {
			get { return false; }
		}

		public override bool CanTimeout {
			get { return false; }
		}

		public CompressionOption CompressionLevel {
			get; set;
		}

		public override long Length {
			get {
				return length;
			}
		}

		public override long Position {
			get { return NativeUnzip.CurrentFilePosition (Archive.Handle); }
			set { throw new NotSupportedException (); }
		}
		
		public UnzipReadStream (UnzipArchive archive, CompressionOption compressionLevel)
		{
			Archive = archive;
			Archive.FileActive = true;
			CompressionLevel = compressionLevel;
			length = NativeVersion.Use32Bit ? NativeUnzip.CurrentFileLength32 (Archive.Handle) : NativeUnzip.CurrentFileLength64 (Archive.Handle);
		}

		public override void Close()
		{
			Archive.FileActive = false;
			NativeUnzip.CloseCurrentFile (Archive.Handle);
		}

		public override void Flush()
		{
			
		}
 
		public override int Read(byte[] buffer, int offset, int count)
		{
			return NativeUnzip.Read (Archive.Handle, buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException ();
		}
		
		public override void SetLength(long value)
		{
			throw new NotSupportedException ();
		}
		
		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException ();
		}
	}
}
