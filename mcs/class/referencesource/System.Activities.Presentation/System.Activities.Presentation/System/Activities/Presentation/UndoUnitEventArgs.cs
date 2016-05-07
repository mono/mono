//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Runtime;

    [Fx.Tag.XamlVisible(false)]
    public class UndoUnitEventArgs : EventArgs
    {
        public UndoUnit UndoUnit { get; set; }
    }
}
