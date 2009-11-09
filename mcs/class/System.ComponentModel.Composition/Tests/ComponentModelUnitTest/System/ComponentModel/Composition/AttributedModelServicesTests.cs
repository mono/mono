// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition.Diagnostics;
using System.ComponentModel.Composition.Factories;
using System.Diagnostics;
using System.Linq;
using System.UnitTesting;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.ComponentModel.Composition.Extensibility;

namespace System.ComponentModel.Composition.AttributedModel
{
    [TestClass]
    public class AttributedModelServicesTests
    {
        [TestMethod]
        public void CreatePartDefinition1_NullAsType_ShouldThrowArgumentNull()
        {
            var origin = ElementFactory.Create();

            ExceptionAssert.ThrowsArgumentNull("type", () =>
            {
                AttributedModelServices.CreatePartDefinition((Type)null, origin);
            });
        }

        [TestMethod]
        public void CreatePartDefinition2_NullAsType_ShouldThrowArgumentNull()
        {
            var origin = ElementFactory.Create();

            ExceptionAssert.ThrowsArgumentNull("type", () =>
            {
                AttributedModelServices.CreatePartDefinition((Type)null, origin, false);
            });
        }

#if !SILVERLIGHT

        [TestMethod]
        public void CreatePartDefinition2_TypeMarkedWithPartNotDiscoverableAttribute_ShouldTraceInformation()
        {
            var types = GetTypesMarkedWithPartNotDiscoverableAttribute();

            foreach (Type type in types)
            {
                using (TraceContext context = new TraceContext(SourceLevels.Information))
                {
                    AttributedModelServices.CreatePartDefinition(type, null, true);

                    Assert.IsNotNull(context.LastTraceEvent);
                    Assert.AreEqual(context.LastTraceEvent.EventType, TraceEventType.Information);
                    Assert.AreEqual(context.LastTraceEvent.Id, TraceId.Discovery_DefinitionMarkedWithPartNotDiscoverableAttribute);
                }
            }
        }

        [TestMethod]
        public void CreatePartDefinition2_OpenGenericType_ShouldTraceInformation()
        {
            var types = GetOpenGenericTypes();

            foreach (Type type in types)
            {
                using (TraceContext context = new TraceContext(SourceLevels.Information))
                {
                    AttributedModelServices.CreatePartDefinition(type, null, true);

                    Assert.IsNotNull(context.LastTraceEvent);
                    Assert.AreEqual(context.LastTraceEvent.EventType, TraceEventType.Information);
                    Assert.AreEqual(context.LastTraceEvent.Id, TraceId.Discovery_DefinitionContainsGenericParameters);
                }
            }
        }

        [TestMethod]
        public void CreatePartDefinition2_TypeWithNoExports_ShouldTraceInformation()
        {
            var types = GetTypesWithNoExports();

            foreach (Type type in types)
            {
                using (TraceContext context = new TraceContext(SourceLevels.Information))
                {
                    var result = AttributedModelServices.CreatePartDefinition(type, null, true);

                    Assert.IsNotNull(context.LastTraceEvent);
                    Assert.AreEqual(context.LastTraceEvent.EventType, TraceEventType.Information);
                    Assert.AreEqual(context.LastTraceEvent.Id, TraceId.Discovery_DefinitionContainsNoExports);
                }
            }
        }

        private static IEnumerable<Type> GetTypesMarkedWithPartNotDiscoverableAttribute()
        {
            yield return typeof(ClassMarkedWithPartNotDiscoverableAttribute);
            yield return typeof(ClassMarkedWithPartNotDiscoverableAttribute<>);
        }

        private static IEnumerable<Type> GetOpenGenericTypes()
        {
            yield return typeof(OpenGenericType<>);
        }

        private static IEnumerable<Type> GetTypesWithNoExports()
        {
            yield return typeof(ClassWithNoExports);
            yield return typeof(ClassWithOnlyFieldImport);
            yield return typeof(ClassWithOnlyPropertyImport);
            yield return typeof(ClassWithOnlyParameterImport);
        }

        public class ClassWithNoExports
        {
        }

        public class ClassWithOnlyFieldImport
        {
            [Import]
            public string Field;
        }

        public class ClassWithOnlyPropertyImport
        {
            [Import]
            public string Property
            {
                get;
                set;
            }
        }

        public class ClassWithOnlyParameterImport
        {
            [ImportingConstructor]
            public ClassWithOnlyParameterImport(string parameter)
            {
            }
        }

        public class OpenGenericType<T>
        {
        }

        [PartNotDiscoverable]
        public class ClassMarkedWithPartNotDiscoverableAttribute
        {
        }

        [PartNotDiscoverable]
        public class ClassMarkedWithPartNotDiscoverableAttribute<T>
        {
        }
#endif

    }
}
