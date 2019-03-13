using System;
using System.Text.RegularExpressions;

namespace System.Web.RegularExpressions
{
  public class ServerTagsRegex : Regex
  {
    public ServerTagsRegex()
      : base("<%(?![#$])(([^%]*)%)*?>", RegexOptions.Multiline | RegexOptions.Singleline)
    {
    }
  }
}
