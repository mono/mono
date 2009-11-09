// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using Microsoft.Internal;
using Microsoft.Internal.Collections;
using System.Threading;

namespace System.ComponentModel.Composition.Hosting
{
    public class AggregateExportProvider : ExportProvider , IDisposable
    {
        private ReadOnlyCollection<ExportProvider> _providers;
        private volatile int _isDisposed = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateExportProvider"/> class.
        /// </summary>
        /// <param name="providers">The prioritized list of export providers.</param>
        /// <remarks>
        ///     <para>
        ///         The <see cref="AggregateExportProvider"/> will consult the providers in the order they have been specfied when 
        ///         executing <see cref="ExportProvider.GetExports(ImportDefinition,AtomicComposition)"/>. 
        ///     </para>
        ///     <para>
        ///         The <see cref="AggregateExportProvider"/> does not take ownership of the specified providers. 
        ///         That is, it will not try to dispose of any of them when it gets disposed.
        ///     </para>
        /// </remarks>
        public AggregateExportProvider(params ExportProvider[] providers) 
            : this(providers.AsEnumerable())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateExportProvider"/> class.
        /// </summary>
        /// <param name="providers">The prioritized list of export providers. The providers are consulted in order in which they are supplied.</param>
        /// <remarks>
        ///     <para>
        ///         The <see cref="AggregateExportProvider"/> will consult the providers in the order they have been specfied when 
        ///         executing <see cref="ExportProvider.GetExports(ImportDefinition,AtomicComposition)"/>. 
        ///     </para>
        ///     <para>
        ///         The <see cref="AggregateExportProvider"/> does not take ownership of the specified providers. 
        ///         That is, it will not try to dispose of any of them when it gets disposed.
        ///     </para>
        /// </remarks>
        public AggregateExportProvider(IEnumerable<ExportProvider> providers)
        {
            List<ExportProvider> providerList = new List<ExportProvider>();

            if (providers != null)
            {
                // we are in the constructor, so there's no need to lock anything
                foreach (var provider in providers)
                {
                    if (provider == null)
                    {
                        throw ExceptionBuilder.CreateContainsNullElement("providers");
                    }

                    providerList.Add(provider);

                    provider.ExportsChanged += this.OnExportChangedInternal;
                    provider.ExportsChanging += this.OnExportChangingInternal;
                }
            }

            // this will always fully copy the array
            this._providers = new ReadOnlyCollection<ExportProvider>(providerList);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // NOTE : According to http://msdn.microsoft.com/en-us/library/4bw5ewxy.aspx, the warning is bogus when used with Interlocked API.
#pragma warning disable 420
                if (Interlocked.CompareExchange(ref this._isDisposed, 1, 0) == 0)
#pragma warning restore 420
                {
                    this._providers.ForEach(provider =>
                    {
                        provider.ExportsChanged -= this.OnExportChangedInternal;
                        provider.ExportsChanging -= this.OnExportChangingInternal;
                    });
                }
            }
        }

        /// <summary>
        ///     Gets the export providers which the aggregate export provider aggregates.
        /// </summary>
        /// <value>
        ///     A <see cref="ReadOnlyCollection{T}"/> of <see cref="ExportProvider"/> objects
        ///     which the <see cref="AggregateExportProvider"/> aggregates.
        /// </value>
        /// <exception cref="ObjectDisposedException">
        ///     The <see cref="AggregateExportProvider"/> has been disposed of.
        /// </exception>
        public ReadOnlyCollection<ExportProvider> Providers
        {
            get
            {
                this.ThrowIfDisposed();

                return this._providers;
            }
        }

        /// <summary>
        /// Returns all exports that match the conditions of the specified import.
        /// </summary>
        /// <param name="definition">The <see cref="ImportDefinition"/> that defines the conditions of the
        /// <see cref="Export"/> to get.</param>
        /// <returns></returns>
        /// <result>
        /// An <see cref="IEnumerable{T}"/> of <see cref="Export"/> objects that match
        /// the conditions defined by <see cref="ImportDefinition"/>, if found; otherwise, an
        /// empty <see cref="IEnumerable{T}"/>.
        /// </result>
        /// <remarks>
        /// 	<note type="inheritinfo">
        /// The implementers should not treat the cardinality-related mismatches as errors, and are not
        /// expected to throw exceptions in those cases.
        /// For instance, if the import requests exactly one export and the provider has no matching exports or more than one,
        /// it should return an empty <see cref="IEnumerable{T}"/> of <see cref="Export"/>.
        /// </note>
        /// </remarks>
        protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition)
        {
            this.ThrowIfDisposed();

            if (definition.Cardinality == ImportCardinality.ZeroOrMore)
            {
                var exports = new List<Export>();
                foreach (var provider in this._providers)
                {
                    foreach (var export in provider.GetExports(definition, atomicComposition))
                    {
                        exports.Add(export);
                    }
                }
                return exports;
            }
            else
            {
                IEnumerable<Export> allExports = Enumerable.Empty<Export>();

                // if asked for "one or less", the prioriry is at play - the first provider that agrees to return the value 
                // which best complies with the request, wins.
                foreach (ExportProvider provider in this._providers)
                {
                    IEnumerable<Export> exports;
                    bool cardinalityCheckResult = provider.TryGetExports(definition, atomicComposition, out exports);
                    bool anyExports = exports.FastAny();
                    if (cardinalityCheckResult && anyExports)
                    {
                        // NOTE : if the provider returned nothing, we need to proceed, even if it indicated that the
                        // cardinality is correct - when asked for "one or less", the provider might - correctly - 
                        // return an empty sequence, but we shouldn't be satisfied with that as providers down the list
                        // might have a value we are interested in.
                        return exports;
                    }
                    else
                    {
                        // TODO
                        // This is a sneaky thing that we do - if in the end no provider returns the exports with the right cardinality
                        // we simply return the aggregation of all exports they have restuned. This way the end result is still not waht we want
                        // but no information is lost.
                        // WE SHOULD fix this behavior, but this is ONLY possible if we can treat many exports as no exports for the sake of singles
                        if (anyExports)
                        {
                            allExports = allExports.Concat(exports);
                        }
                    }
                }

                return allExports;
            }
        }

        private void OnExportChangedInternal(object sender, ExportsChangeEventArgs e)
        {
            this.OnExportsChanged(e);
        }

        private void OnExportChangingInternal(object sender, ExportsChangeEventArgs e)
        {
            this.OnExportsChanging(e);
        }

        private void ThrowIfDisposed()
        {
            if (this._isDisposed == 1)
            {
                throw ExceptionBuilder.CreateObjectDisposed(this);
            }
        }
    }
}
