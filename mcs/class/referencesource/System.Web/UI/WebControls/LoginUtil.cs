//------------------------------------------------------------------------------
// <copyright file="LoginUtil.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections.Specialized;
    using System.Drawing;
    using System.Net.Mail;
    using System.Security.Principal;
    using System.Web.Security;

    // Utility methods used by the Login controls
    internal static class LoginUtil {
        private const string _userNameReplacementKey = "<%\\s*UserName\\s*%>";
        private const string _passwordReplacementKey = "<%\\s*Password\\s*%>";
        private const string _templateDesignerRegion = "0";

        // Will apply style to literals if provided and determines if it should be visible
        internal static void ApplyStyleToLiteral(Literal literal, string text, Style style, bool setTableCellVisible) {
            // setTableCellVisible is used when we DO NOT make the whole table cell invisible, because in some layouts
            // it must exist for things to align correctly and its uncommon that this property will be empty anyway.

            bool visible = false;
            if (!String.IsNullOrEmpty(text)) {
                literal.Text = text;
                if (style != null) {
                    LoginUtil.SetTableCellStyle(literal, style);
                }
                visible = true;
            }

            if (setTableCellVisible) {
                LoginUtil.SetTableCellVisible(literal, visible);
            }
            else {
                literal.Visible = visible;
            }
        }

        // These two functions are used by the LoginControls for the border table styles
        internal static void CopyBorderStyles(WebControl control, Style style) {
            if (style == null || style.IsEmpty) {
                return;
            }

            control.BorderStyle = style.BorderStyle;
            control.BorderColor = style.BorderColor;
            control.BorderWidth = style.BorderWidth;
            control.BackColor = style.BackColor;
            control.CssClass = style.CssClass;
        }

        internal static void CopyStyleToInnerControl(WebControl control, Style style) {
            if (style == null || style.IsEmpty) {
                return;
            }

            control.ForeColor = style.ForeColor;
            control.Font.CopyFrom(style.Font);
        }

        internal static Table CreateChildTable(bool convertingToTemplate) {
            if (convertingToTemplate) {
                return new Table();
            }
            return new ChildTable(2);
        }

        private static MailMessage CreateMailMessage(string email, string userName, string password,
                                                     MailDefinition mailDefinition, string defaultBody,
                                                     Control owner) {
            ListDictionary replacements = new ListDictionary();

            // Need to html encode the username and password if the body is HTML
            if (mailDefinition.IsBodyHtml) {
                userName = HttpUtility.HtmlEncode(userName);
                password = HttpUtility.HtmlEncode(password);
            }
            replacements.Add(_userNameReplacementKey, userName);
            replacements.Add(_passwordReplacementKey, password);
            
            if (String.IsNullOrEmpty(mailDefinition.BodyFileName) && defaultBody != null) {
                return mailDefinition.CreateMailMessage(email, replacements, defaultBody, owner);
            }
            else {
                return mailDefinition.CreateMailMessage(email, replacements, owner);
            }
        }
        
        internal static MembershipProvider GetProvider(string providerName) {
            MembershipProvider provider;
            if (String.IsNullOrEmpty(providerName)) {
                provider = Membership.Provider;
            }
            else {
                provider = Membership.Providers[providerName];
                if (provider == null) {
                    throw new HttpException(SR.GetString(SR.WebControl_CantFindProvider));
                }
            }
            return provider;
        }

        // Returns the IPrincipal of the currently logged in user.  Returns null if no user
        // is logged in, or if Page and HttpContext are not available.
        internal static IPrincipal GetUser(Control c) {
            IPrincipal user = null;

            Page page = c.Page;
            if (page != null) {
                user = page.User;
            }
            else {
                HttpContext context = HttpContext.Current;
                if (context != null) {
                    user = context.User;
                }
            }

            return user;
        }

        // Returns the username of the currently logged in user.  Returns null or String.Empty
        // if no user is logged in, or if Page and HttpContext are not available.
        internal static string GetUserName(Control c) {
            string userName = null;

            IPrincipal user = GetUser(c);
            if (user != null) {
                IIdentity identity = user.Identity;
                if (identity != null) {
                    userName = identity.Name;
                }
            }

            return userName;
        }

        internal static void SendPasswordMail(string email, string userName, string password,
                                              MailDefinition mailDefinition,
                                              string defaultSubject, string defaultBody,
                                              OnSendingMailDelegate onSendingMailDelegate,
                                              OnSendMailErrorDelegate onSendMailErrorDelegate,
                                              Control owner) {
            // If the MailAddress ctor throws an exception, raise the error event but do not
            // rethrow the exception.  We do not rethrow the exception since the email address
            // is user-entered data, and it should not cause an unhandled exception in the page.
            // If any other part of creating or sending the MailMessage throws an exception,
            // it is most likely a developer error so the exception should be rethrown.
            // (VSWhidbey 490984)
            try {
                new MailAddress(email);
            }
            catch (Exception e) {
                SendMailErrorEventArgs args = new SendMailErrorEventArgs(e);
                // SendMailErrorEventArgs.Handled should be true, to indicate that the exception
                // will not be rethrown. (VSWhidbey 529233)
                args.Handled = true;
                onSendMailErrorDelegate(args);
                return;
            }

            try {
                using (MailMessage message = CreateMailMessage(email, userName, password,
                                                               mailDefinition, defaultBody,
                                                               owner)) {
                    if (mailDefinition.SubjectInternal == null && defaultSubject != null) {
                        message.Subject = defaultSubject;
                    }
                    MailMessageEventArgs args = new MailMessageEventArgs(message);
                    onSendingMailDelegate(args);
                    if (args.Cancel) {
                        return;
                    }

                    SmtpClient smtp = new SmtpClient();
                    smtp.Send(message);
                }
            } catch (Exception e) {
                SendMailErrorEventArgs args = new SendMailErrorEventArgs(e);
                onSendMailErrorDelegate(args);

                // If the error wasn't handled, we rethrow
                if (!args.Handled) {
                    throw;
                }
            }
        }

        // Sets the style of the table cell that contains the control.
        internal static void SetTableCellStyle(Control control, Style style) {
            Control parent = control.Parent;
            if (parent != null) {
                ((TableCell) parent).ApplyStyle(style);
            }
        }

        // Sets the visibility of the table cell that contains the control.  The whole cell is made invisible
        // to shrink rendered size and improve rendered appearance if cell padding or spacing is set.
        internal static void SetTableCellVisible(Control control, bool visible) {
            Control parent = control.Parent;
            if (parent != null) {
                parent.Visible = visible;
            }
        }

        internal delegate void OnSendingMailDelegate(MailMessageEventArgs e);
        internal delegate void OnSendMailErrorDelegate(SendMailErrorEventArgs e);

        /// <devdoc>
        /// TableRow that only renders if any of its cells are visible.  Improves the appearance
        /// of the control by removing empty rows.  Use this class instead of changing the
        /// visibility of the table rows, since that causes problems in the designer.
        /// (VSWhidbey 81265)
        /// </devdoc>
        internal sealed class DisappearingTableRow : TableRow {
            protected internal override void Render(HtmlTextWriter writer) {
                bool rowVisible = false;
                foreach (TableCell cell in Cells) {
                    if (cell.Visible) {
                        rowVisible = true;
                        break;
                    }
                }

                if (rowVisible) {
                    base.Render(writer);
                }
            }
        }

        /// <devdoc>
        /// The base class for all containers used for individual views in the
        /// Login and PasswordRecovery controls.  Internal because used from PasswordRecovery.
        /// </devdoc>
        internal abstract class GenericContainer<ControlType> : WebControl where ControlType : WebControl, 
                                                                                               IBorderPaddingControl, 
                                                                                               IRenderOuterTableControl {
            private bool _renderDesignerRegion = false;
            private ControlType _owner;

            private Table _layoutTable;
            private Table _borderTable;

            public GenericContainer(ControlType owner) {
                _owner = owner;
            }

            internal int BorderPadding {
                get {
                    return _owner.BorderPadding;
                }
            }

            internal Table BorderTable {
                get {
                    return _borderTable;
                }
                set {
                    _borderTable = value;
                }
            }

            protected abstract bool ConvertingToTemplate { get; }

            internal Table LayoutTable {
                get {
                    return _layoutTable;
                }
                set {
                    _layoutTable = value;
                }
            }

            internal ControlType Owner {
                get {
                    return _owner;
                }
            }

            internal bool RenderDesignerRegion {
                get {
                    return DesignMode && _renderDesignerRegion;
                }
                set {
                    _renderDesignerRegion = value;
                }
            }

            private bool RenderOuterTable {
                get {
                    return _owner.RenderOuterTable;
                }
            }

            // Returns true when using the default template, and false when using a custom template.
            private bool UsingDefaultTemplate {
                get {
                    return (BorderTable != null);
                }
            }

            public sealed override void Focus() {
                throw new NotSupportedException(SR.GetString(SR.NoFocusSupport, this.GetType().Name));
            }

            private Control FindControl<RequiredType>(string id, bool required, string errorResourceKey) {
                Control control = FindControl(id);
                if (control is RequiredType) {
                    return control;
                }
                else {
                    // Do not throw exception at design-time (VSWhidbey 429452)
                    if (required && !Owner.DesignMode) {
                        throw new HttpException(SR.GetString(errorResourceKey, Owner.ID, id));
                    }
                    else {
                        return null;
                    }
                }
            }

            protected Control FindOptionalControl<RequiredType>(string id) {
                return FindControl<RequiredType>(id, false, null);
            }

            protected Control FindRequiredControl<RequiredType>(string id, string errorResourceKey) {
                return FindControl<RequiredType>(id, true, errorResourceKey);
            }

            protected internal virtual string ModifiedOuterTableStylePropertyName() {
                // Verify that style properties are not set (not different than their defaults).
                if (BorderPadding != 1) {
                    return "BorderPadding";
                }
                return ModifiedOuterTableBasicStylePropertyName(Owner);
            }

            /// <devdoc>
            ///     Renders the template contents.  The default template is rendered directly since it is already a table.
            ///     A user template is rendered inside a table with one row and one cell.
            ///     User a single-cell table instead of a <div>, since the <div> always spans the full width of its
            ///     containing element, while a <table> sets width to contents.
            /// </devdoc>
            protected internal sealed override void Render(HtmlTextWriter writer) {
                if (!RenderOuterTable) {
                    string propertyName = ModifiedOuterTableStylePropertyName();
                    if (!string.IsNullOrEmpty(propertyName)) {
                        throw new InvalidOperationException(SR.GetString(SR.IRenderOuterTableControl_CannotSetStyleWhenDisableRenderOuterTable,
                                                            propertyName, _owner.GetType().Name, _owner.ID));
                    }
                }

                if (UsingDefaultTemplate) {
                    // If we are converting to template, then do not apply the base attributes or ControlStyle
                    // to the BorderTable or LayoutTable.  These will be rendered around the template contents
                    // at runtime.
                    if (!ConvertingToTemplate) {
                        BorderTable.CopyBaseAttributes(this);
                        if (ControlStyleCreated) {
                            // I assume we apply the BorderStyles and Font/ForeColor separately to make the rendering
                            // work in IE Quirks mode.  If we only wanted to support Standards mode, we could probably
                            // apply all the styles to the BorderTable.  I'm not changing this for Whidbey RTM
                            // since it is high-risk.
                            LoginUtil.CopyBorderStyles(BorderTable, ControlStyle);
                            LoginUtil.CopyStyleToInnerControl(LayoutTable, ControlStyle);
                        }
                    }

                    // Need to set height and width even when converting to template, to force the inner
                    // LayoutTable to fill the contents of the outer control.
                    LayoutTable.Height = Height;
                    LayoutTable.Width = Width;

                    if (RenderOuterTable) {
                        RenderContents(writer);
                    }
                    else {
                        // just render what would have been rendered in the outer table's single cell
                        ControlCollection controlsInCell = BorderTable.Rows[0].Cells[0].Controls;
                        RenderControls(writer, controlsInCell);
                    }
                }
                else {
                    RenderContentsInUnitTable(writer);
                }
            }

            private void RenderContentsInUnitTable(HtmlTextWriter writer) {
                // there are two situations in which we need an outer table:
                // 1) it was explicitly asked for (RenderOuterTable property), or
                // 2) a custom template is in use and the designer needs a container to host 
                //    the editable inner region (RenderDesignerRegion property)
                
                if (!RenderOuterTable && !RenderDesignerRegion) {
                    RenderControls(writer, Controls);
                    return;
                }

                LayoutTable table = new LayoutTable(1, 1, Page);

                // Don't render out the child controls if we are using region editing, just output the region attribute
                if (RenderDesignerRegion) {
                    table[0, 0].Attributes[HtmlTextWriter.DesignerRegionAttributeName] = _templateDesignerRegion;
                }
                else {
                    foreach (Control child in Controls) {
                        table[0, 0].Controls.Add(child);
                    }
                }

                // don't want to copy any style attributes to the outer table if we're only rendering it to satisfy
                // the designer, otherwise the designer will show styles that won't be rendered on an actual page
                if (RenderOuterTable) {
                    string parentID = Parent.ID;
                    if ((parentID != null) && (parentID.Length != 0)) {
                        table.ID = Parent.ClientID;
                    }

                    table.CopyBaseAttributes(this);
                    table.ApplyStyle(ControlStyle);

                    // This table should not add any cell padding or spacing around the template contents
                    table.CellPadding = 0;
                    table.CellSpacing = 0;
                }

                table.RenderControl(writer);
            }

            private static void RenderControls(HtmlTextWriter writer, ControlCollection controls) {
                foreach (Control child in controls) {
                    child.RenderControl(writer);
                }
            }

            // Throws an exception if a control with the specified id and type is found within
            // the container.  Does not throw exception at design-time.
            protected void VerifyControlNotPresent<RequiredType>(string id, string errorResourceKey) {
                Control control = FindOptionalControl<RequiredType>(id);
                if (control != null && !Owner.DesignMode) {
                    throw new HttpException(SR.GetString(errorResourceKey, Owner.ID, id));
                }
            }
        }

        internal static string ModifiedOuterTableBasicStylePropertyName(WebControl control) {
            // Verify that basic style properties are not set (not different than their defaults).
            if (control.BackColor != Color.Empty) {
                return "BackColor";
            }
            if (control.BorderColor != Color.Empty) {
                return "BorderColor";
            }
            if (control.BorderWidth != Unit.Empty) {
                return "BorderWidth";
            }
            if (control.BorderStyle != BorderStyle.NotSet) {
                return "BorderStyle";
            }
            if (!String.IsNullOrEmpty(control.CssClass)) {
                return "CssClass";
            }
            if (control.ForeColor != Color.Empty) {
                return "ForeColor";
            }
            if (control.Height != Unit.Empty) {
                return "Height";
            }
            if (control.Width != Unit.Empty) {
                return "Width";
            }
            return String.Empty;
        }
    }
}

