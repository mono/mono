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
	public enum IsolationLevel
	{
		Chaos,
		ReadCommitted,
		ReadUnCommitted,
		RepeatableRead,
		Serializable,
		Unspecified

	}
}