// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition.Factories;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Reflection;
using Microsoft.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace System.ComponentModel.Composition.ReflectionModel
{
    [TestClass]
    public class ReflectionMemberExportDefinitionTests
    {
        private static ReflectionMemberExportDefinition CreateReflectionExportDefinition(LazyMemberInfo exportMember, string contractname, IDictionary<string, object> metadata)
        {
            return CreateReflectionExportDefinition(exportMember, contractname, metadata, null);
        }

        private static ReflectionMemberExportDefinition CreateReflectionExportDefinition(LazyMemberInfo exportMember, string contractname, IDictionary<string, object> metadata, ICompositionElement origin)
        {
            return (ReflectionMemberExportDefinition)ReflectionModelServices.CreateExportDefinition(
                exportMember, contractname, CreateLazyMetadata(metadata), origin);
        }

        private static Lazy<IDictionary<string, object>> CreateLazyMetadata(IDictionary<string, object> metadata)
        {
            return new Lazy<IDictionary<string, object>>(() => metadata, false);
        }

        [TestMethod]
        public void Constructor()
        {
            MemberInfo expectedMember = this.GetType();
            LazyMemberInfo expectedExportingMemberInfo = new LazyMemberInfo(expectedMember);

            string expectedContractName = "Contract";
            IDictionary<string, object> expectedMetadata = new Dictionary<string, object>();
            expectedMetadata["Key1"] = 1;
            expectedMetadata["Key2"] = "Value2";

            ReflectionMemberExportDefinition definition = CreateReflectionExportDefinition(expectedExportingMemberInfo, expectedContractName, expectedMetadata);

            Assert.AreEqual(expectedExportingMemberInfo, definition.ExportingLazyMember);
            Assert.AreSame(expectedMember, definition.ExportingLazyMember.GetAccessors()[0]);
            Assert.AreEqual(MemberTypes.TypeInfo, definition.ExportingLazyMember.MemberType);

            Assert.AreSame(expectedContractName, definition.ContractName);

            Assert.IsNotNull(definition.Metadata);
            Assert.IsTrue(definition.Metadata.Keys.SequenceEqual(expectedMetadata.Keys));
            Assert.IsTrue(definition.Metadata.Values.SequenceEqual(expectedMetadata.Values));

            Assert.IsNull(((ICompositionElement)definition).Origin);
        }

        [TestMethod]
        public void Constructor_NullMetadata()
        {
            MemberInfo expectedMember = this.GetType();
            LazyMemberInfo expectedExportingMemberInfo = new LazyMemberInfo(expectedMember);

            string expectedContractName = "Contract";

            ReflectionMemberExportDefinition definition = CreateReflectionExportDefinition(expectedExportingMemberInfo, expectedContractName, null);

            Assert.AreEqual(expectedExportingMemberInfo, definition.ExportingLazyMember);
            Assert.AreSame(expectedMember, definition.ExportingLazyMember.GetAccessors()[0]);
            Assert.AreEqual(MemberTypes.TypeInfo, definition.ExportingLazyMember.MemberType);

            Assert.AreSame(expectedContractName, definition.ContractName);

            Assert.IsNotNull(definition.Metadata);
            Assert.AreEqual(0, definition.Metadata.Count);

            Assert.IsNull(((ICompositionElement)definition).Origin);
        }

        [TestMethod]
        public void SetDefinition_OriginIsSet()
        {
            var expectedPartDefinition = PartDefinitionFactory.CreateAttributed(typeof(object));
            var exportDefinition = CreateReflectionExportDefinition(new LazyMemberInfo(this.GetType()), "ContractName", null, expectedPartDefinition);

            Assert.AreSame(expectedPartDefinition, ((ICompositionElement)exportDefinition).Origin);
        }
        
        [TestMethod]
        public void SetDefinition_PartDefinitionDoesNotContainCreationPolicy_CreationPolicyShouldNotBeInMetadata()
        {
            var expectedPartDefinition = PartDefinitionFactory.CreateAttributed(typeof(object));
            var exportDefinition = CreateReflectionExportDefinition(new LazyMemberInfo(this.GetType()), "ContractName", null);

            Assert.IsFalse(exportDefinition.Metadata.ContainsKey(CompositionConstants.PartCreationPolicyMetadataName));
        }

        [TestMethod]
        public void ICompositionElementDisplayName_ValueAsContractName_ShouldIncludeContractName()
        {
            var contractNames = Expectations.GetContractNamesWithEmpty();

            foreach (var contractName in contractNames)
            {
                if (string.IsNullOrEmpty(contractName)) continue;
                var definition = (ICompositionElement)CreateReflectionExportDefinition(new LazyMemberInfo(typeof(string)), contractName, null);

                var e = CreateDisplayNameExpectation(contractName);

                Assert.AreEqual(e, definition.DisplayName);
            }
        }

        [TestMethod]
        public void ICompositionElementDisplayName_TypeAsMember_ShouldIncludeMemberDisplayName()
        {
            var types = Expectations.GetTypes();

            foreach (var type in types)
            {
                var definition = (ICompositionElement)CreateReflectionExportDefinition(new LazyMemberInfo(type), "Contract", null);

                var e = CreateDisplayNameExpectation(type);

                Assert.AreEqual(e, definition.DisplayName);
            }
        }

        [TestMethod]
        public void ICompositionElementDisplayName_ValueAsMember_ShouldIncludeMemberDisplayName()
        {
            var members = Expectations.GetMembers();

            foreach (var member in members)
            {
                var definition = (ICompositionElement)CreateReflectionExportDefinition(new LazyMemberInfo(member), "Contract", null);

                var e = CreateDisplayNameExpectation(member);

                Assert.AreEqual(e, definition.DisplayName);
            }
        }

        [TestMethod]
        public void ToString_ShouldReturnDisplayName()
        {
            var members = Expectations.GetMembers();

            foreach (var member in members)
            {
                var definition = (ICompositionElement)CreateReflectionExportDefinition(new LazyMemberInfo(member), "Contract", null);

                Assert.AreEqual(definition.DisplayName, definition.ToString());
            }
        }

        private static string CreateDisplayNameExpectation(string contractName)
        {
            return String.Format("System.String (ContractName=\"{0}\")", contractName);
        }

        private static string CreateDisplayNameExpectation(MemberInfo member)
        {
            return String.Format("{0} (ContractName=\"Contract\")", member.GetDisplayName());
        }

    }
}
