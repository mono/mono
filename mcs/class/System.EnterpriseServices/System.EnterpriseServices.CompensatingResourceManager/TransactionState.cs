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
		Aborted,
		Active,
		Committed,
		Indoubt
	}
}
