namespace System.Web.DynamicData.Util {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Threading;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Globalization;

    internal static class DisplayAttributeExtensions {
        public static string GetLocalizedDescription(this DisplayAttribute attribute) {
            return GetLocalizedString(attribute, () => attribute.Description) ?? 
                attribute.GetPropertyValue(a => a.GetDescription(), null);
        }

        public static string GetLocalizedName(this DisplayAttribute attribute) {
            return GetLocalizedString(attribute, () => attribute.Name) ?? 
                attribute.GetPropertyValue(a => a.GetName(), null);
        }

        public static string GetLocalizedShortName(this DisplayAttribute attribute) {
            return GetLocalizedString(attribute, () => attribute.ShortName) ?? 
                attribute.GetPropertyValue(a => a.GetShortName(), null);
        }

        public static string GetLocalizedPrompt(this DisplayAttribute attribute) {
            return GetLocalizedString(attribute, () => attribute.Prompt) ??
                attribute.GetPropertyValue(a => a.GetPrompt(), null);
        }

        private static string GetLocalizedString(DisplayAttribute attribute, Func<string> getString) {
            string localizedString = null;

            if (attribute != null && attribute.ResourceType == null &&
                !string.IsNullOrEmpty(getString())) {
                localizedString = StringLocalizerProviders.DataAnnotationStringLocalizerProvider
                                    .GetLocalizedString(Thread.CurrentThread.CurrentUICulture, getString());
            }

            // It's possible localizedString is null or empty.
            // Should fall back to the old logic, if it is the case.
            return localizedString;
        }
    }
}