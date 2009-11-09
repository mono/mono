// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.AttributedModel;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace System.ComponentModel.Composition.Factories
{
    internal static partial class ContainerFactory
    {
        public static CompositionContainer Create()
        {
            return Create((ComposablePart[])null);
        }

        public static CompositionContainer Create(ComposablePartCatalog catalog)
        {
            return new CompositionContainer(catalog);
        }

        public static CompositionContainer Create(CompositionContainer parent)
        {
            return new CompositionContainer(parent);
        }

        public static CompositionContainer Create(params ComposablePart[] parts)
        {
            return Create((CompositionContainer)null, parts);
        }

        public static CompositionContainer CreateWithDefaultAttributedCatalog()
        {
            var catalog = CatalogFactory.CreateDefaultAttributed();

            return Create(catalog);            
        }

        public static CompositionContainer CreateWithAttributedCatalog(params Type[] types)
        {
            var catalog = CatalogFactory.CreateAttributed(types);

            return Create(catalog);
        }

        public static CompositionContainer CreateAttributed(params object[] parts)
        {
            var container = new CompositionContainer();
            var partsArray = new ComposablePart[parts.Length];

            for (int i = 0; i < parts.Length; i++)
            {
                Assert.IsNotInstanceOfType(parts[i], typeof(Type), "You should be using CreateWithAttributedCatalog not CreateAttributed");
                partsArray[i] = PartFactory.CreateAttributed(parts[i]);
            }

            return Create(partsArray);
        }

        public static CompositionContainer Create(CompositionContainer parent, params ComposablePart[] parts)
        {
            CompositionContainer container;
            if (parent == null)
            {
                container = new CompositionContainer();
            }
            else
            {
                container = new CompositionContainer(parent);
            }

            if (parts != null)
            {
                CompositionBatch batch = new CompositionBatch(parts, Enumerable.Empty<ComposablePart>());
                container.Compose(batch);
            }

            return container;
        }

        public static CompositionContainer Create(params MicroExport[] exports)
        {
            var part = PartFactory.CreateExporter(exports);

            return Create(part);
        }

        public static CompositionContainer Create(CompositionContainer parent, params MicroExport[] exports)
        {
            var part = PartFactory.CreateExporter(exports);

            return Create(parent, part);
        }

     
        public static CompositionContainer CreateDisposable(Action<bool> disposeCallback)
        {
            return new DisposableCompositionContainer(disposeCallback);
        }
    }
}
