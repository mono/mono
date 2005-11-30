//
// CodeCompilerCas.cs 
//	- CAS unit tests for System.CodeDom.Compiler.CodeCompiler
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
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace MonoCasTests.System.CodeDom.Compiler {

	class CodeCompilerTest : CodeCompiler {

		public CodeCompilerTest ()
		{
		}

		protected override string CmdArgsFromParameters (CompilerParameters options)
		{
			return String.Empty;
		}

		protected override string CompilerName {
			get { return String.Empty; }
		}

		protected override string FileExtension {
			get { return String.Empty; }
		}

		protected override void ProcessCompilerOutputLine (CompilerResults results, string line)
		{
		}

		protected override string CreateEscapedIdentifier (string value)
		{
			return String.Empty;
		}

		protected override string CreateValidIdentifier (string value)
		{
			return String.Empty;
		}

		protected override void GenerateArgumentReferenceExpression (CodeArgumentReferenceExpression e)
		{
		}

		protected override void GenerateArrayCreateExpression (CodeArrayCreateExpression e)
		{
		}

		protected override void GenerateArrayIndexerExpression (CodeArrayIndexerExpression e)
		{
		}

		protected override void GenerateAssignStatement (CodeAssignStatement e)
		{
		}

		protected override void GenerateAttachEventStatement (CodeAttachEventStatement e)
		{
		}

		protected override void GenerateAttributeDeclarationsEnd (CodeAttributeDeclarationCollection attributes)
		{
		}

		protected override void GenerateAttributeDeclarationsStart (CodeAttributeDeclarationCollection attributes)
		{
		}

		protected override void GenerateBaseReferenceExpression (CodeBaseReferenceExpression e)
		{
		}

		protected override void GenerateCastExpression (CodeCastExpression e)
		{
		}

		protected override void GenerateComment (CodeComment e)
		{
		}

		protected override void GenerateConditionStatement (CodeConditionStatement e)
		{
		}

		protected override void GenerateConstructor (CodeConstructor e, CodeTypeDeclaration c)
		{
		}

		protected override void GenerateDelegateCreateExpression (CodeDelegateCreateExpression e)
		{
		}

		protected override void GenerateDelegateInvokeExpression (CodeDelegateInvokeExpression e)
		{
		}

		protected override void GenerateEntryPointMethod (CodeEntryPointMethod e, CodeTypeDeclaration c)
		{
		}

		protected override void GenerateEvent (CodeMemberEvent e, CodeTypeDeclaration c)
		{
		}

		protected override void GenerateEventReferenceExpression (CodeEventReferenceExpression e)
		{
		}

		protected override void GenerateExpressionStatement (CodeExpressionStatement e)
		{
		}

		protected override void GenerateField (CodeMemberField e)
		{
		}

		protected override void GenerateFieldReferenceExpression (CodeFieldReferenceExpression e)
		{
		}

		protected override void GenerateGotoStatement (CodeGotoStatement e)
		{
		}

		protected override void GenerateIndexerExpression (CodeIndexerExpression e)
		{
		}

		protected override void GenerateIterationStatement (CodeIterationStatement e)
		{
		}

		protected override void GenerateLabeledStatement (CodeLabeledStatement e)
		{
		}

		protected override void GenerateLinePragmaEnd (CodeLinePragma e)
		{
		}

		protected override void GenerateLinePragmaStart (CodeLinePragma e)
		{
		}

		protected override void GenerateMethod (CodeMemberMethod e, CodeTypeDeclaration c)
		{
		}

		protected override void GenerateMethodInvokeExpression (CodeMethodInvokeExpression e)
		{
		}

		protected override void GenerateMethodReferenceExpression (CodeMethodReferenceExpression e)
		{
		}

		protected override void GenerateMethodReturnStatement (CodeMethodReturnStatement e)
		{
		}

		protected override void GenerateNamespaceEnd (CodeNamespace e)
		{
		}

		protected override void GenerateNamespaceImport (CodeNamespaceImport e)
		{
		}

		protected override void GenerateNamespaceStart (CodeNamespace e)
		{
		}

		protected override void GenerateObjectCreateExpression (CodeObjectCreateExpression e)
		{
		}

		protected override void GenerateProperty (CodeMemberProperty e, CodeTypeDeclaration c)
		{
		}

		protected override void GeneratePropertyReferenceExpression (CodePropertyReferenceExpression e)
		{
		}

		protected override void GeneratePropertySetValueReferenceExpression (CodePropertySetValueReferenceExpression e)
		{
		}

		protected override void GenerateRemoveEventStatement (CodeRemoveEventStatement e)
		{
		}

		protected override void GenerateSnippetExpression (CodeSnippetExpression e)
		{
		}

		protected override void GenerateSnippetMember (CodeSnippetTypeMember e)
		{
		}

		protected override void GenerateThisReferenceExpression (CodeThisReferenceExpression e)
		{
		}

		protected override void GenerateThrowExceptionStatement (CodeThrowExceptionStatement e)
		{
		}

		protected override void GenerateTryCatchFinallyStatement (CodeTryCatchFinallyStatement e)
		{
		}

		protected override void GenerateTypeConstructor (CodeTypeConstructor e)
		{
		}

		protected override void GenerateTypeEnd (CodeTypeDeclaration e)
		{
		}

		protected override void GenerateTypeStart (CodeTypeDeclaration e)
		{
		}

		protected override void GenerateVariableDeclarationStatement (CodeVariableDeclarationStatement e)
		{
		}

		protected override void GenerateVariableReferenceExpression (CodeVariableReferenceExpression e)
		{
		}

		protected override string GetTypeOutput (CodeTypeReference value)
		{
			return String.Empty;
		}

		protected override bool IsValidIdentifier (string value)
		{
			return true;
		}

		protected override string NullToken {
			get { return String.Empty; }
		}

		protected override void OutputType (CodeTypeReference typeRef)
		{
		}

		protected override string QuoteSnippetString (string value)
		{
			return String.Empty;
		}

		protected override bool Supports (GeneratorSupport support)
		{
			return true;
		}

		// publicity

		public CompilerResults _FromDom (CompilerParameters options, CodeCompileUnit e)
		{
			return FromDom (options, e);
		}

		public CompilerResults _FromDomBatch (CompilerParameters options, CodeCompileUnit[] ea)
		{
			return FromDomBatch (options, ea);
		}

		public CompilerResults _FromFile (CompilerParameters options, string fileName)
		{
			return FromFile (options, fileName);
		}

		public CompilerResults _FromFileBatch (CompilerParameters options, string[] fileNames)
		{
			return FromFileBatch (options, fileNames);
		}

		public CompilerResults _FromSource (CompilerParameters options, string source)
		{
			return FromSource (options, source);
		}

		public CompilerResults _FromSourceBatch (CompilerParameters options, string[] sources)
		{
			return FromSourceBatch (options, sources);
		}

		public string _GetResponseFileCmdArgs (CompilerParameters options, string cmdArgs)
		{
			return GetResponseFileCmdArgs (options, cmdArgs);
		}

		static public string _JoinStringArray (string[] sa, string separator)
		{
			return CodeCompiler.JoinStringArray (sa, separator);
		}
	}

	[TestFixture]
	[Category ("CAS")]
	public class CodeCompilerCas {

		private string filename;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			// at full trust
			filename = Path.GetTempFileName ();
		}

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		[EnvironmentPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[ExpectedException (typeof (InvalidOperationException))]
		public void FromDom_PermitOnly ()
		{
			CodeCompilerTest cc = new CodeCompilerTest ();
			cc._FromDom (new CompilerParameters (), new CodeCompileUnit ());
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void FromDom_Deny_UnmanagedCode ()
		{
			CodeCompilerTest cc = new CodeCompilerTest ();
			cc._FromDom (new CompilerParameters (), new CodeCompileUnit ());
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void FromDom_Deny_FileIO ()
		{
			CodeCompilerTest cc = new CodeCompilerTest ();
			cc._FromDom (new CompilerParameters (), new CodeCompileUnit ());
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
#if ONLY_1_1
		[Category ("NotDotNet")] // MS doesn't check for Environment under 1.x
#endif
		public void FromDom_Deny_Environment ()
		{
			CodeCompilerTest cc = new CodeCompilerTest ();
			cc._FromDom (new CompilerParameters (), new CodeCompileUnit ());
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		[EnvironmentPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[ExpectedException (typeof (InvalidOperationException))]
		public void FromDomBatch_PermitOnly ()
		{
			CodeCompilerTest cc = new CodeCompilerTest ();
			cc._FromDomBatch (new CompilerParameters (), new CodeCompileUnit[] { new CodeCompileUnit () });
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void FromDomBatch_Deny_UnmanagedCode ()
		{
			CodeCompilerTest cc = new CodeCompilerTest ();
			cc._FromDomBatch (new CompilerParameters (), new CodeCompileUnit[] { new CodeCompileUnit () });
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void FromDomBatch_Deny_FileIO ()
		{
			CodeCompilerTest cc = new CodeCompilerTest ();
			cc._FromDomBatch (new CompilerParameters (), new CodeCompileUnit[] { new CodeCompileUnit () });
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
#if ONLY_1_1
		[Category ("NotDotNet")] // MS doesn't check for Environment under 1.x
#endif
		public void FromDomBatch_Deny_Environment ()
		{
			CodeCompilerTest cc = new CodeCompilerTest ();
			cc._FromDomBatch (new CompilerParameters (), new CodeCompileUnit[] { new CodeCompileUnit () });
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		[EnvironmentPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[ExpectedException (typeof (InvalidOperationException))]
		public void FromFile_PermitOnly ()
		{
			CodeCompilerTest cc = new CodeCompilerTest ();
			cc._FromFile (new CompilerParameters (), filename);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void FromFile_Deny_UnmanagedCode ()
		{
			CodeCompilerTest cc = new CodeCompilerTest ();
			cc._FromFile (new CompilerParameters (), String.Empty);
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void FromFile_Deny_FileIO ()
		{
			CodeCompilerTest cc = new CodeCompilerTest ();
			cc._FromFile (new CompilerParameters (), filename);
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
#if ONLY_1_1
		[Category ("NotDotNet")] // MS doesn't check for Environment under 1.x
#endif
		public void FromFile_Deny_Environment ()
		{
			CodeCompilerTest cc = new CodeCompilerTest ();
			cc._FromFile (new CompilerParameters (), filename);
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		[EnvironmentPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[ExpectedException (typeof (InvalidOperationException))]
		public void FromFileBatch_PermitOnly ()
		{
			CodeCompilerTest cc = new CodeCompilerTest ();
			cc._FromFileBatch (new CompilerParameters (), new string[] { String.Empty });
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void FromFileBatch_Deny_UnmanagedCode ()
		{
			CodeCompilerTest cc = new CodeCompilerTest ();
			cc._FromFileBatch (new CompilerParameters (), new string[] { String.Empty });
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void FromFileBatch_Deny_FileIO ()
		{
			CodeCompilerTest cc = new CodeCompilerTest ();
			cc._FromFileBatch (new CompilerParameters (), new string[] { String.Empty });
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
#if ONLY_1_1
		[Category ("NotDotNet")] // MS doesn't check for Environment under 1.x
#endif
		public void FromFileBatch_Deny_Environment ()
		{
			CodeCompilerTest cc = new CodeCompilerTest ();
			cc._FromFileBatch (new CompilerParameters (), new string[] { String.Empty });
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		[EnvironmentPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[ExpectedException (typeof (InvalidOperationException))]
		public void FromSource_PermitOnly ()
		{
			CodeCompilerTest cc = new CodeCompilerTest ();
			cc._FromSource (new CompilerParameters (), String.Empty);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void FromSource_Deny_UnmanagedCode ()
		{
			CodeCompilerTest cc = new CodeCompilerTest ();
			cc._FromSource (new CompilerParameters (), String.Empty);
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void FromSource_Deny_FileIO ()
		{
			CodeCompilerTest cc = new CodeCompilerTest ();
			cc._FromSource (new CompilerParameters (), String.Empty);
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
#if ONLY_1_1
		[Category ("NotDotNet")] // MS doesn't check for Environment under 1.x
#endif
		public void FromSource_Deny_Environment ()
		{
			CodeCompilerTest cc = new CodeCompilerTest ();
			cc._FromSource (new CompilerParameters (), String.Empty);
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		[EnvironmentPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[ExpectedException (typeof (InvalidOperationException))]
		public void FromSourceBatch_PermitOnly ()
		{
			CodeCompilerTest cc = new CodeCompilerTest ();
			cc._FromSourceBatch (new CompilerParameters (), new string[] { String.Empty });
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void FromSourceBatch_Deny_UnmanagedCode ()
		{
			CodeCompilerTest cc = new CodeCompilerTest ();
			cc._FromSourceBatch (new CompilerParameters (), new string[] { String.Empty });
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void FromSourceBatch_Deny_FileIO ()
		{
			CodeCompilerTest cc = new CodeCompilerTest ();
			cc._FromSourceBatch (new CompilerParameters (), new string[] { String.Empty });
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
#if ONLY_1_1
		[Category ("NotDotNet")] // MS doesn't check for Environment under 1.x
#endif
		public void FromSourceBatch_Deny_Environment ()
		{
			CodeCompilerTest cc = new CodeCompilerTest ();
			cc._FromSourceBatch (new CompilerParameters (), new string[] { String.Empty });
		}

		[Test]
		[EnvironmentPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		public void GetResponseFileCmdArgs_PermitOnly ()
		{
			CodeCompilerTest cc = new CodeCompilerTest ();
			try {
				Assert.IsNotNull (cc._GetResponseFileCmdArgs (new CompilerParameters (), String.Empty));
			}
			catch (NotImplementedException) {
				// mono
			}
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
#if ONLY_1_1
		[Category ("NotDotNet")] // MS doesn't check for Environment under 1.x
#endif
		public void GetResponseFileCmdArgs_Deny_Environment ()
		{
			CodeCompilerTest cc = new CodeCompilerTest ();
			try {
				cc._GetResponseFileCmdArgs (new CompilerParameters (), String.Empty);
			}
			catch (NotImplementedException) {
				Assert.Ignore ("GetResponseFileCmdArgs not implemented");
			}
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void GetResponseFileCmdArgs_Deny_FileIO ()
		{
			CodeCompilerTest cc = new CodeCompilerTest ();
			try {
				cc._GetResponseFileCmdArgs (new CompilerParameters (), String.Empty);
			}
			catch (NotImplementedException) {
				Assert.Ignore ("GetResponseFileCmdArgs not implemented");
			}
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void JoinStringArray ()
		{
			string[] data = new string[] { "a", "b", "c" };
			Assert.AreEqual ("\"a\"#\"b\"#\"c\"", CodeCompilerTest._JoinStringArray (data, "#"), "JoinStringArray");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void LinkDemand_Deny_Unrestricted ()
		{
			ConstructorInfo ci = typeof (CodeCompilerTest).GetConstructor (new Type[0]);
			Assert.IsNotNull (ci, "default .ctor()");
			Assert.IsNotNull (ci.Invoke (null), "invoke");
		}
	}
}
