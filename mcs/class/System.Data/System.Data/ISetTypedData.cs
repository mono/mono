//
// System.Data.ISetTypedData.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

namespace System.Data {
	public interface ISetTypedData
	{
		#region Methods

		void SetBoolean (int i, bool value);
		void SetByte (int i, byte value);
		void SetBytes (int i, long fieldOffset, byte[] buffer, int bufferOffset, int length);
		void SetChar (int i, char value);
		void SetChars (int i, long fieldOffset, char[] buffer, int bufferOffset, int length);
		void SetDateTime (int i, DateTime value);
		void SetDecimal (int i, decimal value);
		void SetDouble (int i, double value);
		void SetFloat (int i, float value);
		void SetGuid (int i, Guid value);
		void SetInt16 (int i, short value);
		void SetInt32 (int i, int value);
		void SetInt64 (int i, long value);
		void SetObjectRef (int i, object o);
		void SetString (int i, string value);
		void SetValue (int i, object value);

		#endregion // Methods
	}
}

#endif // NET_1_2
