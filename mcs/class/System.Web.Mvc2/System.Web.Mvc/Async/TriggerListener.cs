/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc.Async {
    using System;
    using System.Threading;

    // This class is used to wait for triggers and a continuation. When the continuation has been provded
    // and all triggers have been fired, the continuation is called. Similar to WaitHandle.WaitAll().
    // New instances of this type are initially in the inactive state; activation is enabled by a call
    // to Activate().

    // This class is thread-safe.

    internal sealed class TriggerListener {

        private readonly Trigger _activateTrigger;
        private volatile Action _continuation;
        private readonly SingleEntryGate _continuationFiredGate = new SingleEntryGate();
        private int _outstandingTriggers;
        private readonly Trigger _setContinuationTrigger;

        public TriggerListener() {
            _activateTrigger = CreateTrigger();
            _setContinuationTrigger = CreateTrigger();
        }

        public void Activate() {
            _activateTrigger.Fire();
        }

        public Trigger CreateTrigger() {
            Interlocked.Increment(ref _outstandingTriggers);

            SingleEntryGate triggerFiredGate = new SingleEntryGate();
            return new Trigger(() => {
                if (triggerFiredGate.TryEnter()) {
                    HandleTriggerFired();
                }
            });
        }

        private void HandleTriggerFired() {
            if (Interlocked.Decrement(ref _outstandingTriggers) == 0) {
                if (_continuationFiredGate.TryEnter()) {
                    _continuation();
                }
            }
        }

        public void SetContinuation(Action continuation) {
            if (continuation != null) {
                _continuation = continuation;
                _setContinuationTrigger.Fire();
            }
        }

    }
}
