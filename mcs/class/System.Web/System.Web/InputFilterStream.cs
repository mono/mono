//
// System.Web.InputFilterStream
//
// Author:
//   Gonzalo Paniagua Javier (gonzalo@ximian.com
//
// (c) 2005 Novell, Inc. (http://www.novell.com)
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

using System.Runtime.InteropServices;
using System.IO;

namespace System.Web {

	class InputFilterStream : Stream {
		Stream stream;

		public InputFilterStream ()
		{
		}

		internal Stream BaseStream {
			set { stream = value; }
		}

		public override bool CanRead {
			get {
				return true;
			}
		}

		public override bool CanSeek {
			get {
				return true;
			}
		}

		public override bool CanWrite {
			get {
				return false;
			}
		}

		public override long Position {
			get {
				return stream.Position;
			}

			set {
				stream.Position = value;
			}
		}

		public override long Length {
			get {
				return stream.Length;
			}
		}

		public override int Read (byte [] buffer, int offset, int count)
		{
			return stream.Read (buffer, offset, count);
		}

		public override int ReadByte ()
		{
			return stream.ReadByte ();
		}

		public override long Seek (long offset, SeekOrigin loc)
		{
			return stream.Seek (offset, loc);
		}
		
		public override void SetLength (long value)
		{
			throw new NotSupportedException ("This stream can not change its size");
		}

		public override void Write (byte [] buffer, int offset, int count)
		{
			throw new NotSupportedException ("This stream can not change its size");
		}

		public override void WriteByte (byte value)
		{
			throw new NotSupportedException ("This stream can not change its size");
		}
		
		public override void Flush ()
		{
		}

		public override void Close ()
		{
			stream.Close ();
		}
	}
}

