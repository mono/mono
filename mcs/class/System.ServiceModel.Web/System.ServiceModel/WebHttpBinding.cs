//
// WebHttpBinding.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.ComponentModel;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;
#if !MOBILE
using System.Configuration;
using System.ServiceModel.Configuration;
#endif

namespace System.ServiceModel
{
	public class WebHttpBinding : Binding, IBindingRuntimePreferences
	{
		public WebHttpBinding ()
			: this (String.Empty)
		{
		}

		public WebHttpBinding (WebHttpSecurityMode securityMode)
		{
			Initialize (securityMode);
		}

		public WebHttpBinding (string configurationName)
		{
#if !MOBILE && !XAMMAC_4_5
			BindingsSection bindingsSection = ConfigUtil.BindingsSection;
			WebHttpBindingElement el = (WebHttpBindingElement) bindingsSection ["webHttpBinding"].ConfiguredBindings.FirstOrDefault (c => c.Name == configurationName);
			if (el != null) {
				Initialize (el.Security.Mode); // to initialize Transport correctly.
				el.ApplyConfiguration (this);
			}
			else if (!String.IsNullOrEmpty (configurationName))
				throw new ConfigurationException (String.Format ("Specified webHttpBinding configuration '{0}' was not found", configurationName));
			else
				Initialize (WebHttpSecurityMode.None);
#else
			Initialize (WebHttpSecurityMode.None);
#endif
		}

		void Initialize (WebHttpSecurityMode mode)
		{
			security.Mode = mode;
			// MSDN says that this security mode can be set only
			// at .ctor(), so there is no problem on depending on
			// this value here.
			t = mode == WebHttpSecurityMode.Transport ? new HttpsTransportBindingElement () : new HttpTransportBindingElement ();
			t.ManualAddressing = true;
		}

		WebHttpSecurity security = new WebHttpSecurity ();
		HttpTransportBindingElement t;
		// This can be changed only using <synchronousReceive> configuration element.
		WebMessageEncodingBindingElement msgenc = new WebMessageEncodingBindingElement ();

		public EnvelopeVersion EnvelopeVersion {
			get { return EnvelopeVersion.None; }
		}

#if !MOBILE && !XAMMAC_4_5
		[DefaultValue (false)]
		public bool AllowCookies {
			get { return t.AllowCookies; }
			set { t.AllowCookies = value; }
		}

		[DefaultValue (false)]
		public bool BypassProxyOnLocal {
			get { return t.BypassProxyOnLocal; }
			set { t.BypassProxyOnLocal = value; }
		}

		[MonoTODO]
		public bool CrossDomainScriptAccessEnabled { get; set; }

		public WebContentTypeMapper ContentTypeMapper {
			get { return msgenc.ContentTypeMapper; }
			set { msgenc.ContentTypeMapper = value; }
		}

		[DefaultValue (HostNameComparisonMode.StrongWildcard)]
		public HostNameComparisonMode HostNameComparisonMode {
			get { return t.HostNameComparisonMode; }
			set { t.HostNameComparisonMode = value; }
		}

		[DefaultValue (0x10000)]
		public long MaxBufferPoolSize {
			get { return t.MaxBufferPoolSize; }
			set { t.MaxBufferPoolSize = value; }
		}

		[DefaultValue (TransferMode.Buffered)]
		public TransferMode TransferMode {
			get { return t.TransferMode; }
			set { t.TransferMode = value; }
		}

		[DefaultValue (true)]
		public bool UseDefaultWebProxy {
			get { return t.UseDefaultWebProxy; }
			set { t.UseDefaultWebProxy = value; }
		}

		[DefaultValue (null)]
		public Uri ProxyAddress {
			get { return t.ProxyAddress; }
			set { t.ProxyAddress = value; }
		}
#endif

		[DefaultValue (0x80000)]
		public int MaxBufferSize {
			get { return t.MaxBufferSize; }
			set { t.MaxBufferSize = value; }
		}

		[DefaultValue (0x10000)]
		public long MaxReceivedMessageSize {
			get { return t.MaxReceivedMessageSize; }
			set { t.MaxReceivedMessageSize = value; }
		}

		public XmlDictionaryReaderQuotas ReaderQuotas {
			get { return msgenc.ReaderQuotas; }
			set { msgenc.ReaderQuotas = value; }
		}

		public override string Scheme {
			get { return Security.Mode == WebHttpSecurityMode.Transport ? Uri.UriSchemeHttps : Uri.UriSchemeHttp; }
		}

		public WebHttpSecurity Security {
			get { return security; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				security = value;
			}
		}

		public Encoding WriteEncoding {
			get { return msgenc.WriteEncoding; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				msgenc.WriteEncoding = value; 
			}
		}

		public override BindingElementCollection CreateBindingElements ()
		{
			return new BindingElementCollection (new BindingElement [] { msgenc, t.Clone () });
		}

		bool IBindingRuntimePreferences.ReceiveSynchronously {
			get { return false; }
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public bool ShouldSerializeReaderQuotas ()
		{
			return false;
		}
		
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public bool ShouldSerializeSecurity ()
		{
			return false;
		}
		
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public bool ShouldSerializeWriteEncoding ()
		{
			return false;
		}
	}
}
