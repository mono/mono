//
// System.Data.SqlClient.SqlDataReader.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc
//

namespace System.Data.SqlClient
{
	/// <summary>
	/// Provides a means of reading one or more forward-only streams of result sets obtained by executing a command at a SQL database.
	/// </summary>
	public interface SqlDataReader : IDataReader
	{
		[MonoTODO]
		void Close()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		DataTable GetSchemaTable()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		bool NextResult()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		bool Read()
	        {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		int Depth
		{
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		bool IsClosed
		{
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		int RecordsAffected
		{
			get { throw new NotImplementedException (); }
		}


	}
}
