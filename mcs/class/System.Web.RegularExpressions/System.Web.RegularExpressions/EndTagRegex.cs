using System;
using System.Text.RegularExpressions;

namespace System.Web.RegularExpressions
{
  public class EndTagRegex : Regex
  {
    public EndTagRegex()
      : base("\\G</(?<tagname>[\\w:\\.]+)\\s*>", RegexOptions.Multiline | RegexOptions.Singleline)
    {
    }
  }
}
