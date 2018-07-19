//------------------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//
//------------------------------------------------------------------------------

using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace System.Security.Authentication.ExtendedProtection
{
    public abstract class ChannelBinding : SafeHandleZeroOrMinusOneIsInvalid
    {
        protected ChannelBinding()
            : base(true)
        {
        }

        protected ChannelBinding(bool ownsHandle)
            : base(ownsHandle)
        {
        }

        public abstract int Size
        {
            get;
        }
    }
}
