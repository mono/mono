namespace System.Web.Mvc {
    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class ChildActionOnlyAttribute : FilterAttribute, IAuthorizationFilter {

        public void OnAuthorization(AuthorizationContext filterContext) {
            if (filterContext == null) {
                throw new ArgumentNullException("filterContext");
            }

            if (!filterContext.IsChildAction) {
                throw Error.ChildActionOnlyAttribute_MustBeInChildRequest(filterContext.ActionDescriptor);
            }
        }

    }
}
