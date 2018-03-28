//------------------------------------------------------------------------------
// <copyright file="MonthChangedEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    /// <devdoc>
    ///    <para>Provides data for the 
    ///    <see langword='VisibleMonthChanged'/> event.</para>
    /// </devdoc>
    public class MonthChangedEventArgs {
        DateTime newDate, previousDate;


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.MonthChangedEventArgs'/> class.</para>
        /// </devdoc>
        public MonthChangedEventArgs(DateTime newDate, DateTime previousDate) {
            this.newDate = newDate;
            this.previousDate = previousDate;
        }


        /// <devdoc>
        ///    <para> Gets the date that determines the month currently 
        ///       displayed by the <see cref='System.Web.UI.WebControls.Calendar'/> .</para>
        /// </devdoc>
        public DateTime NewDate {
            get {
                return newDate;
            }
        } 


        /// <devdoc>
        ///    <para> Gets the date that determines the month previously displayed 
        ///       by the <see cref='System.Web.UI.WebControls.Calendar'/>.</para>
        /// </devdoc>
        public DateTime PreviousDate {
            get {
                return previousDate;
            }
        } 
    }
}
