//
// System.Data.XmlWriteMode.cs
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
	/// Use the members of this enumeration when setting the WriteMode parameter of the WriteXml method.
	/// </summary>
	[Serializable]
	public enum XmlWriteMode
	{
		WriteSchema = 0,
		IgnoreSchema = 1,
		DiffGram = 2
	}
}