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
		AbortPhase,
		AllPhases,
		CommitPhase,
		FailIfInDoubtsRemain,
		PreparePhase
	}
}
