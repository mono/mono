// 
// System.Web.Services.Protocols.SoapServerProtocol.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.IO;
using System.Web.Services;

namespace System.Web.Services.Protocols {
	[MonoTODO ("Figure out what this class does.")]
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

		[MonoTODO]
		public override Exception OnewayInitException {
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override bool Initialize ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override object[] ReadParameters ()
		{
				throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override bool WriteException (Exception e, Stream outputStream)
		{
				throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override void WriteReturns (object[] returnValues, Stream outputStream)
		{
				throw new NotImplementedException ();
		}

		#endregion
	}
}
