// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using Microsoft.Internal;

#if (SILVERLIGHT)
namespace System.ComponentModel.Composition.Hosting
{
    /// <summary>
    /// Implements a MEF catalog that supports Asynchronous download of Silverlast Xap files.
    /// </summary>
    public class DeploymentCatalog : ComposablePartCatalog, INotifyComposablePartCatalogChanged
    {
        static class State
        {
            public const int Created = 0;
            public const int Initialized = 1000;
            public const int DownloadStarted = 2000;
            public const int DownloadCompleted = 3000;
            public const int DownloadCancelled = 4000;
        }

        private Lock _lock = new Lock();
        private volatile bool _isDisposed = false;
        private Uri _uri = null;
        private int _state = State.Created;
        private AggregateCatalog _catalogCollection = new AggregateCatalog();
        private WebClient _webClient = null;

        /// <summary>
        /// Construct a Deployment catalog with the parts from the main Xap.
        /// </summary>
        public DeploymentCatalog()
        {
            this.DiscoverParts(Package.CurrentAssemblies);
            this._state = State.Initialized;
        }

        /// <summary>
        /// Construct a Deployment catalog with a string form relative uri.
        /// </summary>
        /// <value>
        ///     A relative Uri to the Download Xap file
        ///     <see cref="DeploymentCatalog"/>.
        /// </value>
        /// <exception cref="ArgumentException">
        ///     The argument is null or an empty string.
        /// </exception>
        public DeploymentCatalog(string uriRelative)
        {
            Requires.NotNullOrEmpty(uriRelative, "uriRelative");
            this._uri = new Uri(uriRelative, UriKind.Relative);
        }

        /// <summary>
        /// Construct a Deployment catalog with the parts from uri.
        /// </summary>
        /// <value>
        ///     A Uri to the Download Xap file
        ///     <see cref="System.Uri"/>.
        /// </value>
        /// <exception cref="ArgumentException">
        ///     The argument is null.
        /// </exception>
        public DeploymentCatalog(Uri uri)
        {
            Requires.NotNull<Uri>(uri, "uri");
            this._uri = uri;
        }

        /// <summary>
        /// Notify when the contents of the Catalog has changed.
        /// </summary>
        public event EventHandler<ComposablePartCatalogChangeEventArgs> Changed;

        /// <summary>
        /// Notify when the contents of the Catalog is changing.
        /// </summary>
        public event EventHandler<ComposablePartCatalogChangeEventArgs> Changing;

        /// <summary>
        /// Notify when the download has been completed.
        /// </summary>
        public event EventHandler<AsyncCompletedEventArgs> DownloadCompleted;

        /// <summary>
        /// Notify when the contents of the Progress of the download has changed.
        /// </summary>
        public event EventHandler<DownloadProgressChangedEventArgs> DownloadProgressChanged;
       
        /// <summary>
        /// Retrieve or create the WebClient.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     The <see cref="DeploymentCatalog"/> has been disposed of.
        /// </exception>
        private WebClient WebClient
        {
            get
            {
                this.ThrowIfDisposed();
                if(this._webClient == null)
                {
                    Interlocked.CompareExchange<WebClient>(ref this._webClient, new WebClient(), null);
                }
                return this._webClient;
            }
        }
        
        /// <summary>
        ///     Gets the part definitions of the Deployment catalog.
        /// </summary>
        /// <value>
        ///     A <see cref="IQueryable{T}"/> of <see cref="ComposablePartDefinition"/> objects of the 
        ///     <see cref="DeploymentCatalog"/>.
        /// </value>
        /// <exception cref="ObjectDisposedException">
        ///     The <see cref="DeploymentCatalog"/> has been disposed of.
        /// </exception>
        public override IQueryable<ComposablePartDefinition> Parts
        {
            get
            {
                this.ThrowIfDisposed();
                return this._catalogCollection.Parts;
            }
        }

        /// <summary>
        ///     Gets the Uri of this catalog
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     The <see cref="DeploymentCatalog"/> has been disposed of.
        /// </exception>
        public Uri Uri
        {
            get
            {
                this.ThrowIfDisposed();
                return this._uri;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="assemblies">
        /// </param>
        /// <exception cref="ObjectDisposedException">
        ///     The <see cref="DeploymentCatalog"/> has been disposed of.
        /// </exception>
        private void DiscoverParts(IEnumerable<Assembly> assemblies)
        {
            this.ThrowIfDisposed();

            var addedDefinitions = new List<ComposablePartDefinition>();
            var addedCatalogs = new Dictionary<string, ComposablePartCatalog>();
            using(new ReadLock(this._lock))
            {
                foreach (var assembly in assemblies)
                {
                    if (addedCatalogs.ContainsKey(assembly.FullName)) 
                    {
                        // Nothing to do because the assembly has already been added.
                        continue;
                    }

                    var catalog = new AssemblyCatalog(assembly);
                    addedDefinitions.AddRange(catalog.Parts);
                    addedCatalogs.Add(assembly.FullName, catalog);
                }
            }

            // Generate notifications
            using (var atomicComposition = new AtomicComposition())
            {
                var changingArgs = new ComposablePartCatalogChangeEventArgs(addedDefinitions, Enumerable.Empty<ComposablePartDefinition>(), atomicComposition);
                this.OnChanging(changingArgs);

                using (new WriteLock(this._lock))
                {
                    foreach (var item in addedCatalogs)
                    {
                        this._catalogCollection.Catalogs.Add(item.Value);
                    }
                }
                atomicComposition.Complete();
            }

            var changedArgs = new ComposablePartCatalogChangeEventArgs(addedDefinitions, Enumerable.Empty<ComposablePartDefinition>(), null);
            this.OnChanged(changedArgs);
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
        ///     The <see cref="DeploymentCatalog"/> has been disposed of.
        /// </exception>
        public override IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> GetExports(ImportDefinition definition)
        {
            this.ThrowIfDisposed();
            Requires.NotNull(definition, "definition");

            return this._catalogCollection.GetExports(definition);
        }

        /// <summary>
        /// Cancel the async operation.
        /// </summary>
        public void CancelAsync()
        {
            ThrowIfDisposed();
            MutateStateOrThrow(State.DownloadCancelled, State.DownloadStarted);
            this.WebClient.CancelAsync();
        }

        /// <summary>
        /// Begin the asynchronous download.
        /// </summary>
        public void DownloadAsync()
        {
            ThrowIfDisposed();

            if (Interlocked.CompareExchange(ref this._state, State.DownloadStarted, State.Created) == State.Created)
            {
                // Created with Downloadable content do download
                this.WebClient.OpenReadCompleted += new OpenReadCompletedEventHandler(HandleOpenReadCompleted);
                this.WebClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(HandleDownloadProgressChanged);
                this.WebClient.OpenReadAsync(Uri, this);
            }
            else
            {
                // Created with LocalAssemblies 
                MutateStateOrThrow(State.DownloadCompleted, State.Initialized);

                this.OnDownloadCompleted(new AsyncCompletedEventArgs(null, false, this));
            }
        }

        void HandleDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            EventHandler<DownloadProgressChangedEventArgs> downloadProgressChangedEvent = this.DownloadProgressChanged;
            if (downloadProgressChangedEvent != null)
            {
                downloadProgressChangedEvent(this, e);
            }
        }

        private void HandleOpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            Exception error = e.Error;
            bool cancelled = e.Cancelled;

            // Possible valid current states are DownloadStarted and DownloadCancelled.
            int currentState = Interlocked.CompareExchange(ref this._state, State.DownloadCompleted, State.DownloadStarted);

            if (currentState != State.DownloadStarted)
            {
                cancelled = true;
            }

            if (error == null && !cancelled)
            {
                try
                {
                    var assemblies = Package.LoadPackagedAssemblies(e.Result);
                    this.DiscoverParts(assemblies);
                }
                catch (Exception ex)
                {
                    error = new InvalidOperationException(Strings.InvalidOperationException_ErrorReadingXap, ex);
                }
            }

            this.OnDownloadCompleted(new AsyncCompletedEventArgs(error, cancelled, this));
        }

        /// <summary>
        ///     Raises the <see cref="INotifyComposablePartCatalogChanged.Changed"/> event.
        /// </summary>
        /// <param name="e">
        ///     An <see cref="ComposablePartCatalogChangeEventArgs"/> containing the data for the event.
        /// </param>
        protected virtual void OnChanged(ComposablePartCatalogChangeEventArgs e)
        {
            EventHandler<ComposablePartCatalogChangeEventArgs> changedEvent = this.Changed;
            if (changedEvent != null)
            {
                changedEvent(this, e);
            }
        }

        /// <summary>
        ///     Raises the <see cref="INotifyComposablePartCatalogChanged.Changing"/> event.
        /// </summary>
        /// <param name="e">
        ///     An <see cref="ComposablePartCatalogChangeEventArgs"/> containing the data for the event.
        /// </param>
        protected virtual void OnChanging(ComposablePartCatalogChangeEventArgs e)
        {
            EventHandler<ComposablePartCatalogChangeEventArgs> changingEvent = this.Changing;
            if (changingEvent != null)
            {
                changingEvent(this, e);
            }
        }

        /// <summary>
        ///     Raises the <see cref="DownloadCompleted"/> event.
        /// </summary>
        /// <param name="e">
        ///     An <see cref="AsyncCompletedEventArgs"/> containing the data for the event.
        /// </param>
        protected virtual void OnDownloadCompleted(AsyncCompletedEventArgs e)
        {
            EventHandler<AsyncCompletedEventArgs> downloadCompletedEvent = this.DownloadCompleted;
            if (downloadCompletedEvent != null)
            {
                downloadCompletedEvent(this, e);
            }
        }

        /// <summary>
        ///     Raises the <see cref="DownloadProgressChanged"/> event.
        /// </summary>
        /// <param name="e">
        ///     An <see cref="ProgressChangedEventArgs"/> containing the data for the event.
        /// </param>
        protected virtual void OnDownloadProgressChanged(DownloadProgressChangedEventArgs e)
        {
            EventHandler<DownloadProgressChangedEventArgs> downloadProgressChangedEvent = this.DownloadProgressChanged;
            if (downloadProgressChangedEvent != null)
            {
                downloadProgressChangedEvent(this, e);
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (!this._isDisposed)
                    {
                        AggregateCatalog catalogs = null;
                        bool disposeLock = false;
                        try
                        {
                            using (new WriteLock(this._lock))
                            {
                                if (!this._isDisposed)
                                {
                                    disposeLock = true;
                                    catalogs = this._catalogCollection;
                                    this._catalogCollection = null;
                                    this._isDisposed = true;
                                }
                            }
                        }
                        finally
                        {
                            if (catalogs != null)
                            {
                                catalogs.Dispose();
                            }

                            if (disposeLock)
                            {
                                this._lock.Dispose();
                            }
                        }
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private void ThrowIfDisposed()
        {
            if (this._isDisposed)
            {
                throw new ObjectDisposedException(this.GetType().ToString()); 
            }
        }

        private void MutateStateOrThrow(int toState, int fromState)
        {
            int currentState = Interlocked.CompareExchange(ref this._state, toState, fromState);
            if(currentState != fromState)
            {
                throw new InvalidOperationException(Strings.InvalidOperationException_DeploymentCatalogInvalidStateChange);
            }
        }
    }
}
#endif
