// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Globalization;
using System.Threading;

namespace System.ComponentModel.Composition.Hosting
{
    public static class CompositionHost
    {
        // Field is internal only to assist in testing
        internal static CompositionContainer _container = null;
        private static object _lockObject = new object();

        /// <summary>
        ///     This method can be used to initialize the global container used by <see cref="PartInitializer.SatisfyImports"/>
        ///     in case where the default container doesn't provide enough flexibility. 
        ///     
        ///     If this method is needed it should be called exactly once and as early as possible in the application host. It will need
        ///     to be called before the first call to <see cref="PartInitializer.SatisfyImports"/>
        /// </summary>
        /// <param name="container">
        ///     <see cref="CompositionContainer"/> that should be used instead of the default global container.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="container"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     Either <see cref="InitializeContainer" /> has already been called or someone has already made use of the global 
        ///     container via <see cref="PartInitializer.SatisfyImports"/>. In either case you need to ensure that it 
        ///     is called only once and that it is called early in the application host startup code.
        /// </exception>
        public static void InitializeContainer(CompositionContainer container)
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