//
// System.Reflection.Emit/ILGenerator.cs
//
// Author:
//   Zoltan Varga (vargaz@freemail.hu)
//
// (C) 2004 Novell (http://www.novell.com)
//

namespace System.Reflection.Emit
{
	public sealed class MethodRental {

		public const int JitImmediate = 1;
		public const int JitOnDemand = 1;

		private MethodRental() {
		}

		[MonoTODO]
		public static void SwapMethodBody (Type cls, int methodtoken, IntPtr rgIL, int methodSize, int flags)
		{
			if (methodSize <= 0 || methodSize >= 0x3f0000) {
				throw new ArgumentException ("Data size must be > 0 and < 0x3f0000", "methodSize");
			}

			if (cls == null)
				throw new ArgumentNullException ("cls");
			if ((cls is TypeBuilder) && (! ((TypeBuilder)cls).is_created))
				throw new NotSupportedException ("Type '" + cls + "' is not yet created.");

			throw new NotImplementedException ();
		}
	}
}

