// created on 18/5/2002 at 01:25

// Npgsql.NpgsqlParameter.cs
// 
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
//
//	Copyright (C) 2002 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
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
using NpgsqlTypes;


namespace Npgsql
{
	///<summary>
	/// This class represents a parameter to a command that will be sent to server
	///</summary>
	public sealed class NpgsqlParameter : IDbDataParameter, IDataParameter
	{
	
    // Logging related values
    private static readonly String CLASSNAME = "NpgsqlParameter";
    
		// Fields to implement IDbDataParameter interface.
		private byte 				precision;
		private byte 				scale;
		private Int32				size;
		
		// Fields to implement IDataParameter
		private DbType				db_type;
		private ParameterDirection	direction;
		private Boolean				is_nullable;
		private String				name;
		private String				source_column;
		private DataRowVersion		source_version;
		private Object				value;
		
		
		
		// Constructors
		// [TODO] Implement other constructors.
		
		public NpgsqlParameter()
		{
			
		}
		
		public NpgsqlParameter(String parameterName, DbType parameterType)
		{
			name = parameterName;
		  if (name[0] != ':') // Support both ':'paramname and paramname constructions.
		    name = ':' + name;
		  
			db_type = parameterType;
			
		}
		
		public NpgsqlParameter(String parameterName, DbType parameterType, Int32 size, String sourceColumn)
		{
			name = parameterName;
		  if (name[0] != ':') // Support both ':'paramname and paramname constructions.
		    name = ':' + name;
			db_type = parameterType;
			this.size = size;
			source_column = sourceColumn;
			direction = ParameterDirection.Input;
		}
		// Implementation of IDbDataParameter
		
		public Byte Precision
		{
			get
			{
				return precision;
			}
			
			set
			{
				precision = value;
				NpgsqlEventLog.LogMsg("Set " + CLASSNAME + ".Precision = " + value, LogLevel.Normal);
			}
		}
		
		public Byte Scale
		{
			get
			{
				return scale;
			}
			
			set
			{
				scale = value;
				NpgsqlEventLog.LogMsg("Set " + CLASSNAME + ".Scale = " + value, LogLevel.Normal);
			}
		}
		
		public Int32 Size
		{
			get
			{
				return size;
			}
			
			set
			{
				size = value;
				NpgsqlEventLog.LogMsg("Set " + CLASSNAME + ".Size = " + value, LogLevel.Normal);
			}
		}
		
		public DbType DbType
		{
			get
			{
				return db_type;
			}
			
			// [TODO] Validate data type.
			set
			{
				db_type = value;
				NpgsqlEventLog.LogMsg("Set " + CLASSNAME + ".DbType = " + value, LogLevel.Normal);
			}
		}
		
		
		
		public ParameterDirection Direction
		{
			get
			{
				NpgsqlEventLog.LogMsg("Get " + CLASSNAME + ".Direction", LogLevel.Normal);
				return direction;
			}
			
			set
			{
				direction = value;
				NpgsqlEventLog.LogMsg("Set " + CLASSNAME + ".Direction = " + value, LogLevel.Normal);
			}
		}
		
		public Boolean IsNullable
		{
			get
			{
				return is_nullable;
			}
			
			set
			{
				is_nullable = value;
				NpgsqlEventLog.LogMsg("Set " + CLASSNAME + ".IsNullable = " + value, LogLevel.Normal);
			}
		}
		
		public String ParameterName
		{
			get
			{
				NpgsqlEventLog.LogMsg("Get " + CLASSNAME + ".ParameterName", LogLevel.Normal);
				return name;
			}
			
			set
			{
				name = value;
			  if (name[0] != ':')
			    name = ':' + name;
				NpgsqlEventLog.LogMsg("Set " + CLASSNAME + ".ParameterName = " + value, LogLevel.Normal);
			}
		}
		
		public String SourceColumn 
		{
			get
			{
				NpgsqlEventLog.LogMsg(CLASSNAME + ".get_SourceColumn" + value, LogLevel.Normal);
				return source_column;
			}
			
			set
			{
				source_column = value;
				NpgsqlEventLog.LogMsg("Set " + CLASSNAME + ".SourceColumn = " + value, LogLevel.Normal);
			}
		}
		
		public DataRowVersion SourceVersion
		{
			get
			{
				NpgsqlEventLog.LogMsg(CLASSNAME + ".get_SourceVersion = " + value, LogLevel.Normal);
				return source_version;
			}
			
			set
			{
				source_version = value;
				NpgsqlEventLog.LogMsg("Set " + CLASSNAME + ".SourceVersion = " + value, LogLevel.Normal);
			}
		}
		
		public Object Value
		{
			get
			{
				NpgsqlEventLog.LogMsg("Get " + CLASSNAME + ".Value", LogLevel.Normal);
				return value;
			}
			
			// [TODO] Check and validate data type.
			set
			{
				this.value = value;
				NpgsqlEventLog.LogMsg("Set " + CLASSNAME + ".Value", LogLevel.Normal);
			}
		}
				
	}
}
