// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition.Factories;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.AttributedModel;
using System.Reflection;
using Microsoft.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace System.ComponentModel.Composition.ReflectionModel
{
    [TestClass]
    public class ReflectionParameterImportDefinitionTests
    {
        [TestMethod]
        public void Constructor()
        {
            Lazy<ParameterInfo> parameter = CreateLazyParameter();
            IEnumerable<KeyValuePair<string, Type>> requiredMetadata = new KeyValuePair<string, Type>[] { new KeyValuePair<string, Type>("Foo", typeof(object)) };

            ReflectionParameterImportDefinition definition = new ReflectionParameterImportDefinition(
                parameter, "Contract", (string)null, requiredMetadata, ImportCardinality.ZeroOrMore, CreationPolicy.NonShared, null);

            Assert.AreSame(parameter, definition.ImportingLazyParameter);
            Assert.AreEqual("Contract", definition.ContractName);
            Assert.AreSame(requiredMetadata, definition.RequiredMetadata);
            Assert.AreEqual(CreationPolicy.NonShared, definition.RequiredCreationPolicy);
            Assert.AreEqual(false, definition.IsRecomposable);
            Assert.AreEqual(true, definition.IsPrerequisite);
            Assert.IsNull(((ICompositionElement)definition).Origin);
            Assert.IsNotNull(((ICompositionElement)definition).DisplayName);
        }

        [TestMethod]
        public void Constructor_WithNullRequiredMetadata()
        {
            Lazy<ParameterInfo> parameter = CreateLazyParameter();

            ReflectionParameterImportDefinition definition = new ReflectionParameterImportDefinition(
                parameter, "Contract", (string)null, null, ImportCardinality.ZeroOrMore, CreationPolicy.NonShared, null);


            Assert.IsNotNull(definition.RequiredMetadata);
            Assert.AreEqual(0, definition.RequiredMetadata.Count());
        }

        [TestMethod]
        public void SetDefinition_OriginIsSet()
        {
            Lazy<ParameterInfo> parameter = CreateLazyParameter();
            var expectedPartDefinition = PartDefinitionFactory.CreateAttributed(typeof(object));

            ReflectionParameterImportDefinition definition = new ReflectionParameterImportDefinition(
                parameter, "Contract", (string)null, null, ImportCardinality.ZeroOrMore, CreationPolicy.NonShared, expectedPartDefinition);

            Assert.AreSame(expectedPartDefinition, ((ICompositionElement)definition).Origin);
        }


        [TestMethod]
        public void ICompositionElementDisplayName_ValueAsParameter_ShouldIncludeParameterName()
        {
            var names = Expectations.GetContractNamesWithEmpty();

            foreach (var name in names)
            {
                var definition = CreateReflectionParameterImportDefinition(name);

                var e = CreateDisplayNameExpectationFromParameterName(definition, name);

                Assert.AreEqual(e, ((ICompositionElement)definition).DisplayName);
            }
        }

        [TestMethod]
        public void ICompositionElementDisplayName_ValueAsParameter_ShouldIncludeContractName()
        {
            var types = Expectations.GetTypes();

            foreach (var type in types)
            {
                var definition = CreateReflectionParameterImportDefinition(type);

                var e = CreateDisplayNameExpectationFromContractName(definition, type);

                Assert.AreEqual(e, ((ICompositionElement)definition).DisplayName);
            }
        }

        [TestMethod]
        public void ToString_ShouldReturnICompositionElementDisplayName()
        {
            var types = Expectations.GetTypes();

            foreach (var type in types)
            {
                var definition = CreateReflectionParameterImportDefinition(type);

                Assert.AreEqual(((ICompositionElement)definition).DisplayName, definition.ToString());
            }
        }


        private Lazy<ParameterInfo> CreateLazyParameter()
        {
            return typeof(SimpleConstructorInjectedObject).GetConstructors().First().GetParameters().First().AsLazy();
        }

        private static string CreateDisplayNameExpectationFromContractName(ReflectionParameterImportDefinition definition, Type type)
        {
            string contractName = AttributedModelServices.GetContractName(type);

            return String.Format("{0} (Parameter=\"\", ContractName=\"{1}\")", definition.ImportingLazyParameter.Value.Member.GetDisplayName(), contractName);
        }

        private static string CreateDisplayNameExpectationFromParameterName(ReflectionParameterImportDefinition definition, string name)
        {
            return String.Format("{0} (Parameter=\"{1}\", ContractName=\"System.String\")", definition.ImportingLazyParameter.Value.Member.GetDisplayName(), name);
        }

        private static ReflectionParameterImportDefinition CreateReflectionParameterImportDefinition(Type parameterType)
        {
            var parameter = ReflectionFactory.CreateParameter(parameterType);

            return CreateReflectionParameterImportDefinition(parameter);
        }

        private static ReflectionParameterImportDefinition CreateReflectionParameterImportDefinition(string name)
        {
            var parameter = ReflectionFactory.CreateParameter(name);

            return CreateReflectionParameterImportDefinition(parameter);
        }

        private static ReflectionParameterImportDefinition CreateReflectionParameterImportDefinition(ParameterInfo parameter)
        {
            return new ReflectionParameterImportDefinition(
                parameter.AsLazy(), AttributedModelServices.GetContractName(parameter.ParameterType), (string)null, null, ImportCardinality.ZeroOrMore, CreationPolicy.NonShared, null);
        }
    }
}
