using System;
using System.Text.RegularExpressions;

namespace System.Web.RegularExpressions
{
  public class IncludeRegex : Regex
  {
    public IncludeRegex()
      : base("\\G<!--\\s*#(?i:include)\\s*(?<pathtype>[\\w]+)\\s*=\\s*[\"']?(?<filename>[^\\\"']*?)[\"']?\\s*-->", RegexOptions.Multiline | RegexOptions.Singleline)
    {
    }
  }
}
