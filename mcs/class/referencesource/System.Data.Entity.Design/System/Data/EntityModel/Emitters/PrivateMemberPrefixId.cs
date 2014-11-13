//---------------------------------------------------------------------
// <copyright file="PrivateMemberPrefixId.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Specialized;
using System.Collections.Generic;




namespace System.Data.EntityModel.Emitters
{
    internal enum PrivateMemberPrefixId
    {
        Field,
        IntializeMethod,
        PropertyInfoProperty,
        PropertyInfoField,
        // add additional members here
        Count 
    }  
}
