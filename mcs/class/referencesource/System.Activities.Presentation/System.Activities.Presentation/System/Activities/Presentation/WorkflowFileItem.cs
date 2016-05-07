//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------


namespace System.Activities.Presentation
{
    using System;

    public class WorkflowFileItem : ContextItem
    {
        public string LoadedFile
        { set; get; }

        public sealed override Type ItemType
        {
            get { return typeof(WorkflowFileItem); }
        }
    }
}
