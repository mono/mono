// 
// System.Web.Services.Protocols.WebClientAsyncResult.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Threading;

namespace System.Web.Services.Protocols {
	public class WebClientAsyncResult : IAsyncResult {

		#region Fields

		object asyncState;
		WaitHandle waitHandle;

		#endregion // Fields

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

		[MonoTODO]
		public void Abort ()
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
