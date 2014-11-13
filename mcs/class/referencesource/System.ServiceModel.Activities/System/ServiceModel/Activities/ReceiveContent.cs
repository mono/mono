//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Activities;
    using System.Collections.Generic;
    using System.ServiceModel.Description;
    
    // used in Receive.Content
    public abstract class ReceiveContent
    {
        static ReceiveContent defaultReceiveContent;

        // internal ctor since we control the hierarchy
        // only supported subclasses are ReceiveMessageContent and ReceiveParametersContent
        internal ReceiveContent()
        {
        }

        internal static ReceiveContent DefaultReceiveContent
        {
            get
            {
                if (defaultReceiveContent == null)
                {
                    defaultReceiveContent = new ReceiveMessageContent();
                }
                return defaultReceiveContent;
            }
        }

        public static ReceiveMessageContent Create(OutArgument message)
        {
            return new ReceiveMessageContent(message);
        }

        public static ReceiveMessageContent Create(OutArgument message, Type declaredMessageType)
        {
            return new ReceiveMessageContent(message) { DeclaredMessageType = declaredMessageType };
        }

        public static ReceiveParametersContent Create(IDictionary<string, OutArgument> parameters)
        {
            return new ReceiveParametersContent(parameters);
        }

        internal abstract void CacheMetadata(ActivityMetadata metadata, Activity owner, string operationName);

        internal abstract void ConfigureInternalReceive(InternalReceiveMessage internalReceiveMessage, out FromRequest requestFormatter);

        internal abstract void ConfigureInternalReceiveReply(InternalReceiveMessage internalReceiveMessage, out FromReply responseFormatter);

        internal abstract void InferMessageDescription(OperationDescription operation, object owner, MessageDirection direction);
        
        internal abstract void ValidateContract(NativeActivityContext context, OperationDescription targetOperation, object owner, MessageDirection direction);
    }
}
