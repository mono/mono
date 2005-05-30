//
// System.Data.OleDb.OleDbDataReader
//
// Author:
//   Boris Kirzner (borisk@mainsoft.com)
//

using System.Data.Common;
using System.Data.ProviderBase;

using java.sql;

namespace System.Data.OleDb
{
	public sealed class OleDbDataReader : AbstractDataReader
	{
		#region Fields

		#endregion // Fields

		#region Constructors

		internal OleDbDataReader(OleDbCommand command) : base(command)
		{
		}

		#endregion // Constructors

		#region Methods

		protected override SystemException CreateException(string message, SQLException e)
		{
			return new OleDbException(message,e, (OleDbConnection)_command.Connection);		
		}

		protected override SystemException CreateException(java.io.IOException e)
		{
			return new OleDbException(e, (OleDbConnection)_command.Connection);		
		}

		public override String GetDataTypeName(int columnIndex)
		{
			try {
				string jdbcTypeName = Results.getMetaData().getColumnTypeName(columnIndex + 1);
				
				return OleDbConvert.JdbcTypeNameToDbTypeName(jdbcTypeName);
			}
			catch (SQLException e) {
				throw CreateException(e);
			}
		}

		protected override int GetProviderType(int jdbcType)
		{
			return (int)OleDbConvert.JdbcTypeToOleDbType(jdbcType);   
		}

		#endregion // Methods
	}
}