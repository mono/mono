//------------------------------------------------------------------------------
// <copyright file="TransportContext.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Net.Security;
using System.Security.Authentication.ExtendedProtection;

namespace System.Net
{
    public abstract class TransportContext
    {
        public abstract ChannelBinding GetChannelBinding(ChannelBindingKind kind);
        public virtual IEnumerable<TokenBinding> GetTlsTokenBindings()
        {
            throw new NotSupportedException();
        }
    }

#if MONO_FEATURE_WEB_STACK
    internal class ConnectStreamContext : TransportContext
    {
        internal ConnectStreamContext(ConnectStream connectStream)
        {
            GlobalLog.Assert(connectStream != null, "ConnectStreamContext..ctor(): Not expecting a null connectStream!");
            this.connectStream = connectStream;
        }

        public override ChannelBinding GetChannelBinding(ChannelBindingKind kind)
        {
            return connectStream.GetChannelBinding(kind);
        }

        private ConnectStream connectStream;
    }
#endif

#if MONO_FEATURE_NEW_TLS
    internal class SslStreamContext : TransportContext
    {
        internal SslStreamContext(SslStream sslStream)
        {
            GlobalLog.Assert(sslStream != null, "SslStreamContext..ctor(): Not expecting a null sslStream!");
            this.sslStream = sslStream;
        }

        public override ChannelBinding GetChannelBinding(ChannelBindingKind kind)
        {
            return sslStream.GetChannelBinding(kind);
        }

        private SslStream sslStream;
    }
#endif

#if MONO_FEATURE_WEB_STACK
    internal class HttpListenerRequestContext : TransportContext
    {
        internal HttpListenerRequestContext(HttpListenerRequest request)
        {
            GlobalLog.Assert(request != null, "HttpListenerRequestContext..ctor(): Not expecting a null request!");
            this.request = request;
        }

        public override ChannelBinding GetChannelBinding(ChannelBindingKind kind)
        {
            if (kind != ChannelBindingKind.Endpoint)
            {
                throw new NotSupportedException(SR.GetString(
                    SR.net_listener_invalid_cbt_type, kind.ToString()));
            }
            return request.GetChannelBinding();
        }

        public override IEnumerable<TokenBinding> GetTlsTokenBindings()
        {
            return request.GetTlsTokenBindings();
        }

        private HttpListenerRequest request;
    }
#endif

    // Holds a cached Endpoint binding to be reused by HttpWebRequest preauthentication
    internal class CachedTransportContext : TransportContext
    {
        internal CachedTransportContext(ChannelBinding binding)
        {
            this.binding = binding;
        }

        public override ChannelBinding GetChannelBinding(ChannelBindingKind kind)
        {
            if (kind != ChannelBindingKind.Endpoint)
                return null;

            return binding;
        }

        private ChannelBinding binding;
    }
}
