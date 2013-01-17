// System.Net.Sockets.SocketAsyncEventArgs.cs
//
// Authors:
//	Marek Habersack (mhabersack@novell.com)
//
// Copyright (c) 2008 Novell, Inc. (http://www.novell.com)
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

namespace System.Net.Sockets
{
	public class SendPacketsElement
	{
		public byte[] Buffer { get; private set; }
		public int Count { get; private set; }
		public bool EndOfPacket { get; private set; }
		public string FilePath { get; private set; }
		public int Offset { get; private set; }

		public SendPacketsElement (byte[] buffer) : this (buffer, 0, buffer != null ? buffer.Length : 0)
		{
		}

		public SendPacketsElement (byte[] buffer, int offset, int count) : this (buffer, offset, count, false)
		{
		}

		public SendPacketsElement (byte[] buffer, int offset, int count, bool endOfPacket)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			int buflen = buffer.Length;
			if (offset < 0 || offset >= buflen)
				throw new ArgumentOutOfRangeException ("offset");

			if (count < 0 || offset + count >= buflen)
				throw new ArgumentOutOfRangeException ("count");

			Buffer = buffer;
			Offset = offset;
			Count = count;
			EndOfPacket = endOfPacket;
			FilePath = null;
		}
		
		public SendPacketsElement (string filepath) : this (filepath, 0, 0, false)
		{
		}

		public SendPacketsElement (string filepath, int offset, int count) : this (filepath, offset, count, false)
		{
		}

		// LAME SPEC: only ArgumentNullException for filepath is thrown
		public SendPacketsElement (string filepath, int offset, int count, bool endOfPacket)
		{
			if (filepath == null)
				throw new ArgumentNullException ("filepath");

			Buffer = null;
			Offset = offset;
			Count = count;
			EndOfPacket = endOfPacket;
			FilePath = filepath;
		}
	}
}
