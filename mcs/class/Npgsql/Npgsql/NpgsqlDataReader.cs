
// Npgsql.NpgsqlDataReader.cs
// 
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
//
//	Copyright (C) 2002 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
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
using System.Collections;
using NpgsqlTypes;


namespace Npgsql
{
		
	public class NpgsqlDataReader : IDataReader, IEnumerable
	{
		
		
		
	  private NpgsqlConnection 	_connection;
		private ArrayList 				_resultsets;
		private ArrayList					_responses;
	  private Int32 						_rowIndex;
		private Int32							_resultsetIndex;
		private	NpgsqlResultSet		_currentResultset;
		private DataTable					_currentResultsetSchema;
		
		
		// Logging related values
    private static readonly String CLASSNAME = "NpgsqlDataReader";
		
	  internal NpgsqlDataReader( ArrayList resultsets, ArrayList responses, NpgsqlConnection connection)
	  {
	    _resultsets					= resultsets;
	  	_responses					= responses;
	  	_connection 				= connection;
	  	_rowIndex						= -1;
	  	_resultsetIndex			= 0;
	  	
	  	_currentResultset 	= (NpgsqlResultSet)_resultsets[_resultsetIndex];
	  	
	  	
	  	
	  }
	  
	  private Boolean CanRead()
	  {
	  	NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".CanRead() ", LogLevel.Debug);
	  	/*if (_currentResultset == null)
	  		return false;*/
	  	return ((_currentResultset != null) && (_currentResultset.Count > 0));
	  	
	  }
	  
	  private void CheckCanRead()
	  {
	    if (!CanRead())
	      throw new InvalidOperationException("Cannot read data");
	  }
	  
	  public void Dispose()
	  {
	  	
	  }
	  public Int32 Depth 
	  {
	  	get
	  	{
	  		NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".get_Depth() ", LogLevel.Debug);
	  		return 0;
	  	}
	  }
	  
	  public Boolean IsClosed
	  {
	  	get
	  	{
	  		NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".get_IsClosed()", LogLevel.Debug);
	  		return false; 
	  	}
	  }
	  
	  public Int32 RecordsAffected 
	  {
	  	get
	  	{
	  		NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".get_RecordsAffected()", LogLevel.Debug);
	  		
	  		/*if (_currentResultset == null)
	  			return 0;	//[FIXME] Get the actual number of rows deleted, updated or inserted.
	  		return -1;
	  		*/
	  		
	  		if (CanRead())
	  			return -1;
	  		
	  		String[] ret_string_tokens = ((String)_responses[_resultsetIndex]).Split(null);	// whitespace separator.
				
				return Int32.Parse(ret_string_tokens[ret_string_tokens.Length - 1]);
	  	}
	    
	  }
	  
	  public void Close()
	  {
	    
	  }
	  
	  public Boolean NextResult()
	  {
	  	NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".NextResult()", LogLevel.Debug);
	    //throw new NotImplementedException();
	  	
	  	//[FIXME] Should the currentResultset not be modified
	  	// in case there aren't any more resultsets?
	  	// SqlClient modify to a invalid resultset and throws exceptions
	  	// when trying to access any data.
	  	
	  		  	
	  	if((_resultsetIndex + 1) < _resultsets.Count)
	  	{
	  		_resultsetIndex++;
	  		_rowIndex = -1;
	  		_currentResultset = (NpgsqlResultSet)_resultsets[_resultsetIndex];
	  		return true;
	  	}
	  	else
	  		return false;
	  	
	  }
	  
	  public Boolean Read()
	  {
	  	NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".Read()", LogLevel.Debug);
	  	
	  	if (!CanRead())
	  		return false;
	  	
	    _rowIndex++;
	  	return (_rowIndex < _currentResultset.Count);
	  }
	  
	  public DataTable GetSchemaTable()
	  {
	  	NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".GetSchemaTable()", LogLevel.Debug);
	    //throw new NotImplementedException();
	  	
	  	if (!CanRead())
	  		return null; //[FIXME] Should we return null or throw an exception??
	  	
	  	if(_currentResultsetSchema == null)
	  		_currentResultsetSchema = GetResultsetSchema();
	  	
	  	return _currentResultsetSchema;
	  	
	  }
	  
	  
	  public Int32 FieldCount
	  {
	  	get
	  	{
	  		
	  		NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".get_FieldCount()", LogLevel.Debug);
	  		//return ((_currentResultset == null) ? 0 : _currentResultset.RowDescription.NumFields);
	  		if (CanRead())
	  			return _currentResultset.RowDescription.NumFields;
	  		else
	  			return -1;
	  			
	  	}
	    
	  }
	  
	  public String GetName(Int32 i)
	  {
	    //throw new NotImplementedException();
	  	NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".GetName(Int32)", LogLevel.Debug);
	  	
	  	if (CanRead())
	  		return _currentResultset.RowDescription[i].name;
	  	else
	  		return String.Empty;
	  }
	  
	  public String GetDataTypeName(Int32 i)
	  {
	  	// FIXME: have a type name instead of the oid
			return (_currentResultset.RowDescription[i].type_oid).ToString();
	  }
	  
	  public Type GetFieldType(Int32 i)
	  {
	  	NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".GetFieldType(Int32)", LogLevel.Debug);
	    	  	
	  	  	
	  	//return Type.GetType(NpgsqlTypesHelper.GetSystemTypeNameFromTypeOid(_connection.OidToNameMapping, _currentResultset.RowDescription[i].type_oid));
	  	
	  	return NpgsqlTypesHelper.GetSystemTypeFromTypeOid(_connection.OidToNameMapping, _currentResultset.RowDescription[i].type_oid);
	  }
	  
	  public Object GetValue(Int32 i)
	  {
	  	NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".GetValue(Int32)", LogLevel.Debug);
	    
	    CheckCanRead();
	    
	  	if (i < 0 || _rowIndex < 0)
	  		throw new InvalidOperationException("Cannot read data.");
	  	return ((NpgsqlAsciiRow)_currentResultset[_rowIndex])[i];
	  	
	  	
	  }
	  
	  
	  public Int32 GetValues(Object[] values)
	  {
	  	NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".GetValues(Object[])", LogLevel.Debug);
	  	
	  	CheckCanRead();
	    
	  	// Only the number of elements in the array are filled.
	  	// It's also possible to pass an array with more that FieldCount elements.
	  	Int32 maxColumnIndex = (values.Length < FieldCount) ? values.Length : FieldCount;
	  	
	  	for (Int32 i = 0; i < maxColumnIndex; i++)
	  		values[i] = GetValue(i);
	  	
	  	return maxColumnIndex;
	  	
	  }
	  
	  public Int32 GetOrdinal(String name)
	  {
	    CheckCanRead();
	    return _currentResultset.RowDescription.FieldIndex(name);
	  }
	  
	  public Object this [ Int32 i ]
	  {
	  	get
	  	{
	  		NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".this[Int32]", LogLevel.Debug);
	  		return GetValue(i);
	  	}
	  }
	  
	  public Object this [ String name ]
	  {
	  	get
	  	{
		  	//throw new NotImplementedException();
	  		NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".this[String]", LogLevel.Debug);
	  		return GetValue(_currentResultset.RowDescription.FieldIndex(name));
	  	}
	  }
	  
	  public Boolean GetBoolean(Int32 i)
	  {
	    // Should this be done using the GetValue directly and not by converting to String
	  	// and parsing from there?
	  	NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".GetBoolean(Int32)", LogLevel.Debug);
	  	
  		  		
  		return (Boolean) GetValue(i);
	  	
	  }
	  
	  public Byte GetByte(Int32 i)
	  {
	    throw new NotImplementedException();
	  }
	  
	  public Int64 GetBytes(Int32 i, Int64 fieldOffset, Byte[] buffer, Int32 bufferoffset, Int32 length)
	  {
	    throw new NotImplementedException();
	  }
	  
	  public Char GetChar(Int32 i)
	  {
	    throw new NotImplementedException();
	  }
	  
	  public Int64 GetChars(Int32 i, Int64 fieldoffset, Char[] buffer, Int32 bufferoffset, Int32 length)
	  {
			String		str;

			str = GetString(i);
	  	if (buffer == null)
	  		return str.Length;
	  	
			str.ToCharArray(bufferoffset, length).CopyTo(buffer, 0);
			return buffer.GetLength(0);
	  }
	  
	  public Guid GetGuid(Int32 i)
	  {
	    throw new NotImplementedException();
	  }
	  
	  public Int16 GetInt16(Int32 i)
	  {
	    // Should this be done using the GetValue directly and not by converting to String
	  	// and parsing from there?
	  	NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".GetInt16(Int32)", LogLevel.Debug);
	  	/*try
	  	{
		    return Int16.Parse((String) this[i]);
	  	} catch (System.FormatException)
	  	{
	  		throw new System.InvalidCastException();
	  	}*/
	  	
	  	return (Int16) GetValue(i);
	  	

	  }
	  
	  
	  public Int32 GetInt32(Int32 i)
	  {
	    // Should this be done using the GetValue directly and not by converting to String
	  	// and parsing from there?
	  	NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".GetInt32(Int32)", LogLevel.Debug);
	  	/*try
	  	{
		    return Int32.Parse((String) this[i]);
	  	} catch (System.FormatException)
	  	{
	  		throw new System.InvalidCastException();
	  	}*/
	  	
	  	
	  	return (Int32) GetValue(i);
	  
	  }
	  
	  
	  public Int64 GetInt64(Int32 i)
	  {
	    // Should this be done using the GetValue directly and not by converting to String
	  	// and parsing from there?
	  	NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".GetInt64(Int32)", LogLevel.Debug);
	  	/*try
	  	{
		    return Int64.Parse((String) this[i]);
	  	} catch (System.FormatException)
	  	{
	  		throw new System.InvalidCastException();
	  	}*/
	  	return (Int64) GetValue(i);
	  }
	  
	  public Single GetFloat(Int32 i)
	  {
	    // Should this be done using the GetValue directly and not by converting to String
	  	// and parsing from there?
	  	NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".GetFloat(Int32)", LogLevel.Debug);
	  	try
	  	{
	  		return Single.Parse((String) this[i]);
	  	} catch (System.FormatException)
	  	{
	  		throw new System.InvalidCastException();
	  	}
	  }
	  
	  public Double GetDouble(Int32 i)
	  {
	    // Should this be done using the GetValue directly and not by converting to String
	  	// and parsing from there?
	  	NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".GetDouble(Int32)", LogLevel.Debug);
	  	try
	  	{
		    return Double.Parse((String) this[i]);
	  	} catch (System.FormatException)
	  	{
	  		throw new System.InvalidCastException();
	  	}
	  }
	  
	  public String GetString(Int32 i)
	  {
	  	NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".GetString(Int32)", LogLevel.Debug);
	    return (String) GetValue(i);
	  }
	  
	  public Decimal GetDecimal(Int32 i)
	  {
	    // Should this be done using the GetValue directly and not by converting to String
	  	// and parsing from there?
	  	NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".GetDecimal(Int32)", LogLevel.Debug);
	  	
	  	
	  	return (Decimal) GetValue(i);
	  }
	  
	  public DateTime GetDateTime(Int32 i)
	  {
	    //throw new NotImplementedException();
	  	return (DateTime) GetValue(i);
	  }
	  
	  public IDataReader GetData(Int32 i)
	  {
	    throw new NotImplementedException();
	  }
	  
	  public Boolean IsDBNull(Int32 i)
	  {
	  	
	  	CheckCanRead();
	    
	  	return ((NpgsqlAsciiRow)_currentResultset[_rowIndex]).IsNull(i);
	  }


		
	  
	  

		private DataTable GetResultsetSchema()
		{
			
			NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".GetResultsetSchema()", LogLevel.Debug);
			DataTable result = null;

			NpgsqlRowDescription rd = _currentResultset.RowDescription;
			Int16 numFields = rd.NumFields;
			if(numFields > 0) {
				result = new DataTable("SchemaTable");

				result.Columns.Add ("ColumnName", typeof (string));
				result.Columns.Add ("ColumnOrdinal", typeof (int));
				result.Columns.Add ("ColumnSize", typeof (int));
				result.Columns.Add ("NumericPrecision", typeof (int));
				result.Columns.Add ("NumericScale", typeof (int));
				result.Columns.Add ("IsUnique", typeof (bool));
				result.Columns.Add ("IsKey", typeof (bool));
				DataColumn dc = result.Columns["IsKey"];
				dc.AllowDBNull = true; // IsKey can have a DBNull
				result.Columns.Add ("BaseCatalogName", typeof (string));
				result.Columns.Add ("BaseColumnName", typeof (string));
				result.Columns.Add ("BaseSchemaName", typeof (string));
				result.Columns.Add ("BaseTableName", typeof (string));
				result.Columns.Add ("DataType", typeof(Type));
				result.Columns.Add ("AllowDBNull", typeof (bool));
				result.Columns.Add ("ProviderType", typeof (int));
				result.Columns.Add ("IsAliased", typeof (bool));
				result.Columns.Add ("IsExpression", typeof (bool));
				result.Columns.Add ("IsIdentity", typeof (bool));
				result.Columns.Add ("IsAutoIncrement", typeof (bool));
				result.Columns.Add ("IsRowVersion", typeof (bool));
				result.Columns.Add ("IsHidden", typeof (bool));
				result.Columns.Add ("IsLong", typeof (bool));
				result.Columns.Add ("IsReadOnly", typeof (bool));

				DataRow row;

				for (Int16 i = 0; i < numFields; i++) {
					row = result.NewRow();

					row["ColumnName"] = GetName(i);
					row["ColumnOrdinal"] = i + 1;
					row["ColumnSize"] = (int) rd[i].type_size;
					row["NumericPrecision"] = 0;
					row["NumericScale"] = 0;
					row["IsUnique"] = false;
					row["IsKey"] = DBNull.Value;
					row["BaseCatalogName"] = "";
					row["BaseColumnName"] = GetName(i);
					row["BaseSchemaName"] = "";
					row["BaseTableName"] = "";
					row["DataType"] = GetFieldType(i);
					row["AllowDBNull"] = false;
					row["ProviderType"] = (int) rd[i].type_oid;
					row["IsAliased"] = false;
					row["IsExpression"] = false;
					row["IsIdentity"] = false;
					row["IsAutoIncrement"] = false;
					row["IsRowVersion"] = false;
					row["IsHidden"] = false;
					row["IsLong"] = false;
					row["IsReadOnly"] = false;

					result.Rows.Add(row);
				}
			}

			return result;

		}
		
		
		
		IEnumerator IEnumerable.GetEnumerator () 
		{
			return new System.Data.Common.DbEnumerator (this);
		}
	}
}
