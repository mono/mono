//
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
//
// Authors:
//	Vladimir Krasnov <vladimirk@mainsoft.com>
//	Konstantin Triger <kostat@mainsoft.com>
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

namespace System.Web.J2EE
{
	internal class ObjectOutputStream : System.IO.Stream {
		readonly java.io.ObjectOutput _javaObjectOutput;

		public ObjectOutputStream(java.io.ObjectOutput stream) {
			_javaObjectOutput = stream;
		}

		public override bool CanRead {
			get {
				return false;
			}
		}

		public override bool CanSeek {
			get {
				return false;
			}
		}

		public override bool CanWrite {
			get {
				return true;
			}
		}

		public override void Close() {
			_javaObjectOutput.close();
		}

		public override void Flush() {
			_javaObjectOutput.flush();
		}

		public override long Length {
			get {
				throw new NotSupportedException();
			}
		}

		public override long Position {
			get {
				throw new NotSupportedException();
			}
			set {
				throw new NotSupportedException();
			}
		}

		public override long Seek(long offset, System.IO.SeekOrigin origin) {
			throw new NotSupportedException();
		}

		public override void SetLength(long value) {
			throw new NotSupportedException();
		}

		public override int Read(byte[] buffer, int offset, int count) {
			throw new NotSupportedException();
		}

		public override void Write(byte[] buffer, int offset, int count) {
			_javaObjectOutput.write( vmw.common.TypeUtils.ToSByteArray(buffer), offset, count );
		}

		public override void WriteByte(byte value) {
			_javaObjectOutput.write(value);
		}

		public java.io.ObjectOutput NativeStream {
			get {return _javaObjectOutput;}
		}
	}
}
