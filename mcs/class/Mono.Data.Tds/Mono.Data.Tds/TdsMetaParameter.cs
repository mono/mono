//
// Mono.Data.Tds.TdsMetaParameter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using Mono.Data.Tds.Protocol;
using System;
using System.Text;

namespace Mono.Data.Tds {
	public class TdsMetaParameter
	{
		#region Fields

		TdsParameterDirection direction = TdsParameterDirection.Input;
		byte precision;
		byte scale;
		int size;
		string typeName;
		string name;
		bool isSizeSet = false;
		bool isNullable;
		object value;

		#endregion // Fields

		public TdsMetaParameter (string name, object value)
			: this (name, String.Empty, value)
		{
		}

		public TdsMetaParameter (string name, string typeName, object value)
		{
			ParameterName = name;
			Value = value;
			TypeName = typeName;
			IsNullable = false;
		}

		public TdsMetaParameter (string name, int size, bool isNullable, byte precision, byte scale, object value)
		{
			ParameterName = name;
			Size = size;
			IsNullable = isNullable;
			Precision = precision;
			Scale = scale;
			Value = value;
		}

		#region Properties

		public TdsParameterDirection Direction {
			get { return direction; }
			set { direction = value; }
		}

		public string TypeName {
			get { return typeName; }
			set { typeName = value; }
		}

		public string ParameterName {
			get { return name; }
			set { name = value; }
		}

		public bool IsNullable {
			get { return isNullable; }
			set { isNullable = value; }
		}

		public object Value {
			get { return value; }
			set { this.value = value; }
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
			get { return GetSize (); }
			set {
				size = value; 
				isSizeSet = true;
			}
		}

		#endregion // Properties

		#region Methods

		internal string Prepare ()
		{
			StringBuilder result = new StringBuilder (String.Format ("{0} {1}", ParameterName, TypeName));
			switch (TypeName) {
			case "decimal":
			case "numeric":
				result.Append (String.Format ("({0},{1})", Precision, Scale));
				break;
			case "varchar":
			case "nvarchar":
			case "varbinary":
				result.Append (String.Format ("({0})", Size));
				break;
			case "char":
			case "nchar":
			case "binary":
				if (isSizeSet && Size > 0)
					result.Append (String.Format ("({0})", Size));
				break;
			}
			return result.ToString ();
		}

		internal int GetActualSize ()
		{
			if (Value == DBNull.Value || Value == null)
				return 0;

			switch (Value.GetType ().ToString ()) {
			case "System.String":
				return ((string) value).Length;
			case "System.Byte[]":
				return ((byte[]) value).Length;
			}
			return GetSize ();
		}

		private int GetSize ()
		{
			if (IsNullable) {
				switch (TypeName) {
				case "bigint":
					return 8;
				case "datetime":
					return 8;
				case "float":
					return 8;
				case "int":
					return 4;
				case "real":
					return 4;
				case "smalldatetime":
					return 4;
				case "smallint":
					return 2;
				case "tinyint":
					return 1;
				}
			}
			return size;
		}

		internal TdsColumnType GetMetaType ()
		{
			switch (TypeName) {
			case "binary":
				return TdsColumnType.Binary;
			case "bit":
				return TdsColumnType.Bit;
			case "char":
				return TdsColumnType.Char;
			case "decimal":
				return TdsColumnType.Decimal;
			case "datetime":
				if (IsNullable)
					return TdsColumnType.DateTimeN;
				return TdsColumnType.DateTime;
			case "float":
				return TdsColumnType.Float8;
			case "image":
				return TdsColumnType.Image;
			case "int":
				if (IsNullable)
					return TdsColumnType.IntN;
				return TdsColumnType.Int4;
			case "numeric":
				return TdsColumnType.Numeric;
			case "nchar":
				return TdsColumnType.NChar;
			case "ntext":
				return TdsColumnType.NText;
			case "nvarchar":
				return TdsColumnType.NVarChar;
			case "real":
				return TdsColumnType.Real;
			case "smallint":
				if (IsNullable)
					return TdsColumnType.IntN;
				return TdsColumnType.Int2;
			case "text":
				return TdsColumnType.Text;
			case "tinyint":
				if (IsNullable)
					return TdsColumnType.IntN;
				return TdsColumnType.Int1;
			case "uniqueidentifier":
				return TdsColumnType.UniqueIdentifier;
			case "varbinary":
				return TdsColumnType.VarBinary;
			case "varchar":
				return TdsColumnType.VarChar;
			default:
				throw new NotSupportedException ();
			}
		}

		#endregion // Methods
	}
}
