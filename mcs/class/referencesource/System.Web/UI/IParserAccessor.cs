//------------------------------------------------------------------------------
// <copyright file="IParserAccessor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * The interface used to interact with the Parser.
 *
 * Copyright (c) 1999 Microsoft Corporation
 */
namespace System.Web.UI {

using System;

/// <devdoc>
///    <para>[To be supplied.]</para>
/// </devdoc>
public interface IParserAccessor {
    /*
     * A sub-object tag was parsed by the parser; add it to this container.
     */

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    void AddParsedSubObject(object obj);
}
}
