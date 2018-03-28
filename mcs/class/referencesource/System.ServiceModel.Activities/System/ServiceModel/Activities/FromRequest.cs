//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Activities;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using SR2 = System.ServiceModel.Activities.SR;
    
    class FromRequest : NativeActivity
    {
        Collection<OutArgument> parameters;

        public InOutArgument<Message> Message
        {
            get;
            set;
        }

        public IDispatchMessageFormatter Formatter
        {
            get;
            set;
        }

        public Collection<OutArgument> Parameters
        {
            get
            {
                if (this.parameters == null)
                {
                    this.parameters = new Collection<OutArgument>();
                }
                return this.parameters;
            }
        }
        
        public InArgument<NoPersistHandle> NoPersistHandle
        {
            get;
            set;
        }

        internal bool CloseMessage
        {
            get;
            set;
        }
        
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            RuntimeArgument messageArgument = new RuntimeArgument(Constants.Message, Constants.MessageType, ArgumentDirection.InOut, true);
            metadata.Bind(this.Message, messageArgument);
            metadata.AddArgument(messageArgument);

            if (this.parameters != null)
            {
                int count = 0;
                foreach (OutArgument parameter in this.parameters)
                {
                    RuntimeArgument parameterArgument = new RuntimeArgument(Constants.Parameter + count++, parameter.ArgumentType, ArgumentDirection.Out);
                    metadata.Bind(parameter, parameterArgument);
                    metadata.AddArgument(parameterArgument);
                }
            }

            RuntimeArgument noPersistHandleArgument = new RuntimeArgument(Constants.NoPersistHandle, Constants.NoPersistHandleType, ArgumentDirection.In);
            metadata.Bind(this.NoPersistHandle, noPersistHandleArgument);
            metadata.AddArgument(noPersistHandleArgument);
        }

        protected override void Execute(NativeActivityContext context)
        {
            Message inMessage = null;
            try
            {
                inMessage = this.Message.Get(context);
                object[] outObjects;
                if (this.parameters == null || this.parameters.Count == 0)
                {
                    outObjects = Constants.EmptyArray;
                }
                else
                {
                    outObjects = new object[this.parameters.Count];
                }

                // The formatter would be null if there is no parameters to Deserialize in message contracts
                if (this.Formatter != null)
                {
                    this.Formatter.DeserializeRequest(inMessage, outObjects);
                }
                else
                {
                    Fx.Assert(this.parameters == null, "There shouldn't be any parameters to be deserialized");
                }

                if (this.parameters != null)
                {
                    for (int i = 0; i < this.parameters.Count; i++)
                    {
                        OutArgument outArgument = this.parameters[i];
                        Fx.Assert(outArgument != null, "Parameter cannot be null");

                        object obj = outObjects[i];
                        if (obj == null)
                        {
                            obj = ProxyOperationRuntime.GetDefaultParameterValue(outArgument.ArgumentType);
                        }

                        outArgument.Set(context, obj);
                    }
                }
            }
            finally
            {
                if (this.CloseMessage && inMessage != null)
                {
                    inMessage.Close();
                }

                this.Message.Set(context, null);

                bool useNoPersistHandle = UseNoPersistHandle(context);
                if (useNoPersistHandle)
                {
                    NoPersistHandle handle = (this.NoPersistHandle == null) ? null : this.NoPersistHandle.Get(context);
                    if (handle != null)
                    {
                        handle.Exit(context);
                    }
                }
            }
        }

        static bool UseNoPersistHandle(NativeActivityContext executionContext)
        {
            // Default is set to true because we want NoPersistHandle to be 
            // used if the SendReceiveExtension is not available.
            bool result = true;

            SendReceiveExtension sendReceiveExtension = executionContext.GetExtension<SendReceiveExtension>();
            if (sendReceiveExtension != null)
            {
                result = sendReceiveExtension.HostSettings.UseNoPersistHandle;
            }

            return result;
        }

    }
}
