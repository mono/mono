//
// MonoTests.System.Diagnostics.DebuggerDisplayAttributeTest.cs
//
// Author:
//      Rolf Bjarne Kvinge  (RKvinge@novell.com)
//
// (C) 2007
//

#if NET_2_0

using System;
using System.Diagnostics;
using NUnit.Framework;

namespace MonoTests.System.Diagnostics
{
	/// <summary>
	/// Tests the case where StackFrame is created for specified file name and
	/// location inside it.
	/// </summary>
	[TestFixture]
	public class DebuggerDisplayAttributeTest
	{
		[Test]
		public void ConstructorTest ()
		{
			DebuggerDisplayAttribute dda;

			dda = new DebuggerDisplayAttribute (null);
			Assert.AreEqual ("", dda.Value, "A1");
			Assert.AreEqual (null,dda.Target, "A2");
			Assert.AreEqual (null, dda.TargetTypeName, "A3");
			Assert.AreEqual ("", dda.Type, "A4");
			Assert.AreEqual ("", dda.Name, "A4");

			dda = new DebuggerDisplayAttribute ("abc");
			Assert.AreEqual ("abc", dda.Value, "B1");
			Assert.AreEqual (null,dda.Target, "B2");
			Assert.AreEqual (null, dda.TargetTypeName, "B3");
			Assert.AreEqual ("", dda.Type, "B4");
			Assert.AreEqual ("", dda.Name, "B4");
		}
		
		[Test]
		public void TargetTest ()
		{
			DebuggerDisplayAttribute dda;

			dda = new DebuggerDisplayAttribute (null);
			Assert.AreEqual ("",dda.Value, "A1");
			Assert.AreEqual (null,dda.Target, "A2");
			Assert.AreEqual (null, dda.TargetTypeName, "A3");
			Assert.AreEqual ("", dda.Type, "A4");
			Assert.AreEqual ("", dda.Name, "A4");

			dda.Target = typeof(string);
			Assert.AreEqual ("",dda.Value, "B1");
			Assert.IsNotNull (dda.Target, "B2");
			Assert.AreSame (typeof(string), dda.Target, "B2-b");
			Assert.AreEqual ("System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", dda.TargetTypeName, "B3");
			Assert.AreEqual ("", dda.Type, "B4");
			Assert.AreEqual ("", dda.Name, "B4");

			try {
				dda.Target = null;
				Assert.Fail ("Excepted ArgumentNullException, got no exception.");
			} catch (ArgumentNullException) {
			} catch (Exception exception) {
				Assert.Fail ("Excepted ArgumentNullException, got " + exception.GetType ().Name + ".");
			}
		}

		[Test]
		public void TargetTypeNameTest ()
		{
			DebuggerDisplayAttribute dda;

			dda = new DebuggerDisplayAttribute (null);
			Assert.AreEqual ("",dda.Value, "A1");
			Assert.AreEqual (null,dda.Target, "A2");
			Assert.AreEqual (null, dda.TargetTypeName, "A3");
			Assert.AreEqual ("", dda.Type, "A4");
			Assert.AreEqual ("", dda.Name, "A4");

			dda.TargetTypeName = "System.String";
			Assert.AreEqual ("",dda.Value, "B1");
			Assert.AreEqual (null,dda.Target, "B2");
			Assert.AreEqual ("System.String", dda.TargetTypeName, "B3");
			Assert.AreEqual ("", dda.Type, "B4");
			Assert.AreEqual ("", dda.Name, "B4");

			dda.TargetTypeName = null;
			Assert.AreEqual ("",dda.Value, "C1");
			Assert.AreEqual (null,dda.Target, "C2");
			Assert.AreEqual (null, dda.TargetTypeName, "C3");
			Assert.AreEqual ("", dda.Type, "C4");
			Assert.AreEqual ("", dda.Name, "C4");

			dda.TargetTypeName = "";
			Assert.AreEqual ("",dda.Value, "D1");
			Assert.AreEqual (null,dda.Target, "D2");
			Assert.AreEqual ("", dda.TargetTypeName, "D3");
			Assert.AreEqual ("", dda.Type, "D4");
			Assert.AreEqual ("", dda.Name, "D4");
		}

		[Test]
		public void TypeTest ()
		{
			DebuggerDisplayAttribute dda;

			dda = new DebuggerDisplayAttribute (null);
			Assert.AreEqual ("", dda.Value, "A1");
			Assert.AreEqual (null, dda.Target, "A2");
			Assert.AreEqual (null, dda.TargetTypeName, "A3");
			Assert.AreEqual ("", dda.Type, "A4");
			Assert.AreEqual ("", dda.Name, "A4");

			dda.Type = "System.String";
			Assert.AreEqual ("", dda.Value, "B1");
			Assert.AreEqual (null, dda.Target, "B2");
			Assert.AreEqual (null, dda.TargetTypeName, "B3");
			Assert.AreEqual ("System.String", dda.Type, "B4");
			Assert.AreEqual ("", dda.Name, "B4");

			dda.Type = null;
			Assert.AreEqual ("", dda.Value, "C1");
			Assert.AreEqual (null, dda.Target, "C2");
			Assert.AreEqual (null, dda.TargetTypeName, "C3");
			Assert.AreEqual (null, dda.Type, "C4");
			Assert.AreEqual ("", dda.Name, "C4");

			dda.Type = "";
			Assert.AreEqual ("", dda.Value, "D1");
			Assert.AreEqual (null, dda.Target, "D2");
			Assert.AreEqual (null, dda.TargetTypeName, "D3");
			Assert.AreEqual ("", dda.Type, "D4");
			Assert.AreEqual ("", dda.Name, "D4");
		}

		[Test]
		public void NameTest ()
		{
			DebuggerDisplayAttribute dda;

			dda = new DebuggerDisplayAttribute (null);
			Assert.AreEqual ("", dda.Value, "A1");
			Assert.AreEqual (null, dda.Target, "A2");
			Assert.AreEqual (null, dda.TargetTypeName, "A3");
			Assert.AreEqual ("", dda.Type, "A4");
			Assert.AreEqual ("", dda.Name, "A4");

			dda.Name = "who?";
			Assert.AreEqual ("", dda.Value, "B1");
			Assert.AreEqual (null, dda.Target, "B2");
			Assert.AreEqual (null, dda.TargetTypeName, "B3");
			Assert.AreEqual ("", dda.Type, "B4");
			Assert.AreEqual ("who?", dda.Name, "B4");

			dda.Name = null;
			Assert.AreEqual ("", dda.Value, "C1");
			Assert.AreEqual (null, dda.Target, "C2");
			Assert.AreEqual (null, dda.TargetTypeName, "C3");
			Assert.AreEqual ("", dda.Type, "C4");
			Assert.AreEqual (null, dda.Name, "C4");

			dda.Name = "";
			Assert.AreEqual ("", dda.Value, "D1");
			Assert.AreEqual (null, dda.Target, "D2");
			Assert.AreEqual (null, dda.TargetTypeName, "D3");
			Assert.AreEqual ("", dda.Type, "D4");
			Assert.AreEqual ("", dda.Name, "D4");
		}
	}
}

#endif