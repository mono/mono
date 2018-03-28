//------------------------------------------------------------------------------
// <copyright file="FontUnit.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Web.Util;

    /// <devdoc>
    ///    <para>Respresent the font unit.</para>
    /// </devdoc>
    [
        TypeConverterAttribute(typeof(FontUnitConverter))
    ]
    [Serializable]
    public struct FontUnit {


        /// <devdoc>
        /// <para>Specifies an empty <see cref='System.Web.UI.WebControls.FontUnit'/>. This field is read only. </para>
        /// </devdoc>
        public static readonly FontUnit Empty = new FontUnit();


        /// <devdoc>
        /// <para>Specifies a <see cref='System.Web.UI.WebControls.FontUnit'/> with 
        /// <see langword='FontSize.Smaller'/> font. This field is read only. </para>
        /// </devdoc>
        public static readonly FontUnit Smaller = new FontUnit(FontSize.Smaller);

        /// <devdoc>
        /// <para>Specifies a <see cref='System.Web.UI.WebControls.FontUnit'/> with <see langword='FontSize.Larger'/> 
        /// font. This field is read only.</para>
        /// </devdoc>
        public static readonly FontUnit Larger = new FontUnit(FontSize.Larger);

        /// <devdoc>
        /// <para>Specifies a <see cref='System.Web.UI.WebControls.FontUnit'/> with 
        /// <see langword='FontSize.XXSmall'/> font. This field is read only.</para>
        /// </devdoc>
        public static readonly FontUnit XXSmall = new FontUnit(FontSize.XXSmall);

        /// <devdoc>
        /// <para>Specifies a <see cref='System.Web.UI.WebControls.FontUnit'/> with <see langword='FontSize.XSmall'/> 
        /// font. This field is read only.</para>
        /// </devdoc>
        public static readonly FontUnit XSmall = new FontUnit(FontSize.XSmall);

        /// <devdoc>
        /// <para>Specifies a <see cref='System.Web.UI.WebControls.FontUnit'/> with <see langword='FontSize.Small'/> 
        /// font. This field is read only.</para>
        /// </devdoc>
        public static readonly FontUnit Small = new FontUnit(FontSize.Small);

        /// <devdoc>
        /// <para>Specifies a <see cref='System.Web.UI.WebControls.FontUnit'/> with <see langword='FontSize.Medium'/> 
        /// font. This field is read only.</para>
        /// </devdoc>
        public static readonly FontUnit Medium = new FontUnit(FontSize.Medium);

        /// <devdoc>
        /// <para>Specifies a <see cref='System.Web.UI.WebControls.FontUnit'/> with <see langword='FontSize.Large'/> 
        /// font. This field is read only.</para>
        /// </devdoc>
        public static readonly FontUnit Large = new FontUnit(FontSize.Large);

        /// <devdoc>
        /// <para>Specifies a <see cref='System.Web.UI.WebControls.FontUnit'/> with <see langword='FontSize.XLarge'/> 
        /// font. This field is read only.</para>
        /// </devdoc>
        public static readonly FontUnit XLarge = new FontUnit(FontSize.XLarge);

        /// <devdoc>
        ///    Specifies a <see cref='System.Web.UI.WebControls.FontUnit'/> with
        /// <see langword='FontSize.XXLarge'/> font. This field is read only.
        /// </devdoc>
        public static readonly FontUnit XXLarge = new FontUnit(FontSize.XXLarge);

        private readonly FontSize type;
        private readonly Unit value;


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.FontUnit'/> class with a <see cref='System.Web.UI.WebControls.FontSize'/>.</para>
        /// </devdoc>
        public FontUnit(FontSize type) {
            if (type < FontSize.NotSet || type > FontSize.XXLarge) {
                throw new ArgumentOutOfRangeException("type");
            }
            this.type = type;
            if (this.type == FontSize.AsUnit) {
                value = Unit.Point(10);
            }
            else {
                value = Unit.Empty;
            }
        }


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.FontUnit'/> class with a <see cref='System.Web.UI.WebControls.Unit'/>.</para>
        /// </devdoc>
        public FontUnit(Unit value) {
            this.type = FontSize.NotSet;
            if (value.IsEmpty == false) {
                this.type = FontSize.AsUnit;
                this.value = value;
            }
            else {
                this.value = Unit.Empty;
            }
        }


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.FontUnit'/> class with an integer value.</para>
        /// </devdoc>
        public FontUnit(int value) {
            this.type = FontSize.AsUnit;
            this.value = Unit.Point(value);
        }


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.FontUnit'/> class with a double value.</para>
        /// </devdoc>
        public FontUnit(double value) : this(new Unit(value, UnitType.Point)) {
        }


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.FontUnit'/> class with a double value.</para>
        /// </devdoc>
        public FontUnit(double value, UnitType type) : this(new Unit(value, type)) {
        }


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.FontUnit'/> class with a string.</para>
        /// </devdoc>
        public FontUnit(string value) : this(value, CultureInfo.CurrentCulture) {
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public FontUnit(string value, CultureInfo culture) {
            this.type = FontSize.NotSet;
            this.value = Unit.Empty;

            if (!String.IsNullOrEmpty(value)) {
                // This is invariant because it acts like an enum with a number together. 
                // The enum part is invariant, but the number uses current culture. 
                char firstChar = Char.ToLower(value[0], CultureInfo.InvariantCulture);
                if (firstChar == 'x') {
                    if (String.Equals(value, "xx-small", StringComparison.OrdinalIgnoreCase) || 
                        String.Equals(value, "xxsmall", StringComparison.OrdinalIgnoreCase)) {
                        this.type = FontSize.XXSmall;
                        return;
                    }
                    else if (String.Equals(value, "x-small", StringComparison.OrdinalIgnoreCase) || 
                        String.Equals(value, "xsmall", StringComparison.OrdinalIgnoreCase)) {
                        this.type = FontSize.XSmall;
                        return;
                    }
                    else if (String.Equals(value, "x-large", StringComparison.OrdinalIgnoreCase) || 
                        String.Equals(value, "xlarge", StringComparison.OrdinalIgnoreCase)) {
                        this.type = FontSize.XLarge;
                        return;
                    }
                    else if (String.Equals(value, "xx-large", StringComparison.OrdinalIgnoreCase) || 
                        String.Equals(value, "xxlarge", StringComparison.OrdinalIgnoreCase)) {
                        this.type = FontSize.XXLarge;
                        return;
                    }
                }
                else if (firstChar == 's') {
                    if (String.Equals(value, "small", StringComparison.OrdinalIgnoreCase)) {
                        this.type = FontSize.Small;
                        return;
                    }
                    else if (String.Equals(value, "smaller", StringComparison.OrdinalIgnoreCase)) {
                        this.type = FontSize.Smaller;
                        return;
                    }
                }
                else if (firstChar == 'l') {
                    if (String.Equals(value, "large", StringComparison.OrdinalIgnoreCase)) {
                        this.type = FontSize.Large;
                        return;
                    }
                    if (String.Equals(value, "larger", StringComparison.OrdinalIgnoreCase)) {
                        this.type = FontSize.Larger;
                        return;
                    }
                }
                else if ((firstChar == 'm') && String.Equals(value, "medium", StringComparison.OrdinalIgnoreCase)) {
                    this.type = FontSize.Medium;
                    return;
                }

                this.value = new Unit(value, culture, UnitType.Point);
                this.type = FontSize.AsUnit;
            }
        }
        


        /// <devdoc>
        ///    <para>Indicates whether the font size has been set.</para>
        /// </devdoc>
        public bool IsEmpty {
            get {
                return type == FontSize.NotSet;
            }
        }


        /// <devdoc>
        ///    <para>Indicates the font size by type.</para>
        /// </devdoc>
        public FontSize Type {
            get {
                return type;
            }
        }
        

        /// <devdoc>
        /// <para>Indicates the font size by <see cref='System.Web.UI.WebControls.Unit'/>.</para>
        /// </devdoc>
        public Unit Unit {
            get {
                return value;
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override int GetHashCode() {
            return HashCodeCombiner.CombineHashCodes(type.GetHashCode(), value.GetHashCode());
        }


        /// <devdoc>
        /// <para>Determines if the specified <see cref='System.Object' qualify='true'/> is equivilent to the <see cref='System.Web.UI.WebControls.FontUnit'/> represented by this instance.</para>
        /// </devdoc>
        public override bool Equals(object obj) {
            if (obj == null || !(obj is FontUnit))
                return false;

            FontUnit f = (FontUnit)obj;

            if ((f.type == type) && (f.value == value)) {
                return true;
            }
            return false;
        }


        /// <devdoc>
        /// <para>Compares two <see cref='System.Web.UI.WebControls.FontUnit'/> objects for equality.</para>
        /// </devdoc>
        public static bool operator ==(FontUnit left, FontUnit right) {
            return ((left.type == right.type) && (left.value == right.value));                
        }
        

        /// <devdoc>
        /// <para>Compares two <see cref='System.Web.UI.WebControls.FontUnit'/> objects 
        ///    for inequality.</para>
        /// </devdoc>
        public static bool operator !=(FontUnit left, FontUnit right) {
            return ((left.type != right.type) || (left.value != right.value));                
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static FontUnit Parse(string s) {
            return new FontUnit(s, CultureInfo.InvariantCulture);
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static FontUnit Parse(string s, CultureInfo culture) {
            return new FontUnit(s, culture);
        }
        

        /// <devdoc>
        /// <para>Creates a <see cref='System.Web.UI.WebControls.FontUnit'/> of type Point from an integer value.</para>
        /// </devdoc>
        public static FontUnit Point(int n) {
            return new FontUnit(n);
        }


        /// <devdoc>
        /// <para>Convert a <see cref='System.Web.UI.WebControls.FontUnit'/> to a string.</para>
        /// </devdoc>
        public override string ToString() {
            return ToString((IFormatProvider)CultureInfo.CurrentCulture);
        }


        public string ToString(CultureInfo culture) {
            return ToString((IFormatProvider)culture);
        }


        public string ToString(IFormatProvider formatProvider) {
            string s = String.Empty;

            if (IsEmpty)
                return s;

            switch (type) {
                case FontSize.AsUnit:
                    s = value.ToString(formatProvider);
                    break;
                case FontSize.XXSmall:
                    s = "XX-Small";
                    break;
                case FontSize.XSmall:
                    s = "X-Small";
                    break;
                case FontSize.XLarge:
                    s = "X-Large";
                    break;
                case FontSize.XXLarge:
                    s = "XX-Large";
                    break;
                default:
                    s = PropertyConverter.EnumToString(typeof(FontSize), type);
                    break;
            }
            return s;
        }
        

        /// <devdoc>
        /// <para>Implicitly creates a <see cref='System.Web.UI.WebControls.FontUnit'/> of type Point from an integer value.</para>
        /// </devdoc>
        public static implicit operator FontUnit(int n) {
            return FontUnit.Point(n);
        }
    }
}
