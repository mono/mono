//
// System.CLSCompliantAttribute.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System {

	/// <summary>
	///   Used to indicate if an element of a program is CLS compliant.
	/// </summary>
	///
	/// <remarks>
	/// </remarks>
	[AttributeUsage(AttributeTargets.All)]
	public class CLSCompliant: Attribute {

		bool is_compliant;

		public CLSCompliant(bool is_compliant)
		{
			this.is_compliant = is_compliant;
		}

		public bool IsCompliant {
			get {
				return is_compliant;
			}
		}
	}
}
