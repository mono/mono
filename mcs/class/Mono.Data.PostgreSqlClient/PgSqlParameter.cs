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
		
		DbType DbType{get;set;}

		ParameterDirection Direction{get;set;}

		bool IsNullable{get;}

		string ParameterName{get;set;}

		string SourceColumn{get;set;}

		DataRowVersion SourceVersion {get;set;}

		object Value {get;set;}

		byte Precision{get; set;}

                byte Scale{get; set;}

                int Size{get; set;}

	}
}
