//------------------------------------------------------------------------------
// <copyright file="Label.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;



    /// <devdoc>
    /// <para>Interacts with the parser to build a <see cref='System.Web.UI.WebControls.Label'/> control.</para>
    /// </devdoc>
    public class LabelControlBuilder : ControlBuilder {


        /// <internalonly/>
        /// <devdoc>
        ///    <para>Specifies whether white space literals are allowed.</para>
        /// </devdoc>
        public override bool AllowWhitespaceLiterals() {
            return false;
        }
    }



    /// <devdoc>
    ///    <para>Constructs a label for displaying text programmatcially on a
    ///       page.</para>
    /// </devdoc>
    [
    ControlBuilderAttribute(typeof(LabelControlBuilder)),
    ControlValueProperty("Text"),
    DataBindingHandler("System.Web.UI.Design.TextDataBindingHandler, " + AssemblyRef.SystemDesign),
    DefaultProperty("Text"),
    ParseChildren(false),
    Designer("System.Web.UI.Design.WebControls.LabelDesigner, " + AssemblyRef.SystemDesign),
    ToolboxData("<{0}:Label runat=\"server\" Text=\"Label\"></{0}:Label>")
    ]
    public class Label : WebControl, ITextControl {

        private bool _textSetByAddParsedSubObject = false;

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.Label'/> class and renders
        ///    it as a SPAN tag.</para>
        /// </devdoc>
        public Label() {
        }


        /// <devdoc>
        /// </devdoc>
        internal Label(HtmlTextWriterTag tag) : base(tag) {
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        [
        DefaultValue(""),
        IDReferenceProperty(),
        TypeConverter(typeof(AssociatedControlConverter)),
        WebCategory("Accessibility"),
        WebSysDescription(SR.Label_AssociatedControlID),
        Themeable(false)
        ]
        public virtual string AssociatedControlID {
            get {
                string s = (string)ViewState["AssociatedControlID"];
                return (s == null) ? String.Empty : s;
            }
            set {
                ViewState["AssociatedControlID"] = value;
            }
        }

        internal bool AssociatedControlInControlTree {
            get {
                object o = ViewState["AssociatedControlNotInControlTree"];
                return (o == null ? true : (bool)o);
            }
            set {
                ViewState["AssociatedControlNotInControlTree"] = value;
            }
        }

        public override bool SupportsDisabledAttribute {
            get {
                return RenderingCompatibility < VersionUtil.Framework40;
            }
        }

        internal override bool RequiresLegacyRendering {
            get {
                return true;
            }
        }


        protected override HtmlTextWriterTag TagKey {
            get {
                if (AssociatedControlID.Length != 0) {
                    return HtmlTextWriterTag.Label;
                }
                return base.TagKey;
            }
        }


        /// <devdoc>
        /// <para>Gets or sets the text content of the <see cref='System.Web.UI.WebControls.Label'/>
        /// control.</para>
        /// </devdoc>
        [
        Localizable(true),
        Bindable(true),
        WebCategory("Appearance"),
        DefaultValue(""),
        WebSysDescription(SR.Label_Text),
        PersistenceMode(PersistenceMode.InnerDefaultProperty)
        ]
        public virtual string Text {
            get {
                object o = ViewState["Text"];
                return((o == null) ? String.Empty : (string)o);
            }
            set {
                if (HasControls()) {
                    Controls.Clear();
                }
                ViewState["Text"] = value;
            }
        }


        protected override void AddAttributesToRender(HtmlTextWriter writer) {
            string associatedControlID = AssociatedControlID;
            if (associatedControlID.Length != 0) {
                if (AssociatedControlInControlTree) {
                    Control wc = FindControl(associatedControlID);
                    if (wc == null) {
                        // Don't throw in the designer.
                        if (!DesignMode)
                            throw new HttpException(SR.GetString(SR.LabelForNotFound, associatedControlID, ID));
                    }
                    else {
                        writer.AddAttribute(HtmlTextWriterAttribute.For, wc.ClientID);
                    }
                }
                else {
                    writer.AddAttribute(HtmlTextWriterAttribute.For, associatedControlID);
                }
            }

            base.AddAttributesToRender(writer);
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected override void AddParsedSubObject(object obj) {
            if (HasControls()) {
                base.AddParsedSubObject(obj);
            }
            else {
                if (obj is LiteralControl) {
                    if (_textSetByAddParsedSubObject) {
                        Text += ((LiteralControl)obj).Text;
                    }
                    else {
                        Text = ((LiteralControl)obj).Text;
                    }
                    _textSetByAddParsedSubObject = true;
                }
                else {
                    string currentText = Text;
                    if (currentText.Length != 0) {
                        Text = String.Empty;
                        base.AddParsedSubObject(new LiteralControl(currentText));
                    }
                    base.AddParsedSubObject(obj);
                }
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///    <para>Load previously saved state.
        ///       Overridden to synchronize Text property with LiteralContent.</para>
        /// </devdoc>
        protected override void LoadViewState(object savedState) {
            if (savedState != null) {
                base.LoadViewState(savedState);
                string s = (string)ViewState["Text"];
                // Dev10 703061 If Text is set, we want to clear out any child controls, but not dirty viewstate
                if (s != null && HasControls()) {
                    Controls.Clear();
                }
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// <para>Renders the contents of the <see cref='System.Web.UI.WebControls.Label'/> into the specified writer.</para>
        /// </devdoc>
        protected internal override void RenderContents(HtmlTextWriter writer) {
            if (HasRenderingData()) {
                base.RenderContents(writer);
            }
            else {
                writer.Write(Text);
            }
        }
    }
}

