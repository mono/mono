//------------------------------------------------------------------------------
// <copyright file="RepeatLayout.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------
namespace System.Web.UI.WebControls {

    using System;
    
    // Specifies the layout of items of a list-bound control.
    public enum RepeatLayout {
        Table = 0, // The items are displayed using a tabular layout.
        Flow = 1,  // The items are displayed using a flow layout.
        UnorderedList = 2,
        OrderedList = 3,
    }
}
