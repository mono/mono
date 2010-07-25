// 
// Novell.Directory.Ldap.Security.SecureStream.cs
//
// Authors:
//  Boris Kirzner <borsk@mainsoft.com>
//	Konstantin Triger <kostat@mainsoft.com>
//	
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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

namespace Novell.Directory.Ldap.Security
{
	internal class SecureStream : Stream
	{
		#region Fields

		private readonly Stream _stream;
		private readonly Krb5Helper _helper;

		private readonly byte [] _lenBuf = new byte [4];  
		private byte [] _buffer;
		private int _bufferPosition;

		#endregion // Fields

		#region Constructors

		public SecureStream(Stream stream, Krb5Helper helper): base () 
		{
			_stream = stream;
			_helper = helper;
		}

		#endregion // Constructors

		#region Properties

		public override bool CanRead 
		{ 
			get { return _stream.CanRead; }
		}

		public override bool CanSeek 
		{ 
			get { return _stream.CanSeek; } 
		}

		public override bool CanWrite 
		{ 
			get { return _stream.CanWrite; } 
		}

		public override long Length 
		{ 
			get { throw new NotSupportedException (); } 
		}

		public override long Position 
		{ 
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}

		#endregion // Properties

		#region Methods

		public override void Flush()
		{
			_stream.Flush ();
		}

		public override int Read( byte [] buffer, int offset, int count)
		{
			if (_buffer == null || _bufferPosition >= _buffer.Length) {
				int actual = Fill ();
				while (actual == 0)
					actual = Fill ();

				if (actual == -1)
					return -1;				
			}

			int available = _buffer.Length - _bufferPosition;
			if (count > available) {
				Array.Copy (_buffer, _bufferPosition, buffer, offset, available);
				_bufferPosition = _buffer.Length;
				return available;
			}
			else {
				Array.Copy (_buffer, _bufferPosition, buffer, offset, count);
				_bufferPosition += count;
				return count;
			}		
		}

		public override void Close() {
			_stream.Close();
			_helper.Dispose();
		}

		private int Fill()
		{
			int actual = ReadAll (_lenBuf, 4);
	
			if (actual != 4) 
				return -1;

			int length = NetworkByteOrderToInt (_lenBuf, 0, 4);

//			if (length > _recvMaxBufSize)
//				throw new LdapException(length + " exceeds the negotiated receive buffer size limit: " + _recvMaxBufSize, 80, "");

			byte [] rawBuffer = new byte [length];
			actual = ReadAll (rawBuffer, length);

			if (actual != length)
				throw new LdapException("Expected to read " + length + " bytes, but get " + actual, 80, "");

			_buffer = _helper.Unwrap (rawBuffer, 0, length);
			_bufferPosition = 0;
			return _buffer.Length;
		}

		private int ReadAll(byte [] buffer, int total)
		{
			int count = 0;
			int pos = 0;
			while (total > 0) {
				count = _stream.Read (buffer, pos, total);

				if (count == -1)
					break;
					//return ((pos == 0) ? -1 : pos);

				pos += count;
				total -= count;
			}
			return pos;
		}

		public override long Seek(long offset, SeekOrigin loc)
		{
			return _stream.Seek (offset, loc);
		}

		public override void SetLength(long value)
		{
			_stream.SetLength (value);
		}

		public override void Write(byte [] buffer, int offset, int count)
		{
			// FIXME: use GSSCOntext.getWrapSizeLimit to divide the buffer
			// Generate wrapped token 
			byte [] wrappedToken = _helper.Wrap (buffer, offset, count);
			// Write out length
			IntToNetworkByteOrder (wrappedToken.Length, _lenBuf, 0, 4);
			_stream.Write (_lenBuf, 0, 4);
			// Write out wrapped token
			_stream.Write (wrappedToken, 0, wrappedToken.Length);
		}

		internal static int NetworkByteOrderToInt(byte [] buf, int start, int count) 
		{
			int answer = 0;
			for (int i = 0; i < count; i++) {
				answer <<= 8;
				answer |= ((int)buf [start + i] & 0xff);
			}
			return answer;
		}

		internal static void IntToNetworkByteOrder(int num, byte [] buf, int start, int count) 
		{
			for (int i = count-1; i >= 0; i--) {
				buf [start + i] = (byte)(num & 0xff);
				num >>= 8;
			}
		}

		#endregion // Methods
	}
}
