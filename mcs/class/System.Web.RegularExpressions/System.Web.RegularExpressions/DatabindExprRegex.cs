using System;
using System.Text.RegularExpressions;

namespace System.Web.RegularExpressions
{
  public class DatabindExprRegex : Regex
  {
    public DatabindExprRegex()
      : base("\\G<%#(?<encode>:)?(?<code>.*?)?%>", RegexOptions.Multiline | RegexOptions.Singleline)
    {
    }
  }
}
