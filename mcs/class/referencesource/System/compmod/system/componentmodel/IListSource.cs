//------------------------------------------------------------------------------
// <copyright file="IListSource.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {

    using System;
    using Microsoft.Win32;
    using System.Collections;

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [
    TypeConverterAttribute("System.Windows.Forms.Design.DataSourceConverter, " + AssemblyRef.SystemDesign),
    Editor("System.Windows.Forms.Design.DataSourceListEditor, " + AssemblyRef.SystemDesign, "System.Drawing.Design.UITypeEditor, " + AssemblyRef.SystemDrawing),
    MergableProperty(false)
    ]
    public interface IListSource {

        bool ContainsListCollection { get; }

        IList GetList();
    }
}
