//
// System.Data.DataRowVersion.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

namespace System.Data
{
	/// <summary>
	/// Describes the version of a DataRow.
	/// </summary>
	[Serializable]
	public enum DataRowVersion
	{
		Original = 256,
		Current = 512,
		Proposed = 1024,
		Default = 1536
	}
}