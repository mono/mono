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
		Anonymous,
		Default,
		Delegate,
		Identify,
		Impersonate
	}
}
