//
// System.Data.Common.DbDataAdapter.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc
//

namespace System.Data.Common
{
	/// <summary>
	/// Provides data for the RowUpdated event of a .NET data provider.
	/// </summary>
	public abstract class RowUpdatedEventArgs : EventArgs
	{
		[MonoTODO]
		protected RowUpdatedEventArgs(DataRow dataRow,
					      IDbCommand command,
					      StatementType statementType,
					      DataTableMapping tableMapping) {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public IDbCommand Command {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public Exception Errors {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public int RecordsAffected {
			get { throw new NotImplementedException (); }
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
