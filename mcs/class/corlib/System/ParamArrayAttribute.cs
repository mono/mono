//
// System.ParamArrayAttribute.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System
{
	/// <summary>
	///   Used to flag that the method will take a variable number
	///   of arguments
	/// </summary>
	[AttributeUsage (AttributeTargets.Parameter)]
	public sealed class ParamArrayAttribute : Attribute
	{
		public ParamArrayAttribute ()
		{
		}
	}
}
