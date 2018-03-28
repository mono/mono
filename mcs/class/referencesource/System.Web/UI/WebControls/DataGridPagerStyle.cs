//------------------------------------------------------------------------------
// <copyright file="DataGridPagerStyle.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Web;
    using System.Web.UI;

    /// <devdoc>
    /// <para>Specifies the <see cref='System.Web.UI.WebControls.DataGrid'/> pager style for the control. This class cannot be inherited.</para>
    /// </devdoc>
    public sealed class DataGridPagerStyle : TableItemStyle {


        /// <devdoc>
        ///    <para>Represents the Mode property.</para>
        /// </devdoc>
        const int PROP_MODE = 0x00080000;

        /// <devdoc>
        ///    <para>Represents the Next Page Text property.</para>
        /// </devdoc>
        const int PROP_NEXTPAGETEXT = 0x00100000;

        /// <devdoc>
        ///    <para>Represents the Previous Page Text property.</para>
        /// </devdoc>
        const int PROP_PREVPAGETEXT = 0x00200000;

        /// <devdoc>
        ///    <para>Represents the Page Button Count property.</para>
        /// </devdoc>
        const int PROP_PAGEBUTTONCOUNT = 0x00400000;

        /// <devdoc>
        ///    <para>Represents the Position property.</para>
        /// </devdoc>
        const int PROP_POSITION = 0x00800000;

        /// <devdoc>
        ///    <para>Represents the Visible property.</para>
        /// </devdoc>
        const int PROP_VISIBLE = 0x01000000;

        private DataGrid owner;


        /// <devdoc>
        ///   Creates a new instance of DataGridPagerStyle.
        /// </devdoc>
        internal DataGridPagerStyle(DataGrid owner) {
            this.owner = owner;
        }



        /// <devdoc>
        /// </devdoc>
        internal bool IsPagerOnBottom {
            get {
                PagerPosition position = Position;

                return(position == PagerPosition.Bottom) ||
                (position == PagerPosition.TopAndBottom);
            }
        }


        /// <devdoc>
        /// </devdoc>
        internal bool IsPagerOnTop {
            get {
                PagerPosition position = Position;

                return(position == PagerPosition.Top) ||
                (position == PagerPosition.TopAndBottom);
            }
        }


        /// <devdoc>
        ///    Gets or sets the type of Paging UI to use.
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(PagerMode.NextPrev),
        NotifyParentProperty(true),
        WebSysDescription(SR.DataGridPagerStyle_Mode)
        ]
        public PagerMode Mode {
            get {
                if (IsSet(PROP_MODE)) {
                    return(PagerMode)(ViewState["Mode"]);
                }
                return PagerMode.NextPrev;
            }
            set {
                if (value < PagerMode.NextPrev || value > PagerMode.NumericPages) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["Mode"] = value;
                SetBit(PROP_MODE);
                owner.OnPagerChanged();
            }
        }


        /// <devdoc>
        ///    Gets or sets the text to be used for the Next page
        ///    button.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        DefaultValue("&gt;"),
        NotifyParentProperty(true),
        WebSysDescription(SR.PagerSettings_NextPageText)
        ]
        public string NextPageText {
            get {
                if (IsSet(PROP_NEXTPAGETEXT)) {
                    return(string)(ViewState["NextPageText"]);
                }
                return "&gt;";
            }
            set {
                ViewState["NextPageText"] = value;
                SetBit(PROP_NEXTPAGETEXT);
                owner.OnPagerChanged();
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the number of pages to show in the
        ///       paging UI when the mode is <see langword='PagerMode.NumericPages'/>
        ///       .</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(10),
        NotifyParentProperty(true),
        WebSysDescription(SR.DataGridPagerStyle_PageButtonCount)
        ]
        public int PageButtonCount {
            get {
                if (IsSet(PROP_PAGEBUTTONCOUNT)) {
                    return(int)(ViewState["PageButtonCount"]);
                }
                return 10;
            }
            set {
                if (value < 1) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["PageButtonCount"] = value;
                SetBit(PROP_PAGEBUTTONCOUNT);
                owner.OnPagerChanged();
            }
        }


        /// <devdoc>
        ///    <para> Gets or sets the vertical
        ///       position of the paging UI bar with
        ///       respect to its associated control.</para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(PagerPosition.Bottom),
        NotifyParentProperty(true),
        WebSysDescription(SR.DataGridPagerStyle_Position)
        ]
        public PagerPosition Position {
            get {
                if (IsSet(PROP_POSITION)) {
                    return(PagerPosition)(ViewState["Position"]);
                }
                return PagerPosition.Bottom;
            }
            set {
                if (value < PagerPosition.Bottom || value > PagerPosition.TopAndBottom) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["Position"] = value;
                SetBit(PROP_POSITION);
                owner.OnPagerChanged();
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the text to be used for the Previous
        ///       page button.</para>
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        DefaultValue("&lt;"),
        NotifyParentProperty(true),
        WebSysDescription(SR.PagerSettings_PreviousPageText)
        ]
        public string PrevPageText {
            get {
                if (IsSet(PROP_PREVPAGETEXT)) {
                    return(string)(ViewState["PrevPageText"]);
                }
                return "&lt;";
            }
            set {
                ViewState["PrevPageText"] = value;
                SetBit(PROP_PREVPAGETEXT);
                owner.OnPagerChanged();
            }
        }


        /// <devdoc>
        ///    <para> Gets or set whether the paging
        ///       UI is to be shown.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(true),
        NotifyParentProperty(true),
        WebSysDescription(SR.DataGridPagerStyle_Visible)
        ]
        public bool Visible {
            get {
                if (IsSet(PROP_VISIBLE)) {
                    return(bool)(ViewState["PagerVisible"]);
                }
                return true;
            }
            set {
                ViewState["PagerVisible"] = value;
                SetBit(PROP_VISIBLE);
                owner.OnPagerChanged();
            }
        }


        /// <devdoc>
        /// <para>Copies the data grid pager style from the specified <see cref='System.Web.UI.WebControls.Style'/>.</para>
        /// </devdoc>
        public override void CopyFrom(Style s) {

            if (s != null && !s.IsEmpty) {
                base.CopyFrom(s);

                if (s is DataGridPagerStyle) {
                    DataGridPagerStyle ps = (DataGridPagerStyle)s;

                    if (ps.IsSet(PROP_MODE))
                        this.Mode = ps.Mode;
                    if (ps.IsSet(PROP_NEXTPAGETEXT))
                        this.NextPageText = ps.NextPageText;
                    if (ps.IsSet(PROP_PREVPAGETEXT))
                        this.PrevPageText = ps.PrevPageText;
                    if (ps.IsSet(PROP_PAGEBUTTONCOUNT))
                        this.PageButtonCount = ps.PageButtonCount;
                    if (ps.IsSet(PROP_POSITION))
                        this.Position = ps.Position;
                    if (ps.IsSet(PROP_VISIBLE))
                        this.Visible = ps.Visible;

                }
            }
        }


        /// <devdoc>
        /// <para>Merges the data grid pager style from the specified <see cref='System.Web.UI.WebControls.Style'/>.</para>
        /// </devdoc>
        public override void MergeWith(Style s) {
            if (s != null && !s.IsEmpty) {

                if (IsEmpty) {
                    // merge into an empty style is equivalent to a copy, which
                    // is more efficient
                    CopyFrom(s);
                    return;
                }

                base.MergeWith(s);

                if (s is DataGridPagerStyle) {
                    DataGridPagerStyle ps = (DataGridPagerStyle)s;

                    if (ps.IsSet(PROP_MODE) && !this.IsSet(PROP_MODE))
                        this.Mode = ps.Mode;
                    if (ps.IsSet(PROP_NEXTPAGETEXT) && !this.IsSet(PROP_NEXTPAGETEXT))
                        this.NextPageText = ps.NextPageText;
                    if (ps.IsSet(PROP_PREVPAGETEXT) && !this.IsSet(PROP_PREVPAGETEXT))
                        this.PrevPageText = ps.PrevPageText;
                    if (ps.IsSet(PROP_PAGEBUTTONCOUNT) && !this.IsSet(PROP_PAGEBUTTONCOUNT))
                        this.PageButtonCount = ps.PageButtonCount;
                    if (ps.IsSet(PROP_POSITION) && !this.IsSet(PROP_POSITION))
                        this.Position = ps.Position;
                    if (ps.IsSet(PROP_VISIBLE) && !this.IsSet(PROP_VISIBLE))
                        this.Visible = ps.Visible;

                }
            }
        }


        /// <devdoc>
        ///    <para>Restores the data grip pager style to the default values.</para>
        /// </devdoc>
        public override void Reset() {
            if (IsSet(PROP_MODE))
                ViewState.Remove("Mode");
            if (IsSet(PROP_NEXTPAGETEXT))
                ViewState.Remove("NextPageText");
            if (IsSet(PROP_PREVPAGETEXT))
                ViewState.Remove("PrevPageText");
            if (IsSet(PROP_PAGEBUTTONCOUNT))
                ViewState.Remove("PageButtonCount");
            if (IsSet(PROP_POSITION))
                ViewState.Remove("Position");
            if (IsSet(PROP_VISIBLE))
                ViewState.Remove("PagerVisible");

            base.Reset();
        }

    }
}

