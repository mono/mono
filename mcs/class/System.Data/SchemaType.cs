//
// System.Data.SchemaType.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

namespace System.Data
{
	/// <summary>
	/// Specifies how to handle existing schema mappings when performing a FillSchema operation.
	/// </summary>
	public enum SchemaType
	{
		Mapped,
		Source
	}
}