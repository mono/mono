//------------------------------------------------------------------------------
// <copyright file="CssTextWriter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Web.Util;


    /// <devdoc>
    /// Derived TextWriter that provides CSS rendering API.
    /// </devdoc>
    internal sealed class CssTextWriter : TextWriter {
        private TextWriter _writer;

        private static Hashtable attrKeyLookupTable;
        private static AttributeInformation[] attrNameLookupArray;

        static CssTextWriter() {
            // register known style attributes, HtmlTextWriterStyle.Length
            attrKeyLookupTable = new Hashtable((int)HtmlTextWriterStyle.ZIndex + 1);
            attrNameLookupArray = new AttributeInformation[(int)HtmlTextWriterStyle.ZIndex + 1];

            RegisterAttribute("background-color", HtmlTextWriterStyle.BackgroundColor);
            RegisterAttribute("background-image", HtmlTextWriterStyle.BackgroundImage, true, true);
            RegisterAttribute("border-collapse", HtmlTextWriterStyle.BorderCollapse);
            RegisterAttribute("border-color", HtmlTextWriterStyle.BorderColor);
            RegisterAttribute("border-style", HtmlTextWriterStyle.BorderStyle);
            RegisterAttribute("border-width", HtmlTextWriterStyle.BorderWidth);
            RegisterAttribute("color", HtmlTextWriterStyle.Color);
            RegisterAttribute("cursor", HtmlTextWriterStyle.Cursor);
            RegisterAttribute("direction", HtmlTextWriterStyle.Direction);
            RegisterAttribute("display", HtmlTextWriterStyle.Display);
            RegisterAttribute("filter", HtmlTextWriterStyle.Filter);
            RegisterAttribute("font-family", HtmlTextWriterStyle.FontFamily, true);
            RegisterAttribute("font-size", HtmlTextWriterStyle.FontSize);
            RegisterAttribute("font-style", HtmlTextWriterStyle.FontStyle);
            RegisterAttribute("font-variant", HtmlTextWriterStyle.FontVariant);
            RegisterAttribute("font-weight", HtmlTextWriterStyle.FontWeight);
            RegisterAttribute("height", HtmlTextWriterStyle.Height);
            RegisterAttribute("left", HtmlTextWriterStyle.Left);
            RegisterAttribute("list-style-image", HtmlTextWriterStyle.ListStyleImage, true, true);
            RegisterAttribute("list-style-type", HtmlTextWriterStyle.ListStyleType);
            RegisterAttribute("margin", HtmlTextWriterStyle.Margin);
            RegisterAttribute("margin-bottom", HtmlTextWriterStyle.MarginBottom);
            RegisterAttribute("margin-left", HtmlTextWriterStyle.MarginLeft);
            RegisterAttribute("margin-right", HtmlTextWriterStyle.MarginRight);
            RegisterAttribute("margin-top", HtmlTextWriterStyle.MarginTop);
            RegisterAttribute("overflow-x", HtmlTextWriterStyle.OverflowX);
            RegisterAttribute("overflow-y", HtmlTextWriterStyle.OverflowY);
            RegisterAttribute("overflow", HtmlTextWriterStyle.Overflow);
            RegisterAttribute("padding", HtmlTextWriterStyle.Padding);
            RegisterAttribute("padding-bottom", HtmlTextWriterStyle.PaddingBottom);
            RegisterAttribute("padding-left", HtmlTextWriterStyle.PaddingLeft);
            RegisterAttribute("padding-right", HtmlTextWriterStyle.PaddingRight);
            RegisterAttribute("padding-top", HtmlTextWriterStyle.PaddingTop);
            RegisterAttribute("position", HtmlTextWriterStyle.Position);
            RegisterAttribute("text-align", HtmlTextWriterStyle.TextAlign);
            RegisterAttribute("text-decoration", HtmlTextWriterStyle.TextDecoration);
            RegisterAttribute("text-overflow", HtmlTextWriterStyle.TextOverflow);
            RegisterAttribute("top", HtmlTextWriterStyle.Top);
            RegisterAttribute("vertical-align", HtmlTextWriterStyle.VerticalAlign);
            RegisterAttribute("visibility", HtmlTextWriterStyle.Visibility);
            RegisterAttribute("width", HtmlTextWriterStyle.Width);
            RegisterAttribute("white-space", HtmlTextWriterStyle.WhiteSpace);
            RegisterAttribute("z-index", HtmlTextWriterStyle.ZIndex);
        }


        /// <devdoc>
        /// Initializes an instance of a CssTextWriter with its underlying TextWriter.
        /// </devdoc>
        public CssTextWriter(TextWriter writer) {
            _writer = writer;
        }


        /// <internalonly/>
        public override Encoding Encoding {
            get {
                return _writer.Encoding;
            }
        }


        /// <internalonly/>
        public override string NewLine {
            get {
                return _writer.NewLine;
            }
            set {
                _writer.NewLine = value;
            }
        }


        /// <internalonly/>
        public override void Close() {
            _writer.Close();
        }


        /// <internalonly/>
        public override void Flush() {
            _writer.Flush();
        }


        /// <devdoc>
        /// Returns the HtmlTextWriterStyle value for known style attributes.
        /// </devdoc>
        public static HtmlTextWriterStyle GetStyleKey(string styleName) {
            if (!String.IsNullOrEmpty(styleName)) {
                object key = attrKeyLookupTable[styleName.ToLower(CultureInfo.InvariantCulture)];
                if (key != null) {
                    return (HtmlTextWriterStyle)key;
                }
            }

            return (HtmlTextWriterStyle)(-1);
        }


        /// <devdoc>
        /// Returns the name of the attribute corresponding to the specified HtmlTextWriterStyle value.
        /// </devdoc>
        public static string GetStyleName(HtmlTextWriterStyle styleKey) {
            if ((int)styleKey >= 0 && (int)styleKey < attrNameLookupArray.Length) {
                return attrNameLookupArray[(int)styleKey].name;
            }

            return String.Empty;
        }


        /// <devdoc>
        /// Does the specified style key require attribute value encoding if the value is being
        /// rendered in a style attribute.
        /// </devdoc>
        public static bool IsStyleEncoded(HtmlTextWriterStyle styleKey) {
            if ((int)styleKey >= 0 && (int)styleKey < attrNameLookupArray.Length) {
                return attrNameLookupArray[(int)styleKey].encode;
            }

            return true;
        }


        /// <internalonly/>
        /// <devdoc>
        /// Registers the specified style attribute to create a mapping between a string representation
        /// and the corresponding HtmlTextWriterStyle value.
        /// </devdoc>
        internal static void RegisterAttribute(string name, HtmlTextWriterStyle key) {
            RegisterAttribute(name, key, false, false);
        }


        /// <internalonly/>
        /// <devdoc>
        /// Registers the specified style attribute to create a mapping between a string representation
        /// and the corresponding HtmlTextWriterStyle value.
        /// </devdoc>
        internal static void RegisterAttribute(string name, HtmlTextWriterStyle key, bool encode) {
            RegisterAttribute(name, key, encode, false);
        }


        /// <internalonly/>
        /// <devdoc>
        /// Registers the specified style attribute to create a mapping between a string representation
        /// and the corresponding HtmlTextWriterStyle value.
        /// In addition, the mapping can include additional information about the attribute type
        /// such as whether it is a URL.
        /// </devdoc>
        internal static void RegisterAttribute(string name, HtmlTextWriterStyle key, bool encode, bool isUrl) {
            string nameLCase = name.ToLower(CultureInfo.InvariantCulture);
            
            attrKeyLookupTable.Add(nameLCase, key);
            if ((int)key < attrNameLookupArray.Length) {
                attrNameLookupArray[(int)key] = new AttributeInformation(name, encode, isUrl);
            }
        }


        /// <internalonly/>
        public override void Write(string s) {
            _writer.Write(s);
        }


        /// <internalonly/>
        public override void Write(bool value) {
            _writer.Write(value);
        }


        /// <internalonly/>
        public override void Write(char value) {
            _writer.Write(value);
        }


        /// <internalonly/>
        public override void Write(char[] buffer) {
            _writer.Write(buffer);
        }


        /// <internalonly/>
        public override void Write(char[] buffer, int index, int count) {
            _writer.Write(buffer, index, count);
        }


        /// <internalonly/>
        public override void Write(double value) {
            _writer.Write(value);
        }


        /// <internalonly/>
        public override void Write(float value) {
            _writer.Write(value);
        }


        /// <internalonly/>
        public override void Write(int value) {
            _writer.Write(value);
        }


        /// <internalonly/>
        public override void Write(long value) {
            _writer.Write(value);
        }


        /// <internalonly/>
        public override void Write(object value) {
            _writer.Write(value);
        }


        /// <internalonly/>
        public override void Write(string format, object arg0) {
            _writer.Write(format, arg0);
        }


        /// <internalonly/>
        public override void Write(string format, object arg0, object arg1) {
            _writer.Write(format, arg0, arg1);
        }


        /// <internalonly/>
        public override void Write(string format, params object[] arg) {
            _writer.Write(format, arg);
        }


        /// <devdoc>
        /// Render out the specified style attribute and value.
        /// </devdoc>
        public void WriteAttribute(string name, string value) {
            WriteAttribute(_writer, GetStyleKey(name), name, value);
        }


        /// <devdoc>
        /// Render out the specified style attribute and value.
        /// </devdoc>
        public void WriteAttribute(HtmlTextWriterStyle key, string value) {
            WriteAttribute(_writer, key, GetStyleName(key), value);
        }

        /// <devdoc>
        /// Render the specified style attribute into the specified TextWriter.
        /// This method contains all the logic for rendering a CSS name/value pair.
        /// </devdoc>
        private static void WriteAttribute(TextWriter writer, HtmlTextWriterStyle key, string name, string value) {
            writer.Write(name);
            writer.Write(':');

            bool isUrl = false;
            if (key != (HtmlTextWriterStyle)(-1)) {
                isUrl = attrNameLookupArray[(int)key].isUrl;
            }

            if (isUrl == false) {
                writer.Write(value);
            }
            else {
                WriteUrlAttribute(writer, value);
            }

            writer.Write(';');
        }

        /// <devdoc>
        /// Render the specified style attributes. This is used by HtmlTextWriter to render out all
        /// its collected style attributes.
        /// </devdoc>
        internal static void WriteAttributes(TextWriter writer, RenderStyle[] styles, int count) {       
            for (int i = 0; i < count; i++) {
                RenderStyle style = styles[i];
                WriteAttribute(writer, style.key, style.name, style.value);
            }
        }


        /// <devdoc>
        /// Start rendering a new CSS rule with the given selector.
        /// </devdoc>
        public void WriteBeginCssRule(string selector) {
            _writer.Write(selector);
            _writer.Write(" { ");
        }


        /// <devdoc>
        /// End the current CSS rule that is being rendered.
        /// </devdoc>
        public void WriteEndCssRule() {
            _writer.WriteLine(" }");
        }


        /// <internalonly/>
        public override void WriteLine(string s) {
            _writer.WriteLine(s);
        }


        /// <internalonly/>
        public override void WriteLine() {
            _writer.WriteLine();
        }


        /// <internalonly/>
        public override void WriteLine(bool value) {
            _writer.WriteLine(value);
        }


        /// <internalonly/>
        public override void WriteLine(char value) {
            _writer.WriteLine(value);
        }


        /// <internalonly/>
        public override void WriteLine(char[] buffer) {
            _writer.WriteLine(buffer);
        }


        /// <internalonly/>
        public override void WriteLine(char[] buffer, int index, int count) {
            _writer.WriteLine(buffer, index, count);
        }


        /// <internalonly/>
        public override void WriteLine(double value) {
            _writer.WriteLine(value);
        }


        /// <internalonly/>
        public override void WriteLine(float value) {
            _writer.WriteLine(value);
        }


        /// <internalonly/>
        public override void WriteLine(int value) {
            _writer.WriteLine(value);
        }


        /// <internalonly/>
        public override void WriteLine(long value) {
            _writer.WriteLine(value);
        }


        /// <internalonly/>
        public override void WriteLine(object value) {
            _writer.WriteLine(value);
        }


        /// <internalonly/>
        public override void WriteLine(string format, object arg0) {
            _writer.WriteLine(format, arg0);
        }


        /// <internalonly/>
        public override void WriteLine(string format, object arg0, object arg1) {
            _writer.WriteLine(format, arg0, arg1);
        }


        /// <internalonly/>
        public override void WriteLine(string format, params object[] arg) {
            _writer.WriteLine(format, arg);
        }


        /// <internalonly/>
        public override void WriteLine(UInt32 value) {
            _writer.WriteLine(value);
        }

        /// <devdoc>
        /// Writes out the specified URL value with the appropriate encoding
        /// and url() syntax.
        /// internal for unit testing.
        /// </devdoc>
        internal static void WriteUrlAttribute(TextWriter writer, string url) {
            string urlValue = url;

            char[] quotes = new char[] { '\'', '"'};
            char? surroundingQuote = null;

            if (StringUtil.StringStartsWith(url, "url(")) {
                int urlIndex = 4;
                int urlLength = url.Length - 4;
                if (StringUtil.StringEndsWith(url, ')')) {
                    urlLength--;
                }

                // extract out the actual URL value
                urlValue = url.Substring(urlIndex, urlLength).Trim();
            }

            // The CSS specification http://www.w3.org/TR/CSS2/syndata.html#uri says the ' and " characters are
            // optional for specifying the url values. 
            // And we do not want to pass them to UrlPathEncode if they are present.
            foreach (char quote in quotes) {
                if (StringUtil.StringStartsWith(urlValue, quote) && StringUtil.StringEndsWith(urlValue, quote)) {
                    urlValue = urlValue.Trim(quote);
                    surroundingQuote = quote;
                    break;
                }
            }

            // write out the "url(" prefix
            writer.Write("url(");
            if (surroundingQuote != null) {
                writer.Write(surroundingQuote);
            }

            writer.Write(HttpUtility.UrlPathEncode(urlValue));

            if (surroundingQuote != null) {
                writer.Write(surroundingQuote);
            }
            // write out the end of the "url()" syntax
            writer.Write(")");
        }


        /// <devdoc>
        /// Holds information about each registered style attribute.
        /// </devdoc>
        private struct AttributeInformation {
            public string name;
            public bool isUrl;
            public bool encode;

            public AttributeInformation(string name, bool encode, bool isUrl) {
                this.name = name;
                this.encode = encode;
                this.isUrl = isUrl;
            }
        }
    }

    /// <devdoc>
    /// Holds information about each style attribute that needs to be rendered.
    /// This is used by the tag rendering API of HtmlTextWriter.
    /// </devdoc>
    internal struct RenderStyle {
        public string name;
        public string value;
        public HtmlTextWriterStyle key;
    }
}
