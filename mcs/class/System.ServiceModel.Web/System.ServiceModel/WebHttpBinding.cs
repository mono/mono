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
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;

namespace System.ServiceModel
{
	public class WebHttpBinding
#if NET_2_1
        : Binding
#else
        : Binding, IBindingRuntimePreferences
#endif
	{
		public WebHttpBinding ()
			: this (WebHttpSecurityMode.None)
		{
		}

		public WebHttpBinding (WebHttpSecurityMode mode)
		{
			security.Mode = mode;
			// MSDN says that this security mode can be set only
			// at .ctor(), so there is no problem on depending on
			// this value here.
			t = mode == WebHttpSecurityMode.Transport ? new HttpsTransportBindingElement () : new HttpTransportBindingElement ();
			t.ManualAddressing = true;
		}

		[MonoTODO]
		public WebHttpBinding (string configurationName)
		{
			throw new NotImplementedException ();
		}

		WebHttpSecurity security = new WebHttpSecurity ();
		HttpTransportBindingElement t;
		// This can be changed only using <synchronousReceive> configuration element.
		bool receive_synchronously;
		WebMessageEncodingBindingElement msgenc = new WebMessageEncodingBindingElement ();

		public EnvelopeVersion EnvelopeVersion {
			get { return EnvelopeVersion.None; }
		}

#if !NET_2_1
#if NET_4_0
		[DefaultValue (false)]
#endif
		public bool AllowCookies {
			get { return t.AllowCookies; }
			set { t.AllowCookies = value; }
		}

#if NET_4_0
		[DefaultValue (false)]
#endif
		public bool BypassProxyOnLocal {
			get { return t.BypassProxyOnLocal; }
			set { t.BypassProxyOnLocal = value; }
		}

#if NET_4_0
		[MonoTODO]
		public bool CrossDomainScriptAccessEnabled { get; set; }

		public WebContentTypeMapper ContentTypeMapper {
			get { return msgenc.ContentTypeMapper; }
			set { msgenc.ContentTypeMapper = value; }
		}
#endif

#if NET_4_0
		[DefaultValue (HostNameComparisonMode.StrongWildcard)]
#endif
		public HostNameComparisonMode HostNameComparisonMode {
			get { return t.HostNameComparisonMode; }
			set { t.HostNameComparisonMode = value; }
		}

#if NET_4_0
		[DefaultValue (0x10000)]
#endif
		public long MaxBufferPoolSize {
			get { return t.MaxBufferPoolSize; }
			set { t.MaxBufferPoolSize = value; }
		}

#if NET_4_0
		[DefaultValue (TransferMode.Buffered)]
#endif
		public TransferMode TransferMode {
			get { return t.TransferMode; }
			set { t.TransferMode = value; }
		}

#if NET_4_0
		[DefaultValue (true)]
#endif
		public bool UseDefaultWebProxy {
			get { return t.UseDefaultWebProxy; }
			set { t.UseDefaultWebProxy = value; }
		}

#if NET_4_0
		[DefaultValue (null)]
#endif
		public Uri ProxyAddress {
			get { return t.ProxyAddress; }
			set { t.ProxyAddress = value; }
		}
#endif

#if NET_4_0
		[DefaultValue (0x80000)]
#endif
		public int MaxBufferSize {
			get { return t.MaxBufferSize; }
			set { t.MaxBufferSize = value; }
		}

#if NET_4_0
		[DefaultValue (0x10000)]
#endif
		public long MaxReceivedMessageSize {
			get { return t.MaxReceivedMessageSize; }
			set { t.MaxReceivedMessageSize = value; }
		}

#if !NET_2_1
		public XmlDictionaryReaderQuotas ReaderQuotas {
			get { return msgenc.ReaderQuotas; }
			set { msgenc.ReaderQuotas = value; }
		}
#endif

		public override string Scheme {
			get { return Security.Mode != WebHttpSecurityMode.None ? Uri.UriSchemeHttps : Uri.UriSchemeHttp; }
		}

		public WebHttpSecurity Security {
			get { return security; }
#if NET_4_0
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				security = value;
			}
#endif
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

#if !NET_2_1
		bool IBindingRuntimePreferences.ReceiveSynchronously {
			get { return receive_synchronously; }
		}
#endif

#if NET_4_0
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
#endif
	}
}
