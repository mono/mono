//---------------------------------------------------------------------
// <copyright file="ModelFunctionTypeElement.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.EntityModel.SchemaObjectModel
{
    using System.Collections.Generic;
    using System.Data.Metadata.Edm;
    using System.Text;
    using Som = System.Data.EntityModel.SchemaObjectModel;

    abstract class ModelFunctionTypeElement : FacetEnabledSchemaElement
    {
        protected TypeUsage _typeUsage = null;

        internal ModelFunctionTypeElement(SchemaElement parentElement)
            : base(parentElement)
        {
            _typeUsageBuilder = new TypeUsageBuilder(this);  
        }

        internal abstract void WriteIdentity(StringBuilder builder);

        internal abstract TypeUsage GetTypeUsage();
        
        internal abstract bool ResolveNameAndSetTypeUsage(Converter.ConversionCache convertedItemCache, Dictionary<Som.SchemaElement, GlobalItem> newGlobalItems);
    }
}
