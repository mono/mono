//
// System.RuntimeTypeHandle.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Serialization;
using System.Globalization;

namespace System {
	
	//[MonoTODO]
	public struct RuntimeTypeHandle : ISerializable {
		IntPtr value;
		
		public IntPtr Value {
			get {
				return (IntPtr) value;
			}
		}
		
		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}
	}
}
