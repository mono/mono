//
// System.Data.ISetTypedData.cs
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

#endif // NET_2_0
