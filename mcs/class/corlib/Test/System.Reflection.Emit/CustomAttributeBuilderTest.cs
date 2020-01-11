// CustomAttributeBuilderTest.cs
//
// Author: Vineeth N <nvineeth@yahoo.com>
//
// (C) 2004 Ximian, Inc. http://www.ximian.com
//
using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace MonoTests.System.Reflection.Emit
{
	/// <summary>
	/// TestFixture for CustomAttributeBuilderTest.
	/// The members to be tested are as follows:
	/// 4 constructors:
	/// 1) public CustomAttributeBuilder(ConstructorInfo, object[]);
	/// 2) public CustomAttributeBuilder(ConstructorInfo, object[], FieldInfo[], object[]);
	/// 3) public CustomAttributeBuilder(ConstructorInfo, object[], PropertyInfo[], object[]);
	/// 4) public CustomAttributeBuilder(ConstructorInfo, object[], PropertyInfo[], object[], FieldInfo[], object[]);
	/// and the exceptions that are thrown.
	/// In the implementation , it can be seen that the first
	/// three type of  constructors call the 4th type of ctor, which takes 6 args
	/// by filling args and substituting null as required.
	/// For testing constructors we have use 4 different test functions,
	/// Various exceptions have been checked for 4th type of consturctor.
	/// </summary>

	[TestFixture]
	public class CustomAttributeBuilderTest
	{
		static string tempDir = Path.Combine (Path.GetTempPath (), typeof (CustomAttributeBuilderTest).FullName);
		
		// the CustomAttribute class is used for testing and it has to be public
		//since it will be associated with a class that belongs to another assembly 

		[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Class)]
		public class CustomAttribute: Attribute
		{
			private string attr1;
			private string attr2;
			public string Feild; //used for testing the second type of constructor

			public CustomAttribute () {}
			public CustomAttribute (String s1 , String s2)
			{
				attr1 = s1;
				attr2=s2;
                        }

			private CustomAttribute (String s1) {}
			static CustomAttribute () {}

			public string AttributeOne
			{
				get { return attr1; }
				set { attr1 = value; }
			}
			
			public string AttributeTwo
			{
				get { return attr2; }
				//the set is skipped and is used later in testing
			}

		}
		
		private class TempClass
		{
			//used for testing the ArgumentException
			public string Field;
			public string FieldProperty
			{
				get { return Field; }
				set { Field = value; }
			}
		}

		[SetUp]
		public void SetUp ()
		{
			Random AutoRand = new Random ();
			string basePath = tempDir;
			while (Directory.Exists (tempDir))
				tempDir = Path.Combine (basePath, AutoRand.Next ().ToString ());
			Directory.CreateDirectory (tempDir);
		}

		[TearDown]
		public void TearDown ()
		{
			try {
				// This throws an exception under MS.NET, since the directory contains loaded
				// assemblies.
				Directory.Delete (tempDir, true);
			} catch (Exception) {
			}
		}
		
		[Test]
		public void CtorOneTest ()
		{
			//test for the constructor with signature--
			// public CustomAttributeBuilder(ConstructorInfo, object[]);
			/*
			 * WE build a imaginary type as follows
			 * class TestType
			 * {
			 *	[CustomAttribute("one","two")]
			 *	public string Str;
			 * 
			 *	[CustomAttribute("hello","world")]
			 *	public void Print()
			 *	{Console.WriteLine("Hello World"); }
			 * 
			 * }
			 * And then check for the validity of attributes in the test functions
			 */
			AssemblyName asmName = new AssemblyName ();
			asmName.Name = "TestAssembly.dll";

			AssemblyBuilder asmBuilder = Thread.GetDomain ().DefineDynamicAssembly (
				asmName , AssemblyBuilderAccess.Run);
			
			ModuleBuilder modBuilder = asmBuilder.DefineDynamicModule ("TestModule");
			
			TypeBuilder typeBuilder = modBuilder.DefineType ("TestType",
				TypeAttributes.Public);

			Type[] ctorParams = new Type[] { typeof (string),typeof (string) };
			
			ConstructorInfo classCtorInfo = 
				typeof (CustomAttribute).GetConstructor (ctorParams);

			CustomAttributeBuilder feildCABuilder = new CustomAttributeBuilder (
				classCtorInfo,
				new object [] { "one","two" }
				),
				methodCABuilder = new CustomAttributeBuilder (
				classCtorInfo,
				new object [] { "hello","world" }
				);
			//now let's build a feild of type string and associate a attribute with it
			FieldBuilder fieldBuilder= typeBuilder.DefineField ("Str",
				typeof (string), FieldAttributes.Public);
			fieldBuilder.SetCustomAttribute (feildCABuilder);
			//now build a method 
			MethodBuilder methodBuilder= typeBuilder.DefineMethod ("Print",
				MethodAttributes.Public, null, null);
			methodBuilder.SetCustomAttribute (methodCABuilder);
			ILGenerator methodIL = methodBuilder.GetILGenerator ();
			methodIL.EmitWriteLine ("Hello, world!");
			methodIL.Emit (OpCodes.Ret);
			
			// create the type
			Type myType = typeBuilder.CreateType ();

			//Now check for the validity of the attributes.
			object testInstance = Activator.CreateInstance (myType);

			//check the validity of the attribute associated with Print method 
			
			object [] methodAttrs =  myType.GetMember ("Print") [0].GetCustomAttributes (true);
			Assert.AreEqual (methodAttrs.Length, 1, "#1");
			CustomAttribute methodAttr = methodAttrs [0] as CustomAttribute;
			Assert.AreEqual (methodAttr.AttributeOne, "hello", "#2");
			Assert.AreEqual (methodAttr.AttributeTwo, "world", "#3");
			
			//check the validity of the attribute associated with Str feild

			object [] fieldAttrs = myType.GetField ("Str").GetCustomAttributes (true);
			Assert.AreEqual(fieldAttrs.Length, 1, "#4");
			CustomAttribute fieldAttr = fieldAttrs [0] as CustomAttribute;
			Assert.AreEqual(fieldAttr.AttributeOne, "one", "#5");
			Assert.AreEqual(fieldAttr.AttributeTwo, "two", "#6");
		}

		[Test]
		public void CtorTwoTest ()
		{
			//test for the constructor with signature--
			// CustomAttributeBuilder Constructor (ConstructorInfo, Object[], FieldInfo[], Object[]) ;
			/*
			 * WE build a imaginary type as follows
			 * [CustomAttribute("Test","Type")]
			 * public class TestType
			 * {
			 * 
			 * }
			 * We also set the "Feild" of class CustomAttribute and the value;
			 * And then check for the validity of attributes in the test functions
			 */
									
			AssemblyName asmName = new AssemblyName ();
			asmName.Name = "TestAssembly.dll";

			AssemblyBuilder asmBuilder = Thread.GetDomain ().DefineDynamicAssembly (
				asmName, AssemblyBuilderAccess.Run);
			
			ModuleBuilder modBuilder = asmBuilder.DefineDynamicModule ("TestModule");
			
			TypeBuilder typeBuilder = modBuilder.DefineType ("TestType",
				TypeAttributes.Public);

			Type [] ctorParams = new Type [] { typeof (string), typeof (string) };
			
			ConstructorInfo classCtorInfo = 
				typeof (CustomAttribute).GetConstructor (ctorParams);

			CustomAttributeBuilder typeCABuilder = new CustomAttributeBuilder (
				classCtorInfo,
				new object [] { "Test","Type" },
				typeof(CustomAttribute).GetFields(),
				new object [] { "TestCase" }
				); 
				
			typeBuilder.SetCustomAttribute (typeCABuilder);
			
			// create the type
			Type myType = typeBuilder.CreateType ();

			//Now check for the validity of the attributes.
			object testInstance = Activator.CreateInstance (myType);

			//check the validity of the attribute associated with Print method 
			object [] customAttrs = myType.GetCustomAttributes (false);
			Assert.AreEqual (customAttrs.Length, 1, "1");

			//Custom Attributes of TestType
			CustomAttribute attr = customAttrs [0] as CustomAttribute;
			Assert.AreEqual (attr.AttributeOne, "Test", "#2");
			Assert.AreEqual (attr.AttributeTwo, "Type", "#3");
			Assert.AreEqual (attr.Feild, "TestCase", "#4");

		}

		[Test]
		public void CtorThreeTest ()
		{
			//test for the constructor with signature--
			// CustomAttributeBuilder Constructor (ConstructorInfo, Object[], PropertyInfo[], Object[]) ;
			/*
			 * WE build a imaginary type as follows
			 * [CustomAttribute()]
			 * public class TestType
			 * {
			 * 
			 * }
			 * We also set the "AttributeOne" of class CustomAttribute by means of the constuctor
			 * And then check for the validity of attribute state 
			 */
			
			AssemblyName asmName = new AssemblyName ();
			asmName.Name = "TestAssembly.dll";

			AssemblyBuilder asmBuilder = Thread.GetDomain ().DefineDynamicAssembly (
				asmName, AssemblyBuilderAccess.Run);
			
			ModuleBuilder modBuilder = asmBuilder.DefineDynamicModule ("TestModule");
			
			TypeBuilder typeBuilder = modBuilder.DefineType ("TestType",
				TypeAttributes.Public);
								
			Type [] ctorParams = new Type [] { };

			ConstructorInfo classCtorInfo = 
				typeof (CustomAttribute).GetConstructor (ctorParams);

			CustomAttributeBuilder typeCABuilder = new CustomAttributeBuilder (
				classCtorInfo,
				new object [] { },
				new PropertyInfo []{ typeof (CustomAttribute).GetProperty ("AttributeOne") },
				new object [] { "TestCase" }
				); 
				
			typeBuilder.SetCustomAttribute (typeCABuilder);
			
			// create the type
			Type myType = typeBuilder.CreateType ();

			//Now check for the validity of the attributes.
			object testInstance = Activator.CreateInstance (myType);

			//check the validity of the attribute associated with Print method 
			object [] customAttrs = myType.GetCustomAttributes (false);
			Assert.AreEqual (customAttrs.Length , 1, "#1");

			//Custom Attributes of TestType
			CustomAttribute attr = customAttrs [0] as CustomAttribute;
			Assert.AreEqual(attr.AttributeOne, "TestCase", "#2");
		}

		[Test]
		public void CtorFourTest ()
		{
			//test for the constructor with signature--
			//public CustomAttributeBuilder(ConstructorInfo, object[], PropertyInfo[], object[], FieldInfo[], object[]);
			/*
			 * WE build a imaginary type as follows
			 * [CustomAttribute()]
			 * public class TestType
			 * {
			 * 
			 * }
			 * We also set the "AttributeOne" property ,
			 * and "Feild" of class CustomAttribute 
			 * by means of the constuctor of CustomAttributeBuilder
			 * And then check for the validity 
			 */
					
			AssemblyName asmName = new AssemblyName ();
			asmName.Name = "TestAssembly.dll";

			AssemblyBuilder asmBuilder = Thread.GetDomain ().DefineDynamicAssembly (
				asmName , AssemblyBuilderAccess.Run);
			
			ModuleBuilder modBuilder = asmBuilder.DefineDynamicModule ("TestModule");
			
			TypeBuilder typeBuilder = modBuilder.DefineType ("TestType",
				TypeAttributes.Public);
		
			Type [] ctorParams = new Type [] { };
			
			ConstructorInfo classCtorInfo = 
				typeof (CustomAttribute).GetConstructor (ctorParams);

			CustomAttributeBuilder typeCABuilder = new CustomAttributeBuilder(
				classCtorInfo,
				new object [] { },
				new PropertyInfo []{ typeof (CustomAttribute).GetProperty ("AttributeOne") },
				new object [] { "TestCase" },
				typeof(CustomAttribute).GetFields (),
				new object [] { "FieldValue" }
				); 
				
			typeBuilder.SetCustomAttribute (typeCABuilder);
			
			// create the type
			Type myType = typeBuilder.CreateType ();

			//Now check for the validity of the attributes.
			object testInstance = Activator.CreateInstance (myType);

			//check the validity of the attribute associated with Print method 
			object [] customAttrs = myType.GetCustomAttributes (false);
			Assert.AreEqual(customAttrs.Length , 1, "#1");

			//Custom Attributes of TestType
			CustomAttribute attr = customAttrs [0] as CustomAttribute;
			Assert.AreEqual (attr.AttributeOne, "TestCase", "#2");
			Assert.AreEqual (attr.Feild, "FieldValue", "#3");
		}
	
		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ArgumentExceptionTest_1 ()
		{
			//here the constructor is static 
					
			Type [] ctorParams = new Type [] { };
			
			ConstructorInfo classCtorInfo = 
				typeof (CustomAttribute).GetConstructor (BindingFlags.Static | BindingFlags.NonPublic,
                                                null, ctorParams, null);

			CustomAttributeBuilder typeCABuilder = new CustomAttributeBuilder (
				classCtorInfo,
				new object [] { },
				new PropertyInfo [] { typeof (CustomAttribute).GetProperty ("AttributeOne") },
				new object [] { "TestCase" },
				typeof (CustomAttribute).GetFields (),
				new object [] { "FieldValue" }
				);
		}

		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ArgumentExceptionTest_2 ()
		{
			//here the consturctor is private
					
			Type [] ctorParams = new Type[] {typeof(string) };
			
			ConstructorInfo classCtorInfo = 
				typeof (CustomAttribute).GetConstructor (BindingFlags.Instance |
                                                BindingFlags.NonPublic, null, ctorParams, null);

			Assert.IsNotNull (classCtorInfo);

			CustomAttributeBuilder typeCABuilder = new CustomAttributeBuilder (
				classCtorInfo,
				new object [] { "hello" },
				new PropertyInfo []{ typeof (CustomAttribute).GetProperty ("AttributeOne") },
				new object [] { "TestCase" },
				typeof (CustomAttribute).GetFields (),
				new object [] { "FieldValue" }
				);
		}

		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ArgumentExceptionTest_3 ()
		{
			// The lengths of the namedProperties and 
			//propertyValues arrays are different. 
			
			Type [] ctorParams = new Type [] { };
			
			ConstructorInfo classCtorInfo = 
				typeof (CustomAttribute).GetConstructor (ctorParams);

			CustomAttributeBuilder typeCABuilder = new CustomAttributeBuilder (
				classCtorInfo,
				new object [] { },
				new PropertyInfo [] { typeof (CustomAttribute).GetProperty ("AttributeOne") },
				new object [] { "TestCase","extra arg" },//<--here is the error
				typeof (CustomAttribute).GetFields (),
				new object [] { "FieldValue" }
				);
		}


		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ArgumentExceptionTest_4()
		{
			//The length of the namedFields and
			//namedValues are different 
			
			Type [] ctorParams = new Type [] { };
			
			ConstructorInfo classCtorInfo = 
				typeof (CustomAttribute).GetConstructor (ctorParams);

			CustomAttributeBuilder typeCABuilder = new CustomAttributeBuilder (
				classCtorInfo,
				new object [] { },
				new PropertyInfo [] { typeof (CustomAttribute).GetProperty ("AttributeOne") },
				new object [] { "TestCase" },
				typeof (CustomAttribute).GetFields (),
				new object [] { }//<--here is the error
				);
		}


		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ArgumentExceptionTest_6 ()
		{
			//The type of supplied argument does not
			//match the type of the parameter declared 
			//in the constructor.
			
			Type [] ctorParams = new Type [] { typeof (string), typeof (string) };
			
			ConstructorInfo classCtorInfo = 
				typeof (CustomAttribute).GetConstructor (ctorParams);

			CustomAttributeBuilder typeCABuilder = new CustomAttributeBuilder (
				classCtorInfo,
				new object [] { "1", 123 },//<--here is the error,(int instead of string)
				new PropertyInfo[]{ typeof (CustomAttribute).GetProperty ("AttributeOne") },
				new object [] { "TestCase" },
				typeof (CustomAttribute).GetFields (),
				new object [] { "FeildValue" }
				);
		}


		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ArgumentExceptionTest_7 ()
		{
			//A property has no setter.(CustomAttribute.AttributeTwo)
						
			Type [] ctorParams = new Type [] { typeof (string), typeof (string) };
			
			ConstructorInfo classCtorInfo = 
				typeof (CustomAttribute).GetConstructor (ctorParams);

			CustomAttributeBuilder typeCABuilder = new CustomAttributeBuilder (
				classCtorInfo,
				new object [] { "1","2" },
				new PropertyInfo []{ typeof (CustomAttribute).GetProperty ("AttributeTwo") },
				new object [] { "TestCase" },
				typeof (CustomAttribute).GetFields (),
				new object [] { "FeildValue" }
				); 
		}


		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ArgumentExceptionTest_8 ()
		{
			//A property doesnot belong to same class
			
			Type [] ctorParams = new Type [] { typeof (string), typeof (string) };
			
			ConstructorInfo classCtorInfo = 
				typeof (CustomAttribute).GetConstructor (ctorParams);

			CustomAttributeBuilder typeCABuilder = new CustomAttributeBuilder (
				classCtorInfo,
				new object [] { "1","2" },
				new PropertyInfo [] { typeof (TempClass).GetProperty ("FieldProperty")}, //here is the error
				new object [] { "TestCase" },
				typeof (CustomAttribute).GetFields (),
				new object [] { "FeildValue" }
				);
		}


		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ArgumentExceptionTest_9 ()
		{
			//A field doesnot belong to same class
			
			Type [] ctorParams = new Type [] { typeof (string), typeof (string) };
			
			ConstructorInfo classCtorInfo = 
				typeof (CustomAttribute).GetConstructor (ctorParams);

			CustomAttributeBuilder typeCABuilder = new CustomAttributeBuilder (
				classCtorInfo,
				new object [] { "1","2" },
				new PropertyInfo []{ typeof (CustomAttribute).GetProperty ("AttributeOne") },
				new object [] {"TestCase"},
				typeof (TempClass).GetFields (), //<-- fields of TempClass are passed
				new object [] { "FeildValue" }
				);
		}


		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ArgumentExceptionTest_10 ()
		{
			//The types of the property values do 
			//not match the types of the named properties.
			
					
			Type [] ctorParams = new Type [] { typeof (string), typeof (string) };
			
			ConstructorInfo classCtorInfo = 
				typeof (CustomAttribute).GetConstructor (ctorParams);

			CustomAttributeBuilder typeCABuilder = new CustomAttributeBuilder (
				classCtorInfo,
				new object [] { "1","2" },
				new PropertyInfo []{ typeof (CustomAttribute).GetProperty ("AttributeOne") },
				new object [] { (long)1212121212 }, //<---type mismatch error(long for string)
				typeof (CustomAttribute).GetFields (),
				new object [] { "FeildValue" }
				);
		}


		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ArgumentExceptionTest_11 ()
		{
			//The types of the field values do 
			//not match the types of the named properties.
			
			Type [] ctorParams = new Type [] { typeof (string), typeof (string) };
			
			ConstructorInfo classCtorInfo = 
				typeof (CustomAttribute).GetConstructor (ctorParams);

			Assert.IsNotNull (classCtorInfo);
			CustomAttributeBuilder typeCABuilder = new CustomAttributeBuilder (
				classCtorInfo,
				new object [] { "1","2" },
				new PropertyInfo [] { typeof (CustomAttribute).GetProperty ("AttributeOne") },
				new object [] { "One" },
				typeof (CustomAttribute).GetFields (),
				new object []{ 12.1212 } //<---type mismatch error(double for string)
				); 
		}


		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ArgumentNullException_1 ()
		{
			//the ctor value array (2nd argument) is null
			Type [] ctorParams = new Type [] { typeof (string),typeof (string) };
			
			ConstructorInfo classCtorInfo = 
				typeof (CustomAttribute).GetConstructor (ctorParams);

			Assert.IsNotNull (classCtorInfo);
			CustomAttributeBuilder typeCABuilder = new CustomAttributeBuilder (
				classCtorInfo,
				null, //<-- here is the error
				new PropertyInfo [] { typeof (CustomAttribute).GetProperty ("AttributeOne") },
				new object [] { "One" },
				typeof (CustomAttribute).GetFields (),
				new object [] { "feild" }
				);
		}


		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ArgumentNullException_2 ()
		{
			//the property value array (4th argument) is null
			Type [] ctorParams = new Type [] { typeof (string), typeof (string) };
			
			ConstructorInfo classCtorInfo = 
				typeof (CustomAttribute).GetConstructor (ctorParams);

			Assert.IsNotNull (classCtorInfo);
			CustomAttributeBuilder typeCABuilder = new CustomAttributeBuilder (
				classCtorInfo,
				new object [] { "one","two" },
				new PropertyInfo []{ typeof (CustomAttribute).GetProperty ("AttributeOne") },
				null, // <-- here is the error
				typeof (CustomAttribute).GetFields (),
				new object [] { "feild" }
				);
		}


		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ArgumentNullException_3 ()
		{
			//the field value array (6th argument) is null
			Type [] ctorParams = new Type [] { typeof (string), typeof (string) };
			
			ConstructorInfo classCtorInfo = 
				typeof (CustomAttribute).GetConstructor (ctorParams);

			Assert.IsNotNull (classCtorInfo);
			CustomAttributeBuilder typeCABuilder = new CustomAttributeBuilder (
				classCtorInfo,
				new object [] { "one","two" },
				new PropertyInfo [] { typeof (CustomAttribute).GetProperty ("AttributeOne") },
				new object [] {"property"},
				typeof (CustomAttribute).GetFields (),
				null // <-- here is the error
				);
		}

		class C {
			public C (object i) {
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ObjectParam_UserDefinedClass ()
		{
			var cab = new CustomAttributeBuilder(
						 typeof (C).GetConstructors ()[0],
						 new object[] { new C (1) });
		}


		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ValueTypeParam_Null ()
		{
	        ConstructorInfo classCtorInfo = 
	            typeof (CattrD).GetConstructors ()[0];
	
	        CustomAttributeBuilder typeCABuilder = new CustomAttributeBuilder (
	            classCtorInfo, new object [] { null });
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ValueTypeArrayParam_Null ()
		{
	        ConstructorInfo classCtorInfo = 
	            typeof (CattrE).GetConstructors ()[0];
	
	        CustomAttributeBuilder typeCABuilder = new CustomAttributeBuilder (
	            classCtorInfo, new object [] { new object[] { null } });
		}
		
		public class CattrD : Attribute
		{
		    public CattrD (bool b) {}
		}
		
		public class CattrE : Attribute
		{
		    public CattrE (bool[] b) {}
		}

		public class JaggedAttr : Attribute {
			public static string[][] Data { get; set; }

			public JaggedAttr (string[][] data) {
				Data = data;
			}
		}

		[Test]
		public void JaggedArrays () {
			var ab = AppDomain.CurrentDomain.DefineDynamicAssembly (new AssemblyName ("Foo"), AssemblyBuilderAccess.Save, tempDir);
			var modb = ab.DefineDynamicModule ("Foo", "Foo.dll");
			var tb = modb.DefineType ("T");
			tb.SetCustomAttribute (new
								   CustomAttributeBuilder(typeof (JaggedAttr).GetConstructors ()[0],
														  new object[] { new string[][] { new string[] { "foo" }, new string[] { "bar" } } }));
			tb.CreateType ();
			ab.Save ("Foo.dll");

			string assemblyPath = Path.Combine (tempDir, "Foo.dll");
			Type t = Assembly.LoadFrom (assemblyPath).GetType ("T");
			Assert.AreEqual (1, t.GetCustomAttributes (false).Length);

			string[][] res = JaggedAttr.Data;
			Assert.AreEqual (2, res.Length);
			Assert.AreEqual ("foo", res [0][0]);
			Assert.AreEqual ("bar", res [1][0]);
		}

		[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
		internal class NonVisibleCustomAttribute : Attribute
		{
			public NonVisibleCustomAttribute () {}
		}

		[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
		public class PublicVisibleCustomAttribute : Attribute
		{
			public PublicVisibleCustomAttribute () {}
		}

		private static void AddCustomClassAttribute (TypeBuilder typeBuilder, Type customAttrType)
		{
			var attribCtorParams = new Type[] {};
			var attribCtorInfo = customAttrType.GetConstructor(attribCtorParams);
			var attribBuilder = new CustomAttributeBuilder(attribCtorInfo, new object[] { });
			typeBuilder.SetCustomAttribute(attribBuilder);
		}

		[Test]
		public void NonvisibleCustomAttribute () {
			//
			// We build:
			//  [VisiblePublicCustom]
			//  [VisiblePublicCustom]
			//  [NonVisibleCustom]
			//  [VisiblePublicCustom]
			//  class BuiltType { public BuiltType () { } }
			//
			// And then we try to get all the attributes.
			//
			// Regression test for https://bugzilla.xamarin.com/show_bug.cgi?id=43291
						var assemblyName = new AssemblyName("Repro43291Asm");
			var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
			var moduleBuilder = assemblyBuilder.DefineDynamicModule("Repro43291Mod");

			var typeBuilder = moduleBuilder.DefineType("BuiltType",
				TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.BeforeFieldInit);

			AddCustomClassAttribute (typeBuilder, typeof (PublicVisibleCustomAttribute));
			AddCustomClassAttribute (typeBuilder, typeof (PublicVisibleCustomAttribute));
			AddCustomClassAttribute (typeBuilder, typeof (NonVisibleCustomAttribute));
			AddCustomClassAttribute (typeBuilder, typeof (PublicVisibleCustomAttribute));

			var createdType = typeBuilder.CreateType ();

			Assert.IsNotNull (createdType);

			var obj = Activator.CreateInstance (createdType);

			Assert.IsNotNull (obj);

			var attrs = obj.GetType ().GetCustomAttributes (typeof (Attribute), true);

			Assert.IsNotNull (attrs);

			Assert.AreEqual (3, attrs.Length);
			Assert.IsInstanceOfType (typeof (PublicVisibleCustomAttribute), attrs[0]);
			Assert.IsInstanceOfType (typeof (PublicVisibleCustomAttribute), attrs[1]);
			Assert.IsInstanceOfType (typeof (PublicVisibleCustomAttribute), attrs[2]);
		}

		[Test]
		public void CustomAttributeSameAssembly () {
			// Regression test for 55681
			//
			// We build:
			// class MyAttr : Attr { public MyAttr () { } }
			// [assembly:MyAttr()]
			//
			// the important bit is that we pass the ConstructorBuilder to the CustomAttributeBuilder
			var assemblyName = new AssemblyName ("Repro55681");
			var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly (assemblyName, AssemblyBuilderAccess.Save, tempDir);
			var moduleBuilder = assemblyBuilder.DefineDynamicModule ("Repro55681", "Repro55681.dll");
			var typeBuilder = moduleBuilder.DefineType ("MyAttr", TypeAttributes.Public, typeof (Attribute));
			ConstructorBuilder ctor = typeBuilder.DefineDefaultConstructor (MethodAttributes.Public);
			typeBuilder.CreateType ();

			assemblyBuilder.SetCustomAttribute (new CustomAttributeBuilder (ctor, new object [] { }));

			assemblyBuilder.Save ("Repro55681.dll");
		}

		[Test]
		public void CustomAttributeAcrossAssemblies () {
			// Regression test for 55681
			//
			// We build:
			// assembly1:
			//   class MyAttr : Attr { public MyAttr () { } }
			// assembly2:
			//   class Dummy { }
			//   [assembly:MyAttr()]
			//
 			// the important bit is that we pass the ConstructorBuilder to the CustomAttributeBuilder
			var assemblyName1 = new AssemblyName ("Repro55681-2a");
			var assemblyBuilder1 = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName1, AssemblyBuilderAccess.Save, tempDir);
			var moduleBuilder1 = assemblyBuilder1.DefineDynamicModule ("Repro55681-2a", "Repro55681-2a.dll");
			var typeBuilder1 = moduleBuilder1.DefineType ("MyAttr", TypeAttributes.Public, typeof (Attribute));
			ConstructorBuilder ctor = typeBuilder1.DefineDefaultConstructor (MethodAttributes.Public);
			typeBuilder1.CreateType ();

			var assemblyName2 = new AssemblyName ("Repro55681-2b");
			var assemblyBuilder2 = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName2, AssemblyBuilderAccess.Save, tempDir);
			var moduleBuilder2 = assemblyBuilder2.DefineDynamicModule ("Repro55681-2b", "Repro55681-2b.dll");

			var typeBuilder2 = moduleBuilder2.DefineType ("Dummy", TypeAttributes.Public);
			typeBuilder2.DefineDefaultConstructor (MethodAttributes.Public);
			typeBuilder2.CreateType ();

			assemblyBuilder2.SetCustomAttribute (new CustomAttributeBuilder (ctor, new object [] { }));

			assemblyBuilder2.Save ("Repro55681-2b.dll");
			assemblyBuilder1.Save ("Repro55681-2a.dll");
		}
		
		[DllImport("SomeLib")]
		private static extern void MethodForNullStringMarshalAsFields([MarshalAs(UnmanagedType.LPWStr)] string param);

		[Test]
		public void NullStringMarshalAsFields () {
			// Regression test for https://github.com/mono/mono/issues/12747
			//
			// MarshalAsAttribute goes through
			// CustomAttributeBuilder.get_umarshal which tries to
			// build an UnmanagedMarshal value by decoding the CAB's data.
			//
			// The data decoding needs to handle null string (encoded as 0xFF) properly.
			var aName = new AssemblyName("Repro12747");
			var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(aName, AssemblyBuilderAccess.RunAndSave, tempDir);
			var module = assembly.DefineDynamicModule(aName.Name, aName.Name + ".dll");

			var prototypeMethodName = nameof(MethodForNullStringMarshalAsFields);
			var someMethod = this.GetType().GetMethod(prototypeMethodName, BindingFlags.Static | BindingFlags.NonPublic);

			var typeBuilder = module.DefineType("NewType" + module.ToString(), TypeAttributes.Class | TypeAttributes.Public);
			var methodBuilder = typeBuilder.DefineMethod("NewMethod", MethodAttributes.Public | MethodAttributes.HideBySig, typeof(void), new[] { typeof(string) });
			var il = methodBuilder.GetILGenerator();
			il.Emit(OpCodes.Ret);

			var param = someMethod.GetParameters()[0];
			var paramBuilder = methodBuilder.DefineParameter(1, param.Attributes, null);
			MarshalAsAttribute attr = param.GetCustomAttribute<MarshalAsAttribute>();
			var attrCtor = typeof(MarshalAsAttribute).GetConstructor(new[] { typeof(UnmanagedType) });
			object[] attrCtorArgs = { attr.Value };

			// copy over the fields from the real MarshalAsAttribute on the parameter of "MethodForNullStringMarshalAsFields",
			// including the ones that were initialized to null
			var srcFields = typeof(MarshalAsAttribute).GetFields(BindingFlags.Public | BindingFlags.Instance);
			var fieldArguments = new FieldInfo[srcFields.Length];
			var fieldArgumentValues = new object[srcFields.Length];
			for(int i = 0; i < srcFields.Length; i++)
			{
				var field =  srcFields[i];
				fieldArguments[i] = field;
				fieldArgumentValues[i] = field.GetValue(attr);
			}

			var attrBuilder = new CustomAttributeBuilder(attrCtor, attrCtorArgs, Array.Empty<PropertyInfo>(), Array.Empty<object>(),
								     fieldArguments, fieldArgumentValues);
			// this encodes the CustomAttributeBuilder as a data
			// blob and then tries to decode it using
			// CustomAttributeBuilder.get_umarshal
			paramBuilder.SetCustomAttribute(attrBuilder);

			var finalType = typeBuilder.CreateType();
			
		}

		[Test]
		public void MethodInfoGetParametersCrash () {
			// Regression test for https://github.com/mono/mono/issues/16570
			//
			// MethodInfo.GetParameters() called on a dynamic assembly would attempt to copy the custom_name and cookie, which could be junk depending
			// on how the union is being used.
			var aName = new AssemblyName("TestAssembly");
			var testAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(aName, AssemblyBuilderAccess.RunAndSave);
			var testModule = testAssembly.DefineDynamicModule(aName.Name, aName.Name + ".dll");

			var typeBuilder = testModule.DefineType("TestType");

			var ctorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);

			var ctorIl = ctorBuilder.GetILGenerator();
			ctorIl.Emit(OpCodes.Ret);

			var methodBuilder = typeBuilder.DefineMethod("TestMethod", MethodAttributes.Public, typeof(void), new[] { typeof(int[]) });
			methodBuilder.DefineParameter(0, ParameterAttributes.Retval, null);
			var paramBuilder = methodBuilder.DefineParameter(1, ParameterAttributes.None, null);

			var attrCtor = typeof(MarshalAsAttribute).GetConstructor(new[] { typeof(UnmanagedType) });
			object[] ctorArgs = { UnmanagedType.LPArray };
			var attr = new CustomAttributeBuilder(attrCtor, ctorArgs);
			paramBuilder.SetCustomAttribute(attr);

			var methodIl = methodBuilder.GetILGenerator();
			methodIl.Emit(OpCodes.Ret);

			var createdType = typeBuilder.CreateType();

			var methodInfo = createdType.GetMethod("TestMethod", BindingFlags.Instance | BindingFlags.Public);
			methodInfo.GetParameters();
		}
	}
}

