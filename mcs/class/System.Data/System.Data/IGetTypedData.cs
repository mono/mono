//
// System.Data.IGetTypedData.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
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

#if NET_2_0

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

#endif // NET_2_0
