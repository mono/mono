//------------------------------------------------------------------------------
// <copyright file="BulletedList.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Drawing.Design;
    using System.Web.Util;



    /// <devdoc>
    ///     <para>Generates a bulleted list.</para>
    /// </devdoc>
    [DefaultProperty("BulletStyle")]
    [DefaultEvent("Click")]
    [Designer("System.Web.UI.Design.WebControls.BulletedListDesigner, " + AssemblyRef.SystemDesign)]
    [SupportsEventValidation]
    public class BulletedList : ListControl, IPostBackEventHandler {

        private static readonly object EventClick = new object();

        private bool _cachedIsEnabled;
        private int _firstItem;
        private int _itemCount;


        /// <devdoc></devdoc>
        public BulletedList() {
            _firstItem = 0;
            _itemCount = -1;
        }


        /// <devdoc>
        ///    <para>Gets the value of the base classes AutoPostBack propert.
        ///       AutoPostBack is not applicable to the bulleted list control</para>
        /// </devdoc>
        [
        Browsable(false),
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override bool AutoPostBack {
            get {
                return base.AutoPostBack;
            }
            set {
                throw new NotSupportedException(SR.GetString(SR.Property_Set_Not_Supported, "AutoPostBack", this.GetType().ToString()));
            }
        }


        /// <devdoc>
        ///     <para>Gets or sets a value indicating the style of bullet to be
        ///        applied to the list.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(BulletStyle.NotSet),
        WebSysDescription(SR.BulletedList_BulletStyle)
        ]
        public virtual BulletStyle BulletStyle {
            get {
                object o = ViewState["BulletStyle"];
                return((o == null) ? BulletStyle.NotSet : (BulletStyle)o);
            }
            set {
                if (value < BulletStyle.NotSet || value > BulletStyle.CustomImage) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["BulletStyle"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the source of the image used for an
        ///       Image styled bulleted list.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty(),
        WebSysDescription(SR.BulletedList_BulletImageUrl)
        ]
        public virtual string BulletImageUrl {
            get {
                object o = ViewState["BulletImageUrl"];
                return((o == null) ? string.Empty : (string)o);
            }
            set {
                ViewState["BulletImageUrl"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets the EmptyControlCollection.</para>
        /// </devdoc>
        public override ControlCollection Controls {
            get {
                return new EmptyControlCollection(this);
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the display mode of the bulleted list.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(BulletedListDisplayMode.Text),
        WebSysDescription(SR.BulletedList_BulletedListDisplayMode) //
        ]
        public virtual BulletedListDisplayMode DisplayMode {
            get {
                object o = ViewState["DisplayMode"];
                return ((o == null) ? BulletedListDisplayMode.Text : (BulletedListDisplayMode)o);
            }
            set {
                if (value < BulletedListDisplayMode.Text || value > BulletedListDisplayMode.LinkButton) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["DisplayMode"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the value at which an ordered list should
        ///       begin its numbering.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(1),
        WebSysDescription(SR.BulletedList_FirstBulletNumber)
        ]
        public virtual int FirstBulletNumber {
            get {
                object o = ViewState["FirstBulletNumber"];
                return((o == null) ? 1 : (int)o);
            }
            set {
                ViewState["FirstBulletNumber"] = value;
            }
        }


        /// <summary>
        /// <para>Indicates whether the control will be rendered when the data source has no items.</para>
        /// </summary>
        [DefaultValue(false)]
        [Themeable(true)]
        [WebCategory("Behavior")]
        [WebSysDescription(SR.ListControl_RenderWhenDataEmpty)]
        public virtual bool RenderWhenDataEmpty {
            get {
                object o = ViewState["RenderWhenDataEmpty"];
                return ((o == null) ? false : (bool)o);
            }
            set {
                ViewState["RenderWhenDataEmpty"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets the value of selected index.  Not applicable to the
        ///       bulleted list control.</para>
        /// </devdoc>
        [
        Bindable(false),
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override int SelectedIndex {
            get {
                return base.SelectedIndex;
            }
            set {
                throw new NotSupportedException(SR.GetString(SR.BulletedList_SelectionNotSupported));
            }
        }


        /// <devdoc>
        ///    <para>Gets the selected item.  Not applicable to the
        ///       bulleted list control.</para>
        /// </devdoc>
        [
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override ListItem SelectedItem {
            get {
                return base.SelectedItem;
            }
        }

        [
        Bindable(false),
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override string SelectedValue {
            get {
                return base.SelectedValue;
            }
            set {
                throw new NotSupportedException(SR.GetString(SR.BulletedList_SelectionNotSupported));
            }
        }


        /// <devdoc>
        ///    <para>Gets the HtmlTextWriterTag value that corresponds
        ///       to the particular bulleted list.</para>
        /// </devdoc>
        protected override HtmlTextWriterTag TagKey {
            get {
                return TagKeyInternal;
            }
        }

        internal HtmlTextWriterTag TagKeyInternal {
            get {
                switch (BulletStyle) {
                    // Ordered Lists
                    case BulletStyle.LowerAlpha:
                    case BulletStyle.UpperAlpha:
                    case BulletStyle.LowerRoman:
                    case BulletStyle.UpperRoman:
                    case BulletStyle.Numbered:
                        return HtmlTextWriterTag.Ol;
                    // Unordered Lists
                    case BulletStyle.Square:
                    case BulletStyle.Circle:
                    case BulletStyle.Disc:
                        return HtmlTextWriterTag.Ul;
                    // Image Lists
                    case BulletStyle.CustomImage:
                        return HtmlTextWriterTag.Ul;
                    // Not Set
                    case BulletStyle.NotSet:
                        // NotSet is specified as an unordered list.
                        return HtmlTextWriterTag.Ul;
                    default:
                        Debug.Assert(false, "Invalid BulletStyle");
                        return HtmlTextWriterTag.Ol;
                }
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the Target window when the
        ///       list is displayed as Hyperlinks.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(""),
        WebSysDescription(SR.BulletedList_Target),
        TypeConverter(typeof(TargetConverter))
        ]
        public virtual string Target {
            get {
                object o = ViewState["Target"];
                return ((o == null) ? string.Empty : (string)o);
            }
            set {
                ViewState["Target"] = value;
            }
        }

        [
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override string Text {
            get {
                return base.Text;
            }
            set {
                throw new NotSupportedException(SR.GetString(SR.BulletedList_TextNotSupported));
            }
        }


        /// <devdoc>
        ///    <para>Occurs when the a link button is clicked.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.BulletedList_OnClick)
        ]
        public event BulletedListEventHandler Click {
            add {
                Events.AddHandler(EventClick, value);
            }
            remove {
                Events.RemoveHandler(EventClick, value);
            }
        }


        /// <devdoc>
        ///    <para>Adds HTML attributes that need to be rendered.</para>
        /// </devdoc>
        protected override void AddAttributesToRender(HtmlTextWriter writer) {
            bool addBulletNumber = false;

            switch (BulletStyle) {
                case BulletStyle.NotSet:
                    break;
                case BulletStyle.Numbered:
                    writer.AddStyleAttribute(HtmlTextWriterStyle.ListStyleType, "decimal");
                    addBulletNumber = true;
                    break;
                case BulletStyle.LowerAlpha:
                    writer.AddStyleAttribute(HtmlTextWriterStyle.ListStyleType, "lower-alpha");
                    addBulletNumber = true;
                    break;
                case BulletStyle.UpperAlpha:
                    writer.AddStyleAttribute(HtmlTextWriterStyle.ListStyleType, "upper-alpha");
                    addBulletNumber = true;
                    break;
                case BulletStyle.LowerRoman:
                    writer.AddStyleAttribute(HtmlTextWriterStyle.ListStyleType, "lower-roman");
                    addBulletNumber = true;
                    break;
                case BulletStyle.UpperRoman:
                    writer.AddStyleAttribute(HtmlTextWriterStyle.ListStyleType, "upper-roman");
                    addBulletNumber = true;
                    break;
                case BulletStyle.Disc:
                    writer.AddStyleAttribute(HtmlTextWriterStyle.ListStyleType, "disc");
                    break;
                case BulletStyle.Circle:
                    writer.AddStyleAttribute(HtmlTextWriterStyle.ListStyleType, "circle");
                    break;
                case BulletStyle.Square:
                    writer.AddStyleAttribute(HtmlTextWriterStyle.ListStyleType, "square");
                    break;
                case BulletStyle.CustomImage:
                    String url = ResolveClientUrl(BulletImageUrl);
                    writer.AddStyleAttribute(HtmlTextWriterStyle.ListStyleImage, "url(" + HttpUtility.UrlPathEncode(url) + ")");
                    break;
                default:
                    Debug.Assert(false, "Invalid BulletStyle");
                    break;
            }
            int firstBulletNumber = FirstBulletNumber;
            if ((addBulletNumber == true) && (firstBulletNumber != 1)) {
                writer.AddAttribute("start", firstBulletNumber.ToString(CultureInfo.InvariantCulture));
            }
            base.AddAttributesToRender(writer);
        }

        private string GetPostBackEventReference(string eventArgument) {
            if (CausesValidation && Page.GetValidators(ValidationGroup).Count > 0) {
                return ClientScriptManager.JscriptPrefix + Util.GetClientValidatedPostback(this, ValidationGroup, eventArgument);
            } else {
                return Page.ClientScript.GetPostBackClientHyperlink(this, eventArgument, true);
            }
        }

        /// <devdoc>
        /// <para>Raises the Click event.</para>
        /// </devdoc>
        protected virtual void OnClick(BulletedListEventArgs e) {
            BulletedListEventHandler onClickHandler = (BulletedListEventHandler)Events[EventClick];
            if (onClickHandler != null)
                onClickHandler(this, e);
        }


        /// <devdoc>
        ///     <para>Writes the text of each bullet according to the list's display mode.</para>
        /// </devdoc>
        protected virtual void RenderBulletText(ListItem item, int index, HtmlTextWriter writer) {
            switch (DisplayMode) {
                case BulletedListDisplayMode.Text:
                    if (!item.Enabled) {
                        RenderDisabledAttributeHelper(writer, false);
                        writer.RenderBeginTag(HtmlTextWriterTag.Span);
                    }
                    HttpUtility.HtmlEncode(item.Text, writer);
                    if (!item.Enabled) {
                        writer.RenderEndTag();
                    }
                    break;

                case BulletedListDisplayMode.HyperLink:
                    if (_cachedIsEnabled && item.Enabled) {
                        writer.AddAttribute(HtmlTextWriterAttribute.Href, ResolveClientUrl(item.Value));
                        string target = Target;
                        if (!String.IsNullOrEmpty(target)) {
                            writer.AddAttribute(HtmlTextWriterAttribute.Target, Target);
                        }
                    }
                    else {
                        RenderDisabledAttributeHelper(writer, item.Enabled);
                    }

                    RenderAccessKey(writer, AccessKey);
                    writer.RenderBeginTag(HtmlTextWriterTag.A);
                    HttpUtility.HtmlEncode(item.Text, writer);
                    writer.RenderEndTag();
                    break;

                case BulletedListDisplayMode.LinkButton:
                    if (_cachedIsEnabled && item.Enabled) {
                        writer.AddAttribute(HtmlTextWriterAttribute.Href, GetPostBackEventReference(index.ToString(CultureInfo.InvariantCulture)));
                    }
                    else {
                        RenderDisabledAttributeHelper(writer, item.Enabled);
                    }

                    RenderAccessKey(writer, AccessKey);
                    writer.RenderBeginTag(HtmlTextWriterTag.A);
                    HttpUtility.HtmlEncode(item.Text, writer);
                    writer.RenderEndTag();
                    break;

                default:
                    Debug.Assert(false, "Invalid BulletedListDisplayMode");
                    break;
            }

        }

        private void RenderDisabledAttributeHelper(HtmlTextWriter writer, bool isItemEnabled) {
            if (SupportsDisabledAttribute) {
                writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
            }
            else if (!isItemEnabled && !String.IsNullOrEmpty(DisabledCssClass)) {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, DisabledCssClass);
            }
        }

        internal void RenderAccessKey(HtmlTextWriter writer, string AccessKey) {
            string s = AccessKey;
            if (s.Length > 0) {
                writer.AddAttribute(HtmlTextWriterAttribute.Accesskey, s);
            }
        }

        protected internal override void Render(HtmlTextWriter writer) {
            // Don't render anything if the control is empty (unless the developer opts in)
            if (Items.Count == 0 && !RenderWhenDataEmpty) {
                return;
            }

            base.Render(writer);
        }


        /// <devdoc>
        ///    <para>Renders the ListItems as bullets in the bulleted list.</para>
        /// </devdoc>
        protected internal override void RenderContents(HtmlTextWriter writer) {
            _cachedIsEnabled = IsEnabled;

            if (_itemCount == -1) {
                for (int i = 0; i < Items.Count; i++) {
                    Items[i].RenderAttributes(writer);
                    writer.RenderBeginTag(HtmlTextWriterTag.Li);
                    RenderBulletText(Items[i], i, writer);
                    writer.RenderEndTag();
                }
            }
            else {
                for (int i = _firstItem; i < _firstItem + _itemCount; i++) {
                    Items[i].RenderAttributes(writer);
                    writer.RenderBeginTag(HtmlTextWriterTag.Li);
                    RenderBulletText(Items[i], i, writer);
                    writer.RenderEndTag();
                }
            }
        }

        protected virtual void RaisePostBackEvent(string eventArgument) {
            ValidateEvent(this.UniqueID, eventArgument);

            if (CausesValidation) {
                Page.Validate(ValidationGroup);
            }
            OnClick(new BulletedListEventArgs(Int32.Parse(eventArgument, CultureInfo.InvariantCulture)));
        }

        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument) {
            RaisePostBackEvent(eventArgument);
        }
    }
}
