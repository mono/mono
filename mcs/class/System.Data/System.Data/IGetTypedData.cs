//
// System.Data.IGetTypedData.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

namespace System.Data {
	public interface IGetTypedData
	{
		#region Methods

		bool GetBoolean (int i);
		byte GetByte (int i);
		long GetBytes (int i, long fieldOffset, byte[] buffer, int bufferOffset, int length);
		char GetChar (int i);
		long GetChars (int i, long fieldOffset, char[] buffer, int bufferOffset, int length);
		DateTime GetDateTime (int i);
		decimal GetDecimal (int i);
		double GetDouble (int i);
		float GetFloat (int i);
		Guid GetGuid (int i);
		short GetInt16 (int i);
		int GetInt32 (int i);
		long GetInt64 (int i);
		object GetObjectRef (int i);
		string GetString (int i);
		object GetValue (int i);
		bool IsDBNull (int i);
		bool IsSetAsDefault (int i);

		#endregion // Methods
	}
}

#endif // NET_1_2
