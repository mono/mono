// 
// System.Web.Services.Protocols.SoapClientMethod.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services;
using System.Web.Services.Description;

namespace System.Web.Services.Protocols {

	internal class SoapClientMethod {

		#region Fields

		public string action;
		public LogicalMethodInfo methodInfo;
		public bool oneWay;
		public bool rpc;
		public SoapBindingUse use;

		#endregion // Fields
	}
}
