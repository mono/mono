// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
namespace System.Runtime.Remoting {
    using System.Globalization;
    using System.Threading;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Contexts;
    using System.Runtime.Remoting.Proxies;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.ConstrainedExecution;
    using System.Reflection;
    using System;
    //  IdentityHolder maintains a lookup service for remoting identities. The methods
    //  provided by it are used during calls to Wrap, UnWrap, Marshal, Unmarshal etc.
    //
    using System.Collections;
    using System.Diagnostics.Contracts;

    // This is just a internal struct to hold the various flags
    // that get passed for different flavors of idtable operations
    // just so that we do not have too many separate boolean parameters
    // all over the place (eg. xxxIdentity(id,uri, true, false, true);)
    internal struct IdOps
    {
        internal const int None           = 0x00000000;
        internal const int GenerateURI    = 0x00000001;
        internal const int StrongIdentity = 0x00000002;
        internal const int IsInitializing = 0x00000004;    // Identity has not been fully initialized yet

        internal static bool bStrongIdentity(int flags)
        {
            return (flags&StrongIdentity)!=0;
        }

        internal static bool bIsInitializing(int flags)
        {
            return (flags & IsInitializing) != 0;
        }
    }

    // Internal enum to specify options for SetIdentity
    [Serializable]
    internal enum DuplicateIdentityOption
    {
        Unique,      // -throw an exception if there is already an identity in the table
        UseExisting, // -if there is already an identity in the table, then use that one.
                     //    (could happen in a Connect ----, but we don't care which identity we get)
    } // enum DuplicateIdentityOption
    
    
    internal sealed class IdentityHolder
    {
        // private static Timer CleanupTimer = null;
        // private const  int CleanupInterval = 60000;           // 1 minute.

        // private static Object staticSyncObject = new Object();
        private static volatile int SetIDCount=0;
        private const int CleanUpCountInterval = 0x40;
        private const int INFINITE = 0x7fffffff;

        private static Hashtable _URITable = new Hashtable();
        private static volatile Context _cachedDefaultContext = null;

           
        internal static Hashtable URITable 
        {
            get { return _URITable; }
        } 

        internal static Context DefaultContext
        {
            [System.Security.SecurityCritical]  // auto-generated
            get
            {
                if (_cachedDefaultContext == null)
                {
                    _cachedDefaultContext = Thread.GetDomain().GetDefaultContext();
                }
                return _cachedDefaultContext;
            }
        }

        // NOTE!!!: This must be used to convert any uri into something that can
        //   be used as a key in the URITable!!!
        private static String MakeURIKey(String uri) 
        { 
            return Identity.RemoveAppNameOrAppGuidIfNecessary(
                uri.ToLower(CultureInfo.InvariantCulture)); 
        }       
        
        private static String MakeURIKeyNoLower(String uri) 
        { 
            return Identity.RemoveAppNameOrAppGuidIfNecessary(uri); 
        }       

        internal static ReaderWriterLock TableLock 
        {
            get { return Thread.GetDomain().RemotingData.IDTableLock;}
        }


        //  Cycles through the table periodically and cleans up expired entries.
        //
        private static void CleanupIdentities(Object state)
        {
            // <
            Contract.Assert(
                Thread.GetDomain().RemotingData.IDTableLock.IsWriterLockHeld,
                "ID Table being cleaned up without taking a lock!");

            IDictionaryEnumerator e = URITable.GetEnumerator();
            ArrayList removeList = new ArrayList();
            while (e.MoveNext())
            {
                Object o = e.Value;
                WeakReference wr = o as WeakReference;
                if ((null != wr) && (null == wr.Target))
                {
                    removeList.Add(e.Key);
                }
            }
            
            foreach (String key in removeList)
            {
                URITable.Remove(key);
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal static void FlushIdentityTable()
        {
            // We need to guarantee that finally is not interrupted so that the lock is released.
            // TableLock has a long path without reliability contract.  To avoid adding contract on
            // the path, we will use ReaderWriterLock directly.
            ReaderWriterLock rwlock = TableLock;
            bool takeAndRelease = !rwlock.IsWriterLockHeld;

            RuntimeHelpers.PrepareConstrainedRegions();
            try{
                if (takeAndRelease)
                    rwlock.AcquireWriterLock(INFINITE);
                CleanupIdentities(null);
            }
            finally{
                if(takeAndRelease && rwlock.IsWriterLockHeld){
                    rwlock.ReleaseWriterLock();
                }
            }
        }    

        private IdentityHolder() {          // this is a singleton object. Can't construct it.
        }


        //  Looks up the identity corresponding to a URI.
        //
        [System.Security.SecurityCritical]  // auto-generated
        internal static Identity ResolveIdentity(String URI)
        {
            if (URI == null)
                throw new ArgumentNullException("URI");
            Contract.EndContractBlock();
        
            Identity id;
            // We need to guarantee that finally is not interrupted so that the lock is released.
            // TableLock has a long path without reliability contract.  To avoid adding contract on
            // the path, we will use ReaderWriterLock directly.
            ReaderWriterLock rwlock = TableLock;
            bool takeAndRelease = !rwlock.IsReaderLockHeld;

            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                if (takeAndRelease)
                    rwlock.AcquireReaderLock(INFINITE);

                Message.DebugOut("ResolveIdentity:: URI: " + URI + "\n");       
                Message.DebugOut("ResolveIdentity:: table.count: " + URITable.Count + "\n");
                //Console.WriteLine("\n ResolveID: URI = " + URI);
                // This may be called both in the client process and the server process (loopback case).
                id = ResolveReference(URITable[MakeURIKey(URI)]);
            }
            finally
            {
                if (takeAndRelease && rwlock.IsReaderLockHeld){
                    rwlock.ReleaseReaderLock();
                }
            }
            return id;
        } // ResolveIdentity


        // If the identity isn't found, this version will just return
        //   null instead of asserting (this version doesn't need to
        //   take a lock).
        [System.Security.SecurityCritical]  // auto-generated
        internal static Identity CasualResolveIdentity(String uri)
        {
            if (uri == null)
                return null;

            Identity id = CasualResolveReference(URITable[MakeURIKeyNoLower(uri)]);
            if (id == null) {
                id = CasualResolveReference(URITable[MakeURIKey(uri)]);

                // DevDiv 720951 and 911924:
                // CreateWellKnownObject inserts the Identity into the URITable before
                // it is fully initialized.  This can cause a race condition if another
                // concurrent operation re-enters this code and attempts to use it. 
                // If we discover this situation, behave as if it is not in the URITable.
                // This falls into the code below to call CreateWellKnownObject again.
                // That method operates under a lock and will not return until it
                // has been fully initialized.  It will not create or initialize twice.
                if (id == null || id.IsInitializing)
                {
                    // Check if this a well-known object which needs to be faulted in
                    id = RemotingConfigHandler.CreateWellKnownObject(uri);                
                }
            }

            return id;
        } // CasualResolveIdentity
        

        private static Identity ResolveReference(Object o)
        {
            Contract.Assert(
                TableLock.IsReaderLockHeld || TableLock.IsWriterLockHeld ,
                "Should have locked the ID Table!");
            WeakReference wr = o as WeakReference;    
            if (null != wr)
            {
                return((Identity) wr.Target);
            }
            else
            {
                return((Identity) o);
            }
        } // ResolveReference

        private static Identity CasualResolveReference(Object o)
        {
            WeakReference wr = o as WeakReference;    
            if (null != wr)
            {
                return((Identity) wr.Target);
            }
            else
            {
                return((Identity) o);
            }
        } // CasualResolveReference

       //
       //
        // This is typically called when we need to create/establish
        // an identity for a serverObject.               
        [System.Security.SecurityCritical]  // auto-generated
        internal static ServerIdentity FindOrCreateServerIdentity(
            MarshalByRefObject obj,  String objURI, int flags) 
        {
            Message.DebugOut("Entered FindOrCreateServerIdentity \n");
                    
            ServerIdentity srvID = null;

            bool fServer;
            srvID = (ServerIdentity) MarshalByRefObject.GetIdentity(obj, out fServer);

            if (srvID == null)
            {
                // Create a new server identity and add it to the
                // table. IdentityHolder will take care of ----s
                Context serverCtx = null;
                
                if (obj is ContextBoundObject)
                {
                    serverCtx = Thread.CurrentContext;
                }
                else
                {
                    serverCtx = DefaultContext;
                }
                Contract.Assert(null != serverCtx, "null != serverCtx");

                ServerIdentity serverID = new ServerIdentity(obj, serverCtx);

                // Set the identity depending on whether we have the server or proxy
                if(fServer)
                {
                    srvID = obj.__RaceSetServerIdentity(serverID);
                    Contract.Assert(srvID == MarshalByRefObject.GetIdentity(obj), "Bad ID state!" );             
                }
                else
                {
                    RealProxy rp = null;
                    rp = RemotingServices.GetRealProxy(obj);
                    Contract.Assert(null != rp, "null != rp");

                    rp.IdentityObject = serverID;
                    srvID = (ServerIdentity) rp.IdentityObject;
                }

                // DevDiv 720951 and 911924:
                // CreateWellKnownObject creates a ServerIdentity and places it in URITable
                // before it is fully initialized.  This transient flag is set to to prevent
                // other concurrent operations from using it.  CreateWellKnownObject is the
                // only code path that sets this flag, and by default it is false.
                if (IdOps.bIsInitializing(flags))
                {
                    srvID.IsInitializing = true;
                }

                Message.DebugOut("Created ServerIdentity \n");
            }

#if false
            // Check that we are asked to create the identity for the same
            // URI as the one already associated with the server object.
            // It is an error to associate two URIs with the same server 
            // object
            // GopalK: Try eliminating the test because it is also done by GetOrCreateIdentity
            if ((null != objURI) && (null != srvID.ObjURI))
            {
                if (string.Compare(objURI, srvID.ObjURI, StringComparison.OrdinalIgnoreCase) == 0) // case-insensitive compare
                {
                    Message.DebugOut("Trying to associate a URI with identity again .. throwing execption \n");
                    throw new RemotingException(
                        String.Format(
                            Environment.GetResourceString(
                                "Remoting_ResetURI"),
                            srvID.ObjURI, objURI));
                }
            }
#endif

            // NOTE: for purely x-context cases we never execute this ...
            // the server ID is not put in the ID table. 
            if ( IdOps.bStrongIdentity(flags) )
            {
                // We need to guarantee that finally is not interrupted so that the lock is released.
                // TableLock has a long path without reliability contract.  To avoid adding contract on
                // the path, we will use ReaderWriterLock directly.
                ReaderWriterLock rwlock = TableLock;
                bool takeAndRelease = !rwlock.IsWriterLockHeld;

                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    if (takeAndRelease)
                        rwlock.AcquireWriterLock(INFINITE);

                    // It is possible that we are marshaling out of this app-domain
                    // for the first time. We need to do two things
                    // (1) If there is no URI associated with the identity then go ahead 
                    // and generate one.
                    // (2) Add the identity to the URI -> Identity map if not already present
                    // (For purely x-context cases we don't need the URI)   
                    // (3) If the object ref is null, then this object hasn't been
                    // marshalled yet.
                    // (4) if id was created through SetObjectUriForMarshal, it would be
                    // in the ID table
                    if ((srvID.ObjURI == null) ||
                       (srvID.IsInIDTable() == false))
                    {
                        // we are marshalling a server object, so there should not be a
                        //   a different identity at this location.
                        SetIdentity(srvID, objURI, DuplicateIdentityOption.Unique);
                    }

                    // If the object is marked as disconnect, mark it as connected
                    if(srvID.IsDisconnected())
                            srvID.SetFullyConnected();
                }
                finally
                {
                    if (takeAndRelease && rwlock.IsWriterLockHeld)
                    {
                        rwlock.ReleaseWriterLock();
                    }
                }
            }

            Message.DebugOut("Leaving FindOrCreateServerIdentity \n");
            Contract.Assert(null != srvID,"null != srvID");
            return srvID;                
        }

        //
        //
        // This is typically called when we are unmarshaling an objectref
        // in order to create a client side identity for a remote server
        // object.
        [System.Security.SecurityCritical]  // auto-generated
        internal static Identity FindOrCreateIdentity(
            String objURI, String URL, ObjRef objectRef)
        {
            Identity idObj = null;

            Contract.Assert(null != objURI,"null != objURI");

            bool bWellKnown = (URL != null);

            // Lookup the object in the identity table
            // for well-known objects we user the URL
            // as the hash-key (instead of just the objUri)
            idObj = ResolveIdentity(bWellKnown ? URL : objURI);
            if (bWellKnown &&
                (idObj != null) &&
                (idObj is ServerIdentity))
            {
                // We are trying to do a connect to a server wellknown object.
                throw new RemotingException(
                    String.Format(
                        CultureInfo.CurrentCulture, Environment.GetResourceString(
                            "Remoting_WellKnown_CantDirectlyConnect"),
                        URL));                            
            }
                 
            if (null == idObj)
            {
                // There is no entry for this uri in the IdTable.
                Message.DebugOut("RemotingService::FindOrCreateIdentity: Creating Identity\n");

                // This identity is being encountered for the first time.
                // We have to do the following things
                // (1) Create an identity object for the proxy
                // (2) Add the identity to the identity table
                // (3) Create a proxy for the object represented by the objref      
                
                // Create a new identity
                // <EMAIL>GopalK:</EMAIL> Identity should get only one string that is used for everything
                idObj = new Identity(objURI, URL);                         

                // We need to guarantee that finally is not interrupted so that the lock is released.
                // TableLock has a long path without reliability contract.  To avoid adding contract on
                // the path, we will use ReaderWriterLock directly.
                ReaderWriterLock rwlock = TableLock;
                bool takeAndRelease = !rwlock.IsWriterLockHeld;

                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {                
                    // Add it to the identity table
                    if (takeAndRelease)
                        rwlock.AcquireWriterLock(INFINITE);

                    // SetIdentity will give the correct Id if we raced
                    // between the ResolveIdentity call above and now.
                    //   (we are unmarshaling, and the server should guarantee
                    //    that the uri is unique, so we will use an existing identity
                    //    in case of a ----)
                    idObj = SetIdentity(idObj, null, DuplicateIdentityOption.UseExisting);

                    idObj.RaceSetObjRef(objectRef);
                }
                finally
                {
                    if (takeAndRelease && rwlock.IsWriterLockHeld)
                    {
                        rwlock.ReleaseWriterLock();
                    }                
                }
            }
            else
            {
                Message.DebugOut("RemotingService::FindOrCreateIdentity: Found Identity!\n");
            }
            Contract.Assert(null != idObj,"null != idObj");
            return idObj;                
        }


        //  Creates an identity entry. 
        //  This is used by Unmarshal and Marshal to generate the URI to identity 
        //  mapping
        //  
        //
        [System.Security.SecurityCritical]  // auto-generated
        private static Identity SetIdentity(
            Identity idObj, String URI, DuplicateIdentityOption duplicateOption)
        {
            // NOTE: This function assumes that a lock has been taken 
            // by the calling function
            // idObj could be for a transparent proxy or a server object        
            Message.DebugOut("SetIdentity:: domainid: " + Thread.GetDomainID() + "\n");
            Contract.Assert(null != idObj,"null != idObj");
            
            // WriterLock must already be taken when SetIdentity is called!
            Contract.Assert(
                TableLock.IsWriterLockHeld,
                "Should have write-locked the ID Table!");

            // flag to denote that the id being set is a ServerIdentity
            bool bServerIDSet = idObj is ServerIdentity;
                
            if (null == idObj.URI)
            {
                // No URI has been associated with this identity. It must be a 
                // server identity getting marshaled out of the app domain for 
                // the first time.
                Contract.Assert(bServerIDSet,"idObj should be ServerIdentity");

                // Set the URI on the idObj (generating one if needed)
                idObj.SetOrCreateURI(URI);

                // If objectref is non-null make sure both have same URIs
                // (the URI in the objectRef could have potentially been reset
                // in a past external call to Disconnect()
                if (idObj.ObjectRef != null)
                {
                    idObj.ObjectRef.URI = idObj.URI;
                }
                Message.DebugOut("SetIdentity: Generated URI " + URI + " for identity");
            }

            // If we have come this far then there is no URI to identity
            // mapping present. Go ahead and create one.

            // ID should have a URI by now.
            Contract.Assert(null != idObj.URI,"null != idObj.URI");

            // See if this identity is already present in the Uri table
            String uriKey = MakeURIKey(idObj.URI);
            Object o = URITable[uriKey];

            // flag to denote that the id found in the table is a ServerIdentity
            bool bServerID;
            if (null != o)
            {
                // We found an identity (or a WeakRef to one) for the URI provided
                WeakReference wr = o as WeakReference;
                Identity idInTable = null;
                if (wr != null)
                {
                    // The object we found is a weak referece to an identity
                    
                    // This could be an identity for a client side
                    // proxy 
                    // OR
                    // a server identity which has been weakened since its life
                    // is over.
                    idInTable = (Identity) wr.Target;

                    bServerID = idInTable is ServerIdentity;

                    // If we find a weakRef for a ServerId we will be converting
                    // it to a strong one before releasing the IdTable lock.
                    Contract.Assert(
                        (idInTable == null)||
                        (!bServerID || idInTable.IsRemoteDisconnected()),
                        "Expect to find WeakRef only for remotely disconnected ids");
                    // We could find a weakRef to a client ID that does not 
                    // match the idObj .. but that is a handled ---- case 
                    // during Unmarshaling .. SetIdentity() will return the ID
                    // from the table to the caller.
                }
                else
                {
                    // We found a non-weak (strong) Identity for the URI
                    idInTable = (Identity) o;
                    bServerID = idInTable is ServerIdentity;

                    //We dont put strong refs to client "Identity"s in the table                    
                    Contract.Assert(
                        bServerID, 
                        "Found client side strong ID in the table");
                }

                if ((idInTable != null) && (idInTable != idObj))
                {
                    // We are trying to add another identity for the same URI
                    switch (duplicateOption)
                    {
                    
                    case DuplicateIdentityOption.Unique:
                    {
                        
                        String tempURI = idObj.URI;  

                        // Throw an exception to indicate the error since this could
                        // be caused by a user trying to marshal two objects with the same
                        // URI
                        throw new RemotingException(
                            Environment.GetResourceString("Remoting_URIClash",
                                tempURI));
                    } // case DuplicateIdentityOption.Unique
                    
                    case DuplicateIdentityOption.UseExisting:
                    {
                        // This would be a case where our thread lost the ----
                        // we will return the one found in the table
                        idObj = idInTable;
                        break;
                    } // case DuplicateIdentityOption.UseExisting:
                    
                    default:
                    {
                        Contract.Assert(false, "Invalid DuplicateIdentityOption");
                        break;
                    }
                    
                    } // switch (duplicateOption)
                    
                }
                else
                if (wr!=null)
                {                   
                    // We come here if we found a weakRef in the table but
                    // the target object had been cleaned up 
                    // OR
                    // If there was a weakRef in the table and the target
                    // object matches the idObj just passed in
                    
                    // Strengthen the entry if it a ServerIdentity.
                    if (bServerID)
                    {                       
                        URITable[uriKey] = idObj;
                    }
                    else
                    {
                        // For client IDs associate the table entry
                        // with the one passed in.
                        // (If target was null we would set it ... 
                        // if was non-null then it matches idObj anyway)
                        wr.Target = idObj;  
                    }
                }
            }
            else
            {
                // We did not find an identity entry for the URI
                Object addMe = null;
                if (bServerIDSet)
                {
                    addMe = idObj;
                    ((ServerIdentity)idObj).SetHandle();
                }
                else
                {
                    addMe = new WeakReference(idObj);
                }                    
                
                // Add the entry into the table
                URITable.Add(uriKey, addMe);
                idObj.SetInIDTable();
                
                // After every fixed number of set-id calls we run through
                // the table and cleanup if needed.             
                SetIDCount++;
                if (SetIDCount % CleanUpCountInterval == 0)
                {
                    // This should be called with the write lock held!
                    //   (which is why we assert that at the beginning of this
                    //    method)
                    CleanupIdentities(null);
                }

            }
            
            Message.DebugOut("SetIdentity:: Identity::URI: " + idObj.URI + "\n");       
            return idObj;
        }

#if false
         //  Convert table entry to a weak reference
         //
        internal static void WeakenIdentity(String URI)
        {
            Contract.Assert(URI!=null, "Null URI");
            BCLDebug.Trace("REMOTE", 
                "IdentityHolder.WeakenIdentity ",URI, " for context ", Thread.CurrentContext);         
            
            String uriKey = MakeURIKey(URI);
            // We need to guarantee that finally is not interrupted so that the lock is released.
            // TableLock has a long path without reliability contract.  To avoid adding contract on
            // the path, we will use ReaderWriterLock directly.
            ReaderWriterLock rwlock = TableLock;
            bool takeAndRelease = !rwlock.IsWriterLockHeld;

            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                if (takeAndRelease)
                    rwlock.AcquireWriterLock(INFINITE);

                Object oRef = URITable[uriKey];
                WeakReference wr = oRef as WeakReference;
                if (null == wr)
                {                    
                    // Make the id a weakRef if it isn't already.
                    Contract.Assert(
                       oRef != null &&  (oRef is ServerIdentity), 
                       "Invaild URI given to WeakenIdentity");
                       
                    URITable[uriKey] = new WeakReference(oRef);
                }
            }
            finally
            {
                if (takeAndRelase && rwlock.IsWriterLockHeld){
                    rwlock.ReleaseWriterLock();
                }
            }
        }
#endif

        [System.Security.SecurityCritical]  // auto-generated
        internal static void RemoveIdentity(String uri)
        {
            RemoveIdentity(uri, true);
        }
        
        [System.Security.SecurityCritical]  // auto-generated
        internal static void RemoveIdentity(String uri, bool bResetURI)
        {
            Contract.Assert(uri!=null, "Null URI");
            BCLDebug.Trace("REMOTE",
                "IdentityHolder.WeakenIdentity ",uri, " for context ", Thread.CurrentContext);

            Identity id;
            String uriKey = MakeURIKey(uri);
            // We need to guarantee that finally is not interrupted so that the lock is released.
            // TableLock has a long path without reliability contract.  To avoid adding contract on
            // the path, we will use ReaderWriterLock directly.
            ReaderWriterLock rwlock = TableLock;
            bool takeAndRelease = !rwlock.IsWriterLockHeld;

            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                if (takeAndRelease)
                    rwlock.AcquireWriterLock(INFINITE);

                Object oRef = URITable[uriKey];
                WeakReference wr = oRef as WeakReference;
                if (null != wr)
                {
                    id = (Identity) wr.Target;
                    wr.Target = null;
                }
                else
                {
                    id = (Identity) oRef;
                    if (id != null)
                        ((ServerIdentity)id).ResetHandle();
                }

                if(id != null)
                {
                    URITable.Remove(uriKey);
                    // Mark the ID as not present in the ID Table
                    // This will clear its URI & objRef fields
                    id.ResetInIDTable(bResetURI);
                }
            }
            finally
            {
                if (takeAndRelease && rwlock.IsWriterLockHeld){
                    rwlock.ReleaseWriterLock();
                }
            }
        } // RemoveIdentity


        // Support for dynamically registered property sinks
        [System.Security.SecurityCritical]  // auto-generated
        internal static bool AddDynamicProperty(MarshalByRefObject obj, IDynamicProperty prop)
        {
            if (RemotingServices.IsObjectOutOfContext(obj))
            {
                // We have to add a proxy side property, get the identity
                RealProxy rp = RemotingServices.GetRealProxy(obj);
                return rp.IdentityObject.AddProxySideDynamicProperty(prop);            
            }
            else
            {
                MarshalByRefObject realObj = 
                    (MarshalByRefObject)
                        RemotingServices.AlwaysUnwrap((ContextBoundObject)obj);
                // This is a real object. See if we have an identity for it
                ServerIdentity srvID = (ServerIdentity)MarshalByRefObject.GetIdentity(realObj);
                if (srvID != null)
                {
                    return srvID.AddServerSideDynamicProperty(prop);
                }
                else
                {
                    // identity not found, we can't set a sink for this object.
                    throw new RemotingException(
                       Environment.GetResourceString("Remoting_NoIdentityEntry"));

                }                        
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal static bool RemoveDynamicProperty(MarshalByRefObject obj, String name)
        {
            if (RemotingServices.IsObjectOutOfContext(obj))
            {
                // We have to add a proxy side property, get the identity
                RealProxy rp = RemotingServices.GetRealProxy(obj);
                return rp.IdentityObject.RemoveProxySideDynamicProperty(name);            
            }
            else
            {

                MarshalByRefObject realObj = 
                    (MarshalByRefObject)
                        RemotingServices.AlwaysUnwrap((ContextBoundObject)obj);
                        
                // This is a real object. See if we have an identity for it
                ServerIdentity srvID = (ServerIdentity)MarshalByRefObject.GetIdentity(realObj);
                if (srvID != null)
                {
                    return srvID.RemoveServerSideDynamicProperty(name);
                }
                else
                {
                    // identity not found, we can't set a sink for this object.
                    throw new RemotingException(
                       Environment.GetResourceString("Remoting_NoIdentityEntry"));
                }
            }
        }
    } // class IdentityHolder

}
