// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition.Factories;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ComponentModel.Composition
{
    [TestClass]
    public class CompositionContainerExtensibilityTests
    {
        [TestMethod]
        public void Dispose_DoesNotThrow()
        {
            var container = CreateCustomCompositionContainer();
            container.Dispose();
        }

        [TestMethod]
        public void DerivedCompositionContainer_CanExportItself()
        {
            var container = CreateCustomCompositionContainer();
            container.AddAndComposeExportedValue<CustomCompositionContainer>(container);

            Assert.AreSame(container, container.GetExportedValue<CustomCompositionContainer>());
        }

        [TestMethod]
        public void ICompositionService_CanBeExported()
        {
            var container = CreateCustomCompositionContainer();
            container.AddAndComposeExportedValue<ICompositionService>(container);

            Assert.AreSame(container, container.GetExportedValue<ICompositionService>());
        }

        [TestMethod]
        public void CompositionContainer_CanBeExported()
        {
            var container = CreateCustomCompositionContainer();
            container.AddAndComposeExportedValue<CompositionContainer>(container);

            Assert.AreSame(container, container.GetExportedValue<CompositionContainer>());
        }

        private CustomCompositionContainer CreateCustomCompositionContainer()
        {
            return new CustomCompositionContainer();
        }

        // Type needs to be public otherwise container.GetExportedValue<CustomCompositionContainer> 
        // fails on Silverlight because it cannot construct a Lazy<T,M> factory. 
        public class CustomCompositionContainer : CompositionContainer
        {
        }
    }
}
