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

		public enum E : byte {
			E1, E2
		}

		[Test]
		public void SetConstantNullable ()
		{
			// SetConstant  for a Nullable<X> parameter for various X.
			var aName = new AssemblyName ("TestSetConstantNullable");
			var ab = AppDomain.CurrentDomain.DefineDynamicAssembly (aName, AssemblyBuilderAccess.Run);

			var mb = ab.DefineDynamicModule (aName.Name);

			var tb = mb.DefineType ("TestItf", TypeAttributes.Abstract | TypeAttributes.Interface);

			var paramTypes = new[] { typeof (int?), typeof (int?),
						 typeof (E?), typeof (E?) };

			var methb = tb.DefineMethod ("Method", MethodAttributes.Public | MethodAttributes.Abstract | MethodAttributes.Virtual, typeof (void), paramTypes);
			var pb = new ParameterBuilder [paramTypes.Length];
			for (int idx = 0, pnum = 1; idx < paramTypes.Length; idx++, pnum++) {
				pb [idx] = methb.DefineParameter (pnum, ParameterAttributes.Optional | ParameterAttributes.HasDefault, "arg" + idx);
			}

			pb [0].SetConstant (null);
			pb [1].SetConstant (42);
			pb [2].SetConstant (null);
			pb [3].SetConstant (E.E2);

			var t = tb.CreateType ();

			var mi = t.GetMethod ("Method");

			Assert.IsNotNull (mi);

			var ps = mi.GetParameters ();

			Assert.AreEqual (null, ps[0].DefaultValue);
			Assert.AreEqual (42, ps[1].DefaultValue);
			Assert.AreEqual (null, ps[2].DefaultValue);
			Assert.AreEqual ((byte)E.E2, ps[3].DefaultValue);
		}
	}
}
