//
// System.Data.IsolationLevel.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

namespace System.Data
{
	/// <summary>
	/// Specifies the transaction locking behavior for the connection.
	/// This enumeration has a FlagsAttribute that allows a bitwise combination of its member values.
	/// </summary>
	[Flags]
	[Serializable]
	public enum IsolationLevel
	{
		Unspecified = -1,
		Chaos = 16,
		ReadUncommitted = 256,
		ReadCommitted = 4096,
		RepeatableRead = 65536,
		Serializable = 1048576
	}
}