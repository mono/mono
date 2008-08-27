//
// System.Reflection/Pointer.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
//
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

using System;
using System.Reflection;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;

namespace System.Reflection {

#if NET_2_0
	[ComVisible (true)]
	[Serializable]
#endif
	[CLSCompliant(false)]
	public unsafe sealed class Pointer : ISerializable {
		void *data;
#pragma warning disable 169, 414
		Type type;
#pragma warning restore 169, 414

		private Pointer () {
		}
		
		public static object Box (void *ptr, Type type) 
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			if (!type.IsPointer)
				throw new ArgumentException ("type");
			Pointer res = new Pointer ();
			res.data = ptr;
			res.type = type;
			return res;
		}

		public static void* Unbox (object ptr)
		{
			Pointer p = ptr as Pointer;
			if (p == null)
				throw new ArgumentException ("ptr");
			return p.data;
		}

		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotSupportedException ("Pointer deserializatioon not supported.");
		}
	}
}
