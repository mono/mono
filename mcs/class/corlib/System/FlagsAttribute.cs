//
// System.FlagsAttribute.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System {

	/// <summary>
	///   The FlagsAttribute tags enumerations as bitfields.
	/// </summary>
	///
	/// <remarks>
	///   The FlagsAttribute can be used to tag an enumeration to be 
	///   a bit field.  This will allow the compiler and visual tools
	///   to treat the bits in an enumeration as a set of flags.
	/// </remarks>

	[AttributeUsage (AttributeTargets.Enum)]
	public class FlagsAttribute : Attribute {

		// No methods.
		
	}
}
