//------------------------------------------------------------------------------
// <copyright file="NextPreviousPagerField.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Design;
using System.Globalization;
using System.Web;
using System.Web.Resources;
using System.Web.UI;

namespace System.Web.UI.WebControls {
    public class NextPreviousPagerField : DataPagerField {
        private int _startRowIndex;
        private int _maximumRows;
        private int _totalRowCount;

        public NextPreviousPagerField() {
        }

        [
        Category("Appearance"),
        DefaultValue(""),
        ResourceDescription("NextPreviousPagerField_ButtonCssClass"),
        CssClassPropertyAttribute
        ]
        public string ButtonCssClass {
            get {
                object o = ViewState["ButtonCssClass"];
                if (o != null) {
                    return (string)o;
                }
                return String.Empty;
            }
            set {
                if (value != ButtonCssClass) {
                    ViewState["ButtonCssClass"] = value;
                    OnFieldChanged();
                }
            }
        }
        
        /// <devdoc>
        /// <para>Indicates the button type for the field.</para>
        /// </devdoc>
        [
        Category("Appearance"),
        DefaultValue(ButtonType.Link),
        ResourceDescription("NextPreviousPagerField_ButtonType")
        ]
        public ButtonType ButtonType {
            get {
                object o = ViewState["ButtonType"];
                if (o != null)
                    return(ButtonType)o;
                return ButtonType.Link;
            }
            set {
                if (value < ButtonType.Button || value > ButtonType.Link) {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (value != ButtonType) {
                    ViewState["ButtonType"] = value;
                    OnFieldChanged();
                }
            }
        }

        private bool EnableNextPage {
            get {
                return _startRowIndex + _maximumRows < _totalRowCount;
            }
        }

        private bool EnablePreviousPage {
            get {
                return _startRowIndex > 0;
            }
        }

        [
        Category("Appearance"),
        DefaultValue(""),
        Editor(typeof(System.Web.UI.Design.ImageUrlEditor), typeof(UITypeEditor)),
        ResourceDescription("NextPreviousPagerField_FirstPageImageUrl"),
        SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings",
                        Justification="Required by ASP.NET parser."),
        UrlProperty()
        ]
        public string FirstPageImageUrl {
            get {
                object o = ViewState["FirstPageImageUrl"];
                if (o != null) {
                    return (string)o;
                }
                return String.Empty;
            }
            set {
                if (value != FirstPageImageUrl) {
                    ViewState["FirstPageImageUrl"] = value;
                    OnFieldChanged();
                }
            }
        }
        
        [
        Category("Appearance"),
        Localizable(true),
        ResourceDefaultValue("NextPrevPagerField_DefaultFirstPageText"),
        ResourceDescription("NextPreviousPagerField_FirstPageText")
        ]
        public string FirstPageText {
            get {
                object o = ViewState["FirstPageText"];
                if (o != null) {
                    return (string)o;
                }
                return AtlasWeb.NextPrevPagerField_DefaultFirstPageText;
            }
            set {
                if (value != FirstPageText) {
                    ViewState["FirstPageText"] = value;
                    OnFieldChanged();
                }
            }
        }
        
        [
        Category("Appearance"),
        DefaultValue(""),
        Editor(typeof(System.Web.UI.Design.ImageUrlEditor), typeof(UITypeEditor)),
        ResourceDescription("NextPreviousPagerField_LastPageImageUrl"),
        SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings",
                        Justification = "Required by ASP.NET parser."),
        UrlProperty()
        ]
        public string LastPageImageUrl {
            get {
                object o = ViewState["LastPageImageUrl"];
                if (o != null) {
                    return (string)o;
                }
                return String.Empty;
            }
            set {
                if (value != LastPageImageUrl) {
                    ViewState["LastPageImageUrl"] = value;
                    OnFieldChanged();
                }
            }
        }
        
        [
        Category("Appearance"),
        Localizable(true),
        ResourceDefaultValue("NextPrevPagerField_DefaultLastPageText"),
        ResourceDescription("NextPreviousPagerField_LastPageText")
        ]
        public string LastPageText {
            get {
                object o = ViewState["LastPageText"];
                if (o != null) {
                    return (string)o;
                }
                return AtlasWeb.NextPrevPagerField_DefaultLastPageText;
            }
            set {
                if (value != LastPageText) {
                    ViewState["LastPageText"] = value;
                    OnFieldChanged();
                }
            }
        }
        
        [
        Category("Appearance"),
        DefaultValue(""),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        ResourceDescription("NextPreviousPagerField_NextPageImageUrl"),
        SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings",
                        Justification = "Required by ASP.NET parser."),
        UrlProperty()
        ]
        public string NextPageImageUrl {
            get {
                object o = ViewState["NextPageImageUrl"];
                if (o != null) {
                    return (string)o;
                }
                return String.Empty;
            }
            set {
                if (value != NextPageImageUrl) {
                    ViewState["NextPageImageUrl"] = value;
                    OnFieldChanged();
                }
            }
        }
        
        [
        Category("Appearance"),
        Localizable(true),
        ResourceDefaultValue("NextPrevPagerField_DefaultNextPageText"),
        ResourceDescription("NextPreviousPagerField_NextPageText")
        ]
        public string NextPageText {
            get {
                object o = ViewState["NextPageText"];
                if (o != null) {
                    return (string)o;
                }
                return AtlasWeb.NextPrevPagerField_DefaultNextPageText;
            }
            set {
                if (value != NextPageText) {
                    ViewState["NextPageText"] = value;
                    OnFieldChanged();
                }
            }
        }
        
        [
        Category("Appearance"),
        DefaultValue(""),
        Editor(typeof(System.Web.UI.Design.ImageUrlEditor), typeof(UITypeEditor)),
        ResourceDescription("NextPreviousPagerField_PreviousPageImageUrl"),
        SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings",
                        Justification = "Required by ASP.NET parser."),
        UrlProperty()
        ]
        public string PreviousPageImageUrl {
            get {
                object o = ViewState["PreviousPageImageUrl"];
                if (o != null) {
                    return (string)o;
                }
                return String.Empty;
            }
            set {
                if (value != PreviousPageImageUrl) {
                    ViewState["PreviousPageImageUrl"] = value;
                    OnFieldChanged();
                }
            }
        }
        
        [
        Category("Appearance"),
        Localizable(true),
        ResourceDefaultValue("NextPrevPagerField_DefaultPreviousPageText"),
        ResourceDescription("NextPreviousPagerField_PreviousPageText")
        ]
        public string PreviousPageText {
            get {
                object o = ViewState["PreviousPageText"];
                if (o != null) {
                    return (string)o;
                }
                return AtlasWeb.NextPrevPagerField_DefaultPreviousPageText;
            }
            set {
                if (value != PreviousPageText) {
                    ViewState["PreviousPageText"] = value;
                    OnFieldChanged();
                }
            }
        }

        [
        DefaultValue(true),
        Category("Behavior"),
        ResourceDescription("NextPreviousPagerField_RenderNonBreakingSpacesBetweenControls"),
        ]
        public bool RenderNonBreakingSpacesBetweenControls {
            get {
                object o = ViewState["RenderNonBreakingSpacesBetweenControls"];
                if (o != null) {
                    return (bool)o;
                }
                return true;
            }
            set {
                if (value != RenderNonBreakingSpacesBetweenControls) {
                    ViewState["RenderNonBreakingSpacesBetweenControls"] = value;
                    OnFieldChanged();
                }
            }
        }

        [
        DefaultValue(false),
        Category("Behavior"),
        ResourceDescription("NextPreviousPagerField_RenderDisabledButtonsAsLabels"),
        ]
        public bool RenderDisabledButtonsAsLabels {
            get {
                object o = ViewState["RenderDisabledButtonsAsLabels"];
                if (o != null) {
                    return (bool)o;
                }
                return false;
            }
            set {
                if (value != RenderDisabledButtonsAsLabels) {
                    ViewState["RenderDisabledButtonsAsLabels"] = value;
                    OnFieldChanged();
                }
            }
        }
        
        [
        DefaultValue(false),
        Category("Behavior"),
        ResourceDescription("NextPreviousPagerField_ShowFirstPageButton"),
        ]
        public bool ShowFirstPageButton {
            get {
                object o = ViewState["ShowFirstPageButton"];
                if (o != null) {
                    return (bool)o;
                }
                return false;
            }
            set {
                if (value != ShowFirstPageButton) {
                    ViewState["ShowFirstPageButton"] = value;
                    OnFieldChanged();
                }
            }
        }
        
        [
        DefaultValue(false),
        Category("Behavior"),
        ResourceDescription("NextPreviousPagerField_ShowLastPageButton"),
        ]
        public bool ShowLastPageButton {
            get {
                object o = ViewState["ShowLastPageButton"];
                if (o != null) {
                    return (bool)o;
                }
                return false;
            }
            set {
                if (value != ShowLastPageButton) {
                    ViewState["ShowLastPageButton"] = value;
                    OnFieldChanged();
                }
            }
        }
        
        [
        DefaultValue(true),
        Category("Behavior"),
        ResourceDescription("NextPreviousPagerField_ShowNextPageButton"),
        ]
        public bool ShowNextPageButton {
            get {
                object o = ViewState["ShowNextPageButton"];
                if (o != null) {
                    return (bool)o;
                }
                return true;
            }
            set {
                if (value != ShowNextPageButton) {
                    ViewState["ShowNextPageButton"] = value;
                    OnFieldChanged();
                }
            }
        }
        
        
        [
        DefaultValue(true),
        Category("Behavior"),
        ResourceDescription("NextPreviousPagerField_ShowPreviousPageButton"),
        ]
        public bool ShowPreviousPageButton {
            get {
                object o = ViewState["ShowPreviousPageButton"];
                if (o != null) {
                    return (bool)o;
                }
                return true;
            }
            set {
                if (value != ShowPreviousPageButton) {
                    ViewState["ShowPreviousPageButton"] = value;
                    OnFieldChanged();
                }
            }
        }

        [
        SuppressMessage("Microsoft.Usage", "CA2204:LiteralsShouldBeSpelledCorrectly", MessageId = "nbsp",
                        Justification = "Literal is HTML escape sequence."),
        SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters",
                        MessageId = "System.Web.UI.LiteralControl.#ctor(System.String)",
                        Justification = "Literal is HTML escape sequence.")
        ]
        private void AddNonBreakingSpace(DataPagerFieldItem container) {
            if (RenderNonBreakingSpacesBetweenControls) {
                container.Controls.Add(new LiteralControl("&nbsp;"));
            }
        }
        
        protected override void CopyProperties(DataPagerField newField) {
            ((NextPreviousPagerField)newField).ButtonCssClass = ButtonCssClass;
            ((NextPreviousPagerField)newField).ButtonType = ButtonType;
            ((NextPreviousPagerField)newField).FirstPageImageUrl = FirstPageImageUrl;
            ((NextPreviousPagerField)newField).FirstPageText = FirstPageText;
            ((NextPreviousPagerField)newField).LastPageImageUrl = LastPageImageUrl;
            ((NextPreviousPagerField)newField).LastPageText = LastPageText;
            ((NextPreviousPagerField)newField).NextPageImageUrl = NextPageImageUrl;
            ((NextPreviousPagerField)newField).NextPageText = NextPageText;
            ((NextPreviousPagerField)newField).PreviousPageImageUrl = PreviousPageImageUrl;
            ((NextPreviousPagerField)newField).PreviousPageText = PreviousPageText;
            ((NextPreviousPagerField)newField).ShowFirstPageButton = ShowFirstPageButton;
            ((NextPreviousPagerField)newField).ShowLastPageButton = ShowLastPageButton;
            ((NextPreviousPagerField)newField).ShowNextPageButton = ShowNextPageButton;
            ((NextPreviousPagerField)newField).ShowPreviousPageButton = ShowPreviousPageButton;

            base.CopyProperties(newField);
        }

        protected override DataPagerField CreateField() {
            return new NextPreviousPagerField();
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        public override void HandleEvent(CommandEventArgs e) {
            if (String.IsNullOrEmpty(DataPager.QueryStringField)) {
                if (String.Equals(e.CommandName, DataControlCommands.PreviousPageCommandArgument)) {
                    int newStartRowIndex = _startRowIndex - DataPager.PageSize;
                    if (newStartRowIndex < 0) {
                        newStartRowIndex = 0;
                    }

                    DataPager.SetPageProperties(newStartRowIndex, DataPager.PageSize, true);
                }
                else if (String.Equals(e.CommandName, DataControlCommands.NextPageCommandArgument)) {
                    int newStartRowIndex = _startRowIndex + DataPager.PageSize;
                    if (newStartRowIndex > _totalRowCount) {
                        newStartRowIndex = _totalRowCount - DataPager.PageSize;
                    }

                    DataPager.SetPageProperties(newStartRowIndex, DataPager.PageSize, true);
                }
                else if (String.Equals(e.CommandName, DataControlCommands.FirstPageCommandArgument)) {
                    DataPager.SetPageProperties(0, DataPager.PageSize, true);
                }
                else if (String.Equals(e.CommandName, DataControlCommands.LastPageCommandArgument)) {
                    int newStartRowIndex;

                    int recordsOnLastPage = _totalRowCount % DataPager.PageSize;
                    if (recordsOnLastPage == 0) {
                        newStartRowIndex = _totalRowCount - DataPager.PageSize;
                    }
                    else {
                        newStartRowIndex = _totalRowCount - recordsOnLastPage;
                    }
                    DataPager.SetPageProperties(newStartRowIndex, DataPager.PageSize, true);
                }
            }
        }

        private Control CreateControl(string commandName, string buttonText, int fieldIndex, string imageUrl, bool enabled) {
            IButtonControl button;
            if (!enabled && RenderDisabledButtonsAsLabels) {
                Label label = new Label();
                label.Text = buttonText;
                if (!String.IsNullOrEmpty(ButtonCssClass)) {
                    label.CssClass = ButtonCssClass;
                }
                return label;
            }

            switch (ButtonType) {
                case ButtonType.Link: 
                    button = new LinkButton();
                    ((LinkButton)button).Enabled = enabled;
                    break;
                case ButtonType.Button: 
                    button = new Button();
                    ((Button)button).Enabled = enabled;
                    break;
                case ButtonType.Image:
                default: 
                    button = new ImageButton();
                    ((ImageButton)button).ImageUrl = imageUrl;
                    ((ImageButton)button).Enabled = enabled;
                    ((ImageButton)button).AlternateText = HttpUtility.HtmlDecode(buttonText);
                    break;
            }

            button.Text = buttonText;
            button.CausesValidation = false;
            button.CommandName = commandName;
            button.CommandArgument = fieldIndex.ToString(CultureInfo.InvariantCulture);
            WebControl webControl = button as WebControl;
            if (webControl != null && !String.IsNullOrEmpty(ButtonCssClass)) {
                webControl.CssClass = ButtonCssClass;
            }

            return button as Control;
        }

        private HyperLink CreateLink(string buttonText, int pageIndex, string imageUrl, bool enabled) {
            int pageNumber = pageIndex + 1;
            HyperLink link = new HyperLink();
            link.Text = buttonText;
            link.NavigateUrl = GetQueryStringNavigateUrl(pageNumber);
            link.ImageUrl = imageUrl;
            link.Enabled = enabled;
            if (!String.IsNullOrEmpty(ButtonCssClass)) {
                link.CssClass = ButtonCssClass;
            }
            return link;
        }

        public override void CreateDataPagers(DataPagerFieldItem container, int startRowIndex, int maximumRows, int totalRowCount, int fieldIndex) {
            _startRowIndex = startRowIndex;
            _maximumRows = maximumRows;
            _totalRowCount = totalRowCount;

            if (String.IsNullOrEmpty(DataPager.QueryStringField)) {
                CreateDataPagersForCommand(container, fieldIndex);
            }
            else {
                CreateDataPagersForQueryString(container, fieldIndex);
            }
        }

        private void CreateDataPagersForCommand(DataPagerFieldItem container, int fieldIndex) {
            if (ShowFirstPageButton) {
                container.Controls.Add(CreateControl(DataControlCommands.FirstPageCommandArgument, FirstPageText, fieldIndex, FirstPageImageUrl, EnablePreviousPage));
                AddNonBreakingSpace(container);
            }

            if (ShowPreviousPageButton) {
                container.Controls.Add(CreateControl(DataControlCommands.PreviousPageCommandArgument, PreviousPageText, fieldIndex, PreviousPageImageUrl, EnablePreviousPage));
                AddNonBreakingSpace(container);
            }

            if (ShowNextPageButton) {
                container.Controls.Add(CreateControl(DataControlCommands.NextPageCommandArgument, NextPageText, fieldIndex, NextPageImageUrl, EnableNextPage));
                AddNonBreakingSpace(container);
            }

            if (ShowLastPageButton) {
                container.Controls.Add(CreateControl(DataControlCommands.LastPageCommandArgument, LastPageText, fieldIndex, LastPageImageUrl, EnableNextPage));
                AddNonBreakingSpace(container);
            }
        }

        private void CreateDataPagersForQueryString(DataPagerFieldItem container, int fieldIndex) {
            QueryStringHandled = true;
            
            if (ShowFirstPageButton) {
                container.Controls.Add(CreateLink(FirstPageText, 0, FirstPageImageUrl, EnablePreviousPage));
                AddNonBreakingSpace(container);
            }

            if (ShowPreviousPageButton) {
                int pageIndex = (_startRowIndex / _maximumRows) - 1;
                container.Controls.Add(CreateLink(PreviousPageText, pageIndex, PreviousPageImageUrl, EnablePreviousPage));
                AddNonBreakingSpace(container);
            }

            if (ShowNextPageButton) {
                int pageIndex = (_startRowIndex + _maximumRows) / _maximumRows;
                container.Controls.Add(CreateLink(NextPageText, pageIndex, NextPageImageUrl, EnableNextPage));
                AddNonBreakingSpace(container);
            }

            if (ShowLastPageButton) {
                int pageIndex = (_totalRowCount / _maximumRows) - (_totalRowCount % _maximumRows == 0 ? 1 : 0);
                container.Controls.Add(CreateLink(LastPageText, pageIndex, LastPageImageUrl, EnableNextPage));
                AddNonBreakingSpace(container);
            }
        }

        // Required for design-time support (DesignerPagerStyle)
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public override bool Equals(object o) {
            NextPreviousPagerField field = o as NextPreviousPagerField;
            if (field != null) {
                if (String.Equals(field.ButtonCssClass, this.ButtonCssClass) &&
                    field.ButtonType == this.ButtonType &&
                    String.Equals(field.FirstPageImageUrl, this.FirstPageImageUrl) &&
                    String.Equals(field.FirstPageText, this.FirstPageText) &&
                    String.Equals(field.LastPageImageUrl, this.LastPageImageUrl) &&
                    String.Equals(field.LastPageText, this.LastPageText) &&
                    String.Equals(field.NextPageImageUrl, this.NextPageImageUrl) &&
                    String.Equals(field.NextPageText, this.NextPageText) &&
                    String.Equals(field.PreviousPageImageUrl, this.PreviousPageImageUrl) &&
                    String.Equals(field.PreviousPageText, this.PreviousPageText) &&
                    field.ShowFirstPageButton == this.ShowFirstPageButton &&
                    field.ShowLastPageButton == this.ShowLastPageButton &&
                    field.ShowNextPageButton == this.ShowNextPageButton &&
                    field.ShowPreviousPageButton == this.ShowPreviousPageButton) {
                    return true;
                }
            }
            return false;
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public override int GetHashCode() {
            return 
                this.ButtonCssClass.GetHashCode() |
                this.ButtonType.GetHashCode() |
                this.FirstPageImageUrl.GetHashCode() |
                this.FirstPageText.GetHashCode() |
                this.LastPageImageUrl.GetHashCode() |
                this.LastPageText.GetHashCode() |
                this.NextPageImageUrl.GetHashCode() |
                this.NextPageText.GetHashCode() |
                this.PreviousPageImageUrl.GetHashCode() |
                this.PreviousPageText.GetHashCode() |
                this.ShowFirstPageButton.GetHashCode() |
                this.ShowLastPageButton.GetHashCode() |
                this.ShowNextPageButton.GetHashCode() |
                this.ShowPreviousPageButton.GetHashCode();
        }

    }
}
