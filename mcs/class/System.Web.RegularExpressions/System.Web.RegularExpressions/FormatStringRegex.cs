using System;
using System.Text.RegularExpressions;

namespace System.Web.RegularExpressions
{
  internal class FormatStringRegex : Regex
  {
    public FormatStringRegex()
      : base("^(([^\"]*(\"\")?)*)$", RegexOptions.Multiline | RegexOptions.Singleline)
    {
    }
  }
}
