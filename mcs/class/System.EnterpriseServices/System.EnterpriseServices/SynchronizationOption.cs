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
		Disabled,
		NotSupported,
		Required,
		RequiresNew,
		Supported
	}
}
