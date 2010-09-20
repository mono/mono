//
// MethodBuilderTest.cs - NUnit Test Cases for the MethodBuilder class
//
// Zoltan Varga (vargaz@freemail.hu)
//
// (C) Ximian, Inc.  http://www.ximian.com

// TODO:
//  - implement 'Signature' (what the hell it does???) and test it
//  - implement Equals and test it

using System;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using System.Runtime.InteropServices;

using NUnit.Framework;

namespace MonoTests.System.Reflection.Emit
{
	[TestFixture]
	public class MethodBuilderTest
	{
		private TypeBuilder genClass;
		private ModuleBuilder module;

		[SetUp]
		protected void SetUp ()
		{
			AssemblyName assemblyName = new AssemblyName ();
			assemblyName.Name = "MonoTests.System.Reflection.Emit.MethodBuilderTest";

			AssemblyBuilder assembly = Thread.GetDomain ().DefineDynamicAssembly (
				assemblyName, AssemblyBuilderAccess.Run);
			module = assembly.DefineDynamicModule ("module1");
			genClass = module.DefineType (genTypeName (), TypeAttributes.Public);
		}

		static int methodIndexer = 0;
		static int typeIndexer = 0;

		// Return a unique method name
		private string genMethodName ()
		{
			return "m" + (methodIndexer++);
		}

		// Return a unique type name
		private string genTypeName ()
		{
			return "class" + (typeIndexer++);
		}

		[Test]
		public void TestAttributes ()
		{
			MethodBuilder mb = genClass.DefineMethod (
				genMethodName (), MethodAttributes.Public, typeof (void), new Type [0]);
			Assert.AreEqual (MethodAttributes.Public, mb.Attributes);
		}

		[Test]
		public void TestCallingConvention ()
		{
			MethodBuilder mb = genClass.DefineMethod (
				genMethodName (), 0, typeof (void), new Type [0]);
			Assert.AreEqual (CallingConventions.Standard | CallingConventions.HasThis,
				mb.CallingConvention, "CallingConvetion defaults to Standard+HasThis");

			MethodBuilder mb3 = genClass.DefineMethod (
				genMethodName (), 0, CallingConventions.VarArgs, typeof (void), new Type [0]);
			Assert.AreEqual (CallingConventions.VarArgs | CallingConventions.HasThis,
				mb3.CallingConvention, "CallingConvetion works");

			MethodBuilder mb4 = genClass.DefineMethod (
				genMethodName (), MethodAttributes.Static, CallingConventions.Standard,
				typeof (void), new Type [0]);
			Assert.AreEqual (CallingConventions.Standard, mb4.CallingConvention,
				"Static implies !HasThis");
		}

		[Test]
		public void TestDeclaringType ()
		{
			MethodBuilder mb = genClass.DefineMethod (
				genMethodName (), 0, typeof (void), new Type [0]);

			Assert.AreEqual (genClass, mb.DeclaringType, "DeclaringType works");
		}

		[Test]
		public void TestInitLocals ()
		{
			MethodBuilder mb = genClass.DefineMethod (
				genMethodName (), 0, typeof (void), new Type [0]);

			Assert.IsTrue (mb.InitLocals, "#1");
			mb.InitLocals = false;
			Assert.IsFalse (mb.InitLocals, "#2");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void TestMethodHandleIncomplete ()
		{
			MethodBuilder mb = genClass.DefineMethod (
				genMethodName (), 0, typeof (void), new Type [0]);

			RuntimeMethodHandle handle = mb.MethodHandle;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void TestMethodHandleComplete ()
		{
			MethodBuilder mb = genClass.DefineMethod (
				genMethodName (), 0, typeof (void), new Type [0]);
			mb.CreateMethodBody (new byte [2], 1);
			genClass.CreateType ();

			RuntimeMethodHandle handle = mb.MethodHandle;
		}

		[Test]
		public void TestName ()
		{
			string name = genMethodName ();
			MethodBuilder mb = genClass.DefineMethod (
				name, 0, typeof (void), new Type [0]);

			Assert.AreEqual (name, mb.Name);
		}

		[Test]
		public void TestReflectedType ()
		{
			MethodBuilder mb = genClass.DefineMethod (
				genMethodName (), 0, typeof (void), new Type [0]);

			Assert.AreEqual (genClass, mb.ReflectedType);
		}

		[Test]
		public void TestReturnType ()
		{
			MethodBuilder mb = genClass.DefineMethod (
				genMethodName (), 0, typeof (Console), new Type [0]);
			Assert.AreEqual (typeof (Console), mb.ReturnType, "#1");

			MethodBuilder mb2 = genClass.DefineMethod (
				genMethodName (), 0, null, new Type [0]);
			Assert.IsTrue (mb2.ReturnType == null || mb2.ReturnType == typeof (void), "#2");
		}

		[Test]
		public void TestReturnTypeCustomAttributes ()
		{
			MethodBuilder mb = genClass.DefineMethod (
				genMethodName (), 0, typeof (Console), new Type [0]);
			Assert.IsNull (mb.ReturnTypeCustomAttributes);
		}

		/*
		public void TestSignature () {
			MethodBuilder mb = genClass.DefineMethod (
				"m91", 0, typeof (Console), new Type [1] { typeof (Console) });

			Console.WriteLine (mb.Signature);
		}
		*/

		[Test]
		public void TestCreateMethodBody ()
		{
			MethodBuilder mb = genClass.DefineMethod (
				genMethodName (), 0, typeof (void), new Type [0]);

			// Clear body
			mb.CreateMethodBody (null, 999);

			// Check arguments 1.
			try {
				mb.CreateMethodBody (new byte [1], -1);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException) {
			}

			// Check arguments 2.
			try {
				mb.CreateMethodBody (new byte [1], 2);
				Assert.Fail ("#2");
			} catch (ArgumentOutOfRangeException) {
			}

			mb.CreateMethodBody (new byte [2], 1);

			// Could only be called once
			try {
				mb.CreateMethodBody (new byte [2], 1);
				Assert.Fail ("#3");
			} catch (InvalidOperationException) {
			}

			// Can not be called on a created type
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
			MethodBuilder mb2 = tb.DefineMethod (
				genMethodName (), 0, typeof (void), new Type [0]);
			ILGenerator ilgen = mb2.GetILGenerator ();
			ilgen.Emit (OpCodes.Ret);
			tb.CreateType ();

			try {
				mb2.CreateMethodBody (new byte [2], 1);
				Assert.Fail ("#4");
			} catch (InvalidOperationException) {
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestDefineParameterInvalidIndexComplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
			MethodBuilder mb = tb.DefineMethod (genMethodName (), 0, typeof (void),
				new Type [2] { typeof(int), typeof(int) });
			mb.CreateMethodBody (new byte [2], 1);
			tb.CreateType ();
			mb.DefineParameter (-5, ParameterAttributes.None, "param1");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestDefineParameterValidIndexComplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
			MethodBuilder mb = tb.DefineMethod (genMethodName (), 0, typeof (void),
				new Type [2] { typeof(int), typeof(int) });
			mb.CreateMethodBody (new byte [2], 1);
			tb.CreateType ();
			mb.DefineParameter (1, ParameterAttributes.None, "param1");
		}

		[Test]
		public void TestDefineParameter ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
			MethodBuilder mb = tb.DefineMethod (
				genMethodName (), 0, typeof (void),
				new Type [2] { typeof (int), typeof (int) });

			// index out of range

			// This fails on mono because the mono version accepts a 0 index
			/*
			try {
				mb.DefineParameter (0, 0, "param1");
				Fail ();
			} catch (ArgumentOutOfRangeException) {
			}
			*/

			try {
				mb.DefineParameter (3, 0, "param1");
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException) {
			}

			// Normal usage
			mb.DefineParameter (1, 0, "param1");
			mb.DefineParameter (1, 0, "param1");
			mb.DefineParameter (2, 0, null);

			mb.CreateMethodBody (new byte [2], 1);
			tb.CreateType ();
			try {
				mb.DefineParameter (1, 0, "param1");
				Assert.Fail ("#2");
			} catch (InvalidOperationException) {
			}
		}

		[Test]
#if NET_2_0
		// MS.NET 2.x no longer allows a zero length method body
		// to be emitted
		[ExpectedException (typeof (InvalidOperationException))]
#endif
		public void ZeroLengthBodyTest1 ()
		{
			MethodBuilder mb = genClass.DefineMethod (
				genMethodName (), 0, typeof (void),
				new Type [2] { typeof (int), typeof (int) });
			mb.CreateMethodBody (new byte [2], 0);
			genClass.CreateType ();
		}

		// A zero length method body can be created
		[Test]
		public void ZeroLengthBodyTest2 ()
		{
			MethodBuilder mb = genClass.DefineMethod (
				genMethodName (), 0, typeof (void),
				new Type [2] { typeof (int), typeof (int) });
			mb.CreateMethodBody (new byte [2], 0);
		}

		[Test]
		public void TestHashCode ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
			string methodName = genMethodName ();
			MethodBuilder mb = tb.DefineMethod (methodName, 0, typeof (void),
				new Type [2] { typeof(int), typeof(int) });
			Assert.AreEqual (methodName.GetHashCode (), mb.GetHashCode ());
		}

		[Test]
		public void TestGetBaseDefinition ()
		{
			MethodBuilder mb = genClass.DefineMethod (
				genMethodName (), 0, typeof (void), new Type [0]);
			Assert.AreEqual (mb.GetBaseDefinition (), mb);
		}

		[Test]
		public void TestGetILGenerator ()
		{
			MethodBuilder mb = genClass.DefineMethod (
				genMethodName (), 0, typeof (void), new Type [0]);

			// The same instance is returned on the second call
			ILGenerator ilgen1 = mb.GetILGenerator ();
			ILGenerator ilgen2 = mb.GetILGenerator ();

			Assert.AreEqual (ilgen1, ilgen2, "#1");

			// Does not work on unmanaged code
			MethodBuilder mb2 = genClass.DefineMethod (
				genMethodName (), 0, typeof (void), new Type [0]);
			try {
				mb2.SetImplementationFlags (MethodImplAttributes.Unmanaged);
				mb2.GetILGenerator ();
				Assert.Fail ("#2");
			} catch (InvalidOperationException) {
			}
			try {
				mb2.SetImplementationFlags (MethodImplAttributes.Native);
				mb2.GetILGenerator ();
				Assert.Fail ("#3");
			} catch (InvalidOperationException) {
			}
		}

		[Test]
		public void TestMethodImplementationFlags ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
			MethodBuilder mb = tb.DefineMethod (
				genMethodName (), 0, typeof (void), new Type [0]);

			Assert.AreEqual (MethodImplAttributes.Managed | MethodImplAttributes.IL,
				mb.GetMethodImplementationFlags (), "#1");

			mb.SetImplementationFlags (MethodImplAttributes.OPTIL);

			Assert.AreEqual (MethodImplAttributes.OPTIL, mb.GetMethodImplementationFlags (), "#2");

			mb.CreateMethodBody (new byte [2], 1);
			mb.SetImplementationFlags (MethodImplAttributes.Managed);
			tb.CreateType ();
			try {
				mb.SetImplementationFlags (MethodImplAttributes.OPTIL);
				Assert.Fail ("#3");
			} catch (InvalidOperationException) {
			}
		}

		[Test]
		public void TestGetModule ()
		{
			MethodBuilder mb = genClass.DefineMethod (
				genMethodName (), 0, typeof (void), new Type [0]);
			Assert.AreEqual (module, mb.GetModule ());
		}

		[Test]
		public void TestGetParameters ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
			MethodBuilder mb = tb.DefineMethod (genMethodName (), 0, typeof (void),
				new Type [1] { typeof (void) });

			/*
			 * According to the MSDN docs, this method should fail with a
			 * NotSupportedException. In reality, it throws an 
			 * InvalidOperationException under MS .NET, and returns the 
			 * requested data under mono.
			 */
			/*
			try {
				mb.GetParameters ();
				Fail ("#161");
			} catch (InvalidOperationException ex) {
				Console.WriteLine (ex);
			}
			*/
		}

		[Test]
		public void TestGetToken ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
			MethodBuilder mb = tb.DefineMethod (
				genMethodName (), 0, typeof (void), new Type [1] { typeof (void) });
			mb.GetToken ();
		}

		[Test]
		public void TestInvoke ()
		{
			MethodBuilder mb = genClass.DefineMethod (
				genMethodName (), 0, typeof (void),
				new Type [1] { typeof (int) });

			try {
				mb.Invoke (null, new object [1] { 42 });
				Assert.Fail ("#1");
			} catch (NotSupportedException) {
			}

			try {
				mb.Invoke (null, 0, null, new object [1] { 42 }, null);
				Assert.Fail ("#2");
			} catch (NotSupportedException) {
			}
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void TestIsDefined ()
		{
			MethodBuilder mb = genClass.DefineMethod (
				genMethodName (), 0, typeof (void),
				new Type [1] { typeof (int) });
			mb.IsDefined (null, true);
		}

		[Test]
		public void TestGetCustomAttributes_Incomplete ()
		{
			MethodBuilder mb = genClass.DefineMethod (
				genMethodName (), 0, typeof (void),
				new Type [1] { typeof (int) });

			try {
				mb.GetCustomAttributes (true);
				Assert.Fail ("#1");
			} catch (NotSupportedException) {
			}

			try {
				mb.GetCustomAttributes (null, true);
				Assert.Fail ("#2");
			} catch (NotSupportedException) {
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void TestGetCustomAttributes_Complete ()
		{
			MethodBuilder mb = genClass.DefineMethod (
				genMethodName (), 0, typeof (void),
				new Type [1] { typeof (int) });
			mb.GetILGenerator ().Emit (OpCodes.Ret);
			genClass.CreateType ();

			try {
				mb.GetCustomAttributes (true);
				Assert.Fail ("#1");
			} catch (NotSupportedException) {
			}

			try {
				mb.GetCustomAttributes (null, true);
				Assert.Fail ("#2");
			} catch (NotSupportedException) {
			}
		}

		[Test]
		public void TestSetCustomAttribute ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
			string name = genMethodName ();
			MethodBuilder mb = tb.DefineMethod (
				name, MethodAttributes.Public, typeof (void),
				new Type [1] { typeof (int) });

			// Null argument
			try {
				mb.SetCustomAttribute (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}

			byte [] custAttrData = { 1, 0, 0, 0, 0 };
			Type attrType = Type.GetType
				("System.Reflection.AssemblyKeyNameAttribute");
			Type [] paramTypes = new Type [1];
			paramTypes [0] = typeof (String);
			ConstructorInfo ctorInfo =
				attrType.GetConstructor (paramTypes);

			mb.SetCustomAttribute (ctorInfo, custAttrData);

			// Test MethodImplAttribute
			mb.SetCustomAttribute (new CustomAttributeBuilder (typeof (MethodImplAttribute).GetConstructor (new Type [1] { typeof (short) }), new object [1] { (short) MethodImplAttributes.Synchronized }));
			mb.GetILGenerator ().Emit (OpCodes.Ret);

			Type t = tb.CreateType ();

			Assert.AreEqual (t.GetMethod (name).GetMethodImplementationFlags (),
				MethodImplAttributes.Synchronized, "#2");

			// Null arguments again
			try {
				mb.SetCustomAttribute (null, new byte [2]);
				Assert.Fail ("#3");
			} catch (ArgumentNullException) {
			}

			try {
				mb.SetCustomAttribute (ctorInfo, null);
				Assert.Fail ("#4");
			} catch (ArgumentNullException) {
			}
		}

		[Test]
		public void SetCustomAttribute_DllImport_DllName_Empty ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
			MethodBuilder mb = tb.DefineMethod (genMethodName (),
				MethodAttributes.Public, typeof (void),
				new Type [1] { typeof (int) });

			ConstructorInfo ctorInfo = typeof (DllImportAttribute).GetConstructor (
				new Type [] { typeof (string) });
			try {
				mb.SetCustomAttribute (new CustomAttributeBuilder (ctorInfo,
					new object [] { string.Empty }));
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test]
		public void SetCustomAttribute_DllImport_DllName_Null ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
			MethodBuilder mb = tb.DefineMethod (genMethodName (),
				MethodAttributes.Public, typeof (void),
				new Type [1] { typeof (int) });

			ConstructorInfo ctorInfo = typeof (DllImportAttribute).GetConstructor (
				new Type [] { typeof (string) });
			try {
				mb.SetCustomAttribute (new CustomAttributeBuilder (ctorInfo,
					new object [] { null }));
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test]
		public void SetCustomAttribute_SuppressUnmanagedCodeSecurity ()
		{
			string mname = genMethodName ();

			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
			MethodBuilder mb = tb.DefineMethod (mname, MethodAttributes.Public,
				typeof (void), new Type [] { typeof (int), typeof (string) });
			ConstructorInfo attrCtor = typeof (SuppressUnmanagedCodeSecurityAttribute).
				GetConstructor (new Type [0]);
			CustomAttributeBuilder caBuilder = new CustomAttributeBuilder (
				attrCtor, new object [0]);
			Assert.IsTrue ((mb.Attributes & MethodAttributes.HasSecurity) == 0, "#1");
			mb.SetCustomAttribute (caBuilder);
			//Assert.IsTrue ((mb.Attributes & MethodAttributes.HasSecurity) == 0, "#2");
			mb.GetILGenerator ().Emit (OpCodes.Ret);
			Type emittedType = tb.CreateType ();
			MethodInfo emittedMethod = emittedType.GetMethod (mname);
			Assert.AreEqual (MethodAttributes.HasSecurity, emittedMethod.Attributes & MethodAttributes.HasSecurity, "#3");
			//Assert.IsTrue ((mb.Attributes & MethodAttributes.HasSecurity) == 0, "#4");
			object [] emittedAttrs = emittedMethod.GetCustomAttributes (typeof (SuppressUnmanagedCodeSecurityAttribute), true);
			Assert.AreEqual (1, emittedAttrs.Length, "#5");
		}

		[AttributeUsage (AttributeTargets.Parameter)]
		class PrivateAttribute : Attribute
		{

			public PrivateAttribute ()
			{
			}
		}

		[Test]
		public void GetCustomAttributes ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
			MethodBuilder mb = tb.DefineMethod ("foo", MethodAttributes.Public,
												typeof (void),
												new Type [1] { typeof (int) });
			mb.GetILGenerator ().Emit (OpCodes.Ret);

			Type attrType = typeof (ObsoleteAttribute);
			ConstructorInfo ctorInfo =
				attrType.GetConstructor (new Type [] { typeof (String) });

			mb.SetCustomAttribute (new CustomAttributeBuilder (ctorInfo, new object [] { "FOO" }));

			// Check that attributes not accessible are not returned
			mb.SetCustomAttribute (new CustomAttributeBuilder (typeof (PrivateAttribute).GetConstructor (new Type [0]), new object [] { }));

			Type t = tb.CreateType ();

			// Try the created type
			{
				MethodInfo mi = t.GetMethod ("foo");
				object [] attrs = mi.GetCustomAttributes (true);

				Assert.AreEqual (1, attrs.Length, "#A1");
				Assert.IsTrue (attrs [0] is ObsoleteAttribute, "#A2");
				Assert.AreEqual ("FOO", ((ObsoleteAttribute) attrs [0]).Message, "#A3");
			}

			// Try the type builder
			{
				MethodInfo mi = tb.GetMethod ("foo");
				object [] attrs = mi.GetCustomAttributes (true);

				Assert.AreEqual (1, attrs.Length, "#B1");
				Assert.IsTrue (attrs [0] is ObsoleteAttribute, "#B2");
				Assert.AreEqual ("FOO", ((ObsoleteAttribute) attrs [0]).Message, "#B3");
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestAddDeclarativeSecurityAlreadyCreated ()
		{
			MethodBuilder mb = genClass.DefineMethod (
				genMethodName (), MethodAttributes.Public, typeof (void),
				new Type [0]);
			ILGenerator ilgen = mb.GetILGenerator ();
			ilgen.Emit (OpCodes.Ret);
			genClass.CreateType ();

			PermissionSet set = new PermissionSet (PermissionState.Unrestricted);
			mb.AddDeclarativeSecurity (SecurityAction.Demand, set);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestAddDeclarativeSecurityNullPermissionSet ()
		{
			MethodBuilder mb = genClass.DefineMethod (
				genMethodName (), MethodAttributes.Public, typeof (void),
				new Type [0]);
			mb.AddDeclarativeSecurity (SecurityAction.Demand, null);
		}

		[Test]
		public void TestAddDeclarativeSecurityInvalidAction ()
		{
			MethodBuilder mb = genClass.DefineMethod (
				genMethodName (), MethodAttributes.Public, typeof (void),
				new Type [0]);

			SecurityAction [] actions = new SecurityAction [] { 
			SecurityAction.RequestMinimum,
			SecurityAction.RequestOptional,
			SecurityAction.RequestRefuse };
			PermissionSet set = new PermissionSet (PermissionState.Unrestricted);

			foreach (SecurityAction action in actions) {
				try {
					mb.AddDeclarativeSecurity (action, set);
					Assert.Fail ();
				} catch (ArgumentOutOfRangeException) {
				}
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestAddDeclarativeSecurityDuplicateAction ()
		{
			MethodBuilder mb = genClass.DefineMethod (
				genMethodName (), MethodAttributes.Public, typeof (void),
				new Type [0]);
			PermissionSet set = new PermissionSet (PermissionState.Unrestricted);
			mb.AddDeclarativeSecurity (SecurityAction.Demand, set);
			mb.AddDeclarativeSecurity (SecurityAction.Demand, set);
		}

		[AttributeUsage (AttributeTargets.Parameter)]
		class ParamAttribute : Attribute
		{

			public ParamAttribute ()
			{
			}
		}

		[Test]
		public void TestDynamicParams ()
		{
			string mname = genMethodName ();

			MethodBuilder mb = genClass.DefineMethod (
				mname, MethodAttributes.Public, typeof (void),
				new Type [] { typeof (int), typeof (string) });
			ParameterBuilder pb = mb.DefineParameter (1, ParameterAttributes.In, "foo");
			pb.SetConstant (52);
			pb.SetCustomAttribute (new CustomAttributeBuilder (typeof (ParamAttribute).GetConstructors () [0], new object [] { }));
			ParameterBuilder pb2 = mb.DefineParameter (2, 0, "bar");
			pb2.SetConstant ("foo");
			mb.GetILGenerator ().Emit (OpCodes.Ret);

			Type t = genClass.CreateType ();
			MethodInfo m = t.GetMethod (mname);
			ParameterInfo [] pi = m.GetParameters ();

			Assert.AreEqual ("foo", pi [0].Name, "#A1");
			Assert.IsTrue (pi [0].IsIn, "#A2");
			Assert.AreEqual (52, pi [0].DefaultValue, "#A3");
			object [] cattrs = pi [0].GetCustomAttributes (true);
#if NET_2_0
			Assert.AreEqual (1, cattrs.Length, "#A4");
			Assert.AreEqual (typeof (InAttribute), cattrs [0].GetType (), "#A5");
#else
			Assert.AreEqual (0, cattrs.Length, "#A4");
#endif

			cattrs = pi [1].GetCustomAttributes (true);
			Assert.AreEqual ("foo", pi [1].DefaultValue, "#B1");
		}

		[Test]
		public void SetCustomAttribute_DllImport1 ()
		{
			string mname = genMethodName ();

			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
			MethodBuilder mb = tb.DefineMethod (
				mname, MethodAttributes.Public, typeof (void), 
				new Type [] { typeof (int), typeof (string) });

			// Create an attribute with default values
			mb.SetCustomAttribute (new CustomAttributeBuilder(typeof(DllImportAttribute).GetConstructor(new Type[] { typeof(string) }), new object[] { "kernel32" }));

			Type t = tb.CreateType ();

			DllImportAttribute attr = (DllImportAttribute)((t.GetMethod (mname).GetCustomAttributes (typeof (DllImportAttribute), true)) [0]);

			Assert.AreEqual (CallingConvention.Winapi, attr.CallingConvention, "#1");
			Assert.AreEqual (mname, attr.EntryPoint, "#2");
			Assert.AreEqual ("kernel32", attr.Value, "#3");
			Assert.IsFalse (attr.ExactSpelling, "#4");
			Assert.IsTrue (attr.PreserveSig, "#5");
			Assert.IsFalse (attr.SetLastError, "#6");
			Assert.IsFalse (attr.BestFitMapping, "#7");
			Assert.IsFalse (attr.ThrowOnUnmappableChar, "#8");
		}

		[Test]
		public void SetCustomAttribute_DllImport2 () {
			string mname = genMethodName ();

			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
			MethodBuilder mb = tb.DefineMethod (
				mname, MethodAttributes.Public, typeof (void), 
				new Type [] { typeof (int), typeof (string) });

			CustomAttributeBuilder cb = new CustomAttributeBuilder (typeof (DllImportAttribute).GetConstructor (new Type [] {typeof (String)}), new object [] { "foo" }, new FieldInfo [] {typeof (DllImportAttribute).GetField ("EntryPoint"), typeof (DllImportAttribute).GetField ("CallingConvention"), typeof (DllImportAttribute).GetField ("CharSet"), typeof (DllImportAttribute).GetField ("ExactSpelling"), typeof (DllImportAttribute).GetField ("PreserveSig")}, new object [] { "bar", CallingConvention.StdCall, CharSet.Unicode, true, false });
			mb.SetCustomAttribute (cb);

			Type t = tb.CreateType ();

			DllImportAttribute attr = (DllImportAttribute)((t.GetMethod (mname).GetCustomAttributes (typeof (DllImportAttribute), true)) [0]);

			Assert.AreEqual (CallingConvention.StdCall, attr.CallingConvention, "#1");
			Assert.AreEqual (CharSet.Unicode, attr.CharSet, "#2");
			Assert.AreEqual ("bar", attr.EntryPoint, "#3");
			Assert.AreEqual ("foo", attr.Value, "#4");
			Assert.IsTrue (attr.ExactSpelling, "#5");
			Assert.IsFalse (attr.PreserveSig, "#6");
			Assert.IsFalse (attr.SetLastError, "#7");
			Assert.IsFalse (attr.BestFitMapping, "#8");
			Assert.IsFalse (attr.ThrowOnUnmappableChar, "#9");
		}

		[Test]
		public void SetCustomAttribute_DllImport3 () {
			string mname = genMethodName ();

			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
			MethodBuilder mb = tb.DefineMethod (
				mname, MethodAttributes.Public, typeof (void), 
				new Type [] { typeof (int), typeof (string) });

			// Test attributes with three values (on/off/missing)
			CustomAttributeBuilder cb = new CustomAttributeBuilder (typeof (DllImportAttribute).GetConstructor (new Type [] {typeof (String)}), new object [] { "foo" }, new FieldInfo [] { typeof (DllImportAttribute).GetField ("BestFitMapping"), typeof (DllImportAttribute).GetField ("ThrowOnUnmappableChar")}, new object [] { false, false });
			mb.SetCustomAttribute (cb);

			Type t = tb.CreateType ();

			DllImportAttribute attr = (DllImportAttribute)((t.GetMethod (mname).GetCustomAttributes (typeof (DllImportAttribute), true)) [0]);

			Assert.IsFalse (attr.BestFitMapping, "#1");
			Assert.IsFalse (attr.ThrowOnUnmappableChar, "#2");
		}

		[Test]
		public void SetCustomAttribute_DllImport4 ()
		{
			string mname = genMethodName ();

			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
			MethodBuilder mb = tb.DefineMethod (
				mname, MethodAttributes.Public, typeof (void), 
				new Type [] { typeof (int), typeof (string) });

			CustomAttributeBuilder cb = new CustomAttributeBuilder (typeof (DllImportAttribute).GetConstructor (new Type [] {typeof (String)}), new object [] { "foo" }, new FieldInfo [] { typeof (DllImportAttribute).GetField ("SetLastError"), typeof (DllImportAttribute).GetField ("BestFitMapping"), typeof (DllImportAttribute).GetField ("ThrowOnUnmappableChar")}, new object [] { true, true, true });
			mb.SetCustomAttribute (cb);

			Type t = tb.CreateType ();

			DllImportAttribute attr = (DllImportAttribute)((t.GetMethod (mname).GetCustomAttributes (typeof (DllImportAttribute), true)) [0]);

			Assert.IsTrue (attr.SetLastError, "#1");
			Assert.IsTrue (attr.BestFitMapping, "#2");
			Assert.IsTrue (attr.ThrowOnUnmappableChar, "#3");
		}

		public class GenericFoo <T> {
			public static T field;
		}

		[Test]
		public void ILGen_GenericTypeParameterBuilder ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
			MethodBuilder mb = tb.DefineMethod ("box_int", 
												MethodAttributes.Public|MethodAttributes.Static, typeof (object), new Type [] { typeof (int) });

			GenericTypeParameterBuilder[] pars = mb.DefineGenericParameters (new string [] { "foo" });

			ILGenerator ilgen = mb.GetILGenerator ();
			ilgen.Emit (OpCodes.Ldarg_0);
			ilgen.Emit (OpCodes.Box, pars [0]);
			ilgen.Emit (OpCodes.Ret);

			Type t = tb.CreateType ();
			MethodInfo mi = t.GetMethod ("box_int");
			MethodInfo mi2 = mi.MakeGenericMethod (new Type [] { typeof (int) });
			Assert.AreEqual (1, mi2.Invoke (null, new object [] { 1 }));
		}

		public void ILGen_InstantiatedGenericType ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
			MethodBuilder mb = tb.DefineMethod ("return_type", 
												MethodAttributes.Public|MethodAttributes.Static, typeof (object), new Type [] { });

			GenericTypeParameterBuilder[] pars = mb.DefineGenericParameters (new string [] { "foo" });

			ILGenerator ilgen = mb.GetILGenerator ();

			Type genericFoo = typeof (GenericFoo<int>).GetGenericTypeDefinition ().MakeGenericType (new Type [] { pars [0] });

			ilgen.Emit (OpCodes.Ldtoken, genericFoo);
			ilgen.Emit (OpCodes.Call, typeof (Type).GetMethod ("GetTypeFromHandle"));
			ilgen.Emit (OpCodes.Ret);

			Type t = tb.CreateType ();
			MethodInfo mi = t.GetMethod ("box_int");
			MethodInfo mi2 = mi.MakeGenericMethod (new Type [] { typeof (int) });
			Assert.AreEqual (typeof (GenericFoo<int>), mi2.Invoke (null, new object [] { 1 }));
		}

		public void ILGen_InstantiatedTypeBuilder ()
		{
			TypeBuilder genericTb = module.DefineType (genTypeName (), TypeAttributes.Public);
			genericTb.DefineGenericParameters (new string [] { "foo" });
			Type generatedGenericType = genericTb.CreateType ();

			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
			MethodBuilder mb = tb.DefineMethod ("return_type", 
												MethodAttributes.Public|MethodAttributes.Static, typeof (object), new Type [] { });

			GenericTypeParameterBuilder[] pars = mb.DefineGenericParameters (new string [] { "foo" });

			ILGenerator ilgen = mb.GetILGenerator ();

			ilgen.Emit (OpCodes.Ldtoken, genericTb.MakeGenericType (new Type [] { pars [0] }));
			ilgen.Emit (OpCodes.Call, typeof (Type).GetMethod ("GetTypeFromHandle"));
			ilgen.Emit (OpCodes.Ret);

			Type t = tb.CreateType ();
			MethodInfo mi = t.GetMethod ("return_type");
			MethodInfo mi2 = mi.MakeGenericMethod (new Type [] { typeof (int) });
			Assert.AreEqual (generatedGenericType.MakeGenericType (new Type [] { typeof (int) }), mi2.Invoke (null, new object [] { 1 }));
		}

		[Test]
		public void Bug354757 ()
		{
			TypeBuilder gtb = module.DefineType (genTypeName (), TypeAttributes.Public);
			gtb.DefineGenericParameters ("T");
			MethodBuilder mb = gtb.DefineMethod ("foo", MethodAttributes.Public);
			mb.DefineGenericParameters ("S");
			Assert.IsTrue (mb.IsGenericMethodDefinition);

			Type gt = gtb.MakeGenericType (typeof (object));
			MethodInfo m = TypeBuilder.GetMethod (gt, mb);
			Assert.IsTrue (m.IsGenericMethodDefinition);

			MethodInfo mopen = m.MakeGenericMethod (m.GetGenericArguments ());
			Assert.IsFalse (mopen.IsGenericMethodDefinition);
		}

		[Test]
		public void DefineGenericParameters_Names_Empty ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
			MethodBuilder mb = tb.DefineMethod ("foo", MethodAttributes.Public);

			try {
				mb.DefineGenericParameters (new string [0]);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Value does not fall within the expected range
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}



		[Test]
		public void DefineGenericParameters_Names_Null ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
			MethodBuilder mb = tb.DefineMethod ("foo", MethodAttributes.Public);

			try {
				mb.DefineGenericParameters ((string []) null);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("names", ex.ParamName, "#A5");
			}

			try {
				mb.DefineGenericParameters ("K", null, "V");
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("names", ex.ParamName, "#B5");
			}
		}


		public static int Foo<T> (T a, T b) {
			return 99;
		}

		[Test]//bug #591226
		public void GenericMethodIsProperlyInflated ()
		{
			var tb = module.DefineType ("foo");
			var met = typeof (MethodBuilderTest).GetMethod ("Foo");

			var mb = tb.DefineMethod ("myFunc", MethodAttributes.Public | MethodAttributes.Static, typeof (int), Type.EmptyTypes);
			var garg = mb.DefineGenericParameters ("a") [0];
			mb.SetParameters (garg, garg);

			var ilgen = mb.GetILGenerator ();
			ilgen.Emit (OpCodes.Ldarg_0);
			ilgen.Emit (OpCodes.Ldarg_1);
			ilgen.Emit (OpCodes.Call, met.MakeGenericMethod (garg));
			ilgen.Emit (OpCodes.Ret);

			var res = tb.CreateType ();
			var mm = res.GetMethod ("myFunc").MakeGenericMethod (typeof (int));

			var rt = mm.Invoke (null, new object[] { 10, 20 });
			Assert.AreEqual (99, rt, "#1");
		}

	    public static void VarargMethod (string headline, __arglist) {
	        ArgIterator ai = new ArgIterator (__arglist);
	
	        Console.Write (headline);
	        while (ai.GetRemainingCount () > 0)
	            Console.Write (TypedReference.ToObject (ai.GetNextArg ()));
	        Console.WriteLine ();
	    }

		[Test]//bug #626441
		public void CanCallVarargMethods ()
		{
			var tb = module.DefineType ("foo");
			MethodBuilder mb = tb.DefineMethod ("CallVarargMethod", 
				MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard,
				typeof (void), Type.EmptyTypes);

			ILGenerator il = mb.GetILGenerator ();
			MethodInfo miVarargMethod = typeof (MethodBuilderTest).GetMethod ("VarargMethod");
			
			il.Emit (OpCodes.Ldstr, "Hello world from ");
			il.Emit (OpCodes.Call, typeof(Assembly).GetMethod ("GetExecutingAssembly"));
			il.EmitCall (OpCodes.Call, miVarargMethod, new Type[] { typeof(Assembly) });
			
			il.Emit (OpCodes.Ldstr, "Current time: ");
			il.Emit (OpCodes.Call, typeof(DateTime).GetMethod("get_Now"));
			il.Emit (OpCodes.Ldstr, " (UTC ");
			il.Emit (OpCodes.Call, typeof(DateTime).GetMethod("get_UtcNow"));
			il.Emit (OpCodes.Ldstr, ")");
			il.EmitCall (OpCodes.Call, miVarargMethod, new Type[] { typeof (DateTime), typeof (string), typeof (DateTime), typeof (string) });
			il.Emit (OpCodes.Ret);

			Type type = tb.CreateType ();
			type.GetMethod ("CallVarargMethod").Invoke (null, null);
		}

		public static string GenericMethodWithOneArg<T> (T t)
		{
			return t.ToString ();
		}

		[Test]
		public void ParamerersOfGenericArgumentsAreProperlyEncoded ()
		{
			var type = module.DefineType (
				"Bar",
				TypeAttributes.Public
				| TypeAttributes.Abstract
				| TypeAttributes.Sealed,
				typeof (object));

			var foo_method = typeof (MethodBuilderTest).GetMethod ("GenericMethodWithOneArg");

			var method = type.DefineMethod (
				"ReFoo",
				MethodAttributes.Static | MethodAttributes.Public,
				typeof (string),
				new [] { foo_method.GetGenericArguments () [0] });
			method.DefineGenericParameters ("K");

			var il = method.GetILGenerator ();
			il.Emit (OpCodes.Ldarga, 0);
			il.Emit (OpCodes.Constrained, method.GetGenericArguments () [0]);
			il.Emit (OpCodes.Callvirt, typeof (object).GetMethod ("ToString"));
			il.Emit (OpCodes.Ret);

			type.CreateType ();

			var re_foo_open = module.GetType ("Bar").GetMethod ("ReFoo");

			Assert.AreEqual (re_foo_open.GetGenericArguments ()[0], re_foo_open.GetParameters () [0].ParameterType, "#1");
		}

		[Test] // #628660
		public void CanCallGetMethodBodyOnDynamicImageMethod ()
		{
			var type = module.DefineType (
				"CanCallGetMethodBodyOnDynamicImageMethod",
				TypeAttributes.Public,
				typeof (object));

			var baz = type.DefineMethod ("Foo", MethodAttributes.Public | MethodAttributes.Static, typeof (object), Type.EmptyTypes);

			var il = baz.GetILGenerator ();
			var temp = il.DeclareLocal (typeof (object));
			il.Emit (OpCodes.Ldnull);
			il.Emit (OpCodes.Stloc, temp);
			il.Emit (OpCodes.Ldloc, temp);
			il.Emit (OpCodes.Ret);

			var body = type.CreateType ().GetMethod ("Foo").GetMethodBody ();

			Assert.IsNotNull (body);
			Assert.AreEqual (1, body.LocalVariables.Count);
			Assert.AreEqual (typeof (object), body.LocalVariables [0].LocalType);
		}


		[Test] //#384127
		public void GetGenericArgumentsReturnsNullForNonGenericMethod ()
		{
			var tb = module.DefineType ("Base");
	
			var mb = tb.DefineMethod ("foo", MethodAttributes.Public, typeof (void), Type.EmptyTypes);
	
			Assert.IsNull (mb.GetGenericArguments ());

		}
	}
}
