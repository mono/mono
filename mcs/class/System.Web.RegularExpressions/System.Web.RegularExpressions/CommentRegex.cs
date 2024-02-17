using System;
using System.Text.RegularExpressions;

namespace System.Web.RegularExpressions
{
  public class CommentRegex : Regex
  {
    public CommentRegex()
      : base("\\G<%--(([^-]*)-)*?-%>", RegexOptions.Multiline | RegexOptions.Singleline)
    {
    }
  }
}
