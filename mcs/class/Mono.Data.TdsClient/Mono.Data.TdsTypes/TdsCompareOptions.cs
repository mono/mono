//
// Mono.Data.TdsTypes.TdsCompareOptions.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using Mono.Data.TdsClient;
using System;

namespace Mono.Data.TdsTypes {
	/// <summary>
	/// Specifies the compare option values for a TdsString structure.
	/// </summary>
	[Flags]
	[Serializable]
	public enum TdsCompareOptions {
		BinarySort = 0x8000,
		IgnoreCase = 0x1,
		IgnoreKanaType = 0x8,
		IgnoreNonSpace = 0x2,
		IgnoreWidth = 0x10,
		None = 0
	}

}


