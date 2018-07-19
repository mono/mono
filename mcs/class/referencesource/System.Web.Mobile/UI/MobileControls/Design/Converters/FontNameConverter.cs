//------------------------------------------------------------------------------
// <copyright file="FontNameConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls.Converters
{
    using Microsoft.Win32;

    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Diagnostics;
    using System.Globalization;

    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal sealed class FontNameConverter : TypeConverter 
    {
        private StandardValuesCollection values;

        /// <devdoc>
        ///      Creates a new font name converter.
        /// </devdoc>
        public FontNameConverter() 
        {
            // Sink an event to let us know when the installed
            // set of fonts changes.
            //
            SystemEvents.InstalledFontsChanged += new EventHandler(this.OnInstalledFontsChanged);
        }

        /// <devdoc>
        ///      Determines if this converter can convert an object in the given source
        ///      type to the native type of the converter.
        /// </devdoc>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) 
        {
            if (sourceType == typeof(string)) 
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        /// <devdoc>
        ///      Converts the given object to the converter's native type.
        /// </devdoc>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) 
        {
            if (value is string) 
            {
                return MatchFontName((string)value, context);
            }
            return base.ConvertFrom(context, culture, value);
        }

        /// <devdoc>
        ///      We need to know when we're finalized.
        /// </devdoc>
        ~FontNameConverter() 
        {
            SystemEvents.InstalledFontsChanged -= new EventHandler(this.OnInstalledFontsChanged);
        }

        /// <devdoc>
        ///      Retrieves a collection containing a set of standard values
        ///      for the data type this validator is designed for.  This
        ///      will return null if the data type does not support a
        ///      standard set of values.
        /// </devdoc>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) 
        {
            if (values == null) 
            {
                FontFamily[] fonts =  FontFamily.Families;

                Hashtable hash = new Hashtable();
                for (int i = 0; i < fonts.Length; i++) 
                {
                    string name = fonts[i].Name;
                    hash[name.ToLower(CultureInfo.InvariantCulture)] = name;
                }

                object[] array = new object[hash.Values.Count];
                hash.Values.CopyTo(array, 0);
                Array.Sort(array);
                values = new StandardValuesCollection(array);
            }

            return values;
        }

        /// <devdoc>
        ///      Determines if the list of standard values returned from
        ///      GetStandardValues is an exclusive list.  If the list
        ///      is exclusive, then no other values are valid, such as
        ///      in an enum data type.  If the list is not exclusive,
        ///      then there are other valid values besides the list of
        ///      standard values GetStandardValues provides.
        /// </devdoc>
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) 
        {
            return false;
        }

        /// <devdoc>
        ///      Determines if this object supports a standard set of values
        ///      that can be picked from a list.
        /// </devdoc>
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) 
        {
            return true;
        }

        private string MatchFontName(string name, ITypeDescriptorContext context) 
        {
            Debug.Assert(name != null, "Expected an actual font name to match in FontNameConverter::MatchFontName.");

            // AUI 2300
            if (name.Trim().Length == 0)
            {
                return String.Empty;
            }
                
            // Try a partial match
            //
            string bestMatch = null;
            IEnumerator e = GetStandardValues(context).GetEnumerator();
            while (e.MoveNext())
            {
                string fontName = e.Current.ToString();
                if (String.Equals(fontName, name, StringComparison.OrdinalIgnoreCase)) {
                    // For an exact match, return immediately
                    //
                    return fontName;
                }
                else if (fontName.StartsWith(name, StringComparison.OrdinalIgnoreCase)) {
                    if (bestMatch == null || fontName.Length <= bestMatch.Length) 
                    {
                        bestMatch = fontName;
                    }
                }
            }
                
            if (bestMatch == null) 
            {
                // no match... fall back on whatever was provided
                bestMatch = name;
            }
            return bestMatch;
        }

        /// <devdoc>
        ///      Called by system events when someone adds or removes a font.  Here
        ///      we invalidate our font name collection.
        /// </devdoc>
        private void OnInstalledFontsChanged(object sender, EventArgs e) 
        {
            values = null;
        }
    }
}
