//
// System.Data.XmlReadMode.cs
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
	/// Specifies how to read XML data and a relational schema into a DataSet.
	/// </summary>
	[Serializable]
	public enum XmlReadMode
	{
		Auto = 0,
		ReadSchema = 1,
		IgnoreSchema = 2,
		InferSchema = 3,
		DiffGram = 4,
		Fragment = 5
	}
}