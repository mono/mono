//
// System.Data.XmlWriteMode.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

namespace System.Data
{
	/// <summary>
	/// Use the members of this enumeration when setting the WriteMode parameter of the WriteXml method.
	/// </summary>
	public enum XmlWriteMode
	{
		DiffGram,
		IgnoreSchema,
		WriteSchema
	}
}