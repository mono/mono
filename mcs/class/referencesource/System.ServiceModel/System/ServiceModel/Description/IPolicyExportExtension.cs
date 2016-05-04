//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Description
{
    public interface IPolicyExportExtension
    {
        void ExportPolicy(MetadataExporter exporter, PolicyConversionContext context);
    }
}
