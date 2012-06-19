//
// StronglyTypedResourceBuilderNamespaceTests.cs - tests the GeneratedCodeNamespace
// and ResourcesNamespace params of the main Create overload
// 
// Author:
//  	Gary Barnett (2012)
// 
// Copyright (C) Gary Barnett (2012)
//

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
#if NET_2_0

using NUnit.Framework;
using System;
using System.Resources.Tools;
using System.CodeDom;
using Microsoft.CSharp;
using System.Collections.Generic;

namespace MonoTests.System.Resources.Tools {
	[TestFixture]
	public class StronglyTypedResourceBuilderNamespaceTests	{
		static string [] keywords = {"abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", 
					"checked", "class", "const", "continue", "decimal", "default", "delegate", 
					"do", "double", "else", "enum", "event", "explicit", "extern", "FALSE", 
					"false", "finally", "fixed", "float", "for", "foreach", "goto", "if", 
					"implicit", "in", "int", "interface", "internal", "is", "lock", "long", 
					"namespace", "new", "null", "object", "operator", "out", "override", "params", 
					"private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", 
					"short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", 
					"throw", "TRUE", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", 
					"ushort", "using", "virtual", "volatile", "void", "while" };
		static char [] specialChars = { ' ', '\u00A0', '.', ',', ';', '|', '~', '@', '#', '%', '^', '&', 
					'*', '+', '-', '/', '\\', '<', '>', '?', '[', ']', '(', ')', '{', 
					'}', '\"', '\'', ':', '!'};
		CSharpCodeProvider provider = new CSharpCodeProvider ();
		Dictionary<string, object> testResources;
		
		[SetUp]
		public void Setup ()
		{
			testResources = new Dictionary<string, object> ();
			testResources.Add ("akey", String.Empty);
		}
		
		[Test]
		public void GeneratedCodeNamespaceEmpty ()
		{
			// empty namespace allowed
			string [] unmatchables;
			string input, expected;
			CodeCompileUnit ccu;
			
			input = String.Empty;
			
			expected = String.Empty;
			
			ccu = StronglyTypedResourceBuilder.Create (testResources,
								"TestClass",
								input,
								"TestResourceNameSpace",
								provider,
								true,
								out unmatchables);
			
			Assert.AreEqual (expected,ccu.Namespaces [0].Name);
		}
		
		[Test]
		public void GeneratedCodeNamespaceNull ()
		{
			// null should be replaced with String.Empty
			string [] unmatchables;
			string input, expected;
			CodeCompileUnit ccu;
			
			input = null;
			
			expected = String.Empty;
			
			ccu = StronglyTypedResourceBuilder.Create (testResources,
								"TestClass",
								input,
								"TestResourceNameSpace",
								provider,
								true,
								out unmatchables);
			
			Assert.AreEqual (expected,ccu.Namespaces [0].Name);
		}
		
		[Test]
		public void GeneratedCodeNamespaceProviderInvalidIdentifiersOK ()
		{
			// identifiers which are still invalid after CreateValidIdentifier called allowed through in .NET framework
			string [] unmatchables;
			string input, output, expected;
			CodeCompileUnit ccu;
			
			input = "te$st";
			expected = "te$st";
			
			ccu = StronglyTypedResourceBuilder.Create (testResources,
								"TestClass",
								input,
								"TestResourcesNameSpace",
								provider,
								true,
								out unmatchables);
			
			output = ccu.Namespaces [0].Name;
			
			Assert.AreEqual (expected,output);
		}
		
		[Test]
		public void GeneratedCodeNamespaceProviderKeywords ()
		{
			string expected;
			string [] unmatchables;
			CodeCompileUnit ccu;

			foreach (string input in keywords) {
				ccu = StronglyTypedResourceBuilder.Create (testResources,
									"TestClass",
									input,
									"TestResourcesNameSpace",
									provider,
									true,
									out unmatchables);
				
				expected = provider.CreateValidIdentifier (input);
				
				Assert.AreEqual (expected,ccu.Namespaces [0].Name);
			}
		}
		
		[Test]
		public void GeneratedCodeNamespaceProviderKeywordsMultipart ()
		{
			// .NET framework does not check individiual elements of multipart namespace
			string expected, input;
			string [] unmatchables;
			CodeCompileUnit ccu;
			
			foreach (string word in keywords) {
				
				input = "Primary." + word;
				
				ccu = StronglyTypedResourceBuilder.Create (testResources,
									"TestClass",
									input,
									"TestResourcesNameSpace",
									provider,
									true,
									out unmatchables);
				
				expected = provider.CreateValidIdentifier (input);
				
				Assert.AreEqual (expected,ccu.Namespaces [0].Name);
			}
		}
		
		[Test]
		public void GeneratedCodeNamespaceSpecialChars ()
		{
			// invalid chars replaced with _ noting (. and :) are allowed by .NET framework
			string [] unmatchables;
			string input, output, expected;
			
			CodeCompileUnit ccu;
			
			foreach (char c in specialChars) {
				input = "test" + c.ToString ();
				
				if (c == '.' || c == ':')
					expected = input;
				else
					expected = StronglyTypedResourceBuilder.VerifyResourceName(input, provider);
			
				ccu = StronglyTypedResourceBuilder.Create (testResources,
									"TestClass",
									input,
									"TestResourcesNameSpace",
									provider,
									true,
									out unmatchables);
				
				output = ccu.Namespaces [0].Name;
				
				Assert.AreEqual (expected,output);
			}
		}
		
		[Test]
		public void ResourcesNamespaceEmpty ()
		{
			// when ResourcesNamespace is String.Empty no namespace is used
			string [] unmatchables;
			string input, output, expected;
			CodeCompileUnit ccu;
			CodeMemberProperty resourceManager;
			CodeVariableDeclarationStatement cvds;
			
			input = String.Empty;
			
			expected = "TestClass";
			
			ccu = StronglyTypedResourceBuilder.Create (testResources,
								"TestClass",
								"TestNameSpace",
								input,
								provider,
								true,
								out unmatchables);
			
			resourceManager = StronglyTypedResourceBuilderCodeDomTest.Get<CodeMemberProperty> (
													"ResourceManager", ccu);
			cvds = ((CodeVariableDeclarationStatement)((CodeConditionStatement)resourceManager.GetStatements [0]).TrueStatements [0]);
			output  = ((CodePrimitiveExpression)((CodeObjectCreateExpression)cvds.InitExpression).Parameters [0]).Value.ToString ();
			
			Assert.AreEqual (expected,output);
		}
		
		[Test]
		public void ResourcesNamespaceNull ()
		{
			// when ResourcesNamespace is null generatedCodeNamespace is used in its place
			string[] unmatchables;
			string input, output, expected;
			CodeCompileUnit ccu;
			CodeMemberProperty resourceManager;
			CodeVariableDeclarationStatement cvds;
			
			input = null;
			
			expected = "TestNameSpace.TestClass";
			
			ccu = StronglyTypedResourceBuilder.Create (testResources,
								"TestClass",
								"TestNameSpace",
								input,
								provider,
								true,
								out unmatchables);
			
			resourceManager = StronglyTypedResourceBuilderCodeDomTest.Get<CodeMemberProperty> (
													"ResourceManager", ccu);
			cvds = ((CodeVariableDeclarationStatement)((CodeConditionStatement)resourceManager.GetStatements [0]).TrueStatements [0]);
			output  = ((CodePrimitiveExpression)((CodeObjectCreateExpression)cvds.InitExpression).Parameters [0]).Value.ToString ();
			
			Assert.AreEqual (expected,output);
		}

		[Test]
		public void ResourcesNamespaceProviderKeywords ()
		{
			// not validated against provider keywords in .net framework
			string output,expected;
			string [] unmatchables;
			CodeCompileUnit ccu;
			CodeMemberProperty resourceManager;
			CodeVariableDeclarationStatement cvds;
			
			foreach (string input in keywords) {
				ccu = StronglyTypedResourceBuilder.Create (testResources,
									"TestClass",
									"TestNamespace",
									input,
									provider,
									true,
									out unmatchables);

				expected = input + ".TestClass";
				resourceManager = StronglyTypedResourceBuilderCodeDomTest.Get<CodeMemberProperty> (
													"ResourceManager", ccu);
				cvds = ((CodeVariableDeclarationStatement)((CodeConditionStatement)resourceManager.GetStatements [0]).TrueStatements [0]);
				output  = ((CodePrimitiveExpression)((CodeObjectCreateExpression)cvds.InitExpression).Parameters [0]).Value.ToString ();
				
				Assert.AreEqual (expected,output);
			}
		}
		
		[Test]
		public void ResourcesNamespaceSpecialChars ()
		{
			// ResourcesNamespace doesnt seem to be validated at all in .NET framework
			string [] unmatchables;
			string input, output, expected;
			CodeCompileUnit ccu;
			CodeMemberProperty resourceManager;
			CodeVariableDeclarationStatement cvds;
			
			foreach (char c in specialChars) {
				input = "test" + c.ToString ();
				
				expected = input + ".TestClass";
				
				ccu = StronglyTypedResourceBuilder.Create (testResources,
									"TestClass",
									"TestNameSpace",
									input,
									provider,
									true,
									out unmatchables);
				
				resourceManager = StronglyTypedResourceBuilderCodeDomTest.Get<CodeMemberProperty> (
													"ResourceManager", ccu);
				cvds = ((CodeVariableDeclarationStatement)((CodeConditionStatement)resourceManager.GetStatements [0]).TrueStatements [0]);
				output  = ((CodePrimitiveExpression)((CodeObjectCreateExpression)cvds.InitExpression).Parameters [0]).Value.ToString ();
				
				Assert.AreEqual (expected,output);
			}
		}
	}
}

#endif
