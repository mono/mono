namespace System.Web.Mvc {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;

    internal static class DescriptorUtil {

        private static void AppendPartToUniqueIdBuilder(StringBuilder builder, object part) {
            if (part == null) {
                builder.Append("[-1]");
            }
            else {
                string partString = Convert.ToString(part, CultureInfo.InvariantCulture);
                builder.AppendFormat("[{0}]{1}", partString.Length, partString);
            }
        }

        public static string CreateUniqueId(params object[] parts) {
            return CreateUniqueId((IEnumerable<object>)parts);
        }

        public static string CreateUniqueId(IEnumerable<object> parts) {
            // returns a unique string made up of the pieces passed in
            StringBuilder builder = new StringBuilder();
            foreach (object part in parts) {
                // We can special-case certain part types

                MemberInfo memberInfo = part as MemberInfo;
                if (memberInfo != null) {
                    AppendPartToUniqueIdBuilder(builder, memberInfo.Module.ModuleVersionId);
                    AppendPartToUniqueIdBuilder(builder, memberInfo.MetadataToken);
                    continue;
                }

                IUniquelyIdentifiable uniquelyIdentifiable = part as IUniquelyIdentifiable;
                if (uniquelyIdentifiable != null) {
                    AppendPartToUniqueIdBuilder(builder, uniquelyIdentifiable.UniqueId);
                    continue;
                }

                AppendPartToUniqueIdBuilder(builder, part);
            }

            return builder.ToString();
        }

        public static TDescriptor[] LazilyFetchOrCreateDescriptors<TReflection, TDescriptor>(ref TDescriptor[] cacheLocation, Func<TReflection[]> initializer, Func<TReflection, TDescriptor> converter) {
            // did we already calculate this once?
            TDescriptor[] existingCache = Interlocked.CompareExchange(ref cacheLocation, null, null);
            if (existingCache != null) {
                return existingCache;
            }

            TReflection[] memberInfos = initializer();
            TDescriptor[] descriptors = memberInfos.Select(converter).Where(descriptor => descriptor != null).ToArray();
            TDescriptor[] updatedCache = Interlocked.CompareExchange(ref cacheLocation, descriptors, null);
            return updatedCache ?? descriptors;
        }

    }
}
