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
	/// compare option values for SqlString
	/// </summary>
	[Flags]
	[Serializable]
	public enum SqlCompareOptions {
		BinarySort,
		IgnoreCase,
		IgnoreKanaType,
		IgnoreNonSpace,
		IgnoreWidth,
		None
	}

}


