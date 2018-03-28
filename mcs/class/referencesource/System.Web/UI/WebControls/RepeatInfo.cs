//------------------------------------------------------------------------------
// <copyright file="RepeatInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    /// <devdoc>
    ///    <para>Defines the information used to render a list of items using
    ///       a <see cref='System.Web.UI.WebControls.Repeater'/>.</para>
    /// </devdoc>
    public sealed class RepeatInfo {

        private RepeatDirection repeatDirection;
        private RepeatLayout repeatLayout;
        private int repeatColumns;
        private string caption;
        private TableCaptionAlign captionAlign;
        private bool useAccessibleHeader;
        private bool outerTableImplied;
        private bool enableLegacyRendering;


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.RepeatInfo'/> class. This class is not 
        ///    inheritable.</para>
        /// </devdoc>
        public RepeatInfo() {
            repeatDirection = RepeatDirection.Vertical;
            repeatLayout = RepeatLayout.Table;
            repeatColumns = 0;
            outerTableImplied = false;
        }


        public string Caption {
            get {
                return (caption == null) ? String.Empty : caption;
            }
            set {
                caption = value;
            }
        }


        public TableCaptionAlign CaptionAlign {
            get {
                return captionAlign;
            }
            set {
                if ((value < TableCaptionAlign.NotSet) ||
                    (value > TableCaptionAlign.Right)) {
                    throw new ArgumentOutOfRangeException("value");
                }
                captionAlign = value;
            }
        }

        // DevDiv 33149: A backward compat. switch for Everett rendering
        internal bool EnableLegacyRendering {
            get {
                return enableLegacyRendering;
            }
            set {
                enableLegacyRendering = value;
            }
        }

        private bool IsListLayout {
            get {
                return
                    (RepeatLayout == RepeatLayout.UnorderedList) ||
                    (RepeatLayout == RepeatLayout.OrderedList);
            }
        }

        /// <devdoc>
        ///    Indicates whether an outer table is implied
        ///    for the items.
        /// </devdoc>
        public bool OuterTableImplied {
            get {
                return outerTableImplied;
            }
            set {
                outerTableImplied = value;
            }
        }


        /// <devdoc>
        ///    <para> Indicates the column count of items.</para>
        /// </devdoc>
        public int RepeatColumns {
            get {
                return repeatColumns;
            }
            set {
                repeatColumns = value;
            }
        }


        /// <devdoc>
        ///    <para>Indicates the direction of flow of items.</para>
        /// </devdoc>
        public RepeatDirection RepeatDirection {
            get {
                return repeatDirection;
            }
            set {
                if (value < RepeatDirection.Horizontal || value > RepeatDirection.Vertical) {
                    throw new ArgumentOutOfRangeException("value");
                }
                repeatDirection = value;
            }
        }


        /// <devdoc>
        ///    Indicates the layout of items.
        /// </devdoc>
        public RepeatLayout RepeatLayout {
            get {
                return repeatLayout;
            }
            set {
                EnumerationRangeValidationUtil.ValidateRepeatLayout(value);
                repeatLayout = value;
            }
        }


        public bool UseAccessibleHeader {
            get {
                return useAccessibleHeader;
            }
            set {
                useAccessibleHeader = value;
            }
        }


        /// <devdoc>
        /// </devdoc>
        private void RenderHorizontalRepeater(HtmlTextWriter writer, IRepeatInfoUser user, Style controlStyle, WebControl baseControl) {
            Debug.Assert(outerTableImplied == false, "Cannot use outer implied table with Horizontal layout");

            int itemCount = user.RepeatedItemCount;

            int totalColumns = repeatColumns;
            int currentColumn = 0;

            if (totalColumns == 0) {
                // 0 implies a complete horizontal repetition without any
                // column count constraints
                totalColumns = itemCount;
            }

            WebControl outerControl = null;
            bool tableLayout = false;

            switch (repeatLayout) {
                case RepeatLayout.Table:
                    outerControl = new Table();
                    if (Caption.Length != 0) {
                        ((Table)outerControl).Caption = Caption;
                        ((Table)outerControl).CaptionAlign = CaptionAlign;
                    }
                    tableLayout = true;
                    break;
                case RepeatLayout.Flow:
                    outerControl = new WebControl(HtmlTextWriterTag.Span);
                    break;
            }

            bool separators = user.HasSeparators;

            // use ClientID (and not ID) since we want to render out the fully qualified client id
            // even though this outer control will not be parented to the control hierarchy
            outerControl.ID = baseControl.ClientID;

            outerControl.CopyBaseAttributes(baseControl);
            outerControl.ApplyStyle(controlStyle);
            outerControl.RenderBeginTag(writer);

            if (user.HasHeader) {
                if (tableLayout) {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                    // add attributes to render for TD/TH
                    if ((totalColumns != 1) || separators) {
                        int columnSpan = totalColumns;
                        if (separators)
                            columnSpan += totalColumns;
                        writer.AddAttribute(HtmlTextWriterAttribute.Colspan, columnSpan.ToString(NumberFormatInfo.InvariantInfo));
                    }
                    if (useAccessibleHeader) {
                        writer.AddAttribute(HtmlTextWriterAttribute.Scope, "col");
                    }

                    // add style attributes to render for TD/TH
                    Style style = user.GetItemStyle(ListItemType.Header, -1);
                    if (style != null) {
                        style.AddAttributesToRender(writer);
                    }

                    // render begin tag
                    if (useAccessibleHeader) {
                        writer.RenderBeginTag(HtmlTextWriterTag.Th);
                    }
                    else {
                        writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    }
                }
                user.RenderItem(ListItemType.Header, -1, this, writer);
                if (tableLayout) {
                    // render end tags TD/TH and TR
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                }
                else {
                    if (totalColumns < itemCount) {
                        // we have multiple rows, so have a break between the header and first row.
                        if (EnableLegacyRendering) {
                            writer.WriteObsoleteBreak();
                        }
                        else {
                            writer.WriteBreak();
                        }
                    }
                }
            }

            for (int i = 0; i < itemCount; i++) {
                if (tableLayout && (currentColumn == 0)) {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                }

                if (tableLayout) {
                    // add style attributes to render for TD
                    Style style = user.GetItemStyle(ListItemType.Item, i);
                    if (style != null)
                        style.AddAttributesToRender(writer);
                    // render begin tag for TD
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                }
                user.RenderItem(ListItemType.Item, i, this, writer);
                if (tableLayout) {
                    // render end tag for TD
                    writer.RenderEndTag();
                }
                if (separators && (i != (itemCount - 1))) {
                    if (tableLayout) {
                        Style style = user.GetItemStyle(ListItemType.Separator, i);
                        if (style != null)
                            style.AddAttributesToRender(writer);
                        writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    }
                    user.RenderItem(ListItemType.Separator, i, this, writer);
                    if (tableLayout)
                        writer.RenderEndTag();
                }

                currentColumn++;
                
                // on the last line, fill in the rest of the empty spots with <td/>s.
                // If there were separators, we need twice as many plus one to accomodate for
                // the last item not having a separator after it.
                if (tableLayout && i == itemCount - 1) {
                    int unfilledColumns = totalColumns - currentColumn;
                    if (separators == true) {
                        int unfilledColumnsWithSeparators = (unfilledColumns * 2) + 1;
                        if (unfilledColumnsWithSeparators > unfilledColumns) {
                            unfilledColumns = unfilledColumnsWithSeparators;
                        }
                    }
                    for (int k = 0; k < unfilledColumns; k++) {
                        writer.RenderBeginTag(HtmlTextWriterTag.Td);
                        writer.RenderEndTag();
                    }
                }
                
                if ((currentColumn == totalColumns) || (i == itemCount - 1)) {
                    if (tableLayout) {
                        // End tag for TR
                        writer.RenderEndTag();
                    }
                    else {
                        // write out the <br> after rows when there are multiple rows
                        if (totalColumns < itemCount) {
                            if (EnableLegacyRendering) {
                                writer.WriteObsoleteBreak();
                            }
                            else {
                                writer.WriteBreak();
                            }
                        }
                    }

                    currentColumn = 0;
                }
            }

            if (user.HasFooter) {
                if (tableLayout) {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                    if ((totalColumns != 1) || separators) {
                        // add attributes to render for TD
                        int columnSpan = totalColumns;
                        if (separators)
                            columnSpan += totalColumns;
                        writer.AddAttribute(HtmlTextWriterAttribute.Colspan, columnSpan.ToString(NumberFormatInfo.InvariantInfo));
                    }
                    // add style attributes to render for TD
                    Style style = user.GetItemStyle(ListItemType.Footer, -1);
                    if (style != null)
                        style.AddAttributesToRender(writer);
                    // render begin tag for TD
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                }
                user.RenderItem(ListItemType.Footer, -1, this, writer);
                if (tableLayout) {
                    // render end tag for TR and TD
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                }
            }

            outerControl.RenderEndTag(writer);
        }


        /// <devdoc>
        ///    <para>Renders the Repeater with the specified
        ///       information.</para>
        /// </devdoc>
        public void RenderRepeater(HtmlTextWriter writer, IRepeatInfoUser user, Style controlStyle, WebControl baseControl) {
            if (IsListLayout) {
                if (user.HasFooter || user.HasHeader || user.HasSeparators) {
                    throw new InvalidOperationException(SR.GetString(SR.RepeatInfo_ListLayoutDoesNotSupportHeaderFooterSeparator));
                }
                if (RepeatDirection != RepeatDirection.Vertical) {
                    throw new InvalidOperationException(SR.GetString(SR.RepeatInfo_ListLayoutOnlySupportsVerticalLayout));
                }
                if ((RepeatColumns != 0) && (RepeatColumns != 1)) {
                    throw new InvalidOperationException(SR.GetString(SR.RepeatInfo_ListLayoutDoesNotSupportMultipleColumn));
                }
                if (OuterTableImplied) {
                    throw new InvalidOperationException(SR.GetString(SR.RepeatInfo_ListLayoutDoesNotSupportImpliedOuterTable));
                }
            }

            if (repeatDirection == RepeatDirection.Vertical) {
                RenderVerticalRepeater(writer, user, controlStyle, baseControl);
            }
            else {
                RenderHorizontalRepeater(writer, user, controlStyle, baseControl);
            }
        }

        /// <devdoc>
        /// </devdoc>
        private void RenderVerticalRepeater(HtmlTextWriter writer, IRepeatInfoUser user, Style controlStyle, WebControl baseControl) {
            int itemCount = user.RepeatedItemCount;
            int totalColumns;
            int totalRows;
            int filledColumns;

            // List Layout Constraint --> Columns = 0 or 1
            if ((repeatColumns == 0) || (repeatColumns == 1)) {
                // A RepeatColumns of 0 implies a completely vertical repetition in
                // a single column. This is same as repeatColumns of 1.
                totalColumns = 1;
                filledColumns = 1;
                totalRows = itemCount;
            }
            else {
                totalColumns = repeatColumns;
                totalRows = (int)((itemCount + repeatColumns - 1) / repeatColumns);

                if ((totalRows == 0) && (itemCount != 0)) {
                    // if repeatColumns is a huge number like Int32.MaxValue, then the
                    // calculation above essentially reduces down to 0
                    totalRows = 1;
                }

                filledColumns = itemCount % totalColumns;
                if (filledColumns == 0) {
                    filledColumns = totalColumns;
                }
            }


            WebControl outerControl = null;
            bool tableLayout = false;

            // List Layout Constraint --> OuterTableImplied = false
            // List Layout Constraint --> tableLayout = false
            if (!outerTableImplied) {
                switch (repeatLayout) {
                    case RepeatLayout.Table:
                        outerControl = new Table();
                        if (Caption.Length != 0) {
                            ((Table)outerControl).Caption = Caption;
                            ((Table)outerControl).CaptionAlign = CaptionAlign;
                        }
                        tableLayout = true;
                        break;
                    case RepeatLayout.Flow:
                        outerControl = new WebControl(HtmlTextWriterTag.Span);
                        break;
                    case RepeatLayout.UnorderedList:
                        outerControl = new WebControl(HtmlTextWriterTag.Ul);
                        break;
                    case RepeatLayout.OrderedList:
                        outerControl = new WebControl(HtmlTextWriterTag.Ol);
                        break;
                }
            }

            bool separators = user.HasSeparators;
            // List Layout Constraint --> separators = false

            if (outerControl != null) {
                // use ClientID (and not ID) since we want to render out the fully qualified client id
                // even though this outer control will not be parented to the control hierarchy
                outerControl.ID = baseControl.ClientID;

                outerControl.CopyBaseAttributes(baseControl);
                outerControl.ApplyStyle(controlStyle);
                outerControl.RenderBeginTag(writer);
            }

            // List Layout Constraint --> HasHeader = false
            if (user.HasHeader) {
                if (tableLayout) {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                    // add attributes to render for TH
                    if (totalColumns != 1) {
                        int columnSpan = totalColumns;
                        if (separators)
                            columnSpan += totalColumns;
                        writer.AddAttribute(HtmlTextWriterAttribute.Colspan, columnSpan.ToString(NumberFormatInfo.InvariantInfo));
                    }
                    if (useAccessibleHeader) {
                        writer.AddAttribute(HtmlTextWriterAttribute.Scope, "col");
                    }

                    // add style attributes to render for TD/TH
                    Style style = user.GetItemStyle(ListItemType.Header, -1);
                    if (style != null) {
                        style.AddAttributesToRender(writer);
                    }

                    // render begin tag for TD/TH
                    if (useAccessibleHeader) {
                        writer.RenderBeginTag(HtmlTextWriterTag.Th);
                    }
                    else {
                        writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    }
                }
                user.RenderItem(ListItemType.Header, -1, this, writer);
                if (tableLayout) {
                    // render end tags TD/TH and TR
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                }
                else if (!outerTableImplied) {
                    if (EnableLegacyRendering) {
                        writer.WriteObsoleteBreak();
                    }
                    else {
                        writer.WriteBreak();
                    }
                }
            }

            int itemCounter = 0;

            for (int currentRow = 0; currentRow < totalRows; currentRow++) {
                // List Layout Constraint --> tableLayout = false
                if (tableLayout) {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                }

                int itemIndex = currentRow;

                for (int currentCol = 0; currentCol < totalColumns; currentCol++) {
                    if (itemCounter >= itemCount) {
                        // done rendering all items, so break out of the loop now...
                        // we might end up here, in unfilled columns attempting to re-render items that
                        // have already been rendered on the next column in a prior row.
                        break;
                    }
                     
                    if (currentCol != 0) {
                        itemIndex += totalRows;
 
                        // if the previous column (currentColumn - 1) was not a filled column, i.e.,
                        // it had one less item (the maximum possible), then subtract 1 from the item index.
                        if ((currentCol - 1) >= filledColumns) {
                            itemIndex--;
                        }
                    }


                    if (itemIndex >= itemCount)
                        continue;

                    itemCounter++;

                    // List Layout Constraint --> tableLayout = false
                    if (tableLayout) {
                        // add style attributes to render for TD
                        Style style = user.GetItemStyle(ListItemType.Item, itemIndex);
                        if (style != null)
                            style.AddAttributesToRender(writer);
                        // render begin tag for TD
                        writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    }

                    if (IsListLayout) {
                        writer.RenderBeginTag(HtmlTextWriterTag.Li);
                    }

                    user.RenderItem(ListItemType.Item, itemIndex, this, writer);

                    if (IsListLayout) {
                        writer.RenderEndTag();
                        writer.WriteLine();
                    }

                    // List Layout Constraint --> tableLayout = false
                    if (tableLayout) {
                        // render end tag for TD
                        writer.RenderEndTag();
                    }

                    // List Layout Constraint --> separators = false
                    if (separators) {
                        if (itemIndex != (itemCount - 1)) {
                            if (totalColumns == 1) {
                                if (tableLayout) {
                                    writer.RenderEndTag();
                                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                                }
                                else {
                                    if (!outerTableImplied) {
                                        if (EnableLegacyRendering) {
                                            writer.WriteObsoleteBreak();
                                        }
                                        else {
                                            writer.WriteBreak();
                                        }
                                    }
                                }
                            }
    
                            if (tableLayout) {
                                Style style = user.GetItemStyle(ListItemType.Separator, itemIndex);
                                if (style != null)
                                    style.AddAttributesToRender(writer);
                                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                            }
                            if (itemIndex < itemCount)
                                user.RenderItem(ListItemType.Separator, itemIndex, this, writer);
                            if (tableLayout)
                                writer.RenderEndTag();
                        }
                        else {
                            // if we're on the last filled line and separators are specified, add another <td/>
                            // to accomodate for the lack of a separator on the last item.  If there's only one
                            // column, though, separators will get their own row anyways.
                            if (tableLayout && totalColumns > 1) {
                                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                                writer.RenderEndTag();
                            }
                        }
                    }
                }
             
                // on the last line, fill in the remaining empty slots with <td/>s.  We need twice as many
                // if there were separators.
                // List Layout Constraint --> tableLayout = false
                if (tableLayout) {
                    if (currentRow == totalRows - 1) {
                        int unfilledColumns = totalColumns - filledColumns;
                        if (separators) {
                            int unfilledColumnsWithSeparators = unfilledColumns * 2;
                            if (unfilledColumnsWithSeparators >= unfilledColumns) {
                                unfilledColumns = unfilledColumnsWithSeparators;
                            }
                        }
                        if (unfilledColumns != 0) {
                            for (int i = 0; i < unfilledColumns; i++) {
                                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                                writer.RenderEndTag();
                            }
                        }
                    }
                    writer.RenderEndTag();
                }
                else {
                    if (((currentRow != totalRows - 1) || user.HasFooter) &&
                        (!outerTableImplied) && (!IsListLayout)) {

                        if (EnableLegacyRendering) {
                            writer.WriteObsoleteBreak();
                        }
                        else {
                            writer.WriteBreak();
                        }
                    }
                }
            }

            // List Layout Constraint --> HasFooter = false
            if (user.HasFooter) {
                if (tableLayout) {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    // add attributes to render for TD
                    if (totalColumns != 1) {
                        int columnSpan = totalColumns;
                        if (separators)
                            columnSpan += totalColumns;
                        writer.AddAttribute(HtmlTextWriterAttribute.Colspan, columnSpan.ToString(NumberFormatInfo.InvariantInfo));
                    }
                    // add style attributes to render for TD
                    Style style = user.GetItemStyle(ListItemType.Footer, -1);
                    if (style != null)
                        style.AddAttributesToRender(writer);
                    // render begin tag for TD
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                }
                user.RenderItem(ListItemType.Footer, -1, this, writer);
                if (tableLayout) {
                    // render end tag for TR and TD
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                }
            }

            if (outerControl != null)
                outerControl.RenderEndTag(writer);
        }
    }
}

