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
		#region Fields

		private DataTable table = null;

		#endregion // Fields

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

		[MonoTODO]
		public byte GetByte(int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public long GetBytes(int i, long fieldOffset, 
			byte[] buffer, int bufferOffset, 
			int length)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public char GetChar(int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public long GetChars(int i, long fieldOffset, 
			char[] buffer, int bufferOffset, 
			int length)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IDataReader GetData(int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetDataTypeName(int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DateTime GetDateTime(int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public decimal GetDecimal(int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public double GetDouble(int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Type GetFieldType(int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public float GetFloat(int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Guid GetGuid(int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public short GetInt16(int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int GetInt32(int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public long GetInt64(int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetName(int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int GetOrdinal(string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetString(int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object GetValue(int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int GetValues(object[] values)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsDBNull(int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool GetBoolean(int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IEnumerator GetEnumerator() {
			throw new NotImplementedException ();
		}

		#endregion // Public Methods

		#region Destructors

		[MonoTODO]
		public void Dispose () {
		}

		[MonoTODO]
		~SqlDataReader() {
		}

		#endregion // Destructors


		#region Properties

		public int Depth {
			[MonoTODO]
			get { 
				throw new NotImplementedException (); 
			}
		}

		public bool IsClosed {
			[MonoTODO]
			get {
				throw new NotImplementedException (); 
			}
		}

		public int RecordsAffected {
			[MonoTODO]
			get { 
				throw new NotImplementedException (); 
			}
		}
	
		public int FieldCount {
			[MonoTODO]
			get { 
				throw new NotImplementedException ();
			}
		}

		public object this[string name] {
			[MonoTODO]
			get { 
				throw new NotImplementedException ();
			}
		}

		public object this[int i] {
			[MonoTODO]
			get 
			{ 
				throw new NotImplementedException ();
			}
		}

		#endregion // Properties
	}
}
