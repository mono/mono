using System;
using System.Text.RegularExpressions;

namespace System.Web.RegularExpressions
{
  public class LTRegex : Regex
  {
    public LTRegex()
      : base("<[^%]", RegexOptions.Multiline | RegexOptions.Singleline)
    {
    }
  }
}
