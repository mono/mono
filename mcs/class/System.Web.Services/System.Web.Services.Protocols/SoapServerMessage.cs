// 
// System.Web.Services.Protocols.SoapServerMessage.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services;

namespace System.Web.Services.Protocols {
	public sealed class SoapServerMessage : SoapMessage {

		#region Fields

		string action;
		LogicalMethodInfo methodInfo;
		bool oneWay;
		object server;
		string url;
		SoapServerProtocol protocol;

		#endregion

		#region Constructors

		[MonoTODO ("Determine what this constructor does.")]
		internal SoapServerMessage (SoapServerProtocol protocol)
		{
		}

		#endregion

		#region Properties

		public override string Action {
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

		public object Server {
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
