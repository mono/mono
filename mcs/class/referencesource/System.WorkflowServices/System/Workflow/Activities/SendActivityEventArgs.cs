//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

#pragma warning disable 1634, 1691
namespace System.Workflow.Activities
{
    using System;

#pragma warning disable 56524
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class SendActivityEventArgs : EventArgs
    {
        SendActivity sendActivity;

#pragma warning disable 56504
        public SendActivityEventArgs(SendActivity sendActivity)
        {
            this.sendActivity = sendActivity;
        }
#pragma warning restore 56504

        public SendActivity SendActivity
        {
            get
            {
                return sendActivity;
            }
        }
    }
}
#pragma warning restore 56524
