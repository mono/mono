//------------------------------------------------------------------------------
// <copyright file="VerticalAlignConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

// 








namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;

    internal class VerticalAlignConverter : EnumConverter {

        static string[] stringValues = new String[(int) VerticalAlign.Bottom + 1];

        static VerticalAlignConverter () { 
            stringValues[(int) VerticalAlign.NotSet] = "NotSet";
            stringValues[(int) VerticalAlign.Top] = "Top";
            stringValues[(int) VerticalAlign.Middle] = "Middle";
            stringValues[(int) VerticalAlign.Bottom] = "Bottom";
        }

        // this constructor needs to be public despite the fact that it's in an internal
        // class so it can be created by Activator.CreateInstance.
        public VerticalAlignConverter () : base(typeof(VerticalAlign)) {}

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            if (sourceType == typeof(string)) {
                return true;
            }
            else {
                return base.CanConvertFrom(context, sourceType);
            }
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value == null)
                return null;

            if (value is string) {
                string textValue = ((string)value).Trim();
                if (textValue.Length == 0)
                    return VerticalAlign.NotSet;

                switch (textValue) {
                    case "NotSet":
                        return VerticalAlign.NotSet;
                    case "Top":
                        return VerticalAlign.Top;
                    case "Middle":
                        return VerticalAlign.Middle;
                    case "Bottom":
                        return VerticalAlign.Bottom;
                }
            }
            
            return base.ConvertFrom(context, culture, value);
        }
        
        public override bool CanConvertTo(ITypeDescriptorContext context, Type sourceType) {
            if (sourceType == typeof(string)) {
                return true;
            }

            return base.CanConvertTo(context, sourceType);
        }
        
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if (destinationType == typeof(string) && ((int) value <= (int)VerticalAlign.Bottom)) {
                return stringValues[(int) value];
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}



