// 
// System.Web.Services.Protocols.SoapClientMessage.cs
//
// Authors:
//   Tim Coleman (tim@timcoleman.com)
//   Miguel de Icaza (miguel@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
// Copyright (C) Ximian, Inc. 2003
//

using System.Web.Services;
using System.Web.Services.Protocols;

namespace System.Web.Services.Protocols {
	public sealed class SoapClientMessage : SoapMessage {

		#region Fields

		SoapHttpClientProtocol client;
		string url;
		LogicalMethodInfo client_method;
		internal MethodStubInfo MethodStubInfo;

		//
		// Expose this one internally
		//
		internal object [] Parameters;
		#endregion

		#region Constructors

		//
		// Constructs the SoapClientMessage
		//
		internal SoapClientMessage (SoapHttpClientProtocol client, MethodStubInfo msi, string url, object [] parameters)
		{
			this.MethodStubInfo = msi;
			this.client = client;
			this.client_method = client_method;
			this.url = url;
			Parameters = parameters;
		}

		#endregion 

		#region Properties

		public override string Action {
			get { return MethodStubInfo.Action; }
		}

		public SoapHttpClientProtocol Client {
			get { return client; }
		}

		public override LogicalMethodInfo MethodInfo {
			get { return client_method; }
		}

		public override bool OneWay {
			get { return MethodStubInfo.OneWay; }
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
