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
	public enum CommandBehavior
	{
		CloseConnection, 
		KeyInfo, 
		SchemaOnly, 
		SequentialAccess, 
		SingleResult, 
		SingleRow
	}
}