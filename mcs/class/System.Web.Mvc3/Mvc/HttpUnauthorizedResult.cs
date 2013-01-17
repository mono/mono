namespace System.Web.Mvc {

    public class HttpUnauthorizedResult : HttpStatusCodeResult {

        // HTTP 401 is the status code for unauthorized access. Other code might
        // intercept this and perform some special logic. For example, the
        // FormsAuthenticationModule looks for 401 responses and instead redirects
        // the user to the login page.
        private const int UnauthorizedCode = 401;

        public HttpUnauthorizedResult()
            : this(null) {
        }

        public HttpUnauthorizedResult(string statusDescription)
            : base(UnauthorizedCode, statusDescription) {
        }
    }
}
