//
// System.Data.StateChangeEventArgs.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc 2002
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace System.Data {
	public sealed class StateChangeEventArgs : EventArgs 
	{
		#region Fields

		ConnectionState originalState;
		ConnectionState currentState;

		#endregion // Fields

		#region Constructors

		public StateChangeEventArgs (ConnectionState originalState, ConnectionState currentState)
		{
			this.originalState = originalState;
			this.currentState = currentState;
		}

		#endregion // Constructors

		#region Properties

		public ConnectionState CurrentState {
			get { return currentState; }
		}

		public ConnectionState OriginalState {
			get { return originalState; }
		}

		#endregion // Properties
	}
}
