// UnzipArchive.cs
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

namespace zipsharp
{
	class UnzipArchive : IDisposable
	{
		string[] files;
		
		internal bool FileActive {
			get; set;
		}
		
		string[] Files {
			get {
				if (files == null)
					files = NativeVersion.Use32Bit ? NativeUnzip.GetFiles32 (Handle) : NativeUnzip.GetFiles64 (Handle);
				return files;
			}
		}
		
		internal UnzipHandle Handle {
			get; private set;
		}
		
		ZipStream Stream {
			get; set;
		}

		public UnzipArchive (string filename)
			: this (File.Open (filename, FileMode.Open), true)
		{
			
		}

		public UnzipArchive (Stream stream)
			: this (stream, false)
		{
			
		}

		public UnzipArchive (Stream stream, bool ownsStream)
		{
			Stream = new ZipStream (stream, ownsStream);
			Handle = NativeVersion.Use32Bit ? NativeUnzip.OpenArchive32 (Stream.IOFunctions32) : NativeUnzip.OpenArchive64 (Stream.IOFunctions64);
		}

		public void Dispose ()
		{
			NativeUnzip.CloseArchive (Handle);
			Stream.Close ();
		}

		public System.IO.Packaging.CompressionOption GetCompressionLevel (string file)
		{
			using (UnzipReadStream stream = (UnzipReadStream) GetStream (file))
				return stream.CompressionLevel;
		}

		public string[] GetFiles ()
		{
			return (string []) Files.Clone ();
		}

		public Stream GetStream (string name)
		{
			foreach (string file in Files)
			{
				if (name.Equals(file, StringComparison.OrdinalIgnoreCase))
				{
					System.IO.Packaging.CompressionOption option;
					NativeUnzip.OpenFile (Handle, name, out option);
					return new UnzipReadStream (this, option);
				}
			}
			
			throw new Exception ("The file doesn't exist in the zip archive");
		}
	}
}
