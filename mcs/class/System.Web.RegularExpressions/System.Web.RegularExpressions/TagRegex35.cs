using System;
using System.Text.RegularExpressions;

namespace System.Web.RegularExpressions
{
  public class TagRegex35 : Regex
  {
    public TagRegex35()
      : base("\\G<(?<tagname>[\\w:\\.]+)(\\s+(?<attrname>\\w[-\\w:]*)(\\s*=\\s*\"(?<attrval>[^\"]*)\"|\\s*=\\s*'(?<attrval>[^']*)'|\\s*=\\s*(?<attrval><%#.*?%>)|\\s*=\\s*(?<attrval>[^\\s=/>]*)|(?<attrval>\\s*?)))*\\s*(?<empty>/)?>", RegexOptions.Multiline | RegexOptions.Singleline)
    {
    }
  }
}
