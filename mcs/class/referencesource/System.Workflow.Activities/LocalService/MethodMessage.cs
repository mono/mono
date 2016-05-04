#pragma warning disable 1634, 1691
using System;
using System.Diagnostics;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Workflow.Runtime;
using System.Security.Principal;
using System.Threading;
using System.Globalization;

namespace System.Workflow.Activities
{
    internal interface IMethodResponseMessage
    {
        void SendResponse(ICollection outArgs);
        void SendException(Exception exception);
        Exception Exception { get; }
        ICollection OutArgs { get; }
    }

    [Serializable]
    internal sealed class MethodMessage : IMethodMessage, IMethodResponseMessage
    {
        [NonSerialized]
        Type interfaceType;
        [NonSerialized]
        string methodName;
        [NonSerialized]
        object[] args;
        [NonSerialized]
        ManualResetEvent returnValueSignalEvent;

        object[] clonedArgs;
        LogicalCallContext callContext;

        ICollection outArgs;
        Exception exception;

        [NonSerialized]
        bool responseSet = false;

        Guid callbackCookie;

        [NonSerialized]
        MethodMessage previousMessage = null;

        static Dictionary<Guid, MethodMessage> staticMethodMessageMap = new Dictionary<Guid, MethodMessage>();
        static Object syncRoot = new Object();

        internal MethodMessage(Type interfaceType, string methodName,
                               object[] args, String identity) :
            this(interfaceType, methodName, args, identity, false)
        {

        }

        internal MethodMessage(Type interfaceType, string methodName,
                               object[] args, String identity, bool responseRequired)
        {
            this.interfaceType = interfaceType;
            this.methodName = methodName;
            this.args = args;
            callContext = GetLogicalCallContext();

            if (responseRequired)
                returnValueSignalEvent = new ManualResetEvent(false);

            PopulateIdentity(callContext, identity);
            Clone();
        }



        [OnSerializing]
        void OnSerializing(StreamingContext context)
        {
            if (returnValueSignalEvent != null && !responseSet)
            {
                callbackCookie = Guid.NewGuid();

                lock (syncRoot)
                {
                    staticMethodMessageMap.Add(callbackCookie, previousMessage ?? this);
                }
            }
        }

        [OnDeserialized]
        void OnDeserialized(StreamingContext context)
        {
            if (callbackCookie != Guid.Empty)
            {
                lock (syncRoot)
                {
                    if (staticMethodMessageMap.TryGetValue(callbackCookie, out previousMessage))
                        staticMethodMessageMap.Remove(callbackCookie);
                }

                if (previousMessage != null)
                {
                    this.responseSet = previousMessage.responseSet;
                    this.returnValueSignalEvent = previousMessage.returnValueSignalEvent;
                }
            }

            callbackCookie = Guid.Empty;
        }

        string IMethodMessage.GetArgName(int index)
        {
            throw new NotImplementedException();
        }

        object IMethodMessage.GetArg(int argNum)
        {
            return this.clonedArgs[argNum];
        }

        string IMethodMessage.Uri
        {
#pragma warning disable 56503
            // not implemented
            get { throw new NotImplementedException(); }
#pragma warning restore 56503
        }

        string IMethodMessage.MethodName
        {
            get { return this.methodName; }
        }

        string IMethodMessage.TypeName
        {
            get
            {
                return (this.interfaceType.ToString());
            }
        }

        object IMethodMessage.MethodSignature
        {
#pragma warning disable 56503
            get { throw new NotImplementedException(); }
#pragma warning restore 56503
        }

        object[] IMethodMessage.Args
        {
            get
            {
                return this.clonedArgs;
            }
        }

        object Clone()
        {
            object[] clones = new object[this.args.Length];

            for (int i = 0; i < this.args.Length; i++)
            {
                clones[i] = Clone(this.args[i]);
            }
            this.clonedArgs = clones;
            return clones;
        }

        object Clone(object source)
        {
            if (source == null || source.GetType().IsValueType)
                return source;

            ICloneable clone = source as ICloneable;
            if (clone != null)
                return clone.Clone();

            BinaryFormatter formatter = new BinaryFormatter();
            System.IO.MemoryStream stream = new System.IO.MemoryStream(1024);
            try
            {
                formatter.Serialize(stream, source);
            }
            catch (SerializationException e)
            {
                throw new InvalidOperationException(SR.GetString(SR.Error_EventArgumentSerializationException), e);
            }
            stream.Position = 0;
            object cloned = formatter.Deserialize(stream);
            return cloned;
        }

        int IMethodMessage.ArgCount
        {
            get { return this.clonedArgs.Length; }
        }

        bool IMethodMessage.HasVarArgs
        {
#pragma warning disable 56503
            get { throw new NotImplementedException(); }
#pragma warning restore 56503
        }

        LogicalCallContext IMethodMessage.LogicalCallContext
        {
            get { return callContext; }
        }

        MethodBase IMethodMessage.MethodBase
        {
#pragma warning disable 56503
            get { throw new NotImplementedException(); }
#pragma warning restore 56503
        }

        IDictionary System.Runtime.Remoting.Messaging.IMessage.Properties
        {
#pragma warning disable 56503
            get { throw new NotImplementedException(); }
#pragma warning restore 56503
        }

        void PopulateIdentity(LogicalCallContext callContext, String identity)
        {
            callContext.SetData(IdentityContextData.IdentityContext, new IdentityContextData(identity));
        }

        static LogicalCallContext singletonCallContext;
        static Object syncObject = new Object();

        static LogicalCallContext GetLogicalCallContext()
        {
            lock (syncObject)
            {
                if (singletonCallContext == null)
                {
                    CallContextProxy contextProxy = new CallContextProxy(typeof(IDisposable));
                    IDisposable disposable = (IDisposable)contextProxy.GetTransparentProxy();
                    disposable.Dispose();
                    singletonCallContext = contextProxy.CallContext;
                }
                return singletonCallContext.Clone() as LogicalCallContext;
            }
        }

        #region IMethodResponseMessage implementation

        internal IMethodResponseMessage WaitForResponseMessage()
        {
            // todo wait for certain timeout
            this.returnValueSignalEvent.WaitOne();
            this.returnValueSignalEvent = null;
            return this;
        }

        public void SendResponse(ICollection outArgs)
        {
            if (this.returnValueSignalEvent == null)
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Error_WorkflowInstanceDehydratedBeforeSendingResponse)));

            if (!this.responseSet)
            {
                this.OutArgs = outArgs;
                this.returnValueSignalEvent.Set();
                this.responseSet = true;
            }
        }

        public void SendException(Exception exception)
        {
            if (this.returnValueSignalEvent == null)
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Error_WorkflowInstanceDehydratedBeforeSendingResponse)));

            if (!this.responseSet)
            {
                this.Exception = exception;
                this.returnValueSignalEvent.Set();
                this.responseSet = true;
            }
        }

        public Exception Exception
        {
            get
            {
                return this.exception;
            }
            private set
            {
                if (previousMessage != null)
                    previousMessage.Exception = value;

                this.exception = value;
            }
        }

        public ICollection OutArgs
        {
            get
            {
                return this.outArgs;
            }
            private set
            {
                if (previousMessage != null)
                    previousMessage.OutArgs = value;

                this.outArgs = value;
            }
        }
        #endregion

        private sealed class CallContextProxy : System.Runtime.Remoting.Proxies.RealProxy
        {
            LogicalCallContext callContext;

            internal LogicalCallContext CallContext
            {
                get
                {
                    return callContext;
                }
            }

            internal CallContextProxy(Type proxiedType)
                : base(proxiedType)
            {

            }

            public override System.Runtime.Remoting.Messaging.IMessage Invoke(System.Runtime.Remoting.Messaging.IMessage msg)
            {
                IMethodCallMessage methodCallMessage = msg as IMethodCallMessage;
                this.callContext = methodCallMessage.LogicalCallContext.Clone() as LogicalCallContext;
                return new ReturnMessage(null, null, 0, methodCallMessage.LogicalCallContext, methodCallMessage);
            }
        }
    }
}
