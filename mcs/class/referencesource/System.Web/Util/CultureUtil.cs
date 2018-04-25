//------------------------------------------------------------------------------
// <copyright file="CultureUtil.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Util {
    using System;
    using System.Globalization;

    // This class contains helper methods for obtaining a CultureInfo instance from a list of candidates.

    internal static class CultureUtil {

        // Given a single culture name, attempts to turn it into a CultureInfo.
        // If 'requireSpecific' is set, this method attempts to return an object where IsNeutral = false.
        public static CultureInfo CreateReadOnlyCulture(string cultureName, bool requireSpecific) {
            if (requireSpecific) {
                return HttpServerUtility.CreateReadOnlySpecificCultureInfo(cultureName);
            }
            else {
                return HttpServerUtility.CreateReadOnlyCultureInfo(cultureName);
            }
        }

        // Given a list of culture names, loop through them until we find one we understand.
        // We expect 'cultureNames' to be the raw Accept-Languages header value, as we'll strip q-values.
        // Otherwise equivalent to the single-element overload.
        public static CultureInfo CreateReadOnlyCulture(string[] cultureNames, bool requireSpecific) {
            return ExtractCultureImpl(cultureNames, requireSpecific, AppSettings.MaxAcceptLanguageFallbackCount);
        }

        // for unit testing, uses 'maxCount' instead of the <appSettings> switch
        internal static CultureInfo ExtractCultureImpl(string[] cultureNames, bool requireSpecific, int maxCount) {
            int lastIndex = Math.Min(cultureNames.Length, maxCount) - 1;

            for (int i = 0; i < cultureNames.Length; i++) {
                string candidate = StripQValue(cultureNames[i]);

                try {
                    return CreateReadOnlyCulture(candidate, requireSpecific);
                }
                catch (CultureNotFoundException) {
                    // If this is the last iteration before giving up, let the exception propagate upward.
                    // Otherwise just ---- and move on to the next candidate.
                    if (i == lastIndex) {
                        throw;
                    }
                }
            }

            return null;
        }

        // Given an input "foo;q=xx", returns "foo".
        private static string StripQValue(string input) {
            if (input != null) {
                int indexOfSemicolon = input.IndexOf(';');
                if (indexOfSemicolon >= 0) {
                    return input.Substring(0, indexOfSemicolon);
                }
            }
            return input;
        }

    }
}
