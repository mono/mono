//
// System.CLSCompliantAttribute.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System
{
	/// <summary>
	///   Used to indicate if an element of a program is CLS compliant.
	/// </summary>
	[AttributeUsage (AttributeTargets.All)]
	[Serializable]
	public sealed class CLSCompliantAttribute : Attribute
	{
		bool is_compliant;

		public CLSCompliantAttribute (bool isCompliant)
		{
			this.is_compliant = isCompliant;
		}

		public bool IsCompliant {
			get {
				return is_compliant;
			}
		}
	}
}
