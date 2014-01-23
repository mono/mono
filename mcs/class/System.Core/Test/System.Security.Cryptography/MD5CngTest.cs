//
// MD5Test.cs - NUnit Test Cases for System.Security.Cryptography.MD5
// 
// Authors
//	Eduardo Garcia Cebollero (kiwnix@yahoo.es)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C)  Eduardo Garcia Cebollero.
// (C)  Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
//

#if !MOBILE

using NUnit.Framework;
using System;
using System.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography {

	[TestFixture]
	public class MD5Test {

		private MD5 md5;

		[SetUp]
		public void SetUp ()
		{
			md5 = new MD5Cng ();
		}

		[Test]
		public void ComputeHashNull () 
		{
			byte [] dato_vacio = {};
			string MD5_dato_vacio = "d41d8cd98f00b204e9800998ecf8427e";

			string result_str = "";
			
			byte [] result = md5.ComputeHash (dato_vacio);
			
			foreach(byte i in result)
				result_str += Convert.ToInt32 (i).ToString ("x2");

			Assert.AreEqual (result_str, MD5_dato_vacio, "#01 MD5 Of {} is wrong");
		}

		[Test]
		public void ComputeHashA ()
		{
			byte [] dato_a = { Convert.ToByte ('a') };
			string MD5_dato_a = "0cc175b9c0f1b6a831c399e269772661";
			string result_str = "";
			byte [] result = md5.ComputeHash (dato_a);
			foreach (byte i in result)
				result_str += Convert.ToInt32 (i).ToString ("x2");

			Assert.AreEqual (result_str, MD5_dato_a, "#02 MD5 Of 'a' is wrong");
		}

		[Test]
		public void ComputeHashB ()
		{
			byte[] dato_b = { Convert.ToByte ('\u00F1') };
			string MD5_dato_b = "edb907361219fb8d50279eabab0b83b1";
			string result_str = "";

			byte[] result = md5.ComputeHash (dato_b);
			foreach(byte i in result)
				result_str += Convert.ToInt32 (i).ToString ("x2");

			Assert.AreEqual (result_str, MD5_dato_b, "#03 MD5 Of '\u00F1' is wrong");
		}
	}
}

#endif