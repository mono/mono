//
// ParameterInfoTest - NUnit Test Cases for the ParameterInfo class
//
// Zoltan Varga (vargaz@freemail.hu)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
//

using System;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

using NUnit.Framework;

namespace MonoTests.System.Reflection
{

[TestFixture]
public class ParameterInfoTest : Assertion
{
	public static void paramMethod (int i, [In] int j, [Out] int k, [Optional] int l, [In,Out] int m) {
	}

#if NET_2_0
	[Test]
	public void PseudoCustomAttributes () {
		ParameterInfo[] info = typeof (ParameterInfoTest).GetMethod ("paramMethod").GetParameters ();
		AssertEquals (0, info[0].GetCustomAttributes (true).Length);
		AssertEquals (1, info[1].GetCustomAttributes (typeof (InAttribute), true).Length);
		AssertEquals (1, info[2].GetCustomAttributes (typeof (OutAttribute), true).Length);
		AssertEquals (1, info[3].GetCustomAttributes (typeof (OptionalAttribute), true).Length);
		AssertEquals (2, info[4].GetCustomAttributes (true).Length);
	}
#endif
}		
}
