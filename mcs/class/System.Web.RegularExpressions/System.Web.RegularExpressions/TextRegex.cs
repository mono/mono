using System;
using System.Text.RegularExpressions;

namespace System.Web.RegularExpressions
{
  public class TextRegex : Regex
  {
    public TextRegex()
      : base("\\G[^<]+", RegexOptions.Multiline | RegexOptions.Singleline)
    {
    }
  }
}
