//
// System.Data.ParameterDirection.cs
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
	/// Specifies the type of a parameter within a query relative to the DataSet.
	/// </summary>
	[Serializable]
	public enum ParameterDirection
	{
		Input = 1,
		Output = 2,
		InputOutput = 3,
		ReturnValue = 6
	}
}