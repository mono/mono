//
// System.Data.PropertyAttributes.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

using System;

namespace System.Data
{
	/// <summary>
	/// Specifies the attributes of a property.
	/// This enumeration has a FlagsAttribute that allows a bitwise combination of its member values
	/// </summary>
	[Flags]
	[Serializable]
	public enum PropertyAttributes
	{
		NotSupported = 0,
		Required = 1,
		Optional = 2,
		Read = 512,
		Write = 1024
	}
}