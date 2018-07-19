//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Activities.Runtime;
    using System.Runtime;
    using System.Runtime.Serialization;

    // This does not need to be data contract since we'll never persist while one of these is active
    class NoPersistProperty : IPropertyRegistrationCallback
    {
        public const string Name = "System.Activities.NoPersistProperty";

        ActivityExecutor executor;
        int refCount;

        public NoPersistProperty(ActivityExecutor executor)
        {
            this.executor = executor;
        }

        public void Enter()
        {
            this.refCount++;
            this.executor.EnterNoPersist();
        }

        public bool Exit()
        {
            Fx.Assert(this.refCount > 0, "We should guard against too many exits elsewhere.");

            this.refCount--;
            this.executor.ExitNoPersist();

            return this.refCount == 0;
        }

        public void Register(RegistrationContext context)
        {
        }

        public void Unregister(RegistrationContext context)
        {
            if (this.refCount > 0)
            {
                for (int i = 0; i < this.refCount; i++)
                {
                    this.executor.ExitNoPersist();
                }

                this.refCount = 0;
            }
        }
    }
}


