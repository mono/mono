// 
// System.Web.Services.Description.OperationFlow.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services;

namespace System.Web.Services.Description {
	public enum OperationFlow {
		None,
		Notification,
		OneWay,
		RequestResponse,
		SolicitResponse
	}
}
