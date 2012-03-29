namespace System.Web.Mvc {
    using System;
    using System.Diagnostics.CodeAnalysis;

    public class ExceptionContext : ControllerContext {

        private ActionResult _result;

        // parameterless constructor used for mocking
        public ExceptionContext() {
        }

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "The virtual property setters are only to support mocking frameworks, in which case this constructor shouldn't be called anyway.")]
        public ExceptionContext(ControllerContext controllerContext, Exception exception)
            : base(controllerContext) {
            if (exception == null) {
                throw new ArgumentNullException("exception");
            }

            Exception = exception;
        }

        public virtual Exception Exception {
            get;
            set;
        }

        public bool ExceptionHandled {
            get;
            set;
        }

        public ActionResult Result {
            get {
                return _result ?? EmptyResult.Instance;
            }
            set {
                _result = value;
            }
        }

    }
}
