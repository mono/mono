//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activation
{
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.ServiceModel, Version=3.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
    public class ServiceHostFactory : ServiceHostFactoryBase
    {
        Collection<string> referencedAssemblies;

        public ServiceHostFactory()
        {
            this.referencedAssemblies = new Collection<string>();
        }

        internal void AddAssemblyReference(string assemblyName)
        {
            this.referencedAssemblies.Add(assemblyName);
        }

        [SuppressMessage(FxCop.Category.ReliabilityBasic, 
            "Reliability104:CaughtAndHandledExceptionsRule",
            Justification = "When using the Asp.Net 'WebSite-model' this exception can happen due to dynamic compilation. This isn't harmful for WCF as long as the ServiceType can be found in another assembly.")]
        public override ServiceHostBase CreateServiceHost(string constructorString, Uri[] baseAddresses)
        {
            if (!AspNetEnvironment.Enabled)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.Hosting_ProcessNotExecutingUnderHostedContext("ServiceHostFactory.CreateServiceHost")));
            }

            if (string.IsNullOrEmpty(constructorString))
            {
                throw FxTrace.Exception.Argument("constructorString", SR.Hosting_ServiceTypeNotProvided);
            }

            Type type = Type.GetType(constructorString, false);

            if (type == null)
            {
                //config service activation scenario
                if (this.referencedAssemblies.Count == 0)
                {
                    AspNetEnvironment.Current.EnsureAllReferencedAssemblyLoaded();
                }

                foreach (string assemblyName in this.referencedAssemblies)
                {
                    Assembly assembly = null;
                    try
                    {
                        assembly = Assembly.Load(assemblyName);
                    }
                    catch (FileNotFoundException fileNotFoundException)
                    {
                        // When using the Asp.Net "WebSite-model" this exception can happen due to dynamic compilation 
                        // This isn't harmful for WCF as long as the ServiceType can be found in another assembly...
                        if (FxTrace.ShouldTraceInformation)
                        {
                            FxTrace.Exception.AsInformation(fileNotFoundException);
                        }
                    }

                    if (assembly != null)
                    {
                        type = assembly.GetType(constructorString, false);
                        if (type != null)
                        {
                            break;
                        }
                    }
                }
            }

            if (type == null)
            {
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < assemblies.Length; i++)
                {
                    type = assemblies[i].GetType(constructorString, false);
                    if (type != null)
                    {
                        break;
                    }
                }
            }

            if (type == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.Hosting_ServiceTypeNotResolved(constructorString)));
            }

            return CreateServiceHost(type, baseAddresses);
        }

        protected virtual ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            return new ServiceHost(serviceType, baseAddresses);
        }
    }
}
