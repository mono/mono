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

#if NET_2_0
using System.Runtime.Serialization;
using System.Threading;

namespace System.Transactions
{
#if !WINDOWS_PHONE && !NETFX_CORE
	[Serializable]
#endif
	public sealed class CommittableTransaction : Transaction,
#if !WINDOWS_PHONE && !NETFX_CORE
		ISerializable, 
#endif
		IDisposable, System.IAsyncResult
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
		{
			options = new TransactionOptions ();
			options.Timeout = timeout;
		}

		public CommittableTransaction (TransactionOptions options)
		{
			this.options = options;
		}

		public IAsyncResult BeginCommit (AsyncCallback callback,
			object user_defined_state)
		{
			this.callback = callback;
			this.user_defined_state = user_defined_state;

			AsyncCallback cb = null;
			if (callback != null)
				cb = new AsyncCallback (CommitCallback);

			asyncResult = BeginCommitInternal (cb);
			return this;
		}
		
		public void EndCommit (IAsyncResult ar)
		{
			if (ar != this)
				throw new ArgumentException ("The IAsyncResult parameter must be the same parameter as returned by BeginCommit.", "asyncResult");

			EndCommitInternal (asyncResult);
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
		
#if !WINDOWS_PHONE && !NETFX_CORE
		[MonoTODO ("Not implemented")]
		void ISerializable.GetObjectData (SerializationInfo info,
			StreamingContext context)
		{
			throw new NotImplementedException ();
		}
#endif

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

#endif
