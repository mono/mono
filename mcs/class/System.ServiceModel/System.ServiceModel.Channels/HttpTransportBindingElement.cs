//
// HttpTransportBindingElement.cs
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
#if !MOONLIGHT
using System.ServiceModel.Channels.Http;
#endif
using System.ServiceModel.Description;

namespace System.ServiceModel.Channels
{
	public class HttpTransportBindingElement : TransportBindingElement,
		IPolicyExportExtension, IWsdlExportExtension
	{
		bool allow_cookies, bypass_proxy_on_local,
			unsafe_ntlm_auth;
		bool use_default_proxy = true, keep_alive_enabled = true;
		int max_buffer_size = 0x10000;
		HostNameComparisonMode host_cmp_mode;
		Uri proxy_address;
		string realm = String.Empty;
		TransferMode transfer_mode;
		IDefaultCommunicationTimeouts timeouts;
#if !MOONLIGHT
		AuthenticationSchemes auth_scheme =
			AuthenticationSchemes.Anonymous;
		AuthenticationSchemes proxy_auth_scheme =
			AuthenticationSchemes.Anonymous;
#endif
		// If you add fields, do not forget them in copy constructor.

		public HttpTransportBindingElement ()
		{
		}

		protected HttpTransportBindingElement (
			HttpTransportBindingElement other)
			: base (other)
		{
			allow_cookies = other.allow_cookies;
			bypass_proxy_on_local = other.bypass_proxy_on_local;
			unsafe_ntlm_auth = other.unsafe_ntlm_auth;
			use_default_proxy = other.use_default_proxy;
			keep_alive_enabled = other.keep_alive_enabled;
			max_buffer_size = other.max_buffer_size;
			host_cmp_mode = other.host_cmp_mode;
			proxy_address = other.proxy_address;
			realm = other.realm;
			transfer_mode = other.transfer_mode;
			// FIXME: it does not look safe
			timeouts = other.timeouts;
#if !MOONLIGHT
			auth_scheme = other.auth_scheme;
			proxy_auth_scheme = other.proxy_auth_scheme;
#endif
		}

#if !MOONLIGHT
		public AuthenticationSchemes AuthenticationScheme {
			get { return auth_scheme; }
			set { auth_scheme = value; }
		}

		public AuthenticationSchemes ProxyAuthenticationScheme {
			get { return proxy_auth_scheme; }
			set { proxy_auth_scheme = value; }
		}
#endif

		public bool AllowCookies {
			get { return allow_cookies; }
			set { allow_cookies = value; }
		}

		public bool BypassProxyOnLocal {
			get { return bypass_proxy_on_local; }
			set { bypass_proxy_on_local = value; }
		}

		public HostNameComparisonMode HostNameComparisonMode {
			get { return host_cmp_mode; }
			set { host_cmp_mode = value; }
		}

		public bool KeepAliveEnabled {
			get { return keep_alive_enabled; }
			set { keep_alive_enabled = value; }
		}

		public int MaxBufferSize {
			get { return max_buffer_size; }
			set { max_buffer_size = value; }
		}

		public Uri ProxyAddress {
			get { return proxy_address; }
			set { proxy_address = value; }
		}

		public string Realm {
			get { return realm; }
			set { realm = value; }
		}

		public override string Scheme {
			get { return Uri.UriSchemeHttp; }
		}

		public TransferMode TransferMode {
			get { return transfer_mode; }
			set { transfer_mode = value; }
		}

		public bool UnsafeConnectionNtlmAuthentication {
			get { return unsafe_ntlm_auth; }
			set { unsafe_ntlm_auth = value; }
		}

		public bool UseDefaultWebProxy {
			get { return use_default_proxy; }
			set { use_default_proxy = value; }
		}

		public override bool CanBuildChannelFactory<TChannel> (
			BindingContext context)
		{
			return typeof (TChannel) == typeof (IRequestChannel);
		}

#if !NET_2_1
		public override bool CanBuildChannelListener<TChannel> (
			BindingContext context)
		{
			return typeof (TChannel) == typeof (IReplyChannel);
		}
#endif

		public override IChannelFactory<TChannel> BuildChannelFactory<TChannel> (
			BindingContext context)
		{
			// remaining contexts are ignored ... e.g. such binding
			// element that always causes an error is ignored.
			return new HttpChannelFactory<TChannel> (this, context);
		}

#if !NET_2_1
		internal static object ListenerBuildLock = new object ();

		public override IChannelListener<TChannel> BuildChannelListener<TChannel> (
			BindingContext context)
		{
			// remaining contexts are ignored ... e.g. such binding
			// element that always causes an error is ignored.
			if (ServiceHostingEnvironment.InAspNet)
				return new AspNetChannelListener<TChannel> (this, context);
			else
				return new HttpStandaloneChannelListener<TChannel> (this, context);
//				return new HttpSimpleChannelListener<TChannel> (this, context);
		}
#endif

		public override BindingElement Clone ()
		{
			return new HttpTransportBindingElement (this);
		}

		public override T GetProperty<T> (BindingContext context)
		{
			// http://blogs.msdn.com/drnick/archive/2007/04/10/interfaces-for-getproperty-part-1.aspx
#if !NET_2_1
			if (typeof (T) == typeof (ISecurityCapabilities))
				return (T) (object) new HttpBindingProperties (this);
			if (typeof (T) == typeof (IBindingDeliveryCapabilities))
				return (T) (object) new HttpBindingProperties (this);
#endif
			if (typeof (T) == typeof (TransferMode))
				return (T) (object) TransferMode;
			return base.GetProperty<T> (context);
		}

#if !NET_2_1
		[MonoTODO]
		void IPolicyExportExtension.ExportPolicy (
			MetadataExporter exporter,
			PolicyConversionContext context)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IWsdlExportExtension.ExportContract (WsdlExporter exporter,
			WsdlContractConversionContext context)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IWsdlExportExtension.ExportEndpoint (WsdlExporter exporter,
			WsdlEndpointConversionContext context)
		{
			throw new NotImplementedException ();
		}
#endif
	}

#if !NET_2_1
	class HttpBindingProperties : ISecurityCapabilities, IBindingDeliveryCapabilities
	{
		HttpTransportBindingElement source;

		public HttpBindingProperties (HttpTransportBindingElement source)
		{
			this.source = source;
		}

		public bool AssuresOrderedDelivery {
			get { return false; }
		}

		public bool QueuedDelivery {
			get { return false; }
		}

		public virtual ProtectionLevel SupportedRequestProtectionLevel {
			get { return ProtectionLevel.None; }
		}

		public virtual ProtectionLevel SupportedResponseProtectionLevel {
			get { return ProtectionLevel.None; }
		}

		public virtual bool SupportsClientAuthentication {
			get { return source.AuthenticationScheme != AuthenticationSchemes.Anonymous; }
		}

		public virtual bool SupportsServerAuthentication {
			get {
				switch (source.AuthenticationScheme) {
				case AuthenticationSchemes.Negotiate:
					return true;
				default:
					return false;
				}
			}
		}

		public virtual bool SupportsClientWindowsIdentity {
			get {
				switch (source.AuthenticationScheme) {
				case AuthenticationSchemes.Basic:
				case AuthenticationSchemes.Digest: // hmm... why? but they return true on .NET
				case AuthenticationSchemes.Negotiate:
				case AuthenticationSchemes.Ntlm:
					return true;
				default:
					return false;
				}
			}
		}
	}
#endif
}
