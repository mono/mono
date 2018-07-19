//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Activities.Activation
{
    using System.Activities;
    using System.Activities.Expressions;
    using System.Activities.XamlIntegration;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceModel.Activation;
    using System.Text;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Hosting;
    using System.Xaml;
    using System.Xml;
    using System.Xml.Linq;

    public class WorkflowServiceHostFactory : ServiceHostFactoryBase
    {
        const string SupportedVersionsGeneratedTypeNamePrefix = "SupportedVersionsGeneratedType_";
        const string SupportedVersionsFolder = "App_Code";
        string xamlVirtualFile;       

        public override ServiceHostBase CreateServiceHost(string constructorString, Uri[] baseAddresses)
        {
            WorkflowServiceHost serviceHost = null;

            if (string.IsNullOrEmpty(constructorString))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.WorkflowServiceHostFactoryConstructorStringNotProvided));
            }

            if (baseAddresses == null)
            {
                throw FxTrace.Exception.ArgumentNull("baseAddresses");
            }

            if (baseAddresses.Length == 0)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.BaseAddressesNotProvided));
            }

            if (!HostingEnvironment.IsHosted)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.Hosting_ProcessNotExecutingUnderHostedContext("WorkflowServiceHostFactory.CreateServiceHost")));
            }

            // We expect most users will use .xamlx file instead of precompiled assembly 
            // Samples of xamlVirtualPath under all scenarios
            //                          constructorString  XamlFileBaseLocation    xamlVirtualPath
            // 1. Xamlx direct          ~/sub/a.xamlx      ~/sub/                  ~/sub/a.xamlx 
            // 2. CBA with precompiled  servicetypeinfo    ~/sub/servicetypeinfo   ~/sub/servicetypeinfo  * no file will be found
            // 3. CBA with xamlx        sub/a.xamlx        ~/                      ~/sub/a.xamlx
            // 4. Svc with precompiled  servicetypeinfo    ~/sub/servicetypeinfo   ~/sub/servicetypeinfo  * no file will be found
            // 5. Svc with Xamlx        ../a.xamlx         ~/sub/                  ~/a.xamlx
                          
            string xamlVirtualPath = VirtualPathUtility.Combine(AspNetEnvironment.Current.XamlFileBaseLocation, constructorString);
            Stream activityStream;
            string compiledCustomString;
            if (GetServiceFileStreamOrCompiledCustomString(xamlVirtualPath, baseAddresses, out activityStream, out compiledCustomString))
            {
                // 

                BuildManager.GetReferencedAssemblies();
                //XmlnsMappingHelper.EnsureMappingPassed();

                this.xamlVirtualFile = xamlVirtualPath;
               
                WorkflowService service;
                using (activityStream)
                {
                    string serviceName = VirtualPathUtility.GetFileName(xamlVirtualPath);
                    string serviceNamespace = String.Format(CultureInfo.InvariantCulture, "/{0}{1}", ServiceHostingEnvironment.SiteName, VirtualPathUtility.GetDirectory(ServiceHostingEnvironment.FullVirtualPath));

                    service = CreatetWorkflowServiceAndSetCompiledExpressionRoot(null, activityStream, XName.Get(XmlConvert.EncodeLocalName(serviceName), serviceNamespace));
                }
                
                if (service != null)
                {                   
                    serviceHost = CreateWorkflowServiceHost(service, baseAddresses);
                }
            }
            else
            {
                Type activityType = this.GetTypeFromAssembliesInCurrentDomain(constructorString);
                if (null == activityType)
                {
                    activityType = GetTypeFromCompileCustomString(compiledCustomString, constructorString);
                }
                if (null == activityType)
                {
                    //for file-less cases, try in referenced assemblies as CompileCustomString assemblies are empty.
                    BuildManager.GetReferencedAssemblies();
                    activityType = this.GetTypeFromAssembliesInCurrentDomain(constructorString);
                }
                if (null != activityType)
                {
                    if (!TypeHelper.AreTypesCompatible(activityType, typeof(Activity)))
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(SR.TypeNotActivity(activityType.FullName)));
                    }

                    Activity activity = (Activity)Activator.CreateInstance(activityType);
                    serviceHost = CreateWorkflowServiceHost(activity, baseAddresses);
                }
            }
            if (serviceHost == null)
            {
                throw FxTrace.Exception.AsError(
                    new InvalidOperationException(SR.CannotResolveConstructorStringToWorkflowType(constructorString)));
            }

            //The Description.Name and Description.NameSpace aren't included intentionally - because 
            //in farm scenarios the sole and unique identifier is the service deployment URL
            ((IDurableInstancingOptions)serviceHost.DurableInstancingOptions).SetScopeName(
                XName.Get(XmlConvert.EncodeLocalName(VirtualPathUtility.GetFileName(ServiceHostingEnvironment.FullVirtualPath)),
                String.Format(CultureInfo.InvariantCulture, "/{0}{1}", ServiceHostingEnvironment.SiteName, VirtualPathUtility.GetDirectory(ServiceHostingEnvironment.FullVirtualPath))));

            return serviceHost;
        }

        internal static object LoadXaml(Stream activityStream)
        {
            object serviceObject;
            XamlXmlReaderSettings xamlXmlReaderSettings = new XamlXmlReaderSettings();
            xamlXmlReaderSettings.ProvideLineInfo = true;
            XamlReader wrappedReader = ActivityXamlServices.CreateReader(new XamlXmlReader(XmlReader.Create(activityStream), xamlXmlReaderSettings));
            if (TD.XamlServicesLoadStartIsEnabled())
            {
                TD.XamlServicesLoadStart();
            }
            serviceObject = XamlServices.Load(wrappedReader);
            if (TD.XamlServicesLoadStopIsEnabled())
            {
                TD.XamlServicesLoadStop();
            }
            return serviceObject;
        }
        
        void AddSupportedVersions(WorkflowServiceHost workflowServiceHost, WorkflowService baseService)
        {
            AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();

            IList<Tuple<string, Stream>> streams = null;
            string xamlFileName = Path.GetFileNameWithoutExtension(VirtualPathUtility.GetFileName(this.xamlVirtualFile));
            GetSupportedVersionStreams(xamlFileName, out streams);
           
            if (streams != null)
            {
                try
                {
                    foreach (Tuple<string, Stream> stream in streams)
                    {
                        try
                        {
                            WorkflowService service = CreatetWorkflowServiceAndSetCompiledExpressionRoot(stream.Item1, stream.Item2, baseService.Name);
                            if (service != null)
                            {
                                workflowServiceHost.SupportedVersions.Add(service);
                            }
                        }
                        catch (Exception e)
                        {
                            Exception newException;
                            if (Fx.IsFatal(e) || !TryWrapSupportedVersionException(stream.Item1, e, out newException))
                            {
                                throw;
                            }

                            throw FxTrace.Exception.AsError(newException);
                        }
                    }
                }
                finally
                {
                    foreach (Tuple<string, Stream> stream in streams)
                    {
                        stream.Item2.Dispose();
                    }
                }
            }
        }

        internal static bool TryWrapSupportedVersionException(string filePath, Exception e, out Exception newException)
        {
            // We replace the exception, rather than simply wrapping it, because the activation error page highlights 
            // the innermost exception, and we want that  exception to clearly show which file is the culprit.
            // Of course, if the exception has an inner exception, that will still get highlighted instead.
            // Also, for Xaml and XmlException, we don't propagate the line info, because the exception message already contains it.
            if (e is XmlException)
            {
                newException = new XmlException(SR.ExceptionLoadingSupportedVersion(filePath, e.Message), e.InnerException);
                return true;
            }

            if (e is XamlException)
            {
                newException = new XamlException(SR.ExceptionLoadingSupportedVersion(filePath, e.Message), e.InnerException);
                return true;
            }

            if (e is InvalidWorkflowException)
            {
                newException = new InvalidWorkflowException(SR.ExceptionLoadingSupportedVersion(filePath, e.Message), e.InnerException);
                return true;
            }

            if (e is ValidationException)
            {
                newException = new ValidationException(SR.ExceptionLoadingSupportedVersion(filePath, e.Message), e.InnerException);
                return true;
            }

            newException = null;
            return false;
        }

        internal static string GetSupportedVersionGeneratedTypeName(string filePath)
        {
            string activityName = Path.GetFileNameWithoutExtension(filePath).ToUpper(CultureInfo.InvariantCulture);
            
            StringBuilder sb = new StringBuilder(WorkflowServiceHostFactory.SupportedVersionsGeneratedTypeNamePrefix);
            for (int i = 0; i < activityName.Length; i++)
            {
                sb.Append(char.ConvertToUtf32(activityName, i));
            }

            return sb.ToString();
        }

        [Fx.Tag.SecurityNote(Critical = "Calls SecurityCritical method HostingEnvironmentWrapper.UnsafeImpersonate().",
           Safe = "Demands unmanaged code permission, disposes impersonation context in a finally, and tightly scopes the usage of the impersonation token.")]
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        [SecuritySafeCritical]
        internal static void GetSupportedVersionStreams(string xamlFileName, out IList<Tuple<string, Stream>> streams)
        {
            AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();

            string virtualFileFolder = string.Format(CultureInfo.InvariantCulture, "~\\{0}", Path.Combine(SupportedVersionsFolder, xamlFileName));
            IDisposable unsafeImpersonate = null;
            List<Tuple<string, Stream>> streamList = new List<Tuple<string, Stream>>();
            bool cleanExit = false;
            streams = null;
            try
            {
                try
                {
                    try
                    {
                    }
                    finally
                    {
                        unsafeImpersonate = HostingEnvironmentWrapper.UnsafeImpersonate();
                    }

                    if (HostingEnvironment.VirtualPathProvider.DirectoryExists(virtualFileFolder))
                    {
                        string xamlFileFolder = HostingEnvironmentWrapper.MapPath(virtualFileFolder);
                        string[] files = Directory.GetFiles(xamlFileFolder, "*.xamlx");
                        foreach (string file in files)
                        {
                            Stream activityStream;
                            string path = Path.Combine(SupportedVersionsFolder, xamlFileName, Path.GetFileName(file));
                            string virtualFile = string.Format(CultureInfo.InvariantCulture, "~\\{0}", path);

                            if (HostingEnvironment.VirtualPathProvider.FileExists(virtualFile))
                            {
                                activityStream = HostingEnvironment.VirtualPathProvider.GetFile(virtualFile).Open();
                                streamList.Add(Tuple.Create(path, activityStream));
                            }
                        }
                    }
                    cleanExit = true;
                }
                finally
                {
                    if (unsafeImpersonate != null)
                    {
                        unsafeImpersonate.Dispose();
                        unsafeImpersonate = null;
                    }                    
                }
            }
            catch
            {
                cleanExit = false;
                throw;
            }
            finally
            {
                if (cleanExit)
                {
                    streams = streamList;
                }
                else
                {
                    foreach (Tuple<string, Stream> stream in streamList)
                    {
                        stream.Item2.Dispose();
                    }
                }
            }
        }

        static WorkflowService CreatetWorkflowServiceAndSetCompiledExpressionRoot(string supportedVersionXamlxfilePath, Stream activityStream, XName defaultServiceName)
        {
            WorkflowService service = CreatetWorkflowService(activityStream, defaultServiceName);
            if (service != null)
            {
                // CompiledExpression is not supported on Configuration Based Activation (CBA) scenario.
                if (ServiceHostingEnvironment.IsHosted && !ServiceHostingEnvironment.IsConfigurationBased)
                {
                    // We use ServiceHostingEnvironment.FullVirtualPath (instead of the constructor string) because we’re passing this path to BuildManager, 
                    // which may not understand the file type referenced by the constructor string (e.g. .xaml).
                    ICompiledExpressionRoot expressionRoot = XamlBuildProviderExtension.GetExpressionRoot(supportedVersionXamlxfilePath, service, ServiceHostingEnvironment.FullVirtualPath);
                    if (expressionRoot != null)
                    {
                        CompiledExpressionInvoker.SetCompiledExpressionRoot(service.Body, expressionRoot);
                    }
                }
            }

            return service;
        }

        internal static WorkflowService CreatetWorkflowService(Stream activityStream, XName defaultServiceName)
        {   
            WorkflowService service = null;
            object serviceObject;

            serviceObject = LoadXaml(activityStream);
   
            if (serviceObject is Activity)
            {
                service = new WorkflowService
                {
                    Body = (Activity)serviceObject
                };
            }
            else if (serviceObject is WorkflowService)
            {
                service = (WorkflowService)serviceObject;
            }

            // If name of the service is not set
            // service name = xaml file name with extension
            // service namespace = IIS virtual path
            // service config name = Activity.DisplayName
            if (service != null)
            {
                if (service.Name == null)
                {
                    service.Name = defaultServiceName;

                    if (service.ConfigurationName == null && service.Body != null)
                    {
                        service.ConfigurationName = XmlConvert.EncodeLocalName(service.Body.DisplayName);
                    }
                }
            }
            return service;
        }        

        Type GetTypeFromAssembliesInCurrentDomain(string typeString)
        {
            Type activityType = Type.GetType(typeString, false);
            if (null == activityType)
            {
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    activityType = assembly.GetType(typeString, false);
                    if (null != activityType)
                    {
                        break;
                    }
                }
            }
            return activityType;
        }

        Type GetTypeFromCompileCustomString(string compileCustomString, string typeString)
        {
            if (string.IsNullOrEmpty(compileCustomString))
            {
                return null;
            }
            string[] components = compileCustomString.Split('|');
            if (components.Length < 3)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.InvalidCompiledString(compileCustomString)));
            }
            Type activityType = null;
            for (int i = 3; i < components.Length; i++)
            {
                Assembly assembly = Assembly.Load(components[i]);
                activityType = assembly.GetType(typeString, false);
                if (activityType != null)
                {
                    break;
                }
            }
            return activityType;
        }

        protected virtual WorkflowServiceHost CreateWorkflowServiceHost(Activity activity, Uri[] baseAddresses)
        {
            return new WorkflowServiceHost(activity, baseAddresses);
        }

        protected virtual WorkflowServiceHost CreateWorkflowServiceHost(WorkflowService service, Uri[] baseAddresses)
        {
            WorkflowServiceHost workflowServiceHost = new WorkflowServiceHost(service, baseAddresses);
            if (service.DefinitionIdentity != null)
            {
                AddSupportedVersions(workflowServiceHost, service);
            }
            return workflowServiceHost;
        }

        // The code is optimized for reducing impersonation 
        // true means serviceFileStream has been set; false means CompiledCustomString has been set
        [Fx.Tag.SecurityNote(Critical = "Calls SecurityCritical method HostingEnvironmentWrapper.UnsafeImpersonate().",
            Safe = "Demands unmanaged code permission, disposes impersonation context in a finally, and tightly scopes the usage of the impersonation token.")]
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        [SecuritySafeCritical]
        bool GetServiceFileStreamOrCompiledCustomString(string virtualPath, Uri[] baseAddresses, out Stream serviceFileStream, out string compiledCustomString)
        {
            AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();

            IDisposable unsafeImpersonate = null;
            compiledCustomString = null;
            serviceFileStream = null;
            try
            {
                try
                {
                    try
                    {
                    }
                    finally
                    {
                        unsafeImpersonate = HostingEnvironmentWrapper.UnsafeImpersonate();
                    }
                    if (HostingEnvironment.VirtualPathProvider.FileExists(virtualPath))
                    {
                        serviceFileStream = HostingEnvironment.VirtualPathProvider.GetFile(virtualPath).Open();
                        return true;
                    }
                    else
                    {
                        if (!AspNetEnvironment.Current.IsConfigurationBased)
                        {
                            compiledCustomString = BuildManager.GetCompiledCustomString(baseAddresses[0].AbsolutePath);
                        }
                        return false;
                    }
                }
                finally
                {
                    if (null != unsafeImpersonate)
                    {
                        unsafeImpersonate.Dispose();
                    }
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
