//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.EnterpriseServices;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Transactions;

    class ComPlusInstanceContextInitializer : IInstanceContextInitializer
    {
        ServiceInfo info;

        static readonly Guid IID_IServiceActivity = new Guid("67532E0C-9E2F-4450-A354-035633944E17");
        static readonly Guid DefaultPartitionId = new Guid("41E90F3E-56C1-4633-81C3-6E8BAC8BDD70");

        private static object manifestLock = new object();
        private static string manifestFileName = Guid.NewGuid().ToString();

        static ComPlusInstanceContextInitializer()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += new ResolveEventHandler(ResolveAssembly);
        }

        public ComPlusInstanceContextInitializer(ServiceInfo info)
        {
            this.info = info;

            if (this.info.HasUdts())
            {
                string tempPath = String.Empty;
                lock (manifestLock)
                {

                    try
                    {
                        tempPath = Path.GetTempPath();
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                            throw;

                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(System.ServiceModel.ComIntegration.Error.CannotAccessDirectory(tempPath));
                    }

                    string manifestDirectory = tempPath + this.info.AppID.ToString();
                    if (Directory.Exists(manifestDirectory))
                        Directory.Delete(manifestDirectory, true);
                }

            }
        }

        public void Initialize(InstanceContext instanceContext, Message message)
        {
            object serviceConfig = null;
            serviceConfig = SetupServiceConfig(instanceContext, message);

            IServiceActivity activity;
            activity = (IServiceActivity)SafeNativeMethods.CoCreateActivity(
                serviceConfig,
                IID_IServiceActivity);

            ComPlusSynchronizationContext syncContext;
            bool postSynchronous = (this.info.ThreadingModel ==
                                    ThreadingModel.MTA);
            syncContext = new ComPlusSynchronizationContext(activity,
                                                            postSynchronous);
            instanceContext.SynchronizationContext = syncContext;

            instanceContext.Closing += this.OnInstanceContextClosing;
            Marshal.ReleaseComObject(serviceConfig);
        }

        public void OnInstanceContextClosing(object sender, EventArgs args)
        {
            InstanceContext instanceContext = (InstanceContext)sender;
            ComPlusSynchronizationContext syncContext;
            syncContext = (ComPlusSynchronizationContext)instanceContext.SynchronizationContext;
            syncContext.Dispose();

        }

        static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            int indexOfComma = args.Name.IndexOf(",", StringComparison.Ordinal);
            if (indexOfComma != -1)
            {
                Guid assemblyGuid = Guid.Empty;
                string assemblyGuidString = args.Name.Substring(0, indexOfComma).Trim().ToLowerInvariant();

                if (Guid.TryParse(assemblyGuidString, out assemblyGuid))
                {
                    return TypeCacheManager.Provider.ResolveAssembly(assemblyGuid);
                }
            }

            return null;
        }


        object SetupServiceConfig(InstanceContext instanceContext, Message message)
        {
            object serviceConfig = new CServiceConfig();

            // Threading
            //
            IServiceThreadPoolConfig threadPoolConfig;
            threadPoolConfig = (IServiceThreadPoolConfig)(serviceConfig);
            switch (this.info.ThreadingModel)
            {
                case ThreadingModel.MTA:
                    threadPoolConfig.SelectThreadPool(ThreadPoolOption.MTA);
                    break;

                case ThreadingModel.STA:
                    threadPoolConfig.SelectThreadPool(ThreadPoolOption.STA);
                    break;

                default:
                    Fx.Assert("Unexpected threading model");

                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.UnexpectedThreadingModel());
            }
            threadPoolConfig.SetBindingInfo(BindingOption.BindingToPoolThread);

            // SxS activation context properties
            //                   


            // Manifest for VARIANT UDT types
            //

            // this only gets executed if we actually have UDTs
            if (this.info.HasUdts())
            {
                IServiceSxsConfig sxsConfig = serviceConfig as IServiceSxsConfig;
                if (sxsConfig == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(System.ServiceModel.ComIntegration.Error.QFENotPresent());
                }

                lock (manifestLock)
                {
                    string tempPath = String.Empty;

                    try
                    {
                        tempPath = Path.GetTempPath();
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                            throw;

                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(System.ServiceModel.ComIntegration.Error.CannotAccessDirectory(tempPath));
                    }

                    string manifestDirectory = tempPath + this.info.AppID.ToString() + @"\";


                    if (!Directory.Exists(manifestDirectory))
                    {
                        try
                        {
                            Directory.CreateDirectory(manifestDirectory);
                        }
                        catch (Exception e)
                        {
                            if (Fx.IsFatal(e))
                                throw;

                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(System.ServiceModel.ComIntegration.Error.CannotAccessDirectory(manifestDirectory));
                        }

                        Guid[] assemblyGuids = this.info.Assemblies;
                        ComIntegrationManifestGenerator.GenerateManifestCollectionFile(assemblyGuids, manifestDirectory + manifestFileName + @".manifest", manifestFileName);

                        foreach (Guid g in assemblyGuids)
                        {
                            Type[] types = this.info.GetTypes(g);
                            if (types.Length > 0)
                            {
                                String guidStr = g.ToString();
                                ComIntegrationManifestGenerator.GenerateWin32ManifestFile(types, manifestDirectory + guidStr + @".manifest", guidStr);
                            }
                        }
                    }


                    sxsConfig.SxsConfig(CSC_SxsConfig.CSC_NewSxs);
                    sxsConfig.SxsName(manifestFileName + @".manifest");
                    sxsConfig.SxsDirectory(manifestDirectory);
                }
            }


            // Partitions
            //
            if (this.info.PartitionId != DefaultPartitionId)
            {
                IServicePartitionConfig partitionConfig;
                partitionConfig = (IServicePartitionConfig)(serviceConfig);
                partitionConfig.PartitionConfig(PartitionOption.New);
                partitionConfig.PartitionID(this.info.PartitionId);
            }

            // Transactions
            //
            IServiceTransactionConfig transactionConfig;
            transactionConfig = (IServiceTransactionConfig)(serviceConfig);
            transactionConfig.ConfigureTransaction(
                TransactionConfig.NoTransaction);

            if ((this.info.TransactionOption == TransactionOption.Required) ||
                (this.info.TransactionOption == TransactionOption.Supported))
            {
                Transaction messageTransaction = null;
                messageTransaction = MessageUtil.GetMessageTransaction(message);
                if (messageTransaction != null)
                {
                    TransactionProxy proxy = new TransactionProxy(info.AppID, info.Clsid);
                    proxy.SetTransaction(messageTransaction);

                    instanceContext.Extensions.Add(proxy);
                    IServiceSysTxnConfig sysTxnconfing = (IServiceSysTxnConfig)transactionConfig;
                    IntPtr pUnk = TransactionProxyBuilder.CreateTransactionProxyTearOff(proxy);
                    sysTxnconfing.ConfigureBYOTSysTxn(pUnk);
                    Marshal.Release(pUnk);
                }
            }
            return serviceConfig;
        }
    }
}
