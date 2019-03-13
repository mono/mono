using System;
using System.Text.RegularExpressions;

namespace System.Web.RegularExpressions
{
  internal class EvalExpressionRegex : Regex
  {
    public EvalExpressionRegex()
      : base("^\\s*eval\\s*\\((?<params>.*)\\)\\s*\\z", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.CultureInvariant)
    {
    }
  }
}
