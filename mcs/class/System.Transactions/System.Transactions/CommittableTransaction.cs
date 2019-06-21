//
// CommittableTransaction.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//	Ankit Jain	 <JAnkit@novell.com>
//
// (C)2005 Novell Inc,
// (C)2006 Novell Inc,
//

using System.Runtime.Serialization;
using System.Threading;

namespace System.Transactions
{
	[Serializable]
	public sealed class CommittableTransaction : Transaction,
		ISerializable, IDisposable, System.IAsyncResult
	{
		TransactionOptions options;

		AsyncCallback callback;
		object user_defined_state;

		IAsyncResult asyncResult;

		public CommittableTransaction ()
			: this (new TransactionOptions ())
		{
		}

		public CommittableTransaction (TimeSpan timeout)
			: base (IsolationLevel.Serializable)
		{
			options = new TransactionOptions ();
			options.Timeout = timeout;
		}

		public CommittableTransaction (TransactionOptions options)
			: base (options.IsolationLevel)
		{
			this.options = options;
		}

		public IAsyncResult BeginCommit (AsyncCallback asyncCallback,
			object asyncState)
		{
			this.callback = asyncCallback;
			this.user_defined_state = asyncState;

			AsyncCallback cb = null;
			if (asyncCallback != null)
				cb = new AsyncCallback (CommitCallback);

			asyncResult = BeginCommitInternal (cb);
			return this;
		}
		
		public void EndCommit (IAsyncResult asyncResult)
		{
			if (asyncResult != this)
				throw new ArgumentException ("The IAsyncResult parameter must be the same parameter as returned by BeginCommit.", "asyncResult");

			EndCommitInternal (this.asyncResult);
		}

		private void CommitCallback (IAsyncResult ar)
		{
			if (asyncResult == null && ar.CompletedSynchronously)
				asyncResult = ar;
			callback (this);
		}

		public void Commit ()
		{
			CommitInternal ();
		}
		
		[MonoTODO ("Not implemented")]
		void ISerializable.GetObjectData (SerializationInfo info,
			StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		object IAsyncResult.AsyncState {
			get { return user_defined_state; }
		}

		WaitHandle IAsyncResult.AsyncWaitHandle {
			get { return asyncResult.AsyncWaitHandle; }
		}

		bool IAsyncResult.CompletedSynchronously {
			get { return asyncResult.CompletedSynchronously; }
		}

		bool IAsyncResult.IsCompleted {
			get { return asyncResult.IsCompleted; }
		}


	}
}

