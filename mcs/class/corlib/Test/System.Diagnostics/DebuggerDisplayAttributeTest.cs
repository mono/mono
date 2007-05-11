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
			Assert.AreEqual (string.Empty, dda.Value, "A1");
			Assert.IsNull (dda.Target, "A2");
			Assert.IsNull (dda.TargetTypeName, "A3");
			Assert.AreEqual (string.Empty, dda.Type, "A4");
			Assert.AreEqual (string.Empty, dda.Name, "A4");

			dda = new DebuggerDisplayAttribute ("abc");
			Assert.AreEqual ("abc", dda.Value, "B1");
			Assert.IsNull (dda.Target, "B2");
			Assert.IsNull (dda.TargetTypeName, "B3");
			Assert.AreEqual (string.Empty, dda.Type, "B4");
			Assert.AreEqual (string.Empty, dda.Name, "B4");
		}
		
		[Test]
		public void TargetTest ()
		{
			DebuggerDisplayAttribute dda;

			dda = new DebuggerDisplayAttribute (null);
			Assert.AreEqual (string.Empty, dda.Value, "A1");
			Assert.IsNull (dda.Target, "A2");
			Assert.IsNull (dda.TargetTypeName, "A3");
			Assert.AreEqual (string.Empty, dda.Type, "A4");
			Assert.AreEqual (string.Empty, dda.Name, "A4");

			dda.Target = typeof(string);
			Assert.AreEqual (string.Empty, dda.Value, "B1");
			Assert.IsNotNull (dda.Target, "B2");
			Assert.AreSame (typeof(string), dda.Target, "B2-b");
			Assert.AreEqual (typeof (string).AssemblyQualifiedName, dda.TargetTypeName, "B3");
			Assert.AreEqual (string.Empty, dda.Type, "B4");
			Assert.AreEqual (string.Empty, dda.Name, "B4");

			try {
				dda.Target = null;
				Assert.Fail ("Excepted ArgumentNullException, got no exception.");
			} catch (ArgumentNullException) {
			}
		}

		[Test]
		public void TargetTypeNameTest ()
		{
			DebuggerDisplayAttribute dda;

			dda = new DebuggerDisplayAttribute (null);
			Assert.AreEqual (string.Empty, dda.Value, "A1");
			Assert.IsNull (dda.Target, "A2");
			Assert.IsNull (dda.TargetTypeName, "A3");
			Assert.AreEqual (string.Empty, dda.Type, "A4");
			Assert.AreEqual (string.Empty, dda.Name, "A4");

			dda.TargetTypeName = "System.String";
			Assert.AreEqual (string.Empty, dda.Value, "B1");
			Assert.IsNull (dda.Target, "B2");
			Assert.AreEqual ("System.String", dda.TargetTypeName, "B3");
			Assert.AreEqual (string.Empty, dda.Type, "B4");
			Assert.AreEqual (string.Empty, dda.Name, "B4");

			dda.TargetTypeName = null;
			Assert.AreEqual (string.Empty, dda.Value, "C1");
			Assert.IsNull (dda.Target, "C2");
			Assert.IsNull (dda.TargetTypeName, "C3");
			Assert.AreEqual (string.Empty, dda.Type, "C4");
			Assert.AreEqual (string.Empty, dda.Name, "C4");

			dda.TargetTypeName = string.Empty;
			Assert.AreEqual (string.Empty, dda.Value, "D1");
			Assert.IsNull (dda.Target, "D2");
			Assert.AreEqual (string.Empty, dda.TargetTypeName, "D3");
			Assert.AreEqual (string.Empty, dda.Type, "D4");
			Assert.AreEqual (string.Empty, dda.Name, "D4");
		}

		[Test]
		public void TypeTest ()
		{
			DebuggerDisplayAttribute dda;

			dda = new DebuggerDisplayAttribute (null);
			Assert.AreEqual (string.Empty, dda.Value, "A1");
			Assert.IsNull (dda.Target, "A2");
			Assert.IsNull (dda.TargetTypeName, "A3");
			Assert.AreEqual (string.Empty, dda.Type, "A4");
			Assert.AreEqual (string.Empty, dda.Name, "A4");

			dda.Type = "System.String";
			Assert.AreEqual (string.Empty, dda.Value, "B1");
			Assert.IsNull (dda.Target, "B2");
			Assert.IsNull (dda.TargetTypeName, "B3");
			Assert.AreEqual ("System.String", dda.Type, "B4");
			Assert.AreEqual (string.Empty, dda.Name, "B4");

			dda.Type = null;
			Assert.AreEqual (string.Empty, dda.Value, "C1");
			Assert.IsNull (dda.Target, "C2");
			Assert.IsNull (dda.TargetTypeName, "C3");
			Assert.IsNull (dda.Type, "C4");
			Assert.AreEqual (string.Empty, dda.Name, "C4");

			dda.Type = string.Empty;
			Assert.AreEqual (string.Empty, dda.Value, "D1");
			Assert.IsNull (dda.Target, "D2");
			Assert.IsNull (dda.TargetTypeName, "D3");
			Assert.AreEqual (string.Empty, dda.Type, "D4");
			Assert.AreEqual (string.Empty, dda.Name, "D4");
		}

		[Test]
		public void NameTest ()
		{
			DebuggerDisplayAttribute dda;

			dda = new DebuggerDisplayAttribute (null);
			Assert.AreEqual (string.Empty, dda.Value, "A1");
			Assert.IsNull (dda.Target, "A2");
			Assert.IsNull (dda.TargetTypeName, "A3");
			Assert.AreEqual (string.Empty, dda.Type, "A4");
			Assert.AreEqual (string.Empty, dda.Name, "A4");

			dda.Name = "who?";
			Assert.AreEqual (string.Empty, dda.Value, "B1");
			Assert.IsNull (dda.Target, "B2");
			Assert.IsNull (dda.TargetTypeName, "B3");
			Assert.AreEqual (string.Empty, dda.Type, "B4");
			Assert.AreEqual ("who?", dda.Name, "B4");

			dda.Name = null;
			Assert.AreEqual (string.Empty, dda.Value, "C1");
			Assert.IsNull (dda.Target, "C2");
			Assert.IsNull (dda.TargetTypeName, "C3");
			Assert.AreEqual (string.Empty, dda.Type, "C4");
			Assert.IsNull (dda.Name, "C4");

			dda.Name = string.Empty;
			Assert.AreEqual (string.Empty, dda.Value, "D1");
			Assert.IsNull (dda.Target, "D2");
			Assert.IsNull (dda.TargetTypeName, "D3");
			Assert.AreEqual (string.Empty, dda.Type, "D4");
			Assert.AreEqual (string.Empty, dda.Name, "D4");
		}
	}
}

#endif