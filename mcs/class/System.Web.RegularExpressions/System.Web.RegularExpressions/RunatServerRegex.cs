using System;
using System.Text.RegularExpressions;

namespace System.Web.RegularExpressions
{
  public class RunatServerRegex : Regex
  {
    public RunatServerRegex()
      : base("runat\\W*server", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.CultureInvariant)
    {
    }
  }
}
