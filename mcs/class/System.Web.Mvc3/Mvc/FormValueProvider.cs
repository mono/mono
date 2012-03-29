namespace System.Web.Mvc {
    using System;
    using System.Globalization;
    using System.Web.Helpers;

    public sealed class FormValueProvider : NameValueCollectionValueProvider {

        public FormValueProvider(ControllerContext controllerContext)
            : this(controllerContext, new UnvalidatedRequestValuesWrapper(controllerContext.HttpContext.Request.Unvalidated())) {
        }

        // For unit testing
        internal FormValueProvider(ControllerContext controllerContext, IUnvalidatedRequestValues unvalidatedValues)
            : base(controllerContext.HttpContext.Request.Form, unvalidatedValues.Form, CultureInfo.CurrentCulture) {
        }

    }
}
