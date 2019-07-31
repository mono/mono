//
// System.Reflection.CustomAttributeData Test Cases
//
// Authors:
//  Zoltan Varga (vargaz@gmail.com)
//
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
// Copyright (C) 2004-2008 Novell, Inc (http://www.novell.com)
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
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MonoTests.System.Reflection
{
	enum Levels { one, two, three }

	class Attr : Attribute {
		public Attr (byte[] arr) {
		}
	}

	[AttributeUsage (AttributeTargets.Method)]
	class TestAttrWithObjectCtorParam : Attribute {
		object o;
		public TestAttrWithObjectCtorParam (object o) => this.o = o;
	}

	[AttributeUsage (AttributeTargets.Method)]
	class TestAttrWithEnumCtorParam : Attribute {
		Levels level;
		public TestAttrWithEnumCtorParam (Levels level) => this.level = level;
	}

	[AttributeUsage (AttributeTargets.Method)]
	class TestAttrWithObjectArrayCtorParam : Attribute {
		object[] o;
		public TestAttrWithObjectArrayCtorParam (object[] o) => this.o = o;
	}

	[AttributeUsage (AttributeTargets.Method)]
	class TestAttrWithObjectArrayCtorParamAndParamsKeyword : Attribute {
		object[] o;
		public TestAttrWithObjectArrayCtorParamAndParamsKeyword (params object[] o) => this.o = o;
	}

	[AttributeUsage (AttributeTargets.Method)]
	class TestAttrWithObjectArrayCtorParams : Attribute {
		object[] o1;
		object[] o2;
		public TestAttrWithObjectArrayCtorParams (object[] o1, params object[] o2) {
			this.o1 = o1;
			this.o2 = o2;
		}
	}

	[TestFixture]
	public class CustomAttributeDataTest
	{
		[DllImport ("libc")]
		public static extern int readlink (string path, byte[] buffer, int buflen);

		[MarshalAs (UnmanagedType.LPStr)]
		[NonSerialized]
		public string fieldDecoratedWithPseudoCustomAttributes = "test";

		[Attr (new byte [] { 1, 2 })]
		public void MethodWithAttr () {
		}

		[TestAttrWithObjectCtorParam (Levels.two)]
		public void MethodDecoratedWithAttribute1 ()
		{
		}

		[TestAttrWithEnumCtorParam (Levels.two)]
		public void MethodDecoratedWithAttribute2 ()
		{
		}

		[TestAttrWithObjectArrayCtorParam (new object[] { Levels.one, Levels.two})]
		public void MethodDecoratedWithAttribute3 ()
		{
		}		

		[TestAttrWithObjectArrayCtorParamAndParamsKeyword (Levels.one, Levels.two)]
		public void MethodDecoratedWithAttribute4 ()
		{
		}

		[TestAttrWithObjectArrayCtorParams (new object[] { Levels.one, Levels.two}, Levels.three, Levels.two)]
		public void MethodDecoratedWithAttribute5 ()
		{
		}

		public void MethodWithParamDecoratedWithPseudoCustomAttributes ([Optional, In, Out, MarshalAs (UnmanagedType.LPStr)] String s)
		{
		}

		[return: MarshalAs (UnmanagedType.LPStr)]
		public string MethodWithReturnValueDecoratedWithMarshalAs ()
		{
			return "test";
		}

		[Test]
		public void Arrays () {
			IList<CustomAttributeData> cdata = CustomAttributeData.GetCustomAttributes (typeof (CustomAttributeDataTest).GetMethod ("MethodWithAttr"));
			Assert.AreEqual (1, cdata.Count);
			CustomAttributeTypedArgument arg = cdata [0].ConstructorArguments [0];
			Assert.IsTrue (typeof (IList<CustomAttributeTypedArgument>).IsAssignableFrom (arg.Value.GetType ()));
			IList<CustomAttributeTypedArgument> arr = (IList<CustomAttributeTypedArgument>)arg.Value;
			Assert.AreEqual (2, arr.Count);
			Assert.AreEqual (typeof (byte), arr [0].ArgumentType);
			Assert.AreEqual (1, arr [0].Value);
			Assert.AreEqual (typeof (byte), arr [1].ArgumentType);
			Assert.AreEqual (2, arr [1].Value);
		}

		[Test]
		// https://github.com/mono/mono/issues/10951
		public void ObjectArrays () 
		{
			CheckObjectArrayParam (nameof (MethodDecoratedWithAttribute3));
			CheckObjectArrayParam (nameof (MethodDecoratedWithAttribute4));
		}

		private void CheckObjectArrayParam (string methodName)
		{
			IList<CustomAttributeData> cdata = CustomAttributeData.GetCustomAttributes (typeof (CustomAttributeDataTest).GetMethod (methodName));
			Assert.AreEqual (1, cdata.Count, $"{methodName}#0");
			
			CustomAttributeTypedArgument arg = cdata [0].ConstructorArguments [0];
			Assert.IsTrue (typeof (IList<CustomAttributeTypedArgument>).IsAssignableFrom (arg.Value.GetType ()), $"{methodName}#1");
			
			IList<CustomAttributeTypedArgument> arr = (IList<CustomAttributeTypedArgument>)arg.Value;
			Assert.AreEqual (2, arr.Count, $"{methodName}#2");
			
			Assert.AreEqual (typeof (Levels), arr [0].ArgumentType, $"{methodName}#3");
			Assert.IsTrue (arr [0].ArgumentType.GetTypeInfo().IsEnum, $"{methodName}#4");
			Assert.AreEqual (0, arr [0].Value, $"{methodName}#5");
			Assert.AreEqual (typeof (int), arr [0].Value.GetType (), $"{methodName}#6");
			
			Assert.AreEqual (typeof (Levels), arr [1].ArgumentType, $"{methodName}#7");
			Assert.IsTrue (arr [1].ArgumentType.GetTypeInfo().IsEnum, $"{methodName}#8");
			Assert.AreEqual (1, arr [1].Value, $"{methodName}#9");
			Assert.AreEqual (typeof (int), arr [1].Value.GetType (), $"{methodName}#10");
		}

		[Test]
		// https://github.com/mono/mono/issues/10951
		public void CheckObjectArrayParams ()
		{
			string methodName = nameof (MethodDecoratedWithAttribute5);
			IList<CustomAttributeData> cdata = CustomAttributeData.GetCustomAttributes (typeof (CustomAttributeDataTest).GetMethod (methodName));
			Assert.AreEqual (1, cdata.Count, $"{methodName}#0");
			Assert.AreEqual (2, cdata [0].ConstructorArguments.Count, $"{methodName}#00");

			CustomAttributeTypedArgument arg = cdata [0].ConstructorArguments [0];
			Assert.IsTrue (typeof (IList<CustomAttributeTypedArgument>).IsAssignableFrom (arg.Value.GetType ()), $"{methodName}#1");
			
			IList<CustomAttributeTypedArgument> arr = (IList<CustomAttributeTypedArgument>)arg.Value;
			Assert.AreEqual (2, arr.Count, $"{methodName}#2");
			
			Assert.AreEqual (typeof (Levels), arr [0].ArgumentType, $"{methodName}#3");
			Assert.IsTrue (arr [0].ArgumentType.GetTypeInfo().IsEnum, $"{methodName}#4");
			Assert.AreEqual (0, arr [0].Value, $"{methodName}#5");
			Assert.AreEqual (typeof (int), arr [0].Value.GetType (), $"{methodName}#6");
			
			Assert.AreEqual (typeof (Levels), arr [1].ArgumentType, $"{methodName}#7");
			Assert.IsTrue (arr [1].ArgumentType.GetTypeInfo().IsEnum, $"{methodName}#8");
			Assert.AreEqual (1, arr [1].Value, $"{methodName}#9");
			Assert.AreEqual (typeof (int), arr [1].Value.GetType (), $"{methodName}#10");

			arg = cdata [0].ConstructorArguments [1];
			Assert.IsTrue (typeof (IList<CustomAttributeTypedArgument>).IsAssignableFrom (arg.Value.GetType ()), $"{methodName}#11");
			
			arr = (IList<CustomAttributeTypedArgument>)arg.Value;
			Assert.AreEqual (2, arr.Count, $"{methodName}#12");
			
			Assert.AreEqual (typeof (Levels), arr [0].ArgumentType, $"{methodName}#13");
			Assert.IsTrue (arr [0].ArgumentType.GetTypeInfo().IsEnum, $"{methodName}#14");
			Assert.AreEqual (2, arr [0].Value, $"{methodName}#15");
			Assert.AreEqual (typeof (int), arr [0].Value.GetType (), $"{methodName}#16");
			
			Assert.AreEqual (typeof (Levels), arr [1].ArgumentType, $"{methodName}#17");
			Assert.IsTrue (arr [1].ArgumentType.GetTypeInfo().IsEnum, $"{methodName}#18");
			Assert.AreEqual (1, arr [1].Value, $"{methodName}#19");
			Assert.AreEqual (typeof (int), arr [1].Value.GetType (), $"{methodName}#20");			
		}		
        
		[Test]
		public void ParameterIncludesPseudoCustomAttributesData ()
		{
			var methodInfo = typeof (CustomAttributeDataTest).GetMethod ("MethodWithParamDecoratedWithPseudoCustomAttributes");
			var paramInfo = methodInfo.GetParameters () [0];
			var customAttributesData = CustomAttributeData.GetCustomAttributes (paramInfo);

			Assert.AreEqual (4, customAttributesData.Count);

			var inAttributeData = customAttributesData [0];
			var outAttributeData = customAttributesData [1];
			var optionalAttributeData = customAttributesData [2];			
			var marshalAsAttributeData = customAttributesData [3];

			var marshalAsAttributeCtorArg = marshalAsAttributeData.ConstructorArguments [0];

			Assert.AreEqual (typeof (InAttribute), inAttributeData.AttributeType);
			Assert.AreEqual (typeof (OutAttribute), outAttributeData.AttributeType);
			Assert.AreEqual (typeof (OptionalAttribute), optionalAttributeData.AttributeType);

			Assert.AreEqual (typeof (MarshalAsAttribute), marshalAsAttributeData.AttributeType);
			Assert.AreEqual (typeof (UnmanagedType), marshalAsAttributeCtorArg.ArgumentType);
			Assert.AreEqual ((int)UnmanagedType.LPStr, marshalAsAttributeCtorArg.Value);
		}

		[Test]
		public void FieldIncludesPseudoCustomAttributesData ()
		{
			var fieldInfo = typeof (CustomAttributeDataTest).GetField ("fieldDecoratedWithPseudoCustomAttributes");
			var customAttributesData = CustomAttributeData.GetCustomAttributes (fieldInfo);

			Assert.AreEqual (2, customAttributesData.Count);

			var nonSerializedAttributeData = customAttributesData [0];
			var marshalAsAttributeData = customAttributesData [1];
			var marshalAsAttributeDataCtorArg = marshalAsAttributeData.ConstructorArguments [0];

			Assert.AreEqual (typeof (NonSerializedAttribute), nonSerializedAttributeData.AttributeType);
			Assert.AreEqual (typeof (MarshalAsAttribute), marshalAsAttributeData.AttributeType);
			Assert.AreEqual (typeof (UnmanagedType), marshalAsAttributeDataCtorArg.ArgumentType);
			Assert.AreEqual ((int)UnmanagedType.LPStr, marshalAsAttributeDataCtorArg.Value);
		}

		[Test]
		public void MethodIncludesMarshalAsAttributeData ()
		{
			var methodInfo = typeof (CustomAttributeDataTest).GetMethod ("MethodWithReturnValueDecoratedWithMarshalAs");
			var paramInfo = (ParameterInfo)methodInfo.ReturnTypeCustomAttributes;
			var customAttributesData = CustomAttributeData.GetCustomAttributes (paramInfo);
			var marshalAsAttributeData = customAttributesData [0];
			var ctorArg = marshalAsAttributeData.ConstructorArguments [0];

			Assert.AreEqual (1, customAttributesData.Count);
			Assert.AreEqual (typeof (MarshalAsAttribute), marshalAsAttributeData.AttributeType);
			Assert.AreEqual (typeof (UnmanagedType), ctorArg.ArgumentType);
			Assert.AreEqual ((int)UnmanagedType.LPStr, ctorArg.Value);
		}

		[Test]
		// https://github.com/mono/mono/issues/10544
		public void MethodIncludesDllImportAttributeData ()
		{
			var mi = typeof (CustomAttributeDataTest).FindMembers (MemberTypes.Method, BindingFlags.Static | BindingFlags.Public, (m, criteria) => m.Name == "readlink", null);
			var data = ((MethodInfo)(mi[0])).CustomAttributes;
			
			Assert.AreEqual (2, data.Count ());

			Assert.AreEqual (typeof (PreserveSigAttribute), data.First ().AttributeType);

			var dllImportAttributeData = data.Last ();
			var ctorArg = dllImportAttributeData.ConstructorArguments [0];

			Assert.AreEqual (typeof (DllImportAttribute), dllImportAttributeData.AttributeType);
			Assert.AreEqual ("libc", ctorArg.Value);
		}

		[Test]
		// https://github.com/mono/mono/issues/10555
		public void CustomAttributeCtor_TakesEnumArg ()
		{
			var method = GetMethod (nameof (MethodDecoratedWithAttribute1));
			var data = method.CustomAttributes;
			var ctorArg = data.First ().ConstructorArguments [0];

			Assert.AreEqual (typeof (Levels), ctorArg.ArgumentType);
			Assert.AreEqual (1, ctorArg.Value);
			Assert.AreEqual (typeof (int), ctorArg.Value.GetType ());

			method = GetMethod (nameof (MethodDecoratedWithAttribute2));
			data = method.CustomAttributes;
			ctorArg = data.First ().ConstructorArguments [0];

			Assert.AreEqual (typeof (Levels), ctorArg.ArgumentType);
			Assert.AreEqual (1, ctorArg.Value);
			Assert.AreEqual (typeof (int), ctorArg.Value.GetType ());			
		}

		private MethodInfo GetMethod (string methodName)
		{
			var mi = typeof (CustomAttributeDataTest).FindMembers (MemberTypes.Method, BindingFlags.Instance | BindingFlags.Public, (m, criteria) => m.Name == methodName, null);
			return (MethodInfo)(mi [0]);
		}
	}
}
