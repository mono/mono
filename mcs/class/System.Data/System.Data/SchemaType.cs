//
// System.Data.SchemaType.cs
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
	/// Specifies how to handle existing schema mappings when performing a FillSchema operation.
	/// </summary>
	[Serializable]
	public enum SchemaType
	{
		Source = 1,
		Mapped = 2
	}
}