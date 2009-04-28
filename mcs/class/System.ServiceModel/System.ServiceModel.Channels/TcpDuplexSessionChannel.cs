// 
// TcpDuplexSessionChannel.cs
// 
// Author: 
//	Marcos Cobena (marcoscobena@gmail.com)
//	Atsushi Enomoto  <atsushi@ximian.com>
// 
// Copyright 2007 Marcos Cobena (http://www.youcannoteatbits.org/)
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

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;

namespace System.ServiceModel.Channels
{
	internal class TcpDuplexSessionChannel : DuplexChannelBase, IDuplexSessionChannel
	{
		TcpChannelInfo info;
		TcpClient client;
		bool is_service_side;
		EndpointAddress local_address;
		TcpListener tcp_listener;
		TimeSpan timeout;
		
		public TcpDuplexSessionChannel (ChannelFactoryBase factory, TcpChannelInfo info, EndpointAddress address, Uri via)
			: base (factory, address, via)
		{
			is_service_side = false;
			this.info = info;
		}
		
		public TcpDuplexSessionChannel (ChannelListenerBase listener, TcpChannelInfo info, TcpClient acceptedRequest, TimeSpan timeout)
			: base (listener)
		{
			is_service_side = true;
			this.info = info;
			this.client = acceptedRequest;
			this.timeout = timeout;

			Stream s = client.GetStream ();

			//while (s.CanRead)
			//	Console.Write ("{0:X02} ", s.ReadByte ());
			
			for (int i = 0; i < 6; i++)
				s.ReadByte ();
			
			int size = s.ReadByte ();
			
			for (int i = 0; i < size; i++)
				s.ReadByte (); // URI
			
			s.ReadByte ();
			s.ReadByte ();
			s.ReadByte ();
			s.WriteByte (0x0B);
		}
		
		public MessageEncoder Encoder {
			get { return info.MessageEncoder; }
		}

		public override EndpointAddress LocalAddress {
			get { return local_address; }
		}
		
		// FIXME: implement
		public IDuplexSession Session {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public override IAsyncResult BeginSend (Message message, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override IAsyncResult BeginSend (Message message, TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override void EndSend (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		public override void Send (Message message)
		{
			Send (message, DefaultSendTimeout);
		}
		
		public override void Send (Message message, TimeSpan timeout)
		{
			client.SendTimeout = (int) timeout.TotalMilliseconds;
			MemoryStream ms = new MemoryStream ();
			BinaryFormatter bf = new BinaryFormatter ();
			
			try
			{
				NetworkStream stream = client.GetStream ();
				MyBinaryWriter bw = new MyBinaryWriter (stream);
				bw.Write ((byte) 6);
				Encoder.WriteMessage (message, ms);
				bw.WriteBytes (ms.ToArray ());
				bw.Write ((byte) 7);
				bw.Flush ();

				stream.ReadByte (); // 7
			}
			catch (Exception e)
			{
				throw e;
			}
		}
		
		[MonoTODO]
		public override IAsyncResult BeginReceive (AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override IAsyncResult BeginReceive (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override IAsyncResult BeginTryReceive (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override IAsyncResult BeginWaitForMessage (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override Message EndReceive (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override bool EndTryReceive (IAsyncResult result, out Message message)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override bool EndWaitForMessage (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}
		
		public override Message Receive ()
		{
			return Receive (DefaultReceiveTimeout);
		}
		
		public override Message Receive (TimeSpan timeout)
		{
			client.ReceiveTimeout = (int) timeout.TotalMilliseconds;
			Stream s = client.GetStream ();
			var packetType = s.ReadByte (); // 6
			if (packetType != 6)
				throw new NotImplementedException (String.Format ("Packet type {0:X} is not implemented", packetType));
			var br = new BinaryFrameSupportReader (s);

			// FIXME: implement [MC-NMF] correctly. Currently it is a guessed protocol hack.
			byte [] buffer = br.ReadSizedChunk ();

			var ms = new MemoryStream (buffer, 0, buffer.Length);
			// The returned buffer consists of a serialized reader 
			// session and the binary xml body. 
			// FIXME: turned out that it could be either in-band dictionary ([MC-NBFSE]), or a mere xml body ([MC-NBFS]).

			var session = new XmlBinaryReaderSession ();
			byte [] rsbuf = new BinaryFrameSupportReader (ms).ReadSizedChunk ();
			int count = 0;
			using (var rms = new MemoryStream (rsbuf, 0, rsbuf.Length)) {
				var rbr = new BinaryReader (rms, Encoding.UTF8);
				while (rms.Position < rms.Length)
					session.Add (count++, rbr.ReadString ());
			}
			var benc = Encoder as BinaryMessageEncoder;
			if (benc != null)
				benc.CurrentBinarySession = session;
			// FIXME: supply maxSizeOfHeaders.
			Message msg = Encoder.ReadMessage (ms, 0x10000);
			if (benc != null)
				benc.CurrentBinarySession = null;
//			s.ReadByte (); // 7
//			s.WriteByte (7);
			s.Flush ();

			return msg;
		}
		
		public override bool TryReceive (TimeSpan timeout, out Message message)
		{
			try {
				message = Receive (timeout);
				return true;
			} catch (TimeoutException) {
				message = null;
				return false;
			}
		}
		
		public override bool WaitForMessage (TimeSpan timeout)
		{
			client.ReceiveTimeout = (int) timeout.TotalMilliseconds;
			try {
				client.GetStream ();
				return true;
			} catch (TimeoutException) {
				return false;
			}
		}
		
		// CommunicationObject
		
		[MonoTODO]
		protected override void OnAbort ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override IAsyncResult OnBeginClose (TimeSpan timeout,
			AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override IAsyncResult OnBeginOpen (TimeSpan timeout,
			AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnClose (TimeSpan timeout)
		{
			client.Close ();
		}
		
		[MonoTODO]
		protected override void OnEndClose (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnEndOpen (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnOpen (TimeSpan timeout)
		{
			if (! is_service_side) {
				int explicitPort = RemoteAddress.Uri.Port;
				client = new TcpClient (RemoteAddress.Uri.Host, explicitPort <= 0 ? TcpTransportBindingElement.DefaultPort : explicitPort);
				                        //RemoteAddress.Uri.Port);
				
				NetworkStream ns = client.GetStream ();
				ns.WriteByte (0); // Version record
				ns.WriteByte (1); //  - major
				ns.WriteByte (0); //  - minor
				ns.WriteByte (1); // Mode record
				ns.WriteByte (2); //  - mode - Duplex
				ns.WriteByte (2); // Via record
				byte [] bytes = System.Text.Encoding.UTF8.GetBytes (RemoteAddress.Uri.ToString ());
				ns.WriteByte ((byte) bytes.Length);
				ns.Write (bytes, 0, bytes.Length);
				ns.WriteByte (3); // Known encoding record
				ns.WriteByte (3); //  - encoding - UTF8
				ns.WriteByte (0xC); // Preamble end record
				if (ns.ReadByte () != 0x0B)
					throw new NotImplementedException ("Preamble Ack was expected");
				//while (ns.CanRead)
				//	Console.Write ("{0:X02} ", ns.ReadByte ());
			}
			// Service side.
			/*
			else
				Console.WriteLine ("Server side.");
			*/
		}
		
		class MyBinaryWriter : BinaryWriter
		{
			public MyBinaryWriter (Stream s)
				: base (s)
			{
			}
			
			public void WriteBytes (byte [] bytes)
			{
				Write7BitEncodedInt (bytes.Length);
				Write (bytes);
			}
		}
	}
		
	class BinaryFrameSupportReader : BinaryReader
	{
		public BinaryFrameSupportReader (Stream s)
			: base (s)
		{
		}
		
		public byte [] ReadSizedChunk ()
		{
			int length = Read7BitEncodedInt ();
			
			if (length > 65536)
				throw new InvalidOperationException ("The message is too large.");

			byte [] buffer = new byte [length];
			Read (buffer, 0, length);
			
			return buffer;
		}
	}
}
