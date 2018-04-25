//------------------------------------------------------------------------------
// <copyright file="HyperLink.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    /// <devdoc>
    /// <para>Interacts with the parser to build a <see cref='System.Web.UI.WebControls.HyperLink'/>.</para>
    /// </devdoc>
    public class HyperLinkControlBuilder : ControlBuilder {


        /// <devdoc>
        ///    <para>Gets a value to indicate whether or not white spaces are allowed in literals for this control. This
        ///       property is read-only.</para>
        /// </devdoc>
        public override bool AllowWhitespaceLiterals() {
            return false;
        }
    }



    /// <devdoc>
    ///    <para>Creates a link for the browser to navigate to another page.</para>
    /// </devdoc>
    [
    ControlBuilderAttribute(typeof(HyperLinkControlBuilder)),
    DataBindingHandler("System.Web.UI.Design.HyperLinkDataBindingHandler, " + AssemblyRef.SystemDesign),
    DefaultProperty("Text"),
    Designer("System.Web.UI.Design.WebControls.HyperLinkDesigner, " + AssemblyRef.SystemDesign),
    ToolboxData("<{0}:HyperLink runat=\"server\">HyperLink</{0}:HyperLink>"),
    ParseChildren(false)
    ]
    public class HyperLink : WebControl {
        private bool _textSetByAddParsedSubObject = false;

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.HyperLink'/> class.</para>
        /// </devdoc>
        public HyperLink() : base(HtmlTextWriterTag.A) {
        }


        /// <devdoc>
        ///    <para>Gets or sets the URL reference to an image to display as an alternative to plain text for the
        ///       hyperlink.</para>
        /// </devdoc>
        [
        Bindable(true),
        WebCategory("Appearance"),
        DefaultValue(""),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty(),
        WebSysDescription(SR.HyperLink_ImageUrl)
        ]
        public virtual string ImageUrl {
            get {
                string s = (string)ViewState["ImageUrl"];
                return((s == null) ? String.Empty : s);
            }
            set {
                ViewState["ImageUrl"] = value;
            }
        }

        [
        WebCategory("Appearance"),
        DefaultValue(typeof(Unit), ""),
        WebSysDescription(SR.HyperLink_ImageHeight)
        ]
        public virtual Unit ImageHeight {
            get {
                object o = ViewState["ImageHeight"];
                if (o != null) {
                    return (Unit)o;
                }
                return Unit.Empty;
            }
            set {
                ViewState["ImageHeight"] = value;
            }
        }

        [
        WebCategory("Appearance"),
        DefaultValue(typeof(Unit), ""),
        WebSysDescription(SR.HyperLink_ImageWidth)
        ]
        public virtual Unit ImageWidth {
            get {
                object o = ViewState["ImageWidth"];
                if (o != null) {
                    return (Unit)o;
                }
                return Unit.Empty;
            }
            set {
                ViewState["ImageWidth"] = value;
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets the URL reference to navigate to when the hyperlink is clicked.</para>
        /// </devdoc>
        [
        Bindable(true),
        WebCategory("Navigation"),
        DefaultValue(""),
        Editor("System.Web.UI.Design.UrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty(),
        WebSysDescription(SR.HyperLink_NavigateUrl)
        ]
        public string NavigateUrl {
            get {
                string s = (string)ViewState["NavigateUrl"];
                return((s == null) ? String.Empty : s);
            }
            set {
                ViewState["NavigateUrl"] = value;
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


        /// <devdoc>
        ///    <para>Gets or sets the target window or frame the contents of
        ///       the <see cref='System.Web.UI.WebControls.HyperLink'/> will be displayed into when clicked.</para>
        /// </devdoc>
        [
        WebCategory("Navigation"),
        DefaultValue(""),
        WebSysDescription(SR.HyperLink_Target),
        TypeConverter(typeof(TargetConverter))
        ]
        public string Target {
            get {
                string s = (string)ViewState["Target"];
                return((s == null) ? String.Empty : s);
            }
            set {
                ViewState["Target"] = value;
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Gets or sets the text displayed for the <see cref='System.Web.UI.WebControls.HyperLink'/>.</para>
        /// </devdoc>
        [
        Localizable(true),
        Bindable(true),
        WebCategory("Appearance"),
        DefaultValue(""),
        WebSysDescription(SR.HyperLink_Text),
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

        /// <internalonly/>
        /// <devdoc>
        /// <para>Adds the attribututes of the a <see cref='System.Web.UI.WebControls.HyperLink'/> to the output
        ///    stream for rendering.</para>
        /// </devdoc>
        protected override void AddAttributesToRender(HtmlTextWriter writer) {
            if (Enabled && !IsEnabled && SupportsDisabledAttribute) {
                // We need to do the cascade effect on the server, because the browser
                // only renders as disabled, but doesn't disable the functionality.
                writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
            }

            base.AddAttributesToRender(writer);

            string s = NavigateUrl;
            if ((s.Length > 0) && IsEnabled) {
                string resolvedUrl = ResolveClientUrl(s);
                writer.AddAttribute(HtmlTextWriterAttribute.Href, resolvedUrl);
            }
            s = Target;
            if (s.Length > 0) {
                writer.AddAttribute(HtmlTextWriterAttribute.Target, s);
            }
        }

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
                        //Text was set to String.Empty, which cause clearing all child controls when LoadViewState(DevDiv Bugs 159505)
                        Text = null;
                        base.AddParsedSubObject(new LiteralControl(currentText));
                    }
                    base.AddParsedSubObject(obj);
                }
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///    Load previously saved state.
        ///    Overridden to synchronize Text property with LiteralContent.
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
        /// <para>Displays the <see cref='System.Web.UI.WebControls.HyperLink'/> on a page.</para>
        /// </devdoc>
        protected internal override void RenderContents(HtmlTextWriter writer) {
            string s = ImageUrl;
            if (s.Length > 0) {
                Image img = new Image();

                // NOTE: The Url resolution happens right here, because the image is not parented
                //       and will not be able to resolve when it tries to do so.
                img.ImageUrl = ResolveClientUrl(s);
                img.UrlResolved = true;
                img.GenerateEmptyAlternateText = true;
                if (ImageHeight != Unit.Empty) {
                    img.Height = ImageHeight;
                }
                if (ImageWidth != Unit.Empty) {
                    img.Width = ImageWidth;
                }

                s = ToolTip;
                if (s.Length != 0) {
                    img.ToolTip = s;
                }

                s = Text;
                if (s.Length != 0) {
                    img.AlternateText = s;
                }
                img.RenderControl(writer);
            }
            else {
                if (HasRenderingData()) {
                    base.RenderContents(writer);
                }
                else {
                    writer.Write(Text);
                }
            }
        }
    }
}

