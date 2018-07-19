//------------------------------------------------------------------------------
// <copyright file="DataRowCreatedEventHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">amirhmy</owner>
// <owner current="true" primary="false">markash</owner>
// <owner current="false" primary="false">jasonzhu</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    internal delegate void DataRowCreatedEventHandler(object sender, DataRow r);
    internal delegate void DataSetClearEventhandler(object sender, DataTable table);
}

