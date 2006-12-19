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

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Web.Services;
using System.Web.Services.Protocols;

namespace System.Web.Services.Protocols {
	public sealed class SoapClientMessage : SoapMessage {

		#region Fields

		SoapHttpClientProtocol client;
		string url;
		internal SoapMethodStubInfo MethodStubInfo;

		//
		// Expose this one internally
		//
		internal object [] Parameters;
		#endregion

		#region Constructors

		//
		// Constructs the SoapClientMessage
		//
		internal SoapClientMessage (SoapHttpClientProtocol client, SoapMethodStubInfo msi, string url, object [] parameters)
		{
			this.MethodStubInfo = msi;
			this.client = client;
			this.url = url;
			Parameters = parameters;
#if NET_2_0
			if (SoapVersion == SoapProtocolVersion.Soap12)
				ContentType = "application/soap+xml";
#endif
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
			get { return MethodStubInfo.MethodInfo; }
		}

		public override bool OneWay {
			get { return MethodStubInfo.OneWay; }
		}

		public override string Url {
			get { return url; }
		}
		
#if NET_2_0
		[System.Runtime.InteropServices.ComVisible(false)]
		public override SoapProtocolVersion SoapVersion {
			get { return client.SoapVersion; }
		}
#endif

		#endregion // Properties

		#region Methods

		protected override void EnsureInStage ()
		{
			EnsureStage (SoapMessageStage.BeforeSerialize);
		}

		protected override void EnsureOutStage ()
		{
			EnsureStage (SoapMessageStage.AfterDeserialize);
		}

		#endregion // Methods
	}
}
