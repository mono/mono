//------------------------------------------------------------------------------
// <copyright file="XhtmlBasicObjectListAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Drawing;
using System.Globalization;
using System.Security.Permissions;
using System.Web.Mobile;
using System.Web.UI.MobileControls;
using System.Web.UI.MobileControls.Adapters;
using System.Collections.Specialized;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource.XhtmlAdapters
#else
namespace System.Web.UI.MobileControls.Adapters.XhtmlAdapters
#endif
{

    /// <include file='doc\XhtmlBasicObjectListAdapter.uex' path='docs/doc[@for="XhtmlObjectListAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class XhtmlObjectListAdapter : XhtmlControlAdapter {
        /// <include file='doc\XhtmlBasicObjectListAdapter.uex' path='docs/doc[@for="XhtmlObjectListAdapter.BackToList"]/*' />
        internal protected static readonly String BackToList = "__back";
        /// <include file='doc\XhtmlBasicObjectListAdapter.uex' path='docs/doc[@for="XhtmlObjectListAdapter.ShowMoreFormat"]/*' />
        internal protected static readonly String ShowMoreFormat = "__more{0}";
        /// <include file='doc\XhtmlBasicObjectListAdapter.uex' path='docs/doc[@for="XhtmlObjectListAdapter.ShowMore"]/*' />
        internal protected static readonly String ShowMore = "__more";
        private const int _modeDetails = 1;

        private BooleanOption _hasItemDetails = BooleanOption.NotSet;
        private int _visibleTableFieldsCount = -1;

        /// <include file='doc\XhtmlBasicObjectListAdapter.uex' path='docs/doc[@for="XhtmlObjectListAdapter.Control"]/*' />
        protected new ObjectList Control {
            get {
                return base.Control as ObjectList;
            }
        }

        private int VisibleTableFieldsCount {
            get {
                if (_visibleTableFieldsCount == -1) {
                    int[] tableFieldIndices = Control.TableFieldIndices;
                    _visibleTableFieldsCount = 0;
                    for (int i = 0; i < tableFieldIndices.Length; i++) {
                        if (Control.AllFields[tableFieldIndices[i]].Visible) {
                            _visibleTableFieldsCount++;
                        }
                    }
                }
                return _visibleTableFieldsCount;
            }
        }

        // Encapsulate conditional call to Control.PreShowItemCommands for intelligibility.
        private void ConditionalPreShowItemCommands () {
            if (SecondaryUIMode == _modeDetails && Control.Items.Count > 0) {
                Control.PreShowItemCommands (Control.SelectedIndex);
            }
        }

        /// <include file='doc\XhtmlBasicObjectListAdapter.uex' path='docs/doc[@for="XhtmlObjectListAdapter.CreateTemplatedUI"]/*' />
        public override void CreateTemplatedUI(bool doDataBind) {
            if (Control.ViewMode == ObjectListViewMode.List) {
                Control.CreateTemplatedItemsList(doDataBind);
            }
            else {
                Control.CreateTemplatedItemDetails(doDataBind);
            }
        }

        private void DetermineFieldIndicesAndCount (out int fieldCount, out int[] fieldIndices){
            fieldIndices = Control.TableFieldIndices;
            fieldCount = fieldIndices.Length;

            if (fieldCount == 0) {
                fieldIndices = new int[1];
                fieldIndices[0] = Control.LabelFieldIndex;
                fieldCount = 1;
            }
        }

        public override bool HandlePostBackEvent(String eventArgument) {
            // Review: Consider replacing switch.
            switch (Control.ViewMode) {
                case ObjectListViewMode.List:

                    if (eventArgument.StartsWith(ShowMore, StringComparison.Ordinal)) {
                        int itemIndex = ParseItemArg(eventArgument);

                        if (Control.SelectListItem(itemIndex, true)) {
                            if (Control.SelectedIndex > -1) {
                                // ObjectListViewMode.Commands and .Details same for HTML,
                                // but cannot access ObjLst.Details in Commands mode.
                                Control.ViewMode = ObjectListViewMode.Details;
                            }                    
                        }
                    }
                    else {
                        int itemIndex = -1;
                        try {
                            itemIndex = Int32.Parse(eventArgument, CultureInfo.InvariantCulture);
                        }
                        catch (System.FormatException) {
                            // 
                            throw new Exception (SR.GetString(
                                SR.XhtmlObjectListAdapter_InvalidPostedData));
                        }
                        if (Control.SelectListItem(itemIndex, false)) {
                            Control.RaiseDefaultItemEvent(itemIndex);
                        }
                    }
                    return true;

                case ObjectListViewMode.Commands:
                case ObjectListViewMode.Details:

                    if (eventArgument == BackToList) {
                        Control.ViewMode = ObjectListViewMode.List;
                        return true;
                    }
                    break;
            }

            return false;
        }
                    
        /// <include file='doc\XhtmlBasicObjectListAdapter.uex' path='docs/doc[@for="XhtmlObjectListAdapter.HasCommands"]/*' />
        protected bool HasCommands() {
            return Control.Commands.Count > 0;
        }

        /// <include file='doc\XhtmlBasicObjectListAdapter.uex' path='docs/doc[@for="XhtmlObjectListAdapter.HasDefaultCommand"]/*' />
        protected bool HasDefaultCommand () {
            String controlDefaultCommand = Control.DefaultCommand;
            return controlDefaultCommand != null && controlDefaultCommand.Length > 0;
        }

        /// <include file='doc\XhtmlBasicObjectListAdapter.uex' path='docs/doc[@for="XhtmlObjectListAdapter.HasItemDetails"]/*' />
        protected virtual bool HasItemDetails() {
            if (_hasItemDetails == BooleanOption.NotSet) {
                // Calculate how many visible fields are shown in list view.

                int visibleFieldsInListView;
                int[] tableFieldIndices = Control.TableFieldIndices;
                if (tableFieldIndices.Length != 0) {
                    visibleFieldsInListView = VisibleTableFieldsCount;
                }
                else {
                    visibleFieldsInListView = Control.AllFields[Control.LabelFieldIndex].Visible ?
                        1 : 0;
                }
                // Calculate the number of visible fields.
                _hasItemDetails = BooleanOption.False;
                int visibleFieldCount = 0;
                foreach (ObjectListField field in Control.AllFields) {
                    if (field.Visible) {
                        visibleFieldCount++;
                        if (visibleFieldCount > visibleFieldsInListView) {
                            _hasItemDetails = BooleanOption.True;
                            break;
                        }
                    }
                }
            }

            return _hasItemDetails == BooleanOption.True;
        }

        // Return true iff there is exactly one command and it is the default command.
        /// <include file='doc\XhtmlBasicObjectListAdapter.uex' path='docs/doc[@for="XhtmlObjectListAdapter.OnlyHasDefaultCommand"]/*' />
        protected bool OnlyHasDefaultCommand () {
            return Control.Commands.Count == 1 &&
                (String.Compare (Control.DefaultCommand, Control.Commands[0].Name, true /* ignore case */, CultureInfo.CurrentCulture) == 0);
        }

        /// <include file='doc\XhtmlBasicObjectListAdapter.uex' path='docs/doc[@for="XhtmlObjectListAdapter.OnPreRender"]/*' />
        public override void OnPreRender(EventArgs e) {
            base.OnPreRender (e);
            SetSecondaryUIMode ();
            ConditionalPreShowItemCommands ();
        }


        private static int ParseItemArg(String arg) {
            return Int32.Parse(arg.Substring(ShowMore.Length), CultureInfo.InvariantCulture);
        }

        /// <include file='doc\XhtmlBasicObjectListAdapter.uex' path='docs/doc[@for="XhtmlObjectListAdapter.Render"]/*' />
        public override void Render(XhtmlMobileTextWriter writer) {
            if (Control.ViewMode == ObjectListViewMode.List) {
                if (Control.HasControls()) {
                    ConditionalRenderOpeningDivElement(writer);
                    RenderChildren(writer);
                    ConditionalRenderClosingDivElement(writer);
                }
                else {
                    RenderItemsList(writer);
                }
            }
            else {
                if (Control.Selection.HasControls()) {
                    ConditionalRenderOpeningDivElement(writer);
                    Control.Selection.RenderChildren(writer);
                    ConditionalRenderClosingDivElement(writer);
                }
                else {
                    RenderItemDetails(writer, Control.Selection);
                }
                // Review:  The HTML case calls FormAdapter.DisablePager, but
                // this seems unnecessary, since the pager is not rendered in the secondary ui case anyway.
            }
        }

        // Render the details view
        /// <include file='doc\XhtmlBasicObjectListAdapter.uex' path='docs/doc[@for="XhtmlObjectListAdapter.RenderItemDetails"]/*' />
        protected virtual void RenderItemDetails(XhtmlMobileTextWriter writer, ObjectListItem item) {
            if (Control.AllFields.Count == 0) {
                return;
            }
            if (!Device.Tables) {
                RenderItemDetailsWithoutTableTags(writer, item);
                return;
            }


            Style labelStyle = Control.LabelStyle;
            Style subCommandStyle = Control.CommandStyle;
            Style subCommandStyleNoItalic = (Style)subCommandStyle.Clone();
            subCommandStyleNoItalic.Font.Italic = BooleanOption.False;

            writer.ClearPendingBreak(); // we are writing a block level element in all cases.
            ConditionalEnterLayout(writer, Style);
            writer.WriteBeginTag ("table");
            ConditionalRenderClassAttribute(writer);
            writer.Write(">");
            writer.Write("<tr><td colspan=\"2\">");
            ConditionalEnterStyle(writer, labelStyle);
            writer.WriteEncodedText (item[Control.LabelFieldIndex]);
            ConditionalExitStyle(writer, labelStyle);
            writer.WriteLine("</td></tr>");
            Color foreColor = (Color)Style[Style.ForeColorKey, true];
            RenderRule (writer, foreColor, 2);
            RenderItemFieldsInDetailsView (writer, item);
            RenderRule (writer, foreColor, 2);
            ConditionalPopPhysicalCssClass(writer);
            writer.WriteEndTag("table");
            ConditionalExitLayout(writer, Style);

            ConditionalEnterStyle(writer, subCommandStyleNoItalic);
            writer.Write("[&nbsp;");

            ObjectListCommandCollection commands = Control.Commands;
            String cssClass = GetCustomAttributeValue(XhtmlConstants.CssClassCustomAttribute);
            String subCommandClass = GetCustomAttributeValue(XhtmlConstants.CssCommandClassCustomAttribute);
            if (subCommandClass == null || subCommandClass.Length == 0) {
                subCommandClass = cssClass;
            }

            foreach (ObjectListCommand command in commands) {
                RenderPostBackEventAsAnchor(writer, command.Name, command.Text, null /* accessKey */, subCommandStyle, subCommandClass);
                writer.Write("&nbsp;|&nbsp;");
            }
            String controlBCT = Control.BackCommandText;
            String backCommandText = (controlBCT == null || controlBCT.Length == 0) ?
                GetDefaultLabel(BackLabel) :
                controlBCT;

            RenderPostBackEventAsAnchor(writer, BackToList, backCommandText, null /* accessKey */, subCommandStyle, subCommandClass);
            writer.Write("&nbsp;]");
            ConditionalExitStyle(writer, subCommandStyleNoItalic);
        }

        private void RenderItemDetailsWithoutTableTags(XhtmlMobileTextWriter writer, ObjectListItem item)
        {
            if (Control.VisibleItemCount == 0) {
                return;
            }
            
            Style style = this.Style;
            Style labelStyle = Control.LabelStyle;
            Style subCommandStyle = Control.CommandStyle;
            Style subCommandStyleNoItalic = (Style)subCommandStyle.Clone();
            subCommandStyleNoItalic.Font.Italic = BooleanOption.False;

            ConditionalRenderOpeningDivElement(writer);

            String cssClass = GetCustomAttributeValue(XhtmlConstants.CssClassCustomAttribute);
            String labelClass = GetCustomAttributeValue(XhtmlConstants.CssLabelClassCustomAttribute); 
            if (labelClass == null || labelClass.Length == 0) {
                labelClass = cssClass;
            }
            ConditionalEnterStyle(writer, labelStyle);
            bool requiresLabelClassSpan = CssLocation == StyleSheetLocation.PhysicalFile && labelClass != null && labelClass.Length > 0;
            if (requiresLabelClassSpan) {
                writer.WriteBeginTag("span");
                writer.WriteAttribute("class", labelClass, true);
                writer.Write(">");
            }
            writer.Write(item[Control.LabelFieldIndex]);
            writer.SetPendingBreak();
            if (requiresLabelClassSpan) {
                writer.WriteEndTag("span");
            }
            ConditionalExitStyle(writer, labelStyle);
            writer.WritePendingBreak();

            IObjectListFieldCollection fields = Control.AllFields;
            int fieldIndex = 0;

            ConditionalEnterStyle(writer, style);
            foreach (ObjectListField field in fields)
            {
                if (field.Visible) {
                    writer.Write(field.Title + ":");
                    writer.Write("&nbsp;");
                    writer.Write(item[fieldIndex]);
                    writer.WriteBreak();
                }
                fieldIndex++;
            }
            ConditionalExitStyle(writer, style);

            String commandClass = GetCustomAttributeValue(XhtmlConstants.CssCommandClassCustomAttribute);

            ConditionalEnterStyle(writer, subCommandStyleNoItalic);
            if ((String) Device[XhtmlConstants.BreaksOnInlineElements] != "true") {
                writer.Write("[&nbsp;");
            }
            ConditionalEnterStyle(writer, subCommandStyle);

            ObjectListCommandCollection commands = Control.Commands;
            foreach (ObjectListCommand command in commands)
            {
                RenderPostBackEventAsAnchor(writer, command.Name, command.Text);
                if ((String) Device[XhtmlConstants.BreaksOnInlineElements] != "true") {
                    writer.Write("&nbsp;|&nbsp;");
                }
            }
            String controlBCT = Control.BackCommandText;
            String backCommandText = controlBCT == null || controlBCT.Length == 0 ?
                GetDefaultLabel(BackLabel) :
                Control.BackCommandText;

            RenderPostBackEventAsAnchor(writer, BackToList, backCommandText);
            ConditionalExitStyle(writer, subCommandStyle);
            if ((String) Device[XhtmlConstants.BreaksOnInlineElements] != "true") {
                writer.Write("&nbsp;]");
            }
            ConditionalExitStyle(writer, subCommandStyleNoItalic);

            ConditionalRenderClosingDivElement(writer);
        }
        
        // Called from RenderItemDetails.  (Extracted for intelligibility.)
        private void RenderItemFieldsInDetailsView (XhtmlMobileTextWriter writer, ObjectListItem item) {
            Style style = Style;
            IObjectListFieldCollection fields = Control.AllFields;
            foreach (ObjectListField field in fields) {
                if (field.Visible) {
                    writer.Write("<tr><td>");
                    ConditionalEnterStyle(writer, Style);
                    writer.WriteEncodedText (field.Title);
                    ConditionalExitStyle(writer, Style);
                    writer.Write("</td><td>");
                    ConditionalEnterStyle(writer, style);
                    writer.WriteEncodedText (item [fields.IndexOf (field)]);
                    ConditionalExitStyle(writer, style);
                    writer.WriteLine("</td></tr>");
                }
            }
        }

        // Render the list view
        /// <include file='doc\XhtmlBasicObjectListAdapter.uex' path='docs/doc[@for="XhtmlObjectListAdapter.RenderItemsList"]/*' />
        protected virtual void RenderItemsList(XhtmlMobileTextWriter writer) {

            if (Control.VisibleItemCount == 0) {
                return;
            }

            if (!Device.Tables) {
                RenderItemsListWithoutTableTags(writer);
                return;
            }

            int pageStart = Control.FirstVisibleItemIndex;
            int pageSize = Control.VisibleItemCount;
            ObjectListItemCollection items = Control.Items;

            bool hasDefaultCommand = HasDefaultCommand();
            bool onlyHasDefaultCommand = OnlyHasDefaultCommand();
            bool requiresDetailsScreen = RequiresDetailsScreen ();
            bool itemRequiresHyperlink = requiresDetailsScreen || hasDefaultCommand;
            bool itemRequiresMoreButton = requiresDetailsScreen && hasDefaultCommand;

            int fieldCount;
            int[] fieldIndices;
            DetermineFieldIndicesAndCount (out fieldCount, out fieldIndices);

            Style style = this.Style;
            Style subCommandStyle = Control.CommandStyle;
            Style labelStyle = Control.LabelStyle;
            Color foreColor = (Color)style[Style.ForeColorKey, true];

            // Note: table width is not supported in DTD (the text of the rec says it's supported; a 
            ClearPendingBreakIfDeviceBreaksOnBlockLevel(writer); // we are writing a block level element in all cases.
            ConditionalEnterLayout(writer, Style);
            RenderOpeningListTag(writer, "table");
            RenderListViewTableHeader (writer, fieldCount, fieldIndices, itemRequiresMoreButton);
            RenderRule (writer, foreColor , fieldCount + (itemRequiresMoreButton ? 1 : 0));
            for (int i = 0; i < pageSize; i++) {
                ObjectListItem item = items[pageStart + i];
                RenderListViewItem (writer, item, fieldCount, fieldIndices, itemRequiresMoreButton, itemRequiresHyperlink);
            }
            RenderRule (writer, foreColor , fieldCount + (itemRequiresMoreButton ? 1 : 0));
            RenderClosingListTag(writer, "table");
            ConditionalExitLayout(writer, Style);
        }

        private void RenderItemsListWithoutTableTags(XhtmlMobileTextWriter writer)
        {
            if (Control.VisibleItemCount == 0) {
                return;
            }

            ConditionalRenderOpeningDivElement(writer);
            int startIndex = Control.FirstVisibleItemIndex;
            int pageSize = Control.VisibleItemCount;
            ObjectListItemCollection items = Control.Items;
            IObjectListFieldCollection allFields = Control.AllFields;
            int count = allFields.Count;

            int nextStartIndex =  startIndex + pageSize;
            int labelFieldIndex = Control.LabelFieldIndex;


            Style style = this.Style;
            Style labelStyle = Control.LabelStyle;
            String cssClass = GetCustomAttributeValue(XhtmlConstants.CssClassCustomAttribute);
            String labelClass = GetCustomAttributeValue(XhtmlConstants.CssLabelClassCustomAttribute); 
            if (labelClass == null || labelClass.Length == 0) {
                labelClass = cssClass;
            }
            ConditionalEnterStyle(writer, labelStyle);
            bool requiresLabelClassSpan = CssLocation == StyleSheetLocation.PhysicalFile && labelClass != null && labelClass.Length > 0;
            if (requiresLabelClassSpan) {
                writer.WriteBeginTag("span");
                writer.WriteAttribute("class", labelClass, true);
                writer.Write(">");
            }
            writer.Write(Control.AllFields[labelFieldIndex].Title);
            writer.SetPendingBreak();
            if (requiresLabelClassSpan) {
                writer.WriteEndTag("span");
            }
            ConditionalExitStyle(writer, labelStyle);
            writer.WritePendingBreak();

            bool hasDefaultCommand = HasDefaultCommand();
            bool onlyHasDefaultCommand = OnlyHasDefaultCommand();
            bool requiresDetailsScreen = !onlyHasDefaultCommand && HasCommands();
            // if there is > 1 visible field, need a details screen
            for (int visibleFields = 0, i = 0; !requiresDetailsScreen && i < count; i++) {
                visibleFields += allFields[i].Visible ? 1 : 0;
                requiresDetailsScreen = 
                    requiresDetailsScreen || visibleFields > 1;
            }   
            bool itemRequiresHyperlink = requiresDetailsScreen || hasDefaultCommand;
            bool itemRequiresMoreButton = requiresDetailsScreen && hasDefaultCommand;


            Style subCommandStyle = Control.CommandStyle;
            subCommandStyle.Alignment = style.Alignment;
            subCommandStyle.Wrapping = style.Wrapping;

            ConditionalEnterStyle(writer, style);
            for (int i = startIndex; i < nextStartIndex; i++) {
                ObjectListItem item = items[i];

                String accessKey = GetCustomAttributeValue(item, XhtmlConstants.AccessKeyCustomAttribute);
                String itemClass = GetCustomAttributeValue(item, XhtmlConstants.CssClassCustomAttribute);
                if (itemRequiresHyperlink) {

                    RenderPostBackEventAsAnchor(writer,
                        hasDefaultCommand ?
                        item.Index.ToString(CultureInfo.InvariantCulture) :
                        String.Format(CultureInfo.InvariantCulture, ShowMoreFormat, item.Index),
                        item[labelFieldIndex], accessKey, Style, cssClass);
                }
                else {
                    bool requiresItemClassSpan = CssLocation == StyleSheetLocation.PhysicalFile && itemClass != null && itemClass.Length > 0;
                    if (requiresItemClassSpan) {
                        writer.WriteBeginTag("span");
                        writer.WriteAttribute("class", itemClass, true);
                        writer.Write(">");
                    }
                    writer.Write(item[labelFieldIndex]);
                    if (requiresItemClassSpan) {
                        writer.WriteEndTag("span");
                    }
                }

                if (itemRequiresMoreButton) {
                    String commandClass = GetCustomAttributeValue(XhtmlConstants.CssCommandClassCustomAttribute);
                    BooleanOption cachedItalic = subCommandStyle.Font.Italic;
                    subCommandStyle.Font.Italic = BooleanOption.False;
                    ConditionalEnterFormat(writer, subCommandStyle);
                    if ((String)Device[XhtmlConstants.BreaksOnInlineElements] != "true") {
                        writer.Write(" [");
                    }
                    ConditionalExitFormat(writer, subCommandStyle);
                    subCommandStyle.Font.Italic = cachedItalic;
                    ConditionalEnterFormat(writer, subCommandStyle);
                    String controlMT = Control.MoreText;
                    String moreText = (controlMT == null || controlMT.Length == 0) ?
                        GetDefaultLabel(MoreLabel) :
                        controlMT;
                    RenderPostBackEventAsAnchor(writer,
                        String.Format(CultureInfo.InvariantCulture, ShowMoreFormat, item.Index), 
                        moreText, 
                        null /*accessKey*/,
                        subCommandStyle,
                        commandClass);
                    ConditionalExitFormat(writer, subCommandStyle);
                    subCommandStyle.Font.Italic = BooleanOption.False;
                    ConditionalEnterFormat(writer, subCommandStyle);
                    if ((String)Device[XhtmlConstants.BreaksOnInlineElements] != "true") {
                        writer.Write("]");
                    }
                    ConditionalExitFormat(writer, subCommandStyle);
                    subCommandStyle.Font.Italic = cachedItalic;
                }

                if (i < (nextStartIndex - 1)) {
                    writer.WriteBreak();            
                }
                else {
                    writer.SetPendingBreak();
                }
            }
            ConditionalExitStyle(writer, style);
            ConditionalRenderClosingDivElement(writer);
        }

        // Render a single ObjectListItem in list view.
        private void RenderListViewItem (XhtmlMobileTextWriter writer, ObjectListItem item, int fieldCount, int[] fieldIndices, bool itemRequiresMoreButton, bool itemRequiresHyperlink) {
            Style style = Style;
            Style subCommandStyle = Control.CommandStyle;
            String accessKey = GetCustomAttributeValue(item, XhtmlConstants.AccessKeyCustomAttribute);
            String cssClass = GetCustomAttributeValue(item, XhtmlConstants.CssClassCustomAttribute);
            String subCommandClass = GetCustomAttributeValue(XhtmlConstants.CssCommandClassCustomAttribute);
            if (subCommandClass == null || subCommandClass.Length == 0) {
                subCommandClass = cssClass;
            }

            writer.WriteLine("<tr>");

            // Render fields.
            for (int field = 0; field < fieldCount; field++) {
                writer.Write("<td>");
                if (field == 0 && itemRequiresHyperlink) {
                    String eventArgument = HasDefaultCommand() ? item.Index.ToString(CultureInfo.InvariantCulture) : String.Format(CultureInfo.InvariantCulture, ShowMoreFormat, item.Index.ToString(CultureInfo.InvariantCulture));
                    RenderPostBackEventAsAnchor(writer, eventArgument, item[fieldIndices[0]], accessKey, Style, cssClass);
                }
                else {
                    writer.WriteEncodedText (item[fieldIndices[field]]);
                }
                writer.WriteLine("</td>");
            }

            if (itemRequiresMoreButton) {
                writer.Write("<td>");
                String controlMT = Control.MoreText;
                String moreText = (controlMT == null || controlMT.Length == 0) ? GetDefaultLabel(MoreLabel) : controlMT;
                RenderPostBackEventAsAnchor(writer,
                    String.Format(CultureInfo.InvariantCulture, ShowMoreFormat, item.Index), 
                    moreText, 
                    null /*accessKey*/,
                    subCommandStyle,
                    subCommandClass);
                writer.WriteLine("</td>");
            }
            writer.WriteLine("</tr>");
        }

        private void RenderListViewTableHeader (XhtmlMobileTextWriter writer, int fieldCount, int[] fieldIndices, bool itemRequiresMoreButton){
            String cssClass = GetCustomAttributeValue(XhtmlConstants.CssClassCustomAttribute);
            String labelClass = GetCustomAttributeValue(XhtmlConstants.CssLabelClassCustomAttribute); 
            if (labelClass == null || labelClass.Length == 0) {
                labelClass = cssClass;
            }
            writer.WriteLine("<tr>");
            for (int field = 0; field < fieldCount; field++) {
                writer.WriteBeginTag("td");
                if (CssLocation == StyleSheetLocation.PhysicalFile && labelClass != null && labelClass.Length > 0) {
                    writer.WriteAttribute("class", labelClass, true);
                }
                writer.Write(">");
                Style labelStyle = Control.LabelStyle;
                ConditionalEnterStyle(writer, labelStyle);
                writer.WriteEncodedText(Control.AllFields[fieldIndices[field]].Title);
                ConditionalExitStyle(writer, labelStyle);
                writer.Write("</td>");
            }
            if (itemRequiresMoreButton) {
                writer.WriteLine("<td/>");
            }
            writer.WriteLine();
            writer.WriteLine("</tr>");
        }

        private void RenderRule (XhtmlMobileTextWriter writer, Color foreColor, int columnSpan) {
            if (CssLocation == StyleSheetLocation.PhysicalFile || Device["requiresXhtmlCssSuppression"] == "true") {
                // Review: Since, if there is a physical stylesheet, we cannot know the intended foreColor,
                // do not render a rule.
                return;
            }
            writer.Write("<tr>");
            Style hruleStyle = new Style();
            // Rendering <td...> with background color equal to the style's forecolor renders a thin
            // rule with color forecolor, as intended.
            hruleStyle[Style.BackColorKey] = foreColor == Color.Empty ? Color.Black : foreColor;
            NameValueCollection additionalAttributes = new NameValueCollection();
            additionalAttributes["colspan"] = columnSpan.ToString(CultureInfo.InvariantCulture);              
            writer.EnterStyleInternal(hruleStyle, "td", StyleFilter.BackgroundColor, additionalAttributes);
            writer.ExitStyle(Style);
            writer.WriteEndTag("tr");            
        }

        private bool RequiresDetailsScreen () {
            return HasItemDetails() || (Control.Commands.Count > 0 && !OnlyHasDefaultCommand());
        }

        private void SetSecondaryUIMode () {
            if (WillDetailsBeRendered ()) {
                SecondaryUIMode = _modeDetails;
            }
            else {
                SecondaryUIMode = NotSecondaryUI;
            }
        }

        // Return true (in PreRender) if the details view will be shown.  Used to set secondary UI
        // and call ObjectList.PreShowItemCommands during PreRender.
        private bool WillDetailsBeRendered () {
            return Control.MobilePage.ActiveForm == Control.Form && Control.Visible && 
                (Control.ViewMode == ObjectListViewMode.Commands || 
                Control.ViewMode == ObjectListViewMode.Details);
        }
    }
}
