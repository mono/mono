//
// SslStreamSecurityBindingElement.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Security;
using System.Security.Authentication;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.Xml;

namespace System.ServiceModel.Channels
{
	[MonoTODO]
	public class SslStreamSecurityBindingElement
		: BindingElement, ITransportTokenAssertionProvider, IPolicyExportExtension
	{
		public SslStreamSecurityBindingElement ()
		{
#if !MOBILE && !XAMMAC_4_5
			verifier = IdentityVerifier.CreateDefault ();
#endif
		}

#if !MOBILE && !XAMMAC_4_5
		IdentityVerifier verifier;

		public IdentityVerifier IdentityVerifier {
			get { return verifier; }
			set { verifier = value; }
		}
#endif

		bool require_client_certificate;

		public bool RequireClientCertificate {
			get { return require_client_certificate; }
			set { require_client_certificate = value; }
		}

		[MonoTODO]
		public SslProtocols SslProtocols {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		private SslStreamSecurityBindingElement (
			SslStreamSecurityBindingElement other)
			: base (other)
		{
#if !MOBILE && !XAMMAC_4_5
			verifier = other.verifier;
#endif
			require_client_certificate = other.require_client_certificate;
		}

#if !MOBILE && !XAMMAC_4_5
		[MonoTODO]
		public StreamUpgradeProvider BuildClientStreamUpgradeProvider (BindingContext context)
		{
			return new SslStreamSecurityUpgradeProvider (this);
		}

		[MonoTODO]
		public StreamUpgradeProvider BuildServerStreamUpgradeProvider (BindingContext context)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlElement GetTransportTokenAssertion ()
		{
			var doc = new XmlDocument ();
			var element = doc.CreateElement (
				"msf", "SslTransportSecurity", PolicyImportHelper.FramingPolicyNS);
			return element;
		}
#endif

		[MonoTODO]
		public override IChannelFactory<TChannel>
			BuildChannelFactory<TChannel> (
			BindingContext context)
		{
			throw new NotImplementedException ();
		}

#if !MOBILE && !XAMMAC_4_5
		[MonoTODO]
		public override IChannelListener<TChannel>
			BuildChannelListener<TChannel> (
			BindingContext context)
		{
			throw new NotImplementedException ();
		}
#endif

		[MonoTODO]
		public override bool CanBuildChannelFactory<TChannel> (
			BindingContext context)
		{
			throw new NotImplementedException ();
		}

#if !MOBILE && !XAMMAC_4_5
		[MonoTODO]
		public override bool CanBuildChannelListener<TChannel> (
			BindingContext context)
		{
			throw new NotImplementedException ();
		}
#endif

		public override BindingElement Clone ()
		{
			return new SslStreamSecurityBindingElement (this);
		}

		[MonoTODO]
		public override T GetProperty<T> (BindingContext context)
		{
			throw new NotImplementedException ();
		}

#if !MOBILE && !XAMMAC_4_5
		#region explicit interface implementations
		[MonoTODO]
		void IPolicyExportExtension.ExportPolicy (
			MetadataExporter exporter,
			PolicyConversionContext context)
		{
			var token = GetTransportTokenAssertion ();
			var transportBinding = TransportBindingElement.CreateTransportBinding (token);
			context.GetBindingAssertions ().Add (transportBinding);
		}
		#endregion
#endif
	}
}
