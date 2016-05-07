//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System.ComponentModel;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;

    public sealed class BasicHttpsSecurity
    {
        internal const BasicHttpsSecurityMode DefaultMode = BasicHttpsSecurityMode.Transport;
        BasicHttpSecurity basicHttpSecurity;

        public BasicHttpsSecurity()
            : this(DefaultMode, new HttpTransportSecurity(), new BasicHttpMessageSecurity())
        {
        }

        BasicHttpsSecurity(BasicHttpsSecurityMode mode, HttpTransportSecurity transportSecurity, BasicHttpMessageSecurity messageSecurity)
        {
            if (!BasicHttpsSecurityModeHelper.IsDefined(mode))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("mode"));
            }
            HttpTransportSecurity httpTransportSecurity = transportSecurity == null ? new HttpTransportSecurity() : transportSecurity;
            BasicHttpMessageSecurity httpMessageSecurity = messageSecurity == null ? new BasicHttpMessageSecurity() : messageSecurity;
            BasicHttpSecurityMode basicHttpSecurityMode = BasicHttpsSecurityModeHelper.ToBasicHttpSecurityMode(mode);
            this.basicHttpSecurity = new BasicHttpSecurity()
            {
                Mode = basicHttpSecurityMode,
                Transport = httpTransportSecurity,
                Message = httpMessageSecurity
            };
        }

        public BasicHttpsSecurityMode Mode
        {
            get 
            { 
                return BasicHttpsSecurityModeHelper.ToBasicHttpsSecurityMode(this.basicHttpSecurity.Mode); 
            }

            set
            {
                if (!BasicHttpsSecurityModeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                
                this.basicHttpSecurity.Mode = BasicHttpsSecurityModeHelper.ToBasicHttpSecurityMode(value);
             }
        }

        public HttpTransportSecurity Transport
        {
            get 
            { 
                return this.basicHttpSecurity.Transport; 
            }

            set
            {
                this.basicHttpSecurity.Transport = value;
            }
        }

        public BasicHttpMessageSecurity Message
        {
            get 
            { 
                return this.basicHttpSecurity.Message; 
            }

            set
            {
                this.basicHttpSecurity.Message = value;
            }
        }

        internal BasicHttpSecurity BasicHttpSecurity
        {
            get
            {
                return this.basicHttpSecurity;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeMessage()
        {
            return this.basicHttpSecurity.ShouldSerializeMessage();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeTransport()
        {
            return this.basicHttpSecurity.ShouldSerializeTransport();
        }

        internal static BasicHttpSecurity ToBasicHttpSecurity(BasicHttpsSecurity basicHttpsSecurity)
        {
            Fx.Assert(basicHttpsSecurity != null, "Cannot pass in a null value for basicHttpsSecurity");
            
            BasicHttpSecurity basicHttpSecurity = new BasicHttpSecurity()
            {
                Message = basicHttpsSecurity.Message,
                Transport = basicHttpsSecurity.Transport,
                Mode = BasicHttpsSecurityModeHelper.ToBasicHttpSecurityMode(basicHttpsSecurity.Mode)
            };
            
            return basicHttpSecurity;
        }

        internal static BasicHttpsSecurity ToBasicHttpsSecurity(BasicHttpSecurity basicHttpSecurity)
        {
            Fx.Assert(basicHttpSecurity != null, "basicHttpSecurity cannot be null");
            
            BasicHttpsSecurity basicHttpsSecurity = new BasicHttpsSecurity()
            {
                Message = basicHttpSecurity.Message,
                Transport = basicHttpSecurity.Transport,
                Mode = BasicHttpsSecurityModeHelper.ToBasicHttpsSecurityMode(basicHttpSecurity.Mode)
            };
           
            return basicHttpsSecurity;
        }

        internal static void EnableTransportSecurity(HttpsTransportBindingElement https, HttpTransportSecurity transportSecurity)
        {
            BasicHttpSecurity.EnableTransportSecurity(https, transportSecurity);
        }

        internal static bool IsEnabledTransportAuthentication(HttpTransportBindingElement http, HttpTransportSecurity transportSecurity)
        {
            return BasicHttpSecurity.IsEnabledTransportAuthentication(http, transportSecurity);
        }

        internal void EnableTransportSecurity(HttpsTransportBindingElement https)
        {
            this.basicHttpSecurity.EnableTransportSecurity(https);
        }

        internal void EnableTransportAuthentication(HttpTransportBindingElement http)
        {
            this.basicHttpSecurity.EnableTransportAuthentication(http);
        }

        internal void DisableTransportAuthentication(HttpTransportBindingElement http)
        {
            this.basicHttpSecurity.DisableTransportAuthentication(http);
        }

        internal SecurityBindingElement CreateMessageSecurity()
        {
            return this.basicHttpSecurity.CreateMessageSecurity();
        }

        internal bool InternalShouldSerialize()
        {
            // Default Security mode here is different from that of BasicHttpBinding. Therefore, we do not call into basicHttpSecurity here.
            return this.Mode != BasicHttpsSecurity.DefaultMode
                || this.ShouldSerializeMessage()
                || this.ShouldSerializeTransport();
        }
    }
}
