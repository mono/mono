namespace System.Web.Mvc {
    using System;
    using System.Text;
    using System.Web;

    public class ContentResult : ActionResult {

        public string Content {
            get;
            set;
        }

        public Encoding ContentEncoding {
            get;
            set;
        }

        public string ContentType {
            get;
            set;
        }

        public override void ExecuteResult(ControllerContext context) {
            if (context == null) {
                throw new ArgumentNullException("context");
            }

            HttpResponseBase response = context.HttpContext.Response;

            if (!String.IsNullOrEmpty(ContentType)) {
                response.ContentType = ContentType;
            }
            if (ContentEncoding != null) {
                response.ContentEncoding = ContentEncoding;
            }
            if (Content != null) {
                response.Write(Content);
            }
        }
    }
}
