//
// System.NonSerializedAttribute.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

namespace System
{
	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Property)]
	public sealed class NonSerializedAttribute : Attribute
	{
		public NonSerializedAttribute ()
		{
		}
	}
}
