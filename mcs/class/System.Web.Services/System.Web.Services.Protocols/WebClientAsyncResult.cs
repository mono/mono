// 
// System.Web.Services.Protocols.WebClientAsyncResult.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Net;
using System.Threading;

namespace System.Web.Services.Protocols {
	public class WebClientAsyncResult : IAsyncResult {

		#region Fields

		WaitHandle waitHandle;

		WebClientProtocol protocol;
		WebRequest request;
		AsyncCallback callback;
		object asyncState;

		#endregion // Fields

		#region Constructors 

		[MonoTODO ("Figure out what this does.")]
		internal WebClientAsyncResult (WebClientProtocol protocol, object x, WebRequest request, AsyncCallback callback, object asyncState, int y)
		{
			this.protocol = protocol;
			this.request = request;
			this.callback = callback; 
			this.asyncState = asyncState;
		}

		#endregion // Constructors

		#region Properties

		public object AsyncState {
			get { return asyncState; }
		}

		public WaitHandle AsyncWaitHandle {
			get { return waitHandle; }
		}

		public bool CompletedSynchronously {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public bool IsCompleted {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		public void Abort ()
		{
			request.Abort ();
		}

		#endregion // Methods
	}
}
