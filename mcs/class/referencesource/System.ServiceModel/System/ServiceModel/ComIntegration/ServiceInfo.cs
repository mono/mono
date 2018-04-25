//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Description;
    using System.Collections.Generic;
    using System.Collections;
    using System.Diagnostics;
    using System.EnterpriseServices;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Configuration;
    using System.Transactions;
    using SR = System.ServiceModel.SR;
    using System.ServiceModel.Diagnostics;
    using System.Runtime;

    // The values of the enum are reflected from the values in the
    // COM+ Admin SDK.
    //
    enum Bitness
    {
        Bitness32 = 0x01,
        Bitness64 = 0x02
    }

    enum ThreadingModel
    {
        MTA,
        STA
    }

    enum HostingMode
    {
        ComPlus,             // Living in a DllHost.exe
        WebHostOutOfProcess, // From webhost to dllhost.exe
        WebHostInProcess     // Inside webhost
    }

    class ServiceInfo
    {
        ServiceElement service;
        Guid clsid;
        Guid appid;
        HostingMode hostingMode;
        Guid partitionId;
        Bitness bitness;
        ThreadingModel threadingModel;
        TransactionOption transactionOption;
        IsolationLevel isolationLevel;
        bool checkRoles;
        string[] componentRoleMembers;
        bool objectPoolingEnabled;
        int maxPoolSize;
        Type managedType;
        List<ContractInfo> contracts;
        string serviceName;
        Dictionary<Guid, List<Type>> udts;


        public string ServiceName
        {
            get
            {
                return serviceName;
            }
        }


        // NOTE: Construction of this thing is quite inefficient-- it
        //       has several nested loops that could probably be
        //       improved. Such optimizations have been left for when
        //       it turns out to be a performance problem, for the
        //       sake of simplicity.
        //
        public ServiceInfo(Guid clsid,
                            ServiceElement service,
                            ComCatalogObject application,
                            ComCatalogObject classObject,
                            HostingMode hostingMode)
        {
            // Simple things...
            //
            this.service = service;
            this.clsid = clsid;
            this.appid = Fx.CreateGuid((string)application.GetValue("ID"));
            this.partitionId = Fx.CreateGuid((string)application.GetValue("AppPartitionID"));
            this.bitness = (Bitness)classObject.GetValue("Bitness");
            this.transactionOption = (TransactionOption)classObject.GetValue("Transaction");
            this.hostingMode = hostingMode;
            this.managedType = TypeCacheManager.ResolveClsidToType(clsid);
            this.serviceName = application.Name + "." + classObject.Name;
            this.udts = new Dictionary<Guid, List<Type>>();

            // Isolation Level
            COMAdminIsolationLevel adminIsolationLevel;
            adminIsolationLevel = (COMAdminIsolationLevel)classObject.GetValue("TxIsolationLevel");
            switch (adminIsolationLevel)
            {
                case COMAdminIsolationLevel.Any:
                    this.isolationLevel = IsolationLevel.Unspecified;
                    break;
                case COMAdminIsolationLevel.ReadUncommitted:
                    this.isolationLevel = IsolationLevel.ReadUncommitted;
                    break;
                case COMAdminIsolationLevel.ReadCommitted:
                    this.isolationLevel = IsolationLevel.ReadCommitted;
                    break;
                case COMAdminIsolationLevel.RepeatableRead:
                    this.isolationLevel = IsolationLevel.RepeatableRead;
                    break;
                case COMAdminIsolationLevel.Serializable:
                    this.isolationLevel = IsolationLevel.Serializable;
                    break;
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(
                        SR.GetString(SR.InvalidIsolationLevelValue,
                                     this.clsid, adminIsolationLevel)));
            }

            // Threading Model
            //
            COMAdminThreadingModel adminThreadingModel;
            adminThreadingModel = (COMAdminThreadingModel)classObject.GetValue("ThreadingModel");
            switch (adminThreadingModel)
            {
                case COMAdminThreadingModel.Apartment:
                case COMAdminThreadingModel.Main:
                    this.threadingModel = ThreadingModel.STA;
                    objectPoolingEnabled = false;
                    break;

                default:
                    this.threadingModel = ThreadingModel.MTA;
                    objectPoolingEnabled = (bool)classObject.GetValue("ObjectPoolingEnabled");
                    break;
            }

            // Object Pool settings
            // 

            if (objectPoolingEnabled)
            {
                maxPoolSize = (int)classObject.GetValue("MaxPoolSize");
            }
            else
                maxPoolSize = 0;
            // Security Settings
            //
            bool appSecurityEnabled;
            appSecurityEnabled = (bool)application.GetValue(
                "ApplicationAccessChecksEnabled");
            if (appSecurityEnabled)
            {

                bool classSecurityEnabled;
                classSecurityEnabled = (bool)classObject.GetValue(
                    "ComponentAccessChecksEnabled");
                if (classSecurityEnabled)
                {
                    this.checkRoles = true;
                }
            }

            // Component Roles
            //
            ComCatalogCollection roles;
            roles = classObject.GetCollection("RolesForComponent");
            this.componentRoleMembers = CatalogUtil.GetRoleMembers(application, roles);
            // Contracts
            // One ContractInfo per unique IID exposed, so we need to
            // filter duplicates.
            //
            this.contracts = new List<ContractInfo>();
            ComCatalogCollection interfaces;
            interfaces = classObject.GetCollection("InterfacesForComponent");
            foreach (ServiceEndpointElement endpoint in service.Endpoints)
            {
                ContractInfo contract = null;
                if (endpoint.Contract == ServiceMetadataBehavior.MexContractName)
                    continue;

                Guid iid;
                if (DiagnosticUtility.Utility.TryCreateGuid(endpoint.Contract, out iid))
                {
                    // (Filter duplicates.)
                    bool duplicate = false;
                    foreach (ContractInfo otherContract in this.contracts)
                    {
                        if (iid == otherContract.IID)
                        {
                            duplicate = true;
                            break;
                        }
                    }
                    if (duplicate) continue;

                    foreach (ComCatalogObject interfaceObject in interfaces)
                    {
                        Guid otherInterfaceID;
                        if (DiagnosticUtility.Utility.TryCreateGuid((string)interfaceObject.GetValue("IID"), out otherInterfaceID))
                        {
                            if (otherInterfaceID == iid)
                            {
                                contract = new ContractInfo(iid,
                                                            endpoint,
                                                            interfaceObject,
                                                            application);
                                break;
                            }
                        }
                    }
                }

                if (contract == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(
                        SR.GetString(SR.EndpointNotAnIID,
                                     clsid.ToString("B").ToUpperInvariant(),
                                     endpoint.Contract)));
                }
                this.contracts.Add(contract);
            }
        }

        public Type ServiceType
        {
            get { return this.managedType; }
        }

        public ServiceElement ServiceElement
        {
            get { return this.service; }
        }

        public Guid Clsid
        {
            get { return this.clsid; }
        }

        public Guid AppID
        {
            get { return this.appid; }
        }
        public Guid PartitionId
        {
            get { return this.partitionId; }
        }

        public Bitness Bitness
        {
            get { return this.bitness; }
        }

        public bool CheckRoles
        {
            get { return this.checkRoles; }
        }


        public ThreadingModel ThreadingModel
        {
            get { return this.threadingModel; }
        }

        public TransactionOption TransactionOption
        {
            get { return this.transactionOption; }
        }

        public IsolationLevel IsolationLevel
        {
            get { return this.isolationLevel; }
        }

        public string[] ComponentRoleMembers
        {
            get { return this.componentRoleMembers; }
        }

        public List<ContractInfo> Contracts
        {
            get { return this.contracts; }
        }

        public HostingMode HostingMode
        {
            get { return this.hostingMode; }
        }

        public bool Pooled
        {
            get { return this.objectPoolingEnabled; }
        }

        public int MaxPoolSize
        {
            get { return this.maxPoolSize; }
        }

        internal Guid[] Assemblies
        {
            get
            {
                Guid[] ret = new Guid[this.udts.Keys.Count];
                this.udts.Keys.CopyTo(ret, 0);

                return ret;
            }
        }

        internal bool HasUdts()
        {
            // use the assembly count since we only add assemblies when we got UDTs
            return (this.udts.Keys.Count > 0);
        }

        internal Type[] GetTypes(Guid assemblyId)
        {
            List<Type> ret = null;
            udts.TryGetValue(assemblyId, out ret);
            if (null == ret)
                return new Type[0];


            return ret.ToArray();
        }

        internal void AddUdt(Type udt, Guid assemblyId)
        {
            if (!udts.ContainsKey(assemblyId))
                udts[assemblyId] = new List<Type>();

            if (!udts[assemblyId].Contains(udt))
                udts[assemblyId].Add(udt);
        }

    }

    class ContractInfo
    {
        string name;
        Guid iid;
        string[] interfaceRoleMembers;
        List<OperationInfo> operations;

        public ContractInfo(Guid iid,
                            ServiceEndpointElement endpoint,
                            ComCatalogObject interfaceObject,
                            ComCatalogObject application)
        {
            this.name = endpoint.Contract;
            this.iid = iid;

            // Interface Roles
            //
            ComCatalogCollection roles;
            roles = interfaceObject.GetCollection("RolesForInterface");
            this.interfaceRoleMembers = CatalogUtil.GetRoleMembers(application,
                                                                   roles);

            // Operations
            //
            this.operations = new List<OperationInfo>();

            ComCatalogCollection methods;
            methods = interfaceObject.GetCollection("MethodsForInterface");
            foreach (ComCatalogObject method in methods)
            {
                this.operations.Add(new OperationInfo(method,
                                                      application));
            }
        }

        public string Name
        {
            get { return this.name; }
        }

        public Guid IID
        {
            get { return this.iid; }
        }

        public string[] InterfaceRoleMembers
        {
            get { return this.interfaceRoleMembers; }
        }

        public List<OperationInfo> Operations
        {
            get { return this.operations; }
        }
    }

    class OperationInfo
    {
        string name;
        string[] methodRoleMembers;

        public OperationInfo(ComCatalogObject methodObject,
                             ComCatalogObject application)
        {
            this.name = (string)methodObject.GetValue("Name");

            // Method Roles
            //
            ComCatalogCollection roles;
            roles = methodObject.GetCollection("RolesForMethod");
            this.methodRoleMembers = CatalogUtil.GetRoleMembers(application,
                                                                roles);
        }

        public string Name
        {
            get { return this.name; }
        }

        public string[] MethodRoleMembers
        {
            get { return this.methodRoleMembers; }
        }
    }
}
