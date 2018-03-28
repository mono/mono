//------------------------------------------------------------------------------
// <copyright file="HtmlControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * HtmlControl.cs
 *
 * Copyright (c) 2000 Microsoft Corporation
 */

namespace System.Web.UI.HtmlControls {
    using System;
    using System.Globalization;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.IO;
    using System.Web.Util;
    using System.Web.UI;
    using AttributeCollection = System.Web.UI.AttributeCollection;
    using System.Security.Permissions;

/*
 * An abstract base class representing an intrinsic Html tag that
 * is not represented by both a begin and end tag, for example
 * INPUT or IMG.
 */

/// <devdoc>
///    <para>
///       The <see langword='HtmlControl'/>
///       class defines the methods, properties, and events
///       common to all HTML Server controls in the Web Forms page framework.
///    </para>
/// </devdoc>
    [
    Designer("System.Web.UI.Design.HtmlIntrinsicControlDesigner, " + AssemblyRef.SystemDesign),
    ToolboxItem(false)
    ]
    abstract public class HtmlControl : Control, IAttributeAccessor {
        internal string             _tagName;
        private AttributeCollection _attributes;



        /// <devdoc>
        /// </devdoc>
        protected HtmlControl() : this("span") {
        }


        /// <devdoc>
        /// </devdoc>
        protected HtmlControl(string tag) {
            _tagName = tag;
        }


        /*
         *  Access to collection of Attributes.
         */

        /// <devdoc>
        ///    <para>
        ///       Gets all attribute name/value pairs expressed on a
        ///       server control tag within a selected ASP.NET page.
        ///    </para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public AttributeCollection Attributes {
            get {
                if (_attributes == null)
                    _attributes = new AttributeCollection(ViewState);

                return _attributes;
            }
        }


        /*
         *  Access to collection of styles.
         */

        /// <devdoc>
        ///    <para>
        ///       Gets all
        ///       cascading style sheet (CSS) properties that
        ///       are applied
        ///       to a specified HTML Server control in an .aspx
        ///       file.
        ///    </para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public CssStyleCollection Style {
            get {
                return Attributes.CssStyle;
            }
        }

        /*
         * Property to get name of tag.
         */

        /// <devdoc>
        ///    <para>
        ///       Gets the element name of a tag that contains a runat=server
        ///       attribute/value pair.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public virtual  string TagName {
            get { return _tagName;}
        }

        /*
         * Disabled property.
         */

        /// <devdoc>
        ///    <para>
        ///       Gets or sets
        ///       a value indicating whether ---- attribute is included when a server
        ///       control is rendered.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        TypeConverter(typeof(MinimizableAttributeTypeConverter))
        ]
        public bool Disabled {
            get {
                string s = Attributes["disabled"];
                return((s != null) ? (s.Equals("disabled")) : false);
            }

            set {
                if (value)
                    Attributes["disabled"] = "disabled";
                else
                    Attributes["disabled"] = null;

            }
        }


        /// <devdoc>
        /// </devdoc>
        /// <internalonly/>
        protected override bool ViewStateIgnoresCase {
            get {
                return true;
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override ControlCollection CreateControlCollection() {
            return new EmptyControlCollection(this);
        }

        /*
         * Render the control into the given writer.
         */

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected internal override void Render(HtmlTextWriter writer) {
            RenderBeginTag(writer);
        }

        /*
         * Render only the attributes, attr1=value1 attr2=value2 ...
         */

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected virtual void RenderAttributes(HtmlTextWriter writer) {
            if (ID != null)
                writer.WriteAttribute("id", ClientID);

            Attributes.Render(writer);
        }

        /*
         * Render the begin tag and its attributes, &lt;TAGNAME attr1=value1 attr2=value2&gt;.
         */

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected virtual void RenderBeginTag(HtmlTextWriter writer) {
            writer.WriteBeginTag(TagName);
            RenderAttributes(writer);
            writer.Write(HtmlTextWriter.TagRightChar);
        }

        /*
         * HtmlControls support generic access to Attributes.
         */

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        string IAttributeAccessor.GetAttribute(string name) {
            return GetAttribute(name);
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected virtual string GetAttribute(string name) {
            return Attributes[name];
        }

        /*
         * HtmlControls support generic access to Attributes.
         */

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        void IAttributeAccessor.SetAttribute(string name, string value) {
            SetAttribute(name, value);
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected virtual void SetAttribute(string name, string value) {
            Attributes[name] = value;
        }
        
        internal void PreProcessRelativeReferenceAttribute(HtmlTextWriter writer,
            string attribName) {

            string url = Attributes[attribName];

            // Don't do anything if it's not specified
            if (String.IsNullOrEmpty(url))
                return;

            try { 
                url = ResolveClientUrl(url);
            } 
            catch (Exception e) {
                throw new HttpException(SR.GetString(SR.Property_Had_Malformed_Url, attribName, e.Message));
            }

            writer.WriteAttribute(attribName, url);
            Attributes.Remove(attribName);
        }

        internal static string MapStringAttributeToString(string s) {

            // If it's an empty string, change it to null
            if (s != null && s.Length == 0)
                return null;

            // Otherwise, just return the input
            return s;
        }

        internal static string MapIntegerAttributeToString(int n) {

            // If it's -1, change it to null
            if (n == -1)
                return null;

            // Otherwise, convert the integer to a string
            return n.ToString(NumberFormatInfo.InvariantInfo);
        }
    }
}

