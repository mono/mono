// 
// System.EnterpriseServices.SynchronizationOption.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace System.EnterpriseServices {
	[Serializable]
	public enum SynchronizationOption {
		Disabled  =0,
		NotSupported = 1,
		Required = 3,
		RequiresNew = 4,
		Supported = 2
	}
}
