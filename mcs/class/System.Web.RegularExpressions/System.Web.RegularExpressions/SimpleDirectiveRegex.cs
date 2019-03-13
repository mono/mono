using System;
using System.Text.RegularExpressions;

namespace System.Web.RegularExpressions
{
  public class SimpleDirectiveRegex : Regex

  {
    public SimpleDirectiveRegex()
      : base("<%\\s*@(\\s*(?<attrname>\\w[\\w:]*(?=\\W))(\\s*(?<equal>=)\\s*\"(?<attrval>[^\"]*)\"|\\s*(?<equal>=)\\s*'(?<attrval>[^']*)'|\\s*(?<equal>=)\\s*(?<attrval>[^\\s\"'%>]*)|(?<equal>)(?<attrval>\\s*?)))*\\s*?%>", RegexOptions.Multiline | RegexOptions.Singleline)
    {
    }
  }
}
