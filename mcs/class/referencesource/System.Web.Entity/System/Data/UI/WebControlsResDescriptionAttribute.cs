using System.ComponentModel;

namespace System.Web.UI
{
  [AttributeUsage(AttributeTargets.All)]
  internal sealed class WebControlsResDescriptionAttribute : DescriptionAttribute
  {
    private bool replaced;

    public WebControlsResDescriptionAttribute(string description)
      : base(description)
    {
    }

    public override string Description
    {
      get
      {
        if (!this.replaced)
        {
          this.replaced = true;
          this.DescriptionValue = WebControlsRes.GetString(base.Description);
        }
        return base.Description;
      }
    }
  }
}
