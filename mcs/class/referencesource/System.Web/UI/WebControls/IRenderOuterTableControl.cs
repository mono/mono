//------------------------------------------------------------------------------
// <copyright file="IRenderOuterTableControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------
namespace System.Web.UI {

    /// <devdoc>
    ///    <para>Represents a control that has RenderOuterTable and ID properties.</para>
    /// </devdoc>
    internal interface IRenderOuterTableControl {

        string ID { get; }
        bool RenderOuterTable { get; set; }

    }
}
