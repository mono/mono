// 
// System.EnterpriseServices.CompensatingResourceManager.LogRecordFlags.cs
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
	public enum LogRecordFlags {
		ForgetTarget,
		ReplayInProgress,
		WrittenDuringAbort,
		WrittenDuringCommit,
		WrittenDuringPrepare,
		WrittenDuringReplay,
		WrittenDuringRecovery
	}
}
