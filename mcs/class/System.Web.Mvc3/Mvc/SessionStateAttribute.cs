namespace System.Web.Mvc {
    using System;
    using System.Web.SessionState;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class SessionStateAttribute : Attribute {

        public SessionStateAttribute(SessionStateBehavior behavior) {
            Behavior = behavior;
        }

        public SessionStateBehavior Behavior {
            get;
            private set;
        }
    }
}
