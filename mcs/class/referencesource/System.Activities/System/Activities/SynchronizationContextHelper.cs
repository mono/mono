//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System.Runtime;
    using System.Threading;

    static class SynchronizationContextHelper
    {
        static WFDefaultSynchronizationContext defaultContext;

        public static SynchronizationContext GetDefaultSynchronizationContext()
        {
            if (SynchronizationContextHelper.defaultContext == null)
            {
                SynchronizationContextHelper.defaultContext = new WFDefaultSynchronizationContext();
            }
            return SynchronizationContextHelper.defaultContext;
        }

        public static SynchronizationContext CloneSynchronizationContext(SynchronizationContext context)
        {
            Fx.Assert(context != null, "null context parameter");
            WFDefaultSynchronizationContext wfDefaultContext = context as WFDefaultSynchronizationContext;
            if (wfDefaultContext != null)
            {
                Fx.Assert(SynchronizationContextHelper.defaultContext != null, "We must have set the static member by now!");
                return SynchronizationContextHelper.defaultContext;
            }
            else
            {
                return context.CreateCopy();
            }
        }

        class WFDefaultSynchronizationContext : SynchronizationContext
        {
            public WFDefaultSynchronizationContext()
            {
            }

            public override void Post(SendOrPostCallback d, object state)
            {
                ActionItem.Schedule(delegate(object s) { d(s); }, state);
            }

            public override void Send(SendOrPostCallback d, object state)
            {
                d(state);
            }
        }
    }
}
