//
// System.Runtime.CompilerServices.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.InteropServices;

namespace System.Runtime.CompilerServices {

	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, Inherited=false)] [Serializable]
	public sealed class MethodImplAttribute : Attribute {
		MethodImplOptions impl_options;
		
		public MethodImplAttribute ()
		{
		}

		public MethodImplAttribute (short options)
		{
			impl_options = (MethodImplOptions) options;
		}

		public MethodImplAttribute (MethodImplOptions options)
		{
			impl_options = options;
		}

		public MethodCodeType MethodCodeType;

		public MethodImplOptions Value {
			get {
				return impl_options;
			}
		}
	}
}
