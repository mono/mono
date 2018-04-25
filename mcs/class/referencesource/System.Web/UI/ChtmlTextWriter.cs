//------------------------------------------------------------------------------
// <copyright file="ChtmlTextWriter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

// ChtmlTextWriter.cs
//

namespace System.Web.UI {
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.IO;
    using System.Text;
    using System.Web.UI.WebControls;
    using System.Web.Util;
    using System.Globalization;
    using System.Security.Permissions;


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class ChtmlTextWriter : Html32TextWriter {
        private Hashtable _recognizedAttributes = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
        private Hashtable _suppressedAttributes = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
        private Hashtable _globalSuppressedAttributes = new Hashtable(StringComparer.CurrentCultureIgnoreCase);


        public ChtmlTextWriter(TextWriter writer) : this(writer, DefaultTabString) {
        }


        public ChtmlTextWriter(TextWriter writer, string tabString) : base(writer, tabString) {
            _globalSuppressedAttributes["onclick"] = true;
            _globalSuppressedAttributes["ondblclick"] = true;
            _globalSuppressedAttributes["onmousedown"] = true;
            _globalSuppressedAttributes["onmouseup"] = true;
            _globalSuppressedAttributes["onmouseover"] = true;
            _globalSuppressedAttributes["onmousemove"] = true;
            _globalSuppressedAttributes["onmouseout"] = true;
            _globalSuppressedAttributes["onkeypress"] = true;
            _globalSuppressedAttributes["onkeydown"] = true;
            _globalSuppressedAttributes["onkeyup"] = true;
        
            // Supress certain element attribute pairs that can happen when Html32TextWriter performs the div table
            // substitution.
            RemoveRecognizedAttributeInternal("div", "accesskey");    // VSWhidbey 270254
            RemoveRecognizedAttributeInternal("div", "cellspacing");
            RemoveRecognizedAttributeInternal("div", "cellpadding");
            RemoveRecognizedAttributeInternal("div", "gridlines");
            RemoveRecognizedAttributeInternal("div", "rules");

            RemoveRecognizedAttributeInternal("span", "cellspacing");
            RemoveRecognizedAttributeInternal("span", "cellpadding");
            RemoveRecognizedAttributeInternal("span", "gridlines");
            RemoveRecognizedAttributeInternal("span", "rules");
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual void AddRecognizedAttribute(string elementName, string attributeName) {           
            Hashtable eltAttributes = (Hashtable) _recognizedAttributes[elementName];
            if (eltAttributes == null) {
                eltAttributes = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
                _recognizedAttributes[elementName] = eltAttributes;
            }
            eltAttributes.Add(attributeName, true);
        }
        

        /// <devdoc>
        /// Override to filter out unnecessary attributes
        /// </devdoc>
        protected override bool OnAttributeRender(string name, string value, HtmlTextWriterAttribute key) {
            Hashtable elementRecognizedAttributes = (Hashtable)_recognizedAttributes[TagName];
            if (elementRecognizedAttributes != null && elementRecognizedAttributes[name] != null) {
                return true;
            }
            
            if (_globalSuppressedAttributes[name] != null) {
                return false;
            }
            
            Hashtable elementSuppressedAttributes = (Hashtable)_suppressedAttributes[TagName];
            if (elementSuppressedAttributes != null && elementSuppressedAttributes[name] != null) {
                return false;
            }

            return true;
        }


        protected override bool OnStyleAttributeRender(string name,string value, HtmlTextWriterStyle key) {
            if (key == HtmlTextWriterStyle.TextDecoration) {
                if (StringUtil.EqualsIgnoreCase("line-through", value)) {
                    return false;
                }
            }
            return base.OnStyleAttributeRender(name, value, key);
        }


        protected override bool OnTagRender(string name, HtmlTextWriterTag key) {
            return  base.OnTagRender(name, key) && key != HtmlTextWriterTag.Span;
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual void RemoveRecognizedAttribute(string elementName, string attributeName) { 
            RemoveRecognizedAttributeInternal(elementName, attributeName);
        }
        
        private void RemoveRecognizedAttributeInternal(string elementName, string attributeName) {
            // Since recognized attributes have precedence, we need to do two operations: add this attribute
            // to the suppressed list, and remove it from the recognized list.
            Hashtable eltAttributes = (Hashtable) _suppressedAttributes[elementName];
            if (eltAttributes == null) {
                eltAttributes = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
                _suppressedAttributes[elementName] = eltAttributes;
            }
            eltAttributes.Add(attributeName, true);

            eltAttributes = (Hashtable)_recognizedAttributes[elementName];
            if (eltAttributes == null) {
                eltAttributes = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
                _recognizedAttributes[elementName] = eltAttributes;
            }
            // Note: Hashtable::Remove silently continues if the key does not exist.
            eltAttributes.Remove(attributeName);
        }


        public override void WriteBreak() {
            Write("<br>");
        }

        public override void WriteEncodedText(String text) {
            if (null == text || text.Length == 0) {
                return;
            }

            int length = text.Length;
            int start = -1;
            for(int pos = 0; pos < length; pos++) {
                int ch = text[pos];
                if(ch > 160 && ch < 256) {
                    if(start != -1) {
                        base.WriteEncodedText(text.Substring(start, pos - start));
                        start = -1;
                    }
                    base.Write(text[pos]);
                }
                else {
                    if(start == -1) {
                        start = pos;
                    }
                }
            }
            if(start != -1) {
                if(start == 0) {
                    base.WriteEncodedText(text);
                }
                else {
                    base.WriteEncodedText(text.Substring(start, length - start));
                }
            }
        }

        protected Hashtable RecognizedAttributes {
            get {
                return _recognizedAttributes;
            }
        }

        protected Hashtable SuppressedAttributes {
            get {
                return _suppressedAttributes;
            }
        }

        protected Hashtable GlobalSuppressedAttributes {
            get {
                return _globalSuppressedAttributes;
            }
        }
    }
}
