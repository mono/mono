//
// System.Data.ConnectionState.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

using System;

namespace mono.System.Data
{

	/// <summary>
	/// Returns the current state of the connection to a data source.
	/// This enumeration has a FlagsAttribute that allows a bitwise combination of its member values.
	/// </summary>
	[Flags]
	[Serializable]
	public enum ConnectionState
	{
		Closed = 0,
		Open = 1,
		Connecting = 2,
		Executing = 4,
		Fetching = 8,
		Broken = 16
	}
}