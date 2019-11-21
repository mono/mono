namespace System.Web.DynamicData.Util {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Globalization;
    using System.Threading;

     internal static class StringLocalizerUtil {
        public static string GetLocalizedString(ValidationAttribute attribute, string displayName) {
            string errorMsg = null;

             if (UseStringLocalizerProvider(attribute)) {
                if (attribute is RangeAttribute) {
                    var attr = (RangeAttribute)attribute;
                    errorMsg = GetLocalizedString(attribute.ErrorMessage, displayName, attr.Minimum, attr.Maximum);
                }
                else if (attribute is RegularExpressionAttribute) {
                    var attr = (RegularExpressionAttribute)attribute;
                    errorMsg = GetLocalizedString(attribute.ErrorMessage, displayName, attr.Pattern);
                }
                else if (attribute is StringLengthAttribute) {
                    var attr = (StringLengthAttribute)attribute;
                    errorMsg = GetLocalizedString(attribute.ErrorMessage, displayName, attr.MinimumLength, attr.MaximumLength);
                }
                else if (attribute is MinLengthAttribute) {
                    var attr = (MinLengthAttribute)attribute;
                    errorMsg = GetLocalizedString(attribute.ErrorMessage, displayName, attr.Length);
                }
                else if (attribute is MaxLengthAttribute) {
                    var attr = (MaxLengthAttribute)attribute;
                    errorMsg = GetLocalizedString(attribute.ErrorMessage, displayName, attr.Length);
                }
                else {
                    errorMsg = GetLocalizedString(attribute.ErrorMessage);
                }
            }

             return errorMsg ?? attribute.FormatErrorMessage(displayName);
        }

        private static bool UseStringLocalizerProvider(ValidationAttribute attribute) {
            // if the developer already uses existing localization feature,
            // then we don't opt in the new localization feature.
            return (!string.IsNullOrEmpty(attribute.ErrorMessage) &&
                        string.IsNullOrEmpty(attribute.ErrorMessageResourceName) &&
                        attribute.ErrorMessageResourceType == null);
        }

        private static string GetLocalizedString(string name, params object[] arguments) {
            return StringLocalizerProviders.DataAnnotationStringLocalizerProvider
               .GetLocalizedString(Thread.CurrentThread.CurrentUICulture, name, arguments);
        }
    }
}