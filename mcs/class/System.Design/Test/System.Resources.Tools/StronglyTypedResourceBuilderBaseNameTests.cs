//
// StronglyTypedResourceBuilderBaseNameTests.cs - tests thae baseName 
// parameter passed to main Create overload
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
	public class StronglyTypedResourceBuilderBaseNameTests	{
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
		public void BaseNameEmpty ()
		{
			// empty class name should change to _
			string [] unmatchables;
			CodeCompileUnit ccu;
			string input, expected;
			
			input = String.Empty;
			
			ccu = StronglyTypedResourceBuilder.Create (testResources,
								input,
								"TestNamespace",
								"TestResourcesNameSpace",
								provider,
								true,
								out unmatchables);
			
			expected = "_";
			
			Assert.AreEqual (expected,ccu.Namespaces [0].Types [0].Name);
		}
		
		[Test, ExpectedException (typeof (ArgumentException))]
		public void BaseNameInvalidIdentifier ()
		{
			// identifier invalid after Going through provider.CreateValidIdentifier throw exception in .NET framework
			string [] unmatchables;
			string input;
			
			input = "cla$ss";
			
			StronglyTypedResourceBuilder.Create (testResources,
								input,
								"TestNamespace",
								"TestResourcesNameSpace",
								provider,
								true,
								out unmatchables);
		}
		
		[Test]
		public void BaseNameKeywords ()
		{
			// provider.CreateValidIdentifier used to return valid identifier
			string expected;
			string [] unmatchables;
			CodeCompileUnit ccu;
			
			foreach (string input in keywords) {
				ccu = StronglyTypedResourceBuilder.Create (testResources,
				                                        input,
				                                        "TestNamespace",
				                                        "TestResourcesNameSpace",
				         				provider,
				                                        true,
				                                        out unmatchables);
				
				expected = provider.CreateValidIdentifier (input);
				
				Assert.AreEqual (expected,ccu.Namespaces [0].Types [0].Name);
			}
		}
		
		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void BaseNameNull ()
		{
			// should throw exception
			string [] unmatchables;
			string input;
			
			input = null;
			
			StronglyTypedResourceBuilder.Create (testResources,
								input,
								"TestNamespace",
								"TestResourcesNameSpace",
								provider,
								true,
								out unmatchables);
		}
		
		[Test]
		public void BaseNameSpecialChars ()
		{
			// StronglyTypedResourceBuilder.VerifyResourceName seems to be used
			string [] unmatchables;
			CodeCompileUnit ccu;
			string input, expected;

			foreach (char c in specialChars) {
				input = c.ToString ();
				
				ccu = StronglyTypedResourceBuilder.Create (testResources,
									input,
									"TestNamespace",
									"TestResourcesNameSpace",
									provider,
									true,
									out unmatchables);
				
				expected = StronglyTypedResourceBuilder.VerifyResourceName (input, provider);
				
				Assert.AreEqual (expected,ccu.Namespaces [0].Types [0].Name); 
			}
		}
	}
}

#endif
