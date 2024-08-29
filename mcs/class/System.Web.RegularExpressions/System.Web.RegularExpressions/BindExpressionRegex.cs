using System;
using System.Text.RegularExpressions;

namespace System.Web.RegularExpressions
{
  internal class BindExpressionRegex : Regex
  {
    public BindExpressionRegex()
      : base("^\\s*bind\\s*\\((?<params>.*)\\)\\s*\\z", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.CultureInvariant)
    {
    }
  }
}
