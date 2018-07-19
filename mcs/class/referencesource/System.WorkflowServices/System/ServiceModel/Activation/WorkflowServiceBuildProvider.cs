//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Activation
{
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Web.Compilation;

    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "instantiated from config")]
    [BuildProviderAppliesTo(BuildProviderAppliesTo.All)]
    [ServiceActivationBuildProvider]
    class WorkflowServiceBuildProvider : BuildProvider
    {
        internal const string ruleFileExtension = ".rules";
        object[] virtualPathDependencies;

        public override ICollection VirtualPathDependencies
        {
            get
            {
                if (this.virtualPathDependencies == null)
                {
                    ArrayList dependencies = new ArrayList(base.VirtualPathDependencies.Count + 1);
                    dependencies.AddRange(base.VirtualPathDependencies);
                    dependencies.Add(Path.ChangeExtension(base.VirtualPath, ruleFileExtension));
                    this.virtualPathDependencies = dependencies.ToArray();
                }
                return virtualPathDependencies;
            }
        }

        Type ServiceHostFactoryType
        {
            get
            {
                return typeof(WorkflowServiceHostFactory);
            }
        }

        //CompileStringTemplate : "__VIRTUAL_PATH__|__FACTORY_NAME__|__SERVICE_VALUE__";
        public override string GetCustomString(CompilerResults results)
        {
            return (base.VirtualPath + "|" + ServiceHostFactoryType.AssemblyQualifiedName + "|" + base.VirtualPath);
        }

        public override BuildProviderResultFlags GetResultFlags(CompilerResults results)
        {
            return BuildProviderResultFlags.ShutdownAppDomainOnChange;
        }
    }
}
