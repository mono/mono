//------------------------------------------------------------------------------
// <copyright file="PersonalizationProviderHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {
    using System.Collections;
    using System.Globalization;
    using System.Web.Util;

    internal static class PersonalizationProviderHelper {
        internal static string[] CheckAndTrimNonEmptyStringEntries(string[] array, string paramName,
                                                                   bool throwIfArrayIsNull, bool checkCommas,
                                                                   int lengthToCheck) {
            if (array == null) {
                if (throwIfArrayIsNull) {
                    throw new ArgumentNullException(paramName);
                }
                else {
                    return null;
                }
            }
            if (array.Length == 0) {
                throw new ArgumentException(SR.GetString(SR.PersonalizationProviderHelper_Empty_Collection, paramName));
            }

            string[] result = null;

            for (int i = 0; i < array.Length; i++) {
                string str = array[i];
                string trimmedStr = (str == null) ? null : str.Trim();
                if (String.IsNullOrEmpty(trimmedStr)) {
                    throw new ArgumentException(SR.GetString(SR.PersonalizationProviderHelper_Null_Or_Empty_String_Entries, paramName));
                }
                if (checkCommas && trimmedStr.IndexOf(',') != -1) {
                    throw new ArgumentException(SR.GetString(SR.PersonalizationProviderHelper_CannotHaveCommaInString, paramName, str));
                }
                if (lengthToCheck > -1 && trimmedStr.Length > lengthToCheck) {
                    throw new ArgumentException(SR.GetString(SR.PersonalizationProviderHelper_Trimmed_Entry_Value_Exceed_Maximum_Length,
                        str, paramName, lengthToCheck.ToString(CultureInfo.CurrentCulture)));
                }

                if (str.Length != trimmedStr.Length) {
                    if (result == null) {
                        result = new string[array.Length];
                        Array.Copy(array, result, i);
                    }
                }

                if (result != null) {
                    result[i] = trimmedStr;
                }
            }

            return ((result != null) ? result : array);
        }

        internal static string CheckAndTrimStringWithoutCommas(string paramValue, string paramName) {
            string trimmedValue = StringUtil.CheckAndTrimString(paramValue, paramName);
            if (trimmedValue.IndexOf(',') != -1) {
                throw new ArgumentException(SR.GetString(SR.PersonalizationProviderHelper_CannotHaveCommaInString, paramName, paramValue));
            }
            return trimmedValue;
        }

        internal static void CheckOnlyOnePathWithUsers(string[] paths, string[] usernames) {
            if (usernames != null && usernames.Length > 0 &&
                paths != null && paths.Length > 1) {
                throw new ArgumentException(SR.GetString(SR.PersonalizationProviderHelper_More_Than_One_Path,
                    "paths", "usernames"));
            }
        }

        internal static void CheckNegativeInteger(int paramValue, string paramName) {
            if (paramValue < 0) {
                throw new ArgumentException(
                    SR.GetString(SR.PersonalizationProviderHelper_Negative_Integer),
                    paramName);
            }
        }

        internal static void CheckNegativeReturnedInteger(int returnedValue, string methodName) {
            if (returnedValue < 0) {
                throw new HttpException(SR.GetString(SR.PersonalizationAdmin_UnexpectedPersonalizationProviderReturnValue,
                        returnedValue.ToString(CultureInfo.CurrentCulture),
                        methodName));
            }
        }

        internal static void CheckNullEntries(ICollection array, string paramName) {
            if (array == null) {
                throw new ArgumentNullException(paramName);
            }
            if (array.Count == 0) {
                throw new ArgumentException(SR.GetString(SR.PersonalizationProviderHelper_Empty_Collection, paramName));
            }
            foreach (object item in array) {
                if (item == null) {
                    throw new ArgumentException(SR.GetString(SR.PersonalizationProviderHelper_Null_Entries, paramName));
                }
            }
        }

        internal static void CheckPageIndexAndSize(int pageIndex, int pageSize) {
            if (pageIndex < 0) {
                throw new ArgumentException(SR.GetString(SR.PersonalizationProviderHelper_Invalid_Less_Than_Parameter,
                    "pageIndex", "0"));
            }
            if (pageSize < 1) {
                throw new ArgumentException(SR.GetString(SR.PersonalizationProviderHelper_Invalid_Less_Than_Parameter,
                    "pageSize", "1"));
            }

            long upperBound = (long)pageIndex * pageSize + pageSize - 1;
            if (upperBound > Int32.MaxValue) {
                throw new ArgumentException(SR.GetString(SR.PageIndex_PageSize_bad));
            }
        }

        internal static void CheckPersonalizationScope(PersonalizationScope scope) {
            if (scope < PersonalizationScope.User || scope > PersonalizationScope.Shared) {
                throw new ArgumentOutOfRangeException("scope");
            }
        }

        internal static void CheckUsernamesInSharedScope(string[] usernames) {
            if (usernames != null) {
                throw new ArgumentException(SR.GetString(SR.PersonalizationProviderHelper_No_Usernames_Set_In_Shared_Scope,
                    "usernames", "scope", PersonalizationScope.Shared.ToString()));
            }
        }
    }
}
