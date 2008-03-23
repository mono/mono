//
// System.Data.IDataRecord.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

namespace System.Data
{
	/// <summary>
	/// Provides access to the column values within 
	/// each row for a DataReader, and is implemented by .NET data 
	/// providers that access relational databases.
	/// </summary>
	public interface IDataRecord
	{
		bool GetBoolean(int i);

		byte GetByte(int i);

		long GetBytes(int i, long fieldOffset, byte[] buffer,
			int bufferoffset, int length);

		char GetChar(int i);

		long GetChars (int i, long fieldoffset, char [] buffer,
			int bufferoffset, int length);

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
