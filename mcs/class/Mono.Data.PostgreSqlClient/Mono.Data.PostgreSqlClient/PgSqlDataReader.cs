//
// System.Data.SqlClient.SqlDataReader.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc 2002
//
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;

namespace System.Data.SqlClient
{
	/// <summary>
	/// Provides a means of reading one or more forward-only streams
	/// of result sets obtained by executing a command 
	/// at a SQL database.
	/// </summary>
	//public sealed class SqlDataReader : MarshalByRefObject,
	//	IEnumerable, IDataReader, IDisposable, IDataRecord
	public sealed class SqlDataReader : IEnumerable, 
		IDataReader, IDataRecord
	{
		#region Public Methods

		[MonoTODO]
		public void Close()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataTable GetSchemaTable()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool NextResult()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool Read()
	        {
			throw new NotImplementedException ();
		}

		byte GetByte(int i)
		{
			throw new NotImplementedException ();
		}

		long GetBytes(int i, long fieldOffset, 
			byte[] buffer, int bufferOffset, 
			int length)
		{
			throw new NotImplementedException ();
		}

		char GetChar(int i)
		{
			throw new NotImplementedException ();
		}

		long GetChars(int i, long fieldOffset, 
			char[] buffer, int bufferOffset, 
			int length)
		{
			throw new NotImplementedException ();
		}

		IDataReader GetData(int i)
		{
			throw new NotImplementedException ();
		}

		string GetDataTypeName(int i)
		{
			throw new NotImplementedException ();
		}

		DateTime GetDateTime(int i)
		{
			throw new NotImplementedException ();
		}

		decimal GetDecimal(int i)
		{
			throw new NotImplementedException ();
		}

		double GetDouble(int i)
		{
			throw new NotImplementedException ();
		}

		Type GetFieldType(int i)
		{
			throw new NotImplementedException ();
		}

		float GetFloat(int i)
		{
			throw new NotImplementedException ();
		}

		Guid GetGuid(int i)
		{
			throw new NotImplementedException ();
		}

		short GetInt16(int i)
		{
			throw new NotImplementedException ();
		}

		int GetInt32(int i)
		{
			throw new NotImplementedException ();
		}

		long GetInt64(int i)
		{
			throw new NotImplementedException ();
		}

		string GetName(int i)
		{
			throw new NotImplementedException ();
		}

		int GetOrdinal(string name)
		{
			throw new NotImplementedException ();
		}

		string GetString(int i)
		{
			throw new NotImplementedException ();
		}

		object GetValue(int i)
		{
			throw new NotImplementedException ();
		}

		int GetValues(object[] values)
		{
			throw new NotImplementedException ();
		}

		bool IsDBNull(int i)
		{
			throw new NotImplementedException ();
		}

		bool GetBoolean(int i)
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region Properties

		[MonoTODO]
		public int Depth {
			get { 
				throw new NotImplementedException (); 
			}
		}

		[MonoTODO]
		public bool IsClosed {
			get {
				throw new NotImplementedException (); 
			}
		}

		[MonoTODO]
		public int RecordsAffected {
			get { 
				throw new NotImplementedException (); 
			}
		}
	
		int FieldCount {
			get { 
				throw new NotImplementedException ();
			}
		}

		object this[string name] {
			get { 
				throw new NotImplementedException ();
			}
		}

		object this[int i] {
			get 
			{ 
				throw new NotImplementedException ();
			}
		}

		#endregion
	}
}
