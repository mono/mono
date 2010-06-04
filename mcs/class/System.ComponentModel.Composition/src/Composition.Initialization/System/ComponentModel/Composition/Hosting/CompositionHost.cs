// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Globalization;
using System.Threading;
using System.ComponentModel.Composition.Primitives;
namespace System.ComponentModel.Composition.Hosting
{
    public static class CompositionHost
    {
        // Field is internal only to assist in testing
        internal static CompositionContainer _container = null;
        private static object _lockObject = new object();

        /// <summary>
        ///     This method can be used to initialize the global container used by <see cref="CompositionInitializer.SatisfyImports(object)"/>
        ///     in case where the default container doesn't provide enough flexibility. 
        ///     
        ///     If this method is needed it should be called exactly once and as early as possible in the application host. It will need
        ///     to be called before the first call to <see cref="CompositionInitializer.SatisfyImports(object)"/>
        /// </summary>
        /// <param name="container">
        ///     <see cref="CompositionContainer"/> that should be used instead of the default global container.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="container"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     Either <see cref="Initialize(CompositionContainer)" /> or <see cref="Initialize(ComposablePartCatalog[])" /> has already been called or someone has already made use of the global 
        ///     container via <see cref="CompositionInitializer.SatisfyImports(object)"/>. In either case you need to ensure that it 
        ///     is called only once and that it is called early in the application host startup code.
        /// </exception>
        public static void Initialize(CompositionContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            CompositionContainer globalContainer = null;
            bool alreadyCreated = TryGetOrCreateContainer(() => container, out globalContainer);

            if (alreadyCreated)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, 
                    Strings.InvalidOperationException_GlobalContainerAlreadyInitialized));
            }
        }

        /// <summary>
        ///     This method can be used to initialize the global container used by <see cref="CompositionInitializer.SatisfyImports(object)"/>
        ///     in case where the default container doesn't provide enough flexibility. 
        ///     
        ///     If this method is needed it should be called exactly once and as early as possible in the application host. It will need
        ///     to be called before the first call to <see cref="CompositionInitializer.SatisfyImports(object)"/>
        /// </summary>
        /// <param name="catalogs">
        ///     An array of <see cref="ComposablePartCatalog"/> that should be used to initialize the <see cref="CompositionContainer"/> with.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="catalogs"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     Either <see cref="Initialize(CompositionContainer)" /> or <see cref="Initialize(ComposablePartCatalog[])" />has already been called or someone has already made use of the global 
        ///     container via <see cref="CompositionInitializer.SatisfyImports(object)"/>. In either case you need to ensure that it 
        ///     is called only once and that it is called early in the application host startup code.
        /// </exception>
        public static CompositionContainer Initialize(params ComposablePartCatalog[] catalogs)
        {
            AggregateCatalog aggregateCatalog = new AggregateCatalog(catalogs);
            CompositionContainer container = new CompositionContainer(aggregateCatalog);
            try
            {
                CompositionHost.Initialize(container);
            }
            catch
            {
                container.Dispose();

                // NOTE : this is important, as this prevents the disposal of the catalogs passed as input arguments
                aggregateCatalog.Catalogs.Clear();
                aggregateCatalog.Dispose();

                throw;
            }

            return container;
        }



        internal static bool TryGetOrCreateContainer(Func<CompositionContainer> createContainer, out CompositionContainer globalContainer)
        {
            bool alreadyCreated = true;
            if (_container == null)
            {
                var container = createContainer.Invoke();
                lock (_lockObject)
                {
                    if (_container == null)
                    {
                        Thread.MemoryBarrier();
                        _container = container;
                        alreadyCreated = false;
                    }
                }
            }
            globalContainer = _container;
            return alreadyCreated;
        }
    }
}