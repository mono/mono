//------------------------------------------------------------------------------
// <copyright file="XhtmlStyleClass.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Web;
using System.Web.Caching;
using System.Web.Mobile;
using System.Web.UI.MobileControls;
using System.Web.UI.MobileControls.Adapters;
using System.IO;
using System.Text;
using System.Drawing;
using System.Collections;
using System.Diagnostics;
using System.Globalization;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource.XhtmlAdapters
#else
namespace System.Web.UI.MobileControls.Adapters.XhtmlAdapters
#endif
{
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class XhtmlStyleClass {
        // The filter determines which style attributes are written.  Any style attributes
        // that can be ignored due to style inheritance have their bits set to zero.
        private StyleFilter _filter;
        private Style _controlStyle;

        // Partial copy constructor
        public XhtmlStyleClass (Style source, StyleFilter filter)  {
            _controlStyle = (Style) source.Clone();
            // Filter has bits set to zero for properties which don't need
            // to be written due to style inheritance from parent.
            _filter = filter;
        }

#if UNUSED_CODE
        public XhtmlStyleClass (Style source, string className, StyleFilter filter) : this (source, filter){
            StyleClassName = className;
        }
#endif

        // Review: Internal field.
        internal String StyleClassName;

        public StyleFilter Filter {
            get {
                return _filter;
            }
        }

        protected virtual void SetFilter(StyleFilter arg) {
            _filter = arg;
        }
            
        public override String ToString () {
            StringBuilder builder = new StringBuilder ();
            builder.Append ("." + StyleClassName + "{\r\n");
            builder.Append (GetClassDefinition ());
            builder.Append ("}\r\n");
            return builder.ToString ();
        } 

        public virtual String GetClassDefinition () {
            StringBuilder classBuilder = new StringBuilder ();
            BuildStyleClassFormatProperties (classBuilder);
            BuildStyleClassLayoutProperties (classBuilder);
            return classBuilder.ToString ();
        }

        internal virtual Style Style{
            get {
                return _controlStyle;
            }
        }

        protected virtual void BuildStyleClassFormatProperties (StringBuilder classBuilder){
            BuildStyleClassBold (classBuilder);
            BuildStyleClassItalic (classBuilder);
            BuildStyleClassFontName (classBuilder);
            BuildStyleClassFontSize (classBuilder);
            BuildStyleClassBackgroundColor (classBuilder);
            BuildStyleClassForegroundColor (classBuilder);         
        }

        protected virtual void BuildStyleClassLayoutProperties (StringBuilder classBuilder) {
            BuildStyleClassNoWrap(classBuilder);
            BuildStyleClassAlignment(classBuilder);
        }

        private void BuildStyleClassNoWrap (StringBuilder classBuilder) {
            if ((_filter & StyleFilter.Wrapping) == 0){
                return;
            }
            Wrapping styleWrapping = (Wrapping) _controlStyle [Style.WrappingKey, true /* inherit */];
            if (styleWrapping == Wrapping.NoWrap) {
                classBuilder.Append ("white-space: nowrap;\r\n");
            }
        }


        private void BuildStyleClassAlignment (StringBuilder classBuilder) {
            if ((_filter & StyleFilter.Alignment) == 0){
                return;
            }
            Alignment styleAlignment = (Alignment) _controlStyle [Style.AlignmentKey, true /* inherit */];
            if (styleAlignment != Alignment.NotSet) {
                classBuilder.Append ("text-align: " + styleAlignment.ToString ().ToLower(CultureInfo.InvariantCulture) + ";\r\n");
            }
        }

        private void BuildStyleClassBackgroundColor (StringBuilder classBuilder) {
            if ((_filter & StyleFilter.BackgroundColor) == 0){
                return;
            }
            Color styleColor = (Color) _controlStyle [Style.BackColorKey, true /* inherit */];
            if (styleColor != Color.Empty) {
                classBuilder.Append ("background-color: " + ColorTranslator.ToHtml (styleColor).ToLower(CultureInfo.InvariantCulture) + ";\r\n");
            }
        }

        private void BuildStyleClassForegroundColor (StringBuilder classBuilder) {
            if ((_filter & StyleFilter.ForegroundColor) == 0){
                return;
            }
            Color styleColor = (Color) _controlStyle [Style.ForeColorKey, true /* inherit */];
            if (styleColor != Color.Empty) {
                classBuilder.Append ("color: " + ColorTranslator.ToHtml (styleColor).ToLower(CultureInfo.InvariantCulture) + ";\r\n");
            }
        }

        private void BuildStyleClassItalic (StringBuilder classBuilder) {
            if ((_filter & StyleFilter.Italic) == 0){
                return;
            }            
            if ((BooleanOption) _controlStyle [Style.ItalicKey, true /* inherit */] == BooleanOption.True) {
                classBuilder.Append ("font-style: italic;\r\n");
            }
            else if ((BooleanOption) _controlStyle [Style.ItalicKey, true /* inherit */] == BooleanOption.False) {
                classBuilder.Append ("font-style: normal;\r\n");
            }
        }

        private void BuildStyleClassBold (StringBuilder classBuilder) {
            if ((_filter & StyleFilter.Bold) == 0){
                return;
            }
            if ((BooleanOption) _controlStyle[Style.BoldKey, true /* inherit */] == BooleanOption.True) {
                classBuilder.Append ("font-weight: bold;\r\n");
            }
            else if ((BooleanOption) _controlStyle[Style.BoldKey, true /* inherit */] == BooleanOption.False) {
                classBuilder.Append ("font-weight: normal;\r\n");            
            }
        }

        private void BuildStyleClassFontName (StringBuilder classBuilder) {
            if ((_filter & StyleFilter.FontName) == 0){
                return;
            }
            if ((String) _controlStyle[Style.FontNameKey, true /* inherit */] != (String) XhtmlConstants.DefaultStyle [Style.FontNameKey, false /* do not inherit */]) {
                classBuilder.Append ("font-family: " + _controlStyle[Style.FontNameKey, true /* inherit */].ToString () + ";\r\n");
            }
        }

        private void BuildStyleClassFontSize (StringBuilder classBuilder) {
            if ((_filter & StyleFilter.FontSize) == 0){
                return;
            }
            // Review: Consider replacing switch.
            switch ((FontSize)_controlStyle[Style.FontSizeKey, true /* inherit */]) {
                case FontSize.Large:
                    classBuilder.Append ("font-size: large;\r\n");
                    break;
                case FontSize.Small:
                    classBuilder.Append ("font-size: small;\r\n");
                    break;
                case FontSize.Normal:
                    classBuilder.Append ("font-size: medium;\r\n");
                    break;
                default:
                    break;
            }
        }
    
        // Given a Style, returns a filter with nonzero bits only at properties which have
        // to be written.
        public virtual StyleFilter GetFilter (Style style) {
            // Filter out any child elt style properties which do not have to be written due to style inheritance.
            StyleFilter inheritanceFilter = GetFilterInternal(style);
            
            // Filter out any child elt style properties which are unnecessary because they have default values.
            // Default values never cause a CSS style class property to be written.
            StyleFilter defaultFilter = XhtmlConstants.DefaultStyleClass.GetFilterInternal(style);
            
            return inheritanceFilter & defaultFilter;
        }
        
        // The XhtmlStyleClass for a child control can filter out unnecessary properties
        // due to style inheritance.  This returns an optimized StyleFilter for child 
        // controls with a given style.
        protected virtual StyleFilter GetFilterInternal (Style style) {
            StyleFilter returnValue = StyleFilter.None;

            if ((BooleanOption)_controlStyle [Style.BoldKey, true] != (BooleanOption)style [Style.BoldKey, true]) {
                returnValue |= StyleFilter.Bold;
            }
            if ((BooleanOption)_controlStyle [Style.ItalicKey, true] != (BooleanOption)style [Style.ItalicKey, true]) {
                returnValue |= StyleFilter.Italic;
            }
            if ((String)_controlStyle [Style.FontNameKey, true] != (String)style [Style.FontNameKey, true]) {
                returnValue |= StyleFilter.FontName;
            }
            if ((FontSize)_controlStyle [Style.FontSizeKey, true] != (FontSize)style [Style.FontSizeKey, true]) {
                returnValue |= StyleFilter.FontSize;
            }
            if ((Color)_controlStyle [Style.BackColorKey, true] != (Color)style [Style.BackColorKey, true]) {// value comparison
                returnValue |= StyleFilter.BackgroundColor;
            }
            if ((Color)_controlStyle [Style.ForeColorKey, true] != (Color)style [Style.ForeColorKey, true]) {// value comparison
                returnValue |= StyleFilter.ForegroundColor;
            }
            
            if ((Alignment)_controlStyle [Style.AlignmentKey, true] != (Alignment)style [Style.AlignmentKey, true]) {
                returnValue |= StyleFilter.Alignment;
            }
            if ((Wrapping)_controlStyle [Style.WrappingKey, true] != (Wrapping)style [Style.WrappingKey, true]) {
                returnValue |= StyleFilter.Wrapping;
            }
            return returnValue;
        }
    }

    // XhtmlStyleClass which only writes format properties.
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class XhtmlFormatStyleClass : XhtmlStyleClass {
#if UNUSED_CODE
        public XhtmlFormatStyleClass (Style source, string className, StyleFilter filter) : base (source, className, filter){}
#endif
        public XhtmlFormatStyleClass (Style source, StyleFilter filter) : base (source, filter){
            SetFilter (Filter & XhtmlConstants.Format);
        }
        public override String GetClassDefinition () {
            StringBuilder classBuilder = new StringBuilder ();
            BuildStyleClassFormatProperties (classBuilder);
            return classBuilder.ToString ();
        }
        public override StyleFilter GetFilter (Style style) {
            StyleFilter returnValue = GetFilterInternal(style); 
            // For a child control's style, there are no layout properties
            // we can ignore due to style inheritance.
            returnValue |= XhtmlConstants.Layout;
            // However, we can still ignore properties which are the same as the default.
            returnValue &= XhtmlConstants.DefaultStyleClass.GetFilter(style);
            return returnValue;
        }
    }

    // XhtmlStyleClass which only writes layout properties.
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class XhtmlLayoutStyleClass : XhtmlStyleClass {
#if UNUSED_CODE
        public XhtmlLayoutStyleClass (Style source, string className, StyleFilter filter) : base (source, className, filter){}
#endif
        public XhtmlLayoutStyleClass (Style source, StyleFilter filter) : base (source, filter){
            SetFilter (Filter & XhtmlConstants.Layout);
        }
        public override String GetClassDefinition () {
            StringBuilder classBuilder = new StringBuilder ();
            BuildStyleClassLayoutProperties (classBuilder);
            return classBuilder.ToString ();
        }
        public override StyleFilter GetFilter (Style style) {
            StyleFilter returnValue = GetFilterInternal(style); 
            // For a child control's style, there are no format properties
            // we can ignore due to style inheritance.
            returnValue |= XhtmlConstants.Format;
            // However, we can still ignore properties which are the same as the default.
            returnValue &= XhtmlConstants.DefaultStyleClass.GetFilter(style);
            return returnValue;
        }
    }
}
