//
// System.Runtime.InteropServices.CoClassAttribute.cs
//
// Name: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

namespace System.Runtime.InteropServices {

	[AttributeUsage (AttributeTargets.Interface)]
	public sealed class CoClassAttribute : Attribute
	{

		Type klass;
		
		public CoClassAttribute (Type coClass)
		{
			klass = coClass;
		}

		public Type CoClass {
			get { return klass; }
		}
	}
}
