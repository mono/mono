using System;
using System.Text.RegularExpressions;

namespace System.Web.RegularExpressions
{
  public class AspEncodedExprRegex : Regex
  {
    public AspEncodedExprRegex()
      : base("\\G<%:(?<code>.*?)?%>", RegexOptions.Multiline | RegexOptions.Singleline)
    {
    }
  }
}
