//
// System.Web.HttpCacheValidateHandler.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
//

namespace System.Web
{
        public delegate void HttpCacheValidateHandler(
                                HttpContext context,
                                object data,
                                ref HttpValidationStatus validationStatus);
}
