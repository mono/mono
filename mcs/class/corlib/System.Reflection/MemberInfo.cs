//
// System.Reflection.MemberInfo.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
// TODO: Mucho left to implement.
//

using System.Runtime.InteropServices;

namespace System.Reflection {

	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public abstract class MemberInfo : ICustomAttributeProvider {

		internal MemberInfo () {}
		
		public abstract Type DeclaringType {
			get;
		}

		public abstract MemberTypes MemberType {
			get;
		}

		public abstract string Name {
			get;
		}

		public abstract Type ReflectedType {
			get;
		}

		public abstract bool IsDefined (Type attribute_type, bool inherit);

		public abstract object [] GetCustomAttributes (bool inherit);

		public abstract object [] GetCustomAttributes (Type attribute_type, bool inherit);
	}
}
