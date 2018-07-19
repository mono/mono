//---------------------------------------------------------------------
// <copyright file="EntityStoreSchemaGenerator.EntityCreationStatus.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       jeffreed
// @backupOwner srimand
//---------------------------------------------------------------------
namespace System.Data.Entity.Design
{
    public sealed partial class EntityStoreSchemaGenerator
    {
        private enum EntityCreationStatus
        {
            Normal,
            ReadOnly,
            Invalid
        }
    }
}
