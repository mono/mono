//-----------------------------------------------------------------------
// <copyright file="ComputedKeyAlgorithms.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------


using System;

namespace System.IdentityModel.Tokens
{
    /// <summary>
    /// Used in the RST to indicated the desired algorithm used to compute key based on 
    /// the combined entropies from both token requestor and token issuer.
    /// </summary>
    public static class ComputedKeyAlgorithms
    {
#pragma warning disable 1591
        public const string Psha1 = "http://schemas.microsoft.com/idfx/computedkeyalgorithm/psha1";
#pragma warning restore 1591
    }
}
