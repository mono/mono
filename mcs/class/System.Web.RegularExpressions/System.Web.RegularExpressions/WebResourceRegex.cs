using System;
using System.Text.RegularExpressions;

namespace System.Web.RegularExpressions
{
  internal class WebResourceRegex : Regex
  {
    public WebResourceRegex()
      : base("<%\\s*=\\s*WebResource\\(\"(?<resourceName>[^\"]*)\"\\)\\s*%>", RegexOptions.Multiline | RegexOptions.Singleline)
    {
    }
  }
}
