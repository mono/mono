// 
// System.Web.Services.Protocols.SoapClientMessage.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services;

namespace System.Web.Services.Protocols {
	public sealed class SoapClientMessage : SoapMessage {

		#region Properties

		public override string Action {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public SoapHttpClientProtocol Client {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public override LogicalMethodInfo MethodInfo {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public override bool OneWay {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public override string Url {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		protected override void EnsureInStage ()
		{
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		protected override void EnsureOutStage ()
		{
			throw new NotImplementedException (); 
		}

		#endregion // Methods
	}
}
