// 
// System.Web.Services.Protocols.SoapHeaderDirection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Protocols {
	[Flags]
	[Serializable]
	public enum SoapHeaderDirection {
		In = 0x1,
		InOut = 0x3,
		Out = 0x2
	}
}
