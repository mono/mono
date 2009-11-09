// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using Microsoft.Internal;

namespace System.ComponentModel.Composition.Diagnostics
{
    internal static class CompositionTrace
    {
        internal static void PartDefinitionResurrected(ComposablePartDefinition definition)
        {
            Assumes.NotNull(definition);

            if (CompositionTraceSource.CanWriteInformation)
            {
                CompositionTraceSource.WriteInformation(CompositionTraceId.Rejection_DefinitionResurrected, 
                                                        Strings.CompositionTrace_Rejection_DefinitionResurrected, 
                                                        definition.GetDisplayName());
            }
        }

        internal static void PartDefinitionRejected(ComposablePartDefinition definition, ChangeRejectedException exception)
        {
            Assumes.NotNull(definition, exception);

            if (CompositionTraceSource.CanWriteWarning)
            {
                CompositionTraceSource.WriteWarning(CompositionTraceId.Rejection_DefinitionRejected, 
                                                    Strings.CompositionTrace_Rejection_DefinitionRejected, 
                                                    definition.GetDisplayName(), 
                                                    exception.Message);
            }
        }

#if !SILVERLIGHT

        internal static void AssemblyLoadFailed(DirectoryCatalog catalog, string fileName, Exception exception)
        {
            Assumes.NotNull(catalog, exception);
            Assumes.NotNullOrEmpty(fileName);            

            if (CompositionTraceSource.CanWriteWarning)
            {
                CompositionTraceSource.WriteWarning(CompositionTraceId.Discovery_AssemblyLoadFailed, 
                                                    Strings.CompositionTrace_Discovery_AssemblyLoadFailed, 
                                                    catalog.GetDisplayName(),
                                                    fileName, 
                                                    exception.Message);
            }
        }

#endif

        internal static void DefinitionMarkedWithPartNotDiscoverableAttribute(Type type)
        {
            Assumes.NotNull(type);

            if (CompositionTraceSource.CanWriteInformation)
            {
                CompositionTraceSource.WriteInformation(CompositionTraceId.Discovery_DefinitionMarkedWithPartNotDiscoverableAttribute, 
                                                        Strings.CompositionTrace_Discovery_DefinitionMarkedWithPartNotDiscoverableAttribute, 
                                                        type.GetDisplayName());
            }
        }

        internal static void DefinitionContainsGenericsParameters(Type type)
        {
            Assumes.NotNull(type);

            if (CompositionTraceSource.CanWriteInformation)
            {
                CompositionTraceSource.WriteInformation(CompositionTraceId.Discovery_DefinitionContainsGenericParameters,
                                                        Strings.CompositionTrace_Discovery_DefinitionContainsGenericParameters,
                                                        type.GetDisplayName());
            }
        }

        internal static void DefinitionContainsNoExports(Type type)
        {
            Assumes.NotNull(type);

            if (CompositionTraceSource.CanWriteInformation)
            {
                CompositionTraceSource.WriteInformation(CompositionTraceId.Discovery_DefinitionContainsNoExports,
                                                        Strings.CompositionTrace_Discovery_DefinitionContainsNoExports,
                                                        type.GetDisplayName());
            }
        }

        internal static void MemberMarkedWithMultipleImportAndImportMany(ReflectionItem item)
        {
            Assumes.NotNull(item);

            if (CompositionTraceSource.CanWriteError)
            {
                CompositionTraceSource.WriteError(CompositionTraceId.Discovery_MemberMarkedWithMultipleImportAndImportMany,
                                                  Strings.CompositionTrace_Discovery_MemberMarkedWithMultipleImportAndImportMany,
                                                  item.GetDisplayName());
            }
        }
    }
}
