// 
// TcpBinaryFrameManager.cs
// 
// Author: 
//	Atsushi Enomoto  <atsushi@ximian.com>
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

		static readonly byte [] empty_bytes = new byte [0];

		public byte [] ReadSizedChunk ()
		{
			lock (read_lock) {

			int length = reader.ReadVariableInt ();
			if (length == 0)
				return empty_bytes;

			if (length > 65536)
				throw new InvalidOperationException ("The message is too large.");

			byte [] buffer = new byte [length];
			for (int readSize = 0; readSize < length; )
				readSize += reader.Read (buffer, readSize, length - readSize);
			return buffer;

			}
		}

		void WriteSizedChunk (byte [] data, int index, int length)
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

		public bool ProcessPreambleRecipient ()
		{
			return ProcessPreambleRecipient (-1);
		}
		bool ProcessPreambleRecipient (int initialByte)
		{
			bool preambleEnd = false;
			while (!preambleEnd) {
				int b = initialByte < 0 ? s.ReadByte () : initialByte;
				if (b < 0)
					return false;
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
			return true;
		}

		XmlBinaryReaderSession reader_session;
		int reader_session_items;

		object read_lock = new object ();
		object write_lock = new object ();

		public Message ReadSizedMessage ()
		{
			lock (read_lock) {

			// FIXME: implement full [MC-NMF].

			int packetType;
			try {
				packetType = s.ReadByte ();
			} catch (IOException) {
				// it is already disconnected
				return null;
			} catch (SocketException) {
				// it is already disconnected
				return null;
			}
			// FIXME: .NET never results in -1, so there may be implementation mismatch in Socket (but might be in other places)
			if (packetType == -1)
				return null;
			// FIXME: The client should wait for EndRecord, but if we try to send it, the socket blocks and becomes unable to work anymore.
			if (packetType == EndRecord)
				return null;
			if (packetType != SizedEnvelopeRecord) {
				if (is_service_side) {
					// reconnect
					ProcessPreambleRecipient (packetType);
					ProcessPreambleAckRecipient ();
				}
				else
					throw new NotImplementedException (String.Format ("Packet type {0:X} is not implemented", packetType));
			}

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
		}

		// FIXME: support timeout
		public Message ReadUnsizedMessage (TimeSpan timeout)
		{
			lock (read_lock) {

			// Encoding type 7 is expected
			if (EncodingRecord != EncodingBinary)
				throw new NotImplementedException (String.Format ("Message encoding {0:X} is not implemented yet", EncodingRecord));

			var packetType = s.ReadByte ();

			if (packetType == EndRecord)
				return null;
			if (packetType != UnsizedEnvelopeRecord)
				throw new NotImplementedException (String.Format ("Packet type {0:X} is not implemented", packetType));

			var ms = new MemoryStream ();
			while (true) {
				byte [] buffer = ReadSizedChunk ();
				if (buffer.Length == 0) // i.e. it is UnsizedMessageTerminator (which is '0')
					break;
				ms.Write (buffer, 0, buffer.Length);
			}
			ms.Seek (0, SeekOrigin.Begin);

			// FIXME: supply correct maxSizeOfHeaders.
			Message msg = Encoder.ReadMessage (ms, (int) ms.Length);

			return msg;
			
			}
		}

		byte [] eof_buffer = new byte [1];
		MyXmlBinaryWriterSession writer_session;

		public void WriteSizedMessage (Message message)
		{
			lock (write_lock) {

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
		}

		// FIXME: support timeout
		public void WriteUnsizedMessage (Message message, TimeSpan timeout)
		{
			lock (write_lock) {

			ResetWriteBuffer ();

			if (EncodingRecord != EncodingBinary)
				throw new NotImplementedException (String.Format ("Message encoding {0:X} is not implemented yet", EncodingRecord));

			s.WriteByte (UnsizedEnvelopeRecord);
			s.Flush ();

			Encoder.WriteMessage (message, buffer);
			new MyBinaryWriter (s).WriteVariableInt ((int) buffer.Position);
			s.Write (buffer.GetBuffer (), 0, (int) buffer.Position);

			s.WriteByte (UnsizedMessageTerminator); // terminator
			s.Flush ();

			}
		}

		public void WriteEndRecord ()
		{
			lock (write_lock) {

			s.WriteByte (EndRecord); // it is required
			s.Flush ();

			}
		}

		public void ReadEndRecord ()
		{
			lock (read_lock) {

			int b;
			if ((b = s.ReadByte ()) != EndRecord)
				throw new ProtocolException (String.Format ("EndRecord message was expected, got {0:X}", b));

			}
		}
	}
}
