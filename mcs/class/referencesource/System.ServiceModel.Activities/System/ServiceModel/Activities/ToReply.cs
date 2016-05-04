//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Activities;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using SR2 = System.ServiceModel.Activities.SR;
    using System.Runtime.DurableInstancing;
    using System.Xml.Linq;

    class ToReply : NativeActivity
    {
        IDispatchMessageFormatter formatter;
        IDispatchFaultFormatter faultFormatter;

        Collection<InArgument> parameters;

        public IDispatchMessageFormatter Formatter
        {
            get
            {
                return this.formatter;
            }
            set
            {
                this.formatter = value;
                ValidateFormatters();
            }
        }

        public IDispatchFaultFormatter FaultFormatter
        {
            get
            {
                return this.faultFormatter;
            }
            set
            {
                this.faultFormatter = value;
                ValidateFormatters();
            }
        }

        public bool IncludeExceptionDetailInFaults
        {
            get;
            set;
        }

        public InArgument Result
        {
            get;
            set;
        }

        public Collection<InArgument> Parameters
        {
            get
            {
                if (this.parameters == null)
                {
                    this.parameters = new Collection<InArgument>();
                }
                return this.parameters;
            }
        }

        //CorrelationHandle is required to get the message version from the InternalReceivedMessage
        public InArgument<CorrelationHandle> CorrelatesWith
        {
            get;
            set;
        }

        public OutArgument<Message> Message
        {
            get;
            set;
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            if (this.Result != null)
            {
                RuntimeArgument resultArgument = new RuntimeArgument(Constants.Result, this.Result.ArgumentType, ArgumentDirection.In);
                metadata.Bind(this.Result, resultArgument);
                metadata.AddArgument(resultArgument);
            }

            if (this.parameters != null)
            {
                int count = 0;
                foreach (InArgument parameter in this.parameters)
                {
                    RuntimeArgument parameterArgument = new RuntimeArgument(Constants.Parameter + count++, parameter.ArgumentType, ArgumentDirection.In);
                    metadata.Bind(parameter, parameterArgument);
                    metadata.AddArgument(parameterArgument);
                }
            }

            RuntimeArgument messageArgument = new RuntimeArgument(Constants.Message, Constants.MessageType, ArgumentDirection.Out, true);
            if (this.Message == null)
            {
                this.Message = new OutArgument<Message>();
            }
            metadata.Bind(this.Message, messageArgument);
            metadata.AddArgument(messageArgument);

            RuntimeArgument correlatesWithArgument = new RuntimeArgument(Constants.CorrelatesWith, Constants.CorrelationHandleType, ArgumentDirection.In);
            if (this.CorrelatesWith == null)
            {
                this.CorrelatesWith = new InArgument<CorrelationHandle>();
            }
            metadata.Bind(this.CorrelatesWith, correlatesWithArgument);
            metadata.AddArgument(correlatesWithArgument);

        }

        protected override void Execute(NativeActivityContext context)
        {
            MessageVersion version;

            SendReceiveExtension sendReceiveExtension = context.GetExtension<SendReceiveExtension>();
            if (sendReceiveExtension != null)
            {
                HostSettings hostSettings = sendReceiveExtension.HostSettings;
                this.IncludeExceptionDetailInFaults = hostSettings.IncludeExceptionDetailInFaults;
            }

            CorrelationHandle correlatesWith = (this.CorrelatesWith == null) ? null : this.CorrelatesWith.Get(context);
            if (correlatesWith == null)
            {
                correlatesWith = context.Properties.Find(CorrelationHandle.StaticExecutionPropertyName) as CorrelationHandle;
            }

            CorrelationResponseContext responseContext;
            if (correlatesWith != null)
            {
                if (sendReceiveExtension != null)
                {
                    if (!this.TryGetMessageVersion(correlatesWith.InstanceKey, out version))
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(SR2.MessageVersionInformationNotFound));
                    }
                }
                else if (correlatesWith.TryAcquireResponseContext(context, out responseContext))
                {
                    //Register the ResponseContext so that InternalSendMessage can access it.
                    if (!correlatesWith.TryRegisterResponseContext(context, responseContext))
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(SR2.ResponseContextIsNotNull));
                    }

                    //Use the same MessageVersion as the incoming message that is retrieved using CorrelatonHandle
                    version = responseContext.MessageVersion;
                }
                else
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR2.CorrelationResponseContextShouldNotBeNull));
                }
            }
            else
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR2.CorrelationResponseContextShouldNotBeNull));
            }

            Fx.Assert((this.Formatter == null && this.FaultFormatter != null) ||
                    (this.Formatter != null && this.FaultFormatter == null),
                    "OperationFormatter and FaultFormatter cannot be both null or both set!");

            if (this.FaultFormatter != null)
            {
                Fx.Assert(this.parameters.Count == 1, "Exception should be the only parameter!");
                Exception exception = this.parameters[0].Get(context) as Exception;
                Fx.Assert(exception != null, "InArgument must be an Exception!");

                MessageFault messageFault;
                string action;

                FaultException faultException = exception as FaultException;
                if (faultException != null)
                {
                    // This is an expected fault
                    // Reproduce logic from ErrorBehavior.InitializeFault

                    messageFault = this.FaultFormatter.Serialize(faultException, out action);
                    if (action == null)
                    {
                        action = version.Addressing.DefaultFaultAction;
                    }
                }
                else
                {
                    // This is an unexpected fault
                    // Reproduce logic from ErrorBehavior.ProvideFaultOfLastResort

                    FaultCode code = new FaultCode(FaultCodeConstants.Codes.InternalServiceFault, FaultCodeConstants.Namespaces.NetDispatch);
                    code = FaultCode.CreateReceiverFaultCode(code);

                    action = FaultCodeConstants.Actions.NetDispatcher;

                    if (this.IncludeExceptionDetailInFaults)
                    {
                        messageFault = MessageFault.CreateFault(code,
                            new FaultReason(new FaultReasonText(exception.Message, CultureInfo.CurrentCulture)),
                            new ExceptionDetail(exception));
                    }
                    else
                    {
                        messageFault = MessageFault.CreateFault(code,
                            new FaultReason(new FaultReasonText(SR2.InternalServerError, CultureInfo.CurrentCulture)));
                    }
                }

                if (messageFault == null)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR2.CannotCreateMessageFault));
                }
                else
                {
                    Message outMessage = System.ServiceModel.Channels.Message.CreateMessage(version, messageFault, action);
                    this.Message.Set(context, outMessage);
                }
            }
            else
            {
                object[] inObjects;
                if (this.parameters != null)
                {
                    inObjects = new object[this.parameters.Count];
                    for (int i = 0; i < this.parameters.Count; i++)
                    {
                        Fx.Assert(this.Parameters[i] != null, "Parameter cannot be null");
                        inObjects[i] = this.parameters[i].Get(context);
                    }
                }
                else
                {
                    inObjects = Constants.EmptyArray;
                }

                object returnValue = null;
                if (this.Result != null)
                {
                    returnValue = this.Result.Get(context);
                }

                Message outMessage = this.Formatter.SerializeReply(version, inObjects, returnValue);
                this.Message.Set(context, outMessage);
            }
        }

        bool TryGetMessageVersion(InstanceKey instanceKey, out MessageVersion version)
        {
            Fx.Assert(instanceKey != null, "Expect a valid instanceKey here");
            version = MessageVersion.None;
            if (instanceKey != null)
            {
                InstanceValue messageVersionValue;
                if (instanceKey.Metadata.TryGetValue(WorkflowServiceNamespace.MessageVersionForReplies, out messageVersionValue))
                {
                    version = (MessageVersion)messageVersionValue.Value;
                    return true;
                }
            }

            return false;
        }

        void ValidateFormatters()
        {
            if (this.Formatter == null && this.FaultFormatter == null)
            {
                throw FxTrace.Exception.AsError(new ValidationException(SR2.OperationFormatterAndFaultFormatterNotSet));
            }
            if (this.Formatter != null && this.FaultFormatter != null)
            {
                throw FxTrace.Exception.AsError(new ValidationException(SR2.OperationFormatterAndFaultFormatterIncorrectlySet));
            }
        }
    }
}
