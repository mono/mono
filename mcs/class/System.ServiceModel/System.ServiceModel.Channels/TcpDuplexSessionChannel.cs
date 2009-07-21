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
				owner.DiscardSession ();
			}
		}

		TcpChannelInfo info;
		TcpClient client;
		bool is_service_side;
		EndpointAddress local_address;
		TcpBinaryFrameManager frame;
		TcpDuplexSession session; // do not use this directly. Use Session instead.
		
		public TcpDuplexSessionChannel (ChannelFactoryBase factory, TcpChannelInfo info, EndpointAddress address, Uri via)
			: base (factory, address, via)
		{
			is_service_side = false;
			this.info = info;
		}
		
		public TcpDuplexSessionChannel (ChannelListenerBase listener, TcpChannelInfo info, TcpClient client)
			: base (listener)
		{
			is_service_side = true;
			this.client = client;
			this.info = info;
		}
		
		public MessageEncoder Encoder {
			get { return info.MessageEncoder; }
		}

		public override EndpointAddress LocalAddress {
			get { return local_address; }
		}
		
		public IDuplexSession Session {
			get {
				if (session == null)
					session = new TcpDuplexSession (this);
				return session;
			}
		}

		void DiscardSession ()
		{
			frame.ProcessEndRecordInitiator ();
			session = null;
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
				DateTime start = DateTime.Now;
				message = Receive (timeout);
				if (message != null)
					return true;
				// received EndRecord, so close the session and return false instead.
				// (Closing channel here might not be a good idea, but right now I have no better way.)
				Close (timeout - (DateTime.Now - start));
				return false;
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
			Close (TimeSpan.FromTicks (1));
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
			if (!is_service_side)
				if (session != null)
					session.Close (timeout);

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
				frame.ProcessPreambleInitiator ();
				frame.ProcessPreambleAckInitiator ();
			} else {
				// server side
				Stream s = client.GetStream ();

				frame = new TcpBinaryFrameManager (TcpBinaryFrameManager.DuplexMode, s, is_service_side) { Encoder = this.Encoder };

				// FIXME: use retrieved record properties in the request processing.

				frame.ProcessPreambleRecipient ();
				frame.ProcessPreambleAckRecipient ();
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

		public const byte UnsizedMessageTerminator = 0;
		public const byte SingletonUnsizedMode = 1;
		public const byte DuplexMode = 2;
		public const byte SimplexMode = 3;
		public const byte SingletonSizedMode = 4;

		public const byte EncodingUtf8 = 3;
		public const byte EncodingUtf16 = 4;
		public const byte EncodingUtf16LE = 5;
		public const byte EncodingMtom = 6;
		public const byte EncodingBinary = 7;
		public const byte EncodingBinaryWithDictionary = 8;

		MyBinaryReader reader;
		MyBinaryWriter writer;

		public TcpBinaryFrameManager (int mode, Stream s, bool isServiceSide)
		{
			this.mode = mode;
			this.s = s;
			this.is_service_side = isServiceSide;
			reader = new MyBinaryReader (s);
			ResetWriteBuffer ();

			EncodingRecord = EncodingBinaryWithDictionary; // FIXME: it should depend on mode.
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

		public void WriteSizedChunk (byte [] data, int index, int length)
		{
			writer.WriteVariableInt (length);
			writer.Write (data, index, length);
		}

		public void ProcessPreambleInitiator ()
		{
			ResetWriteBuffer ();

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
			s.Write (buffer.GetBuffer (), 0, (int) buffer.Position);
			s.Flush ();
		}

		public void ProcessPreambleAckInitiator ()
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

		public void ProcessPreambleAckRecipient ()
		{
			s.WriteByte (PreambleAckRecord);
		}

		public void ProcessPreambleRecipient ()
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
					throw new NotImplementedException ("ExtendingEncodingRecord");
				case UpgradeRequestRecord:
					throw new NotImplementedException ("UpgradeRequetRecord");
				case UpgradeResponseRecord:
					throw new NotImplementedException ("UpgradeResponseRecord");
				case PreambleEndRecord:
					preambleEnd = true;
					break;
				default:
					throw new ProtocolException (String.Format ("Unexpected record type {0:X2}", b));
				}
			}
		}

		XmlBinaryReaderSession reader_session;
		int reader_session_items;

		public Message ReadSizedMessage ()
		{
			// FIXME: implement full [MC-NMF].

			var packetType = s.ReadByte ();
			if (packetType == EndRecord)
				return null;
			if (packetType != SizedEnvelopeRecord)
				throw new NotImplementedException (String.Format ("Packet type {0:X} is not implemented", packetType));

			byte [] buffer = ReadSizedChunk ();

			var ms = new MemoryStream (buffer, 0, buffer.Length);

			// FIXME: turned out that it could be either in-band dictionary ([MC-NBFSE]), or a mere xml body ([MC-NBFS]).
			if (EncodingRecord != EncodingBinaryWithDictionary)
				throw new NotImplementedException (String.Format ("Message encoding {0:X} is not implemented yet", EncodingRecord));

			// Encoding type 8:
			// the returned buffer consists of a serialized reader 
			// session and the binary xml body. 

			var session = reader_session ?? new XmlBinaryReaderSession ();
			reader_session = session;
			byte [] rsbuf = new TcpBinaryFrameManager (0, ms, is_service_side).ReadSizedChunk ();
			using (var rms = new MemoryStream (rsbuf, 0, rsbuf.Length)) {
				var rbr = new BinaryReader (rms, Encoding.UTF8);
				while (rms.Position < rms.Length)
					session.Add (reader_session_items++, rbr.ReadString ());
			}
			var benc = Encoder as BinaryMessageEncoder;
			if (benc != null)
				benc.CurrentReaderSession = session;

			// FIXME: supply maxSizeOfHeaders.
			Message msg = Encoder.ReadMessage (ms, 0x10000);
			if (benc != null)
				benc.CurrentReaderSession = null;

			return msg;
		}

		// FIXME: support timeout
		public Message ReadUnsizedMessage (TimeSpan timeout)
		{
			var packetType = s.ReadByte ();

			if (packetType == EndRecord)
				return null;
			if (packetType != UnsizedEnvelopeRecord)
				throw new NotImplementedException (String.Format ("Packet type {0:X} is not implemented", packetType));

			// Encoding type 7 is expected
			if (EncodingRecord != EncodingBinary)
				throw new NotImplementedException (String.Format ("Message encoding {0:X} is not implemented yet", EncodingRecord));

			byte [] buffer = ReadSizedChunk ();
			var ms = new MemoryStream (buffer, 0, buffer.Length);

			// FIXME: supply maxSizeOfHeaders.
			Message msg = Encoder.ReadMessage (ms, 0x10000);

			return msg;
		}

		public void ReadUnsizedMessageTerminator (TimeSpan timeout)
		{
			var terminator = s.ReadByte ();
			if (terminator != UnsizedMessageTerminator)
				throw new InvalidOperationException (String.Format ("Unsized message terminator is expected. Got '{0}' (&#x{1:X};).", (char) terminator, terminator));
		}

		byte [] eof_buffer = new byte [1];
		MyXmlBinaryWriterSession writer_session;

		public void WriteSizedMessage (Message message)
		{
			ResetWriteBuffer ();

			if (EncodingRecord != 8)
				throw new NotImplementedException (String.Format ("Message encoding {0:X} is not implemented yet", EncodingRecord));

			buffer.WriteByte (SizedEnvelopeRecord);

			MemoryStream ms = new MemoryStream ();
			var session = writer_session ?? new MyXmlBinaryWriterSession ();
			writer_session = session;
			int writer_session_count = session.List.Count;
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
			for (int i = writer_session_count; i < session.List.Count; i++)
				dw.Write (session.List [i].Value);
			dw.Flush ();

			int length = (int) (msd.Position + ms.Position);
			var msda = msd.ToArray ();
			int sizeOfLength = writer.GetSizeOfLength (msda.Length);

			writer.WriteVariableInt (length + sizeOfLength); // dictionary array also involves the size of itself.
			WriteSizedChunk (msda, 0, msda.Length);
			// message body
			var arr = ms.GetBuffer ();
			writer.Write (arr, 0, (int) ms.Position);

			writer.Flush ();

			s.Write (buffer.GetBuffer (), 0, (int) buffer.Position);
			s.Flush ();
		}

		// FIXME: support timeout
		public void WriteUnsizedMessage (Message message, TimeSpan timeout)
		{
			ResetWriteBuffer ();

			if (EncodingRecord != EncodingBinary)
				throw new NotImplementedException (String.Format ("Message encoding {0:X} is not implemented yet", EncodingRecord));

			s.WriteByte (UnsizedEnvelopeRecord);
			s.Flush ();

			Encoder.WriteMessage (message, buffer);
			new MyBinaryWriter (s).WriteVariableInt ((int) buffer.Position);
			s.Write (buffer.GetBuffer (), 0, (int) buffer.Position);
			s.Flush ();
		}

		// FIXME: handle timeout
		public void WriteUnsizedMessageTerminator (TimeSpan timeout)
		{
			s.WriteByte (UnsizedMessageTerminator); // terminator
			s.Flush ();
		}

		public void ProcessEndRecordInitiator ()
		{
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
