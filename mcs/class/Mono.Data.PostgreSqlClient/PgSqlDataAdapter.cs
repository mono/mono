//
// System.Data.SqlClient.SqlDataAdapter.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc
//

namespace System.Data.SqlClient
{
	/// <summary>
	/// Represents a set of command-related properties that are used to fill the DataSet and update a data source, all this from a SQL database.
	/// </summary>
	public class SqlDataAdapter : IDbDataAdapter
	{
		[MonoTODO]
		SqlCommand DeleteCommand
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		SqlCommand InsertCommand
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		SqlCommand SelectCommand
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		SqlCommand UpdateCommand
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	}
}
