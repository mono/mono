// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.Text;
    using System.Xml;
        
    public abstract class HttpBindingBase : Binding, IBindingRuntimePreferences
    {
        // private BindingElements
        HttpTransportBindingElement httpTransport;
        HttpsTransportBindingElement httpsTransport;
        TextMessageEncodingBindingElement textEncoding;
        MtomMessageEncodingBindingElement mtomEncoding;

        internal HttpBindingBase()
        {
            this.httpTransport = new HttpTransportBindingElement();
            this.httpsTransport = new HttpsTransportBindingElement();

            this.textEncoding = new TextMessageEncodingBindingElement();
            this.textEncoding.MessageVersion = MessageVersion.Soap11;
            this.mtomEncoding = new MtomMessageEncodingBindingElement();
            this.mtomEncoding.MessageVersion = MessageVersion.Soap11;

            this.httpsTransport.WebSocketSettings = this.httpTransport.WebSocketSettings;
        }

        [DefaultValue(HttpTransportDefaults.AllowCookies)]
        public bool AllowCookies
        {
            get 
            { 
                return this.httpTransport.AllowCookies; 
            }

            set
            {
                this.httpTransport.AllowCookies = value;
                this.httpsTransport.AllowCookies = value;
            }
        }

        [DefaultValue(HttpTransportDefaults.BypassProxyOnLocal)]
        public bool BypassProxyOnLocal
        {
            get 
            { 
                return this.httpTransport.BypassProxyOnLocal; 
            }

            set
            {
                this.httpTransport.BypassProxyOnLocal = value;
                this.httpsTransport.BypassProxyOnLocal = value;
            }
        }

        [DefaultValue(HttpTransportDefaults.HostNameComparisonMode)]
        public HostNameComparisonMode HostNameComparisonMode
        {
            get 
            { 
                return this.httpTransport.HostNameComparisonMode; 
            }

            set
            {
                this.httpTransport.HostNameComparisonMode = value;
                this.httpsTransport.HostNameComparisonMode = value;
            }
        }

        [DefaultValue(TransportDefaults.MaxBufferSize)]
        public int MaxBufferSize
        {
            get 
            { 
                return this.httpTransport.MaxBufferSize; 
            }

            set
            {
                this.httpTransport.MaxBufferSize = value;
                this.httpsTransport.MaxBufferSize = value;
                this.mtomEncoding.MaxBufferSize = value;
            }
        }

        [DefaultValue(TransportDefaults.MaxBufferPoolSize)]
        public long MaxBufferPoolSize
        {
            get 
            { 
                return this.httpTransport.MaxBufferPoolSize; 
            }

            set
            {
                this.httpTransport.MaxBufferPoolSize = value;
                this.httpsTransport.MaxBufferPoolSize = value;
            }
        }

        [DefaultValue(TransportDefaults.MaxReceivedMessageSize)]
        public long MaxReceivedMessageSize
        {
            get 
            { 
                return this.httpTransport.MaxReceivedMessageSize; 
            }

            set
            {
                this.httpTransport.MaxReceivedMessageSize = value;
                this.httpsTransport.MaxReceivedMessageSize = value;
            }
        }

        [DefaultValue(HttpTransportDefaults.ProxyAddress)]
        [TypeConverter(typeof(UriTypeConverter))]
        public Uri ProxyAddress
        {
            get 
            { 
                return this.httpTransport.ProxyAddress; 
            }

            set
            {
                this.httpTransport.ProxyAddress = value;
                this.httpsTransport.ProxyAddress = value;
            }
        }

        public XmlDictionaryReaderQuotas ReaderQuotas
        {
            get 
            { 
                return this.textEncoding.ReaderQuotas; 
            }

            set
            {
                if (value == null)
                {
                    throw FxTrace.Exception.ArgumentNull("value");
                }

                value.CopyTo(this.textEncoding.ReaderQuotas);
                value.CopyTo(this.mtomEncoding.ReaderQuotas);

                this.SetReaderQuotas(value);
            }
        }

        public override string Scheme
        {
            get
            {
                return this.GetTransport().Scheme;
            }
        }

        public EnvelopeVersion EnvelopeVersion
        {
            get { return this.GetEnvelopeVersion(); }
        }

        [TypeConverter(typeof(EncodingConverter))]
        public Encoding TextEncoding
        {
            get 
            { 
                return this.textEncoding.WriteEncoding; 
            }

            set
            {
                this.textEncoding.WriteEncoding = value;
                this.mtomEncoding.WriteEncoding = value;
            }
        }

        [DefaultValue(HttpTransportDefaults.TransferMode)]
        public TransferMode TransferMode
        {
            get 
            { 
                return this.httpTransport.TransferMode; 
            }

            set
            {
                this.httpTransport.TransferMode = value;
                this.httpsTransport.TransferMode = value;
            }
        }

        [DefaultValue(HttpTransportDefaults.UseDefaultWebProxy)]
        public bool UseDefaultWebProxy
        {
            get 
            { 
                return this.httpTransport.UseDefaultWebProxy; 
            }

            set
            {
                this.httpTransport.UseDefaultWebProxy = value;
                this.httpsTransport.UseDefaultWebProxy = value;
            }
        }

        bool IBindingRuntimePreferences.ReceiveSynchronously
        {
            get { return false; }
        }

        internal TextMessageEncodingBindingElement TextMessageEncodingBindingElement
        {
            get 
            {
                return this.textEncoding;
            }
        }

        internal MtomMessageEncodingBindingElement MtomMessageEncodingBindingElement
        {
            get
            {
                return this.mtomEncoding;
            }
        }

        internal abstract BasicHttpSecurity BasicHttpSecurity
        {
            get;
        }

        internal WebSocketTransportSettings InternalWebSocketSettings
        {
            get 
            { 
                return this.httpTransport.WebSocketSettings; 
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeReaderQuotas()
        {
            return !EncoderDefaults.IsDefaultReaderQuotas(this.ReaderQuotas);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeTextEncoding()
        {
            return !this.TextEncoding.Equals(BasicHttpBindingDefaults.TextEncoding);
        }

        internal static bool GetSecurityModeFromTransport(HttpTransportBindingElement http, HttpTransportSecurity transportSecurity, out UnifiedSecurityMode mode)
        {
            mode = UnifiedSecurityMode.None;
            if (http == null)
            {
                return false;
            }

            Fx.Assert(http.AuthenticationScheme.IsSingleton(), "authenticationScheme used in an Http(s)ChannelFactory must be a singleton value.");

            if (http is HttpsTransportBindingElement)
            {
                mode = UnifiedSecurityMode.Transport | UnifiedSecurityMode.TransportWithMessageCredential;
                BasicHttpSecurity.EnableTransportSecurity((HttpsTransportBindingElement)http, transportSecurity);
            }
            else if (HttpTransportSecurity.IsDisabledTransportAuthentication(http))
            {
                mode = UnifiedSecurityMode.Message | UnifiedSecurityMode.None;
            }
            else if (!BasicHttpSecurity.IsEnabledTransportAuthentication(http, transportSecurity))
            {
                return false;
            }
            else
            {
                mode = UnifiedSecurityMode.TransportCredentialOnly;
            }

            return true;
        }

        internal static bool TryCreateSecurity(SecurityBindingElement securityElement, UnifiedSecurityMode mode, HttpTransportSecurity transportSecurity, out BasicHttpSecurity security)
        {
            return BasicHttpSecurity.TryCreate(securityElement, mode, transportSecurity, out security);
        }

        internal TransportBindingElement GetTransport()
        {
            Fx.Assert(this.BasicHttpSecurity != null, "this.BasicHttpSecurity should not return null from a derived class."); 

            BasicHttpSecurity basicHttpSecurity = this.BasicHttpSecurity;
            if (basicHttpSecurity.Mode == BasicHttpSecurityMode.Transport || basicHttpSecurity.Mode == BasicHttpSecurityMode.TransportWithMessageCredential)
            {
                basicHttpSecurity.EnableTransportSecurity(this.httpsTransport);
                return this.httpsTransport;
            }
            else if (basicHttpSecurity.Mode == BasicHttpSecurityMode.TransportCredentialOnly)
            {
                basicHttpSecurity.EnableTransportAuthentication(this.httpTransport);
                return this.httpTransport;
            }
            else
            {
                // ensure that there is no transport security
                basicHttpSecurity.DisableTransportAuthentication(this.httpTransport);
                return this.httpTransport;
            }
        }

        internal abstract EnvelopeVersion GetEnvelopeVersion();

        internal virtual void SetReaderQuotas(XmlDictionaryReaderQuotas readerQuotas) 
        { 
        }

        internal virtual void InitializeFrom(HttpTransportBindingElement transport, MessageEncodingBindingElement encoding)
        {
            this.BypassProxyOnLocal = transport.BypassProxyOnLocal;
            this.HostNameComparisonMode = transport.HostNameComparisonMode;
            this.MaxBufferPoolSize = transport.MaxBufferPoolSize;
            this.MaxBufferSize = transport.MaxBufferSize;
            this.MaxReceivedMessageSize = transport.MaxReceivedMessageSize;
            this.ProxyAddress = transport.ProxyAddress;
            this.TransferMode = transport.TransferMode;
            this.UseDefaultWebProxy = transport.UseDefaultWebProxy;
            this.httpTransport.WebSocketSettings = transport.WebSocketSettings;
            this.httpsTransport.WebSocketSettings = transport.WebSocketSettings;

            if (encoding is TextMessageEncodingBindingElement)
            {
                TextMessageEncodingBindingElement text = (TextMessageEncodingBindingElement)encoding;
                this.TextEncoding = text.WriteEncoding;
                this.ReaderQuotas = text.ReaderQuotas;
            }
            else if (encoding is MtomMessageEncodingBindingElement)
            {
                MtomMessageEncodingBindingElement mtom = (MtomMessageEncodingBindingElement)encoding;
                this.TextEncoding = mtom.WriteEncoding;
                this.ReaderQuotas = mtom.ReaderQuotas;
            }

            this.BasicHttpSecurity.Transport.ExtendedProtectionPolicy = transport.ExtendedProtectionPolicy;
        }

        // In the Win8 profile, some settings for the binding security are not supported.
        internal virtual void CheckSettings()
        {
            if (!UnsafeNativeMethods.IsTailoredApplication.Value)
            {
                return;
            }

            BasicHttpSecurity security = this.BasicHttpSecurity;
            if (security == null)
            {
                return;
            }

            BasicHttpSecurityMode mode = security.Mode;
            if (mode == BasicHttpSecurityMode.None)
            {
                return;
            }
            else if (mode == BasicHttpSecurityMode.Message)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.UnsupportedSecuritySetting, "Mode", mode)));
            }

            // Message.ClientCredentialType = Certificate is not supported.
            if (mode == BasicHttpSecurityMode.TransportWithMessageCredential)
            {
                BasicHttpMessageSecurity message = security.Message;
                if ((message != null) && (message.ClientCredentialType == BasicHttpMessageCredentialType.Certificate))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.UnsupportedSecuritySetting, "Message.ClientCredentialType", message.ClientCredentialType)));
                }
            }

            // Transport.ClientCredentialType = Certificate or InheritedFromHost are not supported.
            Fx.Assert(
                (mode == BasicHttpSecurityMode.Transport) || (mode == BasicHttpSecurityMode.TransportCredentialOnly) || (mode == BasicHttpSecurityMode.TransportWithMessageCredential), 
                "Unexpected BasicHttpSecurityMode value: " + mode);
            HttpTransportSecurity transport = security.Transport;
            if ((transport != null) && ((transport.ClientCredentialType == HttpClientCredentialType.Certificate) || (transport.ClientCredentialType == HttpClientCredentialType.InheritedFromHost)))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.UnsupportedSecuritySetting, "Transport.ClientCredentialType", transport.ClientCredentialType)));
            }
        }
    }
}
