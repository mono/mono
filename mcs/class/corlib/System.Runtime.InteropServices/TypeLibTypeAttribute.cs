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
	public sealed class TypeLibTypeAttribute : Attribute
	{
		TypeLibTypeFlags flags;
		
		public TypeLibTypeAttribute (short flags)
		{
			this.flags = (TypeLibTypeFlags) flags;
		}

		public TypeLibTypeAttribute (TypeLibTypeFlags flags)
		{
			this.flags = flags;
		}

		public TypeLibTypeFlags Value {
			get { return flags; }
		}
	}
}
