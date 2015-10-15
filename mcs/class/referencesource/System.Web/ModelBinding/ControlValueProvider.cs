using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace System.Web.ModelBinding {

    public sealed class ControlValueProvider : SimpleValueProvider {

        public string PropertyName {
            get;
            private set;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.Web.ModelBinding.SimpleValueProvider.#ctor(System.Web.ModelBinding.ModelBindingExecutionContext)",
            Justification = "SimpleValueProvider Constructor specifies the CultureInfo")]
        public ControlValueProvider(ModelBindingExecutionContext modelBindingExecutionContext, string propertyName)
            : base(modelBindingExecutionContext) {
                PropertyName = propertyName;
        }

        protected override object FetchValue(string controlId) {
            if (String.IsNullOrEmpty(controlId)) {
                return null;
            }

            Control dataControl = ModelBindingExecutionContext.GetService<Control>();

            //Following code taken from ControlParameter - code duplicated because ControlPrameter throws exceptions whereas we do not.
            string propertyName = PropertyName;

            //
            Control foundControl = dataControl.FindControl(controlId) ?? DataBoundControlHelper.FindControl(dataControl, controlId);

            if (foundControl == null) {
                return null;
            }

            ControlValuePropertyAttribute controlValueProp = (ControlValuePropertyAttribute)TypeDescriptor.GetAttributes(foundControl)[typeof(ControlValuePropertyAttribute)];

            // If no property name is specified, use the ControlValuePropertyAttribute to determine which property to use.
            if (String.IsNullOrEmpty(propertyName)) {
                if ((controlValueProp != null) && (!String.IsNullOrEmpty(controlValueProp.Name))) {
                    propertyName = controlValueProp.Name;
                }
                else {
                    return null;
                }
            }

            // Get the value of the property
            object value = DataBinder.Eval(foundControl, propertyName);

            // Convert the value to null if this is the default property and the value is the property's default value
            if (controlValueProp != null && 
                controlValueProp.DefaultValue != null &&
                controlValueProp.DefaultValue.Equals(value)) {
                return null;
            }
            return value;
        }
    }
}
