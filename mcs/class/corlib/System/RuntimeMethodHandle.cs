//
// System.RuntimeMethodHandle.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Serialization;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace System {

	//[MonoTODO]
	[Serializable]
	public struct RuntimeMethodHandle : ISerializable {
		IntPtr value;

		internal RuntimeMethodHandle (IntPtr v) {
			value = v;
		}
		
		public IntPtr Value {
			get {
				return (IntPtr) value;
			}
		}
		
                // This is from ISerializable
		[MonoTODO]
                public void GetObjectData (SerializationInfo info, StreamingContext context)
                {
                        if (info == null)
                                throw new ArgumentNullException ("info");
                        
                        throw new NotImplementedException ();
                }

		[MethodImpl (MethodImplOptions.InternalCall)]
		public static extern IntPtr GetFunctionPointer (IntPtr m);
		
		public IntPtr GetFunctionPointer ()
		{
			return GetFunctionPointer (value);
		}
	}
}
