// 
// System.EnterpriseServices.TransactionOption.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.EnterpriseServices {
	[Serializable]
	public enum TransactionOption {
		Disabled = 0,
		NotSupported = 1,
		Supported = 2,
		Required = 3,
		RequiresNew = 4,
	}
}
