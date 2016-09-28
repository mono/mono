//
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
		MobileTlsContext xobileTlsContext;
		Exception lastException;

		AsyncProtocolRequest asyncHandshakeRequest;
		AsyncProtocolRequest asyncReadRequest;
		AsyncProtocolRequest asyncWriteRequest;
		BufferOffsetSize2 readBuffer;
		BufferOffsetSize2 writeBuffer;

		object ioLock = new object ();
		int closeRequested;

		static int uniqueNameInteger = 123;

		public MobileAuthenticatedStream (Stream innerStream, bool leaveInnerStreamOpen,
		                                  MSI.MonoTlsSettings settings, MSI.MonoTlsProvider provider)
			: base (innerStream, leaveInnerStreamOpen)
		{
			Settings = settings;
			Provider = provider;

			readBuffer = new BufferOffsetSize2 (16834);
			writeBuffer = new BufferOffsetSize2 (16384);
		}

		public MSI.MonoTlsSettings Settings {
			get;
			private set;
		}

		public MSI.MonoTlsProvider Provider {
			get;
			private set;
		}

		MSI.MonoTlsProvider MSI.IMonoSslStream.Provider {
			get { return Provider; }
		}

		internal bool HasContext {
			get { return xobileTlsContext != null; }
		}

		internal MobileTlsContext Context {
			get {
				CheckThrow (true);
				return xobileTlsContext;
			}
		}

		internal void CheckThrow (bool authSuccessCheck)
		{
			if (closeRequested != 0)
				throw new InvalidOperationException ("Stream is closed.");
			if (lastException != null)
				throw lastException;
			if (authSuccessCheck && !IsAuthenticated)
				throw new InvalidOperationException ("Must be authenticated.");
		}

		Exception SetException (Exception e)
		{
			e = SetException_internal (e);
			if (e != null && xobileTlsContext != null)
				xobileTlsContext.Dispose ();
			return e;
		}

		Exception SetException_internal (Exception e)
		{
			if (lastException == null)
				lastException = e;
			return lastException;
		}

		SslProtocols DefaultProtocols {
			get { return SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls; }
		}

		public void AuthenticateAsClient (string targetHost)
		{
			AuthenticateAsClient (targetHost, new X509CertificateCollection (), DefaultProtocols, false);
		}

		public void AuthenticateAsClient (string targetHost, X509CertificateCollection clientCertificates, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			ValidateCreateContext (false, targetHost, enabledSslProtocols, null, clientCertificates, false);
			ProcessAuthentication (null);
		}

		public IAsyncResult BeginAuthenticateAsClient (string targetHost, AsyncCallback asyncCallback, object asyncState)
		{
			return BeginAuthenticateAsClient (targetHost, new X509CertificateCollection (), DefaultProtocols, false, asyncCallback, asyncState);
		}

		public IAsyncResult BeginAuthenticateAsClient (string targetHost, X509CertificateCollection clientCertificates, SslProtocols enabledSslProtocols, bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState)
		{
			ValidateCreateContext (false, targetHost, enabledSslProtocols, null, clientCertificates, false);
			var result = new LazyAsyncResult (this, asyncState, asyncCallback);
			ProcessAuthentication (result);
			return result;
		}

		public void EndAuthenticateAsClient (IAsyncResult asyncResult)
		{
			EndProcessAuthentication (asyncResult);
		}

		public void AuthenticateAsServer (X509Certificate serverCertificate)
		{
			AuthenticateAsServer (serverCertificate, false, DefaultProtocols, false);
		}

		public void AuthenticateAsServer (X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			ValidateCreateContext (true, string.Empty, enabledSslProtocols, serverCertificate, null, clientCertificateRequired);
			ProcessAuthentication (null);
		}

		public IAsyncResult BeginAuthenticateAsServer (X509Certificate serverCertificate, AsyncCallback asyncCallback, object asyncState)
		{
			return BeginAuthenticateAsServer (serverCertificate, false, DefaultProtocols, false, asyncCallback, asyncState);
		}

		public IAsyncResult BeginAuthenticateAsServer (X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols enabledSslProtocols, bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState)
		{
			ValidateCreateContext (true, string.Empty, enabledSslProtocols, serverCertificate, null, clientCertificateRequired);
			var result = new LazyAsyncResult (this, asyncState, asyncCallback);
			ProcessAuthentication (result);
			return result;
		}

		public void EndAuthenticateAsServer (IAsyncResult asyncResult)
		{
			EndProcessAuthentication (asyncResult);
		}

		public Task AuthenticateAsClientAsync (string targetHost)
		{
			return Task.Factory.FromAsync (BeginAuthenticateAsClient, EndAuthenticateAsClient, targetHost, null);
		}

		public Task AuthenticateAsClientAsync (string targetHost, X509CertificateCollection clientCertificates, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			return Task.Factory.FromAsync ((callback, state) => BeginAuthenticateAsClient (targetHost, clientCertificates, enabledSslProtocols, checkCertificateRevocation, callback, state), EndAuthenticateAsClient, null);
		}

		public Task AuthenticateAsServerAsync (X509Certificate serverCertificate)
		{
			return Task.Factory.FromAsync (BeginAuthenticateAsServer, EndAuthenticateAsServer, serverCertificate, null);
		}

		public Task AuthenticateAsServerAsync (X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			return Task.Factory.FromAsync ((callback, state) => BeginAuthenticateAsServer (serverCertificate, clientCertificateRequired, enabledSslProtocols, checkCertificateRevocation, callback, state), EndAuthenticateAsServer, null);
		}

		public AuthenticatedStream AuthenticatedStream {
			get { return this; }
		}

		internal void ProcessAuthentication (LazyAsyncResult lazyResult)
		{
			var asyncRequest = new AsyncProtocolRequest (this, lazyResult);
			if (Interlocked.CompareExchange (ref asyncHandshakeRequest, asyncRequest, null) != null)
				throw new InvalidOperationException ("Invalid nested call.");

			try {
				if (lastException != null)
					throw lastException;
				if (xobileTlsContext == null)
					throw new InvalidOperationException ();

				readBuffer.Reset ();
				writeBuffer.Reset ();

				try {
					asyncRequest.StartOperation (ProcessHandshake);
				} catch (Exception ex) {
					throw SetException (ex);
				}
			} finally {
				if (lazyResult == null || lastException != null) {
					readBuffer.Reset ();
					writeBuffer.Reset ();
					asyncHandshakeRequest = null;
				}
			}
		}

		internal void EndProcessAuthentication (IAsyncResult result)
		{
			if (result == null)
				throw new ArgumentNullException ("asyncResult");

			var lazyResult = (LazyAsyncResult)result;
			if (Interlocked.Exchange (ref asyncHandshakeRequest, null) == null)
				throw new InvalidOperationException ("Invalid end call.");

			lazyResult.InternalWaitForCompletion ();

			readBuffer.Reset ();
			writeBuffer.Reset ();

			var e = lazyResult.Result as Exception;
			if (e != null)
				throw SetException (e);
		}

		internal void ValidateCreateContext (bool serverMode, string targetHost, SslProtocols enabledProtocols, X509Certificate serverCertificate, X509CertificateCollection clientCertificates, bool clientCertRequired)
		{
			if (xobileTlsContext != null)
				throw new InvalidOperationException ();

			if (serverMode) {
				if (serverCertificate == null)
					throw new ArgumentException ("serverCertificate");
			} else {				
				if (targetHost == null)
					throw new ArgumentException ("targetHost");
				if (targetHost.Length == 0)
					targetHost = "?" + Interlocked.Increment (ref uniqueNameInteger).ToString (NumberFormatInfo.InvariantInfo);
			}

			xobileTlsContext = CreateContext (this, serverMode, targetHost, enabledProtocols, serverCertificate, clientCertificates, clientCertRequired);
		}

		protected abstract MobileTlsContext CreateContext (
			MobileAuthenticatedStream parent, bool serverMode, string targetHost,
			SSA.SslProtocols enabledProtocols, X509Certificate serverCertificate,
			X509CertificateCollection clientCertificates, bool askForClientCert);

		public override IAsyncResult BeginRead (byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState)
		{
			return BeginReadOrWrite (ref asyncReadRequest, ref readBuffer, ProcessRead, new BufferOffsetSize (buffer, offset, count), asyncCallback, asyncState);
		}

		public override int EndRead (IAsyncResult asyncResult)
		{
			return (int)EndReadOrWrite (asyncResult, ref asyncReadRequest);
		}

		public override IAsyncResult BeginWrite (byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState)
		{
			return BeginReadOrWrite (ref asyncWriteRequest, ref writeBuffer, ProcessWrite, new BufferOffsetSize (buffer, offset, count), asyncCallback, asyncState);
		}

		public override void EndWrite (IAsyncResult asyncResult)
		{
			EndReadOrWrite (asyncResult, ref asyncWriteRequest);
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			return ProcessReadOrWrite (ref asyncReadRequest, ref readBuffer, ProcessRead, new BufferOffsetSize (buffer, offset, count), null);
		}

		public void Write (byte[] buffer)
		{
			Write (buffer, 0, buffer.Length);
		}
		public override void Write (byte[] buffer, int offset, int count)
		{
			ProcessReadOrWrite (ref asyncWriteRequest, ref writeBuffer, ProcessWrite, new BufferOffsetSize (buffer, offset, count), null);
		}

		IAsyncResult BeginReadOrWrite (ref AsyncProtocolRequest nestedRequest, ref BufferOffsetSize2 internalBuffer, AsyncOperation operation, BufferOffsetSize userBuffer, AsyncCallback asyncCallback, object asyncState)
		{
			LazyAsyncResult lazyResult = new LazyAsyncResult (this, asyncState, asyncCallback);
			ProcessReadOrWrite (ref nestedRequest, ref internalBuffer, operation, userBuffer, lazyResult);
			return lazyResult;
		}

		object EndReadOrWrite (IAsyncResult asyncResult, ref AsyncProtocolRequest nestedRequest)
		{
			if (asyncResult == null)
				throw new ArgumentNullException("asyncResult");

			var lazyResult = (LazyAsyncResult)asyncResult;

			if (Interlocked.Exchange (ref nestedRequest, null) == null)
				throw new InvalidOperationException ("Invalid end call.");

			// No "artificial" timeouts implemented so far, InnerStream controls timeout.
			lazyResult.InternalWaitForCompletion ();

			Debug ("EndReadOrWrite");

			var e = lazyResult.Result as Exception;
			if (e != null) {
				var ioEx = e as IOException;
				if (ioEx != null)
					throw ioEx;
				throw new IOException ("read failed", e);
			}

			return lazyResult.Result;
		}

		int ProcessReadOrWrite (ref AsyncProtocolRequest nestedRequest, ref BufferOffsetSize2 internalBuffer, AsyncOperation operation, BufferOffsetSize userBuffer, LazyAsyncResult lazyResult)
		{
			if (userBuffer == null || userBuffer.Buffer == null)
				throw new ArgumentNullException ("buffer");
			if (userBuffer.Offset < 0)
				throw new ArgumentOutOfRangeException ("offset");
			if (userBuffer.Size < 0 || userBuffer.Offset + userBuffer.Size > userBuffer.Buffer.Length)
				throw new ArgumentOutOfRangeException ("count");

			CheckThrow (true);

			var name = internalBuffer == readBuffer ? "read" : "write";
			Debug ("ProcessReadOrWrite: {0} {1}", name, userBuffer);

			var asyncRequest = new AsyncProtocolRequest (this, lazyResult, userBuffer);
			return StartOperation (ref nestedRequest, ref internalBuffer, operation, asyncRequest, name);
		}

		int StartOperation (ref AsyncProtocolRequest nestedRequest, ref BufferOffsetSize2 internalBuffer, AsyncOperation operation, AsyncProtocolRequest asyncRequest, string name)
		{
			if (Interlocked.CompareExchange (ref nestedRequest, asyncRequest, null) != null)
				throw new InvalidOperationException ("Invalid nested call.");

			bool failed = false;
			try {
				internalBuffer.Reset ();
				asyncRequest.StartOperation (operation);
				return asyncRequest.UserResult;
			} catch (Exception e) {
				failed = true;
				if (e is IOException)
					throw;
				throw new IOException (name + " failed", e);
			} finally {
				if (asyncRequest.UserAsyncResult == null || failed) {
					internalBuffer.Reset ();
					nestedRequest = null;
				}
			}
		}

		static int nextId;
		internal readonly int ID = ++nextId;

		[SD.Conditional ("MARTIN_DEBUG")]
		protected internal void Debug (string message, params object[] args)
		{
			Console.Error.WriteLine ("MobileAuthenticatedStream({0}): {1}", ID, string.Format (message, args));
		}

		#region Called back from native code via SslConnection

		/*
		 * Called from within SSLRead() and SSLHandshake().  We only access tha managed byte[] here.
		 */
		internal int InternalRead (byte[] buffer, int offset, int size, out bool wantMore)
		{
			try {
				Debug ("InternalRead: {0} {1} {2} {3}", offset, size, asyncReadRequest != null, readBuffer != null);
				var asyncRequest = asyncHandshakeRequest ?? asyncReadRequest;
				return InternalRead (asyncRequest, readBuffer, buffer, offset, size, out wantMore);
			} catch (Exception ex) {
				Debug ("InternalRead failed: {0}", ex);
				SetException_internal (ex);
				wantMore = false;
				return -1;
			}
		}

		int InternalRead (AsyncProtocolRequest asyncRequest, BufferOffsetSize internalBuffer, byte[] buffer, int offset, int size, out bool wantMore)
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
				Debug ("InternalRead #1: {0} {1}", internalBuffer.Offset, internalBuffer.TotalBytes);
				internalBuffer.Offset = internalBuffer.Size = 0;
				asyncRequest.RequestRead (size);
				wantMore = true;
				return 0;
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
			wantMore = !internalBuffer.Complete && len < size;
			return len;
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
				SetException_internal (ex);
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
		internal int InnerRead (int requestedSize)
		{
			Debug ("InnerRead: {0} {1} {2} {3}", readBuffer.Offset, readBuffer.Size, readBuffer.Remaining, requestedSize);

			var len = System.Math.Min (readBuffer.Remaining, requestedSize);
			if (len == 0)
				throw new InvalidOperationException ();
			var ret = InnerStream.Read (readBuffer.Buffer, readBuffer.EndOffset, len);
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

		internal void InnerWrite ()
		{
			Debug ("InnerWrite: {0} {1}", writeBuffer.Offset, writeBuffer.Size);
			InnerFlush ();
		}

		internal void InnerFlush ()
		{
			if (writeBuffer.Size > 0) {
				InnerStream.Write (writeBuffer.Buffer, writeBuffer.Offset, writeBuffer.Size);
				writeBuffer.TotalBytes += writeBuffer.Size;
				writeBuffer.Offset = writeBuffer.Size = 0;
			}
		}

		#endregion

		#region Main async I/O loop

		AsyncOperationStatus ProcessHandshake (AsyncProtocolRequest asyncRequest, AsyncOperationStatus status)
		{
			Debug ("ProcessHandshake: {0}", status);

			/*
			 * The first time we're called (AsyncOperationStatus.Initialize), we need to setup the SslContext and
			 * start the handshake.
			*/
			if (status == AsyncOperationStatus.Initialize) {
				xobileTlsContext.StartHandshake ();
				return AsyncOperationStatus.Continue;
			} else if (status == AsyncOperationStatus.ReadDone) {
				// remote prematurely closed connection.
				throw new IOException ("Remote prematurely closed connection.");
			} else if (status != AsyncOperationStatus.Continue) {
				throw new InvalidOperationException ();
			}

			/*
			 * SSLHandshake() will return repeatedly with 'SslStatus.WouldBlock', we then need
			 * to take care of I/O and call it again.
			*/
			if (!xobileTlsContext.ProcessHandshake ()) {
				/*
				 * Flush the internal write buffer.
				 */
				InnerFlush ();
				return AsyncOperationStatus.Continue;
			}

			xobileTlsContext.FinishHandshake ();
			return AsyncOperationStatus.Complete;
		}

		AsyncOperationStatus ProcessRead (AsyncProtocolRequest asyncRequest, AsyncOperationStatus status)
		{
			Debug ("ProcessRead - read user: {0} {1}", status, asyncRequest.UserBuffer);

			int ret;
			bool wantMore;
			lock (ioLock) {
				ret = Context.Read (asyncRequest.UserBuffer.Buffer, asyncRequest.UserBuffer.Offset, asyncRequest.UserBuffer.Size, out wantMore);
			}
			Debug ("ProcessRead - read user done: {0} - {1} {2}", asyncRequest.UserBuffer, ret, wantMore);

			if (ret < 0) {
				asyncRequest.UserResult = -1;
				return AsyncOperationStatus.Complete;
			}

			asyncRequest.CurrentSize += ret;
			asyncRequest.UserBuffer.Offset += ret;
			asyncRequest.UserBuffer.Size -= ret;

			Debug ("Process Read - read user done #1: {0} - {1} {2}", asyncRequest.UserBuffer, asyncRequest.CurrentSize, wantMore);

			if (wantMore && asyncRequest.CurrentSize == 0)
				return AsyncOperationStatus.WantRead;

			asyncRequest.ResetRead ();
			asyncRequest.UserResult = asyncRequest.CurrentSize;
			return AsyncOperationStatus.Complete;
		}

		AsyncOperationStatus ProcessWrite (AsyncProtocolRequest asyncRequest, AsyncOperationStatus status)
		{
			Debug ("ProcessWrite - write user: {0} {1}", status, asyncRequest.UserBuffer);

			if (asyncRequest.UserBuffer.Size == 0) {
				asyncRequest.UserResult = asyncRequest.CurrentSize;
				return AsyncOperationStatus.Complete;
			}

			int ret;
			bool wantMore;
			lock (ioLock) {
				ret = Context.Write (asyncRequest.UserBuffer.Buffer, asyncRequest.UserBuffer.Offset, asyncRequest.UserBuffer.Size, out wantMore);
			}
			Debug ("ProcessWrite - write user done: {0} - {1} {2}", asyncRequest.UserBuffer, ret, wantMore);

			if (ret < 0) {
				asyncRequest.UserResult = -1;
				return AsyncOperationStatus.Complete;
			}

			asyncRequest.CurrentSize += ret;
			asyncRequest.UserBuffer.Offset += ret;
			asyncRequest.UserBuffer.Size -= ret;

			if (wantMore || writeBuffer.Size > 0)
				return AsyncOperationStatus.WantWrite;

			asyncRequest.ResetWrite ();
			asyncRequest.UserResult = asyncRequest.CurrentSize;
			return AsyncOperationStatus.Complete;
		}

		AsyncOperationStatus ProcessClose (AsyncProtocolRequest asyncRequest, AsyncOperationStatus status)
		{
			Debug ("ProcessClose: {0}", status);

			lock (ioLock) {
				if (xobileTlsContext == null)
					return AsyncOperationStatus.Complete;

				xobileTlsContext.Close ();
				xobileTlsContext = null;
				return AsyncOperationStatus.Continue;
			}
		}

		AsyncOperationStatus ProcessFlush (AsyncProtocolRequest asyncRequest, AsyncOperationStatus status)
		{
			Debug ("ProcessFlush: {0}", status);
			return AsyncOperationStatus.Complete;
		}

		#endregion

		public override bool IsServer {
			get { return xobileTlsContext != null && xobileTlsContext.IsServer; }
		}

		public override bool IsAuthenticated {
			get { return xobileTlsContext != null && lastException == null && xobileTlsContext.IsAuthenticated; }
		}

		public override bool IsMutuallyAuthenticated {
			get {
				return IsAuthenticated &&
					(Context.IsServer? Context.LocalServerCertificate: Context.LocalClientCertificate) != null &&
					Context.IsRemoteCertificateAvailable;
			}
		}

		protected override void Dispose (bool disposing)
		{
			try {
				lastException = new ObjectDisposedException ("MobileAuthenticatedStream");
				lock (ioLock) {
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
			CheckThrow (true);
			var asyncRequest = new AsyncProtocolRequest (this, null);
			StartOperation (ref asyncWriteRequest, ref writeBuffer, ProcessFlush, asyncRequest, "flush");
		}

		public override void Close ()
		{
			/*
			 * SSLClose() is a little bit tricky as it might attempt to send a close_notify alert
			 * and thus call our write callback.
			 *
			 * It is also not thread-safe with SSLRead() or SSLWrite(), so we need to take the I/O lock here.
			 */
			if (Interlocked.Exchange (ref closeRequested, 1) == 1)
				return;
			if (xobileTlsContext == null)
				return;

			var asyncRequest = new AsyncProtocolRequest (this, null);
			StartOperation (ref asyncWriteRequest, ref writeBuffer, ProcessClose, asyncRequest, "close");
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
			get { return IsAuthenticated & InnerStream.CanWrite; }
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

		public SslProtocols SslProtocol {
			get {
				CheckThrow (true);
				return (SslProtocols)Context.NegotiatedProtocol;
			}
		}

		public X509Certificate RemoteCertificate {
			get {
				CheckThrow (true);
				return Context.RemoteCertificate;
			}
		}

		public X509Certificate LocalCertificate {
			get {
				CheckThrow (true);
				return InternalLocalCertificate;
			}
		}

		public X509Certificate InternalLocalCertificate {
			get {
				CheckThrow (false);
				if (!HasContext)
					return null;
				return Context.IsServer ? Context.LocalServerCertificate : Context.LocalClientCertificate;
			}
		}

		public MSI.MonoTlsConnectionInfo GetConnectionInfo ()
		{
			CheckThrow (true);
			return Context.ConnectionInfo;
		}

		public SSA.CipherAlgorithmType CipherAlgorithm {
			get {
				CheckThrow (true);
				var info = Context.ConnectionInfo;
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
				var info = Context.ConnectionInfo;
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
				var info = Context.ConnectionInfo;
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
