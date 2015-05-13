namespace System.Diagnostics.Eventing.Reader
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Text;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class EventLogRecord : EventRecord
    {
        private ProviderMetadataCachedInformation cachedMetadataInformation;
        private string containerChannel;
        [SecuritySafeCritical]
        private EventLogHandle handle;
        private IEnumerable<string> keywordsNames;
        private string levelName;
        private bool levelNameReady;
        private int[] matchedQueryIds;
        private string opcodeName;
        private bool opcodeNameReady;
        private EventLogSession session;
        private object syncObject;
        private const int SYSTEM_PROPERTY_COUNT = 0x12;
        private NativeWrapper.SystemProperties systemProperties;
        private string taskName;
        private bool taskNameReady;

        [SecuritySafeCritical]
        internal EventLogRecord(EventLogHandle handle, EventLogSession session, ProviderMetadataCachedInformation cachedMetadataInfo)
        {
            this.cachedMetadataInformation = cachedMetadataInfo;
            this.handle = handle;
            this.session = session;
            this.systemProperties = new NativeWrapper.SystemProperties();
            this.syncObject = new object();
        }

        [SecuritySafeCritical]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    EventLogPermissionHolder.GetEventLogPermission().Demand();
                }
                if ((this.handle != null) && !this.handle.IsInvalid)
                {
                    this.handle.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override string FormatDescription()
        {
            return this.cachedMetadataInformation.GetFormatDescription(this.ProviderName, this.handle);
        }

        public override string FormatDescription(IEnumerable<object> values)
        {
            if (values == null)
            {
                return this.FormatDescription();
            }
            string[] array = new string[0];
            int index = 0;
            foreach (object obj2 in values)
            {
                if (array.Length == index)
                {
                    Array.Resize<string>(ref array, index + 1);
                }
                array[index] = obj2.ToString();
                index++;
            }
            return this.cachedMetadataInformation.GetFormatDescription(this.ProviderName, this.handle, array);
        }

        [SecurityCritical]
        internal static EventLogHandle GetBookmarkHandleFromBookmark(EventBookmark bookmark)
        {
            if (bookmark == null)
            {
                return EventLogHandle.Zero;
            }
            return NativeWrapper.EvtCreateBookmark(bookmark.BookmarkText);
        }

        public IList<object> GetPropertyValues(EventLogPropertySelector propertySelector)
        {
            if (propertySelector == null)
            {
                throw new ArgumentNullException("propertySelector");
            }
            return NativeWrapper.EvtRenderBufferWithContextUserOrValues(propertySelector.Handle, this.handle);
        }

        internal void PrepareSystemData()
        {
            if (!this.systemProperties.filled)
            {
                this.session.SetupSystemContext();
                lock (this.syncObject)
                {
                    if (!this.systemProperties.filled)
                    {
                        NativeWrapper.EvtRenderBufferWithContextSystem(this.session.renderContextHandleSystem, this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtRenderFlags.EvtRenderEventValues, this.systemProperties, 0x12);
                        this.systemProperties.filled = true;
                    }
                }
            }
        }

        [SecuritySafeCritical]
        public override string ToXml()
        {
            EventLogPermissionHolder.GetEventLogPermission().Demand();
            StringBuilder buffer = new StringBuilder(0x7d0);
            NativeWrapper.EvtRender(EventLogHandle.Zero, this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtRenderFlags.EvtRenderEventXml, buffer);
            return buffer.ToString();
        }

        public override Guid? ActivityId
        {
            get
            {
                this.PrepareSystemData();
                return this.systemProperties.ActivityId;
            }
        }

        public override EventBookmark Bookmark
        {
            [SecuritySafeCritical]
            get
            {
                EventLogPermissionHolder.GetEventLogPermission().Demand();
                EventLogHandle bookmark = NativeWrapper.EvtCreateBookmark(null);
                NativeWrapper.EvtUpdateBookmark(bookmark, this.handle);
                return new EventBookmark(NativeWrapper.EvtRenderBookmark(bookmark));
            }
        }

        public string ContainerLog
        {
            get
            {
                if (this.containerChannel != null)
                {
                    return this.containerChannel;
                }
                lock (this.syncObject)
                {
                    if (this.containerChannel == null)
                    {
                        this.containerChannel = (string) NativeWrapper.EvtGetEventInfo(this.Handle, Microsoft.Win32.UnsafeNativeMethods.EvtEventPropertyId.EvtEventPath);
                    }
                    return this.containerChannel;
                }
            }
        }

        internal EventLogHandle Handle
        {
            [SecuritySafeCritical]
            get
            {
                return this.handle;
            }
        }

        public override int Id
        {
            get
            {
                this.PrepareSystemData();
                short? id = this.systemProperties.Id;
                int? nullable3 = id.HasValue ? new int?(id.GetValueOrDefault()) : null;
                if (!nullable3.HasValue)
                {
                    return 0;
                }
                return this.systemProperties.Id.Value;
            }
        }

        public override long? Keywords
        {
            get
            {
                this.PrepareSystemData();
                long? keywords = this.systemProperties.Keywords;
                if (!keywords.HasValue)
                {
                    return null;
                }
                return new long?(keywords.GetValueOrDefault());
            }
        }

        public override IEnumerable<string> KeywordsDisplayNames
        {
            get
            {
                if (this.keywordsNames != null)
                {
                    return this.keywordsNames;
                }
                lock (this.syncObject)
                {
                    if (this.keywordsNames == null)
                    {
                        this.keywordsNames = this.cachedMetadataInformation.GetKeywordDisplayNames(this.ProviderName, this.handle);
                    }
                    return this.keywordsNames;
                }
            }
        }

        public override byte? Level
        {
            get
            {
                this.PrepareSystemData();
                return this.systemProperties.Level;
            }
        }

        public override string LevelDisplayName
        {
            get
            {
                if (this.levelNameReady)
                {
                    return this.levelName;
                }
                lock (this.syncObject)
                {
                    if (!this.levelNameReady)
                    {
                        this.levelNameReady = true;
                        this.levelName = this.cachedMetadataInformation.GetLevelDisplayName(this.ProviderName, this.handle);
                    }
                    return this.levelName;
                }
            }
        }

        public override string LogName
        {
            get
            {
                this.PrepareSystemData();
                return this.systemProperties.ChannelName;
            }
        }

        public override string MachineName
        {
            get
            {
                this.PrepareSystemData();
                return this.systemProperties.ComputerName;
            }
        }

        public IEnumerable<int> MatchedQueryIds
        {
            get
            {
                if (this.matchedQueryIds != null)
                {
                    return this.matchedQueryIds;
                }
                lock (this.syncObject)
                {
                    if (this.matchedQueryIds == null)
                    {
                        this.matchedQueryIds = (int[]) NativeWrapper.EvtGetEventInfo(this.Handle, Microsoft.Win32.UnsafeNativeMethods.EvtEventPropertyId.EvtEventQueryIDs);
                    }
                    return this.matchedQueryIds;
                }
            }
        }

        public override short? Opcode
        {
            get
            {
                this.PrepareSystemData();
                byte? opcode = this.systemProperties.Opcode;
                ushort? nullable3 = opcode.HasValue ? new ushort?(opcode.GetValueOrDefault()) : null;
                if (!nullable3.HasValue)
                {
                    return null;
                }
                return new short?((short) nullable3.GetValueOrDefault());
            }
        }

        public override string OpcodeDisplayName
        {
            get
            {
                lock (this.syncObject)
                {
                    if (!this.opcodeNameReady)
                    {
                        this.opcodeNameReady = true;
                        this.opcodeName = this.cachedMetadataInformation.GetOpcodeDisplayName(this.ProviderName, this.handle);
                    }
                    return this.opcodeName;
                }
            }
        }

        public override int? ProcessId
        {
            get
            {
                this.PrepareSystemData();
                int? processId = this.systemProperties.ProcessId;
                if (!processId.HasValue)
                {
                    return null;
                }
                return new int?(processId.GetValueOrDefault());
            }
        }

        public override IList<EventProperty> Properties
        {
            get
            {
                this.session.SetupUserContext();
                IList<object> list = NativeWrapper.EvtRenderBufferWithContextUserOrValues(this.session.renderContextHandleUser, this.handle);
                List<EventProperty> list2 = new List<EventProperty>();
                foreach (object obj2 in list)
                {
                    list2.Add(new EventProperty(obj2));
                }
                return list2;
            }
        }

        public override Guid? ProviderId
        {
            get
            {
                this.PrepareSystemData();
                return this.systemProperties.ProviderId;
            }
        }

        public override string ProviderName
        {
            get
            {
                this.PrepareSystemData();
                return this.systemProperties.ProviderName;
            }
        }

        public override int? Qualifiers
        {
            get
            {
                this.PrepareSystemData();
                short? qualifiers = this.systemProperties.Qualifiers;
                int? nullable3 = qualifiers.HasValue ? new int?(qualifiers.GetValueOrDefault()) : null;
                if (!nullable3.HasValue)
                {
                    return null;
                }
                return new int?(nullable3.GetValueOrDefault());
            }
        }

        public override long? RecordId
        {
            get
            {
                this.PrepareSystemData();
                long? recordId = this.systemProperties.RecordId;
                if (!recordId.HasValue)
                {
                    return null;
                }
                return new long?(recordId.GetValueOrDefault());
            }
        }

        public override Guid? RelatedActivityId
        {
            get
            {
                this.PrepareSystemData();
                return this.systemProperties.RelatedActivityId;
            }
        }

        public override int? Task
        {
            get
            {
                this.PrepareSystemData();
                short? task = this.systemProperties.Task;
                int? nullable3 = task.HasValue ? new int?(task.GetValueOrDefault()) : null;
                if (!nullable3.HasValue)
                {
                    return null;
                }
                return new int?(nullable3.GetValueOrDefault());
            }
        }

        public override string TaskDisplayName
        {
            get
            {
                if (this.taskNameReady)
                {
                    return this.taskName;
                }
                lock (this.syncObject)
                {
                    if (!this.taskNameReady)
                    {
                        this.taskNameReady = true;
                        this.taskName = this.cachedMetadataInformation.GetTaskDisplayName(this.ProviderName, this.handle);
                    }
                    return this.taskName;
                }
            }
        }

        public override int? ThreadId
        {
            get
            {
                this.PrepareSystemData();
                int? threadId = this.systemProperties.ThreadId;
                if (!threadId.HasValue)
                {
                    return null;
                }
                return new int?(threadId.GetValueOrDefault());
            }
        }

        public override DateTime? TimeCreated
        {
            get
            {
                this.PrepareSystemData();
                return this.systemProperties.TimeCreated;
            }
        }

        public override SecurityIdentifier UserId
        {
            get
            {
                this.PrepareSystemData();
                return this.systemProperties.UserId;
            }
        }

        public override byte? Version
        {
            get
            {
                this.PrepareSystemData();
                return this.systemProperties.Version;
            }
        }
    }
}

