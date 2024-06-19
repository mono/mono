using System;
using System.Text.RegularExpressions;

namespace System.Web.RegularExpressions
{
  public class AspCodeRegex : Regex
  {
    public AspCodeRegex()
      : base("\\G<%(?!@)(?<code>.*?)%>", RegexOptions.Multiline | RegexOptions.Singleline)
    {
    }
  }
}
