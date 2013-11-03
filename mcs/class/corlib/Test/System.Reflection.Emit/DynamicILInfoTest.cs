//
// DynamicILInfoTest.cs - NUnit Test Cases for the DynamicILInfo class
//
// Zoltan Varga (vargaz@gmail.com)
//
// (C) 2011 Novell

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.Reflection.Emit
{
[TestFixture]
public class DynamicILInfoTest
{
	[Test]
	public void GetDynamicILInfo_Unique () {
		DynamicMethod dm = new DynamicMethod("HelloWorld", typeof(string), Type.EmptyTypes, typeof(DynamicILInfoTest), false);
		DynamicILInfo il = dm.GetDynamicILInfo();
		DynamicILInfo il2 = dm.GetDynamicILInfo();
		Assert.IsTrue (Object.ReferenceEquals (il, il2));
	}

	[Test]
	public void GetDynamicMethod () {
		DynamicMethod dm = new DynamicMethod("HelloWorld", typeof(string), Type.EmptyTypes, typeof(DynamicILInfoTest), false);
		DynamicILInfo il = dm.GetDynamicILInfo();
		Assert.AreEqual (dm, il.DynamicMethod);
	}

	[Test]
	public void GetTokenFor_String () {
		DynamicMethod dm = new DynamicMethod("HelloWorld", typeof(string), Type.EmptyTypes, typeof(DynamicILInfoTest), false);
		DynamicILInfo il = dm.GetDynamicILInfo();
 
		byte[] code = { 0x00, 0x72, 0x01, 0x00, 0x00, 0x70, 0x2a };
		int token0 = il.GetTokenFor("ABCD");
		PutInteger4(token0, 0x0002, code);
		il.SetCode(code, 8);
 
		var res = dm.Invoke(null, null);
		Assert.AreEqual ("ABCD", res);
	}

	[Test]
	public void GetTokenFor_Method () {
		DynamicMethod dm = new DynamicMethod("HelloWorld", typeof(string), Type.EmptyTypes, typeof(DynamicILInfoTest), false);
		DynamicILInfo il = dm.GetDynamicILInfo();

		// ldstr "ABCD"; call string::ToLower (); ret
		byte[] code = { 0x00, 0x72, 0x01, 0x00, 0x00, 0x70, 0x28, 0x04, 0x00, 0x00, 0x0a, 0x00, 0x2a };
		int token0 = il.GetTokenFor("ABCD");
		int token1 = il.GetTokenFor(typeof(string).GetMethod("ToLower", Type.EmptyTypes).MethodHandle);
		PutInteger4(token0, 0x0002, code);
		PutInteger4(token1, 0x0007, code);
		il.SetCode(code, 8);
 
		var res = dm.Invoke(null, null);
		Assert.AreEqual ("abcd", res);
	}
	
	[Test] // bug #13969
	public void GetTokenFor_Constructor () {
		var m = typeof (object).GetConstructor (Type.EmptyTypes);
		var dm = new DynamicMethod ("Foo", typeof (void), Type.EmptyTypes);
		var dil = dm.GetDynamicILInfo ();
		dil.GetTokenFor (m.MethodHandle);
	}

	[Test]
	public void GetTokenFor_Type () {
		DynamicMethod dm = new DynamicMethod("HelloWorld", typeof(Type), Type.EmptyTypes, typeof(DynamicILInfoTest), false);
		DynamicILInfo il = dm.GetDynamicILInfo();

		byte[] code = { 0x00, 0xd0, 0x01, 0x00, 0x00, 0x70, 0x28, 0x04, 0x00, 0x00, 0x0a, 0x00, 0x2a };
		int token0 = il.GetTokenFor(typeof (int).TypeHandle);
		int token1 = il.GetTokenFor(typeof(Type).GetMethod("GetTypeFromHandle", new Type[] { typeof(RuntimeTypeHandle) }).MethodHandle);
		PutInteger4(token0, 0x0002, code);
		PutInteger4(token1, 0x0007, code);
		il.SetCode(code, 8);
 
		var res = dm.Invoke(null, null);
		Assert.AreEqual (typeof (int), res);
	}

	[Test]
	public void GetTokenFor_DynamicMethod () {
		DynamicMethod dm = new DynamicMethod("HelloWorld", typeof(RuntimeMethodHandle), Type.EmptyTypes, typeof(DynamicILInfoTest), false);
		DynamicILInfo il = dm.GetDynamicILInfo();
 
		byte[] code = { 0x00, 0xd0, 0x01, 0x00, 0x00, 0x70, 0x2a };
		int token0 = il.GetTokenFor(dm);
		PutInteger4(token0, 0x0002, code);
		il.SetCode(code, 8);
 
		var res = dm.Invoke(null, null);
		Assert.AreEqual (dm.MethodHandle, res);
	}

	static int aField;

	[Test]
	public void GetTokenFor_FieldInfo () {
		aField = aField + 1;
		DynamicMethod dm = new DynamicMethod("HelloWorld", typeof(RuntimeFieldHandle), Type.EmptyTypes, typeof(DynamicILInfoTest), false);
		DynamicILInfo il = dm.GetDynamicILInfo();

		var f = typeof (DynamicILInfoTest).GetField ("aField", BindingFlags.Static|BindingFlags.NonPublic).FieldHandle;
 
		byte[] code = { 0x00, 0xd0, 0x01, 0x00, 0x00, 0x70, 0x2a };
		int token0 = il.GetTokenFor(f);
		PutInteger4(token0, 0x0002, code);
		il.SetCode(code, 8);
 
		var res = dm.Invoke(null, null);
		Assert.AreEqual (f, res);
	}

	static void PutInteger4(int value, int startPos, byte[] array) {
		array[startPos++] = (byte)value;
		array[startPos++] = (byte)(value >> 8);
		array[startPos++] = (byte)(value >> 16);
		array[startPos++] = (byte)(value >> 24);
	}
}
}

