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
		None,
		Notification,
		OneWay,
		RequestResponse,
		SolicitResponse
	}
}
