// 
// System.EnterpriseServices.AuthenticationOption.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.EnterpriseServices {
	[Serializable]
	public enum AuthenticationOption {
		Call,
		Connect,
		Default,
		Integrity,
		None,
		Packet,
		Privacy
	}
}
