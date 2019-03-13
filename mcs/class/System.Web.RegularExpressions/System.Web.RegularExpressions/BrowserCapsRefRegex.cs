using System;
using System.Text.RegularExpressions;

namespace System.Web.RegularExpressions
{
  internal class BrowserCapsRefRegex : Regex
  {
    public BrowserCapsRefRegex()
      : base("\\$(?:\\{(?<name>\\w+)\\})", RegexOptions.Multiline | RegexOptions.Singleline)
    {
    }
  }
}
