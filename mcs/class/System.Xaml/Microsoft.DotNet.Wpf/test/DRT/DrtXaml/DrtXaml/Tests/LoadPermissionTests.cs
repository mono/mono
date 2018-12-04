using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Permissions;
using System.Xaml;
using System.Xaml.Permissions;
using DRT;
using DrtXaml.XamlTestFramework;

namespace DrtXaml.Tests
{
    [TestClass]
    sealed class LoadPermissionTests : XamlTestSuite
    {
        public LoadPermissionTests()
            : base("LoadPermissionTests")
        {
        }

        public override DrtTest[] PrepareTests()
        {
            DrtTest[] tests = DrtTestFinder.FindTests(this);
            return tests;
        }

        [TestMethod]
        public void Unrestricted()
        {
            VerifyInvariants(s_UnrestrictedPerm, isUnrestricted : true);

            XamlAccessLevel assemblyAccess = XamlAccessLevel.AssemblyAccessTo(typeof(LoadPermissionTests).Assembly);
            Assert.IsTrue(s_UnrestrictedPerm.Includes(assemblyAccess));
        }

        [TestMethod]
        public void Empty()
        {
            VerifyInvariants(s_EmptyPerm);
            XamlLoadPermission emptyPerm = new XamlLoadPermission(new XamlAccessLevel[0]);
            VerifyInvariants(emptyPerm);
            Assert.AreEqual(s_EmptyPerm, emptyPerm);

            XamlAccessLevel assemblyAccess = XamlAccessLevel.AssemblyAccessTo(typeof(LoadPermissionTests).Assembly);
            Assert.IsFalse(s_EmptyPerm.Includes(assemblyAccess));
        }

        [TestMethod]
        public void SingleAssembly()
        {
            XamlAccessLevel assemblyAccess = XamlAccessLevel.AssemblyAccessTo(typeof(LoadPermissionTests).Assembly);
            XamlLoadPermission assemblyPerm = new XamlLoadPermission(assemblyAccess);
            VerifyInvariants(assemblyPerm);

            XamlLoadPermission testPerm, intersect, union;
            
            // Identical permission
            XamlAccessLevel sameAssemblyAccess = XamlAccessLevel.AssemblyAccessTo(typeof(LoadPermissionTests).Assembly);
            testPerm = new XamlLoadPermission(sameAssemblyAccess);
            VerifyInvariants(testPerm);
            Assert.AreEqual(assemblyPerm, testPerm);
            Assert.IsTrue(testPerm.IsSubsetOf(assemblyPerm));
            Assert.IsTrue(assemblyPerm.IsSubsetOf(testPerm));
            intersect = (XamlLoadPermission)testPerm.Intersect(assemblyPerm);
            Assert.AreEqual(assemblyPerm, intersect);
            union = (XamlLoadPermission)testPerm.Union(assemblyPerm);
            Assert.AreEqual(assemblyPerm, union);
            Assert.IsTrue(testPerm.Includes(sameAssemblyAccess));

            // Type permission to same assembly
            XamlAccessLevel typeAccess = XamlAccessLevel.PrivateAccessTo(typeof(LoadPermissionTests));
            testPerm = new XamlLoadPermission(typeAccess);
            VerifyInvariants(testPerm);
            Assert.AreNotEqual(assemblyPerm, testPerm);
            Assert.IsFalse(testPerm.IsSubsetOf(assemblyPerm));
            Assert.IsTrue(assemblyPerm.IsSubsetOf(testPerm));
            intersect = (XamlLoadPermission)testPerm.Intersect(assemblyPerm);
            Assert.AreEqual(assemblyPerm, intersect);
            union = (XamlLoadPermission)testPerm.Union(assemblyPerm);
            Assert.AreEqual(testPerm, union);
            Assert.IsTrue(testPerm.Includes(sameAssemblyAccess));
            Assert.IsTrue(testPerm.Includes(typeAccess));

            // Assembly permission on different assembly
            XamlAccessLevel diffAssemblyAccess = XamlAccessLevel.AssemblyAccessTo(typeof(string).Assembly);
            testPerm = new XamlLoadPermission(diffAssemblyAccess);
            VerifyInvariants(testPerm);
            Assert.AreNotEqual(assemblyPerm, testPerm);
            Assert.IsFalse(testPerm.IsSubsetOf(assemblyPerm));
            Assert.IsFalse(assemblyPerm.IsSubsetOf(testPerm));
            intersect = (XamlLoadPermission)testPerm.Intersect(assemblyPerm);
            Assert.AreEqual(s_EmptyPerm, intersect);
            union = (XamlLoadPermission)testPerm.Union(assemblyPerm);
            Assert.IsTrue(testPerm.IsSubsetOf(union));
            Assert.IsTrue(assemblyPerm.IsSubsetOf(union));
            Assert.IsFalse(testPerm.Includes(sameAssemblyAccess));
            Assert.IsFalse(testPerm.Includes(typeAccess));
        }

        [TestMethod]
        public void SingleType()
        {
            XamlAccessLevel typeAccess = XamlAccessLevel.PrivateAccessTo(typeof(LoadPermissionTests));
            XamlLoadPermission typePerm = new XamlLoadPermission(typeAccess);
            VerifyInvariants(typePerm);

            XamlLoadPermission testPerm, intersect, union;
            
            // Identical permission
            XamlAccessLevel sameTypeAccess = XamlAccessLevel.PrivateAccessTo(typeof(LoadPermissionTests));
            testPerm = new XamlLoadPermission(sameTypeAccess);
            VerifyInvariants(testPerm);
            Assert.AreEqual(typePerm, testPerm);
            Assert.IsTrue(testPerm.IsSubsetOf(typePerm));
            Assert.IsTrue(typePerm.IsSubsetOf(testPerm));
            intersect = (XamlLoadPermission)testPerm.Intersect(typePerm);
            Assert.AreEqual(typePerm, intersect);
            union = (XamlLoadPermission)testPerm.Union(typePerm);
            Assert.AreEqual(typePerm, union);

            // Assembly permission to the same assembly
            XamlAccessLevel assemblyAccess = XamlAccessLevel.AssemblyAccessTo(typeof(LoadPermissionTests).Assembly);
            testPerm = new XamlLoadPermission(assemblyAccess);
            VerifyInvariants(testPerm);
            Assert.AreNotEqual(typePerm, testPerm);
            Assert.IsTrue(testPerm.IsSubsetOf(typePerm));
            Assert.IsFalse(typePerm.IsSubsetOf(testPerm));
            intersect = (XamlLoadPermission)testPerm.Intersect(typePerm);
            Assert.AreEqual(testPerm, intersect);
            union = (XamlLoadPermission)testPerm.Union(typePerm);
            Assert.AreEqual(typePerm, union);

            // Type permission on different type in same assembly
            XamlAccessLevel sameAsmTypeAccess = XamlAccessLevel.PrivateAccessTo(typeof(SchemaTests));
            testPerm = new XamlLoadPermission(sameAsmTypeAccess);
            VerifyInvariants(testPerm);
            Assert.AreNotEqual(typePerm, testPerm);
            Assert.IsFalse(testPerm.IsSubsetOf(typePerm));
            Assert.IsFalse(typePerm.IsSubsetOf(testPerm));
            intersect = (XamlLoadPermission)testPerm.Intersect(typePerm);
            XamlLoadPermission assemblyPerm = new XamlLoadPermission(assemblyAccess);
            Assert.AreEqual(assemblyPerm, intersect);
            union = (XamlLoadPermission)testPerm.Union(typePerm);
            Assert.IsTrue(testPerm.IsSubsetOf(union));
            Assert.IsTrue(typePerm.IsSubsetOf(union));
            Assert.IsTrue(assemblyPerm.IsSubsetOf(union));

            // Type permission in different assembly
            XamlAccessLevel diffTypeAccess = XamlAccessLevel.PrivateAccessTo(typeof(string));
            testPerm = new XamlLoadPermission(diffTypeAccess);
            VerifyInvariants(testPerm);
            Assert.AreNotEqual(typePerm, testPerm);
            Assert.IsFalse(testPerm.IsSubsetOf(typePerm));
            Assert.IsFalse(typePerm.IsSubsetOf(testPerm));
            intersect = (XamlLoadPermission)testPerm.Intersect(typePerm);
            Assert.AreEqual(s_EmptyPerm, intersect);
            union = (XamlLoadPermission)testPerm.Union(typePerm);
            Assert.IsTrue(testPerm.IsSubsetOf(union));
            Assert.IsTrue(typePerm.IsSubsetOf(union));
        }

        [TestMethod]
        public void AccessLevelCtorsPositive()
        {
            XamlAccessLevel byRef = XamlAccessLevel.AssemblyAccessTo(typeof(LoadPermissionTests).Assembly);
            XamlAccessLevel byName = XamlAccessLevel.AssemblyAccessTo(typeof(LoadPermissionTests).Assembly.GetName());
            Assert.AreEqual(new XamlLoadPermission(byRef), new XamlLoadPermission(byName));

            byRef = XamlAccessLevel.PrivateAccessTo(typeof(LoadPermissionTests));
            byName = XamlAccessLevel.PrivateAccessTo(typeof(LoadPermissionTests).AssemblyQualifiedName);
            Assert.AreEqual(new XamlLoadPermission(byRef), new XamlLoadPermission(byName));
        }

        [TestMethod, TestExpectedException(typeof(ArgumentException))]
        public void AccessLevelCtorUnqualifiedAssemblyName()
        {
            XamlAccessLevel.AssemblyAccessTo(new AssemblyName("DRTXaml"));
        }

        [TestMethod, TestExpectedException(typeof(ArgumentException))]
        public void AccessLevelCtorUnqualifiedTypeName()
        {
            XamlAccessLevel.PrivateAccessTo(typeof(LoadPermissionTests).FullName);
        }

        private void VerifyInvariants(XamlLoadPermission original, bool isUnrestricted = false)
        {
            Assert.AreEqual(original, original);
            Assert.AreEqual(isUnrestricted, original.IsUnrestricted());

            XamlLoadPermission copy = (XamlLoadPermission)original.Copy();
            Assert.AreEqual(original, copy);
            Assert.AreEqual(original.IsUnrestricted(), copy.IsUnrestricted());
            Assert.IsTrue(original.IsSubsetOf(copy));
            Assert.IsTrue(copy.IsSubsetOf(original));

            XamlLoadPermission xmlCopy = new XamlLoadPermission(PermissionState.None);
            xmlCopy.FromXml(original.ToXml());
            Assert.AreEqual(original, xmlCopy);

            var intersect = (XamlLoadPermission)original.Intersect(copy);
            Assert.AreEqual(original, intersect);
            var union = (XamlLoadPermission)original.Union(copy);
            Assert.AreEqual(original, union);

            intersect = (XamlLoadPermission)original.Intersect(s_EmptyPerm);
            Assert.AreEqual(s_EmptyPerm, intersect);
            union = (XamlLoadPermission)original.Union(s_EmptyPerm);
            Assert.AreEqual(original, union);

            intersect = (XamlLoadPermission)original.Intersect(s_UnrestrictedPerm);
            Assert.AreEqual(original, intersect);
            union = (XamlLoadPermission)original.Union(s_UnrestrictedPerm);
            Assert.AreEqual(s_UnrestrictedPerm, union);
        }

        static XamlLoadPermission s_EmptyPerm = new XamlLoadPermission(PermissionState.None);
        static XamlLoadPermission s_UnrestrictedPerm = new XamlLoadPermission(PermissionState.Unrestricted);
    }
}
