namespace System.Web.ModelBinding {
    using System;
    using System.Diagnostics.CodeAnalysis;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "This class is designed to be overridden")]
    public class BindingBehaviorAttribute : Attribute {

        private static readonly object _typeId = new object();

        public BindingBehaviorAttribute(BindingBehavior behavior) {
            Behavior = behavior;
        }

        public BindingBehavior Behavior {
            get;
            private set;
        }

        public override object TypeId {
            get {
                return _typeId;
            }
        }

    }
}
