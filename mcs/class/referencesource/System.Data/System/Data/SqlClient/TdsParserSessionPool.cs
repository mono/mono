//------------------------------------------------------------------------------
// <copyright file="TdsParserSessionPool.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data.SqlClient {

    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Collections.Concurrent;

    internal class TdsParserSessionPool {
        // NOTE: This is a very simplistic, lightweight pooler.  It wasn't
        //       intended to handle huge number of items, just to keep track
        //       of the session objects to ensure that they're cleaned up in
        //       a timely manner, to avoid holding on to an unacceptible 
        //       amount of server-side resources in the event that consumers
        //       let their data readers be GC'd, instead of explicitly 
        //       closing or disposing of them
        
        private const int MaxInactiveCount = 10; // pick something, preferably small...
        
        private static int              _objectTypeCount; // Bid counter
        private readonly int            _objectID = System.Threading.Interlocked.Increment(ref _objectTypeCount);

        private readonly TdsParser                  _parser;       // parser that owns us
        private readonly List<TdsParserStateObject> _cache;        // collection of all known sessions 
        private int                                 _cachedCount;  // lock-free _cache.Count
        private TdsParserStateObject[]              _freeStateObjects; // collection of all sessions available for reuse
        private int                                 _freeStateObjectCount; // Number of available free sessions
        
        internal TdsParserSessionPool(TdsParser parser) {
            _parser = parser;
            _cache  = new List<TdsParserStateObject>();
            _freeStateObjects = new TdsParserStateObject[MaxInactiveCount];
            _freeStateObjectCount = 0;
            if (Bid.AdvancedOn) {
                Bid.Trace("<sc.TdsParserSessionPool.ctor|ADV> %d# created session pool for parser %d\n", ObjectID, parser.ObjectID);
            }
        }

        private bool IsDisposed {
            get {
                return (null == _freeStateObjects);
            }
        }

        internal int ObjectID {
            get {
                return _objectID;
            }
        }

        internal void Deactivate() {
            // When being deactivated, we check all the sessions in the
            // cache to make sure they're cleaned up and then we dispose of
            // sessions that are past what we want to keep around.

            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<sc.TdsParserSessionPool.Deactivate|ADV> %d# deactivating cachedCount=%d\n", ObjectID, _cachedCount);

            try {
                lock(_cache) {
                    // NOTE: The PutSession call below may choose to remove the 
                    //       session from the cache, which will throw off our 
                    //       enumerator.  We avoid that by simply indexing backward
                    //       through the array.

                    for (int i = _cache.Count - 1; i >= 0 ; i--) {
                        TdsParserStateObject session = _cache[i];
                        
                        if (null != session) {
                            if (session.IsOrphaned) {
                                // 
                                
                                if (Bid.AdvancedOn) {
                                    Bid.Trace("<sc.TdsParserSessionPool.Deactivate|ADV> %d# reclaiming session %d\n", ObjectID, session.ObjectID);
                                }

                                PutSession(session);
                            }
                        }
                    }
                    // 

                }
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        // This is called from a ThreadAbort - ensure that it can be run from a CER Catch
        internal void BestEffortCleanup() {
            for (int i = 0; i < _cache.Count; i++) {
                TdsParserStateObject session = _cache[i];
                if (null != session) {
                    var sessionHandle = session.Handle;
                    if (sessionHandle != null) {
                        sessionHandle.Dispose();
                    }
                }
            }
        }

        internal void Dispose() {
            if (Bid.AdvancedOn) {
                Bid.Trace("<sc.TdsParserSessionPool.Dispose|ADV> %d# disposing cachedCount=%d\n", ObjectID, _cachedCount);
            }

            lock(_cache) {
                // Dispose free sessions
                for (int i = 0; i < _freeStateObjectCount; i++) {
                    if (_freeStateObjects[i] != null) {
                        _freeStateObjects[i].Dispose();
                    }
                }
                _freeStateObjects = null;
                _freeStateObjectCount = 0;

                // Dispose orphaned sessions
                for (int i = 0; i < _cache.Count; i++) {
                    if (_cache[i] != null) {
                        if (_cache[i].IsOrphaned) {
                            _cache[i].Dispose();
                        }
                        else {
                            // Remove the "initial" callback (this will allow the stateObj to be GC collected if need be)
                            _cache[i].DecrementPendingCallbacks(false);
                        }
                    }
                }
                _cache.Clear();
                _cachedCount = 0;

                // Any active sessions will take care of themselves
                // (It's too dangerous to dispose them, as this can cause AVs)
            }
        }

        internal TdsParserStateObject GetSession(object owner) {
            TdsParserStateObject session;
            lock (_cache) {
                if (IsDisposed) {
                    throw ADP.ClosedConnectionError();
                }
                else if (_freeStateObjectCount > 0) {
                    // Free state object - grab it
                    _freeStateObjectCount--;
                    session = _freeStateObjects[_freeStateObjectCount];
                    _freeStateObjects[_freeStateObjectCount] = null;
                    Debug.Assert(session != null, "There was a null session in the free session list?");
                }
                else {
                    // No free objects, create a new one
                    session = _parser.CreateSession();

                    if (Bid.AdvancedOn) {
                        Bid.Trace("<sc.TdsParserSessionPool.CreateSession|ADV> %d# adding session %d to pool\n", ObjectID, session.ObjectID);
                    }

                    _cache.Add(session);
                    _cachedCount = _cache.Count;
                }

                session.Activate(owner);
            }

            if (Bid.AdvancedOn) {
                Bid.Trace("<sc.TdsParserSessionPool.GetSession|ADV> %d# using session %d\n", ObjectID, session.ObjectID);
            }

            return session;
        }

        internal void PutSession(TdsParserStateObject session) {
            Debug.Assert (null != session, "null session?");
            //Debug.Assert(null != session.Owner, "session without owner?");

            bool okToReuse = session.Deactivate();

            lock (_cache) {
                if (IsDisposed) {
                    // We're diposed - just clean out the session
                    Debug.Assert(_cachedCount == 0, "SessionPool is disposed, but there are still sessions in the cache?");
                    session.Dispose();
                }
                else if ((okToReuse) && (_freeStateObjectCount < MaxInactiveCount)) {
                    // Session is good to re-use and our cache has space
                    if (Bid.AdvancedOn) {
                        Bid.Trace("<sc.TdsParserSessionPool.PutSession|ADV> %d# keeping session %d cachedCount=%d\n", ObjectID, session.ObjectID, _cachedCount);
                    }
                    Debug.Assert(!session._pendingData, "pending data on a pooled session?");

                    _freeStateObjects[_freeStateObjectCount] = session;
                    _freeStateObjectCount++;
                }
                else {
                    // Either the session is bad, or we have no cache space - so dispose the session and remove it
                    if (Bid.AdvancedOn) {
                        Bid.Trace("<sc.TdsParserSessionPool.PutSession|ADV> %d# disposing session %d cachedCount=%d\n", ObjectID, session.ObjectID, _cachedCount);
                    }
            
                    bool removed = _cache.Remove(session);
                    Debug.Assert(removed, "session not in pool?");
                    _cachedCount = _cache.Count;
                    session.Dispose();
                }

                session.RemoveOwner();
            }
        }

        internal string TraceString() {
            return String.Format(/*IFormatProvider*/ null,
                        "(ObjID={0}, free={1}, cached={2}, total={3})",
                        _objectID,
                        null == _freeStateObjects ? "(null)" : _freeStateObjectCount.ToString((IFormatProvider) null),
                        _cachedCount,
                        _cache.Count);
        }

        internal int ActiveSessionsCount {
            get {
                return _cachedCount - _freeStateObjectCount;
            }
        }
    }
}


