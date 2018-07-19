//------------------------------------------------------------------------------
// <copyright file="_ProxyChain.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net
{
    using System.Collections;
    using System.Collections.Generic;

    internal interface IAutoWebProxy : IWebProxy
    {
        ProxyChain GetProxies(Uri destination);
    }

    internal abstract class ProxyChain : IEnumerable<Uri>, IDisposable
    {
        private List<Uri> m_Cache = new List<Uri>();
        private bool m_CacheComplete;
        private ProxyEnumerator m_MainEnumerator;
        private Uri m_Destination;
        private HttpAbortDelegate m_HttpAbortDelegate;

        protected ProxyChain(Uri destination)
        {
            m_Destination = destination;
        }

        public IEnumerator<Uri> GetEnumerator()
        {
            ProxyEnumerator enumerator = new ProxyEnumerator(this);
            if (m_MainEnumerator == null)
            {
                m_MainEnumerator = enumerator;
            }
            return enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual void Dispose() { }

        internal IEnumerator<Uri> Enumerator
        {
            get
            {
                return m_MainEnumerator == null ? GetEnumerator() : m_MainEnumerator;
            }
        }

        internal Uri Destination
        {
            get
            {
                return m_Destination;
            }
        }

        // MoveNext can be time-consuming (download proxy script).  This lets you abort it.
        internal virtual void Abort() { }

        internal bool HttpAbort(HttpWebRequest request, WebException webException)
        {
            Abort();
            return true;
        }

        internal HttpAbortDelegate HttpAbortDelegate
        {
            get
            {
                if (m_HttpAbortDelegate == null)
                {
                    m_HttpAbortDelegate = new HttpAbortDelegate(HttpAbort);
                }
                return m_HttpAbortDelegate;
            }
        }

        protected abstract bool GetNextProxy(out Uri proxy);

        // This implementation prevents DIRECT (null) from being returned more than once.
        private class ProxyEnumerator : IEnumerator<Uri>
        {
            private ProxyChain m_Chain;
            private bool m_Finished;
            private int m_CurrentIndex = -1;
            private bool m_TriedDirect;

            internal ProxyEnumerator(ProxyChain chain)
            {
                m_Chain = chain;
            }

            public Uri Current
            {
                get
                {
                    if (m_Finished || m_CurrentIndex < 0)
                    {
                        throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_EnumOpCantHappen));
                    }

                    GlobalLog.Assert(m_Chain.m_Cache.Count > m_CurrentIndex, "ProxyEnumerator::Current|Not all proxies made it to the cache.");
                    return m_Chain.m_Cache[m_CurrentIndex];
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public bool MoveNext()
            {
                if (m_Finished)
                {
                    return false;
                }

                checked{m_CurrentIndex++;}
                if (m_Chain.m_Cache.Count > m_CurrentIndex)
                {
                    return true;
                }

                if (m_Chain.m_CacheComplete)
                {
                    m_Finished = true;
                    return false;
                }

                lock (m_Chain.m_Cache)
                {
                    if (m_Chain.m_Cache.Count > m_CurrentIndex)
                    {
                        return true;
                    }

                    if (m_Chain.m_CacheComplete)
                    {
                        m_Finished = true;
                        return false;
                    }

                    Uri nextProxy;
                    while (true)
                    {
                        if (!m_Chain.GetNextProxy(out nextProxy))
                        {
                            m_Finished = true;
                            m_Chain.m_CacheComplete = true;
                            return false;
                        }

                        if (nextProxy == null)
                        {
                            if (m_TriedDirect)
                            {
                                continue;
                            }
                            m_TriedDirect = true;
                        }
                        break;
                    }

                    m_Chain.m_Cache.Add(nextProxy);
                    GlobalLog.Assert(m_Chain.m_Cache.Count > m_CurrentIndex, "ProxyEnumerator::MoveNext|Not all proxies made it to the cache.");
                    return true;
                }
            }

            public void Reset()
            {
                m_Finished = false;
                m_CurrentIndex = -1;
            }

            public void Dispose() { }
        }
    }


    // This class implements failover logic for proxy scripts.
    internal class ProxyScriptChain : ProxyChain
    {
        private WebProxy m_Proxy;
        private Uri[] m_ScriptProxies;
        private int m_CurrentIndex;
        private int m_SyncStatus;

        internal ProxyScriptChain(WebProxy proxy, Uri destination) :
            base(destination)
        {
            m_Proxy = proxy;
        }

        protected override bool GetNextProxy(out Uri proxy)
        {
            if (m_CurrentIndex < 0)
            {
                proxy = null;
                return false;
            }

            if (m_CurrentIndex == 0)
            {
                m_ScriptProxies = m_Proxy.GetProxiesAuto(Destination, ref m_SyncStatus);
            }

            if (m_ScriptProxies == null || m_CurrentIndex >= m_ScriptProxies.Length)
            {
                proxy = m_Proxy.GetProxyAutoFailover(Destination);
                m_CurrentIndex = -1;
                return true;
            }

            proxy = m_ScriptProxies[m_CurrentIndex++];
            return true;
        }

        internal override void Abort()
        {
            m_Proxy.AbortGetProxiesAuto(ref m_SyncStatus);
        }
    }

    // This class says to use no proxy.
    internal class DirectProxy : ProxyChain
    {
        private bool m_ProxyRetrieved;

        internal DirectProxy(Uri destination) : base(destination) { }

        protected override bool GetNextProxy(out Uri proxy)
        {
            proxy = null;
            if (m_ProxyRetrieved)
            {
                return false;
            }
            m_ProxyRetrieved = true;
            return true;
        }
    }

    // This class says to use a single fixed proxy.
    internal class StaticProxy : ProxyChain
    {
        private Uri m_Proxy;

        internal StaticProxy(Uri destination, Uri proxy) :
            base(destination)
        {
            if (proxy == null)
            {
                throw new ArgumentNullException("proxy");
            }
            m_Proxy = proxy;
        }

        protected override bool GetNextProxy(out Uri proxy)
        {
            proxy = m_Proxy;
            if (proxy == null)
            {
                return false;
            }
            m_Proxy = null;
            return true;
        }
    }
}
