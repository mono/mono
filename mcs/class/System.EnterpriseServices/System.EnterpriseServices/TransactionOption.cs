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
		Disabled,
		NotSupported,
		Required,
		RequiresNew,
		Supported 
	}
}
