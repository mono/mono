//
// System.Runtime.InteropServices.DispIdAttribute.cs
//
// Name: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.

namespace System.Runtime.InteropServices {

	[AttributeUsage (AttributeTargets.Method | AttributeTargets.Property |
			 AttributeTargets.Field | AttributeTargets.Event)]
	public sealed class DispIdAttribute : Attribute
	{
		int id;
		
		public DispIdAttribute (int dispId)
		{
			id = dispId;
		}

		public int Value {
			get { return id; }
		}
	}
}
