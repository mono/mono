// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Data;
using System.Data.Common;

namespace System.Data.SqlClient
{
	partial class SqlParameter
	{
		public SqlParameter(string parameterName, SqlDbType dbType, int size, ParameterDirection direction, bool isNullable, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion, object value)
		{
			if (parameterName == null)
				parameterName = string.Empty;

			_isNull = isNullable;
			Value = value;
			Scale = scale;
			Size = size;
			Precision = precision;
			SqlDbType = dbType;
			Direction = direction;
			SourceColumn = sourceColumn;
			SourceVersion = sourceVersion;
		}

		[MonoTODO]
		public string UdtTypeName
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}
	}
}