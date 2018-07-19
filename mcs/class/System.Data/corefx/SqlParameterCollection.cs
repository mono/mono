// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Data.Common;

namespace System.Data.SqlClient
{
	partial class SqlParameterCollection : 
		System.Collections.ICollection, 
		System.Collections.IEnumerable, 
		System.Collections.IList, 
		System.Data.IDataParameterCollection
	{
		public SqlParameter Add(string parameterName, object value)
			=> Add(new SqlParameter(parameterName, value));
	}
}