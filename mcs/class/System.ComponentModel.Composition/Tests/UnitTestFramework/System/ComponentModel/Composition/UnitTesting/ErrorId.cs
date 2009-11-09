// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;

namespace System.ComponentModel.Composition
{
    // We need a public version of CompositionErrorId, so that the QA tests can access and verify the errors.
    public enum ErrorId : int
    {
        Unknown = CompositionErrorId.Unknown,
        InvalidExportMetadata = CompositionErrorId.InvalidExportMetadata,
        RequiredMetadataNotFound = CompositionErrorId.RequiredMetadataNotFound,
        UnsupportedExportType = CompositionErrorId.UnsupportedExportType,
        ImportNotSetOnPart = CompositionErrorId.ImportNotSetOnPart,
        ImportEngine_ComposeTookTooManyIterations = CompositionErrorId.ImportEngine_ComposeTookTooManyIterations,
        ImportEngine_ImportCardinalityMismatch = CompositionErrorId.ImportEngine_ImportCardinalityMismatch,        
        ImportEngine_PartCycle = CompositionErrorId.ImportEngine_PartCycle,
        ImportEngine_PartCannotSetImport = CompositionErrorId.ImportEngine_PartCannotSetImport,
        ImportEngine_PartCannotGetExportedValue = CompositionErrorId.ImportEngine_PartCannotGetExportedValue,
        ImportEngine_PartCannotActivate = CompositionErrorId.ImportEngine_PartCannotActivate,
        ImportEngine_PreventedByExistingImport = CompositionErrorId.ImportEngine_PreventedByExistingImport,
        ImportEngine_InvalidStateForRecomposition = CompositionErrorId.ImportEngine_InvalidStateForRecomposition,
        ReflectionModel_PartConstructorMissing = CompositionErrorId.ReflectionModel_PartConstructorMissing,
        ReflectionModel_PartConstructorThrewException = CompositionErrorId.ReflectionModel_PartConstructorThrewException,
        ReflectionModel_PartOnImportsSatisfiedThrewException = CompositionErrorId.ReflectionModel_PartOnImportsSatisfiedThrewException,
        ReflectionModel_ExportNotReadable = CompositionErrorId.ReflectionModel_ExportNotReadable,
        ReflectionModel_ExportThrewException = CompositionErrorId.ReflectionModel_ExportThrewException,
        ReflectionModel_ExportMethodTooManyParameters = CompositionErrorId.ReflectionModel_ExportMethodTooManyParameters,
        ReflectionModel_ImportNotWritable = CompositionErrorId.ReflectionModel_ImportNotWritable,
        ReflectionModel_ImportThrewException = CompositionErrorId.ReflectionModel_ImportThrewException,
        ReflectionModel_ImportNotAssignableFromExport = CompositionErrorId.ReflectionModel_ImportNotAssignableFromExport,
        ReflectionModel_ImportCollectionNull = CompositionErrorId.ReflectionModel_ImportCollectionNull,
        ReflectionModel_ImportCollectionNotWritable = CompositionErrorId.ReflectionModel_ImportCollectionNotWritable,
        ReflectionModel_ImportCollectionConstructionThrewException = CompositionErrorId.ReflectionModel_ImportCollectionConstructionThrewException,
        ReflectionModel_ImportCollectionGetThrewException = CompositionErrorId.ReflectionModel_ImportCollectionGetThrewException,
        ReflectionModel_ImportCollectionIsReadOnlyThrewException = CompositionErrorId.ReflectionModel_ImportCollectionIsReadOnlyThrewException,
        ReflectionModel_ImportCollectionClearThrewException = CompositionErrorId.ReflectionModel_ImportCollectionClearThrewException,
        ReflectionModel_ImportCollectionAddThrewException = CompositionErrorId.ReflectionModel_ImportCollectionAddThrewException,
        ReflectionModel_ImportManyOnParameterCanOnlyBeAssigned = CompositionErrorId.ReflectionModel_ImportManyOnParameterCanOnlyBeAssigned,
    }
}
