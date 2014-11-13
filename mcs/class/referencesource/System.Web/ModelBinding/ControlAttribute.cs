using System.Diagnostics.CodeAnalysis;
namespace System.Web.ModelBinding {

    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public sealed class ControlAttribute : ValueProviderSourceAttribute {

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ID", Justification = "Legacy way of referring to ControlID.")]
        public string ControlID {
            get;
            private set;
        }

        public string PropertyName {
            get;
            private set;
        }

        public ControlAttribute()
            : this(null, null) {
        }


        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ID", Justification = "Legacy way of referring to ControlID.")]
        public ControlAttribute(string controlID)
            : this(controlID, null) {
        }

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ID", Justification = "Legacy way of referring to ControlID.")]
        public ControlAttribute(string controlID, string propertyName) {
            ControlID = controlID;
            PropertyName = propertyName;
        }

        public override IValueProvider GetValueProvider(ModelBindingExecutionContext modelBindingExecutionContext) {
            if (modelBindingExecutionContext == null) {
                throw new ArgumentNullException("modelBindingExecutionContext");
            }

            return new ControlValueProvider(modelBindingExecutionContext, PropertyName);
        }

        public override string GetModelName() {
            return ControlID;
        }
    }
}
