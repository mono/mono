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
	[TypeConverter(typeof(MySqlParameter.MySqlParameterConverter))]
	public sealed class MySqlParameter : MarshalByRefObject, IDataParameter, IDbDataParameter, ICloneable
	{
		MySqlDbType			dbType  = MySqlDbType.Null;
		DbType				genericType;
		ParameterDirection	direction = ParameterDirection.Input;
		bool				isNullable  = false;
		string				paramName;
		string				sourceColumn;
		DataRowVersion		sourceVersion = DataRowVersion.Current;
		object				paramValue = DBNull.Value;
		int					size;
		byte				precision=0, scale=0;

		#region Constructors
		public MySqlParameter()
		{
		}

		public MySqlParameter(string parameterName, object value)
		{
			ParameterName = parameterName;
			paramValue = value;
			dbType = GetMySqlType( paramValue.GetType() );
			genericType = GetGenericType( paramValue.GetType() );
		}

		public MySqlParameter( string parameterName, MySqlDbType type)
		{
			ParameterName = parameterName;
			dbType   = type;
		}

		public MySqlParameter( string parameterName, MySqlDbType type, int size )
		{
			ParameterName = parameterName;
			dbType = type;
			this.size = size;
		}

		public MySqlParameter( string name, MySqlDbType dbType, int size, string sourceCol )
		{
			ParameterName = name;
			this.dbType = dbType;
			this.size = size;
			this.direction = ParameterDirection.Input;
			this.precision = 0;
			this.scale = 0;
			this.sourceColumn = sourceCol;
			this.sourceVersion = DataRowVersion.Current;
			this.paramValue =null;
		}

		public MySqlParameter(string name, MySqlDbType type, ParameterDirection dir, string col, DataRowVersion ver, object val)
		{
			dbType = type;
			direction = dir;
			ParameterName = name;
			sourceColumn = col;
			sourceVersion = ver;
			paramValue = val;
		}

		public MySqlParameter( string parameterName, MySqlDbType dbType, int size, ParameterDirection direction,
			bool isNullable, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion,
			object value)
		{
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
						MySqlDbType = MySqlDbType.Long; break;
						
					case DbType.Int64:
					case DbType.UInt64:
						MySqlDbType = MySqlDbType.LongLong; break;

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

		[Category("Data")]
		public ParameterDirection Direction 
		{
			get { return direction; }
			set { direction = value; }
		}

		[Browsable(false)]
		public Boolean IsNullable 
		{
			get { return isNullable; }
		}

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

		[Category("Data")]
		public String SourceColumn 
		{
			get { return sourceColumn; }
			set { sourceColumn = value; }
		}

		[Category("Data")]
		public DataRowVersion SourceVersion 
		{
			get { return sourceVersion; }
			set { sourceVersion = value; }
		}

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
					dbType = GetMySqlType( paramValue.GetType() );
					genericType = GetGenericType( paramValue.GetType() );
				}
			}
		}

		// implement methods of IDbDataParameter
		[Category("Data")]
		public byte Precision 
		{
			get { return precision; }
			set { precision = value; }
		}

		[Category("Data")]
		public byte Scale 
		{
			get { return scale; }
			set { scale = value; }
		}

		[Category("Data")]
		public int Size 
		{
			get { return size; }
			set { size = value; }
		}
		#endregion

		private void EscapeByteArray( byte[] bytes, System.IO.MemoryStream s )
		{
			byte[] newbytes = new byte[ bytes.Length * 2 ];

			int newx=0;
			for (int x=0; x < bytes.Length; x++)
			{
				byte b = bytes[x];
				if (b == '\0') 
				{
					newbytes[newx++] = (byte)'\\';
					newbytes[newx++] = (byte)'0';
				}
				else 
				{
					if (b == '\\' || b == '\'' || b == '"')
						newbytes[newx++] = (byte)'\\';
					newbytes[newx++] = b;
				}
			}
			s.Write( newbytes, 0, newx );
		}

		public override string ToString() 
		{
			return paramName;
		}

		private string EscapeString( string s )
		{
			StringBuilder sb = new StringBuilder();

			foreach (char c in s) 
			{
				if (c == '\'')
					sb.Append(c);
				sb.Append(c);
			}
			return sb.ToString();
		}

		public void SerializeToBytes( System.IO.MemoryStream s, MySqlConnection conn )
		{
			string	parm_string = null;
			byte[]	bytes = null;

			//TODO:  should value == null throw an exception?
			if (Value == DBNull.Value || Value == null)
				parm_string = "Null";
			else if (paramValue is bool)
				parm_string = Convert.ToByte(paramValue).ToString();
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

					default:
						parm_string = Value.ToString();
						break;
				}
			}

			bytes = conn.Encoding.GetBytes(parm_string);
			s.Write(bytes, 0, bytes.Length);
		}

		private DbType GetGenericType( Type systemType )
		{
			switch ( Type.GetTypeCode(systemType) )
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

		private MySqlDbType GetMySqlType( Type systemType )
		{
			switch (Type.GetTypeCode( systemType ))
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
				case TypeCode.UInt32: return MySqlDbType.Long;
				case TypeCode.Int64:
				case TypeCode.UInt64: return MySqlDbType.LongLong;
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
		public object Clone() 
		{
			MySqlParameter clone = new MySqlParameter( paramName, dbType, direction,
				sourceColumn, sourceVersion, paramValue );
			return clone;
		}
		#endregion

		/* A TypeConverter for the Triangle object.  Note that you can make it internal,
				private, or any scope you want and the designers will still be able to use
				it through the TypeDescriptor object.  This type converter provides the
				capability to convert to an InstanceDescriptor.  This object can be used by 
		   the .NET Framework to generate source code that creates an instance of a 
		   Triangle object. */
		internal class MySqlParameterConverter : TypeConverter
		{
			/* This method overrides CanConvertTo from TypeConverter. This is called when someone
					wants to convert an instance of Triangle to another type.  Here,
					only conversion to an InstanceDescriptor is supported. */
			public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			{
				if (destinationType == typeof(InstanceDescriptor))
				{
					return true;
				}

				// Always call the base to see if it can perform the conversion.
				return base.CanConvertTo(context, destinationType);
			}

			/* This code performs the actual conversion from a Triangle to an InstanceDescriptor. */
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
