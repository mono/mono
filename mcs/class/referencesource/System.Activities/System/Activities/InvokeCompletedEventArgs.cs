//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;

    [Fx.Tag.XamlVisible(false)]
    public class InvokeCompletedEventArgs : AsyncCompletedEventArgs
    {
        internal InvokeCompletedEventArgs(Exception error, bool cancelled, AsyncInvokeContext context)
            : base(error, cancelled, context.UserState)
        {
            this.Outputs = context.Outputs;
            
        }

        public IDictionary<string, object> Outputs
        {
            get;
            private set;
        }
    }
}
