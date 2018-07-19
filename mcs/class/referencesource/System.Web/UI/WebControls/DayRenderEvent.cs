//------------------------------------------------------------------------------
// <copyright file="DayRenderEvent.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    /// <devdoc>
    /// <para>Provides data for the <see langword='DayRender'/> event of a <see cref='System.Web.UI.WebControls.Calendar'/>.
    /// </para>
    /// </devdoc>
    public sealed class DayRenderEventArgs {
        CalendarDay day;
        TableCell cell;
        string selectUrl;


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.DayRenderEventArgs'/> class.</para>
        /// </devdoc>
        public DayRenderEventArgs(TableCell cell, CalendarDay day) {
            this.day = day;
            this.cell = cell;
        }


        public DayRenderEventArgs(TableCell cell, CalendarDay day, string selectUrl) {
            this.day = day;
            this.cell = cell;
            this.selectUrl = selectUrl;
        }


        /// <devdoc>
        ///    <para>Gets the cell that contains the day. This property is read-only.</para>
        /// </devdoc>
        public TableCell Cell {
            get {
                return cell;
            }
        } 


        /// <devdoc>
        ///    <para>Gets the day to render. This property is read-only.</para>
        /// </devdoc>
        public CalendarDay Day {
            get {
                return day;
            }
        }


        public string SelectUrl {
            get {
                return selectUrl;
            }
        }
    }
}
