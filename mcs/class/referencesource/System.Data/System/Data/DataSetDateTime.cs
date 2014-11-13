//------------------------------------------------------------------------------
// <copyright file="DataSetDateTime.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------
namespace System.Data {
    using System;

    /// <devdoc>
    /// <para>Gets the DateTimeMode of a DateTime <see cref='System.Data.DataColumn'/> object.</para>
    /// </devdoc>
    public enum DataSetDateTime {
        /// <devdoc>
        ///    <para>The datetime column in Local DateTimeMode stores datetime in Local. Adjusts Utc/Unspecifed to Local. Serializes as Local</para>
        /// </devdoc>
        Local  = 1,
        /// <devdoc>
        /// <para>The datetime column in Unspecified DateTimeMode stores datetime in Unspecified. Adjusts Local/Utc to Unspecified. Serializes as Unspecified with no offset across timezones</para>
        /// </devdoc>
        Unspecified = 2,        
        /// <devdoc>
        /// <para>This is the default. The datetime column in UnspecifiedLocal DateTimeMode stores datetime in Unspecfied. Adjusts Local/Utc to Unspecified. Serializes as Unspecified but applying offset across timezones</para>
        /// </devdoc>
        UnspecifiedLocal = 3, //Unspecified while storing and Local when serializing. -> DataSetDateTime.Unspecified | DataSetDateTime.Local 
        /// <devdoc>
        ///    <para>
        ///       <para>The datetime column in Utc DateTimeMode  stores datetime in Utc. Adjusts Local/Unspecified to Utc. Serializes as Utc</para>
        ///    </para>
        /// </devdoc>
        Utc = 4,        
    }
}
