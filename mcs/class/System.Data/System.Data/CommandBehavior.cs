//
// System.Data.CommandBehavior.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

namespace System.Data
{

	/// <summary>
	/// Specifies a description of the results and the affect on the database of the query command.
	/// This enumeration has a FlagsAttribute that allows a bitwise combination of its member values.
	/// </summary>
	[Flags]
	[Serializable]
	public enum CommandBehavior
	{
		Default = 0,
		SingleResult = 1,
		SchemaOnly = 2,
		KeyInfo = 4,
		SingleRow = 8,
		SequentialAccess = 16,
		CloseConnection = 32
		
	}
}