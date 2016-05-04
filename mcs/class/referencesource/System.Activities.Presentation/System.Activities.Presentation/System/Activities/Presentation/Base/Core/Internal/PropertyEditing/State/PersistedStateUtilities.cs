//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.State 
{

    // <summary>
    // A set of utility methods we use to persist state
    // </summary>
    internal static class PersistedStateUtilities 
    {

        // <summary>
        // Escapes '&,;.' characters
        // </summary>
        // <param name="s">String to escape</param>
        // <returns>Escaped string</returns>
        public static string Escape(string s) 
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            s = s.Replace("&", "&&");
            s = s.Replace(",", "&comma&");
            s = s.Replace(";", "&semicolon&");
            s = s.Replace(".", "&dot&");
            return s;
        }

        // <summary>
        // Unescapes '&;,.' characters
        // </summary>
        // <param name="s">Escaped string</param>
        // <returns>Unescaped string</returns>
        public static string Unescape(string s) 
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            s = s.Replace("&comma&", ",");
            s = s.Replace("&semicolon&", ";");
            s = s.Replace("&dot&", ".");
            s = s.Replace("&&", "&");
            return s;
        }

        // <summary>
        // Converts 0 -> false, 1 -> true, anything else -> null
        // </summary>
        // <param name="digit">digit to convert</param>
        // <returns>True/False/Null</returns>
        public static bool? DigitToBool(string digit) 
        {
            return "0".Equals(digit) ? false : ("1".Equals(digit) ? true : (bool?)null);
        }

        // <summary>
        // Converts true -> 1, false -> 0
        // </summary>
        // <param name="value">Value to convert</param>
        // <returns>Bool as a digit string</returns>
        public static string BoolToDigit(bool value) 
        {
            return value ? "1" : "0";
        }
    }
}
