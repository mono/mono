//
// System.Data.CommandType.cs
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
	/// Specifies how a command string is interpreted.
	/// </summary>
	[Serializable]
	public enum CommandType
	{
		Text = 1,
		StoredProcedure = 4,
		TableDirect = 512
		
	}
}
