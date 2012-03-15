// ParameterBuilderTest.cs
//
// Author: Gleb Golubitsky <sectoid@gnolltech.org>
//
// (c) 2012 Gleb Golubitsky
//
using System;
using System.Reflection.Emit;
using System.Reflection;
using NUnit.Framework;

namespace MonoTests.System.Reflection.Emit
{
	/// <summary>
	/// A test fixture for System.Reflection.Emit.ParameterBuilder class
	/// </summary>
	[TestFixture]
	public class ParameterBuilderTest
	{
		[Test]
		/// <summary>
		/// Unit test for a problem in ParameterBuilder.SetConstant
		/// method. The problem is described at bug #3912.
		/// (https://bugzilla.xamarin.com/show_bug.cgi?id=3912)
		/// </summary>
		public void ParameterBuilderSetConstant_Bug3912 ()
		{
			var aName = new AssemblyName("DynamicAssemblyExample");
			var ab = AppDomain.CurrentDomain
				.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run);

			var mb = ab.DefineDynamicModule(aName.Name);

			var eb = mb.DefineEnum("TestEnum",
				TypeAttributes.Public,
				typeof(int));

			eb.DefineLiteral("Low", 0);
			eb.DefineLiteral("High", 1);

			var tb = mb.DefineType("MyDynamicType", TypeAttributes.Public);

			var hello = tb.DefineMethod("Fail",
				MethodAttributes.Public,
				typeof(void),
				new [] { eb, });
			var builder = hello.DefineParameter(1, ParameterAttributes.In, "failParam");
			builder.SetConstant(1);
		}
	}
}
