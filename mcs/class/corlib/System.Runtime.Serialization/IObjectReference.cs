//
// System.Runtime.Serialization.IObjectReference.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.Runtime.Serialization {

	public interface IObjectReference {
		object GetRealObject (StreamingContext context);
	}
}

