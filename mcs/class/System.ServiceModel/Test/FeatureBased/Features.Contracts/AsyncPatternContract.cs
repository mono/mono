using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using System.Threading;

namespace MonoTests.Features.Contracts
{
	// Define a service contract.
	[ServiceContract (Namespace = "http://MonoTests.Features.Contracts")]
	public interface IAsyncPattern
	{
		[OperationContractAttribute (AsyncPattern = true)]
		IAsyncResult BeginAsyncMethod (AsyncCallback callback, object asyncState);
		int EndAsyncMethod (IAsyncResult result);

		// TODO: Need to investigate asyn methods that have ref/out params that are not necessarily first
		// e.g. how does foo(in, ref, out, in) map to AsyncPattern?

	}

	public class AsyncPatternServer : IAsyncPattern
	{
		// Simple async result implementation.
		class CompletedAsyncResult<T> : IAsyncResult
		{
			T data;
			object state;

			public CompletedAsyncResult (T data, object state) { this.data = data; this.state = state; }

			public T Data { get { return data; } }

			#region IAsyncResult Members
			public object AsyncState { get { return (object) state; } }

			public WaitHandle AsyncWaitHandle { get { throw new Exception ("The method or operation is not implemented."); } }

			public bool CompletedSynchronously { get { return true; } }

			public bool IsCompleted { get { return true; } }
			#endregion
		}

		public IAsyncResult BeginAsyncMethod (AsyncCallback callback, object asyncState) {
			IAsyncResult result = new CompletedAsyncResult<int> (3, asyncState);
			new Thread (new ThreadStart (
				delegate {
					callback (result);
				})).Start ();
			return result;
		}

		public int EndAsyncMethod (IAsyncResult r) {
			return ((CompletedAsyncResult<int>) r).Data;
		}
	}
}
