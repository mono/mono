//
// System.Data.Odbc.OdbcInfoMessageEventArgs
//
// Author:
//   Umadevi S (sumadevi@novell.com)
//
// Copyright (C) Novell Inc, 2004
//

using System.Data;
using System.Data.Common;

namespace System.Data.Odbc
{
	public sealed class OdbcInfoMessageEventArgs : EventArgs 
	{
		#region Constructors

		internal OdbcInfoMessageEventArgs() {
		}

		#endregion Constructors

		#region Properties


		public OdbcErrorCollection Errors {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public string Message {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
