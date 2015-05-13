namespace System.Diagnostics.Eventing.Reader
{
    using System;
    using System.Runtime;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class EventProperty
    {
        private object value;

        internal EventProperty(object value)
        {
            this.value = value;
        }

        public object Value
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.value;
            }
        }
    }
}

