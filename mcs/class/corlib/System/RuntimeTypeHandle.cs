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

	public struct RuntimeTypeHandle : ISerializable {

		public IntPtr Value {
			get {
				// FIXME: Implement me!
				return (IntPtr) 0;
			}
		}
		
                // This is from ISerializable
                public void GetObjectData (SerializationInfo info, StreamingContext context)
                {
                        // TODO: IMPLEMENT ME
                }

	}
}
