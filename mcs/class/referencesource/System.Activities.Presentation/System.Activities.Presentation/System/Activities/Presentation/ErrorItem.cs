//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    class ErrorItem : ContextItem
    {
        public string Message
        { get; set; }

        public string Details
        { get; set; }

        public override Type ItemType
        {
            get
            {
                return typeof(ErrorItem);
            }
        }
    }
}
