
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System;

namespace System.Data.Odbc
{
	/// <summary>
	/// Summary description for OdbcColumn.
	/// </summary>
	internal class OdbcColumn
	{
		internal string ColumnName;
		internal OdbcType OdbcType;
                private SQL_TYPE _sqlType = SQL_TYPE.UNASSIGNED;
                private SQL_C_TYPE _sqlCType = SQL_C_TYPE.UNASSIGNED;
		internal bool AllowDBNull;
		internal int MaxLength;
		internal int Digits;
		internal object Value;

		internal OdbcColumn (string Name, OdbcType Type)
		{
			this.ColumnName = Name;
			this.OdbcType = Type;		
			AllowDBNull = false;
			MaxLength = 0;
			Digits = 0;
			Value = null;
		}

                internal OdbcColumn (string Name, SQL_TYPE type)
		{
                        this.ColumnName = Name;
			AllowDBNull = false;
			MaxLength = 0;
			Digits = 0;
			Value = null;
                        UpdateTypes (type);

		}


		internal Type DataType
		{
			get
			{
				switch (OdbcType)
				{
					case OdbcType.TinyInt:
						return typeof (System.Byte);
					case OdbcType.BigInt: 
						return typeof (System.Int64);
					case OdbcType.Image:
					case OdbcType.VarBinary:
					case OdbcType.Binary:
						return typeof (byte[]);
					case OdbcType.Bit:
						return typeof (bool);
					case OdbcType.NChar:
					case OdbcType.Char:
						return typeof (string);
					case OdbcType.Time:
						return typeof (TimeSpan);
					case OdbcType.Timestamp:
					case OdbcType.DateTime:
					case OdbcType.Date:
					case OdbcType.SmallDateTime:
						return typeof (DateTime);
					case OdbcType.Decimal:
					case OdbcType.Numeric:
						return typeof (Decimal);
					case OdbcType.Double:
						return typeof (Double);
					case OdbcType.Int:
						return typeof (System.Int32);
					case OdbcType.Text:
					case OdbcType.NText:
					case OdbcType.NVarChar:
					case OdbcType.VarChar:
						return typeof (string);
					case OdbcType.Real:
						return typeof (float);
					case OdbcType.SmallInt:
						return typeof (System.Int16);
					case OdbcType.UniqueIdentifier:
						return typeof (Guid);
				}
				throw new InvalidCastException();
			}
		}

		internal bool IsDateType
		{
			get
			{
				switch (OdbcType)
				{
					case OdbcType.Time:
					case OdbcType.Timestamp:
					case OdbcType.DateTime:
					case OdbcType.Date:
					case OdbcType.SmallDateTime:
						return true;
					default:
						return false;
				}
			}
		}

		internal bool IsStringType
		{
			get
			{
				switch (OdbcType)
				{
					case OdbcType.Char:
					case OdbcType.Text:
					case OdbcType.NText:
					case OdbcType.NVarChar:
					case OdbcType.VarChar:
						return true;
					default:
						return false;
				}
			}
		}

		internal bool IsVariableSizeType {
			get {
				if (IsStringType)
					return true;
				switch (OdbcType) {
				case OdbcType.Binary :
				case OdbcType.VarBinary :
				case OdbcType.Image :
					return true;
				default : 
					return false;
				}
			}
		}

                internal SQL_TYPE SqlType
                {
                        get {
                                if ( _sqlType == SQL_TYPE.UNASSIGNED)
                                        _sqlType = OdbcTypeConverter.GetTypeMap (OdbcType).SqlType;
                                return _sqlType;
                        }

                        set {_sqlType = value;}
                }

                internal SQL_C_TYPE SqlCType
                {
                        get {
                                
                                if ( _sqlCType == SQL_C_TYPE.UNASSIGNED)
                                        _sqlCType = OdbcTypeConverter.GetTypeMap (OdbcType).NativeType;
                                return _sqlCType;
                        }
                        set {_sqlCType = value;}
                }

                internal void UpdateTypes (SQL_TYPE sqlType)
                {
                        SqlType = sqlType;
                        OdbcTypeMap map = OdbcTypeConverter.GetTypeMap (SqlType);
                        OdbcType = map.OdbcType;
                        SqlCType = map.NativeType;
                }
	}
}
