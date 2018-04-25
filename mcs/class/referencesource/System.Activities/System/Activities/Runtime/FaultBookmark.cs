//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Runtime
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Collections.Generic;

    [DataContract]
    class FaultBookmark
    {
        FaultCallbackWrapper callbackWrapper;

        public FaultBookmark(FaultCallbackWrapper callbackWrapper)
        {
            this.callbackWrapper = callbackWrapper;
        }

        [DataMember(Name = "callbackWrapper")]
        internal FaultCallbackWrapper SerializedCallbackWrapper
        {
            get { return this.callbackWrapper; }
            set { this.callbackWrapper = value; }
        }

        public WorkItem GenerateWorkItem(Exception propagatedException, ActivityInstance propagatedFrom, ActivityInstanceReference originalExceptionSource)
        {
            return this.callbackWrapper.CreateWorkItem(propagatedException, propagatedFrom, originalExceptionSource);
        }
    }
}
