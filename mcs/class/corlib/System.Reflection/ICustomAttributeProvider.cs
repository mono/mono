//
// System.Reflection.ICustomAttributeProvider.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
// TODO: Mucho left to implement.
//

namespace System.Reflection {

	public interface ICustomAttributeProvider {

		object [] GetCustomAttributes (bool inherit);
		object [] GetCustomAttributes (Type attribute_type, bool inherit);

		/// <summary>
		///   Probes whether one or more `attribute_type' types are
		///   defined by this member
		/// </summary>
		bool IsDefined (Type attribute_type, bool inherit);
	}
}
