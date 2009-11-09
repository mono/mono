// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition.Primitives;

namespace System.ComponentModel.Composition.Hosting
{
    public static class CompositionConstants
    {
        private const string CompositionNamespace = "System.ComponentModel.Composition";

        public const string PartCreationPolicyMetadataName = CompositionNamespace + ".CreationPolicy";
        public const string ExportTypeIdentityMetadataName = "ExportTypeIdentity";
        public const string ProductDefinitionMetadataName = "ProductDefinition";

        public const string PartCreatorContractName = CompositionNamespace + ".Contracts.PartCreator";
        public static readonly string PartCreatorTypeIdentity = AttributedModelServices.GetTypeIdentity(typeof(ComposablePartDefinition));
    }
}
