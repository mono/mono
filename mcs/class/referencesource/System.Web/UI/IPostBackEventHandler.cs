//------------------------------------------------------------------------------
// <copyright file="IPostBackEventHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Controls that will generate postback events from the client should implement this interface.
 *
 * Copyright (c) 1999 Microsoft Corporation
 */
namespace System.Web.UI {
using System;

/// <devdoc>
///    <para> Defines the contract that controls must implement to
///       handle low-level post back events.</para>
/// </devdoc>
public interface IPostBackEventHandler {
    /*
     * Process the event that this control wanted fired from a form post back.
     */

    /// <devdoc>
    ///    <para>
    ///       Enables a control to process the event fired by a form post back.
    ///    </para>
    /// </devdoc>
    void RaisePostBackEvent(string eventArgument);
}

}
