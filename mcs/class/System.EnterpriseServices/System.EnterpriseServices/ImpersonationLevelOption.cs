// 
// System.EnterpriseServices.ImpersonationLevelOption.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.EnterpriseServices {
	[Serializable]
	public enum ImpersonationLevelOption {
		Anonymous = 1,
		Default = 0,
		Delegate = 4,
		Identify = 2,
		Impersonate = 3
	}
}
