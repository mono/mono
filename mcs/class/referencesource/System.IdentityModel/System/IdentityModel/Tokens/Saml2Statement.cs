//-----------------------------------------------------------------------
// <copyright file="Saml2Statement.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System;

    /// <summary>
    /// Represents the StatementAbstractType specified in [Saml2Core, 2.7.1].
    /// </summary>
    /// <remarks>
    /// This abstract class provides no operations; however, this type is used
    /// to declare collections of statements, for example Saml2Assertion.Statements.
    /// </remarks>
    public abstract class Saml2Statement
    {
    }
}
