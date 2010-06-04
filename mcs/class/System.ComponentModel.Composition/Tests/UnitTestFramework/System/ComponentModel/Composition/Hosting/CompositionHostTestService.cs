#if SILVERLIGHT 
using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;

namespace System.ComponentModel.Composition.Hosting
{
    public static class CompositionHostTestService
    {
        public static void SetupTestGlobalContainer(CompositionContainer container)
        {
            CompositionHost._container = null;
            CompositionHost.Initialize(container);
        }

        public static void SetupTestGlobalContainer(ComposablePartCatalog catalog)
        {
            CompositionHost._container = null;
            CompositionHost.Initialize(catalog);
        }

        public static void ClearGlobalContainer()
        {
            CompositionHost._container = null;
        }

        public static void ResetGlobalContainer()
        {
            ClearGlobalContainer();
#if !BUILDING_IN_VS
            // We can only use the default SL Deployment option while building in VS otherwise we 
            // will not have a proper Application/Deployment object setup.
            SetupTestGlobalContainer(new AssemblyCatalog(typeof(CompositionHostTestService).Assembly));
#endif
        }

        public static CompositionContainer GlobalContainer
        {
            get { return CompositionHost._container; }
        }
    }
}
#endif
