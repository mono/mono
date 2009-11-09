// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Microsoft.Internal;

namespace System.ComponentModel.Composition.Hosting
{
    /// <summary>
    ///     A mutable collection of <see cref="ComposablePartCatalog"/>s.  
    /// </summary>
    /// <remarks>
    ///     This type is thread safe.
    /// </remarks>
    public class AggregateCatalog : ComposablePartCatalog, INotifyComposablePartCatalogChanged
    {
        private ComposablePartCatalogCollection _catalogs = null;
        private volatile int _isDisposed = 0;
        private IQueryable<ComposablePartDefinition> _partsQuery;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AggregateCatalog"/> class.
        /// </summary>
        public AggregateCatalog()
            : this((IEnumerable<ComposablePartCatalog>)null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AggregateCatalog"/> class 
        ///     with the specified catalogs.
        /// </summary>
        /// <param name="catalogs">
        ///     An <see cref="Array"/> of <see cref="ComposablePartCatalog"/> objects to add to the 
        ///     <see cref="AggregateCatalog"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="catalogs"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="catalogs"/> contains an element that is <see langword="null"/>.
        /// </exception>
        public AggregateCatalog(params ComposablePartCatalog[] catalogs)
            : this((IEnumerable<ComposablePartCatalog>)catalogs)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AggregateCatalog"/> class
        ///     with the specified catalogs.
        /// </summary>
        /// <param name="catalogs">
        ///     An <see cref="IEnumerable{T}"/> of <see cref="ComposablePartCatalog"/> objects to add
        ///     to the <see cref="AggregateCatalog"/>; or <see langword="null"/> to 
        ///     create an <see cref="AggregateCatalog"/> that is empty.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     <paramref name="catalogs"/> contains an element that is <see langword="null"/>.
        /// </exception>
        public AggregateCatalog(IEnumerable<ComposablePartCatalog> catalogs)
        {
            Requires.NullOrNotNullElements(catalogs, "catalogs");

            this._catalogs = new ComposablePartCatalogCollection(catalogs, this.OnChanged, this.OnChanging);
            this._partsQuery = this._catalogs.AsQueryable().SelectMany(catalog => catalog.Parts);
        }

        /// <summary>
        /// Notify when the contents of the Catalog has changed.
        /// </summary>
        public event EventHandler<ComposablePartCatalogChangeEventArgs> Changed
        {
            add
            {
                this._catalogs.Changed += value;
            }
            remove
            {
                this._catalogs.Changed -= value;
            }
        }

        /// <summary>
        /// Notify when the contents of the Catalog has changing.
        /// </summary>
        public event EventHandler<ComposablePartCatalogChangeEventArgs> Changing
        {
            add
            {
                this._catalogs.Changing += value;
            }
            remove
            {
                this._catalogs.Changing -= value;
            }
        }

        /// <summary>
        ///     Gets the part definitions of the catalog.
        /// </summary>
        /// <value>
        ///     A <see cref="IQueryable{T}"/> of <see cref="ComposablePartDefinition"/> objects of the 
        ///     <see cref="AggregateCatalog"/>.
        /// </value>
        /// <exception cref="ObjectDisposedException">
        ///     The <see cref="AggregateCatalog"/> has been disposed of.
        /// </exception>
        public override IQueryable<ComposablePartDefinition> Parts
        {
            get
            {
                this.ThrowIfDisposed();
                return this._partsQuery;
            }
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
        ///     The <see cref="AggregateCatalog"/> has been disposed of.
        /// </exception>
        public override IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> GetExports(ImportDefinition definition)
        {
            this.ThrowIfDisposed();

            Requires.NotNull(definition, "definition");

            // delegate the query to each catalog and merge the results.
            var exports = new List<Tuple<ComposablePartDefinition, ExportDefinition>>();
            foreach (var catalog in this._catalogs)
            {
                foreach (var export in catalog.GetExports(definition))
                {
                    exports.Add(export);
                }
            }
            return exports;
        }

        /// <summary>
        ///     Gets the underlying catalogs of the catalog.
        /// </summary>
        /// <value>
        ///     An <see cref="ICollection{T}"/> of underlying <see cref="ComposablePartCatalog"/> objects
        ///     of the <see cref="AggregateCatalog"/>.
        /// </value>
        /// <exception cref="ObjectDisposedException">
        ///     The <see cref="AggregateCatalog"/> has been disposed of.
        /// </exception>
        public ICollection<ComposablePartCatalog> Catalogs
        {
            get
            {
                this.ThrowIfDisposed();
                return this._catalogs;
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    // NOTE : According to http://msdn.microsoft.com/en-us/library/4bw5ewxy.aspx, the warning is bogus when used with Interlocked API.
#pragma warning disable 420
                    if (Interlocked.CompareExchange(ref this._isDisposed, 1, 0) == 0)
#pragma warning restore 420
                    {
                        this._catalogs.Dispose();
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        /// <summary>
        ///     Raises the <see cref="INotifyComposablePartCatalogChanged.Changed"/> event.
        /// </summary>
        /// <param name="e">
        ///     An <see cref="ComposablePartCatalogChangeEventArgs"/> containing the data for the event.
        /// </param>
        protected virtual void OnChanged(ComposablePartCatalogChangeEventArgs e)
        {
            this._catalogs.OnChanged(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="INotifyComposablePartCatalogChanged.Changing"/> event.
        /// </summary>
        /// <param name="e">
        ///     An <see cref="ComposablePartCatalogChangeEventArgs"/> containing the data for the event.
        /// </param>
        protected virtual void OnChanging(ComposablePartCatalogChangeEventArgs e)
        {
            this._catalogs.OnChanging(this, e);
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
