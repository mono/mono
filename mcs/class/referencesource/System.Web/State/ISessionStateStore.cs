//------------------------------------------------------------------------------
// <copyright file="SessionStateStore.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * SessionStateStoreProviderBase
 * 
 */
namespace System.Web.SessionState {
    using System.Xml;
    using System.Security.Permissions;
    using System.Configuration.Provider;
    using System.Collections.Specialized; 

    [FlagsAttribute()]
    internal enum SessionStateItemFlags : int {
        None =                          0x00000000,
        Uninitialized =                 0x00000001,
        IgnoreCacheItemRemoved =        0x00000002
    }

    [FlagsAttribute()]
    public enum SessionStateActions : int {
        None  =             0x00000000,
        InitializeItem =    0x00000001
    }

    // This interface is used by SessionStateModule to read/write the session state data
    public abstract class SessionStateStoreProviderBase : ProviderBase {
        public abstract void Dispose();

        // Called by SessionStateModule to notify the provider that Session_End is defined
        // in global.asax, and so when an item expires, it should call the expireCallback
        // If the provider does not support session expiry, it should return false.
        public abstract bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback);
        
        // Called at the beginning of the AcquireRequestState event
        public abstract void InitializeRequest(HttpContext context);

        // Get and return a SessionStateStoreData. 
        // Please note that we are implementing a reader/writer lock mechanism.
        //
        // If successful:
        //  - returns the item
        //
        // If not found:
        //  - set 'locked' to false
        //  - returns null
        //
        // If the item is already locked by another request:
        //  - set 'locked' to true
        //  - set 'lockAge' to how long has the item been locked
        //  - set 'lockId' to the context of the lock
        //  - returns null
        public abstract SessionStateStoreData GetItem(HttpContext context,
                                        String id, 
                                        out bool locked,
                                        out TimeSpan lockAge, 
                                        out object lockId,
                                        out SessionStateActions actions);

        // Get and lock a SessionStateStoreData. 
        // Please note that we are implementing a reader/writer lock mechanism.
        //
        // If successful:
        //  - set 'lockId' to the context of the lock
        //  - returns the item
        //
        // If not found:
        //  - set 'locked' to false
        //  - returns null
        //
        // If the item is already locked by another request:
        //  - set 'locked' to true
        //  - set 'lockAge' to how long has the item been locked
        //  - set 'lockId' to the context of the lock
        //  - returns null
        public abstract SessionStateStoreData GetItemExclusive(HttpContext context, 
                                                String id, 
                                                out bool locked,
                                                out TimeSpan lockAge, 
                                                out object lockId,
                                                out SessionStateActions actions);

        // Unlock an item locked by GetExclusive
        // 'lockId' is the lock context returned by previous call to GetExclusive
        public abstract void ReleaseItemExclusive(HttpContext context, 
                                                    String id, 
                                                    object lockId);

        // Write an item.  
        // Note: The item is originally obtained by GetExclusive
        // Because SessionStateModule will release (by ReleaseExclusive) am item if 
        // it has been locked for too long, so it is possible that the request calling
        // Set() may have lost the lock to someone else already.  This can be
        // discovered by comparing the supplied lockId with the lockId value 
        // stored with the state item.
        public abstract void SetAndReleaseItemExclusive(HttpContext context, 
                                    String id, 
                                    SessionStateStoreData item, 
                                    object lockId, 
                                    bool newItem);
        
        // Remove an item.  See the note in Set.
        public abstract void RemoveItem(HttpContext context, 
                                        String id, 
                                        object lockId, 
                                        SessionStateStoreData item);

        // Reset the expire time of an item based on its timeout value
        public abstract void ResetItemTimeout(HttpContext context, String id);

        // Create a brand new SessionStateStoreData. The created SessionStateStoreData must have
        // a non-null ISessionStateItemCollection.
        public abstract SessionStateStoreData CreateNewStoreData(HttpContext context, int timeout);

        public abstract void CreateUninitializedItem(HttpContext context, String id, int timeout);

        // Called during EndRequest event
        public abstract void EndRequest(HttpContext context);

        internal virtual void Initialize(string name, NameValueCollection config, IPartitionResolver partitionResolver) {
        }
    }

    public class SessionStateStoreData {
        ISessionStateItemCollection      _sessionItems;
        HttpStaticObjectsCollection _staticObjects;
        int                         _timeout;

        public SessionStateStoreData(ISessionStateItemCollection sessionItems,
                                    HttpStaticObjectsCollection staticObjects,
                                    int timeout) {
            _sessionItems = sessionItems;
            _staticObjects = staticObjects;
            _timeout = timeout;
        }

        virtual public ISessionStateItemCollection Items { 
            get {
                return _sessionItems;
            }
        }
        
        virtual public HttpStaticObjectsCollection StaticObjects { 
            get {
                return _staticObjects;
            }
        }
        
        virtual public int Timeout { 
            get {
                return _timeout;
            }
            
            set {
                _timeout = value;
            }
        }
    }
}
