//
// System.Data.SqlClient.SqlParameterCollection.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc
//

using System.Collections;

namespace System.Data.SqlClient
{
	/// <summary>
	/// Collects all parameters relevant to a Command object and their mappings to DataSet columns.
	/// </summary>
	public class SqlParameterCollection : IDataParameterCollection
	{
		void RemoveAt(string parameterName);
		
		int IndexOf(string parameterName);
		
		bool Contains(string parameterName);

		object this[string parameterName]{get; set;}
	}
}
