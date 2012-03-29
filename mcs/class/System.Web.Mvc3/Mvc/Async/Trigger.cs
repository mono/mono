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
