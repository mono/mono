//---------------------------------------------------------------------
// <copyright file="DesignXmlConstants.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       jeffreed
// @backupOwner srimand
//---------------------------------------------------------------------
namespace System.Data.Entity.Design
{
    internal static class DesignXmlConstants
    {
        internal const string EntityStoreSchemaGeneratorNamespace = "http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator";
        internal const string EdmAnnotationNamespace = "http://schemas.microsoft.com/ado/2009/02/edm/annotation";
        
        // attributes
        internal const string EntityStoreSchemaGeneratorTypeAttributeName = "Type";
        internal const string EntityStoreSchemaGeneratorSchemaAttributeName = "Schema";
        internal const string EntityStoreSchemaGeneratorNameAttributeName = "Name";
        
        // attribute values
        internal const string TypeValueTables = "Tables";
        internal const string TypeValueViews = "Views";
        internal const string StoreGeneratedPattern = "StoreGeneratedPattern";
        internal const string LazyLoadingEnabled = "LazyLoadingEnabled";
        internal const string AnnotationPrefix = "annotation";
    }
}
