//
// System.Data.ParameterDirection.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

namespace System.Data
{
	/// <summary>
	/// Specifies the type of a parameter within a query relative to the DataSet.
	/// </summary>
	public enum ParameterDirection
	{
		Input,
		InputOutput,
		Output,
		ReturnValue
	}
}