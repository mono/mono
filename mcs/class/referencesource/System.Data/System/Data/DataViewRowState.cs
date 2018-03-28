//------------------------------------------------------------------------------
// <copyright file="DataViewRowState.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">amirhmy</owner>
// <owner current="true" primary="false">markash</owner>
// <owner current="false" primary="false">jasonzhu</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.ComponentModel;

    /// <devdoc>
    /// <para>Describes the version of data in a <see cref='System.Data.DataRow'/>.</para>
    /// </devdoc>
    [
    Flags,
    Editor("Microsoft.VSDesigner.Data.Design.DataViewRowStateEditor, " + AssemblyRef.MicrosoftVSDesigner, "System.Drawing.Design.UITypeEditor, " + AssemblyRef.SystemDrawing)
    ]
    public enum DataViewRowState {
        None = 0x00000000,
        // DataRowState.Detached = 0x01,
        Unchanged = DataRowState.Unchanged,
        Added = DataRowState.Added,
        Deleted   = DataRowState.Deleted,
        ModifiedCurrent  = DataRowState.Modified,
        ModifiedOriginal  = (((int)ModifiedCurrent) << 1),
        OriginalRows = Unchanged | Deleted | ModifiedOriginal,
        CurrentRows  = Unchanged | Added | ModifiedCurrent
    }
}
