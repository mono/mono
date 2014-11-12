// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
namespace System.Runtime.Remoting {
    using System;
    using System.Collections;
    using System.Threading;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Runtime.Remoting.Activation;
    using System.Runtime.Remoting.Contexts;
    using System.Runtime.Remoting.Messaging;    
    using System.Runtime.Remoting.Proxies;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.CompilerServices;
    using System.Globalization;
    
    
    //  ServerIdentity derives from Identity and holds the extra server specific information
    //  associated with each instance of a remoted server object.
    //    
    internal class ServerIdentity : Identity
    {
        // Internal members 
        internal Context _srvCtx;

        // This is used to cache the last server type 
        private class LastCalledType
        {
            public String      typeName;
            public Type  type;
        }
        // These two fields are used for (purely) MarshalByRef object identities
        // For context bound objects we have corresponding fields in RemotingProxy
        // that are used instead. This is done to facilitate GC in x-context cases.
        internal IMessageSink _serverObjectChain;
// disable csharp compiler warning #0414: field assigned unused value
#pragma warning disable 0414
        internal StackBuilderSink _stackBuilderSink;
#pragma warning restore 0414
    
        // This manages the dynamic properties registered on per object/proxy basis
        internal DynamicPropertyHolder _dphSrv;
    
        internal Type _srvType;  // type of server object
        private LastCalledType _lastCalledType; // cache the last type object
        internal bool _bMarshaledAsSpecificType = false;
        internal int _firstCallDispatched = 0;
        
        internal GCHandle   _srvIdentityHandle;

        internal Type GetLastCalledType(String newTypeName)
        {
            LastCalledType lastType = _lastCalledType;                        
            if (lastType == null)
                return null;

            String typeName = lastType.typeName;
            Type t = lastType.type;

            if (typeName==null || t==null)
                return null;

            if (typeName.Equals(newTypeName))
                return t;

            return null;
        } // GetLastCalledMethod

        internal void SetLastCalledType(String newTypeName, Type newType)
        {
            LastCalledType lastType = new LastCalledType();           
            lastType.typeName = newTypeName;
            lastType.type = newType;

            _lastCalledType = lastType;
        } // SetLastCalledMethod

        [System.Security.SecurityCritical]  // auto-generated
        internal void SetHandle()
        {
            bool fLocked = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                Monitor.Enter(this, ref fLocked);
                if (!_srvIdentityHandle.IsAllocated)
                    _srvIdentityHandle = new GCHandle(this, GCHandleType.Normal);
                else
                    _srvIdentityHandle.Target = this;
            }
            finally
            {
                if (fLocked)
                {
                    Monitor.Exit(this);
                }
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal void ResetHandle()
        {
            bool fLocked = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                Monitor.Enter(this, ref fLocked);
                _srvIdentityHandle.Target = null;
            }
            finally
            {
                if (fLocked)
                {
                    Monitor.Exit(this);
                }
            }
        }

        internal GCHandle GetHandle()
        {
            return _srvIdentityHandle;
        }
        
        
        //   Creates a new server identity. This form is used by RemotingServices.Wrap
        //
        [System.Security.SecurityCritical]  // auto-generated
        internal ServerIdentity(MarshalByRefObject obj, Context serverCtx) : base(obj is ContextBoundObject)
        {            
            if(null != obj)
            {
                if(!RemotingServices.IsTransparentProxy(obj))
                {
                    _srvType = obj.GetType();
                }
                else
                {
                    RealProxy rp = RemotingServices.GetRealProxy(obj);
                    _srvType =   rp.GetProxiedType();
                }
            }
            
            _srvCtx = serverCtx;
            _serverObjectChain = null; 
            _stackBuilderSink = null;
        }

        // This is used by RS::SetObjectUriForMarshal
        [System.Security.SecurityCritical]  // auto-generated
        internal ServerIdentity(MarshalByRefObject obj, Context serverCtx, String uri) : 
            this(obj, serverCtx)
        {
            SetOrCreateURI(uri, true); // calling from the constructor
        }
        
    
        // Informational methods on the ServerIdentity.
        // Get the native context for the server object.
        internal Context ServerContext
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]          
            get {return _srvCtx;}
        }
    
        internal void SetSingleCallObjectMode()
        {
            BCLDebug.Assert( !IsSingleCall() && !IsSingleton(), "Bad serverID");
            _flags |= IDFLG_SERVER_SINGLECALL; 
        }

        internal void SetSingletonObjectMode()
        {
            BCLDebug.Assert( !IsSingleCall() && !IsSingleton(), "Bad serverID");
            _flags |= IDFLG_SERVER_SINGLETON; 
        }
       
        internal bool IsSingleCall()
        {
            return ((_flags&IDFLG_SERVER_SINGLECALL) != 0); 
        }

        internal bool IsSingleton()
        {
            return ((_flags&IDFLG_SERVER_SINGLETON) != 0); 
        }
    
        [System.Security.SecurityCritical]  // auto-generated
        internal IMessageSink GetServerObjectChain(out MarshalByRefObject obj)
        {
            obj = null;
            // NOTE: Lifetime relies on the Identity flags for 
            // SingleCall and Singleton being set by the time this getter 
            // is called.
                if (!this.IsSingleCall())
                {
                    // This is the common case 
                    if (_serverObjectChain == null) 
                    {
                        bool fLocked = false;
                        RuntimeHelpers.PrepareConstrainedRegions();
                        try
                        {
                            Monitor.Enter(this, ref fLocked);
                            if(_serverObjectChain == null)
                            {
                                MarshalByRefObject srvObj = 
                                    (MarshalByRefObject) 
                                        this.TPOrObject;

                                _serverObjectChain = 
                                    _srvCtx.CreateServerObjectChain(
                                        srvObj);
                                    
                            }
                        }   
                        finally
                        {
                            if (fLocked)
                            {
                                Monitor.Exit(this);
                            }
                        }
                    }
                    BCLDebug.Assert( null != _serverObjectChain, 
                        "null != _serverObjectChain");

                    return _serverObjectChain;                    
                }
                else 
                {
                    // ---------- SINGLE CALL WKO --------------
                    // In this case, we are expected to provide 
                    // a fresh server object for each dispatch.
                    // Since the server object chain is object 
                    // specific, we must create a fresh chain too.

                    // We must be in the correct context for this
                    // to succeed.

                    // <











                    BCLDebug.Assert(Thread.CurrentContext==_srvCtx,
                                    "Bad context mismatch");

                    MarshalByRefObject srvObj = null;
                    IMessageSink objChain = null;
                    if (_tpOrObject != null && _firstCallDispatched == 0 && Interlocked.CompareExchange(ref _firstCallDispatched, 1, 0) == 0)
                    {
                        // use the instance of server object created to 
                        // set up the pipeline.
                        srvObj = (MarshalByRefObject) _tpOrObject;

                        objChain = _serverObjectChain;

                        if (objChain == null)
                        {
                            objChain = _srvCtx.CreateServerObjectChain(srvObj);
                        }
                    }
                    else
                    {
                        // For singleCall we create a fresh object & its chain
                        // on each dispatch!
                        srvObj = (MarshalByRefObject)
                                 Activator.CreateInstance((Type)_srvType, true);

                        // make sure that object didn't Marshal itself.
                        // (well known objects should live up to their promise
                        // of exporting themselves through exactly one url)
                        String tempUri = RemotingServices.GetObjectUri(srvObj);
                        if (tempUri != null)
                        {
                            throw new RemotingException(
                                String.Format(
                                              CultureInfo.CurrentCulture, Environment.GetResourceString(
                                "Remoting_WellKnown_CtorCantMarshal"),
                                              this.URI));
                        }

                        // Set the identity depending on whether we have the server
                        // or proxy
                        if(!RemotingServices.IsTransparentProxy(srvObj))
                        {

#if _DEBUG
                            Identity idObj = srvObj.__RaceSetServerIdentity(this);
#else
                            srvObj.__RaceSetServerIdentity(this);
#endif
#if _DEBUG
                            BCLDebug.Assert(idObj == this, "Bad ID state!" );             
                            BCLDebug.Assert(idObj == MarshalByRefObject.GetIdentity(srvObj), "Bad ID state!" );             
#endif
                        }
                        else
                        {
                            RealProxy rp = null;
                            rp = RemotingServices.GetRealProxy(srvObj);
                            BCLDebug.Assert(null != rp, "null != rp");
                            //  #if _DEBUG
                            //                      Identity idObj = (ServerIdentity) rp.SetIdentity(this);
                            // #else
                            rp.IdentityObject = this;
                            // #endif 
#if false
#if _DEBUG
                            // 





#endif
#endif
                        }
                        // Create the object chain and return it
                        objChain = _srvCtx.CreateServerObjectChain(srvObj);
                    }

                    // This is passed out to the caller so that for single-call
                    // case we can call Dispose when the incoming call is done
                    obj = srvObj;
                    return objChain;
                }
        }
    
        internal Type ServerType
        {
            get { return _srvType; }
            set { _srvType = value; }
        } // ServerType

        internal bool MarshaledAsSpecificType
        {
            get { return _bMarshaledAsSpecificType; }
            set { _bMarshaledAsSpecificType = value; }
        } // MarshaledAsSpecificType
        
    
        [System.Security.SecurityCritical]  // auto-generated
        internal IMessageSink RaceSetServerObjectChain(
            IMessageSink serverObjectChain)
        {
            if (_serverObjectChain == null)
            {
                bool fLocked = false;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    Monitor.Enter(this, ref fLocked);
                    if (_serverObjectChain == null)
                    {
                        _serverObjectChain = serverObjectChain;
                    }
                }
                finally
                {
                    if (fLocked)
                    {
                        Monitor.Exit(this);
                    }
                }
            }
            return _serverObjectChain;       
        }
    
        /*package*/
        [System.Security.SecurityCritical]  // auto-generated
        internal bool AddServerSideDynamicProperty(
            IDynamicProperty prop)
        {
            if (_dphSrv == null)
            {
                DynamicPropertyHolder dphSrv = new DynamicPropertyHolder();
                bool fLocked = false;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    Monitor.Enter(this, ref fLocked);
                    if (_dphSrv == null)
                    {
                        _dphSrv = dphSrv;
                    }
                }
                finally
                {
                    if (fLocked)
                    {
                        Monitor.Exit(this);
                    }
                }
            }
            return _dphSrv.AddDynamicProperty(prop);
        }
        
        /*package*/
        [System.Security.SecurityCritical]  // auto-generated
        internal bool RemoveServerSideDynamicProperty(String name)
        {
            if (_dphSrv == null) 
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_PropNotFound") );        
            }
            return _dphSrv.RemoveDynamicProperty(name);
        }
    
        internal ArrayWithSize ServerSideDynamicSinks
        {
            [System.Security.SecurityCritical]  // auto-generated
            get
            {
                if (_dphSrv == null)
                    {
                        return null;
                    }
                else
                    {
                        return _dphSrv.DynamicSinks;
                    }
            }
        }
               
        [System.Security.SecurityCritical]  // auto-generated
        internal override void AssertValid()
        {
            base.AssertValid();
            if((null != this.TPOrObject) && !RemotingServices.IsTransparentProxy(this.TPOrObject))
            {
                BCLDebug.Assert(MarshalByRefObject.GetIdentity((MarshalByRefObject)this.TPOrObject) == this, "Server ID mismatch with Object");
            }
        }
    }
}
    













