//------------------------------------------------------------------------------
// <copyright file="AggregateType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">amirhmy</owner>
// <owner current="true" primary="false">markash</owner>
// <owner current="false" primary="false">jasonzhu</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    /// <devdoc>
    ///    <para>Specifies the aggregate function type.</para>
    /// </devdoc>

    internal enum AggregateType { 
        /// <devdoc>
        ///    <para>None.</para>
        /// </devdoc>
        None = 0,
        /// <devdoc>
        ///    <para>Sum.</para>
        /// </devdoc>
        Sum = 4,
        /// <devdoc>
        ///    <para>Average value of the aggregate set.</para>
        /// </devdoc>
        Mean = 5,
        /// <devdoc>
        ///    <para>The minimum value of the set.</para>
        /// </devdoc>
        Min = 6,
        /// <devdoc>
        ///    <para>The maximum value of the set.</para>
        /// </devdoc>
        Max = 7,
        /// <devdoc>
        ///    <para>First element of the set.</para>
        /// </devdoc>
        First = 8,
        /// <devdoc>
        ///    <para>The count of the set.</para>
        /// </devdoc>
        Count = 9,
        /// <devdoc>
        ///    <para>Variance.</para>
        /// </devdoc>
        Var = 10,
        /// <devdoc>
        ///    <para>Standard deviation.</para>
        /// </devdoc>
        StDev = 11
    }
}
