// 
// System.EnterpriseServices.CompensatingResourceManager.CompensatorOptions.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace System.EnterpriseServices.CompensatingResourceManager {
	[Flags]
	[Serializable]
	public enum CompensatorOptions {
		PreparePhase = 0x1,
		CommitPhase = 0x2,
		AbortPhase = 0x4,
		AllPhases =  0x7,
		FailIfInDoubtsRemain = 0x10
	}
}
