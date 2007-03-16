//
// Mono.Data.Tds.TdsMetaParameter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
		bool isVariableSizeType;

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
			get { 
				if (TypeName == "decimal" || TypeName == "numeric") {
					if (scale == 0 && !Convert.IsDBNull(Value)) {
						int[] arr = Decimal.GetBits (
								Convert.ToDecimal(Value));
						scale = (byte)((arr[3]>>16) & (int)0xFF);
					}
				}
				return scale;
			}
			set { scale = value; }
		}

		public int Size {
			get { return GetSize (); }
			set {
				size = value; 
				isSizeSet = true;
			}
		}

		public bool IsVariableSizeType
		{
			get { return isVariableSizeType; }
			set { isVariableSizeType = value; }
		}

		#endregion // Properties

		#region Methods

		internal string Prepare ()
		{
			string typeName = TypeName;
			
			if (typeName == "varbinary") {
				int size = Size;
				if (size <= 0) {
					size = GetActualSize ();
				}
				
				if (size > 8000) {
					typeName = "image";
				}
			}
			
			StringBuilder result = new StringBuilder (String.Format ("{0} {1}", ParameterName, typeName));
			switch (typeName) {
			case "decimal":
			case "numeric":
				// msdotnet sends a default precision of 28
				result.Append (String.Format ("({0},{1})",
					 (Precision == (byte)0 ? (byte)28 : Precision), Scale));
				break;
			case "varchar":
			case "varbinary":
				//A size of 0 is not allowed in declarations.
				int size = Size;
				if (size <= 0) {
					size = GetActualSize ();
					if (size <= 0)
						size = 1;
				}
				result.Append (String.Format ("({0})", size));
				break;
			case "nvarchar":
				result.Append (String.Format ("({0})", Size > 0 ? Size : 4000));
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

		public void Validate (int index)
		{
			Console.WriteLine ("\r\n:{0}: :{1}: :{2}: :{3}:\r\n", this.direction, this.isVariableSizeType, Value, Size);
			if ((this.direction == TdsParameterDirection.InputOutput || this.direction == TdsParameterDirection.Output) &&
				 this.isVariableSizeType && (Value == DBNull.Value || Value == null) && Size == 0
				) 
			{
				throw new InvalidOperationException (String.Format ("{0}[{1}]: the Size property should " +
												"not be of size 0",
												this.typeName,
												index));
			}
		}

		#endregion // Methods
	}
}
