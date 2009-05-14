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
using System.Collections.Generic;
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

			frame = new TcpBinaryFrameManager (TcpBinaryFrameManager.DuplexMode, s) { Encoder = this.Encoder };
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
			frame.WriteSizedMessage (message);

			// FIXME: should EndRecord be sent here?
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

			return frame.ReadSizedMessage ();
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
				frame = new TcpBinaryFrameManager (TcpBinaryFrameManager.DuplexMode, ns) {
					Encoder = this.Encoder,
					Via = RemoteAddress.Uri };
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
	class TcpBinaryFrameManager
	{
		class MyBinaryReader : BinaryReader
		{
			public MyBinaryReader (Stream s)
				: base (s)
			{
			}

			public int ReadVariableInt ()
			{
				return Read7BitEncodedInt ();
			}
		}

		class MyBinaryWriter : BinaryWriter
		{
			public MyBinaryWriter (Stream s)
				: base (s)
			{
			}

			public void WriteVariableInt (int value)
			{
				Write7BitEncodedInt (value);
			}
		}

		class MyXmlBinaryWriterSession : XmlBinaryWriterSession
		{
			public override bool TryAdd (XmlDictionaryString value, out int key)
			{
				if (!base.TryAdd (value, out key))
					return false;
				List.Add (value);
				return true;
			}

			public List<XmlDictionaryString> List = new List<XmlDictionaryString> ();
		}

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
		MyBinaryReader reader;
		MyBinaryWriter writer;

		public TcpBinaryFrameManager (int mode, Stream s)
		{
			this.mode = mode;
			this.s = s;
			reader = new MyBinaryReader (s);
			writer = new MyBinaryWriter (s);

			EncodingRecord = 8; // FIXME: it should depend on mode.
		}

		Stream s;

		int mode;

		public byte EncodingRecord { get; set; }

		public Uri Via { get; set; }

		public MessageEncoder Encoder { get; set; }

		public byte [] ReadSizedChunk ()
		{
			int length = reader.ReadVariableInt ();
			
			if (length > 65536)
				throw new InvalidOperationException ("The message is too large.");

			byte [] buffer = new byte [length];
			reader.Read (buffer, 0, length);
			
			return buffer;
		}

		public void WriteSizedChunk (byte [] data)
		{
			writer.WriteVariableInt (data.Length);
			writer.Write (data, 0, data.Length);
		}

		public void ProcessPreambleInitiator ()
		{
			s.WriteByte (VersionRecord);
			s.WriteByte (1);
			s.WriteByte (0);
			s.WriteByte (ModeRecord);
			s.WriteByte ((byte) mode);
			s.WriteByte (ViaRecord);
			writer.Write (Via.ToString ());
			s.WriteByte (KnownEncodingRecord); // FIXME
			s.WriteByte ((byte) EncodingRecord);
			s.WriteByte (PreambleEndRecord);
			s.Flush ();

			int b = s.ReadByte ();
			switch (b) {
			case PreambleAckRecord:
				return; // success
			case FaultRecord:
				throw new FaultException (reader.ReadString ());
			default:
				throw new ArgumentException (String.Format ("Preamble Ack Record is expected, got {0:X}", b));
			}
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
					Via = new Uri (reader.ReadString ());
					break;
				case KnownEncodingRecord:
					EncodingRecord = (byte) s.ReadByte ();
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

		public Message ReadSizedMessage ()
		{
			// FIXME: implement [MC-NMF] correctly. Currently it is a guessed protocol hack.
			var packetType = s.ReadByte (); // 6
			if (packetType != SizedEnvelopeRecord)
				throw new NotImplementedException (String.Format ("Packet type {0:X} is not implemented", packetType));

			byte [] buffer = ReadSizedChunk ();

			var ms = new MemoryStream (buffer, 0, buffer.Length);

			// FIXME: turned out that it could be either in-band dictionary ([MC-NBFSE]), or a mere xml body ([MC-NBFS]).
			if (EncodingRecord != 8)
				throw new NotImplementedException (String.Format ("Message encoding {0:X} is not implemented yet", EncodingRecord));

			// Encoding type 8:
			// the returned buffer consists of a serialized reader 
			// session and the binary xml body. 

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
				benc.CurrentReaderSession = session;
			// FIXME: supply maxSizeOfHeaders.
			Message msg = Encoder.ReadMessage (ms, 0x10000);
			if (benc != null)
				benc.CurrentReaderSession = null;
			s.Flush ();

			return msg;
		}

		public void WriteSizedMessage (Message message)
		{
			// FIXME: implement full [MC-NMF] protocol.

			if (EncodingRecord != 8)
				throw new NotImplementedException (String.Format ("Message encoding {0:X} is not implemented yet", EncodingRecord));

			s.WriteByte (SizedEnvelopeRecord);
			MemoryStream ms = new MemoryStream ();
			var session = new MyXmlBinaryWriterSession ();
			// FIXME: it is dummy
			int dummy;
			session.TryAdd (new XmlDictionary ().Add ("urn:foo"), out dummy);
			var benc = Encoder as BinaryMessageEncoder;
			try {
				if (benc != null)
					benc.CurrentWriterSession = session;
				Encoder.WriteMessage (message, ms);
			} finally {
				benc.CurrentWriterSession = null;
			}

			// dictionary
			MemoryStream msd = new MemoryStream ();
			BinaryWriter dw = new BinaryWriter (msd);
			foreach (var ds in session.List)
				dw.Write (ds.Value);
			dw.Flush ();
			WriteSizedChunk (msd.ToArray ());
			// message body
			WriteSizedChunk (ms.ToArray ());

			writer.Write (EndRecord);
			writer.Flush ();
		}
	}
}
