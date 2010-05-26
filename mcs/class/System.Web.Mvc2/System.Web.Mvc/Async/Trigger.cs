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
    using System.Runtime.Serialization;

    // Provides a trigger for the TriggerListener class.

    internal sealed class Trigger {

        private readonly Action _fireAction;

        // Constructor should only be called by TriggerListener.
        internal Trigger(Action fireAction) {
            _fireAction = fireAction;
        }

        public void Fire() {
            _fireAction();
        }

    }
}
