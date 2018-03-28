//---------------------------------------------------------------------
// <copyright file="AdditionalEntityFunctions.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
namespace System.Data.Objects
{
    public static partial class EntityFunctions
    {
        /// <summary>
        /// An ELINQ operator that ensures the input string is treated as a unicode string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string AsUnicode(string value)
        {
            return value;
        }

        /// <summary>
        /// An ELINQ operator that treats the input string as a non-unicode string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string AsNonUnicode(string value)
        {
            return value;
        }
    }
}
