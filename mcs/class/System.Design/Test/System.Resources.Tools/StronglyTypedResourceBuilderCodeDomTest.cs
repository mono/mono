//
// StronglyTypedResourceBuilderCodeDomTest.cs - tests a CodeCompileUnit 
// is correctly created for a test set of resources when calling main Create 
// overload
// 
// Author:
//	Gary Barnett (gary.barnett.mono@gmail.com)
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
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MonoTests.System.Resources.Tools {
	[TestFixture]
	public class StronglyTypedResourceBuilderCodeDomTest {
		CodeCompileUnit sampleCcu;
		string [] unmatchables;
		Dictionary<string, object> testResources;
		
		public static T Get<T> (string propertyName, CodeCompileUnit ccu) where T:CodeTypeMember
		{
			foreach (CodeTypeMember ctm in  ccu.Namespaces [0].Types [0].Members) {
				if (ctm.GetType () == typeof (T) && ctm.Name == propertyName)
					return (T) ctm;
			}
			return null;
		}
		
		[TestFixtureSetUp]
		public void TestFixtureSetup ()
		{
			testResources = new Dictionary<string, object> ();
			
			Bitmap bmp = new Bitmap (100, 100);
			MemoryStream wav = new MemoryStream (1000);
			// generate icon
			Bitmap icoBmp = new Bitmap (50, 50);
			Icon ico = Icon.FromHandle (icoBmp.GetHicon ());
			
			DateTime dt = DateTime.Now;
			
			testResources.Add ("astring", "myvalue"); // dont use key of "string" as its a keyword
			testResources.Add ("bmp", bmp);
			testResources.Add ("wav", wav);
			testResources.Add ("ico", ico);
			testResources.Add ("datetime", dt);
			
			sampleCcu = StronglyTypedResourceBuilder.Create (testResources,
									"TestRes",
									"TestNamespace",
									"TestResourcesNamespace",
									new CSharpCodeProvider (),
									true,
									out unmatchables);
			
			wav.Close();
		}
		
		[Test]
		public void GeneratedCodeNamespace ()
		{
			Assert.AreEqual ("TestNamespace", sampleCcu.Namespaces [0].Name);
			
			Assert.AreEqual (1, sampleCcu.ReferencedAssemblies.Count);
			Assert.AreEqual ("System.dll", sampleCcu.ReferencedAssemblies [0]);
			Assert.AreEqual ("System", sampleCcu.Namespaces [0].Imports [0].Namespace);
		}
		
		[Test]
		public void ClassNameAndAccess ()
		{
			CodeTypeDeclaration resType = sampleCcu.Namespaces [0].Types [0];
			
			Assert.AreEqual ("TestRes", resType.Name);
			Assert.IsTrue (resType.IsClass);
			Assert.IsTrue (resType.TypeAttributes ==  TypeAttributes.NotPublic);
		}
		
		[Test]
		public void ClassAttributes ()
		{
			CodeTypeDeclaration resType = sampleCcu.Namespaces [0].Types [0];
			
			//attributes
			Assert.AreEqual (3,resType.CustomAttributes.Count);
			Assert.AreEqual ("System.CodeDom.Compiler.GeneratedCodeAttribute", 
			                 resType.CustomAttributes [0].Name);
			Assert.AreEqual (2, resType.CustomAttributes [0].Arguments.Count);
			
			CodePrimitiveExpression cpe1 = (CodePrimitiveExpression) resType.CustomAttributes [0].Arguments [0].Value;
			CodePrimitiveExpression cpe2 = (CodePrimitiveExpression) resType.CustomAttributes [0].Arguments [1].Value;
			
			Assert.AreEqual ("System.Resources.Tools.StronglyTypedResourceBuilder", (string)cpe1.Value);
			Assert.IsTrue (Regex.IsMatch((string)cpe2.Value,@"^\d\.\d\.\d\.\d$")); // coming back as 4.0.0.0 even with project set to .net 2.0 running on MD under .NET?
			
			Assert.AreEqual ("System.Diagnostics.DebuggerNonUserCodeAttribute", 
			                 resType.CustomAttributes [1].Name);
			Assert.AreEqual (0, resType.CustomAttributes [1].Arguments.Count);
			
			Assert.AreEqual ("System.Runtime.CompilerServices.CompilerGeneratedAttribute", 
			                 resType.CustomAttributes [2].Name);
			Assert.AreEqual (0, resType.CustomAttributes [2].Arguments.Count);
		}
		
		[Test]
		public void Constructor ()
		{
			CodeMemberMethod ctor = Get<CodeConstructor> (".ctor", sampleCcu); //not checking for overloads
			
			Assert.IsNotNull (ctor);
			//access modifier
			Assert.IsTrue (ctor.Attributes ==  MemberAttributes.FamilyAndAssembly);
			
			//attributes
			Assert.AreEqual (1,ctor.CustomAttributes.Count);
			Assert.AreEqual ("System.Diagnostics.CodeAnalysis.SuppressMessageAttribute", 
			                 ctor.CustomAttributes [0].Name);
			Assert.AreEqual (2, ctor.CustomAttributes [0].Arguments.Count);
			
			CodePrimitiveExpression cpe1 = (CodePrimitiveExpression) ctor.CustomAttributes [0].Arguments[0].Value;
			CodePrimitiveExpression cpe2 = (CodePrimitiveExpression) ctor.CustomAttributes [0].Arguments[1].Value;
			
			Assert.AreEqual ("Microsoft.Performance", (string) cpe1.Value);
			Assert.AreEqual ("CA1811:AvoidUncalledPrivateCode", (string) cpe2.Value);
		}
		
		[Test]
		public void ResourceManField ()
		{
			CodeMemberField resourceMan = Get<CodeMemberField> ("resourceMan", sampleCcu);
			
			Assert.IsNotNull (resourceMan);
			
			Assert.AreEqual ("System.Resources.ResourceManager", resourceMan.Type.BaseType);
			//access modifier : outputs to private static but following settings found in .net tests
			Assert.IsTrue (resourceMan.Attributes == (MemberAttributes.Abstract
			                                          | MemberAttributes.Final
			                                          | MemberAttributes.Assembly
			                                          | MemberAttributes.FamilyOrAssembly));			
		}
		
		[Test]
		public void ResourceCultureField ()
		{
			CodeMemberField resourceCulture = Get<CodeMemberField> ("resourceCulture", sampleCcu);
			
			Assert.IsNotNull (resourceCulture);
			
			Assert.AreEqual ("System.Globalization.CultureInfo", resourceCulture.Type.BaseType);
			//access modifier
			Assert.IsTrue (resourceCulture.Attributes == (MemberAttributes.Abstract
									| MemberAttributes.Final
									| MemberAttributes.Assembly
									| MemberAttributes.FamilyOrAssembly));
		}
		
		[Test]
		public void ResourceManagerProperty ()
		{
			CodeMemberProperty resourceManager = Get<CodeMemberProperty> ("ResourceManager", sampleCcu);
			
			Assert.IsNotNull (resourceManager);
			Assert.AreEqual ("System.Resources.ResourceManager", resourceManager.Type.BaseType);
			//access modifer
			Assert.IsTrue (resourceManager.Attributes == (MemberAttributes.Abstract
				               				| MemberAttributes.Final
				                                        | MemberAttributes.Assembly));
			// attributes
			Assert.AreEqual (1, resourceManager.CustomAttributes.Count);
			Assert.AreEqual ("System.ComponentModel.EditorBrowsableAttribute", 
			                 resourceManager.CustomAttributes [0].Name);
			Assert.AreEqual (1, resourceManager.CustomAttributes [0].Arguments.Count);
			
			CodeFieldReferenceExpression cfe1 = (CodeFieldReferenceExpression)resourceManager.CustomAttributes [0].Arguments [0].Value;
			
			Assert.AreEqual ("Advanced", cfe1.FieldName);
			Assert.AreEqual ("System.ComponentModel.EditorBrowsableState", 
			                 ((CodeTypeReferenceExpression)cfe1.TargetObject).Type.BaseType);
			//getter
			Assert.IsInstanceOfType (typeof (CodeConditionStatement), resourceManager.GetStatements [0]);
			Assert.AreEqual (2, 
			                 ((CodeConditionStatement)resourceManager.GetStatements [0]).TrueStatements.Count);
			
			CodeVariableDeclarationStatement cvds = ((CodeVariableDeclarationStatement)((CodeConditionStatement)resourceManager.GetStatements [0]).TrueStatements [0]);
			
			Assert.AreEqual ("System.Resources.ResourceManager",
			                 ((CodeObjectCreateExpression)cvds.InitExpression).CreateType.BaseType);
			Assert.AreEqual ("TestResourcesNamespace.TestRes",
			                 ((CodePrimitiveExpression)((CodeObjectCreateExpression)cvds.InitExpression).Parameters [0]).Value);
			
			CodePropertyReferenceExpression cpre = (CodePropertyReferenceExpression)((CodeObjectCreateExpression)cvds.InitExpression).Parameters [1];
			
			Assert.AreEqual ("Assembly" ,cpre.PropertyName);
			Assert.AreEqual ("TestRes", ((CodeTypeOfExpression)cpre.TargetObject).Type.BaseType);
			
			//return
			Assert.AreEqual ("resourceMan",
			                 ((CodeFieldReferenceExpression)((CodeMethodReturnStatement)resourceManager.GetStatements [1]).Expression).FieldName);
			
		}
		
		[Test]
		public void CultureProperty ()
		{
			CodeMemberProperty culture = Get<CodeMemberProperty> ("Culture", sampleCcu);
			
			Assert.IsNotNull (culture);
			Assert.AreEqual ("System.Globalization.CultureInfo", culture.Type.BaseType);
			
			Assert.IsTrue (culture.Attributes == (MemberAttributes.Abstract
								| MemberAttributes.Final
								| MemberAttributes.Assembly));
			
			Assert.AreEqual (1,culture.CustomAttributes.Count);
			Assert.AreEqual ("System.ComponentModel.EditorBrowsableAttribute", 
			                 culture.CustomAttributes [0].Name);
			CodeFieldReferenceExpression cfe1 = (CodeFieldReferenceExpression) culture.CustomAttributes [0].Arguments [0].Value;
			
			Assert.AreEqual ("Advanced", cfe1.FieldName);
			Assert.AreEqual ("System.ComponentModel.EditorBrowsableState", 
			                 	((CodeTypeReferenceExpression)cfe1.TargetObject).Type.BaseType);
			
			// getter	
			Assert.AreEqual ("resourceCulture",
			                 ((CodeFieldReferenceExpression)((CodeMethodReturnStatement)culture.GetStatements [0]).Expression).FieldName);
			
			
			// setter
			Assert.AreEqual (1, culture.SetStatements.Count);
			Assert.AreEqual("resourceCulture", 
			                ((CodeFieldReferenceExpression)((CodeAssignStatement)culture.SetStatements [0]).Left).FieldName);
			Assert.IsInstanceOfType (typeof (CodePropertySetValueReferenceExpression),
			                         ((CodeAssignStatement)culture.SetStatements [0]).Right);
		}
		
		[Test]
		public void ResourcePropertiesForStandard()
		{
			// property declares variable obj and set it to Object ResourceManager.GetObject(..)
			// then returns obj cast back to its original type
			foreach ( KeyValuePair<string,object> kvp in testResources) {
				if (kvp.Value is String || kvp.Value is Stream)
					continue;
				
				CodeMemberProperty cmp = Get<CodeMemberProperty> (kvp.Key, sampleCcu);
				
				Assert.IsNotNull (cmp);
								
				Type thisType = kvp.Value.GetType ();
				
				// property type
				Assert.AreEqual (thisType.FullName,cmp.Type.BaseType);
				
				CodeVariableDeclarationStatement cvds = (CodeVariableDeclarationStatement)cmp.GetStatements [0];
				
				Assert.AreEqual ("obj", cvds.Name);
				Assert.AreEqual ("GetObject", 
				                 ((CodeMethodInvokeExpression)cvds.InitExpression).Method.MethodName);
				
				CodeMethodInvokeExpression cmie = ((CodeMethodInvokeExpression)cvds.InitExpression);
				
				Assert.AreEqual ("ResourceManager", 
				                 ((CodePropertyReferenceExpression)cmie.Method.TargetObject).PropertyName);
				Assert.AreEqual (kvp.Key, ((CodePrimitiveExpression)cmie.Parameters [0]).Value);
				Assert.AreEqual ("resourceCulture", 
				                 ((CodeFieldReferenceExpression)cmie.Parameters [1]).FieldName);
				
				CodeCastExpression cce = ((CodeCastExpression)((CodeMethodReturnStatement)cmp.GetStatements [1]).Expression);
				
				Assert.AreEqual (thisType.FullName, ((CodeTypeReference)cce.TargetType).BaseType);
				Assert.AreEqual ("obj", 
				                 ((CodeVariableReferenceExpression)cce.Expression).VariableName);
			}
		}
		
		[Test]
		public void ResourcePropertyForStrings ()
		{
			// property returns string ResourceManager.GetString(..) 
			foreach ( KeyValuePair<string,object> kvp in testResources) {
				if (!(kvp.Value is String))
					continue;
				
				CodeMemberProperty cmp = Get<CodeMemberProperty> (kvp.Key, sampleCcu);
				
				Assert.IsNotNull (cmp);
				Assert.AreEqual ("System.String", cmp.Type.BaseType);

				Assert.IsInstanceOfType (typeof (CodeMethodReturnStatement), cmp.GetStatements [0]);
				
				CodeMethodInvokeExpression cmie = ((CodeMethodInvokeExpression)((CodeMethodReturnStatement)cmp.GetStatements [0]).Expression);
				
				Assert.AreEqual ("GetString",cmie.Method.MethodName);
				Assert.AreEqual (kvp.Key,((CodePrimitiveExpression)cmie.Parameters[0]).Value);
				Assert.AreEqual ("ResourceManager", 
				                 ((CodePropertyReferenceExpression)cmie.Method.TargetObject).PropertyName);
				Assert.AreEqual ("resourceCulture", 
				                 ((CodeFieldReferenceExpression)cmie.Parameters [1]).FieldName);
			}
		}
		
		[Test]
		public void ResourcePropertyForStreams () 
		{
			// property returns UnmanagedMemoryStream ResourceManager.GetStream(..)		
			foreach ( KeyValuePair<string,object> kvp in testResources) {
				if (!(kvp.Value is Stream))
					continue;
				
				CodeMemberProperty cmp = Get<CodeMemberProperty> (kvp.Key, sampleCcu);
				
				Assert.IsNotNull (cmp);
				Assert.AreEqual ("System.IO.UnmanagedMemoryStream",cmp.Type.BaseType);
				
				CodeMethodInvokeExpression cmie = ((CodeMethodInvokeExpression)((CodeMethodReturnStatement)cmp.GetStatements [0]).Expression);
				
				Assert.AreEqual ("GetStream", cmie.Method.MethodName);
				Assert.AreEqual (kvp.Key, ((CodePrimitiveExpression)cmie.Parameters[0]).Value);
				Assert.AreEqual ("ResourceManager", 
				                 ((CodePropertyReferenceExpression)cmie.Method.TargetObject).PropertyName);
				Assert.AreEqual ("resourceCulture", 
				                 ((CodeFieldReferenceExpression)cmie.Parameters [1]).FieldName);
			}
		}
	}
}

#endif
