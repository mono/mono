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

    // used to synchronize access to a single-use consumable resource
    internal sealed class SingleEntryGate {

        private const int NOT_ENTERED = 0;
        private const int ENTERED = 1;

        private int _status;

        // returns true if this is the first call to TryEnter(), false otherwise
        public bool TryEnter() {
            int oldStatus = Interlocked.Exchange(ref _status, ENTERED);
            return (oldStatus == NOT_ENTERED);
        }

    }
}
