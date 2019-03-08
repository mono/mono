using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Reflection {
	[StructLayout (LayoutKind.Sequential)]
	public class RuntimeLocalVariableInfo : LocalVariableInfo {
		#region Keep in sync with MonoReflectionLocalVariableInfo in object-internals.h
		internal Type type;
		internal bool is_pinned;
		internal ushort position;
		#endregion

		protected RuntimeLocalVariableInfo ()
		{
		}

		public override bool IsPinned => is_pinned;

		public override int LocalIndex => position;

		public override Type LocalType => type;
	}

}

