//
// System.Data.PropertyAttributes.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

namespace System.Data
{
	/// <summary>
	/// Specifies the attributes of a property.
	/// This enumeration has a FlagsAttribute that allows a bitwise combination of its member values
	/// </summary>
	public enum PropertyAttributes
	{
		NotSupported,
		Optional,
		Read,
		Required,
		Write
	}
}