using System.Collections.Generic;
using System.Web.WebPages;

namespace System.Web.Mvc.Razor
{
    internal delegate WebPageRenderingBase StartPageLookupDelegate(WebPageRenderingBase page, string fileName, IEnumerable<string> supportedExtensions);
}
