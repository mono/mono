//
// System.Data.OleDb.OleDbInfoMessageEventArgs
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Rodrigo Moya, 2002
// Copyright (C) Tim Coleman, 2002
//

using System.Data;
using System.Data.Common;

namespace System.Data.OleDb
{
	public sealed class OleDbInfoMessageEventArgs : EventArgs
	{
		#region Properties

		public int ErrorCode {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public OleDbErrorCollection Errors {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public string Message {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public string Source {
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
