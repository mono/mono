//
// ActivatorTest.cs - NUnit Test Cases for System.Activator
//
// Authors:
//	Nick Drochak <ndrochak@gol.com>
//	Gert Driesen <drieseng@users.sourceforge.net>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
#if !MONOTOUCH && !FULL_AOT_RUNTIME
using System.Reflection.Emit;
#endif
using System.Runtime.InteropServices;
#if !DISABLE_REMOTING
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
#endif
using System.Security;
using System.Security.Permissions;

using NUnit.Framework;

// The class in this namespace is used by the main test class
namespace MonoTests.System.ActivatorTestInternal {

	// We need a COM class to test the Activator class
	[ComVisible (true)]
	public class COMTest : MarshalByRefObject {

		private int id;
		public bool constructorFlag = false;

		public COMTest ()
		{
			id = 0;
		}

		public COMTest (int id)
		{
			this.id = id;
		}

		// This property is visible
		[ComVisible (true)]
		public int Id {
			get { return id; }
			set { id = value; }
		}
	}

	[ComVisible (false)]
	public class NonCOMTest : COMTest {
	}
}

namespace MonoTests.System {

	using MonoTests.System.ActivatorTestInternal;

	class CustomUserType : Type
	{
		public override Assembly Assembly
		{
			get { throw new NotImplementedException (); }
		}

		public override string AssemblyQualifiedName
		{
			get { throw new NotImplementedException (); }
		}

		public override Type BaseType
		{
			get { throw new NotImplementedException (); }
		}

		public override string FullName
		{
			get { throw new NotImplementedException (); }
		}

		public override Guid GUID
		{
			get { throw new NotImplementedException (); }
		}

		protected override TypeAttributes GetAttributeFlagsImpl ()
		{
			throw new NotImplementedException ();
		}

		protected override ConstructorInfo GetConstructorImpl (BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
		{
			throw new NotImplementedException ();
		}

		public override ConstructorInfo[] GetConstructors (BindingFlags bindingAttr)
		{
			throw new NotImplementedException ();
		}

		public override Type GetElementType ()
		{
			throw new NotImplementedException ();
		}

		public override EventInfo GetEvent (string name, BindingFlags bindingAttr)
		{
			throw new NotImplementedException ();
		}

		public override EventInfo[] GetEvents (BindingFlags bindingAttr)
		{
			throw new NotImplementedException ();
		}

		public override FieldInfo GetField (string name, BindingFlags bindingAttr)
		{
			throw new NotImplementedException ();
		}

		public override FieldInfo[] GetFields (BindingFlags bindingAttr)
		{
			throw new NotImplementedException ();
		}

		public override Type GetInterface (string name, bool ignoreCase)
		{
			throw new NotImplementedException ();
		}

		public override Type[] GetInterfaces ()
		{
			throw new NotImplementedException ();
		}

		public override MemberInfo[] GetMembers (BindingFlags bindingAttr)
		{
			throw new NotImplementedException ();
		}

		protected override MethodInfo GetMethodImpl (string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
		{
			throw new NotImplementedException ();
		}

		public override MethodInfo[] GetMethods (BindingFlags bindingAttr)
		{
			throw new NotImplementedException ();
		}

		public override Type GetNestedType (string name, BindingFlags bindingAttr)
		{
			throw new NotImplementedException ();
		}

		public override Type[] GetNestedTypes (BindingFlags bindingAttr)
		{
			throw new NotImplementedException ();
		}

		public override PropertyInfo[] GetProperties (BindingFlags bindingAttr)
		{
			throw new NotImplementedException ();
		}

		protected override PropertyInfo GetPropertyImpl (string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
		{
			throw new NotImplementedException ();
		}

		protected override bool HasElementTypeImpl ()
		{
			throw new NotImplementedException ();
		}

		public override object InvokeMember (string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
		{
			throw new NotImplementedException ();
		}

		protected override bool IsArrayImpl ()
		{
			throw new NotImplementedException ();
		}

		protected override bool IsByRefImpl ()
		{
			throw new NotImplementedException ();
		}

		protected override bool IsCOMObjectImpl ()
		{
			throw new NotImplementedException ();
		}

		protected override bool IsPointerImpl ()
		{
			throw new NotImplementedException ();
		}

		protected override bool IsPrimitiveImpl ()
		{
			throw new NotImplementedException ();
		}

		public override Module Module
		{
			get { throw new NotImplementedException (); }
		}

		public override string Namespace
		{
			get { throw new NotImplementedException (); }
		}

		public override Type UnderlyingSystemType
		{
			get {
				return this;
			}
		}

		public override object[] GetCustomAttributes (Type attributeType, bool inherit)
		{
			throw new NotImplementedException ();
		}

		public override object[] GetCustomAttributes (bool inherit)
		{
			throw new NotImplementedException ();
		}

		public override bool IsDefined (Type attributeType, bool inherit)
		{
			throw new NotImplementedException ();
		}

		public override string Name
		{
			get { throw new NotImplementedException (); }
		}
	}


	[TestFixture]
	public class ActivatorTest {

		private string testLocation = typeof (ActivatorTest).Assembly.Location;

		[Test]
		public void CreateInstance_Type()
		{
			COMTest objCOMTest = (COMTest) Activator.CreateInstance (typeof (COMTest));
			Assert.AreEqual ("MonoTests.System.ActivatorTestInternal.COMTest", (objCOMTest.GetType ()).ToString (), "#A02");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CreateInstance_TypeNull ()
		{
			Activator.CreateInstance ((Type)null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreateInstance_CustomType ()
		{
			Activator.CreateInstance (new CustomUserType ());
		}

#if !DISABLE_REMOTING
		[Test]
		public void CreateInstance_StringString ()
		{
			ObjectHandle objHandle = Activator.CreateInstance (null, "MonoTests.System.ActivatorTestInternal.COMTest");
			COMTest objCOMTest = (COMTest)objHandle.Unwrap ();
			objCOMTest.Id = 2;
			Assert.AreEqual (2, objCOMTest.Id, "#A03");
		}
#endif

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CreateInstance_StringNull ()
		{
			Activator.CreateInstance ((string)null, null);
		}

		[Test]
		[ExpectedException (typeof (TypeLoadException))]
		public void CreateInstance_StringTypeNameDoesNotExists ()
		{
			Activator.CreateInstance ((string)null, "MonoTests.System.ActivatorTestInternal.DoesntExistsCOMTest");
		}

		[Test]
		public void CreateInstance_TypeBool ()
		{
			COMTest objCOMTest = (COMTest)Activator.CreateInstance (typeof (COMTest), false);
			Assert.AreEqual ("MonoTests.System.ActivatorTestInternal.COMTest", objCOMTest.GetType ().ToString (), "#A04");
		}

		[Test]
		public void CreateInstance_TypeObjectArray ()
		{
			object[] objArray = new object[1] { 7 };
			COMTest objCOMTest = (COMTest)Activator.CreateInstance (typeof (COMTest), objArray);
			Assert.AreEqual (7, objCOMTest.Id, "#A05");
		}

#if !MONOTOUCH && !FULL_AOT_RUNTIME
		[Test]
		[ExpectedException (typeof (MissingMethodException))]
		public void CreateInstance_TypeBuilder ()
		{
			Type tb = typeof (TypeBuilder); // no public ctor - but why is it documented as NotSupportedException ?
			ConstructorInfo[] ctors = tb.GetConstructors (BindingFlags.Instance | BindingFlags.NonPublic);
			Activator.CreateInstance (tb, new object [ctors [0].GetParameters ().Length]);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CreateInstance_TypedReference ()
		{
			Activator.CreateInstance (typeof (TypedReference), null);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CreateInstance_ArgIterator ()
		{
			Activator.CreateInstance (typeof (ArgIterator), null);
		}
#endif

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CreateInstance_Void ()
		{
			Activator.CreateInstance (typeof (void), null);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CreateInstance_RuntimeArgumentHandle ()
		{
			Activator.CreateInstance (typeof (RuntimeArgumentHandle), null);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CreateInstance_NotMarshalByReferenceWithActivationAttributes ()
		{
			Activator.CreateInstance (typeof (object), null, new object[1] { null });
		}

		// TODO: Implemente the test methods for all the overriden functions using activationAttribute

		[Test]
		[ExpectedException(typeof(MissingMethodException))]
		public void CreateInstanceAbstract1 () 
		{
			Activator.CreateInstance (typeof (Type));
		}

		[Test]
		[ExpectedException(typeof(MissingMethodException))]
		public void CreateInstanceAbstract2 () 
		{
			Activator.CreateInstance (typeof (Type), true);
		}

		[Test]
		[ExpectedException(typeof(MissingMethodException))]
		public void CreateInstanceAbstract3 () 
		{
			Activator.CreateInstance (typeof (Type), null, null);
		}

		[Test]
		[ExpectedException(typeof(MissingMethodException))]
		public void CreateInstanceAbstract4() 
		{
			Activator.CreateInstance (typeof (Type), BindingFlags.CreateInstance | (BindingFlags.Public | BindingFlags.Instance), null, null, CultureInfo.InvariantCulture, null);
		}

		[Test]
		[ExpectedException (typeof (MissingMethodException))]
		public void CreateInstanceAbstract5 () 
		{
			Activator.CreateInstance (typeof (Type), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, null, CultureInfo.InvariantCulture, null);
		}

		[Test]
		public void CreateInstance_Nullable ()
		{
			Assert.AreEqual (5, Activator.CreateInstance (typeof (Nullable<int>), new object [] { 5 }));
			Assert.AreEqual (typeof (int), Activator.CreateInstance (typeof (Nullable<int>), new object [] { 5 }).GetType ());
			Assert.AreEqual (0, Activator.CreateInstance (typeof (Nullable<int>), new object [] { null }));
			Assert.AreEqual (typeof (int), Activator.CreateInstance (typeof (Nullable<int>), new object [] { null }).GetType ());
			Assert.AreEqual (null, Activator.CreateInstance (typeof (Nullable<int>)));
		}
#if FEATURE_REMOTING
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetObject_TypeNull ()
		{
			Activator.GetObject (null, "tcp://localhost:1234/COMTestUri");
		}

		[Test]
		[Category ("MobileNotWorking")]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetObject_UrlNull ()
		{
			Activator.GetObject (typeof (COMTest), null);
		}
#endif

		// TODO: Implemente the test methods for all the overriden function using activationAttribute

#if !DISABLE_REMOTING
		[Test]
		[Category ("AndroidNotWorking")] // Assemblies aren't accessible using filesystem paths (they're either in apk, embedded in native code or not there at all
		public void CreateInstanceFrom ()
		{
			ObjectHandle objHandle = Activator.CreateInstanceFrom (testLocation, "MonoTests.System.ActivatorTestInternal.COMTest");
			Assert.IsNotNull (objHandle, "#A09");
			objHandle.Unwrap ();
			// TODO: Implement the test methods for all the overriden function using activationAttribute
		}
#endif

#if !MOBILE

		// note: this only ensure that the ECMA key support unification (more test required, outside corlib, for other keys, like MS final).
		private const string CorlibPermissionPattern = "System.Security.Permissions.FileDialogPermission, mscorlib, Version={0}, Culture=neutral, PublicKeyToken=b77a5c561934e089";
		private const string SystemPermissionPattern = "System.Net.DnsPermission, System, Version={0}, Culture=neutral, PublicKeyToken=b77a5c561934e089";
		private const string fx10version = "1.0.3300.0";
		private const string fx11version = "1.0.5000.0";
		private const string fx20version = "2.0.0.0";

		private static object[] psNone = new object [1] { PermissionState.None };

		private void Unification (string fullname)
		{
			Type t = Type.GetType (fullname);
			IPermission p = (IPermission)Activator.CreateInstance (t, psNone);
			string currentVersion = typeof (string).Assembly.GetName ().Version.ToString ();
			Assert.IsTrue ((p.ToString ().IndexOf (currentVersion) > 0), currentVersion);
		}

		[Test]
		public void Unification_FromFx10 ()
		{
			Unification (String.Format (CorlibPermissionPattern, fx10version));
			Unification (String.Format (SystemPermissionPattern, fx10version));
		}

		[Test]
		public void Unification_FromFx11 ()
		{
			Unification (String.Format (CorlibPermissionPattern, fx11version));
			Unification (String.Format (SystemPermissionPattern, fx11version));
		}

		[Test]
		public void Unification_FromFx20 ()
		{
			Unification (String.Format (CorlibPermissionPattern, fx20version));
			Unification (String.Format (SystemPermissionPattern, fx20version));
		}

		[Test]
		public void Unification_FromFx99_Corlib ()
		{
			Unification (String.Format (CorlibPermissionPattern, "9.99.999.9999"));
		}

		[Test]
		[Category ("NotWorking")]
		public void Unification_FromFx99_System ()
		{
			Assert.IsNull (Type.GetType (String.Format (SystemPermissionPattern, "9.99.999.9999")));
		}
#endif
		class foo2<T, U> {}
		class foo1<T> : foo2<T, int> {}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void GenericType_Open1 ()
		{
			Activator.CreateInstance (typeof (foo2<,>));
		}
		[Test, ExpectedException (typeof (ArgumentException))]
		public void GenericType_Open2 ()
		{
			Activator.CreateInstance (typeof (foo1<>));
		}
		[Test]
		public void GenericTypes_Closed ()
		{
			Assert.IsNotNull (Activator.CreateInstance (typeof (foo1<int>)), "foo1<int>");
			Assert.IsNotNull (Activator.CreateInstance (typeof (foo2<long, int>)), "foo2<long, int>");
		}

#if !DISABLE_REMOTING
		[Test]
		public void CreateInstanceCrossDomain ()
		{
			Activator.CreateInstance (AppDomain.CurrentDomain, "mscorlib.dll", "System.Object");
			Activator.CreateInstance (AppDomain.CurrentDomain, "mscorlib.dll", "System.Object", false,
						  BindingFlags.Public | BindingFlags.Instance, null, null, CultureInfo.InvariantCulture,
						  null, null);
		}
#endif

#if !MONOTOUCH && !FULL_AOT_RUNTIME && !DISABLE_REMOTING
		[Test]
		public void CreateInstanceCustomDomain ()
		{
			// FIXME: below works as a standalone case, but does not as a unit test (causes JIT error).
                	Activator.CreateInstance (AppDomain.CreateDomain ("foo"), "mscorlib.dll", "System.Object", false,
						  BindingFlags.Public | BindingFlags.Instance, null, null, null,
						  null, null);
		}
#endif

#if !DISABLE_REMOTING
		[Test]
		public void CreateInstanceCrossDomainNonSerializableArgs ()
		{
			// I'm not sure why this is possible ...
			Activator.CreateInstance (AppDomain.CurrentDomain, "mscorlib.dll", "System.WeakReference", false,
						  BindingFlags.Public | BindingFlags.Instance, null, new object [] {ModuleHandle.EmptyHandle}, null, null, null);
		}
#endif

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CreateInstanceNonSerializableAtts ()
		{
			// even on invalid success it causes different exception though.
			Activator.CreateInstance ("mscorlib.dll", "System.Object", false,
						  BindingFlags.Public | BindingFlags.Instance, null, null, null,
						  new object [] {ModuleHandle.EmptyHandle}, null);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CreateInstanceCrossDomainNonSerializableAtts ()
		{
			// even on invalid success it causes different exception though.
			Activator.CreateInstance (AppDomain.CurrentDomain, "mscorlib.dll", "System.Object", false,
						  BindingFlags.Public | BindingFlags.Instance, null, null, null,
						  new object [] {ModuleHandle.EmptyHandle}, null);
		}

		public class ParamsConstructor {

			public int A;
			public string X;
			public string Y;

			public ParamsConstructor (int a, params string [] s)
			{
				A = a;

				Assert.IsNotNull (s);

				if (s.Length == 0)
					return;

				X = s [0];

				if (s.Length == 1)
					return;

				Y = s [1];
			}
		}

		[Test]
		public void CreateInstanceParamsConstructor ()
		{
			var a = (ParamsConstructor) Activator.CreateInstance (
				typeof (ParamsConstructor), new object [] { 42, "foo", "bar" });

			Assert.AreEqual (42, a.A);
			Assert.AreEqual ("foo", a.X);
			Assert.AreEqual ("bar", a.Y);

			a = (ParamsConstructor) Activator.CreateInstance (
				typeof (ParamsConstructor), new object [] { 42, "foo" });

			Assert.AreEqual (42, a.A);
			Assert.AreEqual ("foo", a.X);
			Assert.AreEqual (null, a.Y);

			a = (ParamsConstructor) Activator.CreateInstance (
				typeof (ParamsConstructor), new object [] { 42 });

			Assert.AreEqual (42, a.A);
			Assert.AreEqual (null, a.X);
			Assert.AreEqual (null, a.Y);

			var b = Activator.CreateInstance (typeof (SimpleParamsObjectConstructor), 1, 2, 3, 4, 5);
			Assert.IsNotNull (b);
		}

		class SimpleParamsObjectConstructor
		{
			public SimpleParamsObjectConstructor (params object[] parameters)
			{
			}
		}

		class SimpleParamsConstructor {

			public string X;
			public string Y;

			public SimpleParamsConstructor (params string [] s)
			{
				Assert.IsNotNull (s);

				if (s.Length == 0)
					return;

				X = s [0];

				if (s.Length == 1)
					return;

				Y = s [1];
			}
		}

		[Test]
		public void CreateInstanceSimpleParamsConstructor ()
		{
			var a = (SimpleParamsConstructor) Activator.CreateInstance (
				typeof (SimpleParamsConstructor), new object [] { "foo", "bar" });

			Assert.AreEqual ("foo", a.X);
			Assert.AreEqual ("bar", a.Y);

			a = (SimpleParamsConstructor) Activator.CreateInstance (
				typeof (SimpleParamsConstructor), new object [0]);

			Assert.AreEqual (null, a.X);
			Assert.AreEqual (null, a.Y);
		}

		class ParamsConstructorWithObjectConversion
		{
			public ParamsConstructorWithObjectConversion (params int[] x)
			{
			}
		}

		[Test]
		public void CreateInstanceParamsConstructorWithObjectConversion ()
		{
			var a = Activator.CreateInstance (typeof(ParamsConstructorWithObjectConversion), new object[] { (object) 2 });
			Assert.IsNotNull (a);
		}
	}
}
