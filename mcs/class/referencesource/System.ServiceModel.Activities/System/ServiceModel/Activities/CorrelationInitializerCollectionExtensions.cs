//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Activities;
    using System.Collections.ObjectModel;

    static class CorrelationInitializerCollectionExtensions
    {
        public static bool TryGetRequestReplyCorrelationHandle(this Collection<CorrelationInitializer> correlationInitializers, NativeActivityContext context, out CorrelationHandle correlationHandle)
        {
            correlationHandle = CorrelationHandle.GetExplicitRequestReplyCorrelation(context, correlationInitializers);
            return correlationHandle != null;
        }

        public static bool TryGetContextCorrelationHandle(this Collection<CorrelationInitializer> correlationInitializers, NativeActivityContext context, out CorrelationHandle correlationHandle)
        {
            correlationHandle = CorrelationHandle.GetExplicitContextCorrelation(context, correlationInitializers);
            return correlationHandle != null;
        }

        public static bool TryGetCallbackCorrelationHandle(this Collection<CorrelationInitializer> correlationInitializers, NativeActivityContext context, out CorrelationHandle correlationHandle)
        {
            correlationHandle = CorrelationHandle.GetExplicitCallbackCorrelation(context, correlationInitializers);
            return correlationHandle != null;
        }
    }
}
