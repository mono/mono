//
// ModuleHandleTest - NUnit Test Cases for the ModuleHandle class
//
// Zoltan Varga (vargaz@freemail.hu)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
//

//
// MISSING TESTS:
// - dynamic assemblies
//

#if NET_2_0

using System;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;
using System.Collections;

using NUnit.Framework;

namespace MonoTests.System
{

[TestFixture]
public class ModuleHandleTest : Assertion
{	
	public static int foo;

	public static int bar {
		get {
			return 0;
		}
	}

	public event EventHandler baz;

	public ModuleHandleTest () {
	}

	public static void gnaf (int i, int j) {
	}

	public ModuleHandle module;

	[SetUp]
	public void SetUp () {
		module = typeof (ModuleHandleTest).Assembly.GetModules ()[0].ModuleHandle;
	}

	[Test]
	public void ResolveTypeToken () {
		// A typedef
		AssertEquals (typeof (ModuleHandleTest), Type.GetTypeFromHandle (module.ResolveTypeHandle (typeof (ModuleHandleTest).MetadataToken)));
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void ResolveTypeTokenInvalidHandle () {
		ModuleHandle.EmptyHandle.ResolveTypeHandle (typeof (ModuleHandleTest).MetadataToken);
	}

	[Test]
	[ExpectedException (typeof (TypeLoadException))]
	public void ResolveTypeTokenInvalidTokenType () {
		module.ResolveTypeHandle (1234);
	}

	[Test]
	[ExpectedException (typeof (TypeLoadException))]
	public void ResolveTypeTokenInvalidTokenType2 () {
		module.ResolveTypeHandle (0x4000001);
	}



	[Test]
	public void ResolveFieldToken () {
		FieldInfo fi = typeof (ModuleHandleTest).GetField ("foo");

		AssertEquals (fi, FieldInfo.GetFieldFromHandle (module.ResolveFieldHandle (fi.MetadataToken)));
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void ResolveFieldTokenInvalidHandle () {
		ModuleHandle.EmptyHandle.ResolveFieldHandle (typeof (ModuleHandleTest).MetadataToken);
	}

	[Test]
	[ExpectedException (typeof (Exception))]
	public void ResolveFieldTokenInvalidTokenType () {
		module.ResolveFieldHandle (1234);
	}

	[Test]
	[ExpectedException (typeof (Exception))]
	public void ResolveFieldTokenInvalidTokenType2 () {
		/* A typedef */
		module.ResolveFieldHandle (0x2000002);
	}

	[Test]
	[ExpectedException (typeof (Exception))]
	public void ResolveFieldTokenInvalidTokenType3 () {
		/* A memberref which points to a method */
		module.ResolveFieldHandle (typeof (Console).GetMethod ("ReadLine").MetadataToken);
	}

	[Test]
	[ExpectedException (typeof (Exception))]
	public void ResolveFieldTokenInvalidToken () {
		/* An out-of-range field def */
		module.ResolveFieldHandle (0x40f0001);
	}




	[Test]
	public void ResolveMethodToken () {
		MethodInfo mi = typeof (ModuleHandleTest).GetMethod ("ResolveMethodToken");

		AssertEquals (mi, MethodInfo.GetMethodFromHandle (module.ResolveMethodHandle (mi.MetadataToken)));
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void ResolveMethodTokenInvalidHandle () {
		ModuleHandle.EmptyHandle.ResolveMethodHandle (typeof (ModuleHandleTest).GetMethod ("ResolveMethodToken").MetadataToken);
	}

	[Test]
	[ExpectedException (typeof (Exception))]
	public void ResolveMethodTokenInvalidTokenType () {
		module.ResolveMethodHandle (1234);
	}

	[Test]
	[ExpectedException (typeof (Exception))]
	public void ResolveMethodTokenInvalidTokenType2 () {
		/* A typedef */
		module.ResolveMethodHandle (0x2000002);
	}

	[Test]
	[ExpectedException (typeof (Exception))]
	public void ResolveMethodTokenInvalidTokenType3 () {
		/* A memberref which points to a field */
		module.ResolveMethodHandle (typeof (Type).GetField ("Delimiter").MetadataToken);
	}

	[Test]
	[ExpectedException (typeof (Exception))]
	public void ResolveMethodTokenInvalidToken () {
		/* An out-of-range method def */
		module.ResolveMethodHandle (0x60f0001);
	}

	[Test]
	public void GetPEKind () {
		PortableExecutableKind pe_kind;
		ImageFileMachine machine;

		module.GetPEKind (out pe_kind, out machine);

		AssertEquals (PortableExecutableKind.ILOnly, pe_kind);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void GetPEKindInvalidHandle () {
		PortableExecutableKind pe_kind;
		ImageFileMachine machine;

		ModuleHandle.EmptyHandle.GetPEKind (out pe_kind, out machine);
	}
}
}

#endif
