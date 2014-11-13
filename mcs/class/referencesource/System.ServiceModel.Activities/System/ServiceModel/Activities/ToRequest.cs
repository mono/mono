//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    class ToRequest : CodeActivity
    {
        MessageVersion messageVersion;

        Collection<InArgument> parameters;

        public ToRequest()
        {
        }

        public Send Send
        {
            get;
            set;
        }

        public IClientMessageFormatter Formatter
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

        public OutArgument<Message> Message
        {
            get;
            set;
        }
       
        internal MessageVersion MessageVersion
        {
            get
            {
                if (this.messageVersion == null)
                {
                    this.messageVersion = this.Send.GetMessageVersion();
                }
                return this.messageVersion;
            }
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
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
            metadata.Bind(this.Message, messageArgument);
            metadata.AddArgument(messageArgument);

            Fx.Assert(this.Send != null, "Send cannot be null");
        }

        protected override void Execute(CodeActivityContext context)
        {
            object[] inObjects;
            if (this.parameters == null || this.parameters.Count == 0)
            {
                inObjects = Constants.EmptyArray;
            }
            else
            {
                inObjects = new object[this.parameters.Count];
                for (int i = 0; i < this.parameters.Count; i++)
                {
                    Fx.Assert(this.parameters[i] != null, "Parameter should not be null");
                    inObjects[i] = this.parameters[i].Get(context);
                }
            }
            // Formatter is cached since it is fixed for each definition of Send
            if (this.Formatter == null)
            {
                OperationDescription operation = ContractInferenceHelper.CreateOneWayOperationDescription(this.Send);
                this.Formatter = ClientOperationFormatterProvider.GetFormatterFromRuntime(operation);

                this.Send.OperationDescription = operation;
            }

            // Send.ChannelCacheEnabled must be set before we call this.MessageVersion
            // because this.MessageVersion will cache description and description resolution depends on the value of ChannelCacheEnabled
            this.Send.InitializeChannelCacheEnabledSetting(context);

            // MessageVersion is cached for perf reasons since it is fixed for each definition of Send
            Message outMessage = this.Formatter.SerializeRequest(this.MessageVersion, inObjects);
            this.Message.Set(context, outMessage);
        }
    }
}
