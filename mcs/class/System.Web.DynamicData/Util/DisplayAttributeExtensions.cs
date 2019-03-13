using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Web.Globalization;

namespace System.Web.DynamicData.Util
{
  internal static class DisplayAttributeExtensions
  {
    public static string GetLocalizedDescription(this DisplayAttribute attribute)
    {
      string localizedString = DisplayAttributeExtensions.GetLocalizedString(attribute, (Func<string>) (() => attribute.Description));
      if (localizedString != null)
        return localizedString;
      DisplayAttribute attribute1 = attribute;
      // ISSUE: variable of the null type
      string local = null;
      return attribute1.GetPropertyValue<DisplayAttribute, string>((Func<DisplayAttribute, string>) (a => a.GetDescription()), (string) local);
    }

    public static string GetLocalizedName(this DisplayAttribute attribute)
    {
      string localizedString = DisplayAttributeExtensions.GetLocalizedString(attribute, (Func<string>) (() => attribute.Name));
      if (localizedString != null)
        return localizedString;
      DisplayAttribute attribute1 = attribute;
      // ISSUE: variable of the null type
      string local = null;
      return attribute1.GetPropertyValue<DisplayAttribute, string>((Func<DisplayAttribute, string>) (a => a.GetName()), (string) local);
    }

    public static string GetLocalizedShortName(this DisplayAttribute attribute)
    {
      string localizedString = DisplayAttributeExtensions.GetLocalizedString(attribute, (Func<string>) (() => attribute.ShortName));
      if (localizedString != null)
        return localizedString;
      DisplayAttribute attribute1 = attribute;
      // ISSUE: variable of the null type
      string local = null;
      return attribute1.GetPropertyValue<DisplayAttribute, string>((Func<DisplayAttribute, string>) (a => a.GetShortName()), (string) local);
    }

    public static string GetLocalizedPrompt(this DisplayAttribute attribute)
    {
      string localizedString = DisplayAttributeExtensions.GetLocalizedString(attribute, (Func<string>) (() => attribute.Prompt));
      if (localizedString != null)
        return localizedString;
      DisplayAttribute attribute1 = attribute;
      // ISSUE: variable of the null type
      string local = null;
      return attribute1.GetPropertyValue<DisplayAttribute, string>((Func<DisplayAttribute, string>) (a => a.GetPrompt()), (string) local);
    }

    private static string GetLocalizedString(DisplayAttribute attribute, Func<string> getString)
    {
      string str = (string) null;
      if (attribute != null && attribute.ResourceType == (Type) null && !string.IsNullOrEmpty(getString()))
        str = StringLocalizerProviders.DataAnnotationStringLocalizerProvider.GetLocalizedString(Thread.CurrentThread.CurrentUICulture, getString(), new object[0]);
      return str;
    }
  }
}
