//
// StronglyTypedResourceBuilderOtherTests.cs - tests the internalClass, 
// codeProvider and resourceList params of the main Create overload
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
using System.IO;
using System.Reflection;
using System.Drawing;

namespace MonoTests.System.Resources.Tools {
	[TestFixture]
	public class StronglyTypedResourceBuilderOtherTests	{
		CSharpCodeProvider provider = new CSharpCodeProvider ();
		
		Dictionary<string,object> GetAllResourceTypes ()
		{
			Dictionary<string, object> tempDict = new Dictionary<string, object> ();
			Bitmap bmp = new Bitmap (100,100);
			MemoryStream wav = new MemoryStream (1000);
			
			tempDict.Add ("astring", "myvalue");
			tempDict.Add ("bmp", bmp);
			tempDict.Add ("wav", wav);  
			
			wav.Close ();
			return tempDict;
		}
		
		Dictionary<string,object> GetTestResources ()
		{
			Dictionary<string, object> tempDict = new Dictionary<string, object> ();
			tempDict.Add ("akey", string.Empty);
			
			return tempDict;
		}
		
		[Test]
		public void InternalClassFalse ()
		{
			// check access modifiers for class, Culture, ResourceManager, string, stream and standard resource properties
			Dictionary<string, object> testResources = GetAllResourceTypes ();
			string [] unmatchables;
			CodeCompileUnit ccu;
			CodeMemberProperty cmp;
			
			bool isInternal = false;
			
			ccu = StronglyTypedResourceBuilder.Create (testResources,
								"TestClass",
								"TestNamespace",
								"TestResourcesNameSpace",
								provider,
								isInternal,
								out unmatchables);
			
			CodeTypeDeclaration resType = ccu.Namespaces [0].Types [0];
			Assert.IsTrue (resType.TypeAttributes ==  TypeAttributes.Public);
			
			cmp = StronglyTypedResourceBuilderCodeDomTest.Get<CodeMemberProperty> (
											"ResourceManager", ccu);
			Assert.IsTrue (cmp.Attributes == (MemberAttributes.Abstract
							| MemberAttributes.Final
							| MemberAttributes.FamilyAndAssembly
							| MemberAttributes.FamilyOrAssembly));
			
			cmp = StronglyTypedResourceBuilderCodeDomTest.Get<CodeMemberProperty> ("Culture", ccu);
			Assert.IsTrue (cmp.Attributes == (MemberAttributes.Abstract
							| MemberAttributes.Final
							| MemberAttributes.FamilyAndAssembly
							| MemberAttributes.FamilyOrAssembly));
			
			cmp = StronglyTypedResourceBuilderCodeDomTest.Get<CodeMemberProperty> ("astring", ccu);
			Assert.IsTrue (cmp.Attributes == (MemberAttributes.Abstract
							| MemberAttributes.Final
							| MemberAttributes.FamilyAndAssembly
							| MemberAttributes.FamilyOrAssembly));
			
			cmp = StronglyTypedResourceBuilderCodeDomTest.Get<CodeMemberProperty> ("bmp", ccu);
			Assert.IsTrue (cmp.Attributes == (MemberAttributes.Abstract
							| MemberAttributes.Final
							| MemberAttributes.FamilyAndAssembly
							| MemberAttributes.FamilyOrAssembly));
			
			cmp = StronglyTypedResourceBuilderCodeDomTest.Get<CodeMemberProperty> ("wav", ccu);
			Assert.IsTrue (cmp.Attributes == (MemberAttributes.Abstract
							| MemberAttributes.Final
							| MemberAttributes.FamilyAndAssembly
							| MemberAttributes.FamilyOrAssembly));
		}
		
		[Test]
		public void InternalClassTrue ()
		{
			// check access modifiers for class, Culture, ResourceManager, string, stream and standard resource properties
			Dictionary<string, object> testResources = GetAllResourceTypes ();
			string [] unmatchables;
			CodeCompileUnit ccu;
			CodeMemberProperty cmp;
			
			bool isInternal = true;
			
			ccu = StronglyTypedResourceBuilder.Create (testResources,
								"TestClass",
								"TestNamespace",
								"TestResourcesNameSpace",
								provider,
								isInternal,
								out unmatchables);
			
			
			CodeTypeDeclaration resType = ccu.Namespaces [0].Types [0];
			Assert.IsTrue (resType.TypeAttributes ==  TypeAttributes.NotPublic);
			
			cmp = StronglyTypedResourceBuilderCodeDomTest.Get<CodeMemberProperty> (
											"ResourceManager", ccu);
			Assert.IsTrue (cmp.Attributes == (MemberAttributes.Abstract
							| MemberAttributes.Final
							| MemberAttributes.Assembly));
			
			cmp = StronglyTypedResourceBuilderCodeDomTest.Get<CodeMemberProperty> ("Culture", ccu);
			Assert.IsTrue (cmp.Attributes == (MemberAttributes.Abstract
							| MemberAttributes.Final
							| MemberAttributes.Assembly));
			
			cmp = StronglyTypedResourceBuilderCodeDomTest.Get<CodeMemberProperty> ("astring", ccu);
			Assert.IsTrue (cmp.Attributes == (MemberAttributes.Abstract
							| MemberAttributes.Final
							| MemberAttributes.Assembly));
			cmp = StronglyTypedResourceBuilderCodeDomTest.Get<CodeMemberProperty> ("bmp", ccu);
			Assert.IsTrue (cmp.Attributes == (MemberAttributes.Abstract
							| MemberAttributes.Final
							| MemberAttributes.Assembly));
			cmp = StronglyTypedResourceBuilderCodeDomTest.Get<CodeMemberProperty> ("wav", ccu);
			Assert.IsTrue (cmp.Attributes == (MemberAttributes.Abstract
							| MemberAttributes.Final
							| MemberAttributes.Assembly));
		}
		
		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void ProviderNull ()
		{
			// should throw exception
			Dictionary<string, object> testResources = GetTestResources ();
			string [] unmatchables;

			StronglyTypedResourceBuilder.Create (testResources,
							"TestClass",
							"TestNamespace",
							"TestResourcesNameSpace",
							null, //setting provider to null
							true,
							out unmatchables);
		}
		
		[Test]
		public void ResourceListEmpty ()
		{
			//should still create class with default members
			Dictionary<string, object> testResources;
			string [] unmatchables;
			CodeCompileUnit ccu;
			
			testResources = new Dictionary<string, object> ();
			
			ccu = StronglyTypedResourceBuilder.Create (testResources,
								"TestRes",
								"TestNamespace",
								"TestResourcesNameSpace",
								provider,
								true,
								out unmatchables);
			
			Assert.AreEqual(5,ccu.Namespaces [0].Types [0].Members.Count);
		}
		
		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void ResourceListNull ()
		{
			// should through exception
			Dictionary<string, object> testResources;
			string [] unmatchables; 
			CSharpCodeProvider provider = new CSharpCodeProvider ();
			
			testResources = null;
			
			StronglyTypedResourceBuilder.Create (testResources,
								"TestRes",
								"TestNamespace",
								"TestResourcesNameSpace",
								provider,
								true,
								out unmatchables);
		}
		
	}
}


#endif
