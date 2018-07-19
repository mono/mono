//
// HttpsTransportBindingElement.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Xml;

namespace System.ServiceModel.Channels
{
	[MonoTODO]
	public class HttpsTransportBindingElement
		: HttpTransportBindingElement, ITransportTokenAssertionProvider,
		IPolicyExportExtension, IWsdlExportExtension
	{
		bool req_cli_cert = false;

		public HttpsTransportBindingElement ()
		{
		}

		protected HttpsTransportBindingElement (
			HttpsTransportBindingElement elementToBeCloned)
			: base (elementToBeCloned)
		{
			req_cli_cert = elementToBeCloned.req_cli_cert;
		}

		public bool RequireClientCertificate {
			get { return req_cli_cert; }
			set { req_cli_cert = value; }
		}

		public override string Scheme {
			get { return Uri.UriSchemeHttps; }
		}

		public override IChannelFactory<TChannel> BuildChannelFactory<TChannel> (
			BindingContext context)
		{
			return base.BuildChannelFactory <TChannel> (context);
		}

#if !MOBILE && !XAMMAC_4_5
		[MonoTODO]
		public override IChannelListener<TChannel>
			BuildChannelListener<TChannel> (
			BindingContext context)
		{
			return base.BuildChannelListener <TChannel> (context);
		}
#endif

		public override BindingElement Clone ()
		{
			return new HttpsTransportBindingElement (this);
		}

#if !MOBILE && !XAMMAC_4_5
		public XmlElement GetTransportTokenAssertion ()
		{
			var doc = new XmlDocument ();
			var token = doc.CreateElement ("sp", "HttpsToken", PolicyImportHelper.SecurityPolicyNS);
			token.SetAttribute ("RequireClientCertificate", req_cli_cert ? "true" : "false");
			return token;
		}
#endif

		// overriden only in full profile
		public override T GetProperty<T> (BindingContext context)
		{
			if (typeof (T) == typeof (ISecurityCapabilities))
				return (T) (object) new HttpsBindingProperties (this);
			return base.GetProperty<T> (context);
		}
	}

	class HttpsBindingProperties : HttpBindingProperties
	{
		HttpsTransportBindingElement source;

		public HttpsBindingProperties (HttpsTransportBindingElement source)
			: base (source)
		{
			this.source = source;
		}

		public override ProtectionLevel SupportedRequestProtectionLevel {
			get { return ProtectionLevel.EncryptAndSign; }
		}

		public override ProtectionLevel SupportedResponseProtectionLevel {
			get { return ProtectionLevel.EncryptAndSign; }
		}

		public override bool SupportsClientAuthentication {
			get { return source.RequireClientCertificate || base.SupportsClientAuthentication; }
		}

		public override bool SupportsServerAuthentication {
			get { return true; }
		}

		public override bool SupportsClientWindowsIdentity {
			get { return source.RequireClientCertificate || base.SupportsClientWindowsIdentity; }
		}
	}
}
