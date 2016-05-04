//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Runtime
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security;

    [DataContract]
    class ActivityCompletionCallbackWrapper : CompletionCallbackWrapper
    {
        static readonly Type completionCallbackType = typeof(CompletionCallback);
        static readonly Type[] completionCallbackParameters = new Type[] { typeof(NativeActivityContext), typeof(ActivityInstance) };

        public ActivityCompletionCallbackWrapper(CompletionCallback callback, ActivityInstance owningInstance)
            : base(callback, owningInstance)
        {
        }

        [Fx.Tag.SecurityNote(Critical = "Because we are calling EnsureCallback",
            Safe = "Safe because the method needs to be part of an Activity and we are casting to the callback type and it has a very specific signature. The author of the callback is buying into being invoked from PT.")]
        [SecuritySafeCritical]
        protected internal override void Invoke(NativeActivityContext context, ActivityInstance completedInstance)
        {
            EnsureCallback(completionCallbackType, completionCallbackParameters);
            CompletionCallback completionCallback = (CompletionCallback)this.Callback;
            completionCallback(context, completedInstance);
        }
    }
}
