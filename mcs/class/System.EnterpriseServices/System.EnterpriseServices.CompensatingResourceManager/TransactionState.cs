// 
// System.EnterpriseServices.CompensatingResourceManager.TransactionState.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace System.EnterpriseServices.CompensatingResourceManager {
	[Serializable]
	public enum TransactionState {
		Active = 0x0,
		Committed = 0x1,
		Aborted = 0x2,
		Indoubt = 0x3
	}
}
