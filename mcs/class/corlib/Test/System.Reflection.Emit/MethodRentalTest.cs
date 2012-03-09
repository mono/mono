//
// MethodRentalTest.cs - NUnit Test Cases for the MethodRental class
//
// Zoltan Varga (vargaz@freemail.hu)
//
// (C) Ximian, Inc.  http://www.ximian.com

using System;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;

using NUnit.Framework;

namespace MonoTests.System.Reflection.Emit
{
	[TestFixture]
	public class MethodRentalTest
	{	
		private TypeBuilder genClass;
		private ModuleBuilder module;
		private static int methodIndexer = 0;
		private static int typeIndexer = 0;

		[SetUp]
		protected void SetUp ()
		{
			AssemblyName assemblyName = new AssemblyName();
			assemblyName.Name = "MonoTests.System.Reflection.Emit.MethodRentalTest";

			AssemblyBuilder assembly = Thread.GetDomain().DefineDynamicAssembly(
				assemblyName, AssemblyBuilderAccess.Run);

			module = assembly.DefineDynamicModule("module1");
		
			genClass = module.DefineType(genTypeName (), 
				 TypeAttributes.Public);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SwapMethodBodyInvalidMethodSize ()
		{
			MethodRental.SwapMethodBody (null, 0, IntPtr.Zero, 0, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SwapMethodBodyNullType ()
		{
			MethodRental.SwapMethodBody (null, 0, IntPtr.Zero, 1, 0);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void SwapMethodBodyUnfinishedType ()
		{
			MethodRental.SwapMethodBody (genClass, 0, IntPtr.Zero, 1, 0);
		}

		// Return a unique method name
		private string genMethodName ()
		{
			return "m" + (methodIndexer++);
		}

		// Return a unique type name
		private string genTypeName ()
		{
			return "class" + (typeIndexer++);
		}
	}
}
