//
// System.Data.SqlTypes.SqlCompareOptions.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc. 2002
//

namespace System.Data.SqlTypes
{
	/// <summary>
	/// Specifies the compare option values for a SqlString structure.
	/// </summary>
	[Flags]
	[Serializable]
	public enum SqlCompareOptions {
		BinarySort = 0x8000,
		IgnoreCase = 0x1,
		IgnoreKanaType = 0x8,
		IgnoreNonSpace = 0x2,
		IgnoreWidth = 0x10,
		None = 0
	}

}


