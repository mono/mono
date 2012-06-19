//
// StronglyTypedResourceBuilderVerifyResourceNameTests.cs tests
// the VerifyResourceName method
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
using Microsoft.CSharp;

namespace MonoTests.System.Resources.Tools {
	[TestFixture]
	public class StronglyTypedResourceBuilderVerifyResourceNameTests {
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
		CSharpCodeProvider provider = new CSharpCodeProvider();
		
		[Test]
		public void VerifyResourceNameEmpty ()
		{
			// should return _
			string output = StronglyTypedResourceBuilder.VerifyResourceName (string.Empty, provider);
			
			Assert.AreEqual ("_", output);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void VerifyResourceNameNull () {
			// should throw exception
			StronglyTypedResourceBuilder.VerifyResourceName (null, provider);
		}

		[Test]
		public void VerifyResourceNameProviderInvalidIdentifiers () {
			// function tests by means of provider.IsValidIdentifier after other checks
			string output;
			
			output = StronglyTypedResourceBuilder.VerifyResourceName ("tes$t", provider);
			
			Assert.AreEqual (null, output);
		}
		
		[Test]
		public void VerifyResourceNameProviderKeywords ()
		{
			// not complete list, doesnt really need to be
			string expected, output;
			
			foreach (string input in keywords) {
				output = StronglyTypedResourceBuilder.VerifyResourceName (input, provider);
				
				expected = provider.CreateValidIdentifier (input);
				
				Assert.AreEqual (expected, output);
			}
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void VerifyResourceNameProviderNull () {
			// should throw exception
			StronglyTypedResourceBuilder.VerifyResourceName ("tes$t", null);
		}

		[Test]
		public void VerifyResourceNameSpecialChars ()
		{
			// should replace with _
			string input, expected, output;
			
			foreach (char c in specialChars) {
				input 	 = string.Format ("{0}a{0}b{0}", c); 
				expected = string.Format ("{0}a{0}b{0}", '_');
				
				output = StronglyTypedResourceBuilder.VerifyResourceName (input, provider);
				
				Assert.AreEqual (expected, output);
			}
		}


	}
}

#endif
