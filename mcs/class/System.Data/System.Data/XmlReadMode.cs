//
// System.Data.XmlReadMode.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

namespace System.Data
{
	/// <summary>
	/// Specifies how to read XML data and a relational schema into a DataSet.
	/// </summary>
	public enum XmlReadMode
	{
		Auto,
		DiffGram,
		Fragment,
		IgnoreSchema,
		InferSchema,
		ReadSchema
	}
}