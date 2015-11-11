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
	public delegate object FrameworkValueGetter (object rawValue, ref bool updated);

	public class TdsMetaParameter
	{
		#region Static 
		public const int maxVarCharCharacters =  2147483647; // According to MS, max size is 2GB, 1 Byte Characters
		public const int maxNVarCharCharacters = 1073741823; // According to MS, max size is 2GB, 2 Byte Characters
		#endregion

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
		FrameworkValueGetter frameworkValueGetter;
		object rawValue;
		bool isUpdated;

		#endregion // Fields

		public TdsMetaParameter (string name, object value)
			: this (name, String.Empty, value)
		{
		}

		public TdsMetaParameter (string name, FrameworkValueGetter valueGetter)
			: this (name, String.Empty, null)
		{
			frameworkValueGetter = valueGetter;
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

		public TdsMetaParameter (string name, int size, bool isNullable, byte precision, byte scale, FrameworkValueGetter valueGetter)
		{
			ParameterName = name;
			Size = size;
			IsNullable = isNullable;
			Precision = precision;
			Scale = scale;
			frameworkValueGetter = valueGetter;
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
			get {
				if (frameworkValueGetter != null) {
					object newValue = frameworkValueGetter (rawValue, ref isUpdated);
					if (isUpdated)
						value = newValue;
				}

				if (isUpdated) {
					value = ResizeValue (value);
					isUpdated = false;
				}
				return value;
			}
			set {
				rawValue = this.value = value;
				isUpdated = true;
			}
		}

		public object RawValue {
			get { return rawValue; }
			set { Value = value; }
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
				isUpdated = true;
				isSizeSet = true;
			}
		}

		public bool IsVariableSizeType
		{
			get { return isVariableSizeType; }
			set { isVariableSizeType = value; }
		}

		public bool IsVarNVarCharMax
		{
			get { return (TypeName == "ntext" && size >= maxNVarCharCharacters); }
		}

		public bool IsVarCharMax
		{
			get { return (TypeName == "text" && size >= maxVarCharCharacters); }
		}

		public bool IsAnyVarCharMax
		{
			get { return IsVarNVarCharMax || IsVarCharMax; }
		}

		public bool IsNonUnicodeText
		{
			get {
				TdsColumnType colType = GetMetaType();
				return (colType == TdsColumnType.VarChar ||
				        colType == TdsColumnType.BigVarChar ||
				        colType == TdsColumnType.Text ||
				        colType == TdsColumnType.Char ||
				        colType == TdsColumnType.BigChar);
			}
		}

		public bool IsMoneyType
		{
			get {
				TdsColumnType colType = GetMetaType();
				return (colType == TdsColumnType.Money ||
				        colType == TdsColumnType.MoneyN ||
				        colType == TdsColumnType.Money4 ||
				        colType == TdsColumnType.SmallMoney);
			}
		}

		public bool IsDateTimeType
		{
			get {
				TdsColumnType colType = GetMetaType();
				return (colType == TdsColumnType.DateTime ||
				        colType == TdsColumnType.DateTime4 ||
				        colType == TdsColumnType.DateTimeN);
			}
		}

		public bool IsTextType
		{
			get {
				TdsColumnType colType = GetMetaType();
				return (colType == TdsColumnType.VarChar ||
				        colType == TdsColumnType.BigVarChar ||
				        colType == TdsColumnType.BigChar ||
				        colType == TdsColumnType.Char ||
				        colType == TdsColumnType.BigNVarChar || 
				        colType == TdsColumnType.NChar ||
				        colType == TdsColumnType.Text ||
				        colType == TdsColumnType.NText);
			}
		}

		public bool IsDecimalType
		{
			get {
				TdsColumnType colType = GetMetaType();
				return (colType == TdsColumnType.Decimal ||
				        colType == TdsColumnType.Numeric);
			}
		}

		#endregion // Properties

		#region Methods

		object ResizeValue (object newValue)
		{
			if (newValue == DBNull.Value || newValue == null)
				return newValue;

			if (!isSizeSet || size <= 0)
				return newValue;

			// if size is set, truncate the value to specified size
			string text = newValue as string;
			if (text != null) {
				if (TypeName == "nvarchar" || 
				    TypeName == "nchar" ||
				    TypeName == "xml") {
					if (text.Length > size)
						return text.Substring (0, size);
				}
			} else if (newValue.GetType () == typeof (byte [])) {
				byte [] buffer = (byte []) newValue;
				if (buffer.Length > size) {
					byte [] tmpVal = new byte [size];
					Array.Copy (buffer, tmpVal, size);
					return tmpVal;
				}
			}
			return newValue;
		}

		internal string Prepare ()
		{
			string typeName = TypeName;
			// Cf. GetDateTimeString
			TdsColumnType actualType = TdsColumnType.Char;
			int size;

			switch (typeName) {
			case "varbinary":
				size = Size;
				if (size <= 0) {
					size = GetActualSize ();
				}

				if (size > 8000) {
					typeName = "varbinary(max)";
				}
				break;
			case "datetime2":
				actualType = TdsColumnType.DateTime2;
				typeName = "char";
				break;
			case "datetimeoffset":
				actualType = TdsColumnType.DateTimeOffset;
				typeName = "char";
				break;
			}

			string includeAt = "@";
			if (ParameterName [0] == '@')
				includeAt = "";
			StringBuilder result = new StringBuilder (String.Format ("{0}{1} {2}", includeAt, ParameterName, typeName));
			switch (typeName) {
			case "decimal":
			case "numeric":
				// msdotnet sends a default precision of 29
				result.Append (String.Format ("({0},{1})",
					 (Precision == (byte)0 ? (byte)38 : Precision), Scale));
				break;
			case "varchar":
			case "varbinary":
				//A size of 0 is not allowed in declarations.
				size = Size;
				if (size <= 0) {
					size = GetActualSize ();
					if (size <= 0)
						size = 1;
				}
				result.Append (size > 8000 ? "(max)" : String.Format ("({0})", size));
				break;
			case "nvarchar":
				int paramSize = Size < 0 ? GetActualSize () / 2 : Size;
				result.Append (paramSize > 0 ? (paramSize > 4000 ? "(max)" : String.Format ("({0})", paramSize)) : "(4000)");
				break;
			case "char":
				size = -1;
				if (actualType != TdsColumnType.Char)
					size = GetDateTimeStringLength (actualType);
				else if (isSizeSet)
					size = Size;
				if (size > 0)
					result.Append (String.Format ("({0})", size));
				break;
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
				int len = ((string)value).Length;
				if (TypeName == "nvarchar" || TypeName == "nchar" 
				    || TypeName == "ntext"
				    || TypeName == "xml")
					len *= 2;
				return len ;	
			case "System.Byte[]":
				return ((byte[]) value).Length;
			}
			return GetSize ();
		}

		private int GetSize ()
		{
			switch (TypeName) {
			case "decimal":
				return 17;
			case "uniqueidentifier":
				return 16;
			case "bigint":
			case "datetime":
			case "float":
			case "money":
				return 8;
			case "datetime2":
				return GetDateTimeStringLength (TdsColumnType.DateTime2);
			case "datetimeoffset":
				return GetDateTimeStringLength (TdsColumnType.DateTimeOffset);
			case "int":
			case "real":
			case "smalldatetime":
			case "smallmoney":
				return 4;
			case "smallint":
				return 2;
			case "tinyint":
			case "bit":
				return 1;
			/*
			case "nvarchar" :
			*/
			case "nchar" :
			case "ntext" :
				return size*2 ;
			}
			return size;
		}

		private int GetDateTimePrecision ()
		{
			int precision = Precision;

			// http://msdn.microsoft.com/en-us/library/bb677335.aspx
			// says that default precision is 7.  How do
			// we distinguish that from zero?
			if (precision == 0 || precision > 7)
				precision = 7;

			return precision;
		}

		private int GetDateTimeStringLength (TdsColumnType type)
		{
			int precision = GetDateTimePrecision ();
			int len = precision == 0 ? 19 : 20 + precision;

			if (type == TdsColumnType.DateTimeOffset)
				len += 6;

			return len;
		}

		// HACK: Wire-level DateTime{2,Offset} require TDS
		// 7.3, which this driver does not implement
		// correctly--so we serialize to ASCII instead.
		private string GetDateTimeString (TdsColumnType type)
		{
			int precision = GetDateTimePrecision ();
			string fmt = "yyyy-MM-dd'T'HH':'mm':'ss";

			if (precision > 0)
				fmt += ".fffffff".Substring(0, precision + 1);

			switch (type) {
			case TdsColumnType.DateTime2:
				DateTime dt = (DateTime)Value;
				return dt.ToString(fmt);
			case TdsColumnType.DateTimeOffset:
				DateTimeOffset dto = (DateTimeOffset)Value;
				return dto.ToString(fmt + "zzz");
			}

			throw new ApplicationException("Should be unreachable");
		}

		internal byte[] GetBytes ()
		{
			byte[] result = {};
			if (Value == DBNull.Value || Value == null)
				return result;

			switch (TypeName)
			{
				case "nvarchar" :
				case "nchar" :
				case "ntext" :
				case "xml" :
					return Encoding.Unicode.GetBytes ((string)Value);
				case "varchar" :
				case "char" :
				case "text" :
					return Encoding.Default.GetBytes ((string)Value);
				case "datetime2":
					return Encoding.Default.GetBytes (GetDateTimeString (TdsColumnType.DateTime2));
				case "datetimeoffset":
					return Encoding.Default.GetBytes (GetDateTimeString (TdsColumnType.DateTimeOffset));
				default :
					return ((byte[]) Value);
			}
		}

		internal TdsColumnType GetMetaType ()
		{
			switch (TypeName) {
			case "binary":
				return TdsColumnType.BigBinary;
			case "bit":
				if (IsNullable)
					return TdsColumnType.BitN;
				return TdsColumnType.Bit;
			case "bigint":
				if (IsNullable)
					return TdsColumnType.IntN ;
				return TdsColumnType.BigInt;
			case "char":
				return TdsColumnType.BigChar;
			case "money":
				if (IsNullable)
					return TdsColumnType.MoneyN;
				return TdsColumnType.Money;
			case "smallmoney":
				if (IsNullable)
					return TdsColumnType.MoneyN ;
				return TdsColumnType.SmallMoney;
			case "decimal":
				return TdsColumnType.Decimal;
			case "datetime":
				if (IsNullable)
					return TdsColumnType.DateTimeN;
				return TdsColumnType.DateTime;
			case "smalldatetime":
				if (IsNullable)
					return TdsColumnType.DateTimeN;
				return TdsColumnType.DateTime4;
			case "datetime2":
				return TdsColumnType.DateTime2;
			case "datetimeoffset":
				return TdsColumnType.DateTimeOffset;
			case "float":
				if (IsNullable)
					return TdsColumnType.FloatN ;
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
			case "xml":
			case "nvarchar":
				return TdsColumnType.BigNVarChar;
			case "real":
				if (IsNullable)
					return TdsColumnType.FloatN ;
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
				return TdsColumnType.BigVarBinary;
			case "varchar":
				return TdsColumnType.BigVarChar;
			case "sql_variant":
				return TdsColumnType.Variant;
			default:
				throw new NotSupportedException ("Unknown Type : " + TypeName);
			}
		}

		public void CalculateIsVariableType()
		{
			switch (GetMetaType ()) {
				case TdsColumnType.UniqueIdentifier:
				case TdsColumnType.BigVarChar:
				case TdsColumnType.BigVarBinary:
				case TdsColumnType.IntN:
				case TdsColumnType.Text:
				case TdsColumnType.FloatN:
				case TdsColumnType.BigNVarChar:
				case TdsColumnType.NText:
				case TdsColumnType.Image:
				case TdsColumnType.Decimal:
				case TdsColumnType.BigBinary:
				case TdsColumnType.DateTimeN:
				case TdsColumnType.MoneyN:
				case TdsColumnType.BitN:
				case TdsColumnType.Char:
				case TdsColumnType.BigChar:
				case TdsColumnType.NChar:
					IsVariableSizeType = true;
					break;
				default:
					IsVariableSizeType = false;
				break;
			}
		}

		public void Validate (int index)
		{
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
