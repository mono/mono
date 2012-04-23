namespace System.Web.Mvc {
    using System;
    using System.Text;
    using System.Web;
    using System.Web.Mvc.Resources;
    using System.Web.Script.Serialization;

    public class JsonResult : ActionResult {

        public JsonResult() {
            JsonRequestBehavior = JsonRequestBehavior.DenyGet;
        }

        public Encoding ContentEncoding {
            get;
            set;
        }

        public string ContentType {
            get;
            set;
        }

        public object Data {
            get;
            set;
        }

        public JsonRequestBehavior JsonRequestBehavior {
            get;
            set;
        }

        public override void ExecuteResult(ControllerContext context) {
            if (context == null) {
                throw new ArgumentNullException("context");
            }
            if (JsonRequestBehavior == JsonRequestBehavior.DenyGet &&
                String.Equals(context.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase)) {
                throw new InvalidOperationException(MvcResources.JsonRequest_GetNotAllowed);
            }

            HttpResponseBase response = context.HttpContext.Response;

            if (!String.IsNullOrEmpty(ContentType)) {
                response.ContentType = ContentType;
            }
            else {
                response.ContentType = "application/json";
            }
            if (ContentEncoding != null) {
                response.ContentEncoding = ContentEncoding;
            }
            if (Data != null) {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                response.Write(serializer.Serialize(Data));
            }
        }
    }
}
