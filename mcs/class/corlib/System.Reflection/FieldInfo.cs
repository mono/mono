//
// System.Reflection.FieldInfo.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
// TODO: Mucho left to implement.
//

namespace System.Reflection {

	public abstract class FieldInfo : MemberInfo {

		public abstract Type FieldType { get; }

		public abstract object GetValue(object obj);

		// FIXME
		public bool IsLiteral { get { return true; } } 

		// FIXME
		public bool IsStatic { get { return false; } }
	}
}
