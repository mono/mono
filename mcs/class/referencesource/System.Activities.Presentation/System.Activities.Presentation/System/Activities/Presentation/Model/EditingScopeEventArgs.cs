//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Runtime;

    [Fx.Tag.XamlVisible(false)]
    public class EditingScopeEventArgs : EventArgs
    {
        public EditingScope EditingScope { get; set; }
    }
}
