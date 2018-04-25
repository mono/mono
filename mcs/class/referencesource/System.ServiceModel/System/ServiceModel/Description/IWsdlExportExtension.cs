//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Description
{
    public interface IWsdlExportExtension
    {
        void ExportContract(WsdlExporter exporter, WsdlContractConversionContext context);
        void ExportEndpoint(WsdlExporter exporter, WsdlEndpointConversionContext context);
    }

}