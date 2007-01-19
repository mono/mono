//
// ServiceDescriptionImporterTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.
//

using NUnit.Framework;

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Web.Services.Description;

namespace MonoTests.System.Web.Services.Description
{
	[TestFixture]
	public class ServiceDescriptionImporterTest
	{
#if NET_2_0
		CodeNamespace GenerateCodeFromWsdl (ServiceDescription sd)
		{
			ServiceDescriptionImporter imp =
				new ServiceDescriptionImporter ();
			imp.AddServiceDescription (sd, null, null);
			CodeNamespace cns = new CodeNamespace ();
			imp.Import (cns, null);
			return cns;
		}

		CodeTypeDeclaration FindTypeFrom (CodeNamespace cns, string name)
		{
			foreach (CodeTypeDeclaration td in cns.Types)
				if (td.Name == name)
					return td;
			return null;
		}

		CodeTypeDeclaration FindMemberFrom (CodeNamespace cns, string name)
		{
			foreach (CodeTypeDeclaration td in cns.Types)
				if (td.Name == name)
					return td;
			return null;
		}

		[Test]
		public void Constructor ()
		{
			ServiceDescriptionImporter imp =
				new ServiceDescriptionImporter ();
			Assert.IsTrue (imp.CodeGenerator is CSharpCodeProvider);
		}

		[Test] // wtf? no ArgumentNullException?
		public void SetNullCodeGenerator ()
		{
			ServiceDescriptionImporter imp =
				new ServiceDescriptionImporter ();
			imp.CodeGenerator = null;
		}

		[Test]
		public void GenerateNullableTypes ()
		{
			CodeNamespace cns = GenerateCodeFromWsdl (
				ServiceDescription.Read ("Test/System.Web.Services.Description/test2.wsdl"));
			CodeTypeDeclaration td = FindTypeFrom (cns, "Service");
			foreach (CodeTypeMember member in td.Members) {
				CodeMemberMethod method = member as CodeMemberMethod;
				if (method == null || method.Name != "Hola")
					continue;
				Assert.AreEqual ("System.Nullable`1", method.ReturnType.BaseType, "#1");
				Assert.AreEqual (1, method.ReturnType.TypeArguments.Count, "#2");
				Assert.AreEqual ("System.Int32", method.ReturnType.TypeArguments [0].BaseType, "#3");
				return;
			}
			Assert.Fail ("The expected member didn't appear.");
		}

		[Test]
		public void GenerateWebReferencesEmpty ()
		{
			// yuck - This causes NRE.
			//ServiceDescriptionImporter.GenerateWebReferences (
			//	new WebReferenceCollection (), null, null, null);
			ServiceDescriptionImporter.GenerateWebReferences (
				new WebReferenceCollection (),
				null,
				new CodeCompileUnit (),
				new WebReferenceOptions ());
		}
#endif
	}
}
