//
// CommittableTransaction.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2005 Novell Inc,
//

#if NET_2_0
using System.Runtime.Serialization;
using System.Threading;

namespace System.Transactions
{
	[Serializable]
	public sealed class CommittableTransaction : Transaction,
		ISerializable, IDisposable, IAsyncResult
	{

		TransactionOptions options;
		AsyncCallback callback;
		object user_defined_state;
		bool committing;
		bool completed;

		public CommittableTransaction ()
			: this (new TransactionOptions ())
		{
		}

		public CommittableTransaction (TimeSpan timeout)
		{
			options = new TransactionOptions ();
			options.Timeout = timeout;
		}

		public CommittableTransaction (TransactionOptions options)
		{
			this.options = options;
		}

		[MonoTODO]
		public IAsyncResult BeginCommit (AsyncCallback callback,
			object user_defined_state)
		{
			if (committing)
				throw new InvalidOperationException ();
			this.committing = true;
			this.callback = callback;
			this.user_defined_state = user_defined_state;
			// FIXME: invoke another thread and set WaitHandle.
			return this;
		}

		public void Commit ()
		{
			EndCommit (BeginCommit (null, null));
		}

		[MonoTODO]
		public void EndCommit (IAsyncResult asyncResult)
		{
			if (asyncResult != this)
				throw new InvalidOperationException ();
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void ISerializable.GetObjectData (SerializationInfo info,
			StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		object IAsyncResult.AsyncState {
			get { return user_defined_state; }
		}

		[MonoTODO]
		WaitHandle IAsyncResult.AsyncWaitHandle {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		bool IAsyncResult.CompletedSynchronously {
			get { throw new NotImplementedException (); }
		}

		bool IAsyncResult.IsCompleted {
			get { return completed; }
		}
	}
}

#endif
