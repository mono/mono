//
// Mono.Data.Tds.TdsRunBehavior.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace Mono.Data.Tds {
	[Serializable]
	public enum TdsParameterDirection
	{
		Input,
		Output,
		InputOutput,
		ReturnValue
	}
}
