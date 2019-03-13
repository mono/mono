using System;
using System.Text.RegularExpressions;

namespace System.Web.RegularExpressions
{
  internal class BindItemExpressionRegex : Regex
  {
    public BindItemExpressionRegex()
      : base("^\\s*BindItem\\.(?<params>.*)\\s*\\z", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.CultureInvariant)
    {
    }
  }
}
