// Copyright (c) Microsoft Corp., 2004. All rights reserved.
#region Using directives

using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Workflow.Runtime;
using System.Workflow.Runtime.Hosting;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Design;
using System.Workflow.ComponentModel.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Remoting.Channels.Ipc;
using System.Configuration;
using System.Security.Permissions;
using System.Globalization;
using Microsoft.Win32;
using System.Security.AccessControl;
using System.Security.Principal;
#endregion

namespace System.Workflow.Runtime.DebugEngine
{
    internal static class RegistryKeys
    {
        internal static readonly string ProductRootRegKey = @"SOFTWARE\Microsoft\Net Framework Setup\NDP\v3.0\Setup\Windows Workflow Foundation";
        internal static readonly string DebuggerSubKey = ProductRootRegKey + @"\Debugger";
    }

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class DebugController : MarshalByRefObject
    {
        #region Data members

        private Guid programId;
        private string hostName;
        private int attachTimeout;
        private ProgramPublisher programPublisher;
        private DebugControllerThread debugControllerThread;
        private Timer attachTimer;
        private WorkflowRuntime serviceContainer;
        private IpcChannel channel;
        private IWorkflowDebugger controllerConduit;
        private bool isZombie;
        private bool isAttached;
        private ManualResetEvent eventConduitAttached;
        private InstanceTable instanceTable;
        private Dictionary<Type, Guid> typeToGuid;
        private Dictionary<byte[], Guid> xomlHashToGuid;
        bool isServiceContainerStarting;
        private const string rootExecutorGuid = "98fcdc7a-8ab4-4fb7-92d4-20f437285729";
        private object eventLock;
        private object syncRoot = new object();
        private static readonly string ControllerConduitTypeName = "ControllerConduitTypeName";

        #endregion

        #region Security related methods
        private delegate void ExceptionNotification(Exception e);

        internal static void InitializeProcessSecurity()
        {
            // Spawn off a separate thread to that does RevertToSelf and adjusts DACLs.
            // This is because RevertToSelf terminates client impersonation on the thread
            // that calls it. We do not want to change that on the current thread when 
            // the runtime is hosted inside ASP.net for example.
            Exception workerThreadException = null;
            ProcessSecurity processSecurity = new ProcessSecurity();
            Thread workerThread = new Thread(new ThreadStart(processSecurity.Initialize));

            processSecurity.exceptionNotification += delegate(Exception e)
            {
                workerThreadException = e;
            };

            workerThread.Start(); workerThread.Join();
            if (workerThreadException != null)
                throw workerThreadException;
        }

        private class ProcessSecurity
        {
            internal ExceptionNotification exceptionNotification;

            internal void Initialize()
            {
                try
                {
                    // This is needed if the thread calling the method is impersonating
                    // a client call (ASP.net hosting scenarios).
                    if (!NativeMethods.RevertToSelf())
                        Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

                    // Get the DACL for process token. Add TOKEN_QUERY permissions for the Administrators group.
                    // Set the updated DACL for process token.
                    RawAcl tokenDacl = GetCurrentProcessTokenDacl();
                    CommonAce adminsGroupAceForToken = new CommonAce(AceFlags.None, AceQualifier.AccessAllowed, NativeMethods.TOKEN_QUERY, new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null), false, null);
                    int i = FindIndexInDacl(adminsGroupAceForToken, tokenDacl);
                    if (i != -1)
                        tokenDacl.InsertAce(i, adminsGroupAceForToken);
                    SetCurrentProcessTokenDacl(tokenDacl);
                }
                catch (Exception e)
                {
                    // Communicate any exceptions from this thread back to the thread
                    // that spawned it.
                    if (exceptionNotification != null)
                        exceptionNotification(e);
                }
            }

            private RawAcl GetCurrentProcessTokenDacl()
            {
                IntPtr hProcess = IntPtr.Zero;
                IntPtr hProcessToken = IntPtr.Zero;
                IntPtr securityDescriptorPtr = IntPtr.Zero;

                try
                {
                    hProcess = NativeMethods.GetCurrentProcess();

                    if (!NativeMethods.OpenProcessToken(hProcess, NativeMethods.TOKEN_ALL_ACCESS, out hProcessToken))
                        Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

                    // Get security descriptor associated with the kernel object, read the DACL and return
                    // that to the caller.
                    uint returnLength;

                    NativeMethods.GetKernelObjectSecurity(hProcessToken, NativeMethods.SECURITY_INFORMATION.DACL_SECURITY_INFORMATION, IntPtr.Zero, 0, out returnLength);
                    int lasterror = Marshal.GetLastWin32Error(); //#pragma warning disable 56523 doesnt recognize 56523

                    securityDescriptorPtr = Marshal.AllocCoTaskMem((int)returnLength);

                    if (!NativeMethods.GetKernelObjectSecurity(hProcessToken, NativeMethods.SECURITY_INFORMATION.DACL_SECURITY_INFORMATION, securityDescriptorPtr, returnLength, out returnLength))
                        Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

                    byte[] sdBytes = new byte[returnLength];
                    Marshal.Copy(securityDescriptorPtr, sdBytes, 0, (int)returnLength);

                    RawSecurityDescriptor rawSecurityDescriptor = new RawSecurityDescriptor(sdBytes, 0);

                    return rawSecurityDescriptor.DiscretionaryAcl;
                }
                finally
                {
                    if (hProcess != IntPtr.Zero && hProcess != (IntPtr)(-1))
                        if (!NativeMethods.CloseHandle(hProcess))
                            Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

                    if (hProcessToken != IntPtr.Zero)
                        if (!NativeMethods.CloseHandle(hProcessToken))
                            Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

                    if (securityDescriptorPtr != IntPtr.Zero)
                        Marshal.FreeCoTaskMem(securityDescriptorPtr);
                }
            }
            private void SetCurrentProcessTokenDacl(RawAcl dacl)
            {
                IntPtr hProcess = IntPtr.Zero;
                IntPtr hProcessToken = IntPtr.Zero;
                IntPtr securityDescriptorPtr = IntPtr.Zero;
                try
                {
                    hProcess = NativeMethods.GetCurrentProcess();

                    if (!NativeMethods.OpenProcessToken(hProcess, NativeMethods.TOKEN_ALL_ACCESS, out hProcessToken))
                        Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

                    // Get security descriptor associated with the kernel object and modify it.
                    uint returnLength;

                    NativeMethods.GetKernelObjectSecurity(hProcessToken, NativeMethods.SECURITY_INFORMATION.DACL_SECURITY_INFORMATION, IntPtr.Zero, 0, out returnLength);
                    int lasterror = Marshal.GetLastWin32Error(); //#pragma warning disable 56523 doesnt recognize 56523

                    securityDescriptorPtr = Marshal.AllocCoTaskMem((int)returnLength);

                    if (!NativeMethods.GetKernelObjectSecurity(hProcessToken, NativeMethods.SECURITY_INFORMATION.DACL_SECURITY_INFORMATION, securityDescriptorPtr, returnLength, out returnLength))
                        Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

                    byte[] sdBytes = new byte[returnLength];
                    Marshal.Copy(securityDescriptorPtr, sdBytes, 0, (int)returnLength);

                    RawSecurityDescriptor rawSecurityDescriptor = new RawSecurityDescriptor(sdBytes, 0);
                    rawSecurityDescriptor.DiscretionaryAcl = dacl;

                    sdBytes = new byte[rawSecurityDescriptor.BinaryLength];
                    rawSecurityDescriptor.GetBinaryForm(sdBytes, 0);
                    Marshal.FreeCoTaskMem(securityDescriptorPtr);
                    securityDescriptorPtr = Marshal.AllocCoTaskMem(rawSecurityDescriptor.BinaryLength);
                    Marshal.Copy(sdBytes, 0, securityDescriptorPtr, rawSecurityDescriptor.BinaryLength);

                    if (!NativeMethods.SetKernelObjectSecurity(hProcessToken, NativeMethods.SECURITY_INFORMATION.DACL_SECURITY_INFORMATION, securityDescriptorPtr))
                        Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                }
                finally
                {
                    if (hProcess != IntPtr.Zero && hProcess != (IntPtr)(-1))
                        if (!NativeMethods.CloseHandle(hProcess))
                            Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

                    if (hProcessToken != IntPtr.Zero)
                        if (!NativeMethods.CloseHandle(hProcessToken))
                            Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

                    if (securityDescriptorPtr != IntPtr.Zero)
                        Marshal.FreeCoTaskMem(securityDescriptorPtr);

                }
            }

            // The preferred order in which ACEs are added to DACLs is
            // documented here: http://search.msdn.microsoft.com/search/results.aspx?qu=Order+of+ACEs+in+a+DACL&View=msdn&st=b.
            // This routine follows that logic to determine the position of an ACE in the DACL.
            private int FindIndexInDacl(CommonAce newAce, RawAcl dacl)
            {
                int i = 0;
                for (i = 0; i < dacl.Count; i++)
                {
                    if (dacl[i] is CommonAce && ((CommonAce)dacl[i]).SecurityIdentifier.Value == newAce.SecurityIdentifier.Value && dacl[i].AceType == newAce.AceType)
                    {
                        i = -1;
                        break;
                    }

                    if (newAce.AceType == AceType.AccessDenied && dacl[i].AceType == AceType.AccessDenied && !newAce.IsInherited && !dacl[i].IsInherited)
                        continue;

                    if (newAce.AceType == AceType.AccessDenied && !newAce.IsInherited)
                        break;

                    if (newAce.AceType == AceType.AccessAllowed && dacl[i].AceType == AceType.AccessAllowed && !newAce.IsInherited && !dacl[i].IsInherited)
                        continue;

                    if (newAce.AceType == AceType.AccessAllowed && !newAce.IsInherited)
                        break;

                }

                return i;
            }
        }
        #endregion

        #region Constructor and Lifetime methods

        internal DebugController(WorkflowRuntime serviceContainer, string hostName)
        {
            if (serviceContainer == null)
                throw new ArgumentNullException("serviceContainer");

            try
            {
                this.programPublisher = new ProgramPublisher();
            }
            catch
            {
                // If we are unable to create the ProgramPublisher, this means that VS does not exist on this machine, so we can't debug.
                return;
            }

            this.serviceContainer = serviceContainer;
            this.programId = Guid.Empty;
            this.controllerConduit = null;
            this.channel = null;
            this.isZombie = false;
            this.hostName = hostName;

            AppDomain.CurrentDomain.ProcessExit += OnDomainUnload;
            AppDomain.CurrentDomain.DomainUnload += OnDomainUnload;

            this.serviceContainer.Started += this.Start;
            this.serviceContainer.Stopped += this.Stop;
        }

        public override object InitializeLifetimeService()
        {
            // We can't use a sponser because VS doesn't like to be attached when the lease renews itself - the 
            // debugee gets an Access Violation and VS freezes. Returning null implies that the proxy shim will be 
            // deleted only when the App Domain unloads. However, we will have disconnected the shim so no 
            // one will be able to attach to it and the same proxy is used everytime a debugger attaches.
            return null;
        }

        #endregion

        #region Attach and Detach methods

        internal void Attach(Guid programId, int attachTimeout, int detachPingInterval, out string hostName, out string uri, out int controllerThreadId, out bool isSynchronousAttach)
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "WDE: DebugController.Attach(): programId = {0}", programId));
            lock (this.syncRoot)
            {
                hostName = String.Empty;
                uri = String.Empty;
                controllerThreadId = 0;
                isSynchronousAttach = false;

                // Race condition:
                // During the call to Attach() if Uninitialize() is also called, we should ignore the call to Attach() and
                // just return. The Zombie flag and lock(this) help us recognize the ----.
                if (this.isZombie)
                    return;


                // Race condition:
                // The isAttached flat along with lock(this) catch the ---- where a debugger may have detached which
                // we haven't detected yet and another debugger may have attached, so we force detach from the first
                // debugger.
                if (this.isAttached)
                    Detach();


                this.isAttached = true;

                this.programId = programId;
                this.debugControllerThread = new DebugControllerThread();
                this.instanceTable = new InstanceTable(this.debugControllerThread.ManagedThreadId);
                this.typeToGuid = new Dictionary<Type, Guid>();
                this.xomlHashToGuid = new Dictionary<byte[], Guid>((IEqualityComparer<byte[]>)new DigestComparer());

                this.debugControllerThread.RunThread(this.instanceTable);

                // Publish our MBR object.
                IDictionary providerProperties = new Hashtable();
                providerProperties["typeFilterLevel"] = "Full";
                BinaryServerFormatterSinkProvider sinkProvider = new BinaryServerFormatterSinkProvider(providerProperties, null);

                Hashtable channelProperties = new Hashtable();
                channelProperties["name"] = string.Empty;
                channelProperties["portName"] = this.programId.ToString();
                SecurityIdentifier si = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
                IdentityReference idRef = si.Translate(typeof(NTAccount));
                channelProperties["authorizedGroup"] = idRef.ToString();
                this.channel = new IpcChannel(channelProperties, null, sinkProvider);
                ChannelServices.RegisterChannel(this.channel, true);

                ObjRef o = RemotingServices.Marshal(this, this.programId.ToString());
                hostName = this.hostName;

                uri = this.channel.GetUrlsForUri(this.programId.ToString())[0];
                controllerThreadId = this.debugControllerThread.ThreadId;
                isSynchronousAttach = !this.isServiceContainerStarting;

                this.attachTimeout = attachTimeout;
                this.attachTimer = new Timer(AttachTimerCallback, null, attachTimeout, detachPingInterval);
            }
        }

        private void Detach()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "WDE: DebugController.Detach():"));

            using (new DebuggerThreadMarker())
            {
                lock (this.syncRoot)
                {

                    AppDomain.CurrentDomain.AssemblyLoad -= OnAssemblyLoad;

                    // See comments in Attach().
                    if (this.isZombie || !this.isAttached)
                        return;

                    this.isAttached = false;

                    // Undone: AkashS - At this point wait for all event handling to complete to avoid exceptions.

                    this.programId = Guid.Empty;

                    if (this.debugControllerThread != null)
                    {
                        this.debugControllerThread.StopThread();
                        this.debugControllerThread = null;
                    }

                    if (this.attachTimer != null)
                    {
                        this.attachTimer.Change(Timeout.Infinite, Timeout.Infinite);
                        this.attachTimer = null;
                    }

                    RemotingServices.Disconnect(this);
                    if (this.channel != null)
                    {
                        ChannelServices.UnregisterChannel(this.channel);
                        this.channel = null;
                    }

                    this.controllerConduit = null;

                    this.eventConduitAttached.Reset();
                    this.instanceTable = null;
                    this.typeToGuid = null;
                    this.xomlHashToGuid = null;

                    // Do this only after we perform the previous cleanup! Otherwise
                    // we may get exceptions from the runtime that may cause the cleanup
                    // to not happen.

                    if (!this.serviceContainer.IsZombie)
                    {
                        foreach (WorkflowInstance instance in this.serviceContainer.GetLoadedWorkflows())
                        {
                            WorkflowExecutor executor = instance.GetWorkflowResourceUNSAFE();
                            using (executor.ExecutorLock.Enter())
                            {
                                if (executor.IsInstanceValid)
                                    executor.WorkflowExecutionEvent -= OnInstanceEvent;
                            }
                        }

                        this.serviceContainer.WorkflowExecutorInitializing -= InstanceInitializing;
                        this.serviceContainer.DefinitionDispenser.WorkflowDefinitionLoaded -= ScheduleTypeLoaded;
                    }
                }
            }
        }

        private void AttachTimerCallback(object state)
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "WDE: DebugController.AttachTimerCallback():"));

            try
            {
                lock (this.syncRoot)
                {
                    // See comments in Attach().
                    if (this.isZombie || !this.isAttached)
                        return;

                    if (!Debugger.IsAttached)
                    {
                        // The debugger had attached and has now detached, so cleanup, or Attach() was called on the 
                        // Program Node, but the process of attach failed thereafter and so we were never actually 
                        // debugged.
                        this.attachTimer.Change(Timeout.Infinite, Timeout.Infinite);
                        Detach();
                    }
                }
            }
            catch
            {
                // Avoid throwing unhandled exceptions!
            }
        }

        private void OnDomainUnload(object sender, System.EventArgs e)
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "WDE: DebugController.OnDomainUnload():"));

            Stop(null, default(WorkflowRuntimeEventArgs));
        }

        #endregion

        #region Methods for the DE

        public void AttachToConduit(Uri url)
        {
            if (url == null)
                throw new ArgumentNullException("url");

            try
            {
                using (new DebuggerThreadMarker())
                {
                    try
                    {
                        RegistryKey debugEngineSubKey = Registry.LocalMachine.OpenSubKey(RegistryKeys.DebuggerSubKey);
                        if (debugEngineSubKey != null)
                        {
                            string controllerConduitTypeName = debugEngineSubKey.GetValue(ControllerConduitTypeName, String.Empty) as string;
                            if (!String.IsNullOrEmpty(controllerConduitTypeName) && Type.GetType(controllerConduitTypeName) != null)
                                this.controllerConduit = Activator.GetObject(Type.GetType(controllerConduitTypeName), url.AbsolutePath) as IWorkflowDebugger;
                        }
                    }
                    catch { }

                    if (this.controllerConduit == null)
                    {
                        const string controllerConduitTypeFormat = "Microsoft.Workflow.DebugEngine.ControllerConduit, Microsoft.Workflow.DebugController, Version={0}.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

                        // Try versions 12.0.0.0, 11.0.0.0, 10.0.0.0
                        Type controllerConduitType = null;
                        for (int version = 12; controllerConduitType == null && version >= 10; --version)
                        {
                            try
                            {
                                controllerConduitType = Type.GetType(string.Format(CultureInfo.InvariantCulture, controllerConduitTypeFormat, version));
                            }
                            catch (TypeLoadException)
                            {
                                // Fall back to next-lower version
                            }
                        }

                        if (controllerConduitType != null)
                        {
                            this.controllerConduit = Activator.GetObject(controllerConduitType, url.AbsolutePath) as IWorkflowDebugger;
                        }
                    }
                    Debug.Assert(this.controllerConduit != null, "Failed to create Controller Conduit");
                    if (this.controllerConduit == null)
                        return;

                    this.eventLock = new object();

                    // Race Condition:
                    // We hook up to the AssemblyLoad event, the Schedule events and Instance events handler 
                    // before we iterate over all loaded assemblies. This means that we need to deal with duplicates in the 
                    // debugee.

                    // Race Condition:
                    // Further the order in which we hook up handlers/iterate is important to avoid ----s if the events fire
                    // before the iterations complete. We need to hook and iterate over the assemblies, then the schedule 
                    // types and finally the instances. This guarantees that we always have all the assemblies when we load
                    // schedules types and we always have all the schedule types when we load instances.

                    AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
                    foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        if (!assembly.IsDynamic
                            && !(assembly is System.Reflection.Emit.AssemblyBuilder)
                            && !(string.IsNullOrEmpty(assembly.Location)))
                        {
                            this.controllerConduit.AssemblyLoaded(this.programId, assembly.Location, assembly.GlobalAssemblyCache);
                        }
                    }
                    this.serviceContainer.DefinitionDispenser.WorkflowDefinitionLoaded += ScheduleTypeLoaded;

                    // In here we load all schedule types defined as they are - with no dynamic updates
                    ReadOnlyCollection<Type> types;
                    ReadOnlyCollection<Activity> values;
                    this.serviceContainer.DefinitionDispenser.GetWorkflowTypes(out types, out values);
                    for (int i = 0; i < types.Count; i++)
                    {
                        Type scheduleType = types[i];
                        Activity rootActivity = values[i];
                        LoadExistingScheduleType(GetScheduleTypeId(scheduleType), scheduleType, false, rootActivity);
                    }

                    ReadOnlyCollection<byte[]> keys;
                    this.serviceContainer.DefinitionDispenser.GetWorkflowDefinitions(out keys, out values);
                    for (int i = 0; i < keys.Count; i++)
                    {
                        byte[] scheduleDefHash = keys[i];
                        Activity rootActivity = values[i];
                        Activity workflowDefinition = (Activity)rootActivity.GetValue(Activity.WorkflowDefinitionProperty);
                        ArrayList changeActions = null;
                        if (workflowDefinition != null)
                            changeActions = (ArrayList)workflowDefinition.GetValue(WorkflowChanges.WorkflowChangeActionsProperty);
                        LoadExistingScheduleType(GetScheduleTypeId(scheduleDefHash), rootActivity.GetType(), (changeActions != null && changeActions.Count != 0), rootActivity);
                    }

                    this.serviceContainer.WorkflowExecutorInitializing += InstanceInitializing;

                    foreach (WorkflowInstance instance in this.serviceContainer.GetLoadedWorkflows())
                    {
                        WorkflowExecutor executor = instance.GetWorkflowResourceUNSAFE();
                        using (executor.ExecutorLock.Enter())
                        {
                            LoadExistingInstance(instance, true);
                        }
                    }

                    this.eventConduitAttached.Set();
                }
            }
            catch (Exception e)
            {
                // Don't throw exceptions to the Runtime. Ignore exceptions that may occur if the debugger detaches 
                // and closes the remoting channel.
                Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "WDE: Failure in DebugController.AttachToConduit: {0}, Call stack:{1}", e.Message, e.StackTrace));
            }

        }

        #endregion

        #region Methods for the Runtime

        private void Start(object source, WorkflowRuntimeEventArgs e)
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "WDE: DebugController.ServiceContainerStarted():"));

            this.isZombie = false;
            this.isAttached = false;
            this.eventConduitAttached = new ManualResetEvent(false);
            this.isServiceContainerStarting = true;

            bool published = this.programPublisher.Publish(this);

            // If the debugger is already attached, then the DE will invoke AttachToConduit() on a separate thread.
            // We need to wait for that to happen to prevent new instances being created and causing a ----. See
            // comments in ControllerConduit.ProgramCreated(). However, if the DE never calls AttachToConduit(),
            // and the detaches instead, we set a wait timeout to that of our Attach Timer.
            // Note that when we publish the program node, if the debugger is attached, isAttached will be set to true
            // when the debugger calls Attach() on the Program Node!

            while (published && this.isAttached && !this.eventConduitAttached.WaitOne(attachTimeout, false));
            this.isServiceContainerStarting = false;
        }

        private void Stop(object source, WorkflowRuntimeEventArgs e)
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "WDE: DebugController.ServiceContainerStopped():"));

            try
            {
                lock (this.syncRoot)
                {
                    Detach();
                    this.programPublisher.Unpublish();

                    // See comments in Attach().
                    this.isZombie = true;
                }
            }
            catch
            {
                // Do not throw exceptions back!
            }
        }

        internal void Close()
        {
            //Unregister from Appdomain event to remove ourselves from GCRoot.
            AppDomain.CurrentDomain.ProcessExit -= OnDomainUnload;
            AppDomain.CurrentDomain.DomainUnload -= OnDomainUnload;

            if (!this.isZombie)
            {
                Stop(null, new WorkflowRuntimeEventArgs(false));
            }
        }


        private void OnInstanceEvent(object sender, WorkflowExecutor.WorkflowExecutionEventArgs e)
        {
            switch (e.EventType)
            {
                case WorkflowEventInternal.Completed:
                    InstanceCompleted(sender, new WorkflowEventArgs(((WorkflowExecutor)sender).WorkflowInstance));
                    break;
                case WorkflowEventInternal.Terminated:
                    InstanceTerminated(sender, new WorkflowEventArgs(((WorkflowExecutor)sender).WorkflowInstance));
                    break;
                case WorkflowEventInternal.Unloaded:
                    InstanceUnloaded(sender, new WorkflowEventArgs(((WorkflowExecutor)sender).WorkflowInstance));
                    break;
                case WorkflowEventInternal.Changed:
                    OnWorkflowChanged(sender, e);
                    break;
                case WorkflowEventInternal.HandlerInvoking:
                    OnHandlerInvoking(sender, e);
                    break;
                case WorkflowEventInternal.HandlerInvoked:
                    OnHandlerInvoked(sender, e);
                    break;
                case WorkflowEventInternal.ActivityStatusChange:
                    OnActivityStatusChanged(sender, (WorkflowExecutor.ActivityStatusChangeEventArgs)e);
                    break;
                case WorkflowEventInternal.ActivityExecuting:
                    OnActivityExecuting(sender, (WorkflowExecutor.ActivityExecutingEventArgs)e);
                    break;
                default:
                    return;
            }
        }


        private void InstanceInitializing(object sender, WorkflowRuntime.WorkflowExecutorInitializingEventArgs e)
        {
            try
            {
                if (e.Loading)
                    LoadExistingInstance(((WorkflowExecutor)sender).WorkflowInstance, true);
                else
                    LoadExistingInstance(((WorkflowExecutor)sender).WorkflowInstance, false);
            }
            catch
            {
                // Don't throw exceptions to the Runtime. Ignore exceptions that may occur if the debugger detaches 
                // and closes the remoting channel.
            }
        }

        private void InstanceCompleted(object sender, WorkflowEventArgs args)
        {
            try
            {
                UnloadExistingInstance(args.WorkflowInstance);
            }
            catch
            {
                // Don't throw exceptions to the Runtime. Ignore exceptions that may occur if the debugger detaches 
                // and closes the remoting channel.
            }
        }

        private void InstanceTerminated(object sender, WorkflowEventArgs args)
        {
            try
            {
                UnloadExistingInstance(args.WorkflowInstance);
            }
            catch
            {
                // Don't throw exceptions to the Runtime. Ignore exceptions that may occur if the debugger detaches 
                // and closes the remoting channel.
            }
        }

        private void InstanceUnloaded(object sender, WorkflowEventArgs args)
        {
            // Treat this as if the instance completed so that it won't show up in the UI anymore.
            try
            {
                UnloadExistingInstance(args.WorkflowInstance);
            }
            catch
            {
                // Don't throw exceptions to the Runtime. Ignore exceptions that may occur if the debugger detaches 
                // and closes the remoting channel.
            }
        }

        private void ScheduleTypeLoaded(object sender, WorkflowDefinitionEventArgs args)
        {
            try
            {
                if (args.WorkflowType != null)
                {
                    Activity rootActivity = ((WorkflowRuntime)sender).DefinitionDispenser.GetWorkflowDefinition(args.WorkflowType);
                    LoadExistingScheduleType(GetScheduleTypeId(args.WorkflowType), args.WorkflowType, false, rootActivity);
                }
                else
                {
                    Activity rootActivity = ((WorkflowRuntime)sender).DefinitionDispenser.GetWorkflowDefinition(args.WorkflowDefinitionHashCode);
                    LoadExistingScheduleType(GetScheduleTypeId(args.WorkflowDefinitionHashCode), rootActivity.GetType(), false, rootActivity);
                }
            }
            catch
            {
                // Don't throw exceptions to the Runtime. Ignore exceptions that may occur if the debugger detaches 
                // and closes the remoting channel.
            }
        }

        private void OnActivityExecuting(object sender, WorkflowExecutor.ActivityExecutingEventArgs eventArgs)
        {
            if (this.isZombie || !this.isAttached)
                return;

            try
            {
                lock (this.eventLock)
                {
                    IWorkflowCoreRuntime workflowCoreRuntime = (IWorkflowCoreRuntime)sender;
                    Guid scheduleTypeId = GetScheduleTypeId(workflowCoreRuntime);

                    // When the activity starts executing, update its handler list for stepping.
                    EnumerateEventHandlersForActivity(scheduleTypeId, eventArgs.Activity);
                    this.controllerConduit.BeforeActivityStatusChanged(this.programId, scheduleTypeId, workflowCoreRuntime.InstanceID, eventArgs.Activity.QualifiedName, GetHierarchicalId(eventArgs.Activity), eventArgs.Activity.ExecutionStatus, GetContextId(eventArgs.Activity));
                    this.controllerConduit.ActivityStatusChanged(this.programId, scheduleTypeId, workflowCoreRuntime.InstanceID, eventArgs.Activity.QualifiedName, GetHierarchicalId(eventArgs.Activity), eventArgs.Activity.ExecutionStatus, GetContextId(eventArgs.Activity));
                }
            }
            catch
            {
                // Don't throw exceptions to the Runtime. Ignore exceptions that may occur if the debugger detaches 
                // and closes the remoting channel.
            }
        }

        private void OnActivityStatusChanged(object sender, WorkflowExecutor.ActivityStatusChangeEventArgs eventArgs)
        {
            if (this.isZombie || !this.isAttached)
                return;

            try
            {
                lock (this.eventLock)
                {
                    // We will receive an event when Activity.Execute() is about to be called.
                    if (eventArgs.Activity.ExecutionStatus == ActivityExecutionStatus.Executing)
                        return;

                    IWorkflowCoreRuntime workflowCoreRuntime = (IWorkflowCoreRuntime)sender;
                    Guid scheduleTypeId = GetScheduleTypeId(workflowCoreRuntime);

                    // When the activity starts executing, update its handler list for stepping.
                    if (eventArgs.Activity.ExecutionStatus == ActivityExecutionStatus.Executing)
                        EnumerateEventHandlersForActivity(scheduleTypeId, eventArgs.Activity);

                    this.controllerConduit.BeforeActivityStatusChanged(this.programId, scheduleTypeId, workflowCoreRuntime.InstanceID, eventArgs.Activity.QualifiedName, GetHierarchicalId(eventArgs.Activity), eventArgs.Activity.ExecutionStatus, GetContextId(eventArgs.Activity));
                    this.controllerConduit.ActivityStatusChanged(this.programId, scheduleTypeId, workflowCoreRuntime.InstanceID, eventArgs.Activity.QualifiedName, GetHierarchicalId(eventArgs.Activity), eventArgs.Activity.ExecutionStatus, GetContextId(eventArgs.Activity));
                }
            }
            catch
            {
                // Don't throw exceptions to the Runtime. Ignore exceptions that may occur if the debugger detaches 
                // and closes the remoting channel.
            }
        }

        private void OnHandlerInvoking(object sender, EventArgs eventArgs)
        {
            // Undone: AkashS - We need to remove EnumerateHandlersForActivity() and set the CPDE
            // breakpoints from here. This is handle the cases where event handlers are modified
            // at runtime.
        }

        private void OnHandlerInvoked(object sender, EventArgs eventArgs)
        {
            if (this.isZombie || !this.isAttached)
                return;

            try
            {
                lock (this.eventLock)
                {
                    IWorkflowCoreRuntime workflowCoreRuntime = sender as IWorkflowCoreRuntime;
                    this.controllerConduit.HandlerInvoked(this.programId, workflowCoreRuntime.InstanceID, NativeMethods.GetCurrentThreadId(), GetHierarchicalId(workflowCoreRuntime.CurrentActivity));
                }
            }
            catch
            {
                // Don't throw exceptions to the Runtime. Ignore exceptions that may occur if the debugger detaches 
                // and closes the remoting channel.
            }
        }

        private void OnWorkflowChanged(object sender, EventArgs eventArgs)
        {
            if (this.isZombie || !this.isAttached)
                return;

            try
            {
                lock (this.eventLock)
                {
                    IWorkflowCoreRuntime workflowCoreRuntime = (IWorkflowCoreRuntime)sender;

                    // Get cached old root activity.
                    Activity oldRootActivity = this.instanceTable.GetRootActivity(workflowCoreRuntime.InstanceID);

                    Guid scheduleTypeId = workflowCoreRuntime.InstanceID; // From now on we will treat the instance id as a dynamic schedule type id.
                    LoadExistingScheduleType(scheduleTypeId, oldRootActivity.GetType(), true, oldRootActivity);

                    // And now reload the instance.
                    this.instanceTable.UpdateRootActivity(workflowCoreRuntime.InstanceID, oldRootActivity);

                    // The DE will update the schedule type on the thread that is running the instance.
                    // DE should be called after the instance table entry is replaced.
                    this.controllerConduit.InstanceDynamicallyUpdated(this.programId, workflowCoreRuntime.InstanceID, scheduleTypeId);
                }
            }
            catch
            {
                // Don't throw exceptions to the Runtime. Ignore exceptions that may occur if the debugger detaches 
                // and closes the remoting channel.
            }
        }

        #endregion

        #region Helper methods and properties

        // Callers of this method should acquire the executor lock only if they 
        // are not being called in the runtime thread.(
        private void LoadExistingInstance(WorkflowInstance instance, bool attaching)
        {
            WorkflowExecutor executor = instance.GetWorkflowResourceUNSAFE();
            if (!executor.IsInstanceValid)
                return;
            IWorkflowCoreRuntime runtimeService = (IWorkflowCoreRuntime)executor;
            Activity rootActivity = runtimeService.RootActivity;
            Guid scheduleTypeId = GetScheduleTypeId(runtimeService);

            // If we are just attaching, need to LoadExistingScheduleType with the dynamic definition and type
            // since the OnDynamicUpdateEvent has never been executed.
            if (attaching && runtimeService.IsDynamicallyUpdated)
                LoadExistingScheduleType(scheduleTypeId, rootActivity.GetType(), true, rootActivity);

            // Add to the InstanceTable before firing the DE event !
            this.instanceTable.AddInstance(instance.InstanceId, rootActivity);

            this.controllerConduit.InstanceCreated(this.programId, instance.InstanceId, scheduleTypeId);

            // Take a lock so that SetInitialActivityStatus is always called before next status events.
            lock (this.eventLock)
            {
                executor.WorkflowExecutionEvent += OnInstanceEvent;
                foreach (Activity activity in DebugController.WalkActivityTree(rootActivity))
                {
#if false
                    //
                    ReplicatorActivity replicator = activity as ReplicatorActivity;
                    if (replicator != null)
                    {
                        foreach (Activity queuedChildActivity in replicator.DynamicActivities)
                            activityQueue.Enqueue(queuedChildActivity);
                    }
                    else
#endif
                    UpdateActivityStatus(scheduleTypeId, instance.InstanceId, activity);

                }

                ActivityExecutionContext rootExecutionContext = new ActivityExecutionContext(rootActivity);
                foreach (ActivityExecutionContext executionContext in DebugController.WalkExecutionContextTree(rootExecutionContext))
                {
                    Activity instanceActivity = executionContext.Activity;
                    foreach (Activity childInstance in DebugController.WalkActivityTree(instanceActivity))
                    {
                        UpdateActivityStatus(scheduleTypeId, instance.InstanceId, childInstance);
                    }
                }
            }
        }

        private void UpdateActivityStatus(Guid scheduleTypeId, Guid instanceId, Activity activity)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");

            // first update its handler list
            if (activity.ExecutionStatus == ActivityExecutionStatus.Executing)
                EnumerateEventHandlersForActivity(scheduleTypeId, activity);

            //report only states different from the initialized
            if (activity.ExecutionStatus != ActivityExecutionStatus.Initialized)
            {
                Activity contextActivity = ContextActivityUtils.ContextActivity(activity);
                int context = ContextActivityUtils.ContextId(contextActivity);
                this.controllerConduit.SetInitialActivityStatus(this.programId, scheduleTypeId, instanceId, activity.QualifiedName, GetHierarchicalId(activity), activity.ExecutionStatus, context);
            }

        }

        private static IEnumerable WalkActivityTree(Activity rootActivity)
        {
            if (rootActivity == null || !rootActivity.Enabled)
                yield break;

            // Return self
            yield return rootActivity;

            // Go through the children as well
            if (rootActivity is CompositeActivity)
            {
                foreach (Activity childActivity in ((CompositeActivity)rootActivity).Activities)
                {
                    foreach (Activity nestedChild in WalkActivityTree(childActivity))
                        yield return nestedChild;
                }
            }
        }

        private static IEnumerable WalkExecutionContextTree(ActivityExecutionContext rootContext)
        {
            if (rootContext == null)
                yield break;

            yield return rootContext;

            foreach (ActivityExecutionContext executionContext in rootContext.ExecutionContextManager.ExecutionContexts)
            {
                foreach (ActivityExecutionContext nestedContext in WalkExecutionContextTree(executionContext))
                    yield return nestedContext;
            }
            yield break;
        }

        private void UnloadExistingInstance(WorkflowInstance instance)
        {
            // Fire DE event before removing from the InstanceTable!
            this.controllerConduit.InstanceCompleted(this.programId, instance.InstanceId);
            this.instanceTable.RemoveInstance(instance.InstanceId);
        }

        private void LoadExistingScheduleType(Guid scheduleTypeId, Type scheduleType, bool isDynamic, Activity rootActivity)
        {
            if (rootActivity == null)
                throw new InvalidOperationException();

            using (StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                using (XmlWriter xmlWriter = Helpers.CreateXmlWriter(stringWriter))
                {
                    WorkflowMarkupSerializer serializer = new WorkflowMarkupSerializer();
                    serializer.Serialize(xmlWriter, rootActivity);
                    string fileName = null;
                    string md5Digest = null;
                    Attribute[] attributes = scheduleType.GetCustomAttributes(typeof(WorkflowMarkupSourceAttribute), false) as Attribute[];
                    if (attributes != null && attributes.Length == 1)
                    {
                        fileName = ((WorkflowMarkupSourceAttribute)attributes[0]).FileName;
                        md5Digest = ((WorkflowMarkupSourceAttribute)attributes[0]).MD5Digest;
                    }

                    this.controllerConduit.ScheduleTypeLoaded(this.programId, scheduleTypeId, scheduleType.Assembly.FullName, fileName, md5Digest, isDynamic, scheduleType.FullName, scheduleType.Name, stringWriter.ToString());
                }
            }
        }

        private string GetHierarchicalId(Activity activity)
        {
            string id = string.Empty;
            while (activity != null)
            {
                string iterationId = string.Empty;

                Activity contextActivity = ContextActivityUtils.ContextActivity(activity);
                int context = ContextActivityUtils.ContextId(contextActivity);
                iterationId = activity.Name + ((context > 1 && activity == contextActivity) ? "(" + context + ")" : string.Empty);

                id = (id.Length > 0) ? iterationId + "." + id : iterationId;

                activity = activity.Parent;
            }

            return id;
        }

        private int GetContextId(Activity activity)
        {
            Activity contextActivity = ContextActivityUtils.ContextActivity(activity);
            return ContextActivityUtils.ContextId(contextActivity);
        }

        private Guid GetScheduleTypeId(IWorkflowCoreRuntime workflowCoreRuntime)
        {
            Activity rootActivity = workflowCoreRuntime.RootActivity;

            if (workflowCoreRuntime.IsDynamicallyUpdated)
                return workflowCoreRuntime.InstanceID;
            else if (string.IsNullOrEmpty(rootActivity.GetValue(Activity.WorkflowXamlMarkupProperty) as string))
                return GetScheduleTypeId(rootActivity.GetType());
            else
                return GetScheduleTypeId(rootActivity.GetValue(WorkflowDefinitionDispenser.WorkflowDefinitionHashCodeProperty) as byte[]);
        }

        private Guid GetScheduleTypeId(Type scheduleType)
        {
            // We cannot use the GUID from the type because that is not guaranteed to be unique, especially when
            // multiple versions are loaded and the stamps a GuidAttribute.
            lock (this.typeToGuid)
            {
                if (!this.typeToGuid.ContainsKey(scheduleType))
                    this.typeToGuid[scheduleType] = Guid.NewGuid();

                return (Guid)this.typeToGuid[scheduleType];
            }
        }

        private Guid GetScheduleTypeId(byte[] scheduleDefHashCode)
        {
            // We use the same hashtable to store schedule definition to Guid mapping.
            lock (this.xomlHashToGuid)
            {
                if (!this.xomlHashToGuid.ContainsKey(scheduleDefHashCode))
                    this.xomlHashToGuid[scheduleDefHashCode] = Guid.NewGuid();

                return (Guid)this.xomlHashToGuid[scheduleDefHashCode];
            }
        }

        private void OnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            // Call assembly load on the conduit for assemblies loaded from disk.
            if (args.LoadedAssembly.Location != String.Empty)
            {
                try
                {
                    this.controllerConduit.AssemblyLoaded(this.programId, args.LoadedAssembly.Location, args.LoadedAssembly.GlobalAssemblyCache);
                }
                catch
                {
                    // Don't throw exceptions to the CLR. Ignore exceptions that may occur if the debugger detaches 
                    // and closes the remoting channel.
                }
            }
        }

        private void EnumerateEventHandlersForActivity(Guid scheduleTypeId, Activity activity)
        {
            List<ActivityHandlerDescriptor> handlerMethods = new List<ActivityHandlerDescriptor>();
            MethodInfo getInvocationListMethod = activity.GetType().GetMethod("System.Workflow.ComponentModel.IDependencyObjectAccessor.GetInvocationList", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

            foreach (EventInfo eventInfo in activity.GetType().GetEvents(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
            {
                DependencyProperty dependencyEvent = DependencyProperty.FromName(eventInfo.Name, activity.GetType());

                if (dependencyEvent != null)
                {
                    try
                    {
                        MethodInfo boundGetInvocationListMethod = getInvocationListMethod.MakeGenericMethod(new Type[] { dependencyEvent.PropertyType });
                        foreach (Delegate handler in (boundGetInvocationListMethod.Invoke(activity, new object[] { dependencyEvent }) as Delegate[]))
                        {
                            MethodInfo handlerMethodInfo = handler.Method;
                            ActivityHandlerDescriptor handlerMethod;
                            handlerMethod.Name = handlerMethodInfo.DeclaringType.FullName + "." + handlerMethodInfo.Name;
                            handlerMethod.Token = handlerMethodInfo.MetadataToken;
                            handlerMethods.Add(handlerMethod);
                        }
                    }
                    catch
                    {
                    }
                }
            }

            this.controllerConduit.UpdateHandlerMethodsForActivity(this.programId, scheduleTypeId, activity.QualifiedName, handlerMethods);
        }

        #endregion
    }
}
