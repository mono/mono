//
// System.Data.MappingType.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

using System;

namespace mono.System.Data
{
	/// <summary>
	/// Specifies how a DataColumn is mapped.
	/// </summary>
	[Serializable]
	public enum MappingType
	{
		Element = 1,
		Attribute = 2,
		SimpleContent = 3,
		Hidden = 4
		
	}
}