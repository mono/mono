//------------------------------------------------------------------------------
// <copyright file="ThreadExceptionEventHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.Threading {
    using System.Threading;
    using System.Diagnostics;

    using System;

    /// <devdoc>
    /// <para>Represents the method that will handle the System.Windows.Forms.Application.OnThreadException
    /// event of a Thread.OnThreadException.</para>
    /// </devdoc>
    public delegate void ThreadExceptionEventHandler(object sender, ThreadExceptionEventArgs e);
}
