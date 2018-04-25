//-----------------------------------------------------------------------
// <copyright file="KeyTypes.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Protocols.WSTrust
{   
    /// <summary>
    /// Used in the RST or RSTR to indicated the desired or required key type. 
    /// </summary>
    public static class KeyTypes
    {
#pragma warning disable 1591
        public const string Symmetric = "http://schemas.microsoft.com/idfx/keytype/symmetric";
        public const string Asymmetric = "http://schemas.microsoft.com/idfx/keytype/asymmetric";
        public const string Bearer = "http://schemas.microsoft.com/idfx/keytype/bearer";
#pragma warning restore 1591
    }
}
