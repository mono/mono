//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System;
    using System.ComponentModel;
    using System.Runtime;

    [Fx.Tag.XamlVisible(false)]
    public class ResolveCompletedEventArgs : AsyncCompletedEventArgs
    {
        ResolveResponse result;

        internal ResolveCompletedEventArgs(Exception error, bool cancelled, object userState, ResolveResponse result)
            : base(error, cancelled, userState)
        {
            this.result = result;
        }

        public ResolveResponse Result
        {
            [Fx.Tag.InheritThrows(From = "Resolve", FromDeclaringType = typeof(DiscoveryClient))]
            get
            {
                RaiseExceptionIfNecessary();
                return this.result;
            }
        }
    }
}
