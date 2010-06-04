// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Internal;

namespace System.ComponentModel.Composition.Primitives
{
    /// <summary>
    ///     Defines the <see langword="abstract"/> base class for composable part catalogs, which produce
    ///     and return <see cref="ComposablePartDefinition"/> objects.
    /// </summary>
    /// <remarks>
    ///     This type is thread safe.
    /// </remarks>
    [DebuggerTypeProxy(typeof(ComposablePartCatalogDebuggerProxy))]
    public abstract class ComposablePartCatalog : IDisposable
    {
        private bool _isDisposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ComposablePartCatalog"/> class.
        /// </summary>
        protected ComposablePartCatalog()
        {
        }

        /// <summary>
        ///     Gets the part definitions of the catalog.
        /// </summary>
        /// <value>
        ///     A <see cref="IQueryable{T}"/> of <see cref="ComposablePartDefinition"/> objects of the 
        ///     <see cref="ComposablePartCatalog"/>.
        /// </value>
        /// <exception cref="ObjectDisposedException">
        ///     The <see cref="ComposablePartCatalog"/> has been disposed of.
        /// </exception>
        /// <remarks>
        ///     <note type="inheritinfo">
        ///         Overriders of this property should never return <see langword="null"/>.
        ///     </note>
        /// </remarks>
        public abstract IQueryable<ComposablePartDefinition> Parts 
        { 
            get; 
        }

        /// <summary>
        ///     Returns the export definitions that match the constraint defined by the specified definition.
        /// </summary>
        /// <param name="definition">
        ///     The <see cref="ImportDefinition"/> that defines the conditions of the 
        ///     <see cref="ExportDefinition"/> objects to return.
        /// </param>
        /// <returns>
        ///     An <see cref="IEnumerable{T}"/> of <see cref="Tuple{T1, T2}"/> containing the 
        ///     <see cref="ExportDefinition"/> objects and their associated 
        ///     <see cref="ComposablePartDefinition"/> for objects that match the constraint defined 
        ///     by <paramref name="definition"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="definition"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     The <see cref="ComposablePartCatalog"/> has been disposed of.
        /// </exception>
        /// <remarks>
        ///     <note type="inheritinfo">
        ///         Overriders of this property should never return <see langword="null"/>, if no 
        ///         <see cref="ExportDefinition"/> match the conditions defined by 
        ///         <paramref name="definition"/>, return an empty <see cref="IEnumerable{T}"/>.
        ///     </note>
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public virtual IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> GetExports(ImportDefinition definition)
        {
            this.ThrowIfDisposed();

            Requires.NotNull(definition, "definition");

            var exports = new List<Tuple<ComposablePartDefinition, ExportDefinition>>();
            foreach (var part in this.Parts)
            {
                foreach (var export in part.ExportDefinitions)
                {
                    if (definition.IsConstraintSatisfiedBy(export))
                    {
                        exports.Add(new Tuple<ComposablePartDefinition, ExportDefinition>(part, export));
                    }
                }
            }
            return exports;

        }

        /// <summary>
        ///     Releases the unmanaged resources used by the <see cref="ComposablePartCatalog"/> and 
        ///     optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     <see langword="true"/> to release both managed and unmanaged resources; 
        ///     <see langword="false"/> to release only unmanaged resources.
        /// </param>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) 
        {
            this._isDisposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (this._isDisposed)
            {
                throw ExceptionBuilder.CreateObjectDisposed(this);
            }
        }
    }
}
