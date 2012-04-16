namespace System.Web.Mvc {
    using System;
    using System.Diagnostics;
    using System.Web;
    using System.Web.Helpers;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class ValidateAntiForgeryTokenAttribute : FilterAttribute, IAuthorizationFilter {

        private string _salt;

        public string Salt {
            get {
                return _salt ?? String.Empty;
            }
            set {
                _salt = value;
            }
        }

        internal Action ValidateAction {
            get;
            private set;
        }

        public ValidateAntiForgeryTokenAttribute()
            : this(AntiForgery.Validate) {
        }

	//Modified to compile MVC3 with the newer System.Web.WebPages helpers
        internal ValidateAntiForgeryTokenAttribute(Action validateAction) {
            Debug.Assert(validateAction != null);
            ValidateAction = validateAction;
        }

        public void OnAuthorization(AuthorizationContext filterContext) {
            if (filterContext == null) {
                throw new ArgumentNullException("filterContext");
            }

	    //Modified to compile MVC3 with the newer System.Web.WebPages helpers
            ValidateAction();
        }
    }
}
