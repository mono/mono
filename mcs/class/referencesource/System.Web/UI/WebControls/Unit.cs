//------------------------------------------------------------------------------
// <copyright file="Unit.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Globalization;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Security.Permissions;
    using System.Web.Util;

    [
        TypeConverterAttribute(typeof(UnitConverter))
    ]
    [Serializable]
    public struct Unit {


        /// <devdoc>
        ///   Specifies an empty unit.
        /// </devdoc>
        public static readonly Unit Empty = new Unit();

        internal const int MaxValue = 32767;
        internal const int MinValue = -32768;

        private readonly UnitType type;
        private readonly double value;


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.Unit'/> structure with the specified 32-bit signed integer as 
        ///    the unit value and <see langword='Pixel'/> as the (default) unit type.</para>
        /// </devdoc>
        public Unit(int value) {
            if ((value < MinValue) || (value > MaxValue)) {
                throw new ArgumentOutOfRangeException("value");
            }

            this.value = value;
            this.type = UnitType.Pixel;
        }


        /// <devdoc>
        /// <para> Initializes a new instance of the <see cref='System.Web.UI.WebControls.Unit'/> structure with the 
        ///    specified double-precision
        ///    floating point number as the unit value and <see langword='Pixel'/>
        ///    as the (default) unit type.</para>
        /// </devdoc>
        public Unit(double value) {
            if ((value < MinValue) || (value > MaxValue)) {
                throw new ArgumentOutOfRangeException("value");
            }
            this.value = (int)value;
            this.type = UnitType.Pixel;
        }


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.Unit'/> structure with the specified 
        ///    double-precision floating point number as the unit value and the specified
        /// <see cref='System.Web.UI.WebControls.UnitType'/> as the unit type.</para>
        /// </devdoc>
        public Unit(double value, UnitType type) {
            if ((value < MinValue) || (value > MaxValue)) {
                throw new ArgumentOutOfRangeException("value");
            }
            if (type == UnitType.Pixel) {
                this.value = (int)value;
            }
            else {
                this.value = value;
            }
            this.type = type;
        }


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.Unit'/> structure with the specified text 
        ///    string that contains the unit value and unit type. If the unit type is not
        ///    specified, the default is <see langword='Pixel'/>
        ///    . </para>
        /// </devdoc>
        public Unit(string value) : this(value, CultureInfo.CurrentCulture, UnitType.Pixel) {
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Unit(string value, CultureInfo culture) : this(value, culture, UnitType.Pixel) {
        }

        internal Unit(string value, CultureInfo culture, UnitType defaultType) {
            if (String.IsNullOrEmpty(value)) {
                this.value = 0;
                this.type = (UnitType)0;
            }
            else {
                if (culture == null) {
                    culture = CultureInfo.CurrentCulture;
                }
                
                // This is invariant because it acts like an enum with a number together. 
                // The enum part is invariant, but the number uses current culture. 
                string trimLcase = value.Trim().ToLower(CultureInfo.InvariantCulture);
                int len = trimLcase.Length;

                int lastDigit = -1;
                for (int i = 0; i < len; i++) {
                    char ch = trimLcase[i];
                    if (((ch < '0') || (ch > '9')) && (ch != '-') && (ch != '.') && (ch != ','))
                        break;
                    lastDigit = i;
                }
                if (lastDigit == -1) {
                    throw new FormatException(SR.GetString(SR.UnitParseNoDigits, value));
                }
                if (lastDigit < len - 1) {
                    type = (UnitType)GetTypeFromString(trimLcase.Substring(lastDigit+1).Trim());
                }
                else {
                    type = defaultType;
                }

                string numericPart = trimLcase.Substring(0, lastDigit+1);
                // Cannot use Double.FromString, because we don't use it in the ToString implementation
                try {
                    TypeConverter converter = new SingleConverter();
                    this.value = (Single)converter.ConvertFromString(null, culture, numericPart);
                    
                    if (type == UnitType.Pixel) {
                        this.value = (int)this.value;
                    }
                }
                catch {
                    throw new FormatException(SR.GetString(SR.UnitParseNumericPart, value, numericPart, type.ToString("G")));
                }
                if ((this.value < MinValue) || (this.value > MaxValue)) {
                    throw new ArgumentOutOfRangeException("value");
                }
            }
        }


        /// <devdoc>
        /// <para>Gets a value indicating whether the <see cref='System.Web.UI.WebControls.Unit'/> is empty.</para>
        /// </devdoc>
        public bool IsEmpty {
            get {
                return type == (UnitType)0;
            }
        }


        /// <devdoc>
        /// <para>Gets or sets the type of the <see cref='System.Web.UI.WebControls.Unit'/> .</para>
        /// </devdoc>
        public UnitType Type {
            get {
                if (!IsEmpty) {
                    return this.type;
                }
                else {
                    return UnitType.Pixel;
                }
            }
        }


        /// <devdoc>
        /// <para>Gets the value of the <see cref='System.Web.UI.WebControls.Unit'/> .</para>
        /// </devdoc>
        public double Value {
            get {
                return this.value;
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override int GetHashCode() {
            return HashCodeCombiner.CombineHashCodes(type.GetHashCode(), value.GetHashCode());
        }


        /// <devdoc>
        /// <para>Compares this <see cref='System.Web.UI.WebControls.Unit'/> with the specified object.</para>
        /// </devdoc>
        public override bool Equals(object obj) {
            if (obj == null || !(obj is Unit)) {
                return false;
            }
            Unit u = (Unit)obj;

            // compare internal values to avoid "defaulting" in the case of "Empty"
            //
            if (u.type == type && u.value == value) {
                return true;
            }

            return false;
        }


        /// <devdoc>
        ///    <para>Compares two units to find out if they have the same value and type.</para>
        /// </devdoc>
        public static bool operator ==(Unit left, Unit right) {

            // compare internal values to avoid "defaulting" in the case of "Empty"
            //
            return (left.type == right.type && left.value == right.value);            
        }


        /// <devdoc>
        ///    <para>Compares two units to find out if they have different
        ///       values and/or types.</para>
        /// </devdoc>
        public static bool operator !=(Unit left, Unit right) {

            // compare internal values to avoid "defaulting" in the case of "Empty"
            //
            return (left.type != right.type || left.value != right.value);            
        }



        /// <devdoc>
        ///  Converts UnitType to persistence string.
        /// </devdoc>
        private static string GetStringFromType(UnitType type) {
            switch (type) {
                case UnitType.Pixel:
                    return "px";
                case UnitType.Point:
                    return "pt";
                case UnitType.Pica:
                    return "pc";
                case UnitType.Inch:
                    return "in";
                case UnitType.Mm:
                    return "mm";
                case UnitType.Cm:
                    return "cm";
                case UnitType.Percentage:
                    return "%";
                case UnitType.Em:
                    return "em";
                case UnitType.Ex:
                    return "ex";
            }
            return String.Empty;
        }


        /// <devdoc>
        ///  Converts persistence string to UnitType.
        /// </devdoc>
        private static UnitType GetTypeFromString(string value) {
            if (!String.IsNullOrEmpty(value)) {
                if (value.Equals("px")) {
                    return UnitType.Pixel;
                }
                else if (value.Equals("pt")) {
                    return UnitType.Point;
                }
                else if (value.Equals("%")) {
                    return UnitType.Percentage;
                }
                else if (value.Equals("pc")) {
                    return UnitType.Pica;
                }
                else if (value.Equals("in")) {
                    return UnitType.Inch;
                }
                else if (value.Equals("mm")) {
                    return UnitType.Mm;
                }
                else if (value.Equals("cm")) {
                    return UnitType.Cm;
                }
                else if (value.Equals("em")) {
                    return UnitType.Em;
                }
                else if (value.Equals("ex")) {
                    return UnitType.Ex;
                }
                else {
                    throw new ArgumentOutOfRangeException("value");
                }
            }
            return UnitType.Pixel;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static Unit Parse(string s) {
            return new Unit(s, CultureInfo.CurrentCulture);
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static Unit Parse(string s, CultureInfo culture) {
            return new Unit(s, culture);
        }


        /// <devdoc>
        /// <para>Creates a <see cref='System.Web.UI.WebControls.Unit'/> of type <see langword='Percentage'/> from the specified 32-bit signed integer.</para>
        /// </devdoc>
        public static Unit Percentage(double n) {
            return new Unit(n,UnitType.Percentage);
        }


        /// <devdoc>
        /// <para>Creates a <see cref='System.Web.UI.WebControls.Unit'/> of type <see langword='Pixel'/> from the specified 32-bit signed integer.</para>
        /// </devdoc>
        public static Unit Pixel(int n) {
            return new Unit(n);
        }


        /// <devdoc>
        /// <para>Creates a <see cref='System.Web.UI.WebControls.Unit'/> of type <see langword='Point'/> from the 
        ///    specified 32-bit signed integer.</para>
        /// </devdoc>
        public static Unit Point(int n) {
            return new Unit(n,UnitType.Point);
        }


        /// <internalonly/>
        /// <devdoc>
        /// <para>Converts a <see cref='System.Web.UI.WebControls.Unit'/> to a <see cref='System.String' qualify='true'/> .</para>
        /// </devdoc>
        public override string ToString() {
            return ToString((IFormatProvider)CultureInfo.CurrentCulture);
        }


        public string ToString(CultureInfo culture) {
            return ToString((IFormatProvider)culture);
        }


        public string ToString(IFormatProvider formatProvider) {
            if (IsEmpty)
                return String.Empty;

            // Double.ToString does not do the right thing, we get extra bits at the end
            string valuePart;
            if (type == UnitType.Pixel) {
                valuePart = ((int)value).ToString(formatProvider);
            }
            else {
                valuePart = ((float)value).ToString(formatProvider);
            }
            
            return valuePart + Unit.GetStringFromType(type);
        }


        /// <devdoc>
        /// <para>Implicitly creates a <see cref='System.Web.UI.WebControls.Unit'/> of type <see langword='Pixel'/> from the specified 32-bit unsigned integer.</para>
        /// </devdoc>
        public static implicit operator Unit(int n) {
            return Unit.Pixel(n);
        }
    }
}
