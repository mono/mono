//
// System.Data.SqlClient.SqlCommand.cs
//
// Author:
//   Rodrigo Moya (rdorigo@ximian.com)
//
// (C) Ximian, Inc
//

namespace System.Data.SqlClient
{
	/// <summary>
	/// Represents a SQL statement that is executed while connected to a SQL database.
	/// </summary>
	public class SqlCommand : IDbCommand
	{
		[MonoTODO]
		void Cancel()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		SqlParameter CreateParameter()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		int ExecuteNonQuery()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		SqlDataReader ExecuteReader()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		SqlDataReader ExecuteReader(CommandBehavior behavior)
		{
		}

		[MonoTODO]
		object ExecuteScalar()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void Prepare()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		string CommandText
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		int CommandTimeout
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		CommandType CommandType
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		SqlConnection Connection
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		SqlParameterCollection Parameters
		{
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		IDbTransaction Transaction
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		UpdateRowSource UpdatedRowSource
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	}
}
