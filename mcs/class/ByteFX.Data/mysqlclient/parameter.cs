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

namespace ByteFX.Data.MySQLClient
{
	public sealed class MySQLParameter : MarshalByRefObject, IDataParameter, IDbDataParameter, ICloneable
	{
		MySQLDbType			m_dbType  = MySQLDbType.Null;
		DbType				m_genericType;
		ParameterDirection	m_direction = ParameterDirection.Input;
		bool				m_fNullable  = false;
		string				m_sParamName;
		string				m_sSourceColumn;
		DataRowVersion		m_sourceVersion = DataRowVersion.Current;
		object				m_value;

		public MySQLParameter()
		{
		}

		public MySQLParameter(string name, MySQLDbType type, ParameterDirection dir, string col, DataRowVersion ver, object val)
		{
			m_dbType = type;
			m_direction = dir;
			m_sParamName = name;
			m_sSourceColumn = col;
			m_sourceVersion = ver;
			m_value = val;
		}

		public MySQLParameter(string parameterName, MySQLDbType type)
		{
			m_sParamName = parameterName;
			m_dbType   = type;
		}

		public MySQLParameter(string parameterName, object value)
		{
			m_sParamName = parameterName;
			this.Value = value;   
			// Setting the value also infers the type.
		}

		public MySQLParameter( string parameterName, MySQLDbType dbType, string sourceColumn )
		{
			m_sParamName  = parameterName;
			m_dbType    = dbType;
			m_sSourceColumn = sourceColumn;
		}

		DbType IDataParameter.DbType 
		{
			get { return m_genericType; }
			set { m_genericType = value; }
		}

		public MySQLDbType DbType 
		{
			get  { return m_dbType; }
			set  { m_dbType = value;  }
		}

		public ParameterDirection Direction 
		{
			get { return m_direction; }
			set { m_direction = value; }
		}

		public Boolean IsNullable 
		{
			get { return m_fNullable; }
		}

		public String ParameterName 
		{
			get { return m_sParamName; }
			set { m_sParamName = value; }
		}

		public String SourceColumn 
		{
			get { return m_sSourceColumn; }
			set { m_sSourceColumn = value; }
		}

		public DataRowVersion SourceVersion 
		{
			get { return m_sourceVersion; }
			set { m_sourceVersion = value; }
		}

		public object Value 
		{
			get
			{
				return m_value;
			}
			set
			{
				m_value    = value;
				m_dbType  = _inferType(value);
			}
		}

		private string ObjectToString()
		{
			return "";
		}

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

		public void SerializeToBytes( System.IO.MemoryStream s )
		{
			string parm_string;

			switch (m_dbType) 
			{
				case MySQLDbType.Null:
					parm_string = "Null";
					break;

				case MySQLDbType.VarChar:
					parm_string = "'" + EscapeString(Value.ToString()) + "'";
					break;

				case MySQLDbType.Date:
				{
					parm_string = "'" + ((DateTime)Value).ToString("yyyy-MM-dd") + "'";
					break;
				}

				case MySQLDbType.Datetime:
				{
					parm_string = "'" + ((DateTime)Value).ToString("yyyy-MM-dd HH:mm:ss") + "'";
					break;
				}

				case MySQLDbType.Blob:
					if (m_value.GetType() == Type.GetType("System.Byte[]"))
					{
						s.WriteByte((byte)'\'');
						EscapeByteArray( (byte[])m_value, s );
						s.WriteByte((byte)'\'');
					}
					return;

				default:
					parm_string = Value.ToString();
					break;
			}
			byte[] bytes = System.Text.Encoding.ASCII.GetBytes(parm_string);
			s.Write(bytes, 0, bytes.Length);
		}

		private MySQLDbType _inferType(Object value)
		{
			switch (Type.GetTypeCode(value.GetType()))
			{
			case TypeCode.Empty:
				throw new SystemException("Invalid data type");

			case TypeCode.Object:
				return MySQLDbType.Blob;

			case TypeCode.DBNull:
				return MySQLDbType.Null;

			case TypeCode.Char:
			case TypeCode.SByte:
			case TypeCode.Boolean:
			case TypeCode.Byte:
				return MySQLDbType.Byte;

			case TypeCode.Int16:
			case TypeCode.UInt16:
				return MySQLDbType.Int24;

			case TypeCode.Int32:
			case TypeCode.UInt32:
				return MySQLDbType.Long;

			case TypeCode.Int64:
			case TypeCode.UInt64:
				return MySQLDbType.LongLong;

			case TypeCode.Single:
				return MySQLDbType.Float;

			case TypeCode.Double:
				return MySQLDbType.Double;

			case TypeCode.Decimal:
				return MySQLDbType.Decimal;

			case TypeCode.DateTime:
				return MySQLDbType.Datetime;

			case TypeCode.String:
				return MySQLDbType.VarChar;

			default:
				throw new SystemException("Value is of unknown data type");
			}
		}

		// implement methods of IDbDataParameter
		public byte Precision 
		{
			get { return 0; }
			set { }
		}

		public byte Scale 
		{
			get { return 0; }
			set { }
		}

		public int Size 
		{
			get { return 0; }
			set { }
		}

		#region ICloneable
		public object Clone() 
		{
			MySQLParameter clone = new MySQLParameter( m_sParamName, m_dbType, m_direction,
								m_sSourceColumn, m_sourceVersion, m_value );
			return clone;
		}
		#endregion

  }
}
