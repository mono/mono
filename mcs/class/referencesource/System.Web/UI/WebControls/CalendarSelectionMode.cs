//------------------------------------------------------------------------------
// <copyright file="CalendarSelectionMode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    

    /// <devdoc>
    /// <para>Specifies the selection method for dates on the System.Web.UI.WebControls.Calender.</para>
    /// </devdoc>
    public enum CalendarSelectionMode {

        /// <devdoc>
        ///    <para>
        ///       No
        ///       dates can be selectioned.
        ///    </para>
        /// </devdoc>
        None = 0,

        /// <devdoc>
        ///    <para>
        ///       Dates
        ///       selected by individual days.
        ///    </para>
        /// </devdoc>
        Day = 1,

        /// <devdoc>
        ///    <para>
        ///       Dates
        ///       selected by individual
        ///       days or entire weeks.
        ///    </para>
        /// </devdoc>
        DayWeek = 2,

        /// <devdoc>
        ///    <para>
        ///       Dates
        ///       selected by individual days, entire
        ///       weeks, or entire months.
        ///    </para>
        /// </devdoc>
        DayWeekMonth = 3
    }

}
