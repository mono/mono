//
// Mono.Data.SybaseTypes.SybaseCompareOptions.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using Mono.Data.SybaseClient;
using System;

namespace Mono.Data.SybaseTypes {
	/// <summary>
	/// Specifies the compare option values for a SybaseString structure.
	/// </summary>
	[Flags]
	[Serializable]
	public enum SybaseCompareOptions {
		BinarySort = 0x8000,
		IgnoreCase = 0x1,
		IgnoreKanaType = 0x8,
		IgnoreNonSpace = 0x2,
		IgnoreWidth = 0x10,
		None = 0
	}

}


