//
// Mono.Data.SybaseClient.SqlInfoMessageEventArgs.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Data;

namespace Mono.Data.SybaseClient {
	public sealed class SybaseInfoMessageEventArgs : EventArgs
	{
		#region Properties

		[MonoTODO]
		public SybaseErrorCollection Errors {
			get { throw new NotImplementedException (); }
		}	

		[MonoTODO]
		public string Message {
			get { throw new NotImplementedException (); }
		}	

		[MonoTODO]
		public string Source {
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override string ToString () 
		{
			// representation of InfoMessage event
			return "'ToString() for SybaseInfoMessageEventArgs Not Implemented'";
		}

		#endregion // Methods
	}
}
