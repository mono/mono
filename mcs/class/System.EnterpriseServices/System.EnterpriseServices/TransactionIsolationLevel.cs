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
		Any,
		ReadCommitted,
		ReadUncommitted,
		RepeatableRead,
		Serializable
	}
}
