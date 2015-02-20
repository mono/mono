//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;

    [DataContract]
    public class NoPersistHandle : Handle
    {
        public NoPersistHandle()
        {
        }

        public void Enter(NativeActivityContext context)
        {
            context.ThrowIfDisposed();
            ThrowIfUninitialized();

            context.EnterNoPersist(this);
        }

        public void Exit(NativeActivityContext context)
        {
            context.ThrowIfDisposed();
            ThrowIfUninitialized();

            context.ExitNoPersist(this);
        }
    }
}


