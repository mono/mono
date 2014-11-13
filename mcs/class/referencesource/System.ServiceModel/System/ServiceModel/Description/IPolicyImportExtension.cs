//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Description
{
    public interface IPolicyImportExtension
    {
        void ImportPolicy(MetadataImporter importer, PolicyConversionContext context);
    }
}
