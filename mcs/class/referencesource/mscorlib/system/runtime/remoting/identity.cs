// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
namespace System.Runtime.Remoting {
    using System.Globalization;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Contexts;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Activation;
    using System.Runtime.Remoting.Lifetime;
    using System.Security.Cryptography;
    using Microsoft.Win32;
    using System.Threading;
    using System;
    //  Identity is the base class for remoting identities. An instance of Identity (or a derived class)
    //  is associated with each instance of a remoted object. Likewise, an instance of Identity is
    //  associated with each instance of a remoting proxy.
    //
    using System.Collections;
    internal class Identity {
        // We use a Guid to create a URI from. Each time a new URI is needed we increment
        // the sequence number and append it to the statically inited Guid.
        // private static readonly Guid IDGuid = Guid.NewGuid();

        internal static String ProcessIDGuid
        {
            get
            {
                return SharedStatics.Remoting_Identity_IDGuid;
            }
        }

        // We need the original and the configured one because we have to compare
        //   both when looking at a uri since something might be marshalled before
        //   the id is set.
        private static String s_originalAppDomainGuid = Guid.NewGuid().ToString().Replace('-', '_');
        private static String s_configuredAppDomainGuid = null;

        internal static String AppDomainUniqueId
        {
            get
            {
                if (s_configuredAppDomainGuid != null)
                    return s_configuredAppDomainGuid;
                else
                    return s_originalAppDomainGuid;
            } // get

        } // AppDomainGuid

        private static String s_originalAppDomainGuidString = "/" + s_originalAppDomainGuid.ToLower(CultureInfo.InvariantCulture) + "/";
        private static String s_configuredAppDomainGuidString = null;

        private static String s_IDGuidString = "/" + s_originalAppDomainGuid.ToLower(CultureInfo.InvariantCulture) + "/";

        // Used to get random numbers 
        private static RNGCryptoServiceProvider s_rng = new RNGCryptoServiceProvider();

        internal static String IDGuidString
        {
            get { return s_IDGuidString; }
        }


        internal static String RemoveAppNameOrAppGuidIfNecessary(String uri)
        {
            // uri is assumed to be in lower-case at this point

            // If the uri starts with either, "/<appname>/" or "/<appdomainguid>/" we
            //   should strip that off.

            // We only need to look further if the uri starts with a "/".
            if ((uri == null) || (uri.Length <= 1) || (uri[0] != '/'))
                return uri;

            // compare to process guid (guid string already has slash at beginnning and end)
            String guidStr;
            if (s_configuredAppDomainGuidString != null)
            {
                guidStr = s_configuredAppDomainGuidString;
                if (uri.Length > guidStr.Length)
                {
                    if (StringStartsWith(uri, guidStr))
                    {
                        // remove "/<appdomainguid>/"
                        return uri.Substring(guidStr.Length);
                    }
                }
            }

            // always need to check original guid as well in case the object with this
            //   uri was marshalled before we changed the app domain id
            guidStr = s_originalAppDomainGuidString;
            if (uri.Length > guidStr.Length)
            {
                if (StringStartsWith(uri, guidStr))
                {
                    // remove "/<appdomainguid>/"
                    return uri.Substring(guidStr.Length);
                }
            }

            // compare to application name (application name will never have slashes)
            String appName = RemotingConfiguration.ApplicationName;
            if (appName != null)
            {
                // add +2 to appName length for surrounding slashes
                if (uri.Length > (appName.Length + 2))
                {
                    if (String.Compare(uri, 1, appName, 0, appName.Length, true, CultureInfo.InvariantCulture) == 0)
                    {
                        // now, make sure there is a slash after "/<appname>" in uri
                        if (uri[appName.Length + 1] == '/')
                        {
                            // remove "/<appname>/"
                            return uri.Substring(appName.Length + 2);
                        }
                    }
                }
            }

            // it didn't start with "/<appname>/" or "/<processguid>/", so just remove the
            //   first slash and return.
            uri = uri.Substring(1);
            return uri;
        } // RemoveAppNameOrAppGuidIfNecessary


        private static bool StringStartsWith(String s1, String prefix)
        {
            // String.StartsWith uses String.Compare instead of String.CompareOrdinal,
            //   so we provide our own implementation of StartsWith.

            if (s1.Length < prefix.Length)
                return false;

            return (String.CompareOrdinal(s1, 0, prefix, 0, prefix.Length) == 0);
        } // StringStartsWith



        // DISCONNECTED_FULL denotes that the object is disconnected
        // from both local & remote (x-appdomain & higher) clients

        // DISCONNECTED_REM denotes that the object is disconnected
        // from remote (x-appdomain & higher) clients ... however
        // x-context proxies continue to work as expected.

        protected const int IDFLG_DISCONNECTED_FULL= 0x00000001;
        protected const int IDFLG_DISCONNECTED_REM = 0x00000002;
        protected const int IDFLG_IN_IDTABLE       = 0x00000004;

        protected const int IDFLG_CONTEXT_BOUND    = 0x00000010;
        protected const int IDFLG_WELLKNOWN        = 0x00000100;
        protected const int IDFLG_SERVER_SINGLECALL= 0x00000200;
        protected const int IDFLG_SERVER_SINGLETON = 0x00000400;

        internal int _flags;

        internal Object _tpOrObject;
        protected String _ObjURI;
        protected String _URL;

        // These have to be "Object" to use Interlocked operations
        internal Object _objRef;
        internal Object _channelSink;

        // Remoting proxy has this field too, we use the latter only for
        // ContextBoundObject identities.
        internal Object _envoyChain;

        // This manages the dynamically registered sinks for the proxy.
        internal DynamicPropertyHolder _dph;

        // Lease for object
        internal Lease _lease;

        internal static String ProcessGuid {get {return ProcessIDGuid;}}

        private static int GetNextSeqNum()
        {
            return SharedStatics.Remoting_Identity_GetNextSeqNum();
        }

        private static Byte[] GetRandomBytes()
        {
            // PERF? In a situation where objects need URIs at a very fast
            // rate, we will end up creating too many of these tiny byte-arrays
            // causing pressure on GC!
            // One option would be to have a buff in the managed thread class
            // and use that to get a chunk of random bytes consuming 
            // 18 bytes at a time. 
            // This would avoid the need to have a lock across threads.
            Byte[] randomBytes = new byte[18];
            s_rng.GetBytes(randomBytes);
            return randomBytes;
        }


        // Constructs a new identity using the given the URI. This is used for
        // creating client side identities.
        //
        //
        internal Identity(String objURI, String URL)
        {
            BCLDebug.Assert(objURI!=null,"objURI should not be null here");
            if (URL != null)
            {
                _flags |= IDFLG_WELLKNOWN;
                _URL = URL;
            }
            SetOrCreateURI(objURI, true /*calling from ID ctor*/);
        }

        // Constructs a new identity. This is used for creating server side
        // identities. The URI for server side identities is lazily generated
        // during the first call to Marshal because if we associate a URI with the
        // object at the time of creation then you cannot call Marshal with a
        // URI of your own choice.
        //
        //
        internal Identity(bool bContextBound)
        {
            if(bContextBound)
                _flags |= IDFLG_CONTEXT_BOUND;
        }

        internal bool IsContextBound {
            get  {
                return (_flags&IDFLG_CONTEXT_BOUND) == IDFLG_CONTEXT_BOUND;
            }
        }

        internal bool IsWellKnown()
        {
            return (_flags&IDFLG_WELLKNOWN) == IDFLG_WELLKNOWN;
        }

        internal void SetInIDTable()
        {
            while(true) {
                int currentFlags = _flags;
                int newFlags = _flags | IDFLG_IN_IDTABLE;
                if(currentFlags == Interlocked.CompareExchange(ref _flags, newFlags, currentFlags))
                    break;
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal void ResetInIDTable(bool bResetURI)
        {
            BCLDebug.Assert(IdentityHolder.TableLock.IsWriterLockHeld, "IDTable should be write-locked");
            while(true) {
                int currentFlags = _flags;
                int newFlags = _flags & (~IDFLG_IN_IDTABLE);
                if(currentFlags == Interlocked.CompareExchange(ref _flags, newFlags, currentFlags))
                    break;
            }
            // bResetURI is true for the external API call to Disconnect, it is
            // false otherwise. Thus when a user Disconnects an object 
            // its URI will get reset but if lifetime service times it out 
            // it will not clear out the URIs
            if (bResetURI)
            {
                ((ObjRef)_objRef).URI = null;
                _ObjURI = null;
            }
        }

        internal bool IsInIDTable()
        {
            return((_flags & IDFLG_IN_IDTABLE) == IDFLG_IN_IDTABLE);
        }

        internal void SetFullyConnected()
        {
            BCLDebug.Assert(
                this is ServerIdentity,
                "should be setting these flags for srvIDs only!");
            BCLDebug.Assert(
                (_ObjURI != null),
                "Object must be assigned a URI to be fully connected!");

            while(true) {
                int currentFlags = _flags;
                int newFlags = _flags & (~(IDFLG_DISCONNECTED_FULL | IDFLG_DISCONNECTED_REM));
                if(currentFlags == Interlocked.CompareExchange(ref _flags, newFlags, currentFlags))
                    break;
            }
        }

        internal bool IsFullyDisconnected()
        {
            BCLDebug.Assert(
                this is ServerIdentity,
                "should be setting these flags for srvIDs only!");
            return (_flags&IDFLG_DISCONNECTED_FULL) == IDFLG_DISCONNECTED_FULL;
        }

        internal bool IsRemoteDisconnected()
        {
            BCLDebug.Assert(
                this is ServerIdentity,
                "should be setting these flags for srvIDs only!");
            return (_flags&IDFLG_DISCONNECTED_REM) == IDFLG_DISCONNECTED_REM;
        }

        internal bool IsDisconnected()
        {
            BCLDebug.Assert(
                this is ServerIdentity,
                "should be setting these flags for srvIDs only!");
            return (IsFullyDisconnected() || IsRemoteDisconnected());
        }

        // Get the URI
        internal String URI
        {
            get
            {
                if(IsWellKnown())
                {
                    return _URL;
                }
                else
                {
                    return _ObjURI;
                }
            }
        }

        internal String ObjURI
        {
            get { return _ObjURI; }
        }

        internal MarshalByRefObject TPOrObject
        {
            get
            {
                return (MarshalByRefObject) _tpOrObject;
            }
        }

       //   Set the transparentProxy field protecting against ----s. The returned transparent
       //   proxy could be different than the one the caller is attempting to set.
       //
        internal Object  RaceSetTransparentProxy(Object tpObj)
        {
            if (_tpOrObject == null)
                Interlocked.CompareExchange(ref _tpOrObject, tpObj, null);
            return _tpOrObject;
        }

        // Get the ObjRef.
        internal ObjRef ObjectRef
        {
            [System.Security.SecurityCritical]  // auto-generated
            get
            {
                return (ObjRef) _objRef;
            }
        }

       //   Set the objRef field protecting against ----s. The returned objRef
       //   could be different than the one the caller is attempting to set.
       //
        [System.Security.SecurityCritical]  // auto-generated
        internal ObjRef  RaceSetObjRef(ObjRef objRefGiven)
        {
            if (_objRef == null)
            {
                Interlocked.CompareExchange(ref _objRef, objRefGiven, null);
            }
            return (ObjRef) _objRef;
        }


        // Get the ChannelSink.
        internal IMessageSink ChannelSink
        {
            get { return (IMessageSink) _channelSink;}
        }

       //   Set the channelSink field protecting against ----s. The returned
       //   channelSink proxy could be different than the one the caller is
       //   attempting to set.
       //
        internal IMessageSink  RaceSetChannelSink(IMessageSink channelSink)
        {
            if (_channelSink == null)
            {
                Interlocked.CompareExchange(
                                        ref _channelSink,
                                        channelSink,
                                        null);
            }
            return (IMessageSink) _channelSink;
        }

        // Get/Set the Envoy Sink chain..
        internal IMessageSink EnvoyChain
        {
            get
            {
                return (IMessageSink)_envoyChain;
            }
        }

        // Get/Set Lease
        internal Lease Lease
        {
            get
            {
                return _lease;
            }
            set
            {
                _lease = value;
            }
        }


       //   Set the channelSink field protecting against ----s. The returned
       //   channelSink proxy could be different than the one the caller is
       //   attempting to set.
       //
        internal IMessageSink RaceSetEnvoyChain(
                    IMessageSink envoyChain)
        {
            if (_envoyChain == null)
            {
                Interlocked.CompareExchange(
                                ref _envoyChain,
                                envoyChain,
                                null);
            }
            return (IMessageSink) _envoyChain;
        }

        // A URI is lazily generated for the identity based on a GUID.
        // Well known objects supply their own URI
        internal void SetOrCreateURI(String uri)
        {
            SetOrCreateURI(uri, false);
        }

        internal void SetOrCreateURI(String uri, bool bIdCtor)
        {
            if(bIdCtor == false)
            {
                // This method is called either from the ID Constructor or
                // with a writeLock on the ID Table
                BCLDebug.Assert(IdentityHolder.TableLock.IsWriterLockHeld, "IDTable should be write-locked");
                if (null != _ObjURI) {
                    throw new RemotingException(
                        Environment.GetResourceString("Remoting_SetObjectUriForMarshal__UriExists"));
                }
            }

            if(null == uri)
            {
                // We insert the tick count, so that the uri is not 100% predictable.
                // (i.e. perhaps we should consider using a random number as well)
                String random = System.Convert.ToBase64String(GetRandomBytes());
                // Need to replace the '/' with '_' since '/' is not a valid uri char
                _ObjURI = (IDGuidString + random.Replace('/',  '_') + "_" + GetNextSeqNum().ToString(CultureInfo.InvariantCulture.NumberFormat) + ".rem").ToLower(CultureInfo.InvariantCulture);
            }
            else
            {
                if (this is ServerIdentity)
                    _ObjURI = IDGuidString + uri;
                else
                    _ObjURI = uri;
            }
        } // SetOrCreateURI

        // This is used by ThreadAffinity/Synchronization contexts
        // (Shares the seqNum space with URIs)
        internal static String GetNewLogicalCallID()
        {
            return IDGuidString + GetNextSeqNum();
        }

        [System.Security.SecurityCritical]  // auto-generated
        [System.Diagnostics.Conditional("_DEBUG")]
        internal virtual void AssertValid()
        {
            if (URI != null)
            {
                Identity resolvedIdentity = IdentityHolder.ResolveIdentity(URI);
                BCLDebug.Assert(
                    (resolvedIdentity == null) || (resolvedIdentity == this),
                    "Server ID mismatch with URI");
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal bool AddProxySideDynamicProperty(IDynamicProperty prop)
        {
            lock(this)
            {
                if (_dph == null)
                {
                    DynamicPropertyHolder dph = new DynamicPropertyHolder();
                    lock(this)
                    {
                        if (_dph == null)
                        {
                            _dph = dph;
                        }
                    }
                }
                return _dph.AddDynamicProperty(prop);
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal bool RemoveProxySideDynamicProperty(String name)
        {
            lock(this)
            {
                if (_dph == null)
                {
                    throw new RemotingException(
                        String.Format(
                            CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Contexts_NoProperty"),
                            name));
                }
                return _dph.RemoveDynamicProperty(name);
            }
        }

        internal ArrayWithSize ProxySideDynamicSinks
        {
            [System.Security.SecurityCritical]  // auto-generated
            get
            {
                if (_dph == null)
                {
                    return null;
                }
                else
                {
                    return _dph.DynamicSinks;
                }
            }
        }

    #if _DEBUG
        public override String ToString()
        {
            return ("IDENTITY: " + " URI = " + _ObjURI);
        }
    #endif
    }
}
