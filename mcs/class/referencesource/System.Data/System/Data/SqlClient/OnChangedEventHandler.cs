//------------------------------------------------------------------------------
// <copyright file="OnChangedEventHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">ramp</owner>
// <owner current="true" primary="false">blained</owner>
//------------------------------------------------------------------------------

namespace System.Data.SqlClient {
    using System;
    using System.ComponentModel;
    using System.Collections;
    using System.Data;

    public delegate void OnChangeEventHandler(object sender, SqlNotificationEventArgs e);
}
    
