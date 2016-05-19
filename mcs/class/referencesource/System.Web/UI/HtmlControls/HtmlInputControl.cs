//------------------------------------------------------------------------------
// <copyright file="HtmlInputControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * HtmlInputControl.cs
 *
 * Copyright (c) 2000 Microsoft Corporation
 */

namespace System.Web.UI.HtmlControls {

    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;
    using Debug=System.Web.Util.Debug;
    using System.Security.Permissions;

/*
 * An abstract base class representing an intrinsic INPUT tag.
 */

/// <devdoc>
///    <para>
///       The <see langword='HtmlInputControl'/> abstract class defines
///       the methods, properties, and events common to all HTML input controls.
///       These include controls for the &lt;input type=text&gt;, &lt;input
///       type=submit&gt;, and &lt;input type=file&gt; elements.
///    </para>
/// </devdoc>
    [
    ControlBuilderAttribute(typeof(HtmlEmptyTagControlBuilder))
    ]
    abstract public class HtmlInputControl : HtmlControl {
        private string _type;

        /*
         *  Creates a new Input
         */

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.HtmlControls.HtmlInputControl'/> class.</para>
        /// </devdoc>
        protected HtmlInputControl(string type) : base("input") {
            _type = type;

            // VSWhidbey 546690: Need to add the type value to the Attributes collection to match Everett behavior.
            Attributes["type"] = type;
        }

        /*
         * Name property
         */

        /// <devdoc>
        ///    <para>
        ///       Gets the value of the HTML
        ///       Name attribute that will be rendered to the browser.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public virtual string Name {
            get { 
                return UniqueID;
                //string s = Attributes["name"];
                //return ((s != null) ? s : String.Empty);
            }
            set { 
                //Attributes["name"] = MapStringAttributeToString(value);
            }
        }

        // Value that gets rendered for the Name attribute
        internal virtual string RenderedNameAttribute {
            get {
                return Name;
                //string name = Name;
                //if (name.Length == 0)
                //    return UniqueID;
                
                //return name;
            }
        }

        /*
         * Value property.
         */

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the contents of a text box.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public virtual string Value {
            get {
                string s = Attributes["value"];
                return((s != null) ? s : String.Empty);
            }
            set {
                Attributes["value"] = MapStringAttributeToString(value);
            }
        }

        /*
         * Type of input
         */

        /// <devdoc>
        ///    <para>
        ///       Gets the Type attribute for a particular HTML input control.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public string Type {
            get {
                string s = Attributes["type"];
                if (!string.IsNullOrEmpty(s)) {
                    return s;
                }
                return((_type != null) ? _type : String.Empty);
            }
        }

        /*
         * Override to render unique name attribute.
         * The name attribute is owned by the framework.
         */

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected override void RenderAttributes(HtmlTextWriter writer) {
        
            writer.WriteAttribute("name", RenderedNameAttribute);
            Attributes.Remove("name");
            bool removedTypeAttribute = false;
            string type = Type;
            if (!String.IsNullOrEmpty(type)) {
                writer.WriteAttribute("type", type);
                Attributes.Remove("type");
                removedTypeAttribute = true;
            }

            base.RenderAttributes(writer);
            if (removedTypeAttribute && DesignMode) {
                Attributes.Add("type", type);
            }
            writer.Write(" /");
        }

    }
}
