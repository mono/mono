//
// System.Data.Common.RowUpdatingEventArgs.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc
//

namespace System.Data.Common
{
	/// <summary>
	/// Provides the data for the RowUpdating event of a .NET data provider.
	/// </summary>
	public abstract class RowUpdatingEventArgs : EventArgs
	{
		[MonoTODO]
		protected RowUpdatingEventArgs(DataRow dataRow,
					       IDbCommand command,
					       StatementType statementType,
					       DataTableMapping tableMapping) {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public IDbCommand Command {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public Exception Errors {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public DataRow Row {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public StatementType StatementType {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public UpdateStatus Status {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public DataTableMapping TableMapping {
			get { throw new NotImplementedException (); }
		}
	}
}
