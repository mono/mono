//
// System.ContextStaticAttribute.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System
{
	/// <summary>
	///   The ContextStatic attribute is used to flag fields as being unique
	/// </summary>
	[AttributeUsage (AttributeTargets.Field)]
	[Serializable]
	public class ContextStaticAttribute : Attribute
	{
		public ContextStaticAttribute ()
			: base ()
		{
		}
	}
}
