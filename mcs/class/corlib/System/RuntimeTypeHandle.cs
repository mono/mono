//
// System.RuntimeTypeHandle.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System {

	// FIXME: Implement me!
	public struct RuntimeTypeHandle : ISerializable {
		IntrPtr value;
		
		public IntPtr Value {
			get {
				return (IntPtr) value;
			}
		}
		
                // This is from ISerializable
                public void GetObjectData (SerializationInfo info, StreamingContext context)
                {
                        // TODO: IMPLEMENT ME.
                }

	}
}
