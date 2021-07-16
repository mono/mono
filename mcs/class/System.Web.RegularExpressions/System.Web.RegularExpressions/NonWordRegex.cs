using System;
using System.Text.RegularExpressions;

namespace System.Web.RegularExpressions
{
  internal class NonWordRegex : Regex
  {
    public NonWordRegex()
      : base("\\W", RegexOptions.Multiline | RegexOptions.Singleline)
    {
    }
  }
}
