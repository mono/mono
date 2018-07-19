//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System;
    using System.ComponentModel;
    using System.Runtime;

    [Fx.Tag.XamlVisible(false)]
    public class FindCompletedEventArgs : AsyncCompletedEventArgs
    {
        FindResponse result;

        internal FindCompletedEventArgs(Exception error, bool cancelled, object userState, FindResponse result)
            : base(error, cancelled, userState)
        {
            this.result = result;
        }

        public FindResponse Result
        {
            [Fx.Tag.InheritThrows(From = "Find", FromDeclaringType = typeof(DiscoveryClient))]
            get
            {
                RaiseExceptionIfNecessary();
                return this.result;
            }
        }
    }
}
