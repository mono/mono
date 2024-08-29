using System;
using System.Text.RegularExpressions;

namespace System.Web.RegularExpressions
{
  public class DirectiveRegex : Regex
  {
    public DirectiveRegex()
      : base("\\G<%\\s*@(\\s*(?<attrname>\\w[\\w:]*(?=\\W))(\\s*(?<equal>=)\\s*\"(?<attrval>[^\"]*)\"|\\s*(?<equal>=)\\s*'(?<attrval>[^']*)'|\\s*(?<equal>=)\\s*(?<attrval>[^\\s\"'%>]*)|(?<equal>)(?<attrval>\\s*?)))*\\s*?%>", RegexOptions.Multiline | RegexOptions.Singleline)
    {
    }
  }
}
