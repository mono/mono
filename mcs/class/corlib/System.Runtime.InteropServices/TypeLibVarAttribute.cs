//
// System.Runtime.InteropServices.TypeLibTypeAttribute.cs
//
// Name: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;

namespace System.Runtime.InteropServices {

	[AttributeUsage (AttributeTargets.Method)]
	public sealed class TypeLibVarAttribute : Attribute
	{
		TypeLibVarFlags flags;
		
		public TypeLibVarAttribute (short flags)
		{
			this.flags = (TypeLibVarFlags) flags;
		}

		public TypeLibVarAttribute (TypeLibVarFlags flags)
		{
			this.flags = flags;
		}

		public TypeLibVarFlags Value {
			get { return flags; }
		}
	}
}
