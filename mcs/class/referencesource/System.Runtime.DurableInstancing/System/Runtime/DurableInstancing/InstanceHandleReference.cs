//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Runtime.DurableInstancing
{
    // This class serves as a reference back to an InstanceHandle from the perspective of an InstanceOwner for tracking lock binds in progress.
    // It works in two modes, one as a pure cancelable handle reference (where cancelling means nulling out the reference) and the
    // other as a queue position marker for determining when all of the in-progress requests at one point in time are all done.  In the
    // marker mode (InstanceOwner.LockResolutionMarker), it carries some additional context for maintaining the state of the
    // InstancePersistenceContext.ResolveExistingLock operation.
    class InstanceHandleReference
    {
        internal InstanceHandleReference(InstanceHandle instanceHandle)
        {
            Fx.Assert(instanceHandle != null, "Null instanceHandle provided to InstanceHandleReference.");
            InstanceHandle = instanceHandle;
        }

        // This is set to null when the InstanceHandleReference is detached from the InstanceHandle - i.e. it is
        // no longer in use, and exists just to make it possible to lazily drain out of the various queues.
        internal InstanceHandle InstanceHandle { get; private set; }

        internal void Cancel()
        {
            Fx.Assert(InstanceHandle != null, "InstanceHandleReference already cancelled.");
            InstanceHandle = null;
        }
    }
}
