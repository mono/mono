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


public class Marshal1 : ICustomMarshaler
{
	public static ICustomMarshaler GetInstance (string s) {
		return new Marshal1 ();
	}

	public void CleanUpManagedData (object managedObj)
	{
	}

	public void CleanUpNativeData (IntPtr pNativeData)
	{
	}

	public int GetNativeDataSize ()
	{
		return 4;
	}

	public IntPtr MarshalManagedToNative (object managedObj)
	{
		return IntPtr.Zero;
 	}

	public object MarshalNativeToManaged (IntPtr pNativeData)
	{
		return null;
	}
}

[TestFixture]
public class ParameterInfoTest : Assertion
{
	public static void paramMethod (int i, [In] int j, [Out] int k, [Optional] int l, [In,Out] int m) {
	}

	[DllImport ("foo")]
	public extern static void marshalAsMethod (
		[MarshalAs(UnmanagedType.Bool)]int p0, 
		[MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPStr)] string [] p1,
		[MarshalAs( UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof (Marshal1), MarshalCookie = "5")] object p2);

#if NET_2_0
	[Test]
	public void PseudoCustomAttributes () {
		ParameterInfo[] info = typeof (ParameterInfoTest).GetMethod ("paramMethod").GetParameters ();
		AssertEquals (0, info[0].GetCustomAttributes (true).Length);
		AssertEquals (1, info[1].GetCustomAttributes (typeof (InAttribute), true).Length);
		AssertEquals (1, info[2].GetCustomAttributes (typeof (OutAttribute), true).Length);
		AssertEquals (1, info[3].GetCustomAttributes (typeof (OptionalAttribute), true).Length);
		AssertEquals (2, info[4].GetCustomAttributes (true).Length);

		ParameterInfo[] pi = typeof (ParameterInfoTest).GetMethod ("marshalAsMethod").GetParameters ();
		MarshalAsAttribute attr;

		attr = (MarshalAsAttribute)(pi [0].GetCustomAttributes (true) [0]);
		AssertEquals (UnmanagedType.Bool, attr.Value);

		attr = (MarshalAsAttribute)(pi [1].GetCustomAttributes (true) [0]);
		AssertEquals (UnmanagedType.LPArray, attr.Value);
		AssertEquals (UnmanagedType.LPStr, attr.ArraySubType);

		attr = (MarshalAsAttribute)(pi [2].GetCustomAttributes (true) [0]);
		AssertEquals (UnmanagedType.CustomMarshaler, attr.Value);
		AssertEquals ("5", attr.MarshalCookie);
		AssertEquals (typeof (Marshal1), Type.GetType (attr.MarshalType));
	}
#endif
}		
}
