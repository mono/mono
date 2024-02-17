using System.ComponentModel;

namespace System.Web.UI
{
  [AttributeUsage(AttributeTargets.All)]
  internal sealed class WebControlsResCategoryAttribute : CategoryAttribute
  {
    public WebControlsResCategoryAttribute(string category)
      : base(category)
    {
    }

    protected override string GetLocalizedString(string value)
    {
      return WebControlsRes.GetString(value);
    }
  }
}