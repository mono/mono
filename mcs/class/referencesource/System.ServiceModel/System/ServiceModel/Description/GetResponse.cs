//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System.Net.Security;

namespace System.ServiceModel.Description
{
    [MessageContract(IsWrapped = false)]
    internal class GetResponse
    {
        MetadataSet metadataSet;

        internal GetResponse() { }
        internal GetResponse(MetadataSet metadataSet)
            : this()
        {
            this.metadataSet = metadataSet;
        }

        [MessageBodyMember(Name = MetadataStrings.MetadataExchangeStrings.Metadata, Namespace = MetadataStrings.MetadataExchangeStrings.Namespace)]
        internal MetadataSet Metadata
        {
            get { return this.metadataSet; }
            set { this.metadataSet = value; }
        }
    }
} 
