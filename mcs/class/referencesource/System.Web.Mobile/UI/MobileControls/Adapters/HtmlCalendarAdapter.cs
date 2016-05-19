//------------------------------------------------------------------------------
// <copyright file="HtmlCalendarAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Web.UI.MobileControls;
using System.Security.Permissions;
using System.Globalization;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource
#else
namespace System.Web.UI.MobileControls.Adapters
#endif    

{
    /*
     * HtmlCalendarAdapter provides the html device functionality for
     * Calendar control.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\HtmlCalendarAdapter.uex' path='docs/doc[@for="HtmlCalendarAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class HtmlCalendarAdapter : HtmlControlAdapter
    {
        // Insert bgcolor="Silver" right after td tag
        private const int _bgColorInsertionPointInPattern = 4;
        // Defines a disk in which the color White is chosen instead of Silver
        private const int _bgColorDistanceTreshold = 1000;
        // Search patterns for locating cells of selected dates
        private const String _selectedDateSearchTableTag = "<table ";
        private const String _selectedDateSearchCellTag = "<td ";
        private const String _selectedDateSearchAttr = "background-color:Silver;";

        /// <include file='doc\HtmlCalendarAdapter.uex' path='docs/doc[@for="HtmlCalendarAdapter.Control"]/*' />
        protected new Calendar Control
        {
            get
            {
                return (Calendar)base.Control;
            }
        }

        private int LocateNextSelectedDate(String webCalendarHtml, int startingIndex)
        {
            int tagBeginIndex = startingIndex;
            do
            {
                tagBeginIndex = webCalendarHtml.IndexOf(_selectedDateSearchCellTag, tagBeginIndex, StringComparison.Ordinal);
                if (tagBeginIndex >= 0)
                {
                    int tagEndIndex = webCalendarHtml.IndexOf(">", tagBeginIndex + _bgColorInsertionPointInPattern, StringComparison.Ordinal);
                    Debug.Assert(tagEndIndex >= 0);
                    String tagComplete = webCalendarHtml.Substring(tagBeginIndex, tagEndIndex-tagBeginIndex+1);
                    if (tagComplete.IndexOf(_selectedDateSearchAttr, StringComparison.Ordinal) >= 0)
                    {
                        return tagBeginIndex;
                    }
                    else
                    {
                        tagBeginIndex += _bgColorInsertionPointInPattern;
                    }
                }
            }
            while (tagBeginIndex >= 0);
            return -1;
        }

        /// <include file='doc\HtmlCalendarAdapter.uex' path='docs/doc[@for="HtmlCalendarAdapter.Render"]/*' />
        public override void Render(HtmlMobileTextWriter writer)
        {
            System.Web.UI.WebControls.WebControl webCalendar = Control.WebCalendar;
            
            Style.ApplyTo(webCalendar);

            // Delegate the rendering effort to the child Web Calendar
            // control for HTML browser
            webCalendar.Visible = true;

            // There is no explicit property for alignment on WebForms
            // Calendar, so we need some special code to set it.
            writer.EnterLayout(Style);
            writer.EnsureStyle();
            Alignment align = (Alignment) Style[Style.AlignmentKey, true];
            if (!Device.SupportsDivAlign)
            {
                webCalendar.Attributes["align"] = align.ToString();
            }

            if (Device.SupportsCss)
            {
                // Target device supports CSS - simply delegate the rendering
                // to the underlying Web Calendar control
                webCalendar.RenderControl(writer);
            }
            else
            {
                // Insert bgcolor attributes in cells that correspond to selected dates
                StringWriter sw = new StringWriter(CultureInfo.InvariantCulture);
                HtmlTextWriter tmpWriter = new HtmlTextWriter(sw);
                webCalendar.RenderControl(tmpWriter);
                String webCalendarHtml = sw.ToString();
                int index = 0, indexLastTable = 0;
                // Search for offset of last <table> tag in the Web Calendar HTML.
                // That table contains the various days.
                do
                {
                    index = webCalendarHtml.IndexOf(_selectedDateSearchTableTag, index, StringComparison.Ordinal);
                    if (index >= 0)
                    {
                        indexLastTable = index;
                        index += 5;
                    }
                } 
                while (index >= 0);
                index = LocateNextSelectedDate(webCalendarHtml, indexLastTable);
                if (index >= 0)
                {
                    // Determine the background color of the containing Form control
                    HtmlControlAdapter formAdapter = (HtmlControlAdapter) Control.Form.Adapter;
                    Color backColor = (Color)formAdapter.Style[Style.BackColorKey, true];
                    int deltaR = System.Math.Abs(backColor.R - 0xC0);
                    int deltaG = System.Math.Abs(backColor.G - 0xC0);
                    int deltaB = System.Math.Abs(backColor.B - 0xC0);
                    // Determine the distance between Silver and the Form's background color
                    int bgColorDistance = deltaR * deltaR + deltaG * deltaG + deltaB * deltaB;
                    // Choose Silver or White depending on that distance
                    String selectedDateBGColor = 
                        String.Format(CultureInfo.CurrentCulture, "bgcolor=\"{0}\" ", bgColorDistance < _bgColorDistanceTreshold ? "White" : "Silver");
                    while (index >= 0)
                    {
                        // Insert the bgcolor attribute for each selected date cell
                        webCalendarHtml = webCalendarHtml.Insert(index + _bgColorInsertionPointInPattern, selectedDateBGColor);
                        index = LocateNextSelectedDate(webCalendarHtml, index + _bgColorInsertionPointInPattern);
                    }
                }
                // Use the HTML after insertions
                writer.Write(webCalendarHtml);
            }

            if(Control.BreakAfter)
            {
                writer.WriteBreak();
            }
            writer.ExitLayout(Style);
        }
    }
}
