using System.Runtime.CompilerServices;

namespace System
{
	partial class Object
	{
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern Type GetType ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		protected extern object MemberwiseClone ();

		// TODO: Move to RuntimeHelpers
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern int InternalGetHashCode (object o);

		[Intrinsic]
		internal ref byte GetRawData () => throw new NotImplementedException ();

		internal object CloneInternal () => MemberwiseClone ();
	}
}
