namespace System.Diagnostics.Eventing.Reader
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime;
    using System.Security;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class ProviderMetadata : IDisposable
    {
        private IList<EventLogLink> channelReferences;
        private CultureInfo cultureInfo;
        private EventLogHandle defaultProviderHandle;
        private EventLogHandle handle;
        private IList<EventKeyword> keywords;
        private IList<EventLevel> levels;
        private string logFilePath;
        private IList<EventOpcode> opcodes;
        private string providerName;
        private EventLogSession session;
        private IList<EventKeyword> standardKeywords;
        private IList<EventLevel> standardLevels;
        private IList<EventOpcode> standardOpcodes;
        private IList<EventTask> standardTasks;
        private object syncObject;
        private IList<EventTask> tasks;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ProviderMetadata(string providerName) : this(providerName, null, null, null)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ProviderMetadata(string providerName, EventLogSession session, CultureInfo targetCultureInfo) : this(providerName, session, targetCultureInfo, null)
        {
        }

        [SecuritySafeCritical]
        internal ProviderMetadata(string providerName, EventLogSession session, CultureInfo targetCultureInfo, string logFilePath)
        {
            this.handle = EventLogHandle.Zero;
            this.defaultProviderHandle = EventLogHandle.Zero;
            EventLogPermissionHolder.GetEventLogPermission().Demand();
            if (targetCultureInfo == null)
            {
                targetCultureInfo = CultureInfo.CurrentCulture;
            }
            if (session == null)
            {
                session = EventLogSession.GlobalSession;
            }
            this.session = session;
            this.providerName = providerName;
            this.cultureInfo = targetCultureInfo;
            this.logFilePath = logFilePath;
            this.handle = NativeWrapper.EvtOpenProviderMetadata(this.session.Handle, this.providerName, this.logFilePath, this.cultureInfo.LCID, 0);
            this.syncObject = new object();
        }

        internal void CheckReleased()
        {
            lock (this.syncObject)
            {
                this.GetProviderListProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId.EvtPublisherMetadataTasks);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        [SecuritySafeCritical]
        protected virtual void Dispose(bool disposing)
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

        internal string FindStandardKeywordDisplayName(string name, long value)
        {
            if (this.standardKeywords == null)
            {
                this.standardKeywords = (List<EventKeyword>) this.GetProviderListProperty(this.defaultProviderHandle, Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId.EvtPublisherMetadataKeywords);
            }
            foreach (EventKeyword keyword in this.standardKeywords)
            {
                if ((keyword.Name == name) && (keyword.Value == value))
                {
                    return keyword.DisplayName;
                }
            }
            return null;
        }

        internal string FindStandardLevelDisplayName(string name, uint value)
        {
            if (this.standardLevels == null)
            {
                this.standardLevels = (List<EventLevel>) this.GetProviderListProperty(this.defaultProviderHandle, Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId.EvtPublisherMetadataLevels);
            }
            foreach (EventLevel level in this.standardLevels)
            {
                if ((level.Name == name) && (level.Value == value))
                {
                    return level.DisplayName;
                }
            }
            return null;
        }

        internal string FindStandardOpcodeDisplayName(string name, uint value)
        {
            if (this.standardOpcodes == null)
            {
                this.standardOpcodes = (List<EventOpcode>) this.GetProviderListProperty(this.defaultProviderHandle, Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId.EvtPublisherMetadataOpcodes);
            }
            foreach (EventOpcode opcode in this.standardOpcodes)
            {
                if ((opcode.Name == name) && (opcode.Value == value))
                {
                    return opcode.DisplayName;
                }
            }
            return null;
        }

        internal string FindStandardTaskDisplayName(string name, uint value)
        {
            if (this.standardTasks == null)
            {
                this.standardTasks = (List<EventTask>) this.GetProviderListProperty(this.defaultProviderHandle, Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId.EvtPublisherMetadataTasks);
            }
            foreach (EventTask task in this.standardTasks)
            {
                if ((task.Name == name) && (task.Value == value))
                {
                    return task.DisplayName;
                }
            }
            return null;
        }

        [SecuritySafeCritical]
        internal object GetProviderListProperty(EventLogHandle providerHandle, Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId metadataProperty)
        {
            object obj2;
            EventLogHandle zero = EventLogHandle.Zero;
            EventLogPermissionHolder.GetEventLogPermission().Demand();
            try
            {
                Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId evtPublisherMetadataOpcodeName;
                Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId evtPublisherMetadataOpcodeValue;
                Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId evtPublisherMetadataOpcodeMessageID;
                ObjectTypeName opcode;
                List<EventLevel> list = null;
                List<EventOpcode> list2 = null;
                List<EventKeyword> list3 = null;
                List<EventTask> list4 = null;
                zero = NativeWrapper.EvtGetPublisherMetadataPropertyHandle(providerHandle, metadataProperty);
                int capacity = NativeWrapper.EvtGetObjectArraySize(zero);
                switch (metadataProperty)
                {
                    case Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId.EvtPublisherMetadataOpcodes:
                        evtPublisherMetadataOpcodeName = Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId.EvtPublisherMetadataOpcodeName;
                        evtPublisherMetadataOpcodeValue = Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId.EvtPublisherMetadataOpcodeValue;
                        evtPublisherMetadataOpcodeMessageID = Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId.EvtPublisherMetadataOpcodeMessageID;
                        opcode = ObjectTypeName.Opcode;
                        list2 = new List<EventOpcode>(capacity);
                        break;

                    case Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId.EvtPublisherMetadataKeywords:
                        evtPublisherMetadataOpcodeName = Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId.EvtPublisherMetadataKeywordName;
                        evtPublisherMetadataOpcodeValue = Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId.EvtPublisherMetadataKeywordValue;
                        evtPublisherMetadataOpcodeMessageID = Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId.EvtPublisherMetadataKeywordMessageID;
                        opcode = ObjectTypeName.Keyword;
                        list3 = new List<EventKeyword>(capacity);
                        break;

                    case Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId.EvtPublisherMetadataLevels:
                        evtPublisherMetadataOpcodeName = Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId.EvtPublisherMetadataLevelName;
                        evtPublisherMetadataOpcodeValue = Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId.EvtPublisherMetadataLevelValue;
                        evtPublisherMetadataOpcodeMessageID = Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId.EvtPublisherMetadataLevelMessageID;
                        opcode = ObjectTypeName.Level;
                        list = new List<EventLevel>(capacity);
                        break;

                    case Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId.EvtPublisherMetadataTasks:
                        evtPublisherMetadataOpcodeName = Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId.EvtPublisherMetadataTaskName;
                        evtPublisherMetadataOpcodeValue = Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId.EvtPublisherMetadataTaskValue;
                        evtPublisherMetadataOpcodeMessageID = Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId.EvtPublisherMetadataTaskMessageID;
                        opcode = ObjectTypeName.Task;
                        list4 = new List<EventTask>(capacity);
                        break;

                    default:
                        return null;
                }
                for (int i = 0; i < capacity; i++)
                {
                    string name = (string) NativeWrapper.EvtGetObjectArrayProperty(zero, i, (int) evtPublisherMetadataOpcodeName);
                    uint num3 = 0;
                    long num4 = 0L;
                    if (opcode != ObjectTypeName.Keyword)
                    {
                        num3 = (uint) NativeWrapper.EvtGetObjectArrayProperty(zero, i, (int) evtPublisherMetadataOpcodeValue);
                    }
                    else
                    {
                        num4 = (long) ((ulong) NativeWrapper.EvtGetObjectArrayProperty(zero, i, (int) evtPublisherMetadataOpcodeValue));
                    }
                    int num5 = (int) ((uint) NativeWrapper.EvtGetObjectArrayProperty(zero, i, (int) evtPublisherMetadataOpcodeMessageID));
                    string displayName = null;
                    if (num5 == -1)
                    {
                        if (providerHandle != this.defaultProviderHandle)
                        {
                            if (this.defaultProviderHandle.IsInvalid)
                            {
                                this.defaultProviderHandle = NativeWrapper.EvtOpenProviderMetadata(this.session.Handle, null, null, this.cultureInfo.LCID, 0);
                            }
                            switch (opcode)
                            {
                                case ObjectTypeName.Level:
                                    displayName = this.FindStandardLevelDisplayName(name, num3);
                                    goto Label_01BA;

                                case ObjectTypeName.Opcode:
                                    displayName = this.FindStandardOpcodeDisplayName(name, num3 >> 0x10);
                                    goto Label_01BA;

                                case ObjectTypeName.Task:
                                    displayName = this.FindStandardTaskDisplayName(name, num3);
                                    goto Label_01BA;

                                case ObjectTypeName.Keyword:
                                    displayName = this.FindStandardKeywordDisplayName(name, num4);
                                    goto Label_01BA;
                            }
                            displayName = null;
                        }
                    }
                    else
                    {
                        displayName = NativeWrapper.EvtFormatMessage(providerHandle, (uint) num5);
                    }
                Label_01BA:
                    switch (opcode)
                    {
                        case ObjectTypeName.Level:
                            list.Add(new EventLevel(name, (int) num3, displayName));
                            break;

                        case ObjectTypeName.Opcode:
                            list2.Add(new EventOpcode(name, (int) (num3 >> 0x10), displayName));
                            break;

                        case ObjectTypeName.Task:
                        {
                            Guid guid = (Guid) NativeWrapper.EvtGetObjectArrayProperty(zero, i, 0x12);
                            list4.Add(new EventTask(name, (int) num3, displayName, guid));
                            break;
                        }
                        case ObjectTypeName.Keyword:
                            list3.Add(new EventKeyword(name, num4, displayName));
                            break;

                        default:
                            return null;
                    }
                }
                switch (opcode)
                {
                    case ObjectTypeName.Level:
                        return list;

                    case ObjectTypeName.Opcode:
                        return list2;

                    case ObjectTypeName.Task:
                        return list4;

                    case ObjectTypeName.Keyword:
                        return list3;
                }
                obj2 = null;
            }
            finally
            {
                zero.Close();
            }
            return obj2;
        }

        public string DisplayName
        {
            [SecurityCritical]
            get
            {
                uint providerMessageID = this.ProviderMessageID;
                if (providerMessageID == uint.MaxValue)
                {
                    return null;
                }
                EventLogPermissionHolder.GetEventLogPermission().Demand();
                return NativeWrapper.EvtFormatMessage(this.handle, providerMessageID);
            }
        }

        public IEnumerable<EventMetadata> Events
        {
            [SecurityCritical]
            get
            {
                EventLogPermissionHolder.GetEventLogPermission().Demand();
                List<EventMetadata> list = new List<EventMetadata>();
                EventLogHandle eventMetadataEnum = NativeWrapper.EvtOpenEventMetadataEnum(this.handle, 0);
                using (eventMetadataEnum)
                {
                    EventLogHandle handle2;
                Label_0020:
                    handle2 = NativeWrapper.EvtNextEventMetadata(eventMetadataEnum, 0);
                    if (handle2 != null)
                    {
                        using (handle2)
                        {
                            string str2;
                            uint id = (uint) NativeWrapper.EvtGetEventMetadataProperty(handle2, Microsoft.Win32.UnsafeNativeMethods.EvtEventMetadataPropertyId.EventMetadataEventID);
                            byte version = (byte) ((uint) NativeWrapper.EvtGetEventMetadataProperty(handle2, Microsoft.Win32.UnsafeNativeMethods.EvtEventMetadataPropertyId.EventMetadataEventVersion));
                            byte channelId = (byte) ((uint) NativeWrapper.EvtGetEventMetadataProperty(handle2, Microsoft.Win32.UnsafeNativeMethods.EvtEventMetadataPropertyId.EventMetadataEventChannel));
                            byte level = (byte) ((uint) NativeWrapper.EvtGetEventMetadataProperty(handle2, Microsoft.Win32.UnsafeNativeMethods.EvtEventMetadataPropertyId.EventMetadataEventLevel));
                            byte opcode = (byte) ((uint) NativeWrapper.EvtGetEventMetadataProperty(handle2, Microsoft.Win32.UnsafeNativeMethods.EvtEventMetadataPropertyId.EventMetadataEventOpcode));
                            short task = (short) ((uint) NativeWrapper.EvtGetEventMetadataProperty(handle2, Microsoft.Win32.UnsafeNativeMethods.EvtEventMetadataPropertyId.EventMetadataEventTask));
                            long keywords = (long) ((ulong) NativeWrapper.EvtGetEventMetadataProperty(handle2, Microsoft.Win32.UnsafeNativeMethods.EvtEventMetadataPropertyId.EventMetadataEventKeyword));
                            string template = (string) NativeWrapper.EvtGetEventMetadataProperty(handle2, Microsoft.Win32.UnsafeNativeMethods.EvtEventMetadataPropertyId.EventMetadataEventTemplate);
                            int num8 = (int) ((uint) NativeWrapper.EvtGetEventMetadataProperty(handle2, Microsoft.Win32.UnsafeNativeMethods.EvtEventMetadataPropertyId.EventMetadataEventMessageID));
                            if (num8 == -1)
                            {
                                str2 = null;
                            }
                            else
                            {
                                str2 = NativeWrapper.EvtFormatMessage(this.handle, (uint) num8);
                            }
                            EventMetadata item = new EventMetadata(id, version, channelId, level, opcode, task, keywords, template, str2, this);
                            list.Add(item);
                            goto Label_0020;
                        }
                    }
                    return list.AsReadOnly();
                }
            }
        }

        internal EventLogHandle Handle
        {
            get
            {
                return this.handle;
            }
        }

        public Uri HelpLink
        {
            get
            {
                string uriString = (string) NativeWrapper.EvtGetPublisherMetadataProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId.EvtPublisherMetadataHelpLink);
                if ((uriString != null) && (uriString.Length != 0))
                {
                    return new Uri(uriString);
                }
                return null;
            }
        }

        public Guid Id
        {
            get
            {
                return (Guid) NativeWrapper.EvtGetPublisherMetadataProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId.EvtPublisherMetadataPublisherGuid);
            }
        }

        public IList<EventKeyword> Keywords
        {
            get
            {
                lock (this.syncObject)
                {
                    if (this.keywords != null)
                    {
                        return this.keywords;
                    }
                    this.keywords = ((List<EventKeyword>) this.GetProviderListProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId.EvtPublisherMetadataKeywords)).AsReadOnly();
                }
                return this.keywords;
            }
        }

        public IList<EventLevel> Levels
        {
            get
            {
                lock (this.syncObject)
                {
                    if (this.levels != null)
                    {
                        return this.levels;
                    }
                    this.levels = ((List<EventLevel>) this.GetProviderListProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId.EvtPublisherMetadataLevels)).AsReadOnly();
                }
                return this.levels;
            }
        }

        public IList<EventLogLink> LogLinks
        {
            [SecurityCritical]
            get
            {
                IList<EventLogLink> channelReferences;
                EventLogHandle zero = EventLogHandle.Zero;
                try
                {
                    lock (this.syncObject)
                    {
                        if (this.channelReferences != null)
                        {
                            return this.channelReferences;
                        }
                        EventLogPermissionHolder.GetEventLogPermission().Demand();
                        zero = NativeWrapper.EvtGetPublisherMetadataPropertyHandle(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId.EvtPublisherMetadataChannelReferences);
                        int capacity = NativeWrapper.EvtGetObjectArraySize(zero);
                        List<EventLogLink> list = new List<EventLogLink>(capacity);
                        for (int i = 0; i < capacity; i++)
                        {
                            bool flag;
                            string str2;
                            string strA = (string) NativeWrapper.EvtGetObjectArrayProperty(zero, i, 7);
                            uint channelId = (uint) NativeWrapper.EvtGetObjectArrayProperty(zero, i, 9);
                            uint num4 = (uint) NativeWrapper.EvtGetObjectArrayProperty(zero, i, 10);
                            if (num4 == 1)
                            {
                                flag = true;
                            }
                            else
                            {
                                flag = false;
                            }
                            int num5 = (int) ((uint) NativeWrapper.EvtGetObjectArrayProperty(zero, i, 11));
                            if (num5 == -1)
                            {
                                str2 = null;
                            }
                            else
                            {
                                str2 = NativeWrapper.EvtFormatMessage(this.handle, (uint) num5);
                            }
                            if ((str2 == null) && flag)
                            {
                                if (string.Compare(strA, "Application", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    num5 = 0x100;
                                }
                                else if (string.Compare(strA, "System", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    num5 = 0x102;
                                }
                                else if (string.Compare(strA, "Security", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    num5 = 0x101;
                                }
                                else
                                {
                                    num5 = -1;
                                }
                                if (num5 != -1)
                                {
                                    if (this.defaultProviderHandle.IsInvalid)
                                    {
                                        this.defaultProviderHandle = NativeWrapper.EvtOpenProviderMetadata(this.session.Handle, null, null, this.cultureInfo.LCID, 0);
                                    }
                                    str2 = NativeWrapper.EvtFormatMessage(this.defaultProviderHandle, (uint) num5);
                                }
                            }
                            list.Add(new EventLogLink(strA, flag, str2, channelId));
                        }
                        this.channelReferences = list.AsReadOnly();
                    }
                    channelReferences = this.channelReferences;
                }
                finally
                {
                    zero.Close();
                }
                return channelReferences;
            }
        }

        public string MessageFilePath
        {
            get
            {
                return (string) NativeWrapper.EvtGetPublisherMetadataProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId.EvtPublisherMetadataMessageFilePath);
            }
        }

        public string Name
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.providerName;
            }
        }

        public IList<EventOpcode> Opcodes
        {
            get
            {
                lock (this.syncObject)
                {
                    if (this.opcodes != null)
                    {
                        return this.opcodes;
                    }
                    this.opcodes = ((List<EventOpcode>) this.GetProviderListProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId.EvtPublisherMetadataOpcodes)).AsReadOnly();
                }
                return this.opcodes;
            }
        }

        public string ParameterFilePath
        {
            get
            {
                return (string) NativeWrapper.EvtGetPublisherMetadataProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId.EvtPublisherMetadataParameterFilePath);
            }
        }

        private uint ProviderMessageID
        {
            get
            {
                return (uint) NativeWrapper.EvtGetPublisherMetadataProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId.EvtPublisherMetadataPublisherMessageID);
            }
        }

        public string ResourceFilePath
        {
            get
            {
                return (string) NativeWrapper.EvtGetPublisherMetadataProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId.EvtPublisherMetadataResourceFilePath);
            }
        }

        public IList<EventTask> Tasks
        {
            get
            {
                lock (this.syncObject)
                {
                    if (this.tasks != null)
                    {
                        return this.tasks;
                    }
                    this.tasks = ((List<EventTask>) this.GetProviderListProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId.EvtPublisherMetadataTasks)).AsReadOnly();
                }
                return this.tasks;
            }
        }

        internal enum ObjectTypeName
        {
            Level,
            Opcode,
            Task,
            Keyword
        }
    }
}

