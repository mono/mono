//
// System.Runtime.InteropServices.TypeLibFuncAttribute.cs
//
// Name: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;

namespace System.Runtime.InteropServices {

	[AttributeUsage (AttributeTargets.Method)]
	public sealed class TypeLibFuncAttribute : Attribute
	{
		TypeLibFuncFlags flags;
		
		[MonoTODO]
		public TypeLibFuncAttribute (short flags)
		{
		}

		public TypeLibFuncAttribute (TypeLibFuncFlags flags)
		{
			this.flags = flags;
		}

		public TypeLibFuncFlags Value {
			get { return flags; }
		}
	}
}
