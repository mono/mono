// Transport Security Layer (TLS)
// Copyright (c) 2003-2004 Carlos Guzman Alvarez

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
using System.Net;

namespace Mono.Security.Protocol.Tls
{
	internal class TlsStream : Stream
	{
		#region Fields

		private bool				canRead;
		private bool				canWrite;
		private MemoryStream		buffer;

		#endregion

		#region Properties

		public bool EOF
		{
			get 
			{
				if (this.Position < this.Length)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
		}

		#endregion

		#region Stream Properties

		public override bool CanWrite
		{
			get { return this.canWrite; }
		}

		public override bool CanRead
		{
			get { return this.canRead; }
		}

		public override bool CanSeek
		{
			get { return this.buffer.CanSeek; }
		}

		public override long Position
		{
			get { return this.buffer.Position; }
			set { this.buffer.Position = value; }
		}

		public override long Length
		{
			get { return this.buffer.Length; }
		}

		#endregion

		#region Constructors

		public TlsStream() : base()
		{
			this.buffer		= new MemoryStream(0);
			this.canRead	= false;
			this.canWrite	= true;
		}

		public TlsStream(byte[] data) : base()
		{
			this.buffer		= new MemoryStream(data);
			this.canRead	= true;
			this.canWrite	= false;
		}

		#endregion

		#region Specific Read Methods

		public new byte ReadByte()
		{
			return (byte)base.ReadByte();
		}

		public short ReadInt16()
		{
			byte[] bytes = this.ReadBytes(2);

			return IPAddress.HostToNetworkOrder(BitConverter.ToInt16(bytes, 0));
		}

		public int ReadInt24()
		{
			byte[] b = this.ReadBytes(3);
			
			return ((b[0] & 0xff) << 16) | ((b[1] & 0xff) << 8) | (b[2] & 0xff);
		}

		public int ReadInt32()
		{
			byte[] bytes = this.ReadBytes(4);

			return IPAddress.HostToNetworkOrder(BitConverter.ToInt32(bytes, 0));
		}

		public long ReadInt64()
		{
			byte[] bytes = this.ReadBytes(8);
						
			return IPAddress.HostToNetworkOrder(BitConverter.ToInt64(bytes, 0));
		}

		public byte[] ReadBytes(int count)
		{
			byte[] bytes = new byte[count];
			this.Read(bytes, 0, count);

			return bytes;
		}

		#endregion

		#region Specific Write Methods

		public void Write(byte value)
		{
			this.WriteByte(value);
		}

		public void Write(short value)
		{
			byte[] bytes = BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder(value));
			this.Write(bytes);
		}

		public void WriteInt24(int value)
		{
			int int24 = IPAddress.HostToNetworkOrder(value);
			byte[] content = new byte[3];
				
			Buffer.BlockCopy(BitConverter.GetBytes(int24), 1, content, 0, 3);

			this.Write(content);
		}

		public void Write(int value)
		{
			byte[] bytes = BitConverter.GetBytes((int)IPAddress.HostToNetworkOrder(value));
			this.Write(bytes);
		}

		public void Write(long value)
		{
			byte[] bytes = BitConverter.GetBytes((long)IPAddress.HostToNetworkOrder(value));
			this.Write(bytes);
		}

		public void Write(byte[] buffer)
		{
			this.Write(buffer, 0, buffer.Length);
		}

		#endregion

		#region Methods

		public void Reset()
		{
			this.buffer.SetLength(0);
			this.buffer.Position = 0;
		}

		public byte[] ToArray()
		{
			return this.buffer.ToArray();
		}

		#endregion

		#region Stream Methods

		public override void Flush()
		{
			this.buffer.Flush();
		}

		public override void SetLength(long length)
		{
			this.buffer.SetLength(length);
		}

		public override long Seek(long offset, System.IO.SeekOrigin loc)
		{
			return this.buffer.Seek(offset, loc);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (this.canRead)
			{
				return this.buffer.Read(buffer, offset, count);
			}
			throw new InvalidOperationException("Read operations are not allowed by this stream");
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (this.canWrite)
			{
				this.buffer.Write(buffer, offset, count);
			}
			else
			{
				throw new InvalidOperationException("Write operations are not allowed by this stream");
			}
		}

		#endregion
	}
}
