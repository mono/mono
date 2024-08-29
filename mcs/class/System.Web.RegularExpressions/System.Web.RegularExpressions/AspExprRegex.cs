using System;
using System.Text.RegularExpressions;

namespace System.Web.RegularExpressions
{
  public class AspExprRegex : Regex
  {
    public AspExprRegex()
      : base("\\G<%\\s*?=(?<code>.*?)?%>", RegexOptions.Multiline | RegexOptions.Singleline)
    {
    }
  }
}
