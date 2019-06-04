#if SECURITY_DEP
//
// AsyncProtocolRequest.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2015 Xamarin, Inc.
//
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using SD = System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.ExceptionServices;

namespace Mono.Net.Security
{
	class BufferOffsetSize
	{
		public byte[] Buffer;
		public int Offset;
		public int Size;
		public int TotalBytes;
		public bool Complete;

		public int EndOffset {
			get { return Offset + Size; }
		}

		public int Remaining {
			get { return Buffer.Length - Offset - Size; }
		}

		public BufferOffsetSize (byte[] buffer, int offset, int size)
		{
			if (buffer == null)
				throw new ArgumentNullException (nameof (buffer));
			if (offset < 0)
				throw new ArgumentOutOfRangeException (nameof (offset));
			if (size < 0 || offset + size > buffer.Length)
				throw new ArgumentOutOfRangeException (nameof (size));

			Buffer = buffer;
			Offset = offset;
			Size = size;
			Complete = false;
		}

		public override string ToString ()
		{
			return string.Format ("[BufferOffsetSize: {0} {1}]", Offset, Size);
		}
	}

	class BufferOffsetSize2 : BufferOffsetSize
	{
		public readonly int InitialSize;

		public BufferOffsetSize2 (int size)
			: base (new byte[size], 0, 0)
		{
			InitialSize = size;
		}

		public void Reset ()
		{
			Offset = Size = 0;
			TotalBytes = 0;
			Buffer = new byte[InitialSize];
			Complete = false;
		}

		public void MakeRoom (int size)
		{
			if (Remaining >= size)
				return;

			int missing = size - Remaining;
			if (Offset == 0 && Size == 0) {
				Buffer = new byte[size];
				return;
			}

			var buffer = new byte[Buffer.Length + missing];
			Buffer.CopyTo (buffer, 0);
			Buffer = buffer;
		}

		public void AppendData (byte[] buffer, int offset, int size)
		{
			MakeRoom (size);
			System.Buffer.BlockCopy (buffer, offset, Buffer, EndOffset, size);
			Size += size;
		}
	}

	enum AsyncOperationStatus
	{
		Initialize,
		Continue,
		ReadDone,
		Complete
	}

	class AsyncProtocolResult
	{
		public int UserResult {
			get;
		}
		public ExceptionDispatchInfo Error {
			get;
		}

		public AsyncProtocolResult (int result)
		{
			UserResult = result;
		}

		public AsyncProtocolResult (ExceptionDispatchInfo error)
		{
			Error = error;
		}
	}

	abstract class AsyncProtocolRequest
	{
		public MobileAuthenticatedStream Parent {
			get;
		}

		public bool RunSynchronously {
			get;
		}

		public int ID => ++next_id;

		public string Name => GetType ().Name;

		public int UserResult {
			get;
			protected set;
		}

		int Started;
		int RequestedSize;
		int WriteRequested;
		readonly object locker = new object ();

		static int next_id;

		public AsyncProtocolRequest (MobileAuthenticatedStream parent, bool sync)
		{
			Parent = parent;
			RunSynchronously = sync;
		}

		[SD.Conditional ("MONO_TLS_DEBUG")]
		protected void Debug (string message, params object[] args)
		{
			Parent.Debug ("{0}({1}:{2}): {3}", Name, Parent.ID, ID, string.Format (message, args));
		}

		internal void RequestRead (int size)
		{
			lock (locker) {
				RequestedSize += size;
				Debug ("RequestRead: {0}", size);
			}
		}

		internal void RequestWrite ()
		{
			WriteRequested = 1;
		}

		internal async Task<AsyncProtocolResult> StartOperation (CancellationToken cancellationToken)
		{
			Debug ("Start Operation: {0}", this);
			if (Interlocked.CompareExchange (ref Started, 1, 0) != 0)
				throw new InvalidOperationException ();

			try {
				await ProcessOperation (cancellationToken).ConfigureAwait (false);
				return new AsyncProtocolResult (UserResult);
			} catch (Exception ex) {
				// Any exceptions thrown by the underlying stream will be propagated.
				var info = Parent.SetException (ex);
				return new AsyncProtocolResult (info);
			}
		}

		async Task ProcessOperation (CancellationToken cancellationToken)
		{
			var status = AsyncOperationStatus.Initialize;
			while (status != AsyncOperationStatus.Complete) {
				cancellationToken.ThrowIfCancellationRequested ();
				Debug ("ProcessOperation: {0}", status);

				var ret = await InnerRead (cancellationToken).ConfigureAwait (false);
				if (ret != null) {
					if (ret == 0) {
						// End-of-stream
						Debug ("END OF STREAM!");
						status = AsyncOperationStatus.ReadDone;
					} else if (ret < 0) {
						// remote prematurely closed connection.
						throw new IOException ("Remote prematurely closed connection.");
					}
				}

				Debug ("ProcessOperation run: {0}", status);

				AsyncOperationStatus newStatus;
				switch (status) {
				case AsyncOperationStatus.Initialize:
				case AsyncOperationStatus.Continue:
				case AsyncOperationStatus.ReadDone:
					try {
						newStatus = Run (status);
					} catch (Exception ex) {
						// We only want to wrap exceptions that are thrown by the TLS code.
						throw MobileAuthenticatedStream.GetSSPIException (ex);
					}
					break;
				default:
					throw new InvalidOperationException ();
				}

				if (Interlocked.Exchange (ref WriteRequested, 0) != 0) {
					// Flush the write queue.
					Debug ("ProcessOperation - flushing write queue");
					await Parent.InnerWrite (RunSynchronously, cancellationToken).ConfigureAwait (false);
				}

				Debug ("ProcessOperation done: {0} -> {1}", status, newStatus);

				status = newStatus;
			}
		}

		async Task<int?> InnerRead (CancellationToken cancellationToken)
		{
			int? totalRead = null;
			var requestedSize = Interlocked.Exchange (ref RequestedSize, 0);
			while (requestedSize > 0) {
				Debug ("ProcessOperation - read inner: {0}", requestedSize);

				var ret = await Parent.InnerRead (RunSynchronously, requestedSize, cancellationToken).ConfigureAwait (false);
				Debug ("ProcessOperation - read inner done: {0} - {1}", requestedSize, ret);

				if (ret <= 0)
					return ret;
				if (ret > requestedSize)
					throw new InvalidOperationException ();

				totalRead += ret;
				requestedSize -= ret;
				var newRequestedSize = Interlocked.Exchange (ref RequestedSize, 0);
				requestedSize += newRequestedSize;
			}

			return totalRead;
		}

		/*
		 * This will operate on the internal buffers and never block.
		 */
		protected abstract AsyncOperationStatus Run (AsyncOperationStatus status);

		public override string ToString ()
		{
			return string.Format ("[{0}]", Name);
		}
	}

	class AsyncHandshakeRequest : AsyncProtocolRequest
	{
		public AsyncHandshakeRequest (MobileAuthenticatedStream parent, bool sync)
			: base (parent, sync)
		{
		}

		protected override AsyncOperationStatus Run (AsyncOperationStatus status)
		{
			return Parent.ProcessHandshake (status, false);
		}
	}

	abstract class AsyncReadOrWriteRequest : AsyncProtocolRequest
	{
		protected BufferOffsetSize UserBuffer {
			get;
		}

		protected int CurrentSize {
			get; set;
		}

		public AsyncReadOrWriteRequest (MobileAuthenticatedStream parent, bool sync, byte[] buffer, int offset, int size)
			: base (parent, sync)
		{
			UserBuffer = new BufferOffsetSize (buffer, offset, size);
		}

		public override string ToString ()
		{
			return string.Format ("[{0}: {1}]", Name, UserBuffer);
		}
	}

	class AsyncReadRequest : AsyncReadOrWriteRequest
	{
		public AsyncReadRequest (MobileAuthenticatedStream parent, bool sync, byte[] buffer, int offset, int size)
			: base (parent, sync, buffer, offset, size)
		{
		}

		protected override AsyncOperationStatus Run (AsyncOperationStatus status)
		{
			Debug ("ProcessRead - read user: {0} {1}", this, status);

			var (ret, wantMore) = Parent.ProcessRead (UserBuffer);

			Debug ("ProcessRead - read user done: {0} - {1} {2}", this, ret, wantMore);

			if (ret < 0) {
				UserResult = -1;
				return AsyncOperationStatus.Complete;
			}

			CurrentSize += ret;
			UserBuffer.Offset += ret;
			UserBuffer.Size -= ret;

			Debug ("Process Read - read user done #1: {0} - {1} {2}", this, CurrentSize, wantMore);

			if (wantMore && CurrentSize == 0)
				return AsyncOperationStatus.Continue;

			UserResult = CurrentSize;
			return AsyncOperationStatus.Complete;
		}
	}

	class AsyncWriteRequest : AsyncReadOrWriteRequest
	{
		public AsyncWriteRequest (MobileAuthenticatedStream parent, bool sync, byte[] buffer, int offset, int size)
			: base (parent, sync, buffer, offset, size)
		{
		}

		protected override AsyncOperationStatus Run (AsyncOperationStatus status)
		{
			Debug ("ProcessWrite - write user: {0} {1}", this, status);

			if (UserBuffer.Size == 0) {
				UserResult = CurrentSize;
				return AsyncOperationStatus.Complete;
			}

			var (ret, wantMore) = Parent.ProcessWrite (UserBuffer);

			Debug ("ProcessWrite - write user done: {0} - {1} {2}", this, ret, wantMore);

			if (ret < 0) {
				UserResult = -1;
				return AsyncOperationStatus.Complete;
			}

			CurrentSize += ret;
			UserBuffer.Offset += ret;
			UserBuffer.Size -= ret;

			if (wantMore)
				return AsyncOperationStatus.Continue;

			UserResult = CurrentSize;
			return AsyncOperationStatus.Complete;
		}
	}

	class AsyncShutdownRequest : AsyncProtocolRequest
	{
		public AsyncShutdownRequest (MobileAuthenticatedStream parent)
			: base (parent, false)
		{
		}

		protected override AsyncOperationStatus Run (AsyncOperationStatus status)
		{
			return Parent.ProcessShutdown (status);
		}
	}

	class AsyncRenegotiateRequest : AsyncProtocolRequest
	{
		public AsyncRenegotiateRequest (MobileAuthenticatedStream parent)
			: base (parent, false)
		{
		}

		protected override AsyncOperationStatus Run (AsyncOperationStatus status)
		{
			return Parent.ProcessHandshake (status, true);
		}
	}
}
#endif
