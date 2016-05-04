//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Tracking
{
    public sealed class FaultPropagationQuery : TrackingQuery
    {
        public FaultPropagationQuery()
        {
            this.FaultSourceActivityName = "*";
            this.FaultHandlerActivityName = "*";
        }

        public string FaultHandlerActivityName { get; set; }

        public string FaultSourceActivityName { get; set; }
    }
}
