//------------------------------------------------------------------------------
// <copyright file="Sec.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Sec
 * 
 * Copyright (c) 1998-1999, Microsoft Corporation
 * 
 */

namespace System.Web.Util {
    using System.Collections.Specialized;
    using System.Web.Mail;
    using System.Configuration;
    using System.Globalization;

    internal static class ProviderUtil {
        internal const int  Infinite = Int32.MaxValue;

        internal static void GetAndRemoveStringAttribute(NameValueCollection config, string attrib, string providerName, ref string val) {
            val = config.Get(attrib);
            config.Remove(attrib);
        }
        
        internal static void GetAndRemovePositiveAttribute(NameValueCollection config, string attrib, string providerName, ref int val) {
            GetPositiveAttribute(config, attrib, providerName, ref val);
            config.Remove(attrib);
        }
            
        internal static void GetPositiveAttribute(NameValueCollection config, string attrib, string providerName, ref int val) {
            string s = config.Get(attrib);
            int t;

            if (s == null) {
                return;
            }

            try {
                t = Convert.ToInt32(s, CultureInfo.InvariantCulture);
            }
            catch (Exception e){
                if (e is ArgumentException || e is FormatException || e is OverflowException) {
                    throw new ConfigurationErrorsException(
                        SR.GetString(SR.Invalid_provider_positive_attributes, attrib, providerName));
                }
                else {
                    throw;
                }
                
            }
            
            if (t < 0) {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.Invalid_provider_positive_attributes, attrib, providerName));

            }

            val = t;
        }

        internal static void GetAndRemovePositiveOrInfiniteAttribute(NameValueCollection config, string attrib, string providerName, ref int val) {
            GetPositiveOrInfiniteAttribute(config, attrib, providerName, ref val);
            config.Remove(attrib);
        }
            
        internal static void GetPositiveOrInfiniteAttribute(NameValueCollection config, string attrib, string providerName, ref int val) {
            string s = config.Get(attrib);
            int t;

            if (s == null) {
                return;
            }

            if (s == "Infinite") {
                t = ProviderUtil.Infinite;
            }
            else {
                try {
                    t = Convert.ToInt32(s, CultureInfo.InvariantCulture);
                }
                catch (Exception e){
                    if (e is ArgumentException || e is FormatException || e is OverflowException) {
                        throw new ConfigurationErrorsException(
                            SR.GetString(SR.Invalid_provider_positive_attributes, attrib, providerName));
                    }
                    else {
                        throw;
                    }
                    
                }
                
                if (t < 0) {
                    throw new ConfigurationErrorsException(
                        SR.GetString(SR.Invalid_provider_positive_attributes, attrib, providerName));

                }
            }

            val = t;
        }

        internal static void GetAndRemoveNonZeroPositiveOrInfiniteAttribute(NameValueCollection config, string attrib, string providerName, ref int val) {
            GetNonZeroPositiveOrInfiniteAttribute(config, attrib, providerName, ref val);
            config.Remove(attrib);
        }

        internal static void GetNonZeroPositiveOrInfiniteAttribute(NameValueCollection config, string attrib, string providerName, ref int val) {
            string s = config.Get(attrib);
            int t;

            if (s == null) {
                return;
            }

            if (s == "Infinite") {
                t = ProviderUtil.Infinite;
            }
            else {
                try {
                    t = Convert.ToInt32(s, CultureInfo.InvariantCulture);
                }
                catch (Exception e){
                    if (e is ArgumentException || e is FormatException || e is OverflowException) {
                        throw new ConfigurationErrorsException(
                            SR.GetString(SR.Invalid_provider_non_zero_positive_attributes, attrib, providerName));
                    }
                    else {
                        throw;
                    }
                    
                }
                
                if (t <= 0) {
                    throw new ConfigurationErrorsException(
                        SR.GetString(SR.Invalid_provider_non_zero_positive_attributes, attrib, providerName));

                }
            }

            val = t;
        }

        internal static void GetAndRemoveBooleanAttribute(NameValueCollection config, string attrib, string providerName, ref bool val) {
            GetBooleanAttribute(config, attrib, providerName, ref val);
            config.Remove(attrib);
        }
        
        internal static void GetBooleanAttribute(NameValueCollection config, string attrib, string providerName, ref bool val) {
            string s = config.Get(attrib);
            if (s == null) {
                return;
            }
            
            if (s == "true") {
                val = true;
            }
            else if (s == "false") {
                val = false;
            }
            else {
                throw new ConfigurationErrorsException(SR.GetString(SR.Invalid_provider_attribute, attrib, providerName, s));
            }
        }

        internal static void GetAndRemoveRequiredNonEmptyStringAttribute(NameValueCollection config, string attrib, string providerName, ref string val) {
            GetRequiredNonEmptyStringAttribute(config, attrib, providerName, ref val);
            config.Remove(attrib);            
        }
        
        internal static void GetRequiredNonEmptyStringAttribute(NameValueCollection config, string attrib, string providerName, ref string val) {
            GetNonEmptyStringAttributeInternal(config, attrib, providerName, ref val, true);
        }
        
        private static void GetNonEmptyStringAttributeInternal(NameValueCollection config, string attrib, string providerName, ref string val, bool required) {
            string s = config.Get(attrib);

            // If it's (null and required) -OR- (empty string) we throw
            if ((s == null && required) || (s.Length == 0)) {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.Provider_missing_attribute, attrib, providerName));
            }

            val = s;
        }

        internal static void CheckUnrecognizedAttributes(NameValueCollection config, string providerName) {
            if (config.Count > 0)
            {
                string attribUnrecognized = config.GetKey(0);
                if (!String.IsNullOrEmpty(attribUnrecognized))
                    throw new ConfigurationErrorsException(
                                    SR.GetString(SR.Unexpected_provider_attribute, attribUnrecognized, providerName));
            }
        }
    }

}
