//
// CodeDomProviderCas.cs 
//	- CAS unit tests for System.CodeDom.Compiler.CodeDomProvider
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using NUnit.Framework;

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace MonoCasTests.System.CodeDom.Compiler {

	class CodeDomProviderTest: CodeDomProvider {

		public CodeDomProviderTest ()
		{
		}

		public override ICodeCompiler CreateCompiler ()
		{
			return null;
		}

		public override ICodeGenerator CreateGenerator ()
		{
			return null;
		}
	}

	[TestFixture]
	[Category ("CAS")]
	public class CodeDomProviderCas {

		private StringWriter writer;
		private CodeDomProviderTest cdp;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			// at full trust
			writer = new StringWriter ();
			cdp = new CodeDomProviderTest ();
		}

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Defaults ()
		{
			cdp = new CodeDomProviderTest (); // execute ctor not a full trust
			Assert.AreEqual (String.Empty, cdp.FileExtension, "FileExtension");
			Assert.AreEqual (LanguageOptions.None, cdp.LanguageOptions, "LanguageOptions");
			Assert.IsNull (cdp.CreateCompiler (), "CreateCompiler");
			Assert.IsNull (cdp.CreateGenerator (), "CreateGenerator");
			Assert.IsNull (cdp.CreateGenerator (String.Empty), "CreateGenerator(string)");
			Assert.IsNull (cdp.CreateGenerator (writer), "CreateGenerator(TextWriter)");
			Assert.IsNull (cdp.CreateParser (), "CreateParser()");
			Assert.IsNotNull (cdp.GetConverter (typeof (string)), "GetConverter");
#if NET_2_0
			Assert.IsNotNull (CodeDomProvider.GetAllCompilerInfo (), "GetAllCompilerInfo");

			// mono returns null (missing config?)
			CodeDomProvider.GetCompilerInfo ("cs"); 
			CodeDomProvider.GetLanguageFromExtension ("cs");

			Assert.IsFalse (CodeDomProvider.IsDefinedExtension (String.Empty), "String.Empty");
			Assert.IsFalse (CodeDomProvider.IsDefinedLanguage (String.Empty), "String.Empty");
#endif
		}

#if NET_2_0
		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (NotImplementedException))]
		public void CompileAssemblyFromDom_Deny_Unrestricted ()
		{
			cdp.CompileAssemblyFromDom (null, null);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (NotImplementedException))]
		public void CompileAssemblyFromFile_Deny_Unrestricted ()
		{
			cdp.CompileAssemblyFromFile (null, null);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (NotImplementedException))]
		public void CompileAssemblyFromSource_Deny_Unrestricted ()
		{
			cdp.CompileAssemblyFromSource (null, null);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (NotImplementedException))]
		public void CreateEscapedIdentifier_Deny_Unrestricted ()
		{
			cdp.CreateEscapedIdentifier (null);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (NotImplementedException))]
		public void CreateValidIdentifier_Deny_Unrestricted ()
		{
			cdp.CreateValidIdentifier (null);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (NotImplementedException))]
		public void GenerateCodeFromCompileUnit_Deny_Unrestricted ()
		{
			cdp.GenerateCodeFromCompileUnit (null, writer, null);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (NotImplementedException))]
		public void GenerateCodeFromExpression_Deny_Unrestricted ()
		{
			cdp.GenerateCodeFromExpression (null, writer, null);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (NotImplementedException))]
		public void GenerateCodeFromMember_Deny_Unrestricted ()
		{
			cdp.GenerateCodeFromMember (null, writer, null);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (NotImplementedException))]
		public void GenerateCodeFromNamespace_Deny_Unrestricted ()
		{
			cdp.GenerateCodeFromNamespace (null, writer, null);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (NotImplementedException))]
		public void GenerateCodeFromStatement_Deny_Unrestricted ()
		{
			cdp.GenerateCodeFromStatement (null, writer, null);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (NotImplementedException))]
		public void GenerateCodeFromType_Deny_Unrestricted ()
		{
			cdp.GenerateCodeFromType (null, writer, null);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (NotImplementedException))]
		public void GetTypeOutput_Deny_Unrestricted ()
		{
			cdp.GetTypeOutput (new CodeTypeReference ());
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (NotImplementedException))]
		public void IsValidIdentifier_Deny_Unrestricted ()
		{
			cdp.IsValidIdentifier (null);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (NotImplementedException))]
		public void Parse_Deny_Unrestricted ()
		{
			cdp.Parse (null);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (NotImplementedException))]
		public void Supports_Deny_Unrestricted ()
		{
			cdp.Supports (GeneratorSupport.ArraysOfArrays);
		}

		// static methods

		[Test]
		public void CreateProvider_Allow_Everything ()
		{
			CodeDomProvider.CreateProvider ("cs");
			// returns null on mono (missing config?)
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "MONO")]
		[ExpectedException (typeof (SecurityException))]
		[Category ("NotWorking")] // mono returns null not an instance
		public void CreateProvider_Deny_Anything ()
		{
			CodeDomProvider cdp = CodeDomProvider.CreateProvider ("cs");
			// requires full trust (i.e. unrestricted permission set)
		}

		[Test]
		public void LinkDemand_StaticMethods_Allow_Everything ()
		{
			object[] language = new object[1] { "cs" };
			object[] empty = new object[1] { String.Empty };

			MethodInfo mi = typeof (CodeDomProvider).GetMethod ("CreateProvider");
			mi.Invoke (null, language); // returns null on mono (missing config?)
			
			mi = typeof (CodeDomProvider).GetMethod ("GetAllCompilerInfo");
			Assert.IsNotNull (mi.Invoke (null, null), "GetAllCompilerInfo()");

			mi = typeof (CodeDomProvider).GetMethod ("GetCompilerInfo");
			mi.Invoke (null, language); // returns null on mono (missing config?)

			mi = typeof (CodeDomProvider).GetMethod ("GetLanguageFromExtension");
			mi.Invoke (null, language); // returns null on mono (missing config?)

			mi = typeof (CodeDomProvider).GetMethod ("IsDefinedExtension");
			Assert.IsFalse ((bool)mi.Invoke (null, empty), "IsDefinedExtension('')");

			mi = typeof (CodeDomProvider).GetMethod ("IsDefinedLanguage");
			Assert.IsFalse ((bool)mi.Invoke (null, empty), "IsDefinedLanguage('')");
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "MONO")]
		[ExpectedException (typeof (SecurityException))]
		public void LinkDemand_CreateProvider_Deny_Anything ()
		{
			MethodInfo mi = typeof (CodeDomProvider).GetMethod ("CreateProvider");
			Assert.IsNotNull (mi, "CreateProvider");
			Assert.IsNotNull (mi.Invoke (null, new object[1] { "cs" }), "CreateProvider(cs)");
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "MONO")]
		[ExpectedException (typeof (SecurityException))]
		public void LinkDemand_GetAllCompilerInfo_Deny_Anything ()
		{
			MethodInfo mi = typeof (CodeDomProvider).GetMethod ("GetAllCompilerInfo");
			Assert.IsNotNull (mi, "GetAllCompilerInfo");
			Assert.IsNotNull (mi.Invoke (null, null), "GetAllCompilerInfo()");
			// requires full trust (i.e. unrestricted permission set)
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "MONO")]
		[ExpectedException (typeof (SecurityException))]
		public void LinkDemand_GetCompilerInfo_Deny_Anything ()
		{
			MethodInfo mi = typeof (CodeDomProvider).GetMethod ("GetCompilerInfo");
			Assert.IsNotNull (mi, "GetCompilerInfo");
			Assert.IsNotNull (mi.Invoke (null, new object[1] { "cs" }), "GetCompilerInfo(cs)");
			// requires full trust (i.e. unrestricted permission set)
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "MONO")]
		[ExpectedException (typeof (SecurityException))]
		public void LinkDemand_GetLanguageFromExtension_Deny_Anything ()
		{
			MethodInfo mi = typeof (CodeDomProvider).GetMethod ("GetLanguageFromExtension");
			Assert.IsNotNull (mi, "GetLanguageFromExtension");
			Assert.IsNotNull (mi.Invoke (null, new object[1] { null }), "invoke (null)");
			// requires full trust (i.e. unrestricted permission set)
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "MONO")]
		[ExpectedException (typeof (SecurityException))]
		public void LinkDemand_IsDefinedExtension_Deny_Anything ()
		{
			MethodInfo mi = mi = typeof (CodeDomProvider).GetMethod ("IsDefinedExtension");
			Assert.IsNotNull (mi, "IsDefinedExtension");
			Assert.IsFalse ((bool) mi.Invoke (null, new object[1] { String.Empty }), "IsDefinedExtension('')");
			// requires full trust (i.e. unrestricted permission set)
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "MONO")]
		[ExpectedException (typeof (SecurityException))]
		public void LinkDemand_IsDefinedLanguage_Deny_Anything ()
		{
			MethodInfo mi = mi = typeof (CodeDomProvider).GetMethod ("IsDefinedLanguage");
			Assert.IsNotNull (mi, "IsDefinedLanguage");
			Assert.IsFalse ((bool) mi.Invoke (null, new object[1] { String.Empty }), "IsDefinedLanguage('')");
			// requires full trust (i.e. unrestricted permission set)
		}
#endif
		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void LinkDemand_Deny_Unrestricted ()
		{
			ConstructorInfo ci = typeof (CodeDomProviderTest).GetConstructor (new Type[0]);
			Assert.IsNotNull (ci, "default .ctor()");
			Assert.IsNotNull (ci.Invoke (null), "invoke");
		}
	}
}
