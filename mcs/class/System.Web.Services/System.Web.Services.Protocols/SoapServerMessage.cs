// 
// System.Web.Services.Protocols.SoapServerMessage.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services;
using System.IO;

namespace System.Web.Services.Protocols {
	public sealed class SoapServerMessage : SoapMessage {

		#region Fields

		string action;
		MethodStubInfo stubInfo;
		object server;
		string url;
		object[] parameters;

		#endregion

		#region Constructors

		internal SoapServerMessage (HttpRequest request, object server, Stream stream)
			: base (stream, null)
		{
			this.action = request.Headers ["SOAPAction"];
			this.server = server;
			this.url = request.Url.ToString();
		}

		internal SoapServerMessage (HttpRequest request, SoapException exception, MethodStubInfo stubInfo, object server, Stream stream)
			: base (stream, exception)
		{
			this.action = request.Headers ["SOAPAction"];
			this.stubInfo = stubInfo;
			this.server = server;
			this.url = request.Url.ToString();
		}

		#endregion

		#region Properties

		public override LogicalMethodInfo MethodInfo {
			get { return stubInfo.MethodInfo; }
		}

		public override string Action {
			get { return action; }
		}

		internal MethodStubInfo MethodStubInfo {
			get { return stubInfo; }
			set { stubInfo = value; }
		}

		public override bool OneWay {
			get { return stubInfo.OneWay; }
		}

		public object Server {
			get { return server; }
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
