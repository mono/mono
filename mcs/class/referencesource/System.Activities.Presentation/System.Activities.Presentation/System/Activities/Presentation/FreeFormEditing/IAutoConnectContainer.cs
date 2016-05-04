//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.FreeFormEditing
{
    using System.Windows;

    /// <summary>
    /// Defines the interface for the container that enables auto-connect
    /// </summary>
    internal interface IAutoConnectContainer
    {
        void DoAutoConnect(DragEventArgs e, UIElement targetElement, AutoConnectDirections direction);

        AutoConnectDirections GetDirectionsAllowed(DragEventArgs e, UIElement targetElement);
    }
}
