//
// IDebugConvert.cs:
//
// Author:
//	 Cesar Lopez Nataren
//
// (C) 2005, Novell Inc. (http://novell.com)
//

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

using System;
using Microsoft.Vsa;
using System.Runtime.InteropServices;

namespace Microsoft.JScript {
	
	[GuidAttribute ("AA51516D-C0F2-49fe-9D38-61D20456904C")]
	[ComVisibleAttribute (true)]
	public interface IDebugConvert {

		string BooleanToString (bool value);
		string ByteToString (byte value, int radix);
		string DoubleToDateString (double value);
		string DoubleToString (double value);
		string GetErrorMessageForHR (int hr, IVsaEngine engine);
		object GetManagedCharObject (ushort i);
		object GetManagedInt64Object (long i);
		object GetManagedObject (object value);
		object GetManagedUInt64Object (ulong i);
		string Int16ToString (short value, int radix);
		string Int32ToString (int value, int radix);
		string Int64ToString (long value, int radix);
		string RegexpToString (string source, bool ignoreCase, bool global, bool multiline);
		string SByteToString (sbyte value, int radix);
		string SingleToString (float value);
		string StringToPrintable (string source);
		object ToPrimitive (object value, TypeCode typeCode, bool truncationPermitted);
		string UInt16ToString (ushort value, int radix);
		string UInt32ToString (uint value, int radix);
		string UInt64ToString (ulong value, int radix);
	}
}
