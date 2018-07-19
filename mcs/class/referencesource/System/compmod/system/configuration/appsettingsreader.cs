//------------------------------------------------------------------------------
// <copyright file="AppSettingsReader.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Reflection;
using System.Configuration;
using System.Collections.Specialized;
using System.Globalization;

namespace System.Configuration {

    /// <devdoc>
    ///     The AppSettingsReader class provides a wrapper for System.Configuration.ConfigurationManager.AppSettings
    ///     which provides a single method for reading values from the config file of a particular type.
    /// </devdoc>
    public class AppSettingsReader {
        private NameValueCollection map;
        static Type stringType = typeof(string);
        static Type[] paramsArray = new Type[] { stringType };
        static string NullString = "None";

        /// <devdoc>
        ///     Constructor
        /// </devdoc>
        public AppSettingsReader() {
            map = System.Configuration.ConfigurationManager.AppSettings;
        }

        /// <devdoc>
        ///     Gets the value for specified key from ConfigurationManager.AppSettings, and returns
        ///     an object of the specified type containing the value from the config file.  If the key
        ///     isn't in the config file, or if it is not a valid value for the given type, it will 
        ///     throw an exception with a descriptive message so the user can make the appropriate
        ///     change
        /// </devdoc>
        public object GetValue(string key, Type type) {
            if (key == null) throw new ArgumentNullException("key");
            if (type == null) throw new ArgumentNullException("type");

            string val = map[key];

            if (val == null) throw new InvalidOperationException(SR.GetString(SR.AppSettingsReaderNoKey, key));

            if (type == stringType) {
                // It's a string, so we can ALMOST just return the value.  The only
                // tricky point is that if it's the string "(None)", then we want to
                // return null.  And of course we need a way to represent the string
                // (None), so we use ((None)), and so on... so it's a little complicated.
                int NoneNesting = GetNoneNesting(val);
                if (NoneNesting == 0) {
                    // val is not of the form ((..((None))..))
                    return val;
                }
                else if (NoneNesting == 1) {
                    // val is (None)
                    return null;
                }
                else {
                    // val is of the form ((..((None))..))
                    return val.Substring(1, val.Length - 2);
                }
            }
            else {
                try {
                    return Convert.ChangeType(val, type, CultureInfo.InvariantCulture);
                } catch (Exception) {
                    string displayString = (val.Length == 0) ? SR.AppSettingsReaderEmptyString : val;
                    throw new InvalidOperationException(SR.GetString(SR.AppSettingsReaderCantParse, displayString, key, type.ToString()));
                }               
            }  
        }

        private int GetNoneNesting(string val) {
            int count = 0;
            int len = val.Length;
            if (len > 1) {
                while (val[count] == '(' && val[len - count - 1] == ')') {
                    count++;
                }
                if (count > 0 && string.Compare(NullString, 0, val, count, len - 2 * count, StringComparison.Ordinal) != 0) {
                    // the stuff between the parens is not "None"
                    count = 0;
                }
            }
            return count;
        }
    }
}
