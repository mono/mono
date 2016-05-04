//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.Metadata
{
    internal class ActivityDelegateArgumentMetadata
    {
        public string Name
        {
            get;
            set;
        }

        public ActivityDelegateArgumentDirection Direction
        {
            get;
            set;
        }

        public Type Type
        {
            get;
            set;
        }
    }
}
