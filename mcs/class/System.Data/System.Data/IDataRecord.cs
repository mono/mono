//
// System.Data.IDataRecord.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

namespace System.Data
{
	/// <summary>
	/// Provides access to the column values within each row for a DataReader, and is implemented by .NET data providers that access relational databases.
	/// </summary>
	public interface IDataRecord
	{
		bool GetBoolean(int i);

		byte GetByte(int i);

		long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferOffset, int length);

		char GetChar(int i);

		long GetChars(int i, long fieldOffset, char[] buffer, int bufferOffset, int length);

		IDataReader GetData(int i);

		string GetDataTypeName(int i);

		DateTime GetDateTime(int i);

		decimal GetDecimal(int i);

		double GetDouble(int i);

		Type GetFieldType(int i);

		float GetFloat(int i);

		Guid GetGuid(int i);

		short GetInt16(int i);

		int GetInt32(int i);

		long GetInt64(int i);

		string GetName(int i);

		int GetOrdinal(string name);

		string GetString(int i);

		object GetValue(int i);

		int GetValues(object[] values);

		bool IsDBNull(int i);

		int FieldCount{get;}

		object this[string name]{get;}
		
		object this[int i]{get;}
	}
}