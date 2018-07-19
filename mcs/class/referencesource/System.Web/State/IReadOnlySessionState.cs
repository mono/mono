//------------------------------------------------------------------------------
// <copyright file="IReadOnlySessionState.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * IReadOnlySessionState
 * 
 * Copyright (c) 1998-1999, Microsoft Corporation
 * 
 */

namespace System.Web.SessionState {

/*
 * Marker interface to indicate that class needs only read-only
 * access to session state.
 */


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public interface IReadOnlySessionState : IRequiresSessionState {
    }

}
