//------------------------------------------------------------------------------
// <copyright file="PartitionResolver.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web {

    using System;
    using System.Configuration;
    using System.Collections;
    using System.Threading;
    using System.IO;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Util;
    using System.Data;
    using System.Data.SqlClient;
    using System.Text;
    using System.Security.Principal;
    using System.Xml;
    using System.Collections.Specialized; 
    using System.Configuration.Provider;
    using System.Globalization;
    using System.Web.Management;
    using System.Web.Hosting;
    using System.Web.Configuration;
    using System.Security.Permissions;

    public interface IPartitionResolver {
        void Initialize();
        string ResolvePartition(Object key);
    }

    internal interface IPartitionInfo {
        string GetTracingPartitionString();
    }

    internal delegate IPartitionInfo CreatePartitionInfo(string connectionString);
    
    class PartitionManager  : IDisposable {
        internal PartitionManager(CreatePartitionInfo createCallback) {
            _createCallback = createCallback;
        }
    
        HybridDictionary    _partitions = new HybridDictionary();
        ReaderWriterLock    _lock = new ReaderWriterLock();
        CreatePartitionInfo _createCallback;

        internal object GetPartition(IPartitionResolver partitionResolver, string id) {
            if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Infrastructure))
                EtwTrace.Trace(EtwTraceType.ETW_TYPE_SESSIONSTATE_PARTITION_START, HttpContext.Current.WorkerRequest, partitionResolver.GetType().FullName, id);
            
            string partitionString = null;
            string errorMessage = null;
            IPartitionInfo partitionInfo = null;
            try {
                try {
                    partitionString = partitionResolver.ResolvePartition(id);

                    if (partitionString == null) {
                        throw new HttpException(
                                SR.GetString(SR.Bad_partition_resolver_connection_string, partitionResolver.GetType().FullName));
                    }
                }
                catch (Exception e) {
                    errorMessage = e.Message;
                    throw;
                }

                try {
                    _lock.AcquireReaderLock(-1);
                    partitionInfo = (IPartitionInfo)_partitions[partitionString];
                    if (partitionInfo != null) {
                        Debug.Trace("PartitionManager", "id=" + id + "; partitionString=" + partitionString);
                        return partitionInfo;
                    }

                }
                finally {
                    if (_lock.IsReaderLockHeld) {
                        _lock.ReleaseReaderLock();
                    }
                }

                // Not found.  Has to add it.
                try {
                    _lock.AcquireWriterLock(-1);
                    // One more time
                    partitionInfo = (IPartitionInfo)_partitions[partitionString];
                    if (partitionInfo == null) {
                        partitionInfo = _createCallback(partitionString);

                        Debug.Trace("PartitionManager", "Add a new partition; id=" + id + "; partitionString=" + partitionString);
                        _partitions.Add(partitionString, partitionInfo);
                    }

                    Debug.Trace("PartitionManager", "id=" + id + "; partitionString=" + partitionString);
                    return partitionInfo;
                }
                finally {
                    if (_lock.IsWriterLockHeld) {
                        _lock.ReleaseWriterLock();
                    }
                }
            }
            finally {
                if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Infrastructure)) {
                    string msg = errorMessage;
                    if (msg == null) {
                        if (partitionInfo != null) {
                            msg = partitionInfo.GetTracingPartitionString();
                        }
                        else {
                            msg = String.Empty;
                        }
                    }
                    EtwTrace.Trace(EtwTraceType.ETW_TYPE_SESSIONSTATE_PARTITION_END, HttpContext.Current.WorkerRequest, msg);
                }
            }
        }

        public void Dispose() {
            if (_partitions == null) {
                return;
            }
            
            try {
                _lock.AcquireWriterLock(-1);
                if (_partitions != null) {
                    foreach (PartitionInfo partitionInfo in _partitions.Values) {
                        partitionInfo.Dispose();
                    }

                    _partitions = null;
                }
            }
            catch {
                // ignore exceptions in dispose
            }
            finally {
                if (_lock.IsWriterLockHeld) {
                    _lock.ReleaseWriterLock();
                }
            }
        }
    }
    
    internal class PartitionInfo : IDisposable, IPartitionInfo {
        ResourcePool    _rpool;

        internal PartitionInfo(ResourcePool rpool) {
            _rpool = rpool;
        }
        
        internal object RetrieveResource() {
            return _rpool.RetrieveResource();
        }
    
        internal void StoreResource(IDisposable o) {
            _rpool.StoreResource(o);
        }

        protected virtual string TracingPartitionString {
            get {
                return String.Empty;
            }
        }

        string IPartitionInfo.GetTracingPartitionString() {
            return TracingPartitionString;
        }

        public void Dispose() {
            if (_rpool == null) {
                return;
            }
            
            lock (this) {
                if (_rpool != null) {
                    _rpool.Dispose();
                    _rpool = null;
                }
            }
        }
    };

    
}
