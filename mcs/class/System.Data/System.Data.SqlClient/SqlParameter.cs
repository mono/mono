//
// System.Data.SqlClient.SqlParameter.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc.
//

namespace System.Data.SqlClient
{
	/// <summary>
	/// Represents a parameter to a Command object, and optionally, its mapping to DataSet columns; and is implemented by .NET data providers that access data sources.
	/// </summary>
	public class SqlParameter : IDbDataParameter, IDataParameter
	{
		[MonoTODO]
		DbType DbType
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		ParameterDirection Direction
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		bool IsNullable
		{
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		string ParameterName
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		string SourceColumn
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		DataRowVersion SourceVersion
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		object Value
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		byte Precision
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
                byte Scale
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
                int Size
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

	}
}
