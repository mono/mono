// 
// System.Web.Services.Protocols.SoapServerProtocol.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services;

namespace System.Web.Services.Protocols {
	[MonoTODO ("Figure out what thsi class does.")]
	internal class SoapServerProtocol : ServerProtocol {

		#region Fields

		bool isOneWay;

		#endregion // Fields

		#region Properties

		public bool IsOneWay {
			get { return isOneWay; }
			set { isOneWay = value; }
		}

		[MonoTODO]
		public LogicalMethodInfo MethodInfo {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		#endregion // Properties
	}
}
