
// NpgsqlTypes.NpgsqlTypesHelper.cs
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
using System.Collections;
using System.Globalization;
using System.Data;
using Npgsql;



/// <summary>
///	This class contains helper methods for type conversion between
/// the .Net type system and postgresql.	
/// </summary>
namespace NpgsqlTypes
{
	
	/*internal struct NpgsqlTypeMapping
	{
	  public String        _backendTypeName;
	  public Type          _frameworkType;
	  public Int32         _typeOid;
	  public NpgsqlDbType  _npgsqlDbType;
	  
	  public NpgsqlTypeMapping(String backendTypeName, Type frameworkType, Int32 typeOid, NpgsqlDbType npgsqlDbType)
	  {
	    _backendTypeName = backendTypeName;
	    _frameworkType = frameworkType;
	    _typeOid = typeOid;
	    _npgsqlDbType = npgsqlDbType;
	    
	  }
	}*/
	
	
	internal class NpgsqlTypesHelper
	{
		
		private static Hashtable _oidToNameMappings = new Hashtable();
		
		// Logging related values
    private static readonly String CLASSNAME = "NpgsqlDataReader";
		
		
		public static String GetBackendTypeNameFromDbType(DbType dbType)
		{
			NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".GetBackendTypeNameFromNpgsqlDbType(NpgsqlDbType)", Npgsql.LogLevel.Debug);
			
			switch (dbType)
			{
				case DbType.Boolean:
					return "bool";
				case DbType.Int64:
					return "int8";
				case DbType.Int32:
					return "int4";
				case DbType.Decimal:
					return "numeric";
				case DbType.Int16:
					return "int2";
				case DbType.String:
					return "text";
				case DbType.DateTime:
					return "timestamp";
				default:
					throw new NpgsqlException(String.Format("This type {0} isn't supported yet.", dbType));
				
			}
		}
		
		
		public static String ConvertNpgsqlParameterToBackendStringValue(NpgsqlParameter parameter)
		{
			NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".ConvertNpgsqlParameterToBackendStringValue(NpgsqlParameter)", Npgsql.LogLevel.Debug);
			
			if (parameter.Value == DBNull.Value)
				return "Null";
			
			switch(parameter.DbType)
			{
				case DbType.Boolean:
				case DbType.Int64:
				case DbType.Int32:
				case DbType.Int16:
					return parameter.Value.ToString();
				
				case DbType.Decimal:
						return ((Decimal)parameter.Value).ToString(NumberFormatInfo.InvariantInfo);
				
				case DbType.String:
						return "'" + parameter.Value.ToString().Replace("'", "\\'") + "'";
				
				case DbType.DateTime:
				{
					return "'" + ((DateTime)parameter.Value).ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
					
				}
				default:
					// This should not happen!
					throw new NpgsqlException(String.Format("This type {0} isn't supported yet.", parameter.DbType));
					
				
			}
			
		}
		
		
		
		
		
		///<summary>
		/// This method is responsible to convert the string received from the backend
		/// to the corresponding NpgsqlType.
		/// </summary>
		/// 
		public static Object ConvertBackendStringToSystemType(Hashtable oidToNameMapping, String data, Int32 typeOid, Int32 typeModifier)
		{
			NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".ConvertBackendStringToSystemType(Hashtable, String, Int32)", Npgsql.LogLevel.Debug);
			//[TODO] Find a way to eliminate this checking. It is just used at bootstrap time
			// when connecting because we don't have yet loaded typeMapping. The switch below
			// crashes with NullPointerReference when it can't find the typeOid.
			
			if (!oidToNameMapping.ContainsKey(typeOid))
				return data;
			
			switch ((DbType)oidToNameMapping[typeOid])
			{
				case DbType.Boolean:
					return (data.ToLower() == "t" ? true : false);
				case DbType.Int16:
					return Int16.Parse(data);
				case DbType.Int32:
					return Int32.Parse(data);
				
				case DbType.Int64:
					return Int64.Parse(data);
				
				case DbType.Decimal:
					// Got this manipulation of typemodifier from jdbc driver - file AbstractJdbc1ResultSetMetaData.java.html method getColumnDisplaySize
					{ 
						typeModifier -= 4;
						//Console.WriteLine("Numeric from server: {0} digitos.digitos {1}.{2}", data, (typeModifier >> 16) & 0xffff, typeModifier & 0xffff);
						return Decimal.Parse(data, NumberFormatInfo.InvariantInfo);
						
					}
				
				case DbType.DateTime:
					
					// Get the date time parsed in all expected formats for timestamp.
					return DateTime.ParseExact(data,
					                           new String[] {"yyyy-MM-dd HH:mm:ss.fff", "yyyy-MM-dd HH:mm:ss.ff", "yyyy-MM-dd HH:mm:ss.f", "yyyy-MM-dd HH:mm:ss"}, 
					                           DateTimeFormatInfo.InvariantInfo,
					                           DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.AllowWhiteSpaces);
					                        
				case DbType.String:
					return data;
				default:
					throw new NpgsqlException(String.Format("This type {0} isn't supported yet.", oidToNameMapping[typeOid]));
				
			
			}
		}
		
		    
    ///<summary>
		/// This method gets a type oid and return the equivalent
		/// Npgsql type.
		/// </summary>
		/// 
		
		public static Type GetSystemTypeFromTypeOid(Hashtable oidToNameMapping, Int32 typeOid)
    {
    	NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".GetSystemTypeFromTypeOid(Hashtable, Int32)", Npgsql.LogLevel.Debug);
    	// This method gets a db type identifier and return the equivalent
    	// system type.
    	
    	//[TODO] Find a way to eliminate this checking. It is just used at bootstrap time
			// when connecting because we don't have yet loaded typeMapping. The switch below
			// crashes with NullPointerReference when it can't find the typeOid.
    	
    	
    	
    	if (!oidToNameMapping.ContainsKey(typeOid))
				return Type.GetType("System.String");
			
    	switch ((DbType)oidToNameMapping[typeOid])
			{
				case DbType.Boolean:
					return Type.GetType("System.Boolean");
				case DbType.Int16:
					return Type.GetType("System.Int16");
				case DbType.Int32:
					return Type.GetType("System.Int32");
				case DbType.Int64:
					return Type.GetType("System.Int64");
				case DbType.Decimal:
					return Type.GetType("System.Decimal");
				case DbType.DateTime:
					return Type.GetType("System.DateTime");
				case DbType.String:
					return Type.GetType("System.String");
				default:
					throw new NpgsqlException(String.Format("This type {0} isn't supported yet.", oidToNameMapping[typeOid]));
			
			}
			
    	
    }
    
		
		///<summary>
		/// This method is responsible to send query to get the oid-to-name mapping.
		/// This is needed as from one version to another, this mapping can be changed and
		/// so we avoid hardcoding them.
		/// </summary>
		public static Hashtable LoadTypesMapping(NpgsqlConnection conn)
		{
			NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".LoadTypesMapping(NpgsqlConnection)", Npgsql.LogLevel.Debug);
			
			// [TODO] Verify another way to get higher concurrency.
			lock(typeof(NpgsqlTypesHelper))
			{
				Hashtable oidToNameMapping = (Hashtable) _oidToNameMappings[conn.ServerVersion];
												
				if (oidToNameMapping != null)
				{
					//conn.OidToNameMapping = oidToNameMapping;
					return oidToNameMapping;
				}
				
				
				oidToNameMapping = new Hashtable();
				//conn.OidToNameMapping = oidToNameMapping;
				
				// Bootstrap value as the datareader below will use ConvertStringToNpgsqlType above.
				//oidToNameMapping.Add(26, "oid");
								
				NpgsqlCommand command = new NpgsqlCommand("select oid, typname from pg_type where typname in ('bool', 'int2', 'int4', 'int8', 'numeric', 'text', 'timestamp');", conn);
				
				NpgsqlDataReader dr = command.ExecuteReader();
				
				// Data was read. Clear the mapping from previous bootstrap value so we don't get
				// exceptions trying to add duplicate key.
				// oidToNameMapping.Clear();
				
				while (dr.Read())
				{
					// Add the key as a Int32 value so the switch in ConvertStringToNpgsqlType can use it
					// in the search. If don't, the key is added as string and the switch doesn't work.
					
					DbType type;
					String typeName = (String) dr[1];
										
					switch (typeName)
					{
						case "bool":
							type = DbType.Boolean;
							break;
						case "int2":
							type = DbType.Int16;
							break;
						case "int4":
							type = DbType.Int32;
							break;
						case "int8":
							type = DbType.Int64;
							break;
						case "numeric":
							type = DbType.Decimal;
							break;
						case "timestamp":
							type = DbType.DateTime;
							break;
						default:
							type = DbType.String; // Default dbtype of the oid. Unsupported types will be returned as String.
							break;
					}
										
					
					oidToNameMapping.Add(Int32.Parse((String)dr[0]), type);
				}
				
				_oidToNameMappings.Add(conn.ServerVersion, oidToNameMapping);
				return oidToNameMapping;
			}
			
			
		}
		
	}
	
}
