// ByteFX.Data data access components for .Net
// Copyright (C) 2002-2003  ByteFX, Inc.
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Data;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Reflection;

namespace ByteFX.Data.MySqlClient
{
	/// <summary>
	/// Represents a parameter to a <see cref="MySqlCommand"/>, and optionally, its mapping to <see cref="DataSet"/> columns. This class cannot be inherited.
	/// </summary>
	[TypeConverter(typeof(MySqlParameter.MySqlParameterConverter))]
	public sealed class MySqlParameter : MarshalByRefObject, IDataParameter, IDbDataParameter, ICloneable
	{
		private MySqlDbType			dbType  = MySqlDbType.Null;
		private DbType				genericType;
		private ParameterDirection	direction = ParameterDirection.Input;
		private bool				isNullable  = false;
		private string				paramName;
		private string				sourceColumn;
		private DataRowVersion		sourceVersion = DataRowVersion.Current;
		private object				paramValue = DBNull.Value;
		private int					size;
		private byte				precision=0, scale=0;

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the MySqlParameter class.
		/// </summary>
		public MySqlParameter()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MySqlParameter"/> class with the parameter name and a value of the new MySqlParameter.
		/// </summary>
		/// <param name="parameterName">The name of the parameter to map. </param>
		/// <param name="value">An <see cref="Object"/> that is the value of the <see cref="MySqlParameter"/>. </param>
		public MySqlParameter(string parameterName, object value)
		{
			ParameterName = parameterName;
			paramValue = value;
			if (value is Byte[])
				size = (value as Byte[]).Length;
			dbType = GetMySqlType();
			genericType = GetGenericType();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MySqlParameter"/> class with the parameter name and the data type.
		/// </summary>
		/// <param name="parameterName">The name of the parameter to map. </param>
		/// <param name="dbType">One of the <see cref="MySqlDbType"/> values. </param>
		public MySqlParameter( string parameterName, MySqlDbType dbType)
		{
			ParameterName = parameterName;
			this.dbType   = dbType;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MySqlParameter"/> class with the parameter name, the <see cref="MySqlDbType"/>, and the size.
		/// </summary>
		/// <param name="parameterName">The name of the parameter to map. </param>
		/// <param name="dbType">One of the <see cref="MySqlDbType"/> values. </param>
		/// <param name="size">The length of the parameter. </param>
		public MySqlParameter( string parameterName, MySqlDbType dbType, int size )
		{
			ParameterName = parameterName;
			this.dbType = dbType;
			this.size = size;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MySqlParameter"/> class with the parameter name, the <see cref="MySqlDbType"/>, the size, and the source column name.
		/// </summary>
		/// <param name="parameterName">The name of the parameter to map. </param>
		/// <param name="dbType">One of the <see cref="MySqlDbType"/> values. </param>
		/// <param name="size">The length of the parameter. </param>
		/// <param name="sourceColumn">The name of the source column. </param>
		public MySqlParameter( string parameterName, MySqlDbType dbType, int size, string sourceColumn )
		{
			ParameterName = parameterName;
			this.dbType = dbType;
			this.size = size;
			this.direction = ParameterDirection.Input;
			this.precision = 0;
			this.scale = 0;
			this.sourceColumn = sourceColumn;
			this.sourceVersion = DataRowVersion.Current;
			this.paramValue =null;
		}

		internal MySqlParameter(string name, MySqlDbType type, ParameterDirection dir, string col, DataRowVersion ver, object val)
		{
			if (direction != ParameterDirection.Input)
				throw new ArgumentException("Only input parameters are supported by MySql");
			dbType = type;
			direction = dir;
			ParameterName = name;
			sourceColumn = col;
			sourceVersion = ver;
			paramValue = val;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MySqlParameter"/> class with the parameter name, the type of the parameter, the size of the parameter, a <see cref="ParameterDirection"/>, the precision of the parameter, the scale of the parameter, the source column, a <see cref="DataRowVersion"/> to use, and the value of the parameter.
		/// </summary>
		/// <param name="parameterName">The name of the parameter to map. </param>
		/// <param name="dbType">One of the <see cref="MySqlDbType"/> values. </param>
		/// <param name="size">The length of the parameter. </param>
		/// <param name="direction">One of the <see cref="ParameterDirection"/> values. </param>
		/// <param name="isNullable">true if the value of the field can be null, otherwise false. </param>
		/// <param name="precision">The total number of digits to the left and right of the decimal point to which <see cref="MySqlParameter.Value"/> is resolved.</param>
		/// <param name="scale">The total number of decimal places to which <see cref="MySqlParameter.Value"/> is resolved. </param>
		/// <param name="sourceColumn">The name of the source column. </param>
		/// <param name="sourceVersion">One of the <see cref="DataRowVersion"/> values. </param>
		/// <param name="value">An <see cref="Object"/> that is the value of the <see cref="MySqlParameter"/>. </param>
		/// <exception cref="ArgumentException"/>
		public MySqlParameter( string parameterName, MySqlDbType dbType, int size, ParameterDirection direction,
			bool isNullable, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion,
			object value)
		{
			if (direction != ParameterDirection.Input)
				throw new ArgumentException("Only input parameters are supported by MySql");

			ParameterName = parameterName;
			this.dbType = dbType;
			this.size = size;
			this.direction = direction;
			this.precision = precision;
			this.scale = scale;
			this.sourceColumn = sourceColumn;
			this.sourceVersion = sourceVersion;
			this.paramValue = value;
		}
		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the <see cref="DbType"/> of the parameter.
		/// </summary>
		public DbType DbType 
		{
			get 
			{ 
				return genericType; 
			}
			set 
			{ 
				genericType = value; 
				switch (genericType) 
				{
					case DbType.AnsiString:
					case DbType.AnsiStringFixedLength:
					case DbType.String:
					case DbType.StringFixedLength:
					case DbType.Guid:
						MySqlDbType = MySqlDbType.VarChar; break;

					case DbType.Byte:
					case DbType.SByte:
					case DbType.Boolean:
						MySqlDbType = MySqlDbType.Byte; break;

					case DbType.Int16:
					case DbType.UInt16:
						MySqlDbType = MySqlDbType.Short; break;

					case DbType.Int32:
					case DbType.UInt32:
						MySqlDbType = MySqlDbType.Int; break;
						
					case DbType.Int64:
					case DbType.UInt64:
						MySqlDbType = MySqlDbType.BigInt; break;

					case DbType.DateTime:
						MySqlDbType = MySqlDbType.Datetime;	break;

					case DbType.Date:
						MySqlDbType = MySqlDbType.Date; break;

					case DbType.Time:
						MySqlDbType = MySqlDbType.Time; break;

					case DbType.Single:
						MySqlDbType = MySqlDbType.Float; break;
					case DbType.Double:
					case DbType.Currency:
						MySqlDbType = MySqlDbType.Double; break;

					case DbType.Decimal:
					case DbType.VarNumeric:
						MySqlDbType = MySqlDbType.Decimal; break;

					case DbType.Binary:
					case DbType.Object:
						MySqlDbType = MySqlDbType.Blob; break;
				}
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the parameter is input-only, output-only, bidirectional, or a stored procedure return value parameter.
		/// As of MySql version 4.1 and earlier, input-only is the only valid choice.
		/// </summary>
		[Category("Data")]
		public ParameterDirection Direction 
		{
			get { return direction; }
			set { direction = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether the parameter accepts null values.
		/// </summary>
		[Browsable(false)]
		public Boolean IsNullable 
		{
			get { return isNullable; }
		}

		/// <summary>
		/// Gets or sets the MySqlDbType of the parameter.
		/// </summary>
		[Category("Data")]
		public MySqlDbType MySqlDbType 
		{
			get  
			{ 
				return dbType; 
			}
			set  
			{ 
				dbType = value;  
			}
		}

		/// <summary>
		/// Gets or sets the name of the MySqlParameter.
		/// </summary>
		[Category("Misc")]
		public String ParameterName 
		{
			get { return paramName; }
			set 
			{ 
				paramName = value; 
				if (paramName[0] == '@')
					paramName = paramName.Substring(1, paramName.Length-1);
			}
		}

		/// <summary>
		/// Gets or sets the maximum number of digits used to represent the <see cref="Value"/> property.
		/// </summary>
		[Category("Data")]
		public byte Precision 
		{
			get { return precision; }
			set { precision = value; }
		}

		/// <summary>
		/// Gets or sets the number of decimal places to which <see cref="Value"/> is resolved.
		/// </summary>
		[Category("Data")]
		public byte Scale 
		{
			get { return scale; }
			set { scale = value; }
		}

		/// <summary>
		/// Gets or sets the maximum size, in bytes, of the data within the column.
		/// </summary>
		[Category("Data")]
		public int Size 
		{
			get { return size; }
			set { size = value; }
		}

		/// <summary>
		/// Gets or sets the name of the source column that is mapped to the <see cref="DataSet"/> and used for loading or returning the <see cref="Value"/>.
		/// </summary>
		[Category("Data")]
		public String SourceColumn 
		{
			get { return sourceColumn; }
			set { sourceColumn = value; }
		}

		/// <summary>
		/// Gets or sets the <see cref="DataRowVersion"/> to use when loading <see cref="Value"/>.
		/// </summary>
		[Category("Data")]
		public DataRowVersion SourceVersion 
		{
			get { return sourceVersion; }
			set { sourceVersion = value; }
		}

		/// <summary>
		/// Gets or sets the value of the parameter.
		/// </summary>
		[TypeConverter(typeof(StringConverter))]
		[Category("Data")]
		public object Value 
		{
			get	{ return paramValue; }
			set	
			{ 
				paramValue = value; 
				if (dbType == MySqlDbType.Null) 
				{
					dbType = GetMySqlType();
					genericType = GetGenericType();
				}
			}
		}

		#endregion

		private void EscapeByteArray( byte[] bytes, System.IO.MemoryStream s )
		{
			int theSize = size == 0 ? bytes.Length : size;
			byte[] newbytes = new byte[ theSize * 2 ];

			int newx=0;
			for (int x=0; x < theSize; x++)
			{
				byte b = bytes[x];
				if (b == '\0') 
				{
					newbytes[newx++] = (byte)'\\';
					newbytes[newx++] = (byte)'0';
				}
				else 
				{
					if (b == '\\' || b == '\'' || b == '"' || b == '`' || b == '´')
						newbytes[newx++] = (byte)'\\';
					newbytes[newx++] = b;
				}
			}
			s.Write( newbytes, 0, newx );
		}

		/// <summary>
		/// Overridden. Gets a string containing the <see cref="ParameterName"/>.
		/// </summary>
		/// <returns></returns>
		public override string ToString() 
		{
			return paramName;
		}

		private string EscapeString( string s )
		{
			s = s.Replace("\'", "\\\'");
			s = s.Replace("\"", "\\\"");
			return s;
		}

		internal void SerializeToBytes( System.IO.MemoryStream s, MySqlConnection conn )
		{
			string	parm_string = null;
			byte[]	bytes = null;

			//TODO:  should value == null throw an exception?
			if (Value == DBNull.Value || Value == null)
				parm_string = "Null";
			else if (paramValue is bool)
				parm_string = Convert.ToByte(paramValue).ToString();
			else if (paramValue is Enum)
				parm_string = ((int)paramValue).ToString();
			else 
			{
				switch (dbType) 
				{
					case MySqlDbType.Null:
						parm_string = "Null";
						break;

					case MySqlDbType.VarChar:
					case MySqlDbType.String:
						parm_string = "'" + EscapeString(Value.ToString()) + "'";
						break;

					case MySqlDbType.Double:
						parm_string = Convert.ToDouble(Value).ToString( conn.NumberFormat );
						break;

					case MySqlDbType.Float:
						parm_string = Convert.ToSingle(Value).ToString( conn.NumberFormat );
						break;

					case MySqlDbType.Decimal:
						parm_string = Convert.ToDecimal(Value).ToString( conn.NumberFormat );
						break;

					case MySqlDbType.Time:
						if (Value is DateTime)
							parm_string = String.Format("'{0:HH:mm:ss}'", ((DateTime)Value));
						else 
							parm_string = String.Format("'{0}'", Value.ToString());
						break;

					case MySqlDbType.Date:
						if (Value is DateTime)
							parm_string = String.Format("'{0:yyyy-MM-dd}'", ((DateTime)Value));
						else 
							parm_string = "'" + Value.ToString() + "'";
						break;

					case MySqlDbType.Datetime:
						if (Value is DateTime)
							parm_string = String.Format("'{0:yyyy-MM-dd HH:mm:ss}'", ((DateTime)Value));
						else
							parm_string = "'" + Value + "'";
						break;

					case MySqlDbType.Blob:
					case MySqlDbType.MediumBlob:
					case MySqlDbType.LongBlob:
					case MySqlDbType.TinyBlob:
						Type t = paramValue.GetType();

						if (t == typeof(System.Byte[]))
						{
							s.WriteByte((byte)'\'');
							EscapeByteArray( (byte[])paramValue, s );
							s.WriteByte((byte)'\'');
							return;
						}
						else if(t == typeof(string)) 
							parm_string = "'" + EscapeString((string)paramValue) + "'";
						else if (t == typeof(System.Guid))
						{
							parm_string = "'" + paramValue.ToString() + "'";
						}
						break;

					case MySqlDbType.Int:
					case MySqlDbType.Int24:
						parm_string = Convert.ToInt32( Value.ToString() ).ToString();
						break;
				
					case MySqlDbType.BigInt:
						parm_string = Convert.ToInt64( Value.ToString() ).ToString();
						break;

					case MySqlDbType.Short:
						parm_string = Convert.ToInt16( Value.ToString() ).ToString();
						break;

					default:
						parm_string = Value.ToString();
						break;
				}
			}

			bytes = conn.Encoding.GetBytes(parm_string);
			s.Write(bytes, 0, bytes.Length);
		}

		private DbType GetGenericType()
		{
			if (paramValue is TimeSpan)
				return DbType.DateTime;
			if (paramValue is Enum)
				return DbType.Int32;
			if (paramValue is Guid)
				return DbType.String;

			switch ( Type.GetTypeCode(paramValue.GetType()) )
			{
				case TypeCode.Boolean: return DbType.Boolean;
				case TypeCode.Byte: return DbType.Byte;
				case TypeCode.Char: return DbType.StringFixedLength;
				case TypeCode.DateTime: return DbType.DateTime;
				case TypeCode.Decimal: return DbType.Decimal;
				case TypeCode.Double: return DbType.Double;
				case TypeCode.Int16: return DbType.Int16;
				case TypeCode.Int32: return DbType.Int32;
				case TypeCode.Int64: return DbType.Int64;
				case TypeCode.Object: return DbType.Binary;
				case TypeCode.SByte: return DbType.SByte;
				case TypeCode.Single: return DbType.Single;
				case TypeCode.String: return DbType.String;
				case TypeCode.UInt16: return DbType.UInt16;
				case TypeCode.UInt32: return DbType.UInt32;
				case TypeCode.UInt64: return DbType.UInt64;
			}
			return DbType.Object;
		}

		private MySqlDbType GetMySqlType()
		{
			if (paramValue is System.TimeSpan)
				return MySqlDbType.Time;
			if (paramValue is Enum)
				return MySqlDbType.Int;
			if (paramValue is Guid)
				return MySqlDbType.String;

			switch (Type.GetTypeCode( paramValue.GetType() ))
			{
				case TypeCode.Empty:
					throw new SystemException("Invalid data type");

				case TypeCode.Object: return MySqlDbType.Blob;
				case TypeCode.DBNull: return MySqlDbType.Null;
				case TypeCode.Char:
				case TypeCode.SByte:
				case TypeCode.Boolean:
				case TypeCode.Byte: return MySqlDbType.Byte;
				case TypeCode.Int16:
				case TypeCode.UInt16: return MySqlDbType.Int24;
				case TypeCode.Int32:
				case TypeCode.UInt32: return MySqlDbType.Int;
				case TypeCode.Int64:
				case TypeCode.UInt64: return MySqlDbType.BigInt;
				case TypeCode.Single: return MySqlDbType.Float;
				case TypeCode.Double: return MySqlDbType.Double;
				case TypeCode.Decimal: return MySqlDbType.Decimal;
				case TypeCode.DateTime: return MySqlDbType.Datetime;
				case TypeCode.String: return MySqlDbType.VarChar;

				default:
					throw new SystemException("Value is of unknown data type");
			}
		}


		#region ICloneable
		object System.ICloneable.Clone() 
		{
			MySqlParameter clone = new MySqlParameter( paramName, dbType, direction,
				sourceColumn, sourceVersion, paramValue );
			return clone;
		}
		#endregion

		internal class MySqlParameterConverter : TypeConverter
		{
			public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			{
				if (destinationType == typeof(InstanceDescriptor))
				{
					return true;
				}

				// Always call the base to see if it can perform the conversion.
				return base.CanConvertTo(context, destinationType);
			}

			public override object ConvertTo(ITypeDescriptorContext context, 
				System.Globalization.CultureInfo culture, object value, Type destinationType)
			{
				if (destinationType == typeof(InstanceDescriptor))
				{
					ConstructorInfo ci = typeof(MySqlParameter).GetConstructor(
						new Type[]{typeof(string), typeof(MySqlDbType), typeof(int), typeof(ParameterDirection),
						typeof(bool), typeof(byte), typeof(byte), typeof(string), typeof(DataRowVersion),
						typeof(object)});
					MySqlParameter p = (MySqlParameter) value;
					return new InstanceDescriptor(ci,new object[]{ 
						p.ParameterName, p.DbType, p.Size, p.Direction, p.IsNullable, p.Precision,
						p.Scale, p.SourceColumn, p.SourceVersion, p.Value});
				}

				// Always call base, even if you can't convert.
				return base.ConvertTo(context, culture, value, destinationType);
			}
		}
	}
}
