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
		[MonoTODO]
		void RemoveAt(string parameterName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		int IndexOf(string parameterName)
	        {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		bool Contains(string parameterName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		object this[string parameterName]
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	}
}
