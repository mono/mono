//
// GZipFilter.cs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2003 Ximian, Inc (http://www.ximian.com)
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

