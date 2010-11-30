// CustomAttributeBuilderTest.cs
//
// Author: Vineeth N <nvineeth@yahoo.com>
//
// (C) 2004 Ximian, Inc. http://www.ximian.com
//
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
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
	}
}

