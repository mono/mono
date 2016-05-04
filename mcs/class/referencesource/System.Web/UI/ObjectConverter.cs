//------------------------------------------------------------------------------
// <copyright file="ObjectConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * An object that can convert from one object type to another, potentially using a format
 * string if the conversion is to a string.
 *
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web.UI {
    using System.Runtime.Serialization.Formatters;
    using System.ComponentModel;
    using System.Globalization;

    using System;
    using System.Security.Permissions;

// 


    /// <internalonly/>
    /// <devdoc>
    /// </devdoc>
    [Obsolete("The recommended alternative is System.Convert and String.Format. http://go.microsoft.com/fwlink/?linkid=14202")]
    public sealed class ObjectConverter {
        internal static readonly char [] formatSeparator = new char[] { ','};

        public ObjectConverter() { }
		

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        public static object ConvertValue(object value, Type toType, string formatString) {
            // Workaround for now to handle inability of reflection to deal with Null.
            if (value == null || Convert.IsDBNull(value)) {
                return value;
            }

            Type fromType = value.GetType();

            if (toType.IsAssignableFrom(fromType)) {
                return value;
            }

            // for now, just hit the sweet spots.
            if (typeof(string).IsAssignableFrom(fromType)) {
                if (typeof(int).IsAssignableFrom(toType)) {
                    return Convert.ToInt32((string)value, CultureInfo.InvariantCulture);
                }
                else if (typeof(bool).IsAssignableFrom(toType)) {
                    return Convert.ToBoolean((string)value, CultureInfo.InvariantCulture);
                }
                else if (typeof(DateTime).IsAssignableFrom(toType)) {
					return Convert.ToDateTime((string)value, CultureInfo.InvariantCulture);
                }
                else if (typeof(Decimal).IsAssignableFrom(toType)) {
                    TypeConverter tc = new DecimalConverter();                
                    return tc.ConvertFromInvariantString((string)value);
                }
                else if (typeof(Double).IsAssignableFrom(toType)) {
					return Convert.ToDouble((string)value, CultureInfo.InvariantCulture);
                }
                else if (typeof(Int16).IsAssignableFrom(toType)) {
					return Convert.ToInt16((Int16)value, CultureInfo.InvariantCulture);
                }
                else {
                    throw new ArgumentException(
                                               SR.GetString(SR.Cannot_convert_from_to, fromType.ToString(), toType.ToString()));
                }
            }
            else if (typeof(string).IsAssignableFrom(toType)) {
                if (typeof(int).IsAssignableFrom(fromType)) {
                    return ((int)value).ToString(formatString, CultureInfo.InvariantCulture);
                }
                else if (typeof(bool).IsAssignableFrom(fromType)) {
                    string [] tokens=null;

                    if (formatString != null) {
                        tokens = formatString.Split(formatSeparator);
                        if (tokens.Length != 2) {
                            tokens = null;
                        }
                    }

                    if ((bool)value) {
                        if (tokens != null) {
                            return tokens[0];
                        }
                        else {
                            return "true";
                        }
                    }
                    else {
                        if (tokens != null) {
                            return tokens[1];
                        }
                        else {
                            return "false";
                        }
                    }
                }
                else if (typeof(DateTime).IsAssignableFrom(fromType)) {
                    return((DateTime)value).ToString(formatString, CultureInfo.InvariantCulture);
                }
                else if (typeof(Decimal).IsAssignableFrom(fromType)) {
                    return ((Decimal)value).ToString(formatString, CultureInfo.InvariantCulture);
                }
                else if (typeof(Double).IsAssignableFrom(fromType)) {
                    return ((Double)value).ToString(formatString, CultureInfo.InvariantCulture);
                }
                else if (typeof(Single).IsAssignableFrom(fromType)) {
                    return ((Single)value).ToString(formatString, CultureInfo.InvariantCulture);
                }
                else if (typeof(Int16).IsAssignableFrom(fromType)) {
                    return ((Int16)value).ToString(formatString, CultureInfo.InvariantCulture);
                }
                else {
                    throw new ArgumentException(
                                               SR.GetString(SR.Cannot_convert_from_to, fromType.ToString(), toType.ToString()));
                }
            }
            else {
                throw new ArgumentException(
                                           SR.GetString(SR.Cannot_convert_from_to, fromType.ToString(), toType.ToString()));
            }
        }
/*

    string t1;
    int t2;
    bool t3;
    DateTime t4;
    bool t5;
    Currency t6;
    string t7, t8, t9, t10, t11;
    string t12;

    void Test()
    {


        t1 = (string)ObjectConverter.ConvertValue("Should be a string", typeof(string), null);

        t2 = (int)ObjectConverter.ConvertValue("32", typeof(int), null);

        t3 = (bool)ObjectConverter.ConvertValue("true", typeof(bool), null);

        t4 = (DateTime)ObjectConverter.ConvertValue("11/14/62", typeof(DateTime), null);

        t5 = (bool)ObjectConverter.ConvertValue("false",  typeof(bool), null);

        t6 = (Currency)ObjectConverter.ConvertValue("$12.50", typeof(Currency),  null);

        t7 = (string)ObjectConverter.ConvertValue(32,  typeof(string), "#");

        t8 = (string)ObjectConverter.ConvertValue(true,  typeof(string), "yes;no");

        t9 = (string)ObjectConverter.ConvertValue(false,  typeof(string), "yes;no");

        t10 = (string)ObjectConverter.ConvertValue(DateTime.Now, typeof(string), "hh:mm");

        t11 = (string)ObjectConverter.ConvertValue(new Currency(12), typeof(string), "C");
    }

    public static void Main(string[] args)
    {
        new ObjectConverter().Test();
    }
    */

    }
}



