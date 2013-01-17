namespace System.Web.Mvc {
    using System;
    using System.Web;

    public class HttpPostedFileBaseModelBinder : IModelBinder {

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext) {
            if (controllerContext == null) {
                throw new ArgumentNullException("controllerContext");
            }
            if (bindingContext == null) {
                throw new ArgumentNullException("bindingContext");
            }

            HttpPostedFileBase theFile = controllerContext.HttpContext.Request.Files[bindingContext.ModelName];
            return ChooseFileOrNull(theFile);
        }

        // helper that returns the original file if there was content uploaded, null if empty
        internal static HttpPostedFileBase ChooseFileOrNull(HttpPostedFileBase rawFile) {
            // case 1: there was no <input type="file" ... /> element in the post
            if (rawFile == null) {
                return null;
            }

            // case 2: there was an <input type="file" ... /> element in the post, but it was left blank
            if (rawFile.ContentLength == 0 && String.IsNullOrEmpty(rawFile.FileName)) {
                return null;
            }

            // case 3: the file was posted
            return rawFile;
        }

    }
}
