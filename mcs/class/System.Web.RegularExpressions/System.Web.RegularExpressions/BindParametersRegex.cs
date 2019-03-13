using System;
using System.Text.RegularExpressions;

namespace System.Web.RegularExpressions
{
  internal class BindParametersRegex : Regex
  {
    public BindParametersRegex()
      : base("\\s*((\"(?<fieldName>(([\\w\\.]+)|(\\[.+\\])))\")|('(?<fieldName>(([\\w\\.]+)|(\\[.+\\])))'))\\s*(,\\s*((\"(?<formatString>.*)\")|('(?<formatString>.*)'))\\s*)?\\s*\\z", RegexOptions.Multiline | RegexOptions.Singleline)
    {
    }
  }
}
