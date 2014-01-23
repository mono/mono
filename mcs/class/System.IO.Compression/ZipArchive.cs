//
// ZipArchive.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc. (http://www.xamarin.com)
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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace System.IO.Compression
{
	[MonoTODO]
	public class ZipArchive : IDisposable
	{
		public ZipArchive (Stream stream)
		{
			throw new NotImplementedException ();
		}

		public ZipArchive (Stream stream, ZipArchiveMode mode)
			: this (stream)
		{
		}

		public ZipArchive (Stream stream, ZipArchiveMode mode,
		                   bool leaveOpen)
			: this (stream, mode)
		{
		}

		public ZipArchive (Stream stream, ZipArchiveMode mode,
		                   bool leaveOpen, Encoding entryNameEncoding)
			: this (stream, mode, leaveOpen)
		{
		}

		public ReadOnlyCollection<ZipArchiveEntry> Entries {
			get {
				throw new NotImplementedException ();
			}
		}

		public ZipArchiveMode Mode {
			get {
				throw new NotImplementedException ();
			}
		}

		public ZipArchiveEntry CreateEntry (string entryName)
		{
			throw new NotImplementedException ();
		}

		public ZipArchiveEntry CreateEntry (string entryName,
		                                    CompressionLevel compressionLevel)
		{
			throw new NotImplementedException ();
		}

		public ZipArchiveEntry GetEntry (string entryName)
		{
			throw new NotImplementedException ();
		}

		protected virtual void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}

		public void Dispose ()
		{
			throw new NotImplementedException ();
		}
	}
}

