namespace System.Diagnostics.Eventing.Reader
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Security;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class EventLogConfiguration : IDisposable
    {
        private string channelName;
        private EventLogHandle handle;
        private EventLogSession session;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public EventLogConfiguration(string logName) : this(logName, null)
        {
        }

        [SecurityCritical]
        public EventLogConfiguration(string logName, EventLogSession session)
        {
            this.handle = EventLogHandle.Zero;
            EventLogPermissionHolder.GetEventLogPermission().Demand();
            if (session == null)
            {
                session = EventLogSession.GlobalSession;
            }
            this.session = session;
            this.channelName = logName;
            this.handle = NativeWrapper.EvtOpenChannelConfig(this.session.Handle, this.channelName, 0);
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

        public void SaveChanges()
        {
            NativeWrapper.EvtSaveChannelConfig(this.handle, 0);
        }

        public bool IsClassicLog
        {
            get
            {
                return (bool) NativeWrapper.EvtGetChannelConfigProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelConfigClassicEventlog);
            }
        }

        public bool IsEnabled
        {
            get
            {
                return (bool) NativeWrapper.EvtGetChannelConfigProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelConfigEnabled);
            }
            set
            {
                NativeWrapper.EvtSetChannelConfigProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelConfigEnabled, value);
            }
        }

        public string LogFilePath
        {
            get
            {
                return (string) NativeWrapper.EvtGetChannelConfigProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelLoggingConfigLogFilePath);
            }
            set
            {
                NativeWrapper.EvtSetChannelConfigProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelLoggingConfigLogFilePath, value);
            }
        }

        public EventLogIsolation LogIsolation
        {
            get
            {
                return (EventLogIsolation) ((uint) NativeWrapper.EvtGetChannelConfigProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelConfigIsolation));
            }
        }

        public EventLogMode LogMode
        {
            get
            {
                object obj2 = NativeWrapper.EvtGetChannelConfigProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelLoggingConfigRetention);
                object obj3 = NativeWrapper.EvtGetChannelConfigProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelLoggingConfigAutoBackup);
                bool flag = (obj2 != null) && ((bool) obj2);
                if ((obj3 != null) && ((bool) obj3))
                {
                    return EventLogMode.AutoBackup;
                }
                if (flag)
                {
                    return EventLogMode.Retain;
                }
                return EventLogMode.Circular;
            }
            set
            {
                switch (value)
                {
                    case EventLogMode.Circular:
                        NativeWrapper.EvtSetChannelConfigProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelLoggingConfigAutoBackup, false);
                        NativeWrapper.EvtSetChannelConfigProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelLoggingConfigRetention, false);
                        return;

                    case EventLogMode.AutoBackup:
                        NativeWrapper.EvtSetChannelConfigProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelLoggingConfigAutoBackup, true);
                        NativeWrapper.EvtSetChannelConfigProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelLoggingConfigRetention, true);
                        return;

                    case EventLogMode.Retain:
                        NativeWrapper.EvtSetChannelConfigProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelLoggingConfigAutoBackup, false);
                        NativeWrapper.EvtSetChannelConfigProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelLoggingConfigRetention, true);
                        return;
                }
            }
        }

        public string LogName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.channelName;
            }
        }

        public EventLogType LogType
        {
            get
            {
                return (EventLogType) ((uint) NativeWrapper.EvtGetChannelConfigProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelConfigType));
            }
        }

        public long MaximumSizeInBytes
        {
            get
            {
                return (long) ((ulong) NativeWrapper.EvtGetChannelConfigProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelLoggingConfigMaxSize));
            }
            set
            {
                NativeWrapper.EvtSetChannelConfigProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelLoggingConfigMaxSize, value);
            }
        }

        public string OwningProviderName
        {
            get
            {
                return (string) NativeWrapper.EvtGetChannelConfigProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelConfigOwningPublisher);
            }
        }

        public int? ProviderBufferSize
        {
            get
            {
                int? nullable = (int?) NativeWrapper.EvtGetChannelConfigProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelPublishingConfigBufferSize);
                if (!nullable.HasValue)
                {
                    return null;
                }
                return new int?(nullable.GetValueOrDefault());
            }
        }

        public Guid? ProviderControlGuid
        {
            get
            {
                return (Guid?) NativeWrapper.EvtGetChannelConfigProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelPublishingConfigControlGuid);
            }
        }

        public long? ProviderKeywords
        {
            get
            {
                long? nullable = (long?) NativeWrapper.EvtGetChannelConfigProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelPublishingConfigKeywords);
                if (!nullable.HasValue)
                {
                    return null;
                }
                return new long?(nullable.GetValueOrDefault());
            }
            set
            {
                NativeWrapper.EvtSetChannelConfigProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelPublishingConfigKeywords, value);
            }
        }

        public int? ProviderLatency
        {
            get
            {
                int? nullable = (int?) NativeWrapper.EvtGetChannelConfigProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelPublishingConfigLatency);
                if (!nullable.HasValue)
                {
                    return null;
                }
                return new int?(nullable.GetValueOrDefault());
            }
        }

        public int? ProviderLevel
        {
            get
            {
                int? nullable = (int?) NativeWrapper.EvtGetChannelConfigProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelPublishingConfigLevel);
                if (!nullable.HasValue)
                {
                    return null;
                }
                return new int?(nullable.GetValueOrDefault());
            }
            set
            {
                NativeWrapper.EvtSetChannelConfigProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelPublishingConfigLevel, value);
            }
        }

        public int? ProviderMaximumNumberOfBuffers
        {
            get
            {
                int? nullable = (int?) NativeWrapper.EvtGetChannelConfigProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelPublishingConfigMaxBuffers);
                if (!nullable.HasValue)
                {
                    return null;
                }
                return new int?(nullable.GetValueOrDefault());
            }
        }

        public int? ProviderMinimumNumberOfBuffers
        {
            get
            {
                int? nullable = (int?) NativeWrapper.EvtGetChannelConfigProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelPublishingConfigMinBuffers);
                if (!nullable.HasValue)
                {
                    return null;
                }
                return new int?(nullable.GetValueOrDefault());
            }
        }

        public IEnumerable<string> ProviderNames
        {
            get
            {
                return (string[]) NativeWrapper.EvtGetChannelConfigProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelPublisherList);
            }
        }

        public string SecurityDescriptor
        {
            get
            {
                return (string) NativeWrapper.EvtGetChannelConfigProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelConfigAccess);
            }
            set
            {
                NativeWrapper.EvtSetChannelConfigProperty(this.handle, Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelConfigAccess, value);
            }
        }
    }
}

