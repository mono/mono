//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Administration
{
    using System;
    using System.Diagnostics;
    using System.Runtime;

    sealed class AppDomainInfo
    {
        static object syncRoot = new object();
        
        // Double-checked locking pattern requires volatile for read/write synchronization
        static volatile AppDomainInfo singleton;
        Guid instanceId;
        string friendlyName;
        bool isDefaultAppDomain;
        string processName;
        string machineName;
        int processId;
        int id;

        AppDomainInfo(AppDomain appDomain)
        {
            // Assumption: Only one AppDomainInfo is created per AppDomain
            Fx.Assert(null != appDomain, "");
            this.instanceId = Guid.NewGuid();
            this.friendlyName = appDomain.FriendlyName;
            this.isDefaultAppDomain = appDomain.IsDefaultAppDomain();
            Process process = Process.GetCurrentProcess();
            this.processName = process.ProcessName;
            this.machineName = Environment.MachineName;
            this.processId = process.Id;
            this.id = appDomain.Id;

        }

        public int Id
        {
            get
            {
                return this.id;
            }
        }

        public Guid InstanceId
        {
            get
            {
                return this.instanceId;
            }
        }

        public string MachineName
        {
            get
            {
                return this.machineName;
            }
        }

        public string Name
        {
            get
            {
                return this.friendlyName;
            }
        }

        public bool IsDefaultAppDomain
        {
            get
            {
                return this.isDefaultAppDomain;
            }
        }

        public int ProcessId
        {
            get
            {
                return this.processId;
            }
        }

        public string ProcessName
        {
            get
            {
                return this.processName;
            }
        }

        internal static AppDomainInfo Current
        {
            get
            {
                if (null == singleton)
                {
                    lock (AppDomainInfo.syncRoot)
                    {
                        if (null == singleton)
                        {
                            singleton = new AppDomainInfo(AppDomain.CurrentDomain);
                        }
                    }
                }
                return singleton;
            }
        }
    }
}
