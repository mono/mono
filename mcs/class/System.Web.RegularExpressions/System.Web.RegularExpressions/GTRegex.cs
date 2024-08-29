using System;
using System.Text.RegularExpressions;

namespace System.Web.RegularExpressions
{
  public class GTRegex : Regex
  {
    public GTRegex()
      : base("[^%]>", RegexOptions.Multiline | RegexOptions.Singleline)
    {
    }
  }
}
