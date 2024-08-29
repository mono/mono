using System;
using System.Text.RegularExpressions;

namespace System.Web.RegularExpressions
{
  internal class BindItemParametersRegex : Regex
  {
    public BindItemParametersRegex()
      : base("(?<fieldName>([\\w\\.]+))\\s*\\z", RegexOptions.Multiline | RegexOptions.Singleline)
    {
    }
  }
}
