//
// System.Data.ConnectionState.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

namespace System.Data
{

	/// <summary>
	/// Returns the current state of the connection to a data source.
	/// This enumeration has a FlagsAttribute that allows a bitwise combination of its member values.
	/// </summary>
	public enum ConnectionState
	{
		Broken,
		Closed,
		Connecting,
		Executing,
		Fetching,
		Open
	}
}