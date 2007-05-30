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
using java.io;

namespace Mainsoft.Web.SessionState
{
	/// <summary>
	/// <para>This class supports the Framework infrastructure and is not intended to be used directly from your code.</para>
	/// </summary>
	public sealed partial class ServletSessionStateStoreProvider
	{
		sealed class ObjectInputStream : System.IO.Stream, ObjectInput
		{
			readonly ObjectInput _javaObjectInput;

			public ObjectInputStream (ObjectInput stream) {
				_javaObjectInput = stream;
			}

			public override bool CanRead {
				get {
					return true;
				}
			}

			public override bool CanWrite {
				get {
					return false;
				}
			}

			public override bool CanSeek {
				get {
					return true;
				}
			}

			public override long Length {
				get {
					throw new NotSupportedException ();
				}
			}

			public override long Position {
				get {
					throw new NotSupportedException ();
				}
				set {
					throw new NotSupportedException ();
				}
			}

			public override void Flush () {
				throw new NotSupportedException ();
			}

			public override long Seek (long offset, System.IO.SeekOrigin origin) {
				if (origin == System.IO.SeekOrigin.Current)
					return _javaObjectInput.skip (offset);

				throw new NotSupportedException ();
			}

			public override void SetLength (long value) {
				throw new NotSupportedException ();
			}

			public override int Read (byte [] buffer, int offset, int count) {
				int rv = _javaObjectInput.read (vmw.common.TypeUtils.ToSByteArray (buffer), offset, count);
				return rv > 0 ? rv : 0;
			}

			public override void Write (byte [] buffer, int offset, int count) {
				throw new NotSupportedException ();
			}

			public override int ReadByte () {
				return _javaObjectInput.read ();
			}

			public override void Close () {
				_javaObjectInput.close ();
			}

			#region ObjectInput Members

			public int available () {
				return _javaObjectInput.available ();
			}

			public void close () {
				_javaObjectInput.close ();
			}

			public int read (sbyte [] __p1, int __p2, int __p3) {
				return _javaObjectInput.read (__p1, __p2, __p3);
			}

			public int read (sbyte [] __p1) {
				return _javaObjectInput.read (__p1);
			}

			public int read () {
				return _javaObjectInput.read ();
			}

			public object readObject () {
				return _javaObjectInput.readObject ();
			}

			public long skip (long __p1) {
				return _javaObjectInput.skip (__p1);
			}

			#endregion

			#region DataInput Members

			public bool readBoolean () {
				return _javaObjectInput.readBoolean ();
			}

			public sbyte readByte () {
				return _javaObjectInput.readByte ();
			}

			public char readChar () {
				return _javaObjectInput.readChar ();
			}

			public double readDouble () {
				return _javaObjectInput.readDouble ();
			}

			public float readFloat () {
				return _javaObjectInput.readFloat ();
			}

			public void readFully (sbyte [] __p1, int __p2, int __p3) {
				_javaObjectInput.readFully (__p1, __p2, __p3);
			}

			public void readFully (sbyte [] __p1) {
				_javaObjectInput.readFully (__p1);
			}

			public int readInt () {
				return _javaObjectInput.readInt ();
			}

			public string readLine () {
				return _javaObjectInput.readLine ();
			}

			public long readLong () {
				return _javaObjectInput.readLong ();
			}

			public short readShort () {
				return _javaObjectInput.readShort ();
			}

			public string readUTF () {
				return _javaObjectInput.readUTF ();
			}

			public int readUnsignedByte () {
				return _javaObjectInput.readUnsignedByte ();
			}

			public int readUnsignedShort () {
				return _javaObjectInput.readUnsignedShort ();
			}

			public int skipBytes (int __p1) {
				return _javaObjectInput.skipBytes (__p1);
			}

			#endregion
		}
	}
}
