//------------------------------------------------------------------------------
// <copyright file="ErrorEventHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.IO {

    using System.Diagnostics;
    using System;
    using System.ComponentModel;

    /// <devdoc>
    ///    <para>Represents the method that will
    ///       handle the <see cref='E:System.IO.FileSystemWatcher.Error'/>
    ///       event of
    ///       a <see cref='T:System.IO.FileSystemWatcher'/>.</para>
    /// </devdoc>
    public delegate void ErrorEventHandler(object sender, ErrorEventArgs e);
}
