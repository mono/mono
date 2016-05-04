//------------------------------------------------------------------------------
// <copyright file="RenamedEventHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------
namespace System.IO {

    using System.Diagnostics;
    using System;


    /// <devdoc>
    /// <para>Represents the method that will handle the <see cref='System.IO.FileSystemWatcher.Renamed'/> event of a <see cref='System.IO.FileSystemWatcher'/>
    /// class.</para>
    /// </devdoc>
    public delegate void RenamedEventHandler(object sender, RenamedEventArgs e);

}
