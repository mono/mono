using System;
using System.Text.RegularExpressions;

namespace System.Web.RegularExpressions
{
  internal class ExpressionBuilderRegex : Regex
  {
    public ExpressionBuilderRegex()
      : base("\\G\\s*<%\\s*\\$\\s*(?<code>.*)?%>\\s*\\z", RegexOptions.Multiline | RegexOptions.Singleline)
    {
    }
  }
}
