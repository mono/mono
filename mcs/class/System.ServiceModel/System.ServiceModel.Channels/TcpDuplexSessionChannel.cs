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
using System.Threading;
using System.Xml;

namespace System.ServiceModel.Channels
{
	internal class TcpDuplexSessionChannel : DuplexChannelBase, IDuplexSessionChannel
	{
		class TcpDuplexSession : DuplexSessionBase
		{
			TcpDuplexSessionChannel owner;

			internal TcpDuplexSession (TcpDuplexSessionChannel owner)
			{
				this.owner = owner;
			}

			public override TimeSpan DefaultCloseTimeout {
				get { return owner.DefaultCloseTimeout; }
			}

			public override void Close (TimeSpan timeout)
			{
				// FIXME: what to do here?
				throw new NotImplementedException ();
			}
		}

		TcpChannelInfo info;
		TcpClient client;
		bool is_service_side;
		EndpointAddress local_address;
		TcpListener tcp_listener;
		TimeSpan timeout;
		TcpBinaryFrameManager frame;
		TcpDuplexSession session;
		
		public TcpDuplexSessionChannel (ChannelFactoryBase factory, TcpChannelInfo info, EndpointAddress address, Uri via)
			: base (factory, address, via)
		{
			is_service_side = false;
			this.info = info;
			session = new TcpDuplexSession (this);
		}
		
		public TcpDuplexSessionChannel (ChannelListenerBase listener, TcpChannelInfo info, TcpListener tcpListener, TimeSpan timeout)
			: base (listener)
		{
			is_service_side = true;
			tcp_listener = tcpListener;
			this.info = info;
			session = new TcpDuplexSession (this);
			this.timeout = timeout;
		}
		
		public MessageEncoder Encoder {
			get { return info.MessageEncoder; }
		}

		public override EndpointAddress LocalAddress {
			get { return local_address; }
		}
		
		public IDuplexSession Session {
			get { return session; }
		}

		public override void Send (Message message)
		{
			Send (message, DefaultSendTimeout);
		}
		
		public override void Send (Message message, TimeSpan timeout)
		{
			if (timeout <= TimeSpan.Zero)
				throw new ArgumentException (String.Format ("Timeout value must be positive value. It was {0}", timeout));

			if (!is_service_side) {
				if (message.Headers.To == null)
					message.Headers.To = RemoteAddress.Uri;
				if (message.Headers.ReplyTo == null)
					message.Headers.ReplyTo = new EndpointAddress (Constants.WsaAnonymousUri);
			} else {
				if (message.Headers.RelatesTo == null)
					message.Headers.RelatesTo = OperationContext.Current.IncomingMessageHeaders.MessageId;
			}

			client.SendTimeout = (int) timeout.TotalMilliseconds;
			frame.WriteSizedMessage (message);
			// FIXME: should EndRecord be sent here?
			//if (is_service_side && client.Available > 0)
			//	frame.ProcessEndRecordRecipient ();
		}
		
		public override Message Receive ()
		{
			return Receive (DefaultReceiveTimeout);
		}
		
		public override Message Receive (TimeSpan timeout)
		{
			if (timeout <= TimeSpan.Zero)
				throw new ArgumentException (String.Format ("Timeout value must be positive value. It was {0}", timeout));
			client.ReceiveTimeout = (int) timeout.TotalMilliseconds;
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
			if (client.Available > 0)
				return true;

			DateTime start = DateTime.Now;
			do {
				Thread.Sleep (50);
				if (client.Available > 0)
					return true;
			} while (DateTime.Now - start < timeout);
			return false;
		}
		
		// CommunicationObject
		
		[MonoTODO]
		protected override void OnAbort ()
		{
			if (client != null)
				client.Close ();
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
			if (client != null)
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
				frame = new TcpBinaryFrameManager (TcpBinaryFrameManager.DuplexMode, ns, is_service_side) {
					Encoder = this.Encoder,
					Via = RemoteAddress.Uri };
			} else {
				// server side
				client = tcp_listener.AcceptTcpClient ();
				Stream s = client.GetStream ();

				frame = new TcpBinaryFrameManager (TcpBinaryFrameManager.DuplexMode, s, is_service_side) { Encoder = this.Encoder };

				// FIXME: use retrieved record properties in the request processing.

			}
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

			public int GetSizeOfLength (int value)
			{
				int x = 0;
				do {
					value /= 0x100;
					x++;
				} while (value != 0);
				return x;
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

		public TcpBinaryFrameManager (int mode, Stream s, bool isServiceSide)
		{
			this.mode = mode;
			this.s = s;
			this.is_service_side = isServiceSide;
			reader = new MyBinaryReader (s);
			ResetWriteBuffer ();

			EncodingRecord = 8; // FIXME: it should depend on mode.
		}

		Stream s;
		MemoryStream buffer;
		bool is_service_side;

		int mode;

		public byte EncodingRecord { get; set; }

		public Uri Via { get; set; }

		public MessageEncoder Encoder { get; set; }

		void ResetWriteBuffer ()
		{
			this.buffer = new MemoryStream ();
			writer = new MyBinaryWriter (buffer);
		}

		public byte [] ReadSizedChunk ()
		{
			int length = reader.ReadVariableInt ();
			
			if (length > 65536)
				throw new InvalidOperationException ("The message is too large.");

			byte [] buffer = new byte [length];
			for (int readSize = 0; readSize < length; )
				readSize += reader.Read (buffer, readSize, length - readSize);
			return buffer;
		}

		public void WriteSizedChunk (byte [] data)
		{
			writer.WriteVariableInt (data.Length);
			writer.Write (data, 0, data.Length);
		}

		void ProcessPreambleInitiator ()
		{
			buffer.WriteByte (VersionRecord);
			buffer.WriteByte (1);
			buffer.WriteByte (0);
			buffer.WriteByte (ModeRecord);
			buffer.WriteByte ((byte) mode);
			buffer.WriteByte (ViaRecord);
			writer.Write (Via.ToString ());
			buffer.WriteByte (KnownEncodingRecord); // FIXME
			buffer.WriteByte ((byte) EncodingRecord);
			buffer.WriteByte (PreambleEndRecord);
			buffer.Flush ();
		}

		void ProcessPreambleAckInitiator ()
		{
			int b = s.ReadByte ();
			switch (b) {
			case PreambleAckRecord:
				return; // success
			case FaultRecord:
				throw new FaultException (reader.ReadString ());
			default:
				throw new ProtocolException (String.Format ("Preamble Ack Record is expected, got {0:X}", b));
			}
		}

		void ProcessPreambleAckRecipient ()
		{
			s.WriteByte (PreambleAckRecord);
		}

		void ProcessPreambleRecipient (bool allowEndRecord)
		{
			bool preambleEnd = false;
			while (!preambleEnd) {
				int b = s.ReadByte ();
				switch (b) {
				case VersionRecord:
					if (s.ReadByte () != 1)
						throw new ProtocolException ("Major version must be 1");
					if (s.ReadByte () != 0)
						throw new ProtocolException ("Minor version must be 0");
					break;
				case ModeRecord:
					if (s.ReadByte () != mode)
						throw new ProtocolException (String.Format ("Duplex mode is expected to be {0:X}", mode));
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
				case EndRecord:
					if (allowEndRecord)
						break;
					goto default;
				default:
					throw new ProtocolException (String.Format ("Unexpected record type {0:X2}", b));
				}
			}
		}

		bool message_already_read_once;

		public Message ReadSizedMessage ()
		{
			// FIXME: implement full [MC-NMF].
			if (is_service_side) {
				ProcessPreambleRecipient (message_already_read_once);
				message_already_read_once = true;
				ProcessPreambleAckRecipient ();
			}

			var packetType = s.ReadByte ();
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
			byte [] rsbuf = new TcpBinaryFrameManager (0, ms, is_service_side).ReadSizedChunk ();
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

			if (!is_service_side)
				if (s.Read (eof_buffer, 0, 1) == 1)
					if (eof_buffer [0] != EndRecord)
						throw new ProtocolException (String.Format ("Expected EndRecord message, got {0:X02}", eof_buffer [0]));

			return msg;
		}

		byte [] eof_buffer = new byte [1];

		public void WriteSizedMessage (Message message)
		{
			ResetWriteBuffer ();

			if (!is_service_side)
				ProcessPreambleInitiator ();

			// FIXME: implement full [MC-NMF] protocol.

			if (EncodingRecord != 8)
				throw new NotImplementedException (String.Format ("Message encoding {0:X} is not implemented yet", EncodingRecord));

			buffer.WriteByte (SizedEnvelopeRecord);

			MemoryStream ms = new MemoryStream ();
			var session = new MyXmlBinaryWriterSession ();
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

			int length = (int) (msd.Position + ms.Position);
			var msda = msd.ToArray ();
			int sizeOfLength = writer.GetSizeOfLength (msda.Length);

			writer.WriteVariableInt (length + sizeOfLength); // dictionary array also involves the size of itself.
			WriteSizedChunk (msda);
			// message body
			var arr = ms.GetBuffer ();
			writer.Write (arr, 0, (int) ms.Position);

			writer.Flush ();

			s.Write (buffer.GetBuffer (), 0, (int) buffer.Position);
			s.Flush ();

			// It is processed at *this* late.
			if (!is_service_side)
				ProcessPreambleAckInitiator ();

			s.WriteByte (EndRecord); // it is required
			s.Flush ();
		}

		public void ProcessEndRecordRecipient ()
		{
			int b;
			if ((b = s.ReadByte ()) != EndRecord)
				throw new ProtocolException (String.Format ("EndRecord message was expected, got {0:X}", b));
		}
	}
}
