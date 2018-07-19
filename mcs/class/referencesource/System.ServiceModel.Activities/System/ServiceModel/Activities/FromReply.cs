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

    class FromReply : CodeActivity
    {
        Collection<OutArgument> parameters;

        public InArgument<Message> Message
        {
            get;
            set;
        }

        public IClientMessageFormatter Formatter
        {
            get;
            set;
        }

        public IClientFaultFormatter FaultFormatter
        {
            get;
            set;
        }

        public OutArgument Result
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

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            RuntimeArgument messageArgument = new RuntimeArgument(Constants.Message, Constants.MessageType, ArgumentDirection.In, true);
            metadata.Bind(this.Message, messageArgument);
            metadata.AddArgument(messageArgument);

            if (this.Result != null)
            {
                RuntimeArgument resultArgument = new RuntimeArgument(Constants.Result, this.Result.ArgumentType, ArgumentDirection.Out);
                metadata.Bind(this.Result, resultArgument);
                metadata.AddArgument(resultArgument);
            }

            if (this.parameters != null)
            {
                int count = 0;
                foreach (OutArgument parameter in this.parameters)
                {
                    RuntimeArgument parameterArgument = new RuntimeArgument(Constants.Message + count++, parameter.ArgumentType, ArgumentDirection.Out);
                    metadata.Bind(parameter, parameterArgument);
                    metadata.AddArgument(parameterArgument);
                }
            }
        }

        protected override void Execute(CodeActivityContext context)
        {
            Message inMessage = this.Message.Get(context);

            if (inMessage == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.NullReplyMessageContractMismatch));
            }
            if (inMessage.IsFault)
            {
                FaultConverter faultConverter = FaultConverter.GetDefaultFaultConverter(inMessage.Version);
                Exception exception = DeserializeFault(inMessage, faultConverter);

                // We simply throw the exception
                throw FxTrace.Exception.AsError(exception);
            }
            else
            {
                object[] outObjects;
                if (this.parameters != null)
                {
                    outObjects = new object[this.parameters.Count];
                }
                else
                {
                    outObjects = Constants.EmptyArray;
                }

                object returnValue = this.Formatter.DeserializeReply(inMessage, outObjects);

                if (this.Result != null)
                {
                    this.Result.Set(context, returnValue);
                }

                if (parameters != null)
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
        }

        Exception DeserializeFault(Message inMessage, FaultConverter faultConverter)
        {
            // Reproduce logic in ClientOperationFormatterHelper

            MessageFault messageFault = MessageFault.CreateFault(inMessage, TransportDefaults.MaxFaultSize);
            string action = inMessage.Headers.Action;
            if (action == inMessage.Version.Addressing.DefaultFaultAction)
            {
                action = null;
            }

            Exception exception = null;
            if (faultConverter != null)
            {
                faultConverter.TryCreateException(inMessage, messageFault, out exception);
            }

            if (exception == null)
            {
                exception = this.FaultFormatter.Deserialize(messageFault, action);
            }

            if (inMessage.State != MessageState.Created)
            {
                inMessage.Close();
            }

            return exception;
        }
    }
}
