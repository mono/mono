//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Activation
{
    using System.Web;
    using System.Web.Hosting;
    using System.IO;
    using System.ServiceModel.Diagnostics;
    using System.Web.Compilation;
    using System.Reflection;
    using System.Workflow.Runtime;
    using System.Workflow.ComponentModel.Compiler;
    using System.Diagnostics;

    [Obsolete("The WF3 types are deprecated.  Instead, please use the new WF4 types from System.Activities.*")]
    public class WorkflowServiceHostFactory : ServiceHostFactoryBase
    {
        TypeProvider typeProvider;

        public override ServiceHostBase CreateServiceHost(string constructorString, Uri[] baseAddresses)
        {
            WorkflowDefinitionContext workflowDefinitionContext = null;

            Stream workflowDefinitionStream = null;
            Stream ruleDefinitionStream = null;

            if (string.IsNullOrEmpty(constructorString))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.WorkflowServiceHostFactoryConstructorStringNotProvided)));
            }

            if (!HostingEnvironment.IsHosted)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.ProcessNotExecutingUnderHostedContext)));
            }

            Type workflowType = this.GetTypeFromString(constructorString, baseAddresses);

            if (workflowType != null)
            {
                workflowDefinitionContext = new CompiledWorkflowDefinitionContext(workflowType);
            }
            else
            {
                try
                {
                    IDisposable impersonationContext = null;
                    try
                    {
                        try
                        {

                        }
                        finally
                        {
                            //Ensure thread.Abort doesnt interfere b/w impersonate & assignment.
                            impersonationContext = HostingEnvironment.Impersonate();
                        }

                        string xomlVirtualPath = Path.Combine(AspNetEnvironment.Current.CurrentVirtualPath, constructorString);

                        if (HostingEnvironment.VirtualPathProvider.FileExists(xomlVirtualPath))
                        {
                            workflowDefinitionStream = HostingEnvironment.VirtualPathProvider.GetFile(xomlVirtualPath).Open();
                            string ruleFilePath = Path.ChangeExtension(xomlVirtualPath, WorkflowServiceBuildProvider.ruleFileExtension);

                            if (HostingEnvironment.VirtualPathProvider.FileExists(ruleFilePath))
                            {
                                ruleDefinitionStream = HostingEnvironment.VirtualPathProvider.GetFile(ruleFilePath).Open();
                                workflowDefinitionContext = new StreamedWorkflowDefinitionContext(workflowDefinitionStream, ruleDefinitionStream, this.typeProvider);
                            }
                            else
                            {
                                workflowDefinitionContext = new StreamedWorkflowDefinitionContext(workflowDefinitionStream, null, this.typeProvider);
                            }
                        }
                    }
                    finally
                    {
                        if (impersonationContext != null)
                        {
                            impersonationContext.Dispose();
                        }
                    }
                }
                catch
                {
                    throw; //Prevent impersonation leak through Exception Filters.
                }
                finally
                {
                    if (workflowDefinitionStream != null)
                    {
                        workflowDefinitionStream.Close();
                    }

                    if (ruleDefinitionStream != null)
                    {
                        ruleDefinitionStream.Close();
                    }
                }
            }

            if (workflowDefinitionContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.CannotResolveConstructorStringToWorkflowType, constructorString)));
            }

            WorkflowServiceHost serviceHost = new WorkflowServiceHost(workflowDefinitionContext, baseAddresses);

            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.WorkflowServiceHostCreated, SR.GetString(SR.TraceCodeWorkflowServiceHostCreated), this);
            }
            return serviceHost;
        }

        Type GetTypeFromString(string typeString, Uri[] baseAddresses)
        {
            if (baseAddresses == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("baseAddresses");
            }
            if (baseAddresses.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.BaseAddressesNotProvided)));
            }

            Type workflowType = Type.GetType(typeString, false);

            if (workflowType == null)
            {
                this.typeProvider = new TypeProvider(null);

                // retrieve the reference assembly names from the compiled string supplied by the build manager
                string compiledString;
                try
                {
                    IDisposable impersonationContext = null;
                    try
                    {
                        try
                        {

                        }
                        finally
                        {
                            //Ensure Impersonation + Assignment is atomic w.r.t to potential Thread.Abort.
                            impersonationContext = HostingEnvironment.Impersonate();
                        }
                        compiledString = BuildManager.GetCompiledCustomString(baseAddresses[0].AbsolutePath);
                    }
                    finally
                    {
                        if (impersonationContext != null)
                        {
                            impersonationContext.Dispose();
                        }
                    }
                }
                catch
                {
                    throw; //Prevent impersonation leak through exception filters.
                }

                if (string.IsNullOrEmpty(compiledString))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.InvalidCompiledString, baseAddresses[0].AbsolutePath)));
                }
                string[] components = compiledString.Split('|');
                if (components.Length < 3)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.InvalidCompiledString, baseAddresses[0].AbsolutePath)));
                }

                //Walk reverse direction to increase our chance to match assembly;
                for (int i = (components.Length - 1); i > 2; i--)
                {
                    Assembly assembly = Assembly.Load(components[i]);
                    this.typeProvider.AddAssembly(assembly);
                    workflowType = assembly.GetType(typeString, false);
                    if (workflowType != null)
                    {
                        break;
                    }
                }

                if (workflowType == null)
                {
                    foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        this.typeProvider.AddAssembly(assembly);

                        workflowType = assembly.GetType(typeString, false);
                        if (workflowType != null)
                        {
                            break;
                        }
                    }
                }
            }
            return workflowType;
        }
    }
}
