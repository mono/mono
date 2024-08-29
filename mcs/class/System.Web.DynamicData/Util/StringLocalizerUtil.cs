using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Web.Globalization;

namespace System.Web.DynamicData.Util
{
  internal static class StringLocalizerUtil
  {
    public static string GetLocalizedString(ValidationAttribute attribute, string displayName)
    {
      string str = (string) null;

      if (StringLocalizerUtil.UseStringLocalizerProvider(attribute))
      {
        if (attribute is RangeAttribute)
        {
          RangeAttribute rangeAttribute = (RangeAttribute) attribute;
          str = StringLocalizerUtil.GetLocalizedString(attribute.ErrorMessage, (object) displayName, rangeAttribute.Minimum, rangeAttribute.Maximum);
        }
        else if (attribute is RegularExpressionAttribute)
        {
          RegularExpressionAttribute expressionAttribute = (RegularExpressionAttribute) attribute;
          str = StringLocalizerUtil.GetLocalizedString(attribute.ErrorMessage, (object) displayName, (object) expressionAttribute.Pattern);
        }
        else if (attribute is StringLengthAttribute)
        {
          StringLengthAttribute stringLengthAttribute = (StringLengthAttribute) attribute;
          str = StringLocalizerUtil.GetLocalizedString(attribute.ErrorMessage, (object) displayName, (object) stringLengthAttribute.MinimumLength, (object) stringLengthAttribute.MaximumLength);
        }
        else if (attribute is MinLengthAttribute)
        {
          MinLengthAttribute minLengthAttribute = (MinLengthAttribute) attribute;
          str = StringLocalizerUtil.GetLocalizedString(attribute.ErrorMessage, (object) displayName, (object) minLengthAttribute.Length);
        }
        else if (attribute is MaxLengthAttribute)
        {
          MaxLengthAttribute maxLengthAttribute = (MaxLengthAttribute) attribute;
          str = StringLocalizerUtil.GetLocalizedString(attribute.ErrorMessage, (object) displayName, (object) maxLengthAttribute.Length);
        }
        else
          str = StringLocalizerUtil.GetLocalizedString(attribute.ErrorMessage);
      }

      return str ?? attribute.FormatErrorMessage(displayName);
    }

    private static bool UseStringLocalizerProvider(ValidationAttribute attribute)
    {
      if (!string.IsNullOrEmpty(attribute.ErrorMessage) && string.IsNullOrEmpty(attribute.ErrorMessageResourceName))
        return attribute.ErrorMessageResourceType == (Type) null;
      
      return false;
    }

    private static string GetLocalizedString(string name, params object[] arguments)
    {
      return StringLocalizerProviders.DataAnnotationStringLocalizerProvider.GetLocalizedString(Thread.CurrentThread.CurrentUICulture, name, arguments);
    }
  }
}
