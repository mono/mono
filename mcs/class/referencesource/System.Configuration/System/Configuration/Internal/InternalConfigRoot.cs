//------------------------------------------------------------------------------
// <copyright file="InternalConfigRoot.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration.Internal {
    using System.Configuration.Internal;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Security;
    using System.Text;
    using System.Xml;
    using System.Threading;

    //
    // InternalConfigRoot holds the root of a configuration hierarchy.
    // It managed creation, removal, and the search for BaseConfigurationRecord's.
    // in a thread-safe manner.
    //
    // The BaseConfigurationRecord hierarchy is protected with the
    // _hierarchyLock. Functions that assume that the lock as been
    // taken begin with the prefix "hl", for example, "hlFindConfigRecord".
    //
    internal sealed class InternalConfigRoot : IInternalConfigRoot {
        IInternalConfigHost         _host;                  // host, need to create records
        ReaderWriterLock            _hierarchyLock;         // lock to protect hierarchy
        BaseConfigurationRecord     _rootConfigRecord;      // root config record, one level above machine.config.
        bool                        _isDesignTime;          // Is the hierarchy for runtime or designtime?
        private Configuration        _CurrentConfiguration      = null;

        public event InternalConfigEventHandler ConfigChanged;
        public event InternalConfigEventHandler ConfigRemoved;

        internal InternalConfigRoot() { }

        internal InternalConfigRoot(Configuration currentConfiguration) {
            _CurrentConfiguration = currentConfiguration;
        }

        void IInternalConfigRoot.Init(IInternalConfigHost host, bool isDesignTime) {
            _host = host;
            _isDesignTime = isDesignTime;
            _hierarchyLock = new ReaderWriterLock();

            // Dummy record to hold _children for root
            if (_isDesignTime) {
                _rootConfigRecord = MgmtConfigurationRecord.Create(this, null, string.Empty, null);
            }
            else {
                _rootConfigRecord = (BaseConfigurationRecord) RuntimeConfigurationRecord.Create(this, null, string.Empty);
            }
        }

        internal IInternalConfigHost Host {
            get {return _host;}
        }

        internal BaseConfigurationRecord RootConfigRecord {
            get {return _rootConfigRecord;}
        }

        bool IInternalConfigRoot.IsDesignTime {
            get {return _isDesignTime;}
        }

        private void AcquireHierarchyLockForRead() {
            // Protect against unexpected recursive entry on this thread.
            // We do this in retail, too, because the results would be very bad if this were to fail,
            // and the testing for this is not easy for all scenarios.
            Debug.Assert(!_hierarchyLock.IsReaderLockHeld, "!_hierarchyLock.IsReaderLockHeld");
            if (_hierarchyLock.IsReaderLockHeld) {
                throw ExceptionUtil.UnexpectedError("System.Configuration.Internal.InternalConfigRoot::AcquireHierarchyLockForRead - reader lock already held by this thread");
            }

            Debug.Assert(!_hierarchyLock.IsWriterLockHeld, "!_hierarchyLock.IsWriterLockHeld");
            if (_hierarchyLock.IsWriterLockHeld) {
                throw ExceptionUtil.UnexpectedError("System.Configuration.Internal.InternalConfigRoot::AcquireHierarchyLockForRead - writer lock already held by this thread");
            }

            _hierarchyLock.AcquireReaderLock(-1);
        }

        private void ReleaseHierarchyLockForRead() {
            Debug.Assert(!_hierarchyLock.IsWriterLockHeld, "!_hierarchyLock.IsWriterLockHeld");

            if (_hierarchyLock.IsReaderLockHeld) {
                _hierarchyLock.ReleaseReaderLock();
            }
        }

        private void AcquireHierarchyLockForWrite() {
            // Protect against unexpected recursive entry on this thread.
            // We do this in retail, too, because the results would be very bad if this were to fail,
            // and the testing for this is not easy for all scenarios.
            if (_hierarchyLock.IsReaderLockHeld) {
                throw ExceptionUtil.UnexpectedError("System.Configuration.Internal.InternalConfigRoot::AcquireHierarchyLockForWrite - reader lock already held by this thread");
            }

            if (_hierarchyLock.IsWriterLockHeld) {
                throw ExceptionUtil.UnexpectedError("System.Configuration.Internal.InternalConfigRoot::AcquireHierarchyLockForWrite - writer lock already held by this thread");
            }

            _hierarchyLock.AcquireWriterLock(-1);
        }

        private void ReleaseHierarchyLockForWrite() {
            Debug.Assert(!_hierarchyLock.IsReaderLockHeld, "!_hierarchyLock.IsReaderLockHeld");

            if (_hierarchyLock.IsWriterLockHeld) {
                _hierarchyLock.ReleaseWriterLock();
            }
        }

        //
        // Find a config record.
        // If found, nextIndex == parts.Length and the resulting record is in currentRecord.
        // If not found, nextIndex is the index of the part of the path not found, and currentRecord
        // is the record that has been found so far (nexIndex - 1).
        //
        private void hlFindConfigRecord(string[] parts, out int nextIndex, out BaseConfigurationRecord currentRecord) {
            currentRecord = _rootConfigRecord;
            nextIndex = 0;
            for (; nextIndex < parts.Length; nextIndex++) {
                BaseConfigurationRecord childRecord = currentRecord.hlGetChild(parts[nextIndex]);
                if (childRecord == null)
                    break;

                currentRecord = childRecord;
            }
        }

        //
        // Get a config section.
        //
        public object GetSection(string section, string configPath) {
            BaseConfigurationRecord configRecord = (BaseConfigurationRecord) GetUniqueConfigRecord(configPath);
            object result = configRecord.GetSection(section);
            return result;
        }

        //
        // Get the nearest ancestor path (including self) which contains unique configuration information.
        //
        public string GetUniqueConfigPath(string configPath) {
            IInternalConfigRecord configRecord = GetUniqueConfigRecord(configPath);
            if (configRecord == null) {
                return null;
            }

            return configRecord.ConfigPath;
        }

        //
        // Get the nearest ancestor record (including self) which contains unique configuration information.
        //
        public IInternalConfigRecord GetUniqueConfigRecord(string configPath) {
            BaseConfigurationRecord configRecord = (BaseConfigurationRecord) GetConfigRecord(configPath);
            while (configRecord.IsEmpty) {
                BaseConfigurationRecord parentConfigRecord = configRecord.Parent;

                // If all config records are empty, return the immediate child of the
                // root placeholder (e.g. machine.config)
                if (parentConfigRecord.IsRootConfig) {
                    break;
                }

                configRecord = parentConfigRecord;
            }

            return configRecord;
        }

        //
        // Get the config record for a path.
        // If the record does not exist, create it if it is needed.
        //
        public IInternalConfigRecord GetConfigRecord(string configPath) {
            if (!ConfigPathUtility.IsValid(configPath)) {
                throw ExceptionUtil.ParameterInvalid("configPath");
            }

            string[] parts = ConfigPathUtility.GetParts(configPath);

            //
            // First search under the reader lock, so that multiple searches
            // can proceed in parallel.
            //
            try {
                int index;
                BaseConfigurationRecord currentRecord;

                AcquireHierarchyLockForRead();

                hlFindConfigRecord(parts, out index, out currentRecord);

                // check if found
                if (index == parts.Length || !currentRecord.hlNeedsChildFor(parts[index])) {
                    return currentRecord;
                }
            }
            finally {
                ReleaseHierarchyLockForRead();
            }

            //
            // Not found, so search again under exclusive writer lock so that
            // we can create the record.
            //
            try {
                int index;
                BaseConfigurationRecord currentRecord;

                AcquireHierarchyLockForWrite();

                hlFindConfigRecord(parts, out index, out currentRecord);

                if (index == parts.Length) {
                    return currentRecord;
                }

                string currentConfigPath = String.Join(BaseConfigurationRecord.ConfigPathSeparatorString, parts, 0, index);

                //
                // create new records
                //

                while (index < parts.Length && currentRecord.hlNeedsChildFor(parts[index])) {
                    string configName = parts[index];
                    currentConfigPath = ConfigPathUtility.Combine(currentConfigPath, configName);
                    BaseConfigurationRecord childRecord;

                    Debug.Trace("ConfigurationCreate", "Creating config record for " + currentConfigPath);
                    if (_isDesignTime) {
                        childRecord = MgmtConfigurationRecord.Create(this, currentRecord, currentConfigPath, null);
                    }
                    else {
                        childRecord = (BaseConfigurationRecord) RuntimeConfigurationRecord.Create(this, currentRecord, currentConfigPath);
                    }

                    currentRecord.hlAddChild(configName, childRecord);

                    index++;
                    currentRecord = childRecord;
                }

                return currentRecord;
            }
            finally {
                ReleaseHierarchyLockForWrite();
            }
        }

        //
        // Find and remove the config record and all its children for the config path.
        // Optionally ensure the config record matches a desired config record.
        //
        void RemoveConfigImpl(string configPath, BaseConfigurationRecord configRecord) {
            if (!ConfigPathUtility.IsValid(configPath)) {
                throw ExceptionUtil.ParameterInvalid("configPath");
            }

            string[] parts = ConfigPathUtility.GetParts(configPath);

            BaseConfigurationRecord currentRecord;

            // search under exclusive writer lock
            try {
                int index;

                AcquireHierarchyLockForWrite();

                hlFindConfigRecord(parts, out index, out currentRecord);

                // Return if not found, or does not match the one we are trying to remove.
                if (index != parts.Length || (configRecord != null && !Object.ReferenceEquals(configRecord, currentRecord)))
                    return;

                // Remove it from the hierarchy.
                currentRecord.Parent.hlRemoveChild(parts[parts.Length - 1]);
            }
            finally {
                ReleaseHierarchyLockForWrite();
            }

            OnConfigRemoved(new InternalConfigEventArgs(configPath));

            // Close the record. This is safe to do outside the lock.
            currentRecord.CloseRecursive();
        }

        //
        // Find and remove the config record and all its children for the config path.
        //
        public void RemoveConfig(string configPath) {
            RemoveConfigImpl(configPath, null);
        }

        //
        // Remove the config record and all its children for the config path.
        //
        public void RemoveConfigRecord(BaseConfigurationRecord configRecord) {
            RemoveConfigImpl(configRecord.ConfigPath, configRecord);
        }


        //
        // Clear the result of a configSection evaluation at a particular point
        // in the hierarchy.
        //
        public void ClearResult(BaseConfigurationRecord configRecord, string configKey, bool forceEvaluation) {
            string[] parts = ConfigPathUtility.GetParts(configRecord.ConfigPath);

            try {
                int index;
                BaseConfigurationRecord currentRecord;

                AcquireHierarchyLockForRead();

                hlFindConfigRecord(parts, out index, out currentRecord);

                // clear result only if configRecord it is still in the hierarchy
                if (index == parts.Length && Object.ReferenceEquals(configRecord, currentRecord)) {
                    currentRecord.hlClearResultRecursive(configKey, forceEvaluation);
                }
            }
            finally {
                ReleaseHierarchyLockForRead();
            }
        }

        // Fire the ConfigRemoved event.
        private void OnConfigRemoved(InternalConfigEventArgs e) {
            InternalConfigEventHandler handler = ConfigRemoved;
            if (handler != null) {
                handler(this, e);
            }
        }

        // Fire the ConfigChanged event for a configPath.
        internal void FireConfigChanged(string configPath) {
            OnConfigChanged(new InternalConfigEventArgs(configPath));
        }

        // Fire the ConfigChanged event.
        private void OnConfigChanged(InternalConfigEventArgs e) {
            InternalConfigEventHandler handler = ConfigChanged;
            if (handler != null) {
                handler(this, e);
            }
        }

        internal Configuration CurrentConfiguration {
            get {
                return _CurrentConfiguration;
            }
        }
    }
}
