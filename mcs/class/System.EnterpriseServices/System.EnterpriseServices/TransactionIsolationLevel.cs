// 
// System.EnterpriseServices.TransactionIsolationLevel.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.EnterpriseServices {
	[Serializable]
	public enum TransactionIsolationLevel {
		Any = 0,
		ReadCommitted = 2,
		ReadUncommitted = 1,
		RepeatableRead = 3,
		Serializable = 4
	}
}
