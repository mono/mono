//
// EventBuilder.cs - NUnit Test Cases for the EventBuilder class
//
// Zoltan Varga (vargaz@freemail.hu)
//
// (C) Ximian, Inc.  http://www.ximian.com

using System;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

using NUnit.Framework;

namespace MonoTests.System.Reflection.Emit
{

[TestFixture]
public class EventBuilderTest
{
	public delegate void AnEvent (object o);

    private TypeBuilder tb;

	private ModuleBuilder module;

	private EventBuilder eb;

	private MethodBuilder mb;

	[SetUp]
	protected void SetUp () {
		AssemblyName assemblyName = new AssemblyName();
		assemblyName.Name = GetType().FullName;

		AssemblyBuilder assembly 
			= Thread.GetDomain().DefineDynamicAssembly(
				assemblyName, AssemblyBuilderAccess.Run);

		module = assembly.DefineDynamicModule("module1");
		
	    tb = module.DefineType("class1", 
							   TypeAttributes.Public);

		eb = tb.DefineEvent ("event1", EventAttributes.None, typeof (AnEvent));
		mb = 
			tb.DefineMethod ("OnAnEvent",
							 MethodAttributes.Public, typeof (void),
							 new Type [] { typeof (AnEvent) });
		ILGenerator ilgen = mb.GetILGenerator();
		ilgen.Emit (OpCodes.Ret);

		// These two are required
		eb.SetAddOnMethod (mb);
		eb.SetRemoveOnMethod (mb);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void TestSetAddOnMethod1 () {
		eb.SetAddOnMethod (null);
	}

	[Test]
	[ExpectedException (typeof (InvalidOperationException))]
	public void TestSetAddOnMethod2 () {
	  tb.CreateType ();

	  eb.SetAddOnMethod (mb);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void TestSetRaiseMethod1 () {
		eb.SetRaiseMethod (null);
	}

	[Test]
	[ExpectedException (typeof (InvalidOperationException))]
	public void TestSetRaiseMethod2 () {
	  tb.CreateType ();

	  eb.SetRaiseMethod (mb);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void TestSetRemoveAddOnMethod1 () {
		eb.SetRemoveOnMethod (null);
	}

	[Test]
	[ExpectedException (typeof (InvalidOperationException))]
	public void TestSetRemoveAddOnMethod2 () {
	  tb.CreateType ();

	  eb.SetRemoveOnMethod (mb);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void TestAddOtherMethod1 () {
		eb.AddOtherMethod (null);
	}

	[Test]
	[ExpectedException (typeof (InvalidOperationException))]
	public void TestAddOtherMethod2 () {
	  tb.CreateType ();

	  eb.AddOtherMethod (mb);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void TestSetCustomAttribute1 () {
		eb.SetCustomAttribute (null);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void TestSetCustomAttribute2 () {
		eb.SetCustomAttribute (null, new byte [1]);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void TestSetCustomAttribute3 () {
		ConstructorInfo con = typeof (String).GetConstructors ()[0];
		eb.SetCustomAttribute (con, null);
	}

	[Test]
	[ExpectedException (typeof (InvalidOperationException))]
	public void TestSetCustomAttribute4 () {
		tb.CreateType ();

		byte[] custAttrData = { 1, 0, 0, 0, 0};
		Type attrType = Type.GetType
			("System.Reflection.AssemblyKeyNameAttribute");
		Type[] paramTypes = new Type[1];
		paramTypes[0] = typeof(String);
		ConstructorInfo ctorInfo =
			attrType.GetConstructor(paramTypes);

		eb.SetCustomAttribute (ctorInfo, custAttrData);
	}

	[Test]
	[ExpectedException (typeof (InvalidOperationException))]
	public void TestSetCustomAttribute5 () {
		tb.CreateType ();

		eb.SetCustomAttribute (new CustomAttributeBuilder (typeof (MethodImplAttribute).GetConstructor (new Type[1] { typeof (short) }), new object[1] {(short)MethodImplAttributes.Synchronized}));
	}

	[Test]
	public void TestCreation () {
		eb = tb.DefineEvent ("event2", EventAttributes.SpecialName, typeof (AnEvent));

		eb.SetRaiseMethod (mb);
		eb.SetAddOnMethod (mb);
		eb.SetRemoveOnMethod (mb);
		eb.AddOtherMethod (mb);
		eb.AddOtherMethod (mb);

		Type t = tb.CreateType ();

		MethodInfo mi = t.GetMethod ("OnAnEvent");

		EventInfo[] events = t.GetEvents ();
		Assert.AreEqual (2, events.Length);

		{
			EventInfo ev = t.GetEvent ("event1");
			Assert.AreEqual ("event1", ev.Name);
			Assert.AreEqual (EventAttributes.None, ev.Attributes);
			Assert.AreEqual (t, ev.DeclaringType);

			Assert.AreEqual (typeof (AnEvent), ev.EventHandlerType);
			Assert.AreEqual (true, ev.IsMulticast);
			Assert.AreEqual (false, ev.IsSpecialName);

			Assert.AreEqual (mi, ev.GetAddMethod ());
			Assert.AreEqual (null, ev.GetRaiseMethod ());
			Assert.AreEqual (mi, ev.GetRemoveMethod ());
		}

		{
			EventInfo ev = t.GetEvent ("event2");
			Assert.AreEqual ("event2", ev.Name);
			Assert.AreEqual (EventAttributes.SpecialName, ev.Attributes);
			Assert.AreEqual (t, ev.DeclaringType);

			Assert.AreEqual (typeof (AnEvent), ev.EventHandlerType);
			Assert.AreEqual (true, ev.IsMulticast);
			Assert.AreEqual (true, ev.IsSpecialName);

			Assert.AreEqual (mi, ev.GetAddMethod ());
			Assert.AreEqual (mi, ev.GetRaiseMethod ());
			Assert.AreEqual (mi, ev.GetRemoveMethod ());
		}
	}		
		
}
}
