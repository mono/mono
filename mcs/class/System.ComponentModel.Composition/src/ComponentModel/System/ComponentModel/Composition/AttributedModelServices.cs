// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Runtime.Serialization;
using System.ComponentModel.Composition.AttributedModel;
using System.Reflection;
using System.Linq;
using Microsoft.Internal;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace System.ComponentModel.Composition
{
    public static class AttributedModelServices
    {
        [SuppressMessage("Microsoft.Design", "CA1004")]
        public static TMetadataView GetMetadataView<TMetadataView>(IDictionary<string, object> metadata)
        {
            Requires.NotNull(metadata, "metadata");

            return MetadataViewProvider.GetMetadataView<TMetadataView>(metadata);
        }

        public static ComposablePart CreatePart(object attributedPart)
        {
            Requires.NotNull(attributedPart, "attributedPart");
            return AttributedModelDiscovery.CreatePart(attributedPart);
        }

        public static ComposablePartDefinition CreatePartDefinition(Type type, ICompositionElement origin)
        {
            Requires.NotNull(type, "type");
            return AttributedModelServices.CreatePartDefinition(type, origin, false);
        }

        public static ComposablePartDefinition CreatePartDefinition(Type type, ICompositionElement origin, bool ensureIsDiscoverable)
        {
            Requires.NotNull(type, "type");
            if (ensureIsDiscoverable)
            {
                return AttributedModelDiscovery.CreatePartDefinitionIfDiscoverable(type, origin);
            }
            else
            {
                return AttributedModelDiscovery.CreatePartDefinition(type, null, false, origin);
            }
        }

        public static string GetTypeIdentity(Type type)
        {
            Requires.NotNull(type, "type");

            return ContractNameServices.GetTypeIdentity(type);
        }

        public static string GetTypeIdentity(MethodInfo method)
        {
            Requires.NotNull(method, "method");

            return ContractNameServices.GetTypeIdentityFromMethod(method);
        }

        public static string GetContractName(Type type)
        {
            return AttributedModelServices.GetTypeIdentity(type);
        }

        public static ComposablePart AddExportedValue<T>(this CompositionBatch batch, T exportedValue)
        {
            Requires.NotNull(batch, "batch");
            string contractName = AttributedModelServices.GetContractName(typeof(T));

            return batch.AddExportedValue<T>(contractName, exportedValue);
        }

        public static void ComposeExportedValue<T>(this CompositionContainer container, T exportedValue)
        {
            Requires.NotNull(container, "container");

            CompositionBatch batch = new CompositionBatch();
            batch.AddExportedValue<T>(exportedValue);
            container.Compose(batch);
        }

        public static ComposablePart AddExportedValue<T>(this CompositionBatch batch, string contractName, T exportedValue)
        {
            Requires.NotNull(batch, "batch");

            string typeIdentity = AttributedModelServices.GetTypeIdentity(typeof(T));

            IDictionary<string, object> metadata = new Dictionary<string, object>();
            metadata.Add(CompositionConstants.ExportTypeIdentityMetadataName, typeIdentity);

            return batch.AddExport(new Export(contractName, metadata, () => exportedValue));
        }

        public static void ComposeExportedValue<T>(this CompositionContainer container, string contractName, T exportedValue)
        {
            Requires.NotNull(container, "container");

            CompositionBatch batch = new CompositionBatch();
            batch.AddExportedValue<T>(contractName, exportedValue);
            container.Compose(batch);
        }

        public static ComposablePart AddPart(this CompositionBatch batch, object attributedPart)
        {
            Requires.NotNull(batch, "batch");
            Requires.NotNull(attributedPart, "attributedPart");

            ComposablePart part = AttributedModelServices.CreatePart(attributedPart);

            batch.AddPart(part);

            return part;
        }

        public static void ComposeParts(this CompositionContainer container, params object[] attributedParts)
        {
            Requires.NotNull(container, "container");
            Requires.NotNullOrNullElements(attributedParts, "attributedParts");

            CompositionBatch batch = new CompositionBatch(
                attributedParts.Select(attributedPart => AttributedModelServices.CreatePart(attributedPart)).ToArray(),
                Enumerable.Empty<ComposablePart>());

            container.Compose(batch);
        }
     
        /// <summary>
        ///     Satisfies the imports of the specified attributed object exactly once and they will not
        ///     ever be recomposed.
        /// </summary>
        /// <param name="part">
        ///     The attributed object to set the imports.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="attributedPart"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="CompositionException">
        ///     An error occurred during composition. <see cref="CompositionException.Errors"/> will
        ///     contain a collection of errors that occurred.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     The <see cref="ICompositionService"/> has been disposed of.
        /// </exception>
        public static ComposablePart SatisfyImportsOnce(this ICompositionService compositionService, object attributedPart)
        {
            Requires.NotNull(compositionService, "compositionService");
            Requires.NotNull(attributedPart, "attributedPart");

            ComposablePart part = AttributedModelServices.CreatePart(attributedPart);
            compositionService.SatisfyImportsOnce(part);

            return part;
        }
    }
}
