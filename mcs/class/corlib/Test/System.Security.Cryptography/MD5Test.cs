// MD5Test.cs - NUnit Test Cases for System.Security.Cryptography.MD5
// 
// Eduardo Garcia Cebollero (kiwnix@yahoo.es)
//
// (C)  Eduardo Garcia Cebollero.
// (C)  Ximian, Inc.  http://www.ximian.com

using NUnit.Framework;
using System;
using System.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography
{
	public class MD5Test : TestCase
	{
		public MD5Test () : base ("MonoTests.System.Security.Cryptography.MD5Test testsuite") {}
		public MD5Test (string name) : base (name) {}

		protected override void SetUp () 
		{
		}

		protected override void TearDown () {}

		public static ITest Suite
		{
			get { 
				return new TestSuite (typeof (MD5Test)); 
			}
		}

		public void TestComputeHashNull () 
		{
			byte [] dato_vacio = {};
			string MD5_dato_vacio = "d41d8cd98f00b204e9800998ecf8427e";

			string result_str = "";
			
			MD5 md5 = new MD5CryptoServiceProvider ();
			byte [] result = md5.ComputeHash (dato_vacio);
			
			foreach(byte i in result)
				result_str += Convert.ToInt32 (i).ToString ("x2");

			AssertEquals ("#01 MD5 Of {} is wrong", result_str, MD5_dato_vacio);
		}

		public void TestComputeHashA()
		{
			byte [] dato_a = { Convert.ToByte ('a') };
			string MD5_dato_a = "0cc175b9c0f1b6a831c399e269772661";
			string result_str = "";
			MD5 md5 = new MD5CryptoServiceProvider ();
			byte [] result = md5.ComputeHash (dato_a);
			foreach (byte i in result)
				result_str += Convert.ToInt32 (i).ToString ("x2");

			AssertEquals ("#02 MD5 Of 'a' is wrong", result_str, MD5_dato_a);
		}

		public void TestComputeHashB ()
		{
			byte[] dato_b = { Convert.ToByte ('\u00F1') };
			string MD5_dato_b = "edb907361219fb8d50279eabab0b83b1";
			string result_str = "";

			MD5 md5 = new MD5CryptoServiceProvider ();
			byte[] result = md5.ComputeHash (dato_b);
			foreach(byte i in result)
				result_str += Convert.ToInt32 (i).ToString ("x2");

			AssertEquals ("#03 MD5 Of '\u00F1' is wrong", result_str, MD5_dato_b);
		}
	}
}

