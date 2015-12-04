//---------------------------------------------------------------------
// <copyright file="EntityViewGenerationConstants.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace System.Data.Entity.Design
{
    internal static class EntityViewGenerationConstants
    {
        internal static readonly string NamespaceName = "Edm_EntityMappingGeneratedViews";
        internal static readonly string ViewGenerationCustomAttributeName = "System.Data.Mapping.EntityViewGenerationAttribute";
        internal static readonly string ViewGenerationTypeNamePrefix = "ViewsForBaseEntitySets";
        internal static readonly string BaseTypeName = "System.Data.Mapping.EntityViewContainer";
        internal static readonly string EdmEntityContainerName = "EdmEntityContainerName";
        internal static readonly string StoreEntityContainerName = "StoreEntityContainerName";
        internal static readonly string HashOverMappingClosure = "HashOverMappingClosure";
        internal static readonly string HashOverAllExtentViews = "HashOverAllExtentViews";
        internal static readonly string ViewCountPropertyName = "ViewCount";
        internal static readonly string GetViewAtMethodName = "GetViewAt";
        internal static readonly string SummaryStartElement = @"<Summary>";
        internal static readonly string SummaryEndElement = @"</Summary>";
        internal static readonly char QualificationCharacter = '.';
    }
}
