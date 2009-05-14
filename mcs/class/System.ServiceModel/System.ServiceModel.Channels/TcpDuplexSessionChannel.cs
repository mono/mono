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
		TcpBinaryFrameManager frame;
		
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

			frame = new TcpBinaryFrameManager (TcpBinaryFrameManager.DuplexMode, s);
			frame.ProcessPreambleRecipient ();

			// FIXME: use retrieved record properties in the request processing.
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

			// FIXME: implement [MC-NMF] correctly. Currently it is a guessed protocol hack.
			byte [] buffer = frame.ReadSizedChunk ();

			var ms = new MemoryStream (buffer, 0, buffer.Length);
			// The returned buffer consists of a serialized reader 
			// session and the binary xml body. 
			// FIXME: turned out that it could be either in-band dictionary ([MC-NBFSE]), or a mere xml body ([MC-NBFS]).

			var session = new XmlBinaryReaderSession ();
			byte [] rsbuf = new TcpBinaryFrameManager (0, ms).ReadSizedChunk ();
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
				frame = new TcpBinaryFrameManager (TcpBinaryFrameManager.DuplexMode, ns);
				// FIXME: it still results in SocketException (remote host closes the connection).
				frame.ProcessPreambleInitiator ();
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

	// seealso: [MC-NMF] Windows Protocol document.
	class TcpBinaryFrameManager : BinaryReader
	{
		public const byte VersionRecord = 0;
		public const byte ModeRecord = 1;
		public const byte ViaRecord = 2;
		public const byte KnownEncodingRecord = 3;
		public const byte ExtendingEncodingRecord = 4;
		public const byte UnsizedEnvelopeRecord = 5;
		public const byte SizedEnvelopeRecord = 6;
		public const byte EndRecord = 7;
		public const byte FaultRecord = 8;
		public const byte UpgradeRequestRecord = 9;
		public const byte UpgradeResponseRecord = 0xA;
		public const byte PreambleAckRecord = 0xB;
		public const byte PreambleEndRecord = 0xC;

		public const byte SingletonUnsizedMode = 1;
		public const byte DuplexMode = 2;
		public const byte SimplexMode = 3;
		public const byte SingletonSizedMode = 4;

		public TcpBinaryFrameManager (int mode, Stream s)
			: base (s)
		{
			this.mode = mode;
			this.s = s;
		}

		Stream s;

		int mode;
		int encoding_record = 3; // SOAP12, UTF-8

		public byte [] ReadSizedChunk ()
		{
			int length = Read7BitEncodedInt ();
			
			if (length > 65536)
				throw new InvalidOperationException ("The message is too large.");

			byte [] buffer = new byte [length];
			Read (buffer, 0, length);
			
			return buffer;
		}

		public Uri Via { get; set; }

		public void ProcessPreambleInitiator ()
		{
			s.WriteByte (VersionRecord);
			s.WriteByte (1);
			s.WriteByte (0);
			s.WriteByte (ModeRecord);
			s.WriteByte ((byte) mode);
			s.WriteByte (KnownEncodingRecord); // FIXME
			s.WriteByte ((byte) encoding_record);
			s.WriteByte (PreambleEndRecord);
			s.Flush ();

			int b;
			if ((b = s.ReadByte ()) != PreambleAckRecord)
				throw new ArgumentException (String.Format ("Preamble Ack Record is expected, got {0:X}", b));
		}

		public void ProcessPreambleRecipient ()
		{
			bool preambleEnd = false;
			while (!preambleEnd) {
				int b = s.ReadByte ();
				switch (b) {
				case VersionRecord:
					if (s.ReadByte () != 1)
						throw new ArgumentException ("Major version must be 1");
					if (s.ReadByte () != 0)
						throw new ArgumentException ("Minor version must be 0");
					break;
				case ModeRecord:
					if (s.ReadByte () != mode)
						throw new ArgumentException (String.Format ("Duplex mode is expected to be {0:X}", mode));
					break;
				case ViaRecord:
					Via = new Uri (ReadString ());
					break;
				case KnownEncodingRecord:
					encoding_record = s.ReadByte ();
					break;
				case ExtendingEncodingRecord:
					throw new NotImplementedException ();
				case UpgradeRequestRecord:
					throw new NotImplementedException ();
				case UpgradeResponseRecord:
					throw new NotImplementedException ();
				case PreambleEndRecord:
					preambleEnd = true;
					break;
				default:
					throw new ArgumentException (String.Format ("Unexpected record type {0:X2}", b));
				}
			}
			s.WriteByte (PreambleAckRecord);
		}
	}
}
