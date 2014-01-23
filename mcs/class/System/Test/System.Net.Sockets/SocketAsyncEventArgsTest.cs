//
// SocketAsyncEventArgsTest.cs - NUnit Test Cases for SocketAsyncEventArgs
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.Net.Sockets {

	[TestFixture]
	public class SocketAsyncEventArgsTest {

		[Test]
		public void Defaults ()
		{
			SocketAsyncEventArgs saea = new SocketAsyncEventArgs ();
			Assert.IsNull (saea.AcceptSocket, "AcceptSocket");
			Assert.IsNull (saea.Buffer, "Buffer");
			Assert.IsNull (saea.BufferList, "BufferList");
			Assert.AreEqual (0, saea.BytesTransferred, "BytesTransferred");
			Assert.AreEqual (0, saea.Count, "Count");
			Assert.IsFalse (saea.DisconnectReuseSocket, "DisconnectReuseSocket");
			Assert.AreEqual (SocketAsyncOperation.None, saea.LastOperation, "LastOperation");
			Assert.AreEqual (0, saea.Offset, "Offset");
			Assert.IsNull (saea.RemoteEndPoint, "RemoteEndPoint");
#if !MOBILE
			Assert.IsNotNull (saea.ReceiveMessageFromPacketInfo, "ReceiveMessageFromPacketInfo");
			Assert.IsNull (saea.SendPacketsElements, "SendPacketsElements");
			Assert.AreEqual (TransmitFileOptions.UseDefaultWorkerThread, saea.SendPacketsFlags, "SendPacketsFlags");
#endif
			Assert.AreEqual (-1, saea.SendPacketsSendSize, "SendPacketsSendSize");
			Assert.AreEqual (SocketError.Success, saea.SocketError, "SocketError");
			Assert.AreEqual (SocketFlags.None, saea.SocketFlags, "SocketFlags");
			Assert.IsNull (saea.UserToken, "UserToken");

			saea.Dispose ();
			saea.Dispose (); // twice
		}

		[Test]
		public void SetBuffer_ByteArray ()
		{
			SocketAsyncEventArgs saea = new SocketAsyncEventArgs ();

			byte [] buffer = new byte [0];
			saea.SetBuffer (buffer, 0, 0);
			Assert.AreEqual (0, saea.Buffer.Length, "0");
			Assert.AreSame (saea.Buffer, buffer, "same");

			saea.SetBuffer (null, 0, 0);
			Assert.IsNull (saea.Buffer, "null");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SetBuffer_BufferList_ByteArray ()
		{
			SocketAsyncEventArgs saea = new SocketAsyncEventArgs ();
			saea.BufferList = new List<ArraySegment<byte>> ();
			saea.SetBuffer (new byte [0], 0, 0);
		}

		[Test]
		public void SetBuffer_BufferList_NullByteArray ()
		{
			SocketAsyncEventArgs saea = new SocketAsyncEventArgs ();
			saea.BufferList = new List<ArraySegment<byte>> ();
			saea.SetBuffer (null, 0, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SetBuffer_ByteArray_BufferList ()
		{
			SocketAsyncEventArgs saea = new SocketAsyncEventArgs ();
			saea.SetBuffer (new byte [0], 0, 0);
			saea.BufferList = new List<ArraySegment<byte>> ();
		}

		[Test]
		public void SetBuffer_NullByteArray_BufferList ()
		{
			SocketAsyncEventArgs saea = new SocketAsyncEventArgs ();
			saea.SetBuffer (null, 0, 0);
			saea.BufferList = new List<ArraySegment<byte>> ();
			Assert.IsNull (saea.Buffer, "Buffer");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void SetBuffer_ByteArray_StartNegative ()
		{
			SocketAsyncEventArgs saea = new SocketAsyncEventArgs ();
			saea.SetBuffer (new byte [10], -1, 0);
		}

		[Test]
		public void SetBuffer_Null_StartNegative ()
		{
			SocketAsyncEventArgs saea = new SocketAsyncEventArgs ();
			saea.SetBuffer (null, -1, 0);
			Assert.IsNull (saea.Buffer, "Buffer");
			Assert.IsNull (saea.BufferList, "BufferList");
			Assert.AreEqual (0, saea.Count, "Count");
			Assert.AreEqual (0, saea.Offset, "Offset");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void SetBuffer_ByteArray_CountNegative ()
		{
			SocketAsyncEventArgs saea = new SocketAsyncEventArgs ();
			saea.SetBuffer (new byte [10], 1, -1);
		}

		[Test]
		public void SetBuffer_BufferList ()
		{
			SocketAsyncEventArgs saea = new SocketAsyncEventArgs ();
			saea.BufferList = new List<ArraySegment<byte>> ();
			saea.SetBuffer (1, -1);
			saea.SetBuffer (-1, 0);
		}

		[Test]
		public void SetBuffer_Null_CountNegative ()
		{
			SocketAsyncEventArgs saea = new SocketAsyncEventArgs ();
			saea.SetBuffer (null, 1, -1);
			Assert.IsNull (saea.Buffer, "Buffer");
			Assert.IsNull (saea.BufferList, "BufferList");
			Assert.AreEqual (0, saea.Count, "Count");
			Assert.AreEqual (0, saea.Offset, "Offset");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void SetBuffer_ByteArray_StartOverflow ()
		{
			SocketAsyncEventArgs saea = new SocketAsyncEventArgs ();
			saea.SetBuffer (new byte [10], Int32.MaxValue, 1);
		}

		[Test]
		public void SetBuffer_Null_StartOverflow ()
		{
			SocketAsyncEventArgs saea = new SocketAsyncEventArgs ();
			saea.SetBuffer (null, Int32.MaxValue, 1);
			Assert.IsNull (saea.Buffer, "Buffer");
			Assert.IsNull (saea.BufferList, "BufferList");
			Assert.AreEqual (0, saea.Count, "Count");
			Assert.AreEqual (0, saea.Offset, "Offset");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void SetBuffer_ByteArray_CountOverflow ()
		{
			SocketAsyncEventArgs saea = new SocketAsyncEventArgs ();
			saea.SetBuffer (new byte [10], 1, Int32.MaxValue);
		}

		[Test]
		public void SetBuffer_Null_CountOverflow ()
		{
			SocketAsyncEventArgs saea = new SocketAsyncEventArgs ();
			saea.SetBuffer (null, 1, Int32.MaxValue);
			Assert.IsNull (saea.Buffer, "Buffer");
			Assert.IsNull (saea.BufferList, "BufferList");
			Assert.AreEqual (0, saea.Count, "Count");
			Assert.AreEqual (0, saea.Offset, "Offset");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void SetBuffer_StartNegative ()
		{
			SocketAsyncEventArgs saea = new SocketAsyncEventArgs ();
			saea.SetBuffer (new byte [0], 0, 0);
			saea.SetBuffer (-1, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void SetBuffer_CountNegative ()
		{
			SocketAsyncEventArgs saea = new SocketAsyncEventArgs ();
			saea.SetBuffer (new byte [10], 1, -1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void SetBuffer_StartOverflow ()
		{
			SocketAsyncEventArgs saea = new SocketAsyncEventArgs ();
			saea.SetBuffer (new byte [10], Int32.MaxValue, 1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void SetBuffer_CountOverflow ()
		{
			SocketAsyncEventArgs saea = new SocketAsyncEventArgs ();
			saea.SetBuffer (new byte [10], 1, Int32.MaxValue);
		}

		class SocketAsyncEventArgsPoker : SocketAsyncEventArgs {

			public void OnCompleted_ (SocketAsyncEventArgs e)
			{
				base.OnCompleted (e);
			}
		}

		[Test]
		public void OnCompleted_Null ()
		{
			SocketAsyncEventArgsPoker saea = new SocketAsyncEventArgsPoker ();
			saea.OnCompleted_ (null);
		}
	}
}

#endif

