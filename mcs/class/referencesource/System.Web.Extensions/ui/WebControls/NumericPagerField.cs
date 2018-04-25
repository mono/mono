//------------------------------------------------------------------------------
// <copyright file="NumericPagerField.cs" company="Microsoft">
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
    public class NumericPagerField : DataPagerField {
        private int _startRowIndex;
        private int _maximumRows;
        private int _totalRowCount;

        public NumericPagerField() {
        }

        [
        DefaultValue(5),
        Category("Appearance"),
        ResourceDescription("NumericPagerField_ButtonCount"),
        ]
        public int ButtonCount {
            get {
                object o = ViewState["ButtonCount"];
                if (o != null) {
                    return (int)o;
                }
                return 5;
            }
            set {
                if (value < 1) {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (value != ButtonCount) {
                    ViewState["ButtonCount"] = value;
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
        ResourceDescription("NumericPagerField_ButtonType"),
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

        [
        Category("Appearance"),
        DefaultValue(""),
        ResourceDescription("NumericPagerField_CurrentPageLabelCssClass"),
        CssClassPropertyAttribute
        ]
        public string CurrentPageLabelCssClass {
            get {
                object o = ViewState["CurrentPageLabelCssClass"];
                if (o != null) {
                    return (string)o;
                }
                return String.Empty;
            }
            set {
                if (value != CurrentPageLabelCssClass) {
                    ViewState["CurrentPageLabelCssClass"] = value;
                    OnFieldChanged();
                }
            }
        }
        
        [
        Category("Appearance"),
        DefaultValue(""),
        Editor(typeof(System.Web.UI.Design.ImageUrlEditor), typeof(UITypeEditor)),
        ResourceDescription("NumericPagerField_NextPageImageUrl"),
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
        ResourceDefaultValue("NumericPagerField_DefaultNextPageText"),
        ResourceDescription("NumericPagerField_NextPageText")
        ]
        public string NextPageText {
            get {
                object o = ViewState["NextPageText"];
                if (o != null) {
                    return (string)o;
                }
                return AtlasWeb.NumericPagerField_DefaultNextPageText;
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
        ResourceDescription("NumericPagerField_NextPreviousButtonCssClass"),
        CssClassPropertyAttribute
        ]
        public string NextPreviousButtonCssClass {
            get {
                object o = ViewState["NextPreviousButtonCssClass"];
                if (o != null) {
                    return (string)o;
                }
                return String.Empty;
            }
            set {
                if (value != NextPreviousButtonCssClass) {
                    ViewState["NextPreviousButtonCssClass"] = value;
                    OnFieldChanged();
                }
            }
        }
        
        [
        Category("Appearance"),
        DefaultValue(""),
        ResourceDescription("NumericPagerField_NumericButtonCssClass"),
        CssClassPropertyAttribute
        ]
        public string NumericButtonCssClass {
            get {
                object o = ViewState["NumericButtonCssClass"];
                if (o != null) {
                    return (string)o;
                }
                return String.Empty;
            }
            set {
                if (value != NumericButtonCssClass) {
                    ViewState["NumericButtonCssClass"] = value;
                    OnFieldChanged();
                }
            }
        }

        [
        Category("Appearance"),
        DefaultValue(""),
        Editor(typeof(System.Web.UI.Design.ImageUrlEditor), typeof(UITypeEditor)),
        ResourceDescription("NumericPagerField_PreviousPageImageUrl"),
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
        ResourceDefaultValue("NumericPagerField_DefaultPreviousPageText"),
        ResourceDescription("NumericPagerField_PreviousPageText")
        ]
        public string PreviousPageText {
            get {
                object o = ViewState["PreviousPageText"];
                if (o != null) {
                    return (string)o;
                }
                return AtlasWeb.NumericPagerField_DefaultPreviousPageText;
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
        ResourceDescription("NumericPagerField_RenderNonBreakingSpacesBetweenControls"),
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
            ((NumericPagerField)newField).ButtonCount = ButtonCount;
            ((NumericPagerField)newField).ButtonType = ButtonType;
            ((NumericPagerField)newField).CurrentPageLabelCssClass = CurrentPageLabelCssClass;
            ((NumericPagerField)newField).NextPageImageUrl = NextPageImageUrl;
            ((NumericPagerField)newField).NextPageText = NextPageText;
            ((NumericPagerField)newField).NextPreviousButtonCssClass = NextPreviousButtonCssClass;
            ((NumericPagerField)newField).NumericButtonCssClass = NumericButtonCssClass;
            ((NumericPagerField)newField).PreviousPageImageUrl = PreviousPageImageUrl;
            ((NumericPagerField)newField).PreviousPageText = PreviousPageText;

            base.CopyProperties(newField);
        }

        protected override DataPagerField CreateField() {
            return new NumericPagerField();
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        public override void HandleEvent(CommandEventArgs e) {
            if (String.IsNullOrEmpty(DataPager.QueryStringField)) {
                int newStartRowIndex = -1;
                int currentPageIndex = _startRowIndex / DataPager.PageSize;
                int firstButtonIndex = (_startRowIndex / (ButtonCount * DataPager.PageSize)) * ButtonCount;
                int lastButtonIndex = firstButtonIndex + ButtonCount - 1;
                int lastRecordIndex = ((lastButtonIndex + 1) * DataPager.PageSize) - 1;

                if (String.Equals(e.CommandName, DataControlCommands.PreviousPageCommandArgument)) {
                    newStartRowIndex = (firstButtonIndex - 1) * DataPager.PageSize;
                    if (newStartRowIndex < 0) {
                        newStartRowIndex = 0;
                    }
                }
                else if (String.Equals(e.CommandName, DataControlCommands.NextPageCommandArgument)) {
                    newStartRowIndex = lastRecordIndex + 1;
                    if (newStartRowIndex > _totalRowCount) {
                        newStartRowIndex = _totalRowCount - DataPager.PageSize;
                    }
                }
                else {
                    int pageIndex = Convert.ToInt32(e.CommandName, CultureInfo.InvariantCulture);
                    newStartRowIndex = pageIndex * DataPager.PageSize;
                }

                if (newStartRowIndex != -1) {
                    DataPager.SetPageProperties(newStartRowIndex, DataPager.PageSize, true);
                }
            }
        }

        private Control CreateNumericButton(string buttonText, string commandArgument, string commandName) {
            IButtonControl button;

            switch (ButtonType) {
            case ButtonType.Button:
                button = new Button();
                break;
            case ButtonType.Link:
            default:
                button = new LinkButton();
                break;
            }

            button.Text = buttonText;
            button.CausesValidation = false;
            button.CommandName = commandName;
            button.CommandArgument = commandArgument;

            WebControl webControl = button as WebControl;
            if (webControl != null && !String.IsNullOrEmpty(NumericButtonCssClass)) {
                webControl.CssClass = NumericButtonCssClass;
            }

            return button as Control;
        }

        private HyperLink CreateNumericLink(int pageIndex) {
            int pageNumber = pageIndex + 1;
            HyperLink link = new HyperLink();
            link.Text = pageNumber.ToString(CultureInfo.InvariantCulture);
            link.NavigateUrl = GetQueryStringNavigateUrl(pageNumber);

            if (!String.IsNullOrEmpty(NumericButtonCssClass)) {
                link.CssClass = NumericButtonCssClass;
            }

            return link;
        }

        private Control CreateNextPrevButton(string buttonText, string commandName, string commandArgument, string imageUrl) {
            IButtonControl button;

            switch (ButtonType) {
            case ButtonType.Link:
                button = new LinkButton();
                break;
            case ButtonType.Button:
                button = new Button();
                break;
            case ButtonType.Image:
            default:
                button = new ImageButton();
                ((ImageButton)button).ImageUrl = imageUrl;
                ((ImageButton)button).AlternateText = HttpUtility.HtmlDecode(buttonText);
                break;
            }
            button.Text = buttonText;
            button.CausesValidation = false;
            button.CommandName = commandName;
            button.CommandArgument = commandArgument;

            WebControl webControl = button as WebControl;
            if (webControl != null && !String.IsNullOrEmpty(NextPreviousButtonCssClass)) {
                webControl.CssClass = NextPreviousButtonCssClass;
            }

            return button as Control;
        }

        private HyperLink CreateNextPrevLink(string buttonText, int pageIndex, string imageUrl) {
            int pageNumber = pageIndex + 1;
            HyperLink link = new HyperLink();
            link.Text = buttonText;
            link.NavigateUrl = GetQueryStringNavigateUrl(pageNumber);
            link.ImageUrl = imageUrl;
            if (!String.IsNullOrEmpty(NextPreviousButtonCssClass)) {
                link.CssClass = NextPreviousButtonCssClass;
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
            int currentPageIndex = _startRowIndex / _maximumRows;
            int firstButtonIndex = (_startRowIndex / (ButtonCount * _maximumRows)) * ButtonCount;
            int lastButtonIndex = firstButtonIndex + ButtonCount - 1;
            int lastRecordIndex = ((lastButtonIndex + 1) * _maximumRows) - 1;

            if (firstButtonIndex != 0) {
                container.Controls.Add(CreateNextPrevButton(PreviousPageText, DataControlCommands.PreviousPageCommandArgument, fieldIndex.ToString(CultureInfo.InvariantCulture), PreviousPageImageUrl));
                AddNonBreakingSpace(container);
            }

            for (int i = 0; i < ButtonCount && _totalRowCount > ((i + firstButtonIndex) * _maximumRows); i++) {
                if (i + firstButtonIndex == currentPageIndex) {
                    Label pageNumber = new Label();
                    pageNumber.Text = (i + firstButtonIndex + 1).ToString(CultureInfo.InvariantCulture);
                    if (!String.IsNullOrEmpty(CurrentPageLabelCssClass)) {
                        pageNumber.CssClass = CurrentPageLabelCssClass;
                    }
                    container.Controls.Add(pageNumber);
                }
                else {
                    container.Controls.Add(CreateNumericButton((i + firstButtonIndex + 1).ToString(CultureInfo.InvariantCulture), fieldIndex.ToString(CultureInfo.InvariantCulture), (i + firstButtonIndex).ToString(CultureInfo.InvariantCulture)));
                }
                AddNonBreakingSpace(container);
            }

            if (lastRecordIndex < _totalRowCount - 1) {
                AddNonBreakingSpace(container);
                container.Controls.Add(CreateNextPrevButton(NextPageText, DataControlCommands.NextPageCommandArgument, fieldIndex.ToString(CultureInfo.InvariantCulture), NextPageImageUrl));
                AddNonBreakingSpace(container);
            }
        }

        private void CreateDataPagersForQueryString(DataPagerFieldItem container, int fieldIndex) {
            int currentPageIndex = _startRowIndex / _maximumRows;
            QueryStringHandled = true;
           
            int firstButtonIndex = (_startRowIndex / (ButtonCount * _maximumRows)) * ButtonCount;
            int lastButtonIndex = firstButtonIndex + ButtonCount - 1;
            int lastRecordIndex = ((lastButtonIndex + 1) * _maximumRows) - 1;

            if (firstButtonIndex != 0) {
                container.Controls.Add(CreateNextPrevLink(PreviousPageText, firstButtonIndex - 1, PreviousPageImageUrl));
                AddNonBreakingSpace(container);
            }

            for (int i = 0; i < ButtonCount && _totalRowCount > ((i + firstButtonIndex) * _maximumRows); i++) {
                if (i + firstButtonIndex == currentPageIndex) {
                    Label pageNumber = new Label();
                    pageNumber.Text = (i + firstButtonIndex + 1).ToString(CultureInfo.InvariantCulture);
                    if (!String.IsNullOrEmpty(CurrentPageLabelCssClass)) {
                        pageNumber.CssClass = CurrentPageLabelCssClass;
                    }
                    container.Controls.Add(pageNumber);
                }
                else {
                    container.Controls.Add(CreateNumericLink(i + firstButtonIndex));
                }
                AddNonBreakingSpace(container);
            }

            if (lastRecordIndex < _totalRowCount - 1) {
                AddNonBreakingSpace(container);
                container.Controls.Add(CreateNextPrevLink(NextPageText, firstButtonIndex + ButtonCount, NextPageImageUrl));
                AddNonBreakingSpace(container);
            }
        }

        // Required for design-time support (DesignerPagerStyle)
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public override bool Equals(object o) {
            NumericPagerField field = o as NumericPagerField;
            if (field != null) {
                if (String.Equals(field.ButtonCount, this.ButtonCount) &&
                    field.ButtonType == this.ButtonType &&
                    String.Equals(field.CurrentPageLabelCssClass, this.CurrentPageLabelCssClass) &&
                    String.Equals(field.NextPageImageUrl, this.NextPageImageUrl) &&
                    String.Equals(field.NextPageText, this.NextPageText) &&
                    String.Equals(field.NextPreviousButtonCssClass, this.NextPreviousButtonCssClass) &&
                    String.Equals(field.NumericButtonCssClass, this.NumericButtonCssClass) &&
                    String.Equals(field.PreviousPageImageUrl, this.PreviousPageImageUrl) &&
                    String.Equals(field.PreviousPageText, this.PreviousPageText)) {
                    return true;
                }
            }
            return false;
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public override int GetHashCode() {
            return
                this.ButtonCount.GetHashCode() |
                this.ButtonType.GetHashCode() |
                this.CurrentPageLabelCssClass.GetHashCode() |
                this.NextPageImageUrl.GetHashCode() |
                this.NextPageText.GetHashCode() |
                this.NextPreviousButtonCssClass.GetHashCode() |
                this.NumericButtonCssClass.GetHashCode() |
                this.PreviousPageImageUrl.GetHashCode() |
                this.PreviousPageText.GetHashCode();
        }

    }
}
