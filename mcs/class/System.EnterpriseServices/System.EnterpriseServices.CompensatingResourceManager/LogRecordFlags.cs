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
		ForgetTarget = 0x1,
		WrittenDuringPrepare = 0x2,
		WrittenDuringCommit = 0x4,
		WrittenDuringAbort = 0x8,
		WrittenDurringRecovery = 0x10, // Typo present in .NET
		WrittenDuringReplay = 0x20,
		ReplayInProgress = 0x40
	}
}
