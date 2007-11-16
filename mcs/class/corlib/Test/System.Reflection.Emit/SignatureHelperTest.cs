//
// SignatureHelperTest.cs
//
// Author: Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.
//
using System;
using System.Reflection;
using System.Reflection.Emit;
using NUnit.Framework;

namespace MonoTests.System.Reflection.Emit
{
	[TestFixture]
	public class SignatureHelperTest
	{
		[Test]
		public void GetFieldSigHelperNullModule ()
		{
			SignatureHelper.GetFieldSigHelper (null);
		}

		[Test]
		public void GetLocalVarSigHelperNullModule ()
		{
			SignatureHelper.GetLocalVarSigHelper (null);
		}

		[Test]
		public void GetMethodSigHelperNullModule ()
		{
			SignatureHelper.GetMethodSigHelper (null, CallingConventions.Standard, typeof (int));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetFieldSigHelperNormalModule ()
		{
			SignatureHelper.GetFieldSigHelper (typeof (int).Module);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetLocalVarSigHelperNormalModule ()
		{
			SignatureHelper.GetLocalVarSigHelper (typeof (int).Module);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetMethodSigHelperNormalModule ()
		{
			SignatureHelper.GetMethodSigHelper (typeof (int).Module, CallingConventions.Standard, typeof (int));
		}
	}
}
