// 
// System.Web.Services.Description.OperationFlow.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Description {
	[Serializable]
	public enum OperationFlow {
		None = 0x0,
		Notification = 0x2,
		OneWay = 0x1,
		RequestResponse = 0x3,
		SolicitResponse = 0x4
	}
}
