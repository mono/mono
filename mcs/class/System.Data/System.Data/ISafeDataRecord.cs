//
// System.Data.ISafeDataRecord
//
// Author:
//   Boris Kirzner (borisk@mainsoft.com)
//
using System;

namespace System.Data
{
	internal interface ISafeDataRecord : IDataRecord
	{
		bool GetBooleanSafe(int i);

		byte GetByteSafe(int i);

		//long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferOffset, int length);

		char GetCharSafe(int i);

		//long GetChars(int i, long fieldOffset, char[] buffer, int bufferOffset, int length);

		//IDataReader GetData(int i);

		//string GetDataTypeName(int i);

		DateTime GetDateTimeSafe(int i);

		decimal GetDecimalSafe(int i);

		double GetDoubleSafe(int i);

		//Type GetFieldType(int i);

		float GetFloatSafe(int i);

		//Guid GetGuid(int i);

		short GetInt16Safe(int i);

		int GetInt32Safe(int i);

		long GetInt64Safe(int i);

		//string GetName(int i);

		//int GetOrdinal(string name);

		string GetStringSafe(int i);

		//object GetValue(int i);

		//int GetValues(object[] values);

		//bool IsDBNull(int i);

		//int FieldCount{get;}

		//object this[string name]{get;}
		
		//object this[int i]{get;}
	}
}
