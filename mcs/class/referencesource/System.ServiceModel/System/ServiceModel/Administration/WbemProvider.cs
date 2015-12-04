//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Administration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Text.RegularExpressions;
    using System.Threading;

    class WbemProvider : WbemNative.IWbemProviderInit, WbemNative.IWbemServices
    {
        object syncRoot = new object();
        WbemNative.IWbemDecoupledRegistrar wbemRegistrar = null;
        WbemNative.IWbemServices wbemServices = null;
        Dictionary<string, IWmiProvider> wmiProviders = new Dictionary<string, IWmiProvider>(StringComparer.OrdinalIgnoreCase);
        string nameSpace;
        string appName;
        bool initialized = false;

        internal WbemProvider(string nameSpace, string appName)
        {
            this.nameSpace = nameSpace;
            this.appName = appName;
        }

        internal void Initialize()
        {
            try
            {
                AppDomain.CurrentDomain.DomainUnload += new EventHandler(ExitOrUnloadEventHandler);
                AppDomain.CurrentDomain.ProcessExit += new EventHandler(ExitOrUnloadEventHandler);
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(ExitOrUnloadEventHandler);
                MTAExecute(new WaitCallback(RegisterWbemProvider), null);
                this.initialized = true;
            }
            catch (SecurityException)
            {
                // WMI is not supported in PT, rethrow a meaningful exception (will fail the service activation)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR.GetString(SR.PartialTrustWMINotEnabled)));
            }
        }

        void RegisterWbemProvider(object state)
        {
            this.wbemRegistrar = (WbemNative.IWbemDecoupledRegistrar)new WbemNative.WbemDecoupledRegistrar();
            int hr = this.wbemRegistrar.Register(0, null, null, null,
                this.nameSpace, this.appName, this);
            if ((int)WbemNative.WbemStatus.WBEM_S_NO_ERROR != hr)
            {
                DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error,
                    (ushort)System.Runtime.Diagnostics.EventLogCategory.Wmi,
                    (uint)System.Runtime.Diagnostics.EventLogEventId.WmiRegistrationFailed,
                    TraceUtility.CreateSourceString(this),
                    hr.ToString("x", CultureInfo.InvariantCulture));

                this.wbemRegistrar = null;
            }
        }

        void UnRegisterWbemProvider(object state)
        {
            if (this.wbemRegistrar != null)
            {
                int hr = this.wbemRegistrar.UnRegister();
                if ((int)WbemNative.WbemStatus.WBEM_S_NO_ERROR != hr)
                {
                    DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error,
                        (ushort)System.Runtime.Diagnostics.EventLogCategory.Wmi,
                        (uint)System.Runtime.Diagnostics.EventLogEventId.WmiUnregistrationFailed,
                        TraceUtility.CreateSourceString(this),
                        hr.ToString("x", CultureInfo.InvariantCulture));
                }
                this.wbemRegistrar = null;
            }
        }

        void ExitOrUnloadEventHandler(object sender, EventArgs e)
        {
            if (this.wbemRegistrar != null)
            {
                MTAExecute(new WaitCallback(UnRegisterWbemProvider), null);
            }
        }

        public void Register(string className, IWmiProvider wmiProvider)
        {
            lock (this.syncRoot)
            {
                if (!this.initialized)
                {
                    Initialize();
                }

                this.wmiProviders.Add(className, wmiProvider);
            }
        }

        IWmiProvider GetProvider(string className)
        {
            IWmiProvider wmiProvider;
            lock (this.wmiProviders)
            {
                if (!this.wmiProviders.TryGetValue(className, out wmiProvider))
                {
                    wmiProvider = NoInstanceWMIProvider.Default;
                }
            }
            return wmiProvider;
        }

        int WbemNative.IWbemProviderInit.Initialize(
            string wszUser,
            Int32 lFlags,
            string wszNamespace,
            string wszLocale,
            WbemNative.IWbemServices wbemServices,
            WbemNative.IWbemContext wbemContext,
            WbemNative.IWbemProviderInitSink wbemSink
            )
        {
            if (wbemServices == null || wbemContext == null || wbemSink == null)
                return (int)WbemNative.WbemStatus.WBEM_E_INVALID_PARAMETER;

            try
            {
                MTAExecute(new WaitCallback(this.RelocateWbemServicesRCWToMTA), wbemServices);
                wbemSink.SetStatus((int)WbemNative.tag_WBEM_EXTRA_RETURN_CODES.WBEM_S_INITIALIZED, 0);
            }
            catch (WbemException e)
            {
                wbemSink.SetStatus(e.ErrorCode, 0);
                return e.ErrorCode;
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception)
            {
                wbemSink.SetStatus((int)WbemNative.WbemStatus.WBEM_E_FAILED, 0);
                return (int)WbemNative.WbemStatus.WBEM_E_FAILED;
            }
            finally
            {
                // WMI relies on destructor of this interface to perform certain task
                // explicitly release this so that the GC won't hold on to the ref of 
                // this after the function
                Marshal.ReleaseComObject(wbemSink);
            }

            return (int)WbemNative.WbemStatus.WBEM_S_NO_ERROR;
        }

        void RelocateWbemServicesRCWToMTA(object comObject)
        {
            IntPtr pUnk = Marshal.GetIUnknownForObject(comObject);
            Marshal.ReleaseComObject(comObject);
            this.wbemServices = (WbemNative.IWbemServices)Marshal.GetObjectForIUnknown(pUnk);
            Marshal.Release(pUnk);
        }

        int WbemNative.IWbemServices.OpenNamespace(
            string nameSpace,
            Int32 flags,
            WbemNative.IWbemContext wbemContext,
            ref WbemNative.IWbemServices wbemServices,
            IntPtr wbemCallResult
            )
        {
            return (int)WbemNative.WbemStatus.WBEM_E_NOT_SUPPORTED;
        }

        int WbemNative.IWbemServices.CancelAsyncCall(
            WbemNative.IWbemObjectSink wbemSink
            )
        {
            return (int)WbemNative.WbemStatus.WBEM_E_NOT_SUPPORTED;
        }

        int WbemNative.IWbemServices.QueryObjectSink(
            Int32 flags,
            out WbemNative.IWbemObjectSink wbemSink
            )
        {
            wbemSink = null;
            return (int)WbemNative.WbemStatus.WBEM_E_NOT_SUPPORTED;
        }

        int WbemNative.IWbemServices.GetObject(
            string objectPath,
            Int32 flags,
            WbemNative.IWbemContext wbemContext,
            ref WbemNative.IWbemClassObject wbemObject,
            IntPtr wbemResult
            )
        {
            return (int)WbemNative.WbemStatus.WBEM_E_NOT_SUPPORTED;
        }

        int WbemNative.IWbemServices.GetObjectAsync(
            string objectPath,
            Int32 flags,
            WbemNative.IWbemContext wbemContext,
            WbemNative.IWbemObjectSink wbemSink)
        {
            if (wbemContext == null || wbemSink == null || this.wbemServices == null)
                return (int)WbemNative.WbemStatus.WBEM_E_INVALID_PARAMETER;

            using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ?
                ServiceModelActivity.CreateActivity(true, SR.GetString(SR.WmiGetObject, string.IsNullOrEmpty(objectPath) ? string.Empty : objectPath), ActivityType.WmiGetObject) : null)
            {
                try
                {
                    ObjectPathRegex objPathRegex = new ObjectPathRegex(objectPath);
                    ParameterContext parms = new ParameterContext(objPathRegex.ClassName, this.wbemServices, wbemContext, wbemSink);
                    WbemInstance wbemInstance = new WbemInstance(parms, objPathRegex);
                    IWmiProvider wmiProvider = this.GetProvider(parms.ClassName);
                    if (wmiProvider.GetInstance(new InstanceContext(wbemInstance)))
                    {
                        wbemInstance.Indicate();
                    }

                    WbemException.ThrowIfFail(wbemSink.SetStatus(
                        (int)WbemNative.tag_WBEM_STATUS_TYPE.WBEM_STATUS_COMPLETE,
                        (int)WbemNative.WbemStatus.WBEM_S_NO_ERROR,
                        null,
                        null));
                }
                catch (WbemException e)
                {
                    DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error, (ushort)System.Runtime.Diagnostics.EventLogCategory.Wmi, (uint)System.Runtime.Diagnostics.EventLogEventId.WmiGetObjectFailed,
                        TraceUtility.CreateSourceString(this), e.ToString());
                    wbemSink.SetStatus((int)WbemNative.tag_WBEM_STATUS_TYPE.WBEM_STATUS_COMPLETE,
                        e.ErrorCode, null, null);
                    return e.ErrorCode;
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error, (ushort)System.Runtime.Diagnostics.EventLogCategory.Wmi, (uint)System.Runtime.Diagnostics.EventLogEventId.WmiGetObjectFailed,
                        TraceUtility.CreateSourceString(this), e.ToString());
                    wbemSink.SetStatus((int)WbemNative.tag_WBEM_STATUS_TYPE.WBEM_STATUS_COMPLETE,
                        (int)WbemNative.WbemStatus.WBEM_E_FAILED, null, null);
                    return (int)WbemNative.WbemStatus.WBEM_E_FAILED;
                }
                finally
                {
                    Marshal.ReleaseComObject(wbemSink);
                }
            }
            return (int)WbemNative.WbemStatus.WBEM_S_NO_ERROR;
        }

        int WbemNative.IWbemServices.PutClass(
            WbemNative.IWbemClassObject wbemObject,
            Int32 flags,
            WbemNative.IWbemContext wbemContext,
            IntPtr wbemCallResult)
        {
            return (int)WbemNative.WbemStatus.WBEM_E_NOT_SUPPORTED;
        }

        int WbemNative.IWbemServices.PutClassAsync(
            WbemNative.IWbemClassObject wbemObject,
            Int32 flags,
            WbemNative.IWbemContext wbemContext,
            WbemNative.IWbemObjectSink wbemSink)
        {
            return (int)WbemNative.WbemStatus.WBEM_E_NOT_SUPPORTED;
        }

        int WbemNative.IWbemServices.DeleteClass(
            string className,
            Int32 flags,
            WbemNative.IWbemContext wbemContext,
            IntPtr wbemCallResult)
        {
            return (int)WbemNative.WbemStatus.WBEM_E_NOT_SUPPORTED;
        }

        int WbemNative.IWbemServices.DeleteClassAsync(
            string className,
            Int32 lFlags,
            WbemNative.IWbemContext wbemContext,
            WbemNative.IWbemObjectSink wbemSink)
        {
            return (int)WbemNative.WbemStatus.WBEM_E_NOT_SUPPORTED;
        }

        int WbemNative.IWbemServices.CreateClassEnum(
            string superClassName,
            Int32 flags,
            WbemNative.IWbemContext wbemContext,
            out WbemNative.IEnumWbemClassObject wbemEnum)
        {
            wbemEnum = null;
            return (int)WbemNative.WbemStatus.WBEM_E_NOT_SUPPORTED;
        }

        int WbemNative.IWbemServices.CreateClassEnumAsync(
            string superClassName,
            Int32 flags,
            WbemNative.IWbemContext wbemContext,
            WbemNative.IWbemObjectSink wbemSink)
        {
            return (int)WbemNative.WbemStatus.WBEM_E_NOT_SUPPORTED;
        }

        int WbemNative.IWbemServices.PutInstance(
            WbemNative.IWbemClassObject pInst,
            Int32 lFlags,
            WbemNative.IWbemContext wbemContext,
            IntPtr wbemCallResult)
        {
            return (int)WbemNative.WbemStatus.WBEM_E_NOT_SUPPORTED;
        }

        int WbemNative.IWbemServices.PutInstanceAsync(
            WbemNative.IWbemClassObject wbemObject,
            Int32 lFlags,
            WbemNative.IWbemContext wbemContext,
            WbemNative.IWbemObjectSink wbemSink
            )
        {
            if (wbemObject == null || wbemContext == null || wbemSink == null || this.wbemServices == null)
                return (int)WbemNative.WbemStatus.WBEM_E_INVALID_PARAMETER;

            using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : null)
            {
                try
                {
                    object val = null;
                    int type = 0;
                    int favor = 0;
                    WbemException.ThrowIfFail(wbemObject.Get("__CLASS", 0, ref val, ref type, ref favor));
                    string className = (string)val;
                    ServiceModelActivity.Start(activity, SR.GetString(SR.WmiPutInstance, string.IsNullOrEmpty(className) ? string.Empty : className), ActivityType.WmiPutInstance);

                    ParameterContext parms = new ParameterContext(className, this.wbemServices, wbemContext, wbemSink);
                    WbemInstance wbemInstance = new WbemInstance(parms, wbemObject);
                    IWmiProvider wmiProvider = this.GetProvider(parms.ClassName);
                    if (wmiProvider.PutInstance(new InstanceContext(wbemInstance)))
                    {
                        wbemInstance.Indicate();
                    }

                    WbemException.ThrowIfFail(wbemSink.SetStatus((int)WbemNative.tag_WBEM_STATUS_TYPE.WBEM_STATUS_COMPLETE,
                        (int)WbemNative.WbemStatus.WBEM_S_NO_ERROR, null, null));
                }
                catch (WbemException e)
                {
                    DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error, (ushort)System.Runtime.Diagnostics.EventLogCategory.Wmi, (uint)System.Runtime.Diagnostics.EventLogEventId.WmiPutInstanceFailed,
                        TraceUtility.CreateSourceString(this), e.ToString());
                    wbemSink.SetStatus((int)WbemNative.tag_WBEM_STATUS_TYPE.WBEM_STATUS_COMPLETE,
                        e.ErrorCode, null, null);
                    return e.ErrorCode;
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error, (ushort)System.Runtime.Diagnostics.EventLogCategory.Wmi, (uint)System.Runtime.Diagnostics.EventLogEventId.WmiPutInstanceFailed,
                        TraceUtility.CreateSourceString(this), e.ToString());
                    wbemSink.SetStatus((int)WbemNative.tag_WBEM_STATUS_TYPE.WBEM_STATUS_COMPLETE,
                        (int)WbemNative.WbemStatus.WBEM_E_FAILED, null, null);
                    return (int)WbemNative.WbemStatus.WBEM_E_FAILED;
                }
                finally
                {
                    Marshal.ReleaseComObject(wbemSink);
                }
            }
            return (int)WbemNative.WbemStatus.WBEM_S_NO_ERROR;
        }

        int WbemNative.IWbemServices.DeleteInstance(
            string objectPath,
            Int32 flags,
            WbemNative.IWbemContext wbemContext,
            IntPtr wbemCallResult)
        {
            return (int)WbemNative.WbemStatus.WBEM_E_NOT_SUPPORTED;
        }

        int WbemNative.IWbemServices.DeleteInstanceAsync(
            string objectPath,
            Int32 lFlags,
            WbemNative.IWbemContext wbemContext,
            WbemNative.IWbemObjectSink wbemSink)
        {
            if (wbemContext == null || wbemSink == null || this.wbemServices == null)
                return (int)WbemNative.WbemStatus.WBEM_E_INVALID_PARAMETER;

            try
            {
                ObjectPathRegex objPathRegex = new ObjectPathRegex(objectPath);
                ParameterContext parms = new ParameterContext(objPathRegex.ClassName, this.wbemServices, wbemContext, wbemSink);
                WbemInstance wbemInstance = new WbemInstance(parms, objPathRegex);
                IWmiProvider wmiProvider = this.GetProvider(parms.ClassName);
                if (wmiProvider.DeleteInstance(new InstanceContext(wbemInstance)))
                {
                    wbemInstance.Indicate();
                }

                WbemException.ThrowIfFail(wbemSink.SetStatus(
                    (int)WbemNative.tag_WBEM_STATUS_TYPE.WBEM_STATUS_COMPLETE,
                    (int)WbemNative.WbemStatus.WBEM_S_NO_ERROR, null, null));
            }
            catch (WbemException e)
            {
                DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error,
                    (ushort)System.Runtime.Diagnostics.EventLogCategory.Wmi,
                    (uint)System.Runtime.Diagnostics.EventLogEventId.WmiDeleteInstanceFailed,
                    e.ToString());
                wbemSink.SetStatus((int)WbemNative.tag_WBEM_STATUS_TYPE.WBEM_STATUS_COMPLETE,
                    e.ErrorCode, null, null);
                return e.ErrorCode;
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error,
                    (ushort)System.Runtime.Diagnostics.EventLogCategory.Wmi,
                    (uint)System.Runtime.Diagnostics.EventLogEventId.WmiDeleteInstanceFailed,
                    e.ToString());
                wbemSink.SetStatus((int)WbemNative.tag_WBEM_STATUS_TYPE.WBEM_STATUS_COMPLETE,
                    (int)WbemNative.WbemStatus.WBEM_E_FAILED, null, null);
                return (int)WbemNative.WbemStatus.WBEM_E_FAILED;
            }
            finally
            {
                Marshal.ReleaseComObject(wbemSink);
            }
            return (int)WbemNative.WbemStatus.WBEM_S_NO_ERROR;
        }

        int WbemNative.IWbemServices.CreateInstanceEnum(
            string filter,
            Int32 flags,
            WbemNative.IWbemContext wbemContext,
            out WbemNative.IEnumWbemClassObject wbemEnum
            )
        {
            wbemEnum = null;
            return (int)WbemNative.WbemStatus.WBEM_E_NOT_SUPPORTED;
        }

        int WbemNative.IWbemServices.CreateInstanceEnumAsync(
            string className,
            Int32 flags,
            WbemNative.IWbemContext wbemContext,
            WbemNative.IWbemObjectSink wbemSink)
        {
            if (wbemContext == null || wbemSink == null || this.wbemServices == null)
                return (int)WbemNative.WbemStatus.WBEM_E_INVALID_PARAMETER;

            try
            {
                ParameterContext parms = new ParameterContext(className, this.wbemServices, wbemContext, wbemSink);
                IWmiProvider wmiProvider = this.GetProvider(parms.ClassName);
                wmiProvider.EnumInstances(new InstancesContext(parms));

                WbemException.ThrowIfFail(wbemSink.SetStatus(
                    (int)WbemNative.tag_WBEM_STATUS_TYPE.WBEM_STATUS_COMPLETE,
                    (int)WbemNative.WbemStatus.WBEM_S_NO_ERROR,
                    null,
                    null));
            }
            catch (WbemException e)
            {
                wbemSink.SetStatus((int)WbemNative.tag_WBEM_STATUS_TYPE.WBEM_STATUS_COMPLETE,
                    e.ErrorCode, null, null);
                DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error,
                    (ushort)System.Runtime.Diagnostics.EventLogCategory.Wmi,
                    (uint)System.Runtime.Diagnostics.EventLogEventId.WmiCreateInstanceFailed,
                    className,
                    e.ToString());
                return e.ErrorCode;
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error,
                    (ushort)System.Runtime.Diagnostics.EventLogCategory.Wmi,
                    (uint)System.Runtime.Diagnostics.EventLogEventId.WmiCreateInstanceFailed,
                    className,
                    e.ToString());
                wbemSink.SetStatus((int)WbemNative.tag_WBEM_STATUS_TYPE.WBEM_STATUS_COMPLETE,
                    (int)WbemNative.WbemStatus.WBEM_E_FAILED, null, null);
                return (int)WbemNative.WbemStatus.WBEM_E_FAILED;
            }
            finally
            {
                Marshal.ReleaseComObject(wbemSink);
            }
            return (int)WbemNative.WbemStatus.WBEM_S_NO_ERROR;
        }

        int WbemNative.IWbemServices.ExecQuery(
            string queryLanguage,
            string query,
            Int32 flags,
            WbemNative.IWbemContext wbemContext,
            out WbemNative.IEnumWbemClassObject wbemEnum)
        {
            wbemEnum = null;
            return (int)WbemNative.WbemStatus.WBEM_E_NOT_SUPPORTED;
        }

        int WbemNative.IWbemServices.ExecQueryAsync(
            string queryLanguage,
            string query,
            Int32 flags,
            WbemNative.IWbemContext wbemContext,
            WbemNative.IWbemObjectSink wbemSink)
        {
            if (wbemContext == null || wbemSink == null || this.wbemServices == null)
                return (int)WbemNative.WbemStatus.WBEM_E_INVALID_PARAMETER;

            try
            {
                QueryRegex queryRegex = new QueryRegex(query);
                ParameterContext parms = new ParameterContext(queryRegex.ClassName, this.wbemServices, wbemContext, wbemSink);
                IWmiProvider wmiProvider = this.GetProvider(parms.ClassName);
                //we let WMI to parse WQL to filter results from appropriate provider
                wmiProvider.EnumInstances(new InstancesContext(parms));

                WbemException.ThrowIfFail(wbemSink.SetStatus(
                    (int)WbemNative.tag_WBEM_STATUS_TYPE.WBEM_STATUS_COMPLETE,
                    (int)WbemNative.WbemStatus.WBEM_S_NO_ERROR, null, null));
            }
            catch (WbemException e)
            {
                DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error,
                   (ushort)System.Runtime.Diagnostics.EventLogCategory.Wmi,
                   (uint)System.Runtime.Diagnostics.EventLogEventId.WmiExecQueryFailed,
                   e.ToString());
                wbemSink.SetStatus((int)WbemNative.tag_WBEM_STATUS_TYPE.WBEM_STATUS_COMPLETE,
                    e.ErrorCode, null, null);
                return e.ErrorCode;
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error,
                   (ushort)System.Runtime.Diagnostics.EventLogCategory.Wmi,
                   (uint)System.Runtime.Diagnostics.EventLogEventId.WmiExecQueryFailed,
                   e.ToString());
                wbemSink.SetStatus((int)WbemNative.tag_WBEM_STATUS_TYPE.WBEM_STATUS_COMPLETE,
                    (int)WbemNative.WbemStatus.WBEM_E_FAILED, null, null);
                return (int)WbemNative.WbemStatus.WBEM_E_FAILED;
            }
            finally
            {
                Marshal.ReleaseComObject(wbemSink);
            }
            return (int)WbemNative.WbemStatus.WBEM_S_NO_ERROR;
        }

        int WbemNative.IWbemServices.ExecNotificationQuery(
            string queryLanguage,
            string query,
            Int32 flags,
            WbemNative.IWbemContext wbemContext,
            out WbemNative.IEnumWbemClassObject wbemEnum)
        {
            wbemEnum = null;
            return (int)WbemNative.WbemStatus.WBEM_E_NOT_SUPPORTED;
        }

        int WbemNative.IWbemServices.ExecNotificationQueryAsync(
            string queryLanguage,
            string query,
            Int32 flags,
            WbemNative.IWbemContext wbemContext,
            WbemNative.IWbemObjectSink wbemSink)
        {
            return (int)WbemNative.WbemStatus.WBEM_E_NOT_SUPPORTED;
        }
        int WbemNative.IWbemServices.ExecMethod(
            string objectPath,
            string methodName,
            Int32 flags,
            WbemNative.IWbemContext wbemContext,
            WbemNative.IWbemClassObject wbemInParams,
            ref WbemNative.IWbemClassObject wbemOutParams,
            IntPtr wbemCallResult)
        {
            return (int)WbemNative.WbemStatus.WBEM_E_NOT_SUPPORTED;
        }

        int WbemNative.IWbemServices.ExecMethodAsync(
            string objectPath,
            string methodName,
            Int32 flags,
            WbemNative.IWbemContext wbemContext,
            WbemNative.IWbemClassObject wbemInParams,
            WbemNative.IWbemObjectSink wbemSink)
        {
            if (wbemContext == null || wbemInParams == null || wbemSink == null || this.wbemServices == null)
                return (int)WbemNative.WbemStatus.WBEM_E_INVALID_PARAMETER;

            int result = (int)WbemNative.WbemStatus.WBEM_S_NO_ERROR;

            try
            {
                ObjectPathRegex objPathRegex = new ObjectPathRegex(objectPath);
                ParameterContext parms = new ParameterContext(objPathRegex.ClassName, this.wbemServices, wbemContext, wbemSink);
                WbemInstance wbemInstance = new WbemInstance(parms, objPathRegex);

                MethodContext methodContext = new MethodContext(parms, methodName, wbemInParams, wbemInstance);
                IWmiProvider wmiProvider = this.GetProvider(parms.ClassName);
                if (!wmiProvider.InvokeMethod(methodContext))
                {
                    result = (int)WbemNative.WbemStatus.WBEM_E_NOT_FOUND;
                }

                WbemException.ThrowIfFail(wbemSink.SetStatus(
                    (int)WbemNative.tag_WBEM_STATUS_TYPE.WBEM_STATUS_COMPLETE,
                    (int)result,
                    null,
                    null));
            }
            catch (WbemException e)
            {
                DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error,
                   (ushort)System.Runtime.Diagnostics.EventLogCategory.Wmi,
                   (uint)System.Runtime.Diagnostics.EventLogEventId.WmiExecMethodFailed,
                   e.ToString());
                result = e.ErrorCode;
                wbemSink.SetStatus((int)WbemNative.tag_WBEM_STATUS_TYPE.WBEM_STATUS_COMPLETE,
                    result, null, null);
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error,
                   (ushort)System.Runtime.Diagnostics.EventLogCategory.Wmi,
                   (uint)System.Runtime.Diagnostics.EventLogEventId.WmiExecMethodFailed,
                   e.ToString());
                result = (int)WbemNative.WbemStatus.WBEM_E_FAILED;
                wbemSink.SetStatus((int)WbemNative.tag_WBEM_STATUS_TYPE.WBEM_STATUS_COMPLETE,
                    (int)result, null, null);
            }
            finally
            {
                Marshal.ReleaseComObject(wbemSink);
            }
            return result;
        }

        class InstancesContext : IWmiInstances
        {
            ParameterContext parms;

            internal InstancesContext(ParameterContext parms)
            {
                this.parms = parms;
            }

            IWmiInstance IWmiInstances.NewInstance(string className)
            {
                return new InstanceContext(new WbemInstance(this.parms, className));
            }

            void IWmiInstances.AddInstance(IWmiInstance inst)
            {
                WbemException.ThrowIfFail(this.parms.WbemSink.Indicate(1,
                    new WbemNative.IWbemClassObject[] { ((InstanceContext)inst).WbemObject }));
            }
        }

        class InstanceContext : IWmiInstance
        {
            WbemInstance wbemInstance;

            internal InstanceContext(WbemInstance wbemInstance)
            {
                this.wbemInstance = wbemInstance;
            }

            internal WbemNative.IWbemClassObject WbemObject
            {
                get { return this.wbemInstance.WbemObject; }
            }

            IWmiInstance IWmiInstance.NewInstance(string className)
            {
                return new InstanceContext(new WbemInstance(this.wbemInstance, className));
            }

            object IWmiInstance.GetProperty(string name)
            {
                return wbemInstance.GetProperty(name);
            }

            void IWmiInstance.SetProperty(string name, object val)
            {
                this.wbemInstance.SetProperty(name, val);
            }
        }

        class MethodContext : IWmiMethodContext
        {
            ParameterContext parms;
            string methodName;
            WbemNative.IWbemClassObject wbemInParms;
            WbemNative.IWbemClassObject wbemOutParms;
            IWmiInstance instance;

            internal MethodContext(ParameterContext parms, string methodName, WbemNative.IWbemClassObject wbemInParms, WbemInstance wbemInstance)
            {
                this.parms = parms;
                this.methodName = methodName;
                this.wbemInParms = wbemInParms;
                this.instance = new InstanceContext(wbemInstance);

                WbemNative.IWbemClassObject wbemObject = null;
                WbemException.ThrowIfFail(parms.WbemServices.GetObject(parms.ClassName, 0, parms.WbemContext,
                    ref wbemObject, IntPtr.Zero));

                WbemNative.IWbemClassObject wbemMethod = null;
                WbemException.ThrowIfFail(wbemObject.GetMethod(methodName, 0, IntPtr.Zero, out wbemMethod));
                WbemException.ThrowIfFail(wbemMethod.SpawnInstance(0, out this.wbemOutParms));
            }

            string IWmiMethodContext.MethodName
            {
                get { return this.methodName; }
            }

            IWmiInstance IWmiMethodContext.Instance
            {
                get { return this.instance; }
            }

            object IWmiMethodContext.ReturnParameter
            {
                set
                {
                    object val = value;
                    WbemException.ThrowIfFail(this.wbemOutParms.Put("ReturnValue",
                        0, ref val, 0));
                    WbemException.ThrowIfFail(this.parms.WbemSink.Indicate(1,
                        new WbemNative.IWbemClassObject[] { this.wbemOutParms }));
                }
            }

            object IWmiMethodContext.GetParameter(string name)
            {
                Fx.Assert(null != this.wbemInParms, "");
                object val = null;
                int type = 0;
                int favor = 0;
                WbemException.ThrowIfFail(this.wbemInParms.Get(name, 0, ref val, ref type, ref favor));
                return val;
            }

            void IWmiMethodContext.SetParameter(string name, object value)
            {
                WbemException.ThrowIfFail(this.wbemOutParms.Put(name, 0, ref value, 0));
            }
        }

        class ObjectPathRegex
        {
            //No support for boolean keys
            static Regex nsRegEx = new Regex("^(?<namespace>[^\"]*?:)(?<path>.*)");
            static Regex classRegEx = new Regex("^(?<className>.*?)\\.(?<keys>.*)");
            static Regex keysRegEx = new Regex("(?<key>.*?)=((?<ival>[\\d]+)|\"(?<sval>.*?)\"),?");

            string className;
            Dictionary<string, object> keys = new Dictionary<string, object>();

            public ObjectPathRegex(string objectPath)
            {
                //WMI infrastructure will double all backslashes. We need to get back to the originals
                objectPath = objectPath.Replace("\\\\", "\\");
                Match match = nsRegEx.Match(objectPath);
                if (match.Success)
                {
                    objectPath = match.Groups["path"].Value;
                }
                match = classRegEx.Match(objectPath);
                this.className = match.Groups["className"].Value;
                string keyValues = match.Groups["keys"].Value;
                match = keysRegEx.Match(keyValues);
                if (!match.Success)
                {
                    WbemException.Throw(WbemNative.WbemStatus.WBEM_E_INVALID_OBJECT_PATH);
                }
                while (match.Success)
                {
                    if (!String.IsNullOrEmpty(match.Groups["ival"].Value))
                    {
                        this.keys.Add(match.Groups["key"].Value, Int32.Parse(match.Groups["ival"].Value, CultureInfo.CurrentCulture));
                    }
                    else
                    {
                        this.keys.Add(match.Groups["key"].Value, match.Groups["sval"].Value);
                    }
                    match = match.NextMatch();
                }
            }

            internal string ClassName { get { return this.className; } }
            internal Dictionary<string, object> Keys { get { return this.keys; } }
        }

        class QueryRegex
        {
            static Regex regEx = new Regex("\\bfrom\\b\\s+(?<className>\\w+)", RegexOptions.IgnoreCase);
            string className;

            internal QueryRegex(string query)
            {
                Match match = regEx.Match(query);
                if (!match.Success)
                {
                    WbemException.Throw(WbemNative.WbemStatus.WBEM_E_INVALID_QUERY);
                }
                this.className = match.Groups["className"].Value;
            }

            internal string ClassName { get { return this.className; } }
        }

        class ParameterContext
        {
            string className;
            WbemNative.IWbemServices wbemServices;
            WbemNative.IWbemContext wbemContext;
            WbemNative.IWbemObjectSink wbemSink;

            internal ParameterContext(
                string className,
                WbemNative.IWbemServices wbemServices,
                WbemNative.IWbemContext wbemContext,
                WbemNative.IWbemObjectSink wbemSink)
            {
                this.className = className;
                this.wbemServices = wbemServices;
                this.wbemContext = wbemContext;
                this.wbemSink = wbemSink;
            }

            internal string ClassName
            {
                get { return this.className; }
            }
            internal WbemNative.IWbemServices WbemServices
            {
                get { return this.wbemServices; }
            }
            internal WbemNative.IWbemContext WbemContext
            {
                get { return this.wbemContext; }
            }
            internal WbemNative.IWbemObjectSink WbemSink
            {
                get { return this.wbemSink; }
            }
        };

        class WbemInstance
        {
            string className;
            ParameterContext parms;
            WbemNative.IWbemClassObject wbemObject;

            internal WbemInstance(ParameterContext parms, ObjectPathRegex objPathRegex)
                : this(parms, objPathRegex.ClassName)
            {
                foreach (KeyValuePair<string, object> kv in objPathRegex.Keys)
                {
                    this.SetProperty(kv.Key, kv.Value);
                }
            }

            internal WbemInstance(WbemInstance wbemInstance, string className)
                : this(wbemInstance.parms, className)
            {
            }

            internal WbemInstance(ParameterContext parms, string className)
            {
                this.parms = parms;
                if (String.IsNullOrEmpty(className))
                {
                    className = parms.ClassName;
                }
                this.className = className;
                WbemNative.IWbemClassObject tempObj = null;
                WbemException.ThrowIfFail(
                    parms.WbemServices.GetObject(className, 0, parms.WbemContext, ref tempObj, IntPtr.Zero)
                );


                if (null != tempObj)
                {
                    WbemException.ThrowIfFail(tempObj.SpawnInstance(0, out this.wbemObject));
                }
            }

            internal WbemInstance(ParameterContext parms, WbemNative.IWbemClassObject wbemObject)
            {
                this.parms = parms;
                this.wbemObject = wbemObject;
            }

            internal WbemNative.IWbemClassObject WbemObject
            {
                get
                {
                    Fx.Assert(null != this.wbemObject, "");
                    return this.wbemObject;
                }
            }

            internal void SetProperty(string name, object val)
            {
                Fx.Assert(null != this.wbemObject, name + " may not be available to WMI");
                if (null != val)
                {
                    WbemNative.CIMTYPE type = 0;
                    if (val is DateTime)
                    {
                        val = ((DateTime)val).ToString("yyyyMMddhhmmss.ffffff", CultureInfo.InvariantCulture) + "+000";
                    }
                    else if (val is TimeSpan)
                    {
                        TimeSpan ts = (TimeSpan)val;
                        long microSeconds = (ts.Ticks % 1000) / 10;
                        val = string.Format(CultureInfo.InvariantCulture, "{0:00000000}{1:00}{2:00}{3:00}.{4:000}{5:000}:000",
                            new object[] { ts.Days, ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds, microSeconds });
                    }
                    else if (val is InstanceContext)
                    {
                        InstanceContext inst = (InstanceContext)val;
                        val = inst.WbemObject;
                    }
                    else if (val is Array)
                    {
                        Array objs = (Array)val;
                        if (objs.GetLength(0) > 0 && objs.GetValue(0) is InstanceContext)
                        {
                            WbemNative.IWbemClassObject[] insts = new WbemNative.IWbemClassObject[objs.GetLength(0)];
                            for (int i = 0; i < insts.Length; ++i)
                            {
                                insts[i] = ((InstanceContext)objs.GetValue(i)).WbemObject;
                            }
                            val = insts;
                        }
                    }
                    else if (val is Int64)
                    {
                        val = ((Int64)val).ToString(CultureInfo.InvariantCulture);
                        type = WbemNative.CIMTYPE.CIM_SINT64;
                    }

                    int hResult = this.wbemObject.Put(name, 0, ref val, (int)type);
                    if ((int)WbemNative.WbemStatus.WBEM_E_TYPE_MISMATCH == hResult || (int)WbemNative.WbemStatus.WBEM_E_NOT_FOUND == hResult)
                    {
                        //This would be most likely a product bug (somebody changed type without updating MOF), improper installation or tampering with MOF
                        System.Runtime.Diagnostics.EventLogEventId eventId;
                        if ((int)WbemNative.WbemStatus.WBEM_E_TYPE_MISMATCH == hResult)
                        {
                            eventId = System.Runtime.Diagnostics.EventLogEventId.WmiAdminTypeMismatch;
                        }
                        else
                        {
                            eventId = System.Runtime.Diagnostics.EventLogEventId.WmiPropertyMissing;
                        }
                        DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error,
                            (ushort)System.Runtime.Diagnostics.EventLogCategory.Wmi,
                            (uint)eventId,
                            this.className,
                            name,
                            val.GetType().ToString());
                    }
                    else
                    {
                        WbemException.ThrowIfFail(hResult);
                    }
                }
            }

            internal object GetProperty(string name)
            {
                object val = null;
                int type = 0;
                int favor = 0;
                WbemException.ThrowIfFail(this.wbemObject.Get(name, 0, ref val, ref type, ref favor));
                return val;
            }

            internal void Indicate()
            {
                WbemException.ThrowIfFail(this.parms.WbemSink.Indicate(1,
                    new WbemNative.IWbemClassObject[] { this.wbemObject }));
            }
        }

        class ThreadJob : IDisposable
        {
            WaitCallback callback;
            object state;
            ManualResetEvent evtDone = new ManualResetEvent(false);
            Exception exception = null;

            public ThreadJob(WaitCallback callback, object state)
            {
                this.callback = callback;
                this.state = state;
            }

            public void Run()
            {
                try
                {
                    this.callback(this.state);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    this.exception = e;
                }
                finally
                {
                    this.evtDone.Set();
                }
            }

            public Exception Wait()
            {
                this.evtDone.WaitOne();
                return exception;
            }

            public void Dispose()
            {
                if (null != this.evtDone)
                {
                    this.evtDone.Close();
                    this.evtDone = null;
                }
            }
        }

        internal static void MTAExecute(WaitCallback callback, object state)
        {
            if (Thread.CurrentThread.GetApartmentState() != ApartmentState.MTA)
            {
                using (ThreadJob job = new ThreadJob(callback, state))
                {
                    Thread thread = new Thread(new ThreadStart(job.Run));
                    thread.SetApartmentState(ApartmentState.MTA);
                    thread.IsBackground = true;
                    thread.Start();
                    Exception exception = job.Wait();
                    if (null != exception)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ApplicationException(SR.GetString(SR.AdminMTAWorkerThreadException), exception));
                    }
                }
            }
            else
            {
                callback(state);
            }
        }

        class NoInstanceWMIProvider : IWmiProvider
        {
            static NoInstanceWMIProvider singleton;
            internal static NoInstanceWMIProvider Default
            {
                get
                {
                    if (null == singleton)
                    {
                        singleton = new NoInstanceWMIProvider();
                    }
                    return singleton;
                }
            }

            void IWmiProvider.EnumInstances(IWmiInstances instances) { }
            bool IWmiProvider.GetInstance(IWmiInstance instance) { return false; }
            bool IWmiProvider.PutInstance(IWmiInstance instance) { return false; }
            bool IWmiProvider.DeleteInstance(IWmiInstance instance) { return false; }
            bool IWmiProvider.InvokeMethod(IWmiMethodContext method) { return false; }
        }
    }

    internal interface IWmiProvider
    {
        //methods with return value should return false if instance is not found
        void EnumInstances(IWmiInstances instances);
        bool GetInstance(IWmiInstance instance);
        bool PutInstance(IWmiInstance instance);
        bool DeleteInstance(IWmiInstance instance);
        bool InvokeMethod(IWmiMethodContext method);
    }

    internal interface IWmiInstances
    {
        IWmiInstance NewInstance(string className);
        void AddInstance(IWmiInstance inst);
    }

    internal interface IWmiInstance
    {
        IWmiInstance NewInstance(string className);
        object GetProperty(string name);
        void SetProperty(string name, object value);
    }

    internal interface IWmiMethodContext
    {
        string MethodName { get; }
        IWmiInstance Instance { get; }
        object ReturnParameter { set; }
        object GetParameter(string name);
        void SetParameter(string name, object value);
    }

    internal interface IWmiInstanceProvider
    {
        string GetInstanceType();
        void FillInstance(IWmiInstance wmiInstance);
    }
}
