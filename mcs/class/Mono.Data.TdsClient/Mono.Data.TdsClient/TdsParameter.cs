//
// Mono.Data.TdsClient.TdsParameter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;

namespace Mono.Data.TdsClient {
	public sealed class TdsParameter : IDbDataParameter, IDataParameter
	{
		#region Fields

		DbType dbType;
		ParameterDirection direction;
		bool isNullable;
		int offset;
		string parameterName;
		object objectValue;
		string sourceColumn;
		DataRowVersion sourceVersion;
		byte precision;
		byte scale;
		int size;

		#endregion // Fields

		#region Constructors

		public TdsParameter ()
		{
		}

		public TdsParameter (string parameterName, object value)
		{
			this.parameterName = parameterName;
			this.objectValue = value;
		}

		#endregion // Constructors

		#region Properties

		public DbType DbType {
			get { return dbType; }
			set { dbType = value; }
		}

		public ParameterDirection Direction {
			get { return direction; }
			set { direction = value; }
		}

		public bool IsNullable {
			get { return isNullable; }
		}

		public int Offset {
			get { return offset; }
			set { offset = value; }
		}

		public string ParameterName {
			get { return parameterName; }
			set { parameterName = value; }
		}

		public byte Precision {
			get { return precision; }
			set { precision = value; }
		}

		public byte Scale {
			get { return scale; }
			set { scale = value; }
		}

		public int Size {
			get { return size; }
			set { size = value; }
		}

		public string SourceColumn {
			get { return sourceColumn; }
			set { sourceColumn = value; }
		}

		public DataRowVersion SourceVersion {
			get { return sourceVersion; }
			set { sourceVersion = value; }
		}

		public object Value {
			get { return objectValue; }
			set { objectValue = value; }
		}

		#endregion // Properties
	}
}

