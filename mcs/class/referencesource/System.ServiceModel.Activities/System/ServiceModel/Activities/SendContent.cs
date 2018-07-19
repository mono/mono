//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Activities;
    using System.Collections.Generic;
    using System.ServiceModel.Description;
    
    // used in Send.Content
    public abstract class SendContent
    {
        static SendContent defaultSendContent;

        // internal ctor since we control the hierarchy
        // only supported subclasses are SendMessageContent and SendParametersContent
        internal SendContent()
        {
        }

        internal static SendContent DefaultSendContent
        {
            get
            {
                if (defaultSendContent == null)
                {
                    defaultSendContent = new SendMessageContent();
                }
                return defaultSendContent;
            }
        }

        public static SendMessageContent Create(InArgument message)
        {
            return new SendMessageContent(message);
        }

        public static SendMessageContent Create(InArgument message, Type declaredMessageType)
        {
            return new SendMessageContent(message) { DeclaredMessageType = declaredMessageType };
        }

        public static SendParametersContent Create(IDictionary<string, InArgument> parameters)
        {
            return new SendParametersContent(parameters);
        }

        internal abstract bool IsFault { get; }

        internal abstract void CacheMetadata(ActivityMetadata metadata, Activity owner, string operationName);

        internal abstract void ConfigureInternalSend(InternalSendMessage internalSendMessage, out ToRequest requestFormatter);

        internal abstract void ConfigureInternalSendReply(InternalSendMessage internalSendMessage, out ToReply responseFormatter);

        internal abstract void InferMessageDescription(OperationDescription operation, object owner, MessageDirection direction);

        internal abstract void ValidateContract(NativeActivityContext context, OperationDescription targetOperation, object owner, MessageDirection direction);
    }
}
