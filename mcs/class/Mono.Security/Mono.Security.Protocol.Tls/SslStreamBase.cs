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
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace Mono.Security.Protocol.Tls
{
	public abstract class SslStreamBase: Stream, IDisposable
	{
		private delegate void AsyncHandshakeDelegate(InternalAsyncResult asyncResult, bool fromWrite);
		
		#region Fields

		static ManualResetEvent record_processing = new ManualResetEvent (true);

		private const int WaitTimeOut = 5 * 60 * 1000;

		internal Stream innerStream;
		internal MemoryStream inputBuffer;
		internal Context context;
		internal RecordProtocol protocol;
		internal bool ownsStream;
		private volatile bool disposed;
		private bool checkCertRevocationStatus;
		private object negotiate;
		private object read;
		private object write;
		private ManualResetEvent negotiationComplete;

		#endregion


		#region Constructors

		protected SslStreamBase(
			Stream stream,
			bool ownsStream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream is null.");
			}
			if (!stream.CanRead || !stream.CanWrite)
			{
				throw new ArgumentNullException("stream is not both readable and writable.");
			}

			this.inputBuffer = new MemoryStream();
			this.innerStream = stream;
			this.ownsStream = ownsStream;
			this.negotiate = new object();
			this.read = new object();
			this.write = new object();
			this.negotiationComplete = new ManualResetEvent(false);
		}

		#endregion

		#region Handshakes
		private void AsyncHandshakeCallback(IAsyncResult asyncResult)
		{
			InternalAsyncResult internalResult = asyncResult.AsyncState as InternalAsyncResult;

			try
			{
				try
				{
					this.OnNegotiateHandshakeCallback(asyncResult);
				}
				catch (TlsException ex)
				{
					this.protocol.SendAlert(ex.Alert);

					throw new IOException("The authentication or decryption has failed.", ex);
				}
				catch (Exception ex)
				{
					this.protocol.SendAlert(AlertDescription.InternalError);

					throw new IOException("The authentication or decryption has failed.", ex);
				}

				if (internalResult.ProceedAfterHandshake)
				{
					//kick off the read or write process (whichever called us) after the handshake is complete
					if (internalResult.FromWrite)
					{
						InternalBeginWrite(internalResult);
					}
					else
					{
						InternalBeginRead(internalResult);
					}
					negotiationComplete.Set();
				}
				else
				{
					negotiationComplete.Set();
					internalResult.SetComplete();
				}

			}
			catch (Exception ex)
			{
				negotiationComplete.Set();
				internalResult.SetComplete(ex);
			}
		}

		internal bool MightNeedHandshake
		{
			get
			{
				if (this.context.HandshakeState == HandshakeState.Finished)
				{
					return false;
				}
				else
				{
					lock (this.negotiate)
					{
						return (this.context.HandshakeState != HandshakeState.Finished);
					}
				}
			}
		}

		internal void NegotiateHandshake()
		{
			if (this.MightNeedHandshake)
			{
				InternalAsyncResult ar = new InternalAsyncResult(null, null, null, 0, 0, false, false);

				//if something already started negotiation, wait for it.
				//otherwise end it ourselves.
				if (!BeginNegotiateHandshake(ar))
				{
					this.negotiationComplete.WaitOne();
				}
				else
				{
					this.EndNegotiateHandshake(ar);
				}
			}
		}

		#endregion

		#region Abstracts/Virtuals

		internal abstract IAsyncResult OnBeginNegotiateHandshake(AsyncCallback callback, object state);
		internal abstract void OnNegotiateHandshakeCallback(IAsyncResult asyncResult);

		internal abstract X509Certificate OnLocalCertificateSelection(X509CertificateCollection clientCertificates,
															X509Certificate serverCertificate,
															string targetHost,
															X509CertificateCollection serverRequestedCertificates);

		internal abstract bool OnRemoteCertificateValidation(X509Certificate certificate, int[] errors);
		internal abstract ValidationResult OnRemoteCertificateValidation2 (Mono.Security.X509.X509CertificateCollection collection);
		internal abstract bool HaveRemoteValidation2Callback { get; }

		internal abstract AsymmetricAlgorithm OnLocalPrivateKeySelection(X509Certificate certificate, string targetHost);

		#endregion

		#region Event Methods

		internal X509Certificate RaiseLocalCertificateSelection(X509CertificateCollection certificates,
															X509Certificate remoteCertificate,
															string targetHost,
															X509CertificateCollection requestedCertificates)
		{
			return OnLocalCertificateSelection(certificates, remoteCertificate, targetHost, requestedCertificates);
		}

		internal bool RaiseRemoteCertificateValidation(X509Certificate certificate, int[] errors)
		{
			return OnRemoteCertificateValidation(certificate, errors);
		}

		internal ValidationResult RaiseRemoteCertificateValidation2 (Mono.Security.X509.X509CertificateCollection collection)
		{
			return OnRemoteCertificateValidation2 (collection);
		}

		internal AsymmetricAlgorithm RaiseLocalPrivateKeySelection(
			X509Certificate certificate,
			string targetHost)
		{
			return OnLocalPrivateKeySelection(certificate, targetHost);
		}
		#endregion

		#region Security Properties

		public bool CheckCertRevocationStatus
		{
			get { return this.checkCertRevocationStatus; }
			set { this.checkCertRevocationStatus = value; }
		}

		public CipherAlgorithmType CipherAlgorithm
		{
			get
			{
				if (this.context.HandshakeState == HandshakeState.Finished)
				{
					return this.context.Current.Cipher.CipherAlgorithmType;
				}

				return CipherAlgorithmType.None;
			}
		}

		public int CipherStrength
		{
			get
			{
				if (this.context.HandshakeState == HandshakeState.Finished)
				{
					return this.context.Current.Cipher.EffectiveKeyBits;
				}

				return 0;
			}
		}

		public HashAlgorithmType HashAlgorithm
		{
			get
			{
				if (this.context.HandshakeState == HandshakeState.Finished)
				{
					return this.context.Current.Cipher.HashAlgorithmType;
				}

				return HashAlgorithmType.None;
			}
		}

		public int HashStrength
		{
			get
			{
				if (this.context.HandshakeState == HandshakeState.Finished)
				{
					return this.context.Current.Cipher.HashSize * 8;
				}

				return 0;
			}
		}

		public int KeyExchangeStrength
		{
			get
			{
				if (this.context.HandshakeState == HandshakeState.Finished)
				{
					return this.context.ServerSettings.Certificates[0].RSA.KeySize;
				}

				return 0;
			}
		}

		public ExchangeAlgorithmType KeyExchangeAlgorithm
		{
			get
			{
				if (this.context.HandshakeState == HandshakeState.Finished)
				{
					return this.context.Current.Cipher.ExchangeAlgorithmType;
				}

				return ExchangeAlgorithmType.None;
			}
		}

		public SecurityProtocolType SecurityProtocol
		{
			get
			{
				if (this.context.HandshakeState == HandshakeState.Finished)
				{
					return this.context.SecurityProtocol;
				}

				return 0;
			}
		}

		public X509Certificate ServerCertificate
		{
			get
			{
				if (this.context.HandshakeState == HandshakeState.Finished)
				{
					if (this.context.ServerSettings.Certificates != null &&
						this.context.ServerSettings.Certificates.Count > 0)
					{
						return new X509Certificate(this.context.ServerSettings.Certificates[0].RawData);
					}
				}

				return null;
			}
		}

		// this is used by Mono's certmgr tool to download certificates
		internal Mono.Security.X509.X509CertificateCollection ServerCertificates
		{
			get { return context.ServerSettings.Certificates; }
		}

		#endregion

		#region Internal Async Result/State Class

		private class InternalAsyncResult : IAsyncResult
		{
			private object locker = new object ();
			private AsyncCallback _userCallback;
			private object _userState;
			private Exception _asyncException;
			private ManualResetEvent handle;
			private bool completed;
			private int _bytesRead;
			private bool _fromWrite;
			private bool _proceedAfterHandshake;

			private byte[] _buffer;
			private int _offset;
			private int _count;

			public InternalAsyncResult(AsyncCallback userCallback, object userState, byte[] buffer, int offset, int count, bool fromWrite, bool proceedAfterHandshake)
			{
				_userCallback = userCallback;
				_userState = userState;
				_buffer = buffer;
				_offset = offset;
				_count = count;
				_fromWrite = fromWrite;
				_proceedAfterHandshake = proceedAfterHandshake;
			}

			public bool ProceedAfterHandshake
			{
				get { return _proceedAfterHandshake; }
			}

			public bool FromWrite
			{
				get { return _fromWrite; }
			}

			public byte[] Buffer
			{
				get { return _buffer; }
			}

			public int Offset
			{
				get { return _offset; }
			}

			public int Count
			{
				get { return _count; }
			}

			public int BytesRead
			{
				get { return _bytesRead; }
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
					if (IsCompleted == false)
						return false;
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
					lock (locker)
						return completed;
				}
			}

			private void SetComplete(Exception ex, int bytesRead)
			{
				lock (locker) {
					if (completed)
						return;

					completed = true;
					_asyncException = ex;
					_bytesRead = bytesRead;
					if (handle != null)
						handle.Set ();
				}
				if (_userCallback != null)
					_userCallback.BeginInvoke (this, null, null);
			}

			public void SetComplete(Exception ex)
			{
				SetComplete(ex, 0);
			}

			public void SetComplete(int bytesRead)
			{
				SetComplete(null, bytesRead);
			}

			public void SetComplete()
			{
				SetComplete(null, 0);
			}
		}
		#endregion

		#region Stream Overrides and Async Stream Operations

		private bool BeginNegotiateHandshake(InternalAsyncResult asyncResult)
		{
			try
			{
				lock (this.negotiate)
				{
					if (this.context.HandshakeState == HandshakeState.None)
					{
						this.OnBeginNegotiateHandshake(new AsyncCallback(AsyncHandshakeCallback), asyncResult);

						return true;
					}
					else
					{
						return false;
					}
				}
			}
			catch (TlsException ex)
			{
				this.negotiationComplete.Set();
				this.protocol.SendAlert(ex.Alert);

				throw new IOException("The authentication or decryption has failed.", ex);
			}
			catch (Exception ex)
			{
				this.negotiationComplete.Set();
				this.protocol.SendAlert(AlertDescription.InternalError);

				throw new IOException("The authentication or decryption has failed.", ex);
			}
		}

		private void EndNegotiateHandshake(InternalAsyncResult asyncResult)
		{
			if (asyncResult.IsCompleted == false)
				asyncResult.AsyncWaitHandle.WaitOne();

			if (asyncResult.CompletedWithError)
			{
				throw asyncResult.AsyncException;
			}
		}

		public override IAsyncResult BeginRead(
			byte[] buffer,
			int offset,
			int count,
			AsyncCallback callback,
			object state)
		{
			this.checkDisposed();

			if (buffer == null)
			{
				throw new ArgumentNullException("buffer is a null reference.");
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset is less than 0.");
			}
			if (offset > buffer.Length)
			{
				throw new ArgumentOutOfRangeException("offset is greater than the length of buffer.");
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count is less than 0.");
			}
			if (count > (buffer.Length - offset))
			{
				throw new ArgumentOutOfRangeException("count is less than the length of buffer minus the value of the offset parameter.");
			}

			InternalAsyncResult asyncResult = new InternalAsyncResult(callback, state, buffer, offset, count, false, true);

			if (this.MightNeedHandshake)
			{
				if (! BeginNegotiateHandshake(asyncResult))
				{
					//we made it down here so the handshake was not started.
					//another thread must have started it in the mean time.
					//wait for it to complete and then perform our original operation
					this.negotiationComplete.WaitOne();

					InternalBeginRead(asyncResult);
				}
			}
			else
			{
				InternalBeginRead(asyncResult);
			}

			return asyncResult;
		}

		// bigger than max record length for SSL/TLS
		private byte[] recbuf = new byte[16384];

		private void InternalBeginRead(InternalAsyncResult asyncResult)
		{
			try
			{
				int preReadSize = 0;

				lock (this.read)
				{
					// If actual buffer is fully read, reset it
					bool shouldReset = this.inputBuffer.Position == this.inputBuffer.Length && this.inputBuffer.Length > 0;

					// If the buffer isn't fully read, but does have data, we need to immediately
					// read the info from the buffer and let the user know that they have more data.
					bool shouldReadImmediately = (this.inputBuffer.Length > 0) && (asyncResult.Count > 0);

					if (shouldReset)
					{
						this.resetBuffer();
					}
					else if (shouldReadImmediately)
					{
						preReadSize = this.inputBuffer.Read(asyncResult.Buffer, asyncResult.Offset, asyncResult.Count);
					}
				}

				// This is explicitly done outside the synclock to avoid 
				// any potential deadlocks in the delegate call.
				if (0 < preReadSize)
				{
					asyncResult.SetComplete(preReadSize);
				}
				else if (!this.context.ConnectionEnd)
				{
					// this will read data from the network until we have (at least) one
					// record to send back to the caller
					this.innerStream.BeginRead(recbuf, 0, recbuf.Length,
						new AsyncCallback(InternalReadCallback), new object[] { recbuf, asyncResult });
				}
				else
				{
					// We're done with the connection so we need to let the caller know with 0 bytes read
					asyncResult.SetComplete(0);
				}
			}
			catch (TlsException ex)
			{
				this.protocol.SendAlert(ex.Alert);

				throw new IOException("The authentication or decryption has failed.", ex);
			}
			catch (Exception ex)
			{
				throw new IOException("IO exception during read.", ex);
			}
		}


		private MemoryStream recordStream = new MemoryStream();

		// read encrypted data until we have enough to decrypt (at least) one
		// record and return are the records (may be more than one) we have
		private void InternalReadCallback(IAsyncResult result)
		{
			if (this.disposed)
				return;

			object[] state = (object[])result.AsyncState;
			byte[] recbuf = (byte[])state[0];
			InternalAsyncResult internalResult = (InternalAsyncResult)state[1];

			try
			{
				int n = innerStream.EndRead(result);
				if (n > 0)
				{
					// Add the just received data to the waiting data
					recordStream.Write(recbuf, 0, n);
				}
				else
				{
					// 0 length data means this read operation is done (lost connection in the case of a network stream).
					internalResult.SetComplete(0);
					return;
				}

				bool dataToReturn = false;
				long pos = recordStream.Position;

				recordStream.Position = 0;
				byte[] record = null;

				// don't try to decode record unless we have at least 5 bytes
				// i.e. type (1), protocol (2) and length (2)
				if (recordStream.Length >= 5)
				{
					record = this.protocol.ReceiveRecord(recordStream);
				}

				// a record of 0 length is valid (and there may be more record after it)
				while (record != null)
				{
					// we probably received more stuff after the record, and we must keep it!
					long remainder = recordStream.Length - recordStream.Position;
					byte[] outofrecord = null;
					if (remainder > 0)
					{
						outofrecord = new byte[remainder];
						recordStream.Read(outofrecord, 0, outofrecord.Length);
					}

					lock (this.read)
					{
						long position = this.inputBuffer.Position;

						if (record.Length > 0)
						{
							// Write new data to the inputBuffer
							this.inputBuffer.Seek(0, SeekOrigin.End);
							this.inputBuffer.Write(record, 0, record.Length);

							// Restore buffer position
							this.inputBuffer.Seek(position, SeekOrigin.Begin);
							dataToReturn = true;
						}
					}

					recordStream.SetLength(0);
					record = null;

					if (remainder > 0)
					{
						recordStream.Write(outofrecord, 0, outofrecord.Length);
						// type (1), protocol (2) and length (2)
						if (recordStream.Length >= 5)
						{
							// try to see if another record is available
							recordStream.Position = 0;
							record = this.protocol.ReceiveRecord(recordStream);
							if (record == null)
								pos = recordStream.Length;
						}
						else
							pos = remainder;
					}
					else
						pos = 0;
				}

				if (!dataToReturn && (n > 0))
				{
					// there is no record to return to caller and (possibly) more data waiting
					// so continue reading from network (and appending to stream)
					recordStream.Position = recordStream.Length;
					this.innerStream.BeginRead(recbuf, 0, recbuf.Length,
						new AsyncCallback(InternalReadCallback), state);
				}
				else
				{
					// we have record(s) to return -or- no more available to read from network
					// reset position for further reading
					recordStream.Position = pos;

					int bytesRead = 0;
					lock (this.read)
					{
						bytesRead = this.inputBuffer.Read(internalResult.Buffer, internalResult.Offset, internalResult.Count);
					}

					internalResult.SetComplete(bytesRead);
				}
			}
			catch (Exception ex)
			{
				internalResult.SetComplete(ex);
			}

		}

		private void InternalBeginWrite(InternalAsyncResult asyncResult)
		{
			try
			{
				// Send the buffer as a TLS record

				lock (this.write)
				{
					byte[] record = this.protocol.EncodeRecord(
						ContentType.ApplicationData, asyncResult.Buffer, asyncResult.Offset, asyncResult.Count);

					this.innerStream.BeginWrite(
						record, 0, record.Length, new AsyncCallback(InternalWriteCallback), asyncResult);
				}
			}
			catch (TlsException ex)
			{
				this.protocol.SendAlert(ex.Alert);
				this.Close();

				throw new IOException("The authentication or decryption has failed.", ex);
			}
			catch (Exception ex)
			{
				throw new IOException("IO exception during Write.", ex);
			}
		}

		private void InternalWriteCallback(IAsyncResult ar)
		{
			if (this.disposed)
				return;
			
			InternalAsyncResult internalResult = (InternalAsyncResult)ar.AsyncState;

			try
			{
				this.innerStream.EndWrite(ar);
				internalResult.SetComplete();
			}
			catch (Exception ex)
			{
				internalResult.SetComplete(ex);
			}
		}

		public override IAsyncResult BeginWrite(
			byte[] buffer,
			int offset,
			int count,
			AsyncCallback callback,
			object state)
		{
			this.checkDisposed();

			if (buffer == null)
			{
				throw new ArgumentNullException("buffer is a null reference.");
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset is less than 0.");
			}
			if (offset > buffer.Length)
			{
				throw new ArgumentOutOfRangeException("offset is greater than the length of buffer.");
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count is less than 0.");
			}
			if (count > (buffer.Length - offset))
			{
				throw new ArgumentOutOfRangeException("count is less than the length of buffer minus the value of the offset parameter.");
			}


			InternalAsyncResult asyncResult = new InternalAsyncResult(callback, state, buffer, offset, count, true, true);

			if (this.MightNeedHandshake)
			{
				if (! BeginNegotiateHandshake(asyncResult))
				{
					//we made it down here so the handshake was not started.
					//another thread must have started it in the mean time.
					//wait for it to complete and then perform our original operation
					this.negotiationComplete.WaitOne();

					InternalBeginWrite(asyncResult);
				}
			}
			else
			{
				InternalBeginWrite(asyncResult);
			}

			return asyncResult;
		}

		public override int EndRead(IAsyncResult asyncResult)
		{
			this.checkDisposed();

			InternalAsyncResult internalResult = asyncResult as InternalAsyncResult;
			if (internalResult == null)
			{
				throw new ArgumentNullException("asyncResult is null or was not obtained by calling BeginRead.");
			}

			// Always wait until the read is complete
			if (!asyncResult.IsCompleted)
			{
				if (!asyncResult.AsyncWaitHandle.WaitOne (WaitTimeOut, false))
					throw new TlsException (AlertDescription.InternalError, "Couldn't complete EndRead");
			}

			if (internalResult.CompletedWithError)
			{
				throw internalResult.AsyncException;
			}

			return internalResult.BytesRead;
		}

		public override void EndWrite(IAsyncResult asyncResult)
		{
			this.checkDisposed();

			InternalAsyncResult internalResult = asyncResult as InternalAsyncResult;
			if (internalResult == null)
			{
				throw new ArgumentNullException("asyncResult is null or was not obtained by calling BeginWrite.");
			}


			if (!asyncResult.IsCompleted)
			{
				if (!internalResult.AsyncWaitHandle.WaitOne (WaitTimeOut, false))
					throw new TlsException (AlertDescription.InternalError, "Couldn't complete EndWrite");
			}

			if (internalResult.CompletedWithError)
			{
				throw internalResult.AsyncException;
			}
		}

		public override void Close()
		{
			base.Close ();
		}

		public override void Flush()
		{
			this.checkDisposed();

			this.innerStream.Flush();
		}

		public int Read(byte[] buffer)
		{
			return this.Read(buffer, 0, buffer.Length);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			this.checkDisposed ();
			
			if (buffer == null)
			{
				throw new ArgumentNullException ("buffer");
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset is less than 0.");
			}
			if (offset > buffer.Length)
			{
				throw new ArgumentOutOfRangeException("offset is greater than the length of buffer.");
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count is less than 0.");
			}
			if (count > (buffer.Length - offset))
			{
				throw new ArgumentOutOfRangeException("count is less than the length of buffer minus the value of the offset parameter.");
			}

			if (this.context.HandshakeState != HandshakeState.Finished)
			{
				this.NegotiateHandshake (); // Handshake negotiation
			}

			lock (this.read) {
				try {
					record_processing.Reset ();
					// do we already have some decrypted data ?
					if (this.inputBuffer.Position > 0) {
						// or maybe we used all the buffer before ?
						if (this.inputBuffer.Position == this.inputBuffer.Length) {
							this.inputBuffer.SetLength (0);
						} else {
							int n = this.inputBuffer.Read (buffer, offset, count);
							if (n > 0) {
								record_processing.Set ();
								return n;
							}
						}
					}

					bool needMoreData = false;
					while (true) {
						// we first try to process the read with the data we already have
						if ((recordStream.Position == 0) || needMoreData) {
							needMoreData = false;
							// if we loop, then it either means we need more data
							byte[] recbuf = new byte[16384];
							int n = 0;
							if (count == 1) {
								int value = innerStream.ReadByte ();
								if (value >= 0) {
									recbuf[0] = (byte) value;
									n = 1;
								}
							} else {
								n = innerStream.Read (recbuf, 0, recbuf.Length);
							}
							if (n > 0) {
								// Add the new received data to the waiting data
								if ((recordStream.Length > 0) && (recordStream.Position != recordStream.Length))
									recordStream.Seek (0, SeekOrigin.End);
								recordStream.Write (recbuf, 0, n);
							} else {
								// or that the read operation is done (lost connection in the case of a network stream).
								record_processing.Set ();
								return 0;
							}
						}

						bool dataToReturn = false;

						recordStream.Position = 0;
						byte[] record = null;

						// don't try to decode record unless we have at least 5 bytes
						// i.e. type (1), protocol (2) and length (2)
						if (recordStream.Length >= 5) {
							record = this.protocol.ReceiveRecord (recordStream);
							needMoreData = (record == null);
						}

						// a record of 0 length is valid (and there may be more record after it)
						while (record != null) {
							// we probably received more stuff after the record, and we must keep it!
							long remainder = recordStream.Length - recordStream.Position;
							byte[] outofrecord = null;
							if (remainder > 0) {
								outofrecord = new byte[remainder];
								recordStream.Read (outofrecord, 0, outofrecord.Length);
							}

							long position = this.inputBuffer.Position;

							if (record.Length > 0) {
								// Write new data to the inputBuffer
								this.inputBuffer.Seek (0, SeekOrigin.End);
								this.inputBuffer.Write (record, 0, record.Length);

								// Restore buffer position
								this.inputBuffer.Seek (position, SeekOrigin.Begin);
								dataToReturn = true;
							}

							recordStream.SetLength (0);
							record = null;

							if (remainder > 0) {
								recordStream.Write (outofrecord, 0, outofrecord.Length);
							}

							if (dataToReturn) {
								// we have record(s) to return -or- no more available to read from network
								// reset position for further reading
								int i = inputBuffer.Read (buffer, offset, count);
								record_processing.Set ();
								return i;
							}
						}
					}
				}
				catch (TlsException ex)
				{
					throw new IOException("The authentication or decryption has failed.", ex);
				}
				catch (Exception ex)
				{
					throw new IOException("IO exception during read.", ex);
				}
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public void Write(byte[] buffer)
		{
			this.Write(buffer, 0, buffer.Length);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			this.checkDisposed ();
			
			if (buffer == null)
			{
				throw new ArgumentNullException ("buffer");
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset is less than 0.");
			}
			if (offset > buffer.Length)
			{
				throw new ArgumentOutOfRangeException("offset is greater than the length of buffer.");
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count is less than 0.");
			}
			if (count > (buffer.Length - offset))
			{
				throw new ArgumentOutOfRangeException("count is less than the length of buffer minus the value of the offset parameter.");
			}

			if (this.context.HandshakeState != HandshakeState.Finished)
			{
				this.NegotiateHandshake ();
			}

			lock (this.write)
			{
				try
				{
					// Send the buffer as a TLS record
					byte[] record = this.protocol.EncodeRecord (ContentType.ApplicationData, buffer, offset, count);
					this.innerStream.Write (record, 0, record.Length);
				}
				catch (TlsException ex)
				{
					this.protocol.SendAlert(ex.Alert);
					this.Close();
					throw new IOException("The authentication or decryption has failed.", ex);
				}
				catch (Exception ex)
				{
					throw new IOException("IO exception during Write.", ex);
				}
			}
		}

		public override bool CanRead
		{
			get { return this.innerStream.CanRead; }
		}

		public override bool CanSeek
		{
			get { return false; }
		}

		public override bool CanWrite
		{
			get { return this.innerStream.CanWrite; }
		}

		public override long Length
		{
			get { throw new NotSupportedException(); }
		}

		public override long Position
		{
			get
			{
				throw new NotSupportedException();
			}
			set
			{
				throw new NotSupportedException();
			}
		}
		#endregion

		#region IDisposable Members and Finalizer

		~SslStreamBase()
		{
			this.Dispose(false);
		}

		protected override void Dispose (bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					if (this.innerStream != null)
					{
						if (this.context.HandshakeState == HandshakeState.Finished &&
							!this.context.ConnectionEnd)
						{
							// Write close notify							
							this.protocol.SendAlert(AlertDescription.CloseNotify);
						}

						if (this.ownsStream)
						{
							// Close inner stream
							this.innerStream.Close();
						}
					}
					this.ownsStream = false;
					this.innerStream = null;
				}

				this.disposed = true;
				base.Dispose (disposing);
			}
		}

		#endregion

		#region Misc Methods

		private void resetBuffer()
		{
			this.inputBuffer.SetLength(0);
			this.inputBuffer.Position = 0;
		}

		internal void checkDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException("The Stream is closed.");
			}
		}

		#endregion

	}
}
