//
// System.Web.TempFileStream.cs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
//
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

using System.IO;
using System.Runtime.InteropServices;
namespace System.Web {
	internal class TempFileStream : FileStream {
		bool read_mode;
		bool disposed;
		long saved_position;

		public TempFileStream (string name)
			: base (name, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 8192)
		{
		}

		public override bool CanRead {
			get { return read_mode; }
		}

                public override bool CanWrite {
                        get { return !read_mode; }
                }

		public void SavePosition ()
		{
			saved_position = Position;
			Position = 0;
		}

		public void RestorePosition ()
		{
			Position = saved_position;
			saved_position = -1;
		}

		public void SetReadOnly ()
		{
			read_mode = true;
			Position = 0;
		}

		public void SetWriteOnly ()
		{
			read_mode = false;
			Position = 0;
		}

		public override void Write (byte [] buffer, int offset, int count)
		{
			if (read_mode)
				throw new InvalidOperationException ("mode read");

			base.Write (buffer, offset, count);
		}

		public override int Read ([In,Out] byte [] buffer, int offset, int count)
		{
			if (!read_mode)
				throw new InvalidOperationException ("mode write");

			return base.Read (buffer, offset, count);
		}

		protected override void Dispose (bool disposing)
		{
			if (!disposed) {
				disposed = true;
				base.Dispose (disposing);
				try {
					File.Delete (Name);
				} catch {}
			}
		}
	}
}

