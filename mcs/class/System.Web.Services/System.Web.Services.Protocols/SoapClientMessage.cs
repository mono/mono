// 
// System.Web.Services.Protocols.SoapClientMessage.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services;
using System.Web.Services.Protocols;

namespace System.Web.Services.Protocols {
	public sealed class SoapClientMessage : SoapMessage {

		#region Fields

		SoapHttpClientProtocol client;
		string url;
		LogicalMethodInfo client_method;
		SoapDocumentMethodAttribute sma;
		#endregion

		#region Constructors

		//
		// Constructs the SoapClientMessage
		//
		internal SoapClientMessage (SoapHttpClientProtocol client, SoapDocumentMethodAttribute sma,
					    LogicalMethodInfo client_method, bool one_way, string url)
		{
			this.sma = sma;
			this.client = client;
			this.client_method = client_method;
			this.url = url;
		}

		#endregion 

		#region Properties

		public override string Action {
			get { return sma.Action; }
		}

		public SoapHttpClientProtocol Client {
			get { return client; }
		}

		public override LogicalMethodInfo MethodInfo {
			get { return client_method; }
		}

		public override bool OneWay {
			get { return sma.OneWay; }
		}

		public override string Url {
			get { return url; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		protected override void EnsureInStage ()
		{
			//
			// I believe for SoapClientMessage, we can safely remove this check
			// as the In parameters are always available
			//
			throw new NotImplementedException ();
		}

		protected override void EnsureOutStage ()
		{
			EnsureStage (SoapMessageStage.AfterDeserialize);
		}

		#endregion // Methods
	}
}
