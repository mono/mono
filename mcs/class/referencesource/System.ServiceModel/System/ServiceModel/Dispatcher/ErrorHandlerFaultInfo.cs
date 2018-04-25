//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    internal struct ErrorHandlerFaultInfo
    {
        Message fault;   // if this is null, then we aren't interested in sending back a fault
        bool isConsideredUnhandled;  // if this is true, it means Fault is the 'internal server error' fault
        string defaultFaultAction;

        public ErrorHandlerFaultInfo(string defaultFaultAction)
        {
            this.defaultFaultAction = defaultFaultAction;
            this.fault = null;
            this.isConsideredUnhandled = false;
        }

        public Message Fault
        {
            get { return this.fault; }
            set { this.fault = value; }
        }

        public string DefaultFaultAction
        {
            get { return this.defaultFaultAction; }
            set { this.defaultFaultAction = value; }
        }

        public bool IsConsideredUnhandled
        {
            get { return this.isConsideredUnhandled; }
            set { this.isConsideredUnhandled = value; }
        }
    }
}
