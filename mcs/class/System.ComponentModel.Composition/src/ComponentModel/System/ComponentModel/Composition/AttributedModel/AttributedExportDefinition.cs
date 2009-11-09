// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.AttributedModel;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Internal;
using Microsoft.Internal.Collections;
using System.Threading;

namespace System.ComponentModel.Composition.AttributedModel
{
    internal class AttributedExportDefinition : ExportDefinition
    {
        private readonly AttributedPartCreationInfo _partCreationInfo;
        private readonly MemberInfo _member;
        private readonly ExportAttribute _exportAttribute;

        private IDictionary<string, object> _metadata;

        public AttributedExportDefinition(AttributedPartCreationInfo partCreationInfo, MemberInfo member, ExportAttribute exportAttribute)
            : base(member.GetContractNameFromExport(exportAttribute), (IDictionary<string, object>)null)
        {
            Assumes.NotNull(partCreationInfo);
            Assumes.NotNull(member);
            Assumes.NotNull(exportAttribute);

            this._partCreationInfo = partCreationInfo;
            this._member = member;
            this._exportAttribute = exportAttribute;
        }

        public override IDictionary<string, object> Metadata
        {
            get
            {
                if (this._metadata == null)
                {
                    IDictionary<string, object> metadata;
                    this._member.TryExportMetadataForMember(out metadata);

                    string typeIdentity = this._member.GetTypeIdentityFromExport(this._exportAttribute);
                    metadata.Add(CompositionConstants.ExportTypeIdentityMetadataName, typeIdentity);

                    var partMetadata = this._partCreationInfo.GetMetadata();
                    if (partMetadata != null && partMetadata.ContainsKey(CompositionConstants.PartCreationPolicyMetadataName))
                    {
                        metadata.Add(CompositionConstants.PartCreationPolicyMetadataName, partMetadata[CompositionConstants.PartCreationPolicyMetadataName]);
                    }

                    this._metadata = metadata;
                }
                return this._metadata;
            }
        }
    }
    
}
