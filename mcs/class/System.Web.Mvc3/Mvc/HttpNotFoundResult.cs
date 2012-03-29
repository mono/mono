namespace System.Web.Mvc {

    public class HttpNotFoundResult : HttpStatusCodeResult {

        // HTTP 404 is the status code for Not Found
        private const int NotFoundCode = 404;

        public HttpNotFoundResult()
            : this(null) {
        }

        public HttpNotFoundResult(string statusDescription)
            : base(NotFoundCode, statusDescription) {
        }
    }
}
