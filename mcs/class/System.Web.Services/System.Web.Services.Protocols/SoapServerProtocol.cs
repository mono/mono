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

		public override bool IsOneWay {
			get { return isOneWay; }
		}

		[MonoTODO]
		public override LogicalMethodInfo MethodInfo {
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties
	}
}
