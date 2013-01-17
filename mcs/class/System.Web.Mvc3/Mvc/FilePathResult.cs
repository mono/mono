namespace System.Web.Mvc {
    using System;
    using System.Web;
    using System.Web.Mvc.Resources;

    public class FilePathResult : FileResult {

        public FilePathResult(string fileName, string contentType)
            : base(contentType) {
            if (String.IsNullOrEmpty(fileName)) {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "fileName");
            }

            FileName = fileName;
        }

        public string FileName {
            get;
            private set;
        }

        protected override void WriteFile(HttpResponseBase response) {
            response.TransmitFile(FileName);
        }

    }
}
