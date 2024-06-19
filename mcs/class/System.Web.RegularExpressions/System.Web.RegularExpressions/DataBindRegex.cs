using System;
using System.Text.RegularExpressions;

namespace System.Web.RegularExpressions
{
  public class DataBindRegex : Regex
  {
    public DataBindRegex()
      : base("\\G\\s*<%\\s*?#(?<encode>:)?(?<code>.*?)?%>\\s*\\z", RegexOptions.Multiline | RegexOptions.Singleline)
    {
    }
  }
}
