//
// System.Reflection/Pointer.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
//
//

using System;
using System.Reflection;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace System.Reflection {

	[Serializable]
	[CLSCompliant(false)]
	public unsafe sealed class Pointer : ISerializable {

		void *data;
		Type type;

		private Pointer () {
		}
		
		public static Pointer Box (void *ptr, Type type) 
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
