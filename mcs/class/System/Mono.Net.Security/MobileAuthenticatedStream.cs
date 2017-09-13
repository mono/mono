﻿//
// MobileAuthenticatedStream.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2015 Xamarin, Inc.
//

#if SECURITY_DEP
#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
#endif

#if MONO_SECURITY_ALIAS
using MSI = MonoSecurity::Mono.Security.Interface;
#else
using MSI = Mono.Security.Interface;
#endif

using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Globalization;
using System.Security.Authentication;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;

using SD = System.Diagnostics;
using SSA = System.Security.Authentication;
using SslProtocols = System.Security.Authentication.SslProtocols;

namespace Mono.Net.Security
{
	abstract class MobileAuthenticatedStream : AuthenticatedStream, MSI.IMonoSslStream
	{
		/*
		 * This is intentionally called `xobileTlsContext'.  It is a "dangerous" object
		 * that must not be touched outside the `ioLock' and we need to be very careful
		 * where we access it.
		 */
		MobileTlsContext xobileTlsContext;
		ExceptionDispatchInfo lastException;

		AsyncProtocolRequest asyncHandshakeRequest;
		AsyncProtocolRequest asyncReadRequest;
		AsyncProtocolRequest asyncWriteRequest;
		BufferOffsetSize2 readBuffer;
		BufferOffsetSize2 writeBuffer;

		object ioLock = new object ();
		int closeRequested;
		bool shutdown;

		static int uniqueNameInteger = 123;

		public MobileAuthenticatedStream (Stream innerStream, bool leaveInnerStreamOpen, SslStream owner,
						  MSI.MonoTlsSettings settings, MSI.MonoTlsProvider provider)
			: base (innerStream, leaveInnerStreamOpen)
		{
			SslStream = owner;
			Settings = settings;
			Provider = provider;

			readBuffer = new BufferOffsetSize2 (16834);
			writeBuffer = new BufferOffsetSize2 (16384);
		}

		public SslStream SslStream {
			get;
		}

		public MSI.MonoTlsSettings Settings {
			get;
		}

		public MSI.MonoTlsProvider Provider {
			get;
		}

		internal bool HasContext {
			get { return xobileTlsContext != null; }
		}

		internal void CheckThrow (bool authSuccessCheck, bool shutdownCheck = false)
		{
			if (lastException != null)
				lastException.Throw ();
			if (authSuccessCheck && !IsAuthenticated)
				throw new InvalidOperationException (SR.net_auth_noauth);
			if (shutdownCheck && shutdown)
				throw new InvalidOperationException (SR.net_ssl_io_already_shutdown);
		}

		internal static Exception GetSSPIException (Exception e)
		{
			if (e is OperationCanceledException || e is IOException || e is ObjectDisposedException || e is AuthenticationException)
				return e;
			return new AuthenticationException (SR.net_auth_SSPI, e);
		}

		internal static Exception GetIOException (Exception e, string message)
		{
			if (e is OperationCanceledException || e is IOException || e is ObjectDisposedException || e is AuthenticationException)
				return e;
			return new IOException (message, e);
		}

		internal ExceptionDispatchInfo SetException (Exception e)
		{
			var info = ExceptionDispatchInfo.Capture (e);
			var old = Interlocked.CompareExchange (ref lastException, info, null);
			return old ?? info;
		}

		SslProtocols DefaultProtocols {
			get { return SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls; }
		}

		enum OperationType {
			Read,
			Write,
			Shutdown
		}

		public void AuthenticateAsClient (string targetHost)
		{
			AuthenticateAsClient (targetHost, new X509CertificateCollection (), DefaultProtocols, false);
		}

		public void AuthenticateAsClient (string targetHost, X509CertificateCollection clientCertificates, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			var task = ProcessAuthentication (true, false, targetHost, enabledSslProtocols, null, clientCertificates, false);
			task.Wait ();
		}

		public IAsyncResult BeginAuthenticateAsClient (string targetHost, AsyncCallback asyncCallback, object asyncState)
		{
			return BeginAuthenticateAsClient (targetHost, new X509CertificateCollection (), DefaultProtocols, false, asyncCallback, asyncState);
		}

		public IAsyncResult BeginAuthenticateAsClient (string targetHost, X509CertificateCollection clientCertificates, SslProtocols enabledSslProtocols, bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState)
		{
			var task = ProcessAuthentication (false, false, targetHost, enabledSslProtocols, null, clientCertificates, false);
			return TaskToApm.Begin (task, asyncCallback, asyncState);
		}

		public void EndAuthenticateAsClient (IAsyncResult asyncResult)
		{
			TaskToApm.End (asyncResult);
		}

		public void AuthenticateAsServer (X509Certificate serverCertificate)
		{
			AuthenticateAsServer (serverCertificate, false, DefaultProtocols, false);
		}

		public void AuthenticateAsServer (X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			var task = ProcessAuthentication (true, true, string.Empty, enabledSslProtocols, serverCertificate, null, clientCertificateRequired);
			task.Wait ();
		}

		public IAsyncResult BeginAuthenticateAsServer (X509Certificate serverCertificate, AsyncCallback asyncCallback, object asyncState)
		{
			return BeginAuthenticateAsServer (serverCertificate, false, DefaultProtocols, false, asyncCallback, asyncState);
		}

		public IAsyncResult BeginAuthenticateAsServer (X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols enabledSslProtocols, bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState)
		{
			var task = ProcessAuthentication (false, true, string.Empty, enabledSslProtocols, serverCertificate, null, clientCertificateRequired);
			return TaskToApm.Begin (task, asyncCallback, asyncState);
		}

		public void EndAuthenticateAsServer (IAsyncResult asyncResult)
		{
			TaskToApm.End (asyncResult);
		}

		public Task AuthenticateAsClientAsync (string targetHost)
		{
			return ProcessAuthentication (false, false, targetHost, DefaultProtocols, null, null, false);
		}

		public Task AuthenticateAsClientAsync (string targetHost, X509CertificateCollection clientCertificates, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			return ProcessAuthentication (false, false, targetHost, enabledSslProtocols, null, clientCertificates, false);
		}

		public Task AuthenticateAsServerAsync (X509Certificate serverCertificate)
		{
			return AuthenticateAsServerAsync (serverCertificate, false, DefaultProtocols, false);
		}

		public Task AuthenticateAsServerAsync (X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			return ProcessAuthentication (false, true, string.Empty, enabledSslProtocols, serverCertificate, null, clientCertificateRequired);
		}

		public Task ShutdownAsync ()
		{
			Debug ("ShutdownAsync");

			/*
			 * SSLClose() is a little bit tricky as it might attempt to send a close_notify alert
			 * and thus call our write callback.
			 *
			 * It is also not thread-safe with SSLRead() or SSLWrite(), so we need to take the I/O lock here.
			 */
			var asyncRequest = new AsyncShutdownRequest (this);
			var task = StartOperation (OperationType.Shutdown, asyncRequest, CancellationToken.None);
			return task;
		}

		public AuthenticatedStream AuthenticatedStream {
			get { return this; }
		}

		async Task ProcessAuthentication (
			bool runSynchronously, bool serverMode, string targetHost, SslProtocols enabledProtocols,
			X509Certificate serverCertificate, X509CertificateCollection clientCertificates, bool clientCertRequired)
		{
			if (serverMode) {
				if (serverCertificate == null)
					throw new ArgumentException (nameof (serverCertificate));
			} else {
				if (targetHost == null)
					throw new ArgumentException (nameof (targetHost));
				if (targetHost.Length == 0)
					targetHost = "?" + Interlocked.Increment (ref uniqueNameInteger).ToString (NumberFormatInfo.InvariantInfo);
			}

			if (lastException != null)
				lastException.Throw ();

			var asyncRequest = new AsyncHandshakeRequest (this, runSynchronously);
			if (Interlocked.CompareExchange (ref asyncHandshakeRequest, asyncRequest, null) != null)
				throw new InvalidOperationException ("Invalid nested call.");
			// Make sure no other async requests can be started during the handshake.
			if (Interlocked.CompareExchange (ref asyncReadRequest, asyncRequest, null) != null)
				throw new InvalidOperationException ("Invalid nested call.");
			if (Interlocked.CompareExchange (ref asyncWriteRequest, asyncRequest, null) != null)
				throw new InvalidOperationException ("Invalid nested call.");

			AsyncProtocolResult result;

			try {
				lock (ioLock) {
					if (xobileTlsContext != null)
						throw new InvalidOperationException ();
					readBuffer.Reset ();
					writeBuffer.Reset ();

					xobileTlsContext = CreateContext (
						serverMode, targetHost, enabledProtocols, serverCertificate,
						clientCertificates, clientCertRequired);
				}

				try {
					result = await asyncRequest.StartOperation (CancellationToken.None).ConfigureAwait (false);
				} catch (Exception ex) {
					result = new AsyncProtocolResult (SetException (GetSSPIException (ex)));
				}
			} finally {
				lock (ioLock) {
					readBuffer.Reset ();
					writeBuffer.Reset ();
					asyncWriteRequest = null;
					asyncReadRequest = null;
					asyncHandshakeRequest = null;
				}
			}

			if (result.Error != null)
				result.Error.Throw ();
		}

		protected abstract MobileTlsContext CreateContext (
			bool serverMode, string targetHost, SSA.SslProtocols enabledProtocols,
			X509Certificate serverCertificate, X509CertificateCollection clientCertificates,
			bool askForClientCert);

		public override IAsyncResult BeginRead (byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState)
		{
			var asyncRequest = new AsyncReadRequest (this, false, buffer, offset, count);
			var task = StartOperation (OperationType.Read, asyncRequest, CancellationToken.None);
			return TaskToApm.Begin (task, asyncCallback, asyncState);
		}

		public override int EndRead (IAsyncResult asyncResult)
		{
			return TaskToApm.End<int> (asyncResult);
		}

		public override IAsyncResult BeginWrite (byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState)
		{
			var asyncRequest = new AsyncWriteRequest (this, false, buffer, offset, count);
			var task = StartOperation (OperationType.Write, asyncRequest, CancellationToken.None);
			return TaskToApm.Begin (task, asyncCallback, asyncState);
		}

		public override void EndWrite (IAsyncResult asyncResult)
		{
			TaskToApm.End (asyncResult);
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			var asyncRequest = new AsyncReadRequest (this, true, buffer, offset, count);
			var task = StartOperation (OperationType.Read, asyncRequest, CancellationToken.None);
			return task.Result;
		}

		public void Write (byte[] buffer)
		{
			Write (buffer, 0, buffer.Length);
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			var asyncRequest = new AsyncWriteRequest (this, true, buffer, offset, count);
			var task = StartOperation (OperationType.Write, asyncRequest, CancellationToken.None);
			task.Wait ();
		}

		public override Task<int> ReadAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			var asyncRequest = new AsyncReadRequest (this, false, buffer, offset, count);
			return StartOperation (OperationType.Read, asyncRequest, cancellationToken);
		}

		public override Task WriteAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			var asyncRequest = new AsyncWriteRequest (this, false, buffer, offset, count);
			return StartOperation (OperationType.Write, asyncRequest, cancellationToken);
		}

		async Task<int> StartOperation (OperationType type, AsyncProtocolRequest asyncRequest, CancellationToken cancellationToken)
		{
			CheckThrow (true, type != OperationType.Read);
			Debug ("StartOperationAsync: {0} {1}", asyncRequest, type);

			if (type == OperationType.Read) {
				if (Interlocked.CompareExchange (ref asyncReadRequest, asyncRequest, null) != null)
					throw new InvalidOperationException ("Invalid nested call.");
			} else {
				if (Interlocked.CompareExchange (ref asyncWriteRequest, asyncRequest, null) != null)
					throw new InvalidOperationException ("Invalid nested call.");
			}

			AsyncProtocolResult result;

			try {
				lock (ioLock) {
					if (type == OperationType.Read)
						readBuffer.Reset ();
					else
						writeBuffer.Reset ();
				}
				result = await asyncRequest.StartOperation (cancellationToken).ConfigureAwait (false);
			} catch (Exception e) {
				var info = SetException (GetIOException (e, asyncRequest.Name + " failed"));
				result = new AsyncProtocolResult (info);
			} finally {
				lock (ioLock) {
					if (type == OperationType.Read) {
						readBuffer.Reset ();
						asyncReadRequest = null;
					} else {
						writeBuffer.Reset ();
						asyncWriteRequest = null;
					}
				}
			}

			if (result.Error != null)
				result.Error.Throw ();
			return result.UserResult;
		}

		static int nextId;
		internal readonly int ID = ++nextId;

		[SD.Conditional ("MONO_TLS_DEBUG")]
		protected internal void Debug (string message, params object[] args)
		{
			MonoTlsProviderFactory.Debug ("MobileAuthenticatedStream({0}): {1}", ID, string.Format (message, args));
		}

#region Called back from native code via SslConnection

		/*
		 * Called from within SSLRead() and SSLHandshake().  We only access tha managed byte[] here.
		 */
		internal int InternalRead (byte[] buffer, int offset, int size, out bool outWantMore)
		{
			try {
				Debug ("InternalRead: {0} {1} {2} {3} {4}", offset, size,
				       asyncHandshakeRequest != null ? "handshake" : "",
				       asyncReadRequest != null ? "async" : "",
				       readBuffer != null ? readBuffer.ToString () : "");
				var asyncRequest = asyncHandshakeRequest ?? asyncReadRequest;
				var (ret, wantMore) = InternalRead (asyncRequest, readBuffer, buffer, offset, size);
				outWantMore = wantMore;
				return ret;
			} catch (Exception ex) {
				Debug ("InternalRead failed: {0}", ex);
				SetException (GetIOException (ex, "InternalRead() failed"));
				outWantMore = false;
				return -1;
			}
		}

		(int, bool) InternalRead (AsyncProtocolRequest asyncRequest, BufferOffsetSize internalBuffer, byte[] buffer, int offset, int size)
		{
			if (asyncRequest == null)
				throw new InvalidOperationException ();

			Debug ("InternalRead: {0} {1} {2}", internalBuffer, offset, size);

			/*
			 * One of Apple's native functions wants to read 'size' bytes of data.
			 *
			 * First, we check whether we already have enough in the internal buffer.
			 *
			 * If the internal buffer is empty (it will be the first time we're called), we save
			 * the amount of bytes that were requested and return 'SslStatus.WouldBlock' to our
			 * native caller.  This native function will then return this code to managed code,
			 * where we read the requested amount of data into the internal buffer, then call the
			 * native function again.
			 */
			if (internalBuffer.Size == 0 && !internalBuffer.Complete) {
				Debug ("InternalRead #1: {0} {1} {2}", internalBuffer.Offset, internalBuffer.TotalBytes, size);
				internalBuffer.Offset = internalBuffer.Size = 0;
				asyncRequest.RequestRead (size);
				return (0, true);
			}

			/*
			 * The second time we're called, the native buffer will contain the exact amount of data that the
			 * previous call requested from us, so we should be able to return it all here.  However, just in
			 * case that Apple's native function changed its mind, we can also return less.
			 *
			 * In either case, if we have any data buffered, then we return as much of it as possible - if the
			 * native code isn't satisfied, then it will call us again to request more.
			 */
			var len = System.Math.Min (internalBuffer.Size, size);
			Buffer.BlockCopy (internalBuffer.Buffer, internalBuffer.Offset, buffer, offset, len);
			internalBuffer.Offset += len;
			internalBuffer.Size -= len;
			return (len, !internalBuffer.Complete && len < size);
		}

		/*
		 * We may get called from SSLWrite(), SSLHandshake() or SSLClose().
		 */
		internal bool InternalWrite (byte[] buffer, int offset, int size)
		{
			try {
				Debug ("InternalWrite: {0} {1}", offset, size);
				var asyncRequest = asyncHandshakeRequest ?? asyncWriteRequest;
				return InternalWrite (asyncRequest, writeBuffer, buffer, offset, size);
			} catch (Exception ex) {
				Debug ("InternalWrite failed: {0}", ex);
				SetException (GetIOException (ex, "InternalWrite() failed"));
				return false;
			}
		}

		bool InternalWrite (AsyncProtocolRequest asyncRequest, BufferOffsetSize2 internalBuffer, byte[] buffer, int offset, int size)
		{
			Debug ("InternalWrite: {0} {1} {2} {3}", asyncRequest != null, internalBuffer, offset, size);

			if (asyncRequest == null) {
				/*
				 * The only situation where 'asyncRequest' could possibly be 'null' is when we're called
				 * from within SSLClose() - which might attempt to send the close_notity notification.
				 * Since this notification message is very small, it should definitely fit into our internal
				 * buffer, so we just save it in there and after SSLClose() returns, the final call to
				 * InternalFlush() - just before closing the underlying stream - will send it out.
				 */
				if (lastException != null)
					return false;

				if (Interlocked.Exchange (ref closeRequested, 1) == 0)
					internalBuffer.Reset ();
				else if (internalBuffer.Remaining == 0)
					throw new InvalidOperationException ();
			}

			/*
			 * Normal write - can be either SSLWrite() or SSLHandshake().
			 *
			 * It is important that we always accept all the data and queue it.
			 */

			internalBuffer.AppendData (buffer, offset, size);

			/*
			 * Calling 'asyncRequest.RequestWrite()' here ensures that ProcessWrite() is called next
			 * time we regain control from native code.
			 *
			 * During the handshake, the native code won't actually realize (unless if attempts to send
			 * so much that the write buffer gets full) that we only buffered the data.
			 *
			 * However, it doesn't matter because it will either return with a completed handshake
			 * (and doesn't care whether the remote actually received the data) or it will expect more
			 * data from the remote and request a read.  In either case, we regain control in managed
			 * code and can flush out the data.
			 *
			 * Note that a calling RequestWrite() followed by RequestRead() will first flush the write
			 * queue once we return to managed code - before attempting to read anything.
			 */
			if (asyncRequest != null)
				asyncRequest.RequestWrite ();

			return true;
		}

#endregion

#region Inner Stream

		/*
		 * Read / write data from the inner stream; we're only called from managed code and only manipulate
		 * the internal buffers.
		 */
		internal async Task<int> InnerRead (bool sync, int requestedSize, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested ();
			Debug ("InnerRead: {0} {1} {2} {3} {4}", sync, readBuffer.Offset, readBuffer.Size, readBuffer.Remaining, requestedSize);

			var len = System.Math.Min (readBuffer.Remaining, requestedSize);
			if (len == 0)
				throw new InvalidOperationException ();

			Task<int> task;
			if (sync)
				task = Task.Run (() => InnerStream.Read (readBuffer.Buffer, readBuffer.EndOffset, len));
			else
				task = InnerStream.ReadAsync (readBuffer.Buffer, readBuffer.EndOffset, len, cancellationToken);

			var ret = await task.ConfigureAwait (false);
			Debug ("InnerRead done: {0} {1} - {2}", readBuffer.Remaining, len, ret);

			if (ret >= 0) {
				readBuffer.Size += ret;
				readBuffer.TotalBytes += ret;
			}

			if (ret == 0) {
				readBuffer.Complete = true;
				Debug ("InnerRead - end of stream!");
				/*
				 * Try to distinguish between a graceful close - first Read() returned 0 - and
				 * the remote prematurely closing the connection without sending us all data.
				 */
				if (readBuffer.TotalBytes > 0)
					ret = -1;
			}

			Debug ("InnerRead done: {0} - {1} {2}", readBuffer, len, ret);
			return ret;
		}

		internal async Task InnerWrite (bool sync, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested ();
			Debug ("InnerWrite: {0} {1}", writeBuffer.Offset, writeBuffer.Size);

			if (writeBuffer.Size == 0)
				return;

			Task task;
			if (sync)
				task = Task.Run (() => InnerStream.Write (writeBuffer.Buffer, writeBuffer.Offset, writeBuffer.Size));
			else
				task = InnerStream.WriteAsync (writeBuffer.Buffer, writeBuffer.Offset, writeBuffer.Size);

			await task.ConfigureAwait (false);

			writeBuffer.TotalBytes += writeBuffer.Size;
			writeBuffer.Offset = writeBuffer.Size = 0;
		}

#endregion

#region Main async I/O loop

		internal AsyncOperationStatus ProcessHandshake (AsyncOperationStatus status)
		{
			Debug ("ProcessHandshake: {0}", status);

			lock (ioLock) {
				/*
				 * The first time we're called (AsyncOperationStatus.Initialize), we need to setup the SslContext and
				 * start the handshake.
				*/
				if (status == AsyncOperationStatus.Initialize) {
					xobileTlsContext.StartHandshake ();
					return AsyncOperationStatus.Continue;
				} else if (status == AsyncOperationStatus.ReadDone) {
					throw new IOException (SR.net_auth_eof);
				} else if (status != AsyncOperationStatus.Continue) {
					throw new InvalidOperationException ();
				}

				/*
				 * SSLHandshake() will return repeatedly with 'SslStatus.WouldBlock', we then need
				 * to take care of I/O and call it again.
				*/
				var newStatus = AsyncOperationStatus.Continue;
				if (xobileTlsContext.ProcessHandshake ()) {
					xobileTlsContext.FinishHandshake ();
					newStatus = AsyncOperationStatus.Complete;
				}

				if (lastException != null)
					lastException.Throw ();

				return newStatus;
			}
		}

		internal (int, bool) ProcessRead (BufferOffsetSize userBuffer)
		{
			lock (ioLock) {
				// This operates on the internal buffer and will never block.
				var ret = xobileTlsContext.Read (userBuffer.Buffer, userBuffer.Offset, userBuffer.Size);
				if (lastException != null)
					lastException.Throw ();
				return ret;
			}
		}

		internal (int, bool) ProcessWrite (BufferOffsetSize userBuffer)
		{
			lock (ioLock) {
				// This operates on the internal buffer and will never block.
				var ret = xobileTlsContext.Write (userBuffer.Buffer, userBuffer.Offset, userBuffer.Size);
				if (lastException != null)
					lastException.Throw ();
				return ret;
			}
		}

		internal AsyncOperationStatus ProcessShutdown (AsyncOperationStatus status)
		{
			Debug ("ProcessShutdown: {0}", status);

			lock (ioLock) {
				xobileTlsContext.Shutdown ();
				shutdown = true;
				return AsyncOperationStatus.Complete;
			}
		}

#endregion

		public override bool IsServer {
			get {
				CheckThrow (false);
				return xobileTlsContext != null && xobileTlsContext.IsServer;
			}
		}

		public override bool IsAuthenticated {
			get {
				lock (ioLock) {
					// Don't use CheckThrow(), we want to return false if we're not authenticated.
					return xobileTlsContext != null && lastException == null && xobileTlsContext.IsAuthenticated;
				}
			}
		}

		public override bool IsMutuallyAuthenticated {
			get {
				lock (ioLock) {
					// Don't use CheckThrow() here.
					if (!IsAuthenticated)
						return false;
					if ((xobileTlsContext.IsServer ? xobileTlsContext.LocalServerCertificate : xobileTlsContext.LocalClientCertificate) == null)
						return false;
					return xobileTlsContext.IsRemoteCertificateAvailable;
				}
			}
		}

		protected override void Dispose (bool disposing)
		{
			try {
				lock (ioLock) {
					Debug ("Dispose: {0}", xobileTlsContext != null);
					lastException = ExceptionDispatchInfo.Capture (new ObjectDisposedException ("MobileAuthenticatedStream"));
					if (xobileTlsContext != null) {
						xobileTlsContext.Dispose ();
						xobileTlsContext = null;
					}
				}
			} finally {
				base.Dispose (disposing);
			}
		}

		public override void Flush ()
		{
			InnerStream.Flush ();
		}

		public SslProtocols SslProtocol {
			get {
				lock (ioLock) {
					CheckThrow (true);
					return (SslProtocols)xobileTlsContext.NegotiatedProtocol;
				}
			}
		}

		public X509Certificate RemoteCertificate {
			get {
				lock (ioLock) {
					CheckThrow (true);
					return xobileTlsContext.RemoteCertificate;
				}
			}
		}

		public X509Certificate LocalCertificate {
			get {
				lock (ioLock) {
					CheckThrow (true);
					return InternalLocalCertificate;
				}
			}
		}

		public X509Certificate InternalLocalCertificate {
			get {
				lock (ioLock) {
					CheckThrow (false);
					if (xobileTlsContext == null)
						return null;
					return xobileTlsContext.IsServer ? xobileTlsContext.LocalServerCertificate : xobileTlsContext.LocalClientCertificate;
				}
			}
		}

		public MSI.MonoTlsConnectionInfo GetConnectionInfo ()
		{
			lock (ioLock) {
				CheckThrow (true);
				return xobileTlsContext.ConnectionInfo;
			}
		}

		//
		// 'xobileTlsContext' must not be accessed below this point.
		//

		public override long Seek (long offset, SeekOrigin origin)
		{
			throw new NotSupportedException ();
		}

		public override void SetLength (long value)
		{
			InnerStream.SetLength (value);
		}

		public TransportContext TransportContext {
			get { throw new NotSupportedException (); }
		}

		public override bool CanRead {
			get { return IsAuthenticated && InnerStream.CanRead; }
		}

		public override bool CanTimeout {
			get { return InnerStream.CanTimeout; }
		}

		public override bool CanWrite {
			get { return IsAuthenticated & InnerStream.CanWrite && !shutdown; }
		}

		public override bool CanSeek {
			get { return false; }
		}

		public override long Length {
			get { return InnerStream.Length; }
		}

		public override long Position {
			get { return InnerStream.Position; }
			set { throw new NotSupportedException (); }
		}

		public override bool IsEncrypted {
			get { return IsAuthenticated; }
		}

		public override bool IsSigned {
			get { return IsAuthenticated; }
		}

		public override int ReadTimeout {
			get { return InnerStream.ReadTimeout; }
			set { InnerStream.ReadTimeout = value; }
		}

		public override int WriteTimeout {
			get { return InnerStream.WriteTimeout; }
			set { InnerStream.WriteTimeout = value; }
		}

		public SSA.CipherAlgorithmType CipherAlgorithm {
			get {
				CheckThrow (true);
				var info = GetConnectionInfo ();
				if (info == null)
					return SSA.CipherAlgorithmType.None;
				switch (info.CipherAlgorithmType) {
				case MSI.CipherAlgorithmType.Aes128:
				case MSI.CipherAlgorithmType.AesGcm128:
					return SSA.CipherAlgorithmType.Aes128;
				case MSI.CipherAlgorithmType.Aes256:
				case MSI.CipherAlgorithmType.AesGcm256:
					return SSA.CipherAlgorithmType.Aes256;
				default:
					return SSA.CipherAlgorithmType.None;
				}
			}
		}

		public SSA.HashAlgorithmType HashAlgorithm {
			get {
				CheckThrow (true);
				var info = GetConnectionInfo ();
				if (info == null)
					return SSA.HashAlgorithmType.None;
				switch (info.HashAlgorithmType) {
				case MSI.HashAlgorithmType.Md5:
				case MSI.HashAlgorithmType.Md5Sha1:
					return SSA.HashAlgorithmType.Md5;
				case MSI.HashAlgorithmType.Sha1:
				case MSI.HashAlgorithmType.Sha224:
				case MSI.HashAlgorithmType.Sha256:
				case MSI.HashAlgorithmType.Sha384:
				case MSI.HashAlgorithmType.Sha512:
					return SSA.HashAlgorithmType.Sha1;
				default:
					return SSA.HashAlgorithmType.None;
				}
			}
		}

		public SSA.ExchangeAlgorithmType KeyExchangeAlgorithm {
			get {
				CheckThrow (true);
				var info = GetConnectionInfo ();
				if (info == null)
					return SSA.ExchangeAlgorithmType.None;
				switch (info.ExchangeAlgorithmType) {
				case MSI.ExchangeAlgorithmType.Rsa:
					return SSA.ExchangeAlgorithmType.RsaSign;
				case MSI.ExchangeAlgorithmType.Dhe:
				case MSI.ExchangeAlgorithmType.EcDhe:
					return SSA.ExchangeAlgorithmType.DiffieHellman;
				default:
					return SSA.ExchangeAlgorithmType.None;
				}
			}
		}

#region Need to Implement
		public int CipherStrength {
			get {
				throw new NotImplementedException ();
			}
		}
		public int HashStrength {
			get {
				throw new NotImplementedException ();
			}
		}
		public int KeyExchangeStrength {
			get {
				throw new NotImplementedException ();
			}
		}
		public bool CheckCertRevocationStatus {
			get {
				throw new NotImplementedException ();
			}
		}

#endregion
	}
}
#endif
