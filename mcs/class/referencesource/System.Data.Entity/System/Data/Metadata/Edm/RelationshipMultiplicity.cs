//---------------------------------------------------------------------
// <copyright file="RelationshipMultiplicity.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       jeffreed
// @backupOwner leil
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// Represents the multiplicity information about the end of a relationship type
    /// </summary>
    public enum RelationshipMultiplicity
    {
        /// <summary>
        /// Lower Bound is Zero and Upper Bound is One
        /// </summary>
        ZeroOrOne,

        /// <summary>
        /// Both lower bound and upper bound is one
        /// </summary>
        One,

        /// <summary>
        /// Lower bound is zero and upper bound is null
        /// </summary>
        Many
    }
}
