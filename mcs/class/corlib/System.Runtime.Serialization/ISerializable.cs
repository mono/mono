//
// System.Runtime.Serialization.ISerializable.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
//

namespace System.Runtime.Serialization {
	public interface ISerializable {
		void GetObjectData (SerializationInfo info, StreamingContext context);
	}
}
