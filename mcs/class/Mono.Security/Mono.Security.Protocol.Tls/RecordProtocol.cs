// Transport Security Layer (TLS)
// Copyright (c) 2003-2004 Carlos Guzman Alvarez
// Copyright (C) 2006-2007 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.IO;
using System.Threading;

using Mono.Security.Protocol.Tls.Handshake;

namespace Mono.Security.Protocol.Tls
{
	internal abstract class RecordProtocol
	{
		#region Fields

		private static ManualResetEvent record_processing = new ManualResetEvent (true);

		protected Stream	innerStream;
		protected Context	context;

		#endregion

		#region Properties

		public Context Context
		{
			get { return this.context; }
			set { this.context = value; }
		}

		#endregion

		#region Constructors

		public RecordProtocol(Stream innerStream, Context context)
		{
			this.innerStream			= innerStream;
			this.context				= context;
			this.context.RecordProtocol = this;
		}

		#endregion

		#region Abstract Methods

		public virtual void SendRecord(HandshakeType type)
		{

			IAsyncResult ar = this.BeginSendRecord(type, null, null);

			this.EndSendRecord(ar);

		}

		protected abstract void ProcessHandshakeMessage(TlsStream handMsg);

		protected virtual void ProcessChangeCipherSpec ()
		{
			Context ctx = this.Context;

			// Reset sequence numbers
			ctx.ReadSequenceNumber = 0;

			if (ctx is ClientContext) {
				ctx.EndSwitchingSecurityParameters (true);
			} else {
				ctx.StartSwitchingSecurityParameters (false);
			}
		}

		public virtual HandshakeMessage GetMessage(HandshakeType type)
		{
			throw new NotSupportedException();
		}

		#endregion

		#region Receive Record Async Result
		private class ReceiveRecordAsyncResult : IAsyncResult
		{
			private object locker = new object ();
			private AsyncCallback _userCallback;
			private object _userState;
			private Exception _asyncException;
			private ManualResetEvent handle;
			private byte[] _resultingBuffer;
			private Stream _record;
			private bool completed;

			private byte[] _initialBuffer;

			public ReceiveRecordAsyncResult(AsyncCallback userCallback, object userState, byte[] initialBuffer, Stream record)
			{
				_userCallback = userCallback;
				_userState = userState;
				_initialBuffer = initialBuffer;
				_record = record;
			}

			public Stream Record
			{
				get { return _record; }
			}

			public byte[] ResultingBuffer
			{
				get { return _resultingBuffer; }
			}

			public byte[] InitialBuffer
			{
				get { return _initialBuffer; }
			}

			public object AsyncState
			{
				get { return _userState; }
			}

			public Exception AsyncException
			{
				get { return _asyncException; }
			}

			public bool CompletedWithError
			{
				get {
					if (!IsCompleted)
						return false; // Perhaps throw InvalidOperationExcetion?

					return null != _asyncException;
				}
			}

			public WaitHandle AsyncWaitHandle
			{
				get {
					lock (locker) {
						if (handle == null)
							handle = new ManualResetEvent (completed);
					}
					return handle;
				}
				
			}

			public bool CompletedSynchronously
			{
				get { return false; }
			}

			public bool IsCompleted
			{
				get {
					lock (locker) {
						return completed;
					}
				}
			}

			private void SetComplete(Exception ex, byte[] resultingBuffer)
			{
				lock (locker) {
					if (completed)
						return;

					completed = true;
					_asyncException = ex;
					_resultingBuffer = resultingBuffer;
					if (handle != null)
						handle.Set ();

					if (_userCallback != null)
						_userCallback.BeginInvoke (this, null, null);
				}
			}

			public void SetComplete(Exception ex)
			{
				SetComplete(ex, null);
			}

			public void SetComplete(byte[] resultingBuffer)
			{
				SetComplete(null, resultingBuffer);
			}

			public void SetComplete()
			{
				SetComplete(null, null);
			}
		}
		#endregion

		#region Receive Record Async Result
		private class SendRecordAsyncResult : IAsyncResult
		{
			private object locker = new object ();
			private AsyncCallback _userCallback;
			private object _userState;
			private Exception _asyncException;
			private ManualResetEvent handle;
			private HandshakeMessage _message;
			private bool completed;

			public SendRecordAsyncResult(AsyncCallback userCallback, object userState, HandshakeMessage message)
			{
				_userCallback = userCallback;
				_userState = userState;
				_message = message;
			}

			public HandshakeMessage Message
			{
				get { return _message; }
			}

			public object AsyncState
			{
				get { return _userState; }
			}

			public Exception AsyncException
			{
				get { return _asyncException; }
			}

			public bool CompletedWithError
			{
				get {
					if (!IsCompleted)
						return false; // Perhaps throw InvalidOperationExcetion?

					return null != _asyncException;
				}
			}

			public WaitHandle AsyncWaitHandle
			{
				get {
					lock (locker) {
						if (handle == null)
							handle = new ManualResetEvent (completed);
					}
					return handle;
				}
				
			}

			public bool CompletedSynchronously
			{
				get { return false; }
			}

			public bool IsCompleted
			{
				get {
					lock (locker) {
						return completed;
					}
				}
			}

			public void SetComplete(Exception ex)
			{
				lock (locker) {
					if (completed)
						return;

					completed = true;
					if (handle != null)
						handle.Set ();

					if (_userCallback != null)
						_userCallback.BeginInvoke (this, null, null);

					_asyncException = ex;
				}
			}

			public void SetComplete()
			{
				SetComplete(null);
			}
		}
		#endregion

		#region Reveive Record Methods

		public IAsyncResult BeginReceiveRecord(Stream record, AsyncCallback callback, object state)
		{
			if (this.context.ReceivedConnectionEnd)
			{
				throw new TlsException(
					AlertDescription.InternalError,
					"The session is finished and it's no longer valid.");
			}

			record_processing.Reset ();
			byte[] recordTypeBuffer = new byte[1];

			ReceiveRecordAsyncResult internalResult = new ReceiveRecordAsyncResult(callback, state, recordTypeBuffer, record);

			record.BeginRead(internalResult.InitialBuffer, 0, internalResult.InitialBuffer.Length, new AsyncCallback(InternalReceiveRecordCallback), internalResult);

			return internalResult;
		}

		private void InternalReceiveRecordCallback(IAsyncResult asyncResult)
		{
			ReceiveRecordAsyncResult internalResult = asyncResult.AsyncState as ReceiveRecordAsyncResult;
			Stream record = internalResult.Record;

			try
			{
				
				int bytesRead = internalResult.Record.EndRead(asyncResult);

				//We're at the end of the stream. Time to bail.
				if (bytesRead == 0)
				{
					internalResult.SetComplete((byte[])null);
					return;
				}

				// Try to read the Record Content Type
				int type = internalResult.InitialBuffer[0];

				// Set last handshake message received to None
				this.context.LastHandshakeMsg = HandshakeType.ClientHello;

				ContentType	contentType	= (ContentType)type;
				byte[] buffer = this.ReadRecordBuffer(type, record);
				if (buffer == null)
				{
					// record incomplete (at the moment)
					internalResult.SetComplete((byte[])null);
					return;
				}

				// Decrypt message contents if needed
				if (contentType == ContentType.Alert && buffer.Length == 2)
				{
				}
				else if ((this.Context.Read != null) && (this.Context.Read.Cipher != null))
				{
					buffer = this.decryptRecordFragment (contentType, buffer);
					DebugHelper.WriteLine ("Decrypted record data", buffer);
				}

				// Process record
				switch (contentType)
				{
					case ContentType.Alert:
						this.ProcessAlert((AlertLevel)buffer [0], (AlertDescription)buffer [1]);
						if (record.CanSeek) 
						{
							// don't reprocess that memory block
							record.SetLength (0); 
						}
						buffer = null;
						break;

					case ContentType.ChangeCipherSpec:
						this.ProcessChangeCipherSpec();
						break;

					case ContentType.ApplicationData:
						break;

					case ContentType.Handshake:
						TlsStream message = new TlsStream (buffer);
						while (!message.EOF)
						{
							this.ProcessHandshakeMessage(message);
						}
						break;

					case (ContentType)0x80:
						this.context.HandshakeMessages.Write (buffer);
						break;

					default:
						throw new TlsException(
							AlertDescription.UnexpectedMessage,
							"Unknown record received from server.");
				}

				internalResult.SetComplete(buffer);
			}
			catch (Exception ex)
			{
				internalResult.SetComplete(ex);
			}

		}

		public byte[] EndReceiveRecord(IAsyncResult asyncResult)
		{
			ReceiveRecordAsyncResult internalResult = asyncResult as ReceiveRecordAsyncResult;

			if (null == internalResult)
				throw new ArgumentException("Either the provided async result is null or was not created by this RecordProtocol.");

			if (!internalResult.IsCompleted)
				internalResult.AsyncWaitHandle.WaitOne();

			if (internalResult.CompletedWithError)
				throw internalResult.AsyncException;

			byte[] result = internalResult.ResultingBuffer;
			record_processing.Set ();
			return result;
		}

		public byte[] ReceiveRecord(Stream record)
		{

			IAsyncResult ar = this.BeginReceiveRecord(record, null, null);
			return this.EndReceiveRecord(ar);

		}

		private byte[] ReadRecordBuffer (int contentType, Stream record)
		{
			switch (contentType)
			{
				case 0x80:
					return this.ReadClientHelloV2(record);

				default:
					if (!Enum.IsDefined(typeof(ContentType), (ContentType)contentType))
					{
						throw new TlsException(AlertDescription.DecodeError);
					}
					return this.ReadStandardRecordBuffer(record);
			}
		}

		private byte[] ReadClientHelloV2 (Stream record)
		{
			int msgLength = record.ReadByte ();
			// process further only if the whole record is available
			if (record.CanSeek && (msgLength + 1 > record.Length)) 
			{
				return null;
			}

			byte[] message = new byte[msgLength];
			record.Read (message, 0, msgLength);

			int msgType		= message [0];
			if (msgType != 1)
			{
				throw new TlsException(AlertDescription.DecodeError);
			}
			int protocol = (message [1] << 8 | message [2]);
			int cipherSpecLength = (message [3] << 8 | message [4]);
			int sessionIdLength = (message [5] << 8 | message [6]);
			int challengeLength = (message [7] << 8 | message [8]);
			int length = (challengeLength > 32) ? 32 : challengeLength;

			// Read CipherSpecs
			byte[] cipherSpecV2 = new byte[cipherSpecLength];
			Buffer.BlockCopy (message, 9, cipherSpecV2, 0, cipherSpecLength);

			// Read session ID
			byte[] sessionId = new byte[sessionIdLength];
			Buffer.BlockCopy (message, 9 + cipherSpecLength, sessionId, 0, sessionIdLength);

			// Read challenge ID
			byte[] challenge = new byte[challengeLength];
			Buffer.BlockCopy (message, 9 + cipherSpecLength + sessionIdLength, challenge, 0, challengeLength);
		
			if (challengeLength < 16 || cipherSpecLength == 0 || (cipherSpecLength % 3) != 0)
			{
				throw new TlsException(AlertDescription.DecodeError);
			}

			// Updated the Session ID
			if (sessionId.Length > 0)
			{
				this.context.SessionId = sessionId;
			}

			// Update the protocol version
			this.Context.ChangeProtocol((short)protocol);

			// Select the Cipher suite
			this.ProcessCipherSpecV2Buffer(this.Context.SecurityProtocol, cipherSpecV2);

			// Updated the Client Random
			this.context.ClientRandom = new byte [32]; // Always 32
			// 1. if challenge is bigger than 32 bytes only use the last 32 bytes
			// 2. right justify (0) challenge in ClientRandom if less than 32
			Buffer.BlockCopy (challenge, challenge.Length - length, this.context.ClientRandom, 32 - length, length);

			// Set 
			this.context.LastHandshakeMsg = HandshakeType.ClientHello;
			this.context.ProtocolNegotiated = true;

			return message;
		}

		private byte[] ReadStandardRecordBuffer (Stream record)
		{
			byte[] header = new byte[4];
			if (record.Read (header, 0, 4) != 4)
				throw new TlsException ("buffer underrun");
			
			short protocol = (short)((header [0] << 8) | header [1]);
			short length = (short)((header [2] << 8) | header [3]);

			// process further only if the whole record is available
			// note: the first 5 bytes aren't part of the length
			if (record.CanSeek && (length + 5 > record.Length)) 
			{
				return null;
			}
			
			// Read Record data
			int	totalReceived = 0;
			byte[]	buffer		= new byte[length];
			while (totalReceived != length)
			{
				int justReceived = record.Read(buffer, totalReceived, buffer.Length - totalReceived);

				//Make sure we get some data so we don't end up in an infinite loop here before shutdown.
				if (0 == justReceived)
				{
					throw new TlsException(AlertDescription.CloseNotify, "Received 0 bytes from stream. It must be closed.");
				}

				totalReceived += justReceived;
			}

			// Check that the message has a valid protocol version
			if (protocol != this.context.Protocol && this.context.ProtocolNegotiated)
			{
				throw new TlsException(
					AlertDescription.ProtocolVersion, "Invalid protocol version on message received");
			}

			DebugHelper.WriteLine("Record data", buffer);

			return buffer;
		}

		private void ProcessAlert(AlertLevel alertLevel, AlertDescription alertDesc)
		{
			switch (alertLevel)
			{
				case AlertLevel.Fatal:
					throw new TlsException(alertLevel, alertDesc);

				case AlertLevel.Warning:
				default:
				switch (alertDesc)
				{
					case AlertDescription.CloseNotify:
						this.context.ReceivedConnectionEnd = true;
						break;
				}
				break;
			}
		}

		#endregion

		#region Send Alert Methods

		public void SendAlert(AlertDescription description)
		{
			this.SendAlert(new Alert(description));
		}

		public void SendAlert(
			AlertLevel			level, 
			AlertDescription	description)
		{
			this.SendAlert(new Alert(level, description));
		}

		public void SendAlert(Alert alert)
		{
			AlertLevel level;
			AlertDescription description;
			bool close;

			if (alert == null) {
				DebugHelper.WriteLine(">>>> Write Alert NULL");
				level = AlertLevel.Fatal;
				description = AlertDescription.InternalError;
				close = true;
			} else {
				DebugHelper.WriteLine(">>>> Write Alert ({0}|{1})", alert.Description, alert.Message);
				level = alert.Level;
				description = alert.Description;
				close = alert.IsCloseNotify;
			}

			// Write record
			this.SendRecord (ContentType.Alert, new byte[2] { (byte) level, (byte) description });

			if (close) {
				this.context.SentConnectionEnd = true;
			}
		}

		#endregion

		#region Send Record Methods

		public void SendChangeCipherSpec()
		{
			DebugHelper.WriteLine(">>>> Write Change Cipher Spec");

			// Send Change Cipher Spec message with the current cipher
			// or as plain text if this is the initial negotiation
			this.SendRecord(ContentType.ChangeCipherSpec, new byte[] {1});

			Context ctx = this.context;

			// Reset sequence numbers
			ctx.WriteSequenceNumber = 0;

			// all further data sent will be encrypted with the negotiated
			// security parameters (now the current parameters)
			if (ctx is ClientContext) {
				ctx.StartSwitchingSecurityParameters (true);
			} else {
				ctx.EndSwitchingSecurityParameters (false);
			}
		}

		public IAsyncResult BeginSendRecord(HandshakeType handshakeType, AsyncCallback callback, object state)
		{
			HandshakeMessage msg = this.GetMessage(handshakeType);

			msg.Process();

			DebugHelper.WriteLine(">>>> Write handshake record ({0}|{1})", context.Protocol, msg.ContentType);

			SendRecordAsyncResult internalResult = new SendRecordAsyncResult(callback, state, msg);

			this.BeginSendRecord(msg.ContentType, msg.EncodeMessage(), new AsyncCallback(InternalSendRecordCallback), internalResult);

			return internalResult;
		}

		private void InternalSendRecordCallback(IAsyncResult ar)
		{
			SendRecordAsyncResult internalResult = ar.AsyncState as SendRecordAsyncResult;
			
			try
			{
				this.EndSendRecord(ar);

				// Update session
				internalResult.Message.Update();

				// Reset message contents
				internalResult.Message.Reset();

				internalResult.SetComplete();
			}
			catch (Exception ex)
			{
				internalResult.SetComplete(ex);
			}
		}

		public IAsyncResult BeginSendRecord(ContentType contentType, byte[] recordData, AsyncCallback callback, object state)
		{
			if (this.context.SentConnectionEnd)
			{
				throw new TlsException(
					AlertDescription.InternalError,
					"The session is finished and it's no longer valid.");
			}

			byte[] record = this.EncodeRecord(contentType, recordData);

			return this.innerStream.BeginWrite(record, 0, record.Length, callback, state);
		}

		public void EndSendRecord(IAsyncResult asyncResult)
		{
			if (asyncResult is SendRecordAsyncResult)
			{
				SendRecordAsyncResult internalResult = asyncResult as SendRecordAsyncResult;
				if (!internalResult.IsCompleted)
					internalResult.AsyncWaitHandle.WaitOne();
				if (internalResult.CompletedWithError)
					throw internalResult.AsyncException;
			}
			else
			{
				this.innerStream.EndWrite(asyncResult);
			}
		}

		public void SendRecord(ContentType contentType, byte[] recordData)
		{
			IAsyncResult ar = this.BeginSendRecord(contentType, recordData, null, null);

			this.EndSendRecord(ar);
		}

		public byte[] EncodeRecord(ContentType contentType, byte[] recordData)
		{
			return this.EncodeRecord(
				contentType,
				recordData,
				0,
				recordData.Length);
		}

		public byte[] EncodeRecord(
			ContentType	contentType, 
			byte[]		recordData,
			int			offset,
			int			count)
		{
			if (this.context.SentConnectionEnd)
			{
				throw new TlsException(
					AlertDescription.InternalError,
					"The session is finished and it's no longer valid.");
			}

			TlsStream record = new TlsStream();

			int	position = offset;

			while (position < ( offset + count ))
			{
				short	fragmentLength = 0;
				byte[]	fragment;

				if ((count + offset - position) > Context.MAX_FRAGMENT_SIZE)
				{
					fragmentLength = Context.MAX_FRAGMENT_SIZE;
				}
				else
				{
					fragmentLength = (short)(count + offset - position);
				}

				// Fill the fragment data
				fragment = new byte[fragmentLength];
				Buffer.BlockCopy(recordData, position, fragment, 0, fragmentLength);

				if ((this.Context.Write != null) && (this.Context.Write.Cipher != null))
				{
					// Encrypt fragment
					fragment = this.encryptRecordFragment (contentType, fragment);
				}

				// Write tls message
				record.Write((byte)contentType);
				record.Write(this.context.Protocol);
				record.Write((short)fragment.Length);
				record.Write(fragment);

				DebugHelper.WriteLine("Record data", fragment);

				// Update buffer position
				position += fragmentLength;
			}

			return record.ToArray();
		}
		
		#endregion

		#region Cryptography Methods

		private byte[] encryptRecordFragment(
			ContentType	contentType, 
			byte[]		fragment)
		{
			byte[] mac	= null;

			// Calculate message MAC
			if (this.Context is ClientContext)
			{
				mac = this.context.Write.Cipher.ComputeClientRecordMAC(contentType, fragment);
			}	
			else
			{
				mac = this.context.Write.Cipher.ComputeServerRecordMAC (contentType, fragment);
			}

			DebugHelper.WriteLine(">>>> Record MAC", mac);

			// Encrypt the message
			byte[] ecr = this.context.Write.Cipher.EncryptRecord (fragment, mac);

			// Update sequence number
			this.context.WriteSequenceNumber++;

			return ecr;
		}

		private byte[] decryptRecordFragment(
			ContentType	contentType, 
			byte[]		fragment)
		{
			byte[]	dcrFragment		= null;
			byte[]	dcrMAC			= null;

			try
			{
				this.context.Read.Cipher.DecryptRecord (fragment, out dcrFragment, out dcrMAC);
			}
			catch
			{
				if (this.context is ServerContext)
				{
					this.Context.RecordProtocol.SendAlert(AlertDescription.DecryptionFailed);
				}

				throw;
			}
			
			// Generate record MAC
			byte[] mac = null;

			if (this.Context is ClientContext)
			{
				mac = this.context.Read.Cipher.ComputeServerRecordMAC(contentType, dcrFragment);
			}
			else
			{
				mac = this.context.Read.Cipher.ComputeClientRecordMAC (contentType, dcrFragment);
			}

			DebugHelper.WriteLine(">>>> Record MAC", mac);

			// Check record MAC
			if (!Compare (mac, dcrMAC))
			{
				throw new TlsException(AlertDescription.BadRecordMAC, "Bad record MAC");
			}

			// Update sequence number
			this.context.ReadSequenceNumber++;

			return dcrFragment;
		}

		private bool Compare (byte[] array1, byte[] array2)
		{
			if (array1 == null)
				return (array2 == null);
			if (array2 == null)
				return false;
			if (array1.Length != array2.Length)
				return false;
			for (int i = 0; i < array1.Length; i++) {
				if (array1[i] != array2[i])
					return false;
			}
			return true;
		}

		#endregion

		#region CipherSpecV2 processing

		private void ProcessCipherSpecV2Buffer (SecurityProtocolType protocol, byte[] buffer)
		{
			TlsStream codes = new TlsStream(buffer);

			string prefix = (protocol == SecurityProtocolType.Ssl3) ? "SSL_" : "TLS_";

			while (codes.Position < codes.Length)
			{
				byte check = codes.ReadByte();

				if (check == 0)
				{
					// SSL/TLS cipher spec
					short code = codes.ReadInt16();	
					int index = this.Context.SupportedCiphers.IndexOf(code);
					if (index != -1)
					{
						this.Context.Negotiating.Cipher = this.Context.SupportedCiphers[index];
						break;
					}
				}
				else
				{
					byte[] tmp = new byte[2];
					codes.Read(tmp, 0, tmp.Length);

					int tmpCode = ((check & 0xff) << 16) | ((tmp[0] & 0xff) << 8) | (tmp[1] & 0xff);
					CipherSuite cipher = this.MapV2CipherCode(prefix, tmpCode);

					if (cipher != null)
					{
						this.Context.Negotiating.Cipher = cipher;
						break;
					}
				}
			}

			if (this.Context.Negotiating == null)
			{
				throw new TlsException(AlertDescription.InsuficientSecurity, "Insuficient Security");
			}
		}

		private CipherSuite MapV2CipherCode(string prefix, int code)
		{
			try
			{
				switch (code)
				{
					case 65664:
						// TLS_RC4_128_WITH_MD5
						return this.Context.SupportedCiphers[prefix + "RSA_WITH_RC4_128_MD5"];
					
					case 131200:
						// TLS_RC4_128_EXPORT40_WITH_MD5
						return this.Context.SupportedCiphers[prefix + "RSA_EXPORT_WITH_RC4_40_MD5"];
					
					case 196736:
						// TLS_RC2_CBC_128_CBC_WITH_MD5
						return this.Context.SupportedCiphers[prefix + "RSA_EXPORT_WITH_RC2_CBC_40_MD5"];
					
					case 262272:
						// TLS_RC2_CBC_128_CBC_EXPORT40_WITH_MD5
						return this.Context.SupportedCiphers[prefix + "RSA_EXPORT_WITH_RC2_CBC_40_MD5"];
					
					case 327808:
						// TLS_IDEA_128_CBC_WITH_MD5
						return null;
					
					case 393280:
						// TLS_DES_64_CBC_WITH_MD5
						return null;

					case 458944:
						// TLS_DES_192_EDE3_CBC_WITH_MD5
						return null;

					default:
						return null;
				}
			}
			catch
			{
				return null;
			}
		}

		#endregion
	}
}
