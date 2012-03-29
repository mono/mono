namespace System.Web.Mvc {
    using System;
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "No compelling performance reason to seal this type.")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class ValidateInputAttribute : FilterAttribute, IAuthorizationFilter {

        public ValidateInputAttribute(bool enableValidation) {
            EnableValidation = enableValidation;
        }

        public bool EnableValidation {
            get;
            private set;
        }

        public virtual void OnAuthorization(AuthorizationContext filterContext) {
            if (filterContext == null) {
                throw new ArgumentNullException("filterContext");
            }

            filterContext.Controller.ValidateRequest = EnableValidation;
        }

    }
}
