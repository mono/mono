//---------------------------------------------------------------------
// <copyright file="ParameterMode.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// The enumeration defining the mode of a parameter
    /// </summary>
    public enum ParameterMode
    {
        /// <summary>
        /// In parameter
        /// </summary>
        In = 0,
        /// <summary>
        /// Out parameter
        /// </summary>
        Out,
        /// <summary>
        /// Both in and out parameter
        /// </summary>
        InOut,
        /// <summary>
        /// Return Parameter
        /// </summary>
        ReturnValue
    }
}
