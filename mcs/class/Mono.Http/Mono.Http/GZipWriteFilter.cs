//
// GZipFilter.cs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2003 Ximian, Inc (http://www.ximian.com)
//

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

using System;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;

namespace Mono.Http
{
	public class GZipWriteFilter : Stream
	{
		Stream baseStream;
		GZipOutputStream st;

		public GZipWriteFilter (Stream baseStream)
		{
			// baseStream is not ready yet for filtering and any attemp to write
			// the gzip header will throw.
			this.baseStream = baseStream;
		}

		public override bool CanRead {
			get { return false; }
		}

		public override bool CanSeek {
                        get { return false; }
                }

                public override bool CanWrite {
                        get {
				if (st == null)
					return false;

				return st.CanWrite;
			}
                }

		public override long Length {
			get { return 0; }
		}

		public override long Position {
			get { return 0; }
			set { }
		}

		public override void Flush ()
		{
		}

		public override int Read (byte [] buffer, int offset, int count)
		{
			return 0;
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			return 0;
		}

		public override void SetLength (long value)
		{
		}

		public override void Write (byte [] buffer, int offset, int count)
		{
			if (st == null)
				st = new GZipOutputStream (baseStream);

			st.Write (buffer, offset, count);
		}

		public override void Close ()
		{
			if (st == null)
				return;
				
			st.Finish ();
			st = null;
			base.Close ();
		}
	}
}

