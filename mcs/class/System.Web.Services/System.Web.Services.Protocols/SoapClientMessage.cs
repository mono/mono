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

		#region Fields

		SoapHttpClientProtocol client;
		SoapClientMethod clientMethod;
		string url;

		#endregion

		#region Constructors
		
		[MonoTODO ("Determine what calls this constructor.")]
		internal SoapClientMessage (SoapHttpClientProtocol client, SoapClientMethod clientMethod, string url)
		{
			this.client = client;
			this.url = url;
			this.clientMethod = clientMethod;
		}

		#endregion 

		#region Properties

		public override string Action {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public SoapHttpClientProtocol Client {
			get { return client; }
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
			get { return url; }
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
