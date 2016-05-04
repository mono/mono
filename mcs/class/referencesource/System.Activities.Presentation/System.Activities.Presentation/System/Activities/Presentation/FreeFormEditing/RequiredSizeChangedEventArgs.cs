//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Presentation.FreeFormEditing
{
    using System;
    using System.Windows;

    class RequiredSizeChangedEventArgs : EventArgs
    {
        public RequiredSizeChangedEventArgs(Size newRequiredSize)
        {
            this.NewRequiredSize = newRequiredSize;
        }

        public Size NewRequiredSize
        {
            get;
            private set;
        }
    }
}
