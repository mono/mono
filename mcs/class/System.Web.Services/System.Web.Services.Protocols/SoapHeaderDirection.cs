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
		In,
		InOut,
		Out
	}
}
