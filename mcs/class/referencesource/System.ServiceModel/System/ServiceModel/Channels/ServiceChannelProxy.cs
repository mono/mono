//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Proxies;
    using System.Security;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.Threading.Tasks;

    [Fx.Tag.SecurityNote(Critical = "Accesses a variety of LinkDemanded classes/methods (especially RealProxy)." +
        "Caller should protect access to the ServiceChannelProxy instance after construction.")]
#pragma warning disable 618 // have not moved to the v4 security model yet
    [SecurityCritical(SecurityCriticalScope.Everything)]
#pragma warning restore 618
    sealed class ServiceChannelProxy : RealProxy, IRemotingTypeInfo
    {
        const String activityIdSlotName = "E2ETrace.ActivityID";
        Type proxiedType;
        Type interfaceType;
        ServiceChannel serviceChannel;
        MbrObject objectWrapper;
        ImmutableClientRuntime proxyRuntime;
        MethodDataCache methodDataCache;

        internal ServiceChannelProxy(Type interfaceType, Type proxiedType, MessageDirection direction, ServiceChannel serviceChannel)
            : base(proxiedType)
        {
            if (!MessageDirectionHelper.IsDefined(direction))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("direction"));

            this.interfaceType = interfaceType;
            this.proxiedType = proxiedType;
            this.serviceChannel = serviceChannel;
            this.proxyRuntime = serviceChannel.ClientRuntime.GetRuntime();
            this.methodDataCache = new MethodDataCache();

            this.objectWrapper = new MbrObject(this, proxiedType);
        }

        //Workaround is to set the activityid in remoting call's LogicalCallContext
        static LogicalCallContext SetActivityIdInLogicalCallContext(LogicalCallContext logicalCallContext)
        {
            if (TraceUtility.ActivityTracing)
            {
                logicalCallContext.SetData(activityIdSlotName, DiagnosticTraceBase.ActivityId);
            }

            return logicalCallContext;
        }

        IMethodReturnMessage CreateReturnMessage(object ret, object[] returnArgs, IMethodCallMessage methodCall)
        {
            if (returnArgs != null)
            {
                return CreateReturnMessage(ret, returnArgs, returnArgs.Length, SetActivityIdInLogicalCallContext(methodCall.LogicalCallContext), methodCall);
            }
            else
            {
                return new SingleReturnMessage(ret, methodCall);
            }
        }

        IMethodReturnMessage CreateReturnMessage(object ret, object[] outArgs, int outArgsCount, LogicalCallContext callCtx, IMethodCallMessage mcm)
        {
            return new ReturnMessage(ret, outArgs, outArgsCount, callCtx, mcm);
        }

        IMethodReturnMessage CreateReturnMessage(Exception e, IMethodCallMessage mcm)
        {
            return new ReturnMessage(e, mcm);
        }

        MethodData GetMethodData(IMethodCallMessage methodCall)
        {
            MethodBase method = methodCall.MethodBase;

            MethodData methodData;
            if (methodDataCache.TryGetMethodData(method, out methodData))
            {
                return methodData;
            }

            bool canCacheMessageData;

            Type declaringType = method.DeclaringType;
            if (declaringType == typeof(object))
            {
                MethodType methodType;
                if (methodCall.MethodBase == typeof(object).GetMethod("GetType"))
                {
                    methodType = MethodType.GetType;
                }
                else
                {
                    methodType = MethodType.Object;
                }
                canCacheMessageData = true;
                methodData = new MethodData(method, methodType);
            }
            else if (declaringType.IsInstanceOfType(this.serviceChannel))
            {
                canCacheMessageData = true;
                methodData = new MethodData(method, MethodType.Channel);
            }
            else
            {
                ProxyOperationRuntime operation = this.proxyRuntime.GetOperation(method, methodCall.Args, out canCacheMessageData);

                if (operation == null)
                {
                    if (this.serviceChannel.Factory != null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SFxMethodNotSupported1, method.Name)));
                    else
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SFxMethodNotSupportedOnCallback1, method.Name)));
                }

                MethodType methodType;

                if (operation.IsTaskCall(methodCall))
                {
                    methodType = MethodType.TaskService;
                }
                else if (operation.IsSyncCall(methodCall))
                {
                    methodType = MethodType.Service;
                }
                else if (operation.IsBeginCall(methodCall))
                {
                    methodType = MethodType.BeginService;
                }
                else
                {
                    methodType = MethodType.EndService;
                }

                methodData = new MethodData(method, methodType, operation);
            }

            if (canCacheMessageData)
            {
                methodDataCache.SetMethodData(methodData);
            }

            return methodData;
        }

        internal ServiceChannel GetServiceChannel()
        {
            return this.serviceChannel;
        }

        public override IMessage Invoke(IMessage message)
        {
            try
            {
                IMethodCallMessage methodCall = message as IMethodCallMessage;

                if (methodCall == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.SFxExpectedIMethodCallMessage)));

                MethodData methodData = GetMethodData(methodCall);

                switch (methodData.MethodType)
                {
                    case MethodType.Service:
                        return InvokeService(methodCall, methodData.Operation);
                    case MethodType.BeginService:
                        return InvokeBeginService(methodCall, methodData.Operation);
                    case MethodType.EndService:
                        return InvokeEndService(methodCall, methodData.Operation);
                    case MethodType.TaskService:
                        return InvokeTaskService(methodCall, methodData.Operation);
                    case MethodType.Channel:
                        return InvokeChannel(methodCall);
                    case MethodType.GetType:
                        return InvokeGetType(methodCall);
                    case MethodType.Object:
                        return InvokeObject(methodCall);
                    default:
                        Fx.Assert("Invalid proxy method type");
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Invalid proxy method type")));
                }
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                return CreateReturnMessage(e, message as IMethodCallMessage);
            }
        }

        static class TaskCreator
        {
            static readonly Func<ServiceChannel, ProxyOperationRuntime, object[], AsyncCallback, object, IAsyncResult> beginCallDelegate = ServiceChannel.BeginCall;
            static readonly Hashtable createGenericTaskDelegateCache = new Hashtable(); // using Hashtable because it allows for lock-free reads
            static readonly MethodInfo createGenericTaskMI = typeof(TaskCreator).GetMethod("CreateGenericTask", new Type[] { typeof(ServiceChannel), typeof(ProxyOperationRuntime), typeof(object[]) });

            static Func<ServiceChannel, ProxyOperationRuntime, object[], Task> GetOrCreateTaskDelegate(Type taskResultType)
            {
                Func<ServiceChannel, ProxyOperationRuntime, object[], Task> createTaskDelegate = createGenericTaskDelegateCache[taskResultType] as Func<ServiceChannel, ProxyOperationRuntime, object[], Task>;
                if (createTaskDelegate != null)
                {
                    return createTaskDelegate;
                }

                lock (createGenericTaskDelegateCache)
                {
                    createTaskDelegate = createGenericTaskDelegateCache[taskResultType] as Func<ServiceChannel, ProxyOperationRuntime, object[], Task>;
                    if (createTaskDelegate != null)
                    {
                        return createTaskDelegate;
                    }
                    
                    MethodInfo methodInfo = createGenericTaskMI.MakeGenericMethod(taskResultType);
                    createTaskDelegate = Delegate.CreateDelegate(typeof(Func<ServiceChannel, ProxyOperationRuntime, object[], Task>), methodInfo) as Func<ServiceChannel, ProxyOperationRuntime, object[], Task>;
                    createGenericTaskDelegateCache[taskResultType] = createTaskDelegate;
                }

                return createTaskDelegate;
            }

            public static Task CreateTask(ServiceChannel channel, IMethodCallMessage methodCall, ProxyOperationRuntime operation)
            {
                if (operation.TaskTResult == ServiceReflector.VoidType)
                {
                    return TaskCreator.CreateTask(channel, operation, methodCall.InArgs);
                }
                return TaskCreator.CreateGenericTask(channel, operation, methodCall.InArgs);
            }

            static Task CreateGenericTask(ServiceChannel channel, ProxyOperationRuntime operation, object[] inputParameters)
            {
                Func<ServiceChannel, ProxyOperationRuntime, object[], Task> createTaskDelegate = GetOrCreateTaskDelegate(operation.TaskTResult);
                return createTaskDelegate(channel, operation, inputParameters);
            }

            static Task CreateTask(ServiceChannel channel, ProxyOperationRuntime operation, object[] inputParameters)
            {
                Action<IAsyncResult> endCallDelegate = (asyncResult) =>
                {
                    Fx.Assert(asyncResult != null, "'asyncResult' MUST NOT be NULL.");
                    OperationContext originalOperationContext = OperationContext.Current;
                    OperationContext.Current = asyncResult.AsyncState as OperationContext;
                    try
                    {
                        channel.EndCall(operation.Action, ProxyOperationRuntime.EmptyArray, asyncResult);
                    }
                    finally
                    {
                        OperationContext.Current = originalOperationContext;
                    }
                };

                return Task.Factory.FromAsync(beginCallDelegate, endCallDelegate, channel, operation, inputParameters, OperationContext.Current);
            }

            public static Task<T> CreateGenericTask<T>(ServiceChannel channel, ProxyOperationRuntime operation, object[] inputParameters)
            {
                Func<IAsyncResult, T> endCallDelegate = (asyncResult) =>
                {
                    OperationContext originalOperationContext = OperationContext.Current;
                    OperationContext.Current = asyncResult.AsyncState as OperationContext;
                    try
                    {
                        return (T)channel.EndCall(operation.Action, ProxyOperationRuntime.EmptyArray, asyncResult);
                    }
                    finally
                    {
                        OperationContext.Current = originalOperationContext;
                    }
                };

                return Task<T>.Factory.FromAsync<ServiceChannel, ProxyOperationRuntime, object[]>(beginCallDelegate, endCallDelegate, channel, operation, inputParameters, OperationContext.Current);
            }
        }

        IMessage InvokeTaskService(IMethodCallMessage methodCall, ProxyOperationRuntime operation)
        {
            Task task = TaskCreator.CreateTask(this.serviceChannel, methodCall, operation);
            return CreateReturnMessage(task, null, methodCall);
        }

        IMethodReturnMessage InvokeChannel(IMethodCallMessage methodCall)
        {
            string activityName = null;
            ActivityType activityType = ActivityType.Unknown;
            if (DiagnosticUtility.ShouldUseActivity)
            {
                if (ServiceModelActivity.Current == null ||
                    ServiceModelActivity.Current.ActivityType != ActivityType.Close)
                {
                    MethodData methodData = this.GetMethodData(methodCall);
                    if (methodData.MethodBase.DeclaringType == typeof(System.ServiceModel.ICommunicationObject)
                        && methodData.MethodBase.Name.Equals("Close", StringComparison.Ordinal))
                    {
                        activityName = SR.GetString(SR.ActivityClose, this.serviceChannel.GetType().FullName);
                        activityType = ActivityType.Close;
                    }
                }
            }

            using (ServiceModelActivity activity = string.IsNullOrEmpty(activityName) ? null : ServiceModelActivity.CreateBoundedActivity())
            {
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    ServiceModelActivity.Start(activity, activityName, activityType);
                }
                return ExecuteMessage(this.serviceChannel, methodCall);
            }
        }

        IMethodReturnMessage InvokeGetType(IMethodCallMessage methodCall)
        {
            return CreateReturnMessage(proxiedType, null, 0, SetActivityIdInLogicalCallContext(methodCall.LogicalCallContext), methodCall);
        }

        IMethodReturnMessage InvokeObject(IMethodCallMessage methodCall)
        {
            return RemotingServices.ExecuteMessage(this.objectWrapper, methodCall);
        }

        IMethodReturnMessage InvokeBeginService(IMethodCallMessage methodCall, ProxyOperationRuntime operation)
        {
            AsyncCallback callback;
            object asyncState;
            object[] ins = operation.MapAsyncBeginInputs(methodCall, out callback, out asyncState);
            object ret = this.serviceChannel.BeginCall(operation.Action, operation.IsOneWay, operation, ins, callback, asyncState);
            return CreateReturnMessage(ret, null, methodCall);
        }

        IMethodReturnMessage InvokeEndService(IMethodCallMessage methodCall, ProxyOperationRuntime operation)
        {
            IAsyncResult result;
            object[] outs;
            operation.MapAsyncEndInputs(methodCall, out result, out outs);
            object ret = this.serviceChannel.EndCall(operation.Action, outs, result);
            object[] returnArgs = operation.MapAsyncOutputs(methodCall, outs, ref ret);
            return CreateReturnMessage(ret, returnArgs, methodCall);
        }

        IMethodReturnMessage InvokeService(IMethodCallMessage methodCall, ProxyOperationRuntime operation)
        {
            object[] outs;
            object[] ins = operation.MapSyncInputs(methodCall, out outs);
            object ret = this.serviceChannel.Call(operation.Action, operation.IsOneWay, operation, ins, outs);
            object[] returnArgs = operation.MapSyncOutputs(methodCall, outs, ref ret);
            return CreateReturnMessage(ret, returnArgs, methodCall);
        }

        IMethodReturnMessage ExecuteMessage(object target, IMethodCallMessage methodCall)
        {
            MethodBase targetMethod = methodCall.MethodBase;

            object[] args = methodCall.Args;
            object returnValue = null;
            try
            {
                returnValue = targetMethod.Invoke(target, args);
            }
            catch (TargetInvocationException e)
            {
                return CreateReturnMessage(e.InnerException, methodCall);
            }

            return CreateReturnMessage(returnValue,
                                       args,
                                       args.Length,
                                       null,
                                       methodCall);
        }

        bool IRemotingTypeInfo.CanCastTo(Type toType, object o)
        {
            return toType.IsAssignableFrom(proxiedType) || serviceChannel.CanCastTo(toType);
        }

        string IRemotingTypeInfo.TypeName
        {
            get { return proxiedType.FullName; }
            set { }
        }

        class MethodDataCache
        {
            MethodData[] methodDatas;

            public MethodDataCache()
            {
                this.methodDatas = new MethodData[4];
            }

            object ThisLock
            {
                get { return this; }
            }

            public bool TryGetMethodData(MethodBase method, out MethodData methodData)
            {
                lock (ThisLock)
                {
                    MethodData[] methodDatas = this.methodDatas;
                    int index = FindMethod(methodDatas, method);
                    if (index >= 0)
                    {
                        methodData = methodDatas[index];
                        return true;
                    }
                    else
                    {
                        methodData = new MethodData();
                        return false;
                    }
                }
            }

            static int FindMethod(MethodData[] methodDatas, MethodBase methodToFind)
            {
                for (int i = 0; i < methodDatas.Length; i++)
                {
                    MethodBase method = methodDatas[i].MethodBase;
                    if (method == null)
                    {
                        break;
                    }
                    if (method == methodToFind)
                    {
                        return i;
                    }
                }
                return -1;
            }

            public void SetMethodData(MethodData methodData)
            {
                lock (ThisLock)
                {
                    int index = FindMethod(this.methodDatas, methodData.MethodBase);
                    if (index < 0)
                    {
                        for (int i = 0; i < this.methodDatas.Length; i++)
                        {
                            if (methodDatas[i].MethodBase == null)
                            {
                                methodDatas[i] = methodData;
                                return;
                            }
                        }
                        MethodData[] newMethodDatas = new MethodData[methodDatas.Length * 2];
                        Array.Copy(methodDatas, newMethodDatas, methodDatas.Length);
                        newMethodDatas[methodDatas.Length] = methodData;
                        this.methodDatas = newMethodDatas;
                    }
                }
            }
        }

        enum MethodType
        {
            Service,
            BeginService,
            EndService,
            Channel,
            Object,
            GetType,
            TaskService
        }

        struct MethodData
        {
            MethodBase methodBase;
            MethodType methodType;
            ProxyOperationRuntime operation;

            public MethodData(MethodBase methodBase, MethodType methodType)
                : this(methodBase, methodType, null)
            {
            }

            public MethodData(MethodBase methodBase, MethodType methodType, ProxyOperationRuntime operation)
            {
                this.methodBase = methodBase;
                this.methodType = methodType;
                this.operation = operation;
            }

            public MethodBase MethodBase
            {
                get { return methodBase; }
            }

            public MethodType MethodType
            {
                get { return methodType; }
            }

            public ProxyOperationRuntime Operation
            {
                get { return operation; }
            }
        }

        class MbrObject : MarshalByRefObject
        {
            RealProxy proxy;
            Type targetType;

            internal MbrObject(RealProxy proxy, Type targetType)
            {
                this.proxy = proxy;
                this.targetType = targetType;
            }

            public override bool Equals(object obj)
            {
                return Object.ReferenceEquals(obj, this.proxy.GetTransparentProxy());
            }

            public override string ToString()
            {
                return this.targetType.ToString();
            }

            public override int GetHashCode()
            {
                return this.proxy.GetHashCode();
            }
        }

        class SingleReturnMessage : IMethodReturnMessage
        {
            IMethodCallMessage methodCall;
            object ret;
            PropertyDictionary properties;

            public SingleReturnMessage(object ret, IMethodCallMessage methodCall)
            {
                this.ret = ret;
                this.methodCall = methodCall;
                this.properties = new PropertyDictionary();
            }

            public int ArgCount
            {
                get { return 0; }
            }

            public object[] Args
            {
                get { return EmptyArray.Instance; }
            }

            public Exception Exception
            {
                get { return null; }
            }

            public bool HasVarArgs
            {
                get { return methodCall.HasVarArgs; }
            }

            public LogicalCallContext LogicalCallContext
            {
                get { return SetActivityIdInLogicalCallContext(methodCall.LogicalCallContext); }
            }

            public MethodBase MethodBase
            {
                get { return methodCall.MethodBase; }
            }

            public string MethodName
            {
                get { return methodCall.MethodName; }
            }

            public object MethodSignature
            {
                get { return methodCall.MethodSignature; }
            }

            public object[] OutArgs
            {
                get { return EmptyArray.Instance; }
            }

            public int OutArgCount
            {
                get { return 0; }
            }

            public IDictionary Properties
            {
                get { return properties; }
            }

            public object ReturnValue
            {
                get { return ret; }
            }

            public string TypeName
            {
                get { return methodCall.TypeName; }
            }

            public string Uri
            {
                get { return null; }
            }

            public object GetArg(int index)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("index"));
            }

            public string GetArgName(int index)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("index"));
            }

            public object GetOutArg(int index)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("index"));
            }

            public string GetOutArgName(int index)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("index"));
            }

            class PropertyDictionary : IDictionary
            {
                ListDictionary properties;

                public object this[object key]
                {
                    get { return Properties[key]; }
                    set { Properties[key] = value; }
                }

                public int Count
                {
                    get { return Properties.Count; }
                }

                public bool IsFixedSize
                {
                    get { return false; }
                }

                public bool IsReadOnly
                {
                    get { return false; }
                }

                public bool IsSynchronized
                {
                    get { return false; }
                }

                public ICollection Keys
                {
                    get { return Properties.Keys; }
                }

                ListDictionary Properties
                {
                    get
                    {
                        if (properties == null)
                        {
                            properties = new ListDictionary();
                        }
                        return properties;
                    }
                }

                public ICollection Values
                {
                    get { return Properties.Values; }
                }

                public object SyncRoot
                {
                    get { return null; }
                }

                public void Add(object key, object value)
                {
                    Properties.Add(key, value);
                }

                public void Clear()
                {
                    Properties.Clear();
                }

                public bool Contains(object key)
                {
                    return Properties.Contains(key);
                }

                public void CopyTo(Array array, int index)
                {
                    Properties.CopyTo(array, index);
                }

                public IDictionaryEnumerator GetEnumerator()
                {
                    if (properties == null)
                    {
                        return EmptyEnumerator.Instance;
                    }
                    else
                    {
                        return properties.GetEnumerator();
                    }
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return ((IEnumerable)Properties).GetEnumerator();
                }

                public void Remove(object key)
                {
                    Properties.Remove(key);
                }

                class EmptyEnumerator : IDictionaryEnumerator
                {
                    static EmptyEnumerator instance = new EmptyEnumerator();

                    EmptyEnumerator()
                    {
                    }

                    public static EmptyEnumerator Instance
                    {
                        get { return instance; }
                    }

                    public bool MoveNext()
                    {
                        return false;
                    }

                    public Object Current
                    {
                        get
                        {
#pragma warning suppress 56503 // Microsoft, IEnumerator guidelines, Current throws exception before calling MoveNext
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxDictionaryIsEmpty)));
                        }
                    }

                    public void Reset()
                    {
                    }

                    public Object Key
                    {
                        get
                        {
#pragma warning suppress 56503 // Microsoft, IEnumerator guidelines, Current throws exception before calling MoveNext
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxDictionaryIsEmpty)));
                        }
                    }

                    public Object Value
                    {
                        get
                        {
#pragma warning suppress 56503 // Microsoft, IEnumerator guidelines, Current throws exception before calling MoveNext
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxDictionaryIsEmpty)));
                        }
                    }

                    public DictionaryEntry Entry
                    {
                        get
                        {
#pragma warning suppress 56503 // Microsoft, IEnumerator guidelines, Current throws exception before calling MoveNext
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxDictionaryIsEmpty)));
                        }
                    }
                }
            }
        }
    }
}
