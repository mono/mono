//
// System.Data.SqlRowUpdatingEventArgs.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc 2002
//

using System;

namespace System.Data {
	public sealed class StateChangeEventArgs : EventArgs {

		[MonoTODO]
		public StateChangeEventArgs(ConnectionState originalState,
			ConnectionState currentState)	{
			// FIXME: do me
		}

		[MonoTODO]
		public ConnectionState CurrentState {
			get {
				// FIXME: do me
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public ConnectionState OriginalState {
			get {
				// FIXME: do me
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		~StateChangeEventArgs() {
			// FIXME: do me
		}
	}
}