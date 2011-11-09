//
// MethodOnTypeBuilderInstTest - NUnit Test Cases for MethodOnTypeBuilderInst
//
// Author:
//   Gert Driesen (drieseng@users.sourceforge.net)
//
// Copyright (C) 2008 Gert Driesen
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if NET_2_0

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

using NUnit.Framework;

namespace MonoTests.System.Reflection.Emit
{
	[TestFixture]
	public class ConstructorOnTypeBuilderInstTest
	{
		private static string ASSEMBLY_NAME = "MonoTests.System.Reflection.Emit.ConstructorOnTypeBuilderInstTest";

		private AssemblyBuilder assembly;
		private ModuleBuilder module;
		private Type typeBarOfInt32;
		private ConstructorBuilder cb;
		private ConstructorInfo ci;
		private TypeBuilder tb;

		[SetUp]
		public void SetUp ()
		{
			SetUp (AssemblyBuilderAccess.RunAndSave);
		}
		
		void SetUp (AssemblyBuilderAccess access)
		{
			AssemblyName assemblyName = new AssemblyName ();
			assemblyName.Name = ASSEMBLY_NAME;

			assembly = AppDomain.CurrentDomain.DefineDynamicAssembly (
				assemblyName, access,
				Path.GetTempPath ());

			module = assembly.DefineDynamicModule ("module1");

			tb = module.DefineType ("Bar");
			GenericTypeParameterBuilder [] typeParams = tb.DefineGenericParameters ("T");

			cb = tb.DefineConstructor (MethodAttributes.Public,
				CallingConventions.Standard,
				new Type [] { typeof (string), typeof (int) });
			ILGenerator ig = cb.GetILGenerator ();
			ig.Emit (OpCodes.Ret);

			typeBarOfInt32 = tb.MakeGenericType (typeof (int));
			ci = TypeBuilder.GetConstructor (typeBarOfInt32, cb);
		}

		[Test]
		[Category ("NotWorking")]
		public void Attributes ()
		{
			Assert.AreEqual (MethodAttributes.PrivateScope |
				MethodAttributes.Public | MethodAttributes.SpecialName,
				ci.Attributes, "#1");
			tb.CreateType ();
			Assert.AreEqual (MethodAttributes.PrivateScope |
				MethodAttributes.Public | MethodAttributes.SpecialName,
				ci.Attributes, "#2");
		}

		[Test]
		[Category ("NotWorking")]
		public void CallingConvention ()
		{
			Assert.AreEqual (CallingConventions.HasThis,
				ci.CallingConvention, "#1");
			tb.CreateType ();
			Assert.AreEqual (CallingConventions.HasThis,
				ci.CallingConvention, "#2");
		}

		[Test]
		public void ContainsGenericParameters ()
		{
			Assert.IsFalse (ci.ContainsGenericParameters, "#1");
			tb.CreateType ();
			Assert.IsFalse (ci.ContainsGenericParameters, "#2");
		}

		[Test]
		public void DeclaringType ()
		{
			Assert.AreSame (typeBarOfInt32, ci.DeclaringType, "#1");
			tb.CreateType ();
			Assert.AreSame (typeBarOfInt32, ci.DeclaringType, "#2");
		}

		[Test] // GetCustomAttributes (Boolean)
		public void GetCustomAttributes1 ()
		{
			try {
				ci.GetCustomAttributes (false);
				Assert.Fail ("#A1");
			} catch (NotSupportedException ex) {
				// The invoked member is not supported in a dynamic module
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			tb.CreateType ();

			try {
				ci.GetCustomAttributes (false);
				Assert.Fail ("#B1");
			} catch (NotSupportedException ex) {
				// The invoked member is not supported in a dynamic module
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test] // GetCustomAttributes (Type, Boolean)
		public void GetCustomAttributes2 ()
		{
			try {
				ci.GetCustomAttributes (typeof (FlagsAttribute), false);
				Assert.Fail ("#A1");
			} catch (NotSupportedException ex) {
				// The invoked member is not supported in a dynamic module
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			tb.CreateType ();

			try {
				ci.GetCustomAttributes (typeof (FlagsAttribute), false);
				Assert.Fail ("#B1");
			} catch (NotSupportedException ex) {
				// The invoked member is not supported in a dynamic module
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test]
		public void GetGenericArguments ()
		{
			try {
				ci.GetGenericArguments ();
				Assert.Fail ("#A1");
			} catch (NotSupportedException ex) {
				// Derived classes must provide an implementation
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			tb.CreateType ();

			try {
				ci.GetGenericArguments ();
				Assert.Fail ("#B1");
			} catch (NotSupportedException ex) {
				// Derived classes must provide an implementation
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test]
		public void GetMethodImplementationFlags ()
		{
			Assert.AreEqual (MethodImplAttributes.Managed,
				ci.GetMethodImplementationFlags (), "#1");
			tb.CreateType ();
			Assert.AreEqual (MethodImplAttributes.Managed,
				ci.GetMethodImplementationFlags (), "#2");
		}

		[Test]
		public void GetParameters ()
		{
			try {
				ci.GetParameters ();
				Assert.Fail ("#A1");
			} catch (NotSupportedException ex) {
				// Type has not been created
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			tb.CreateType ();

			ParameterInfo [] parameters = ci.GetParameters ();
			Assert.IsNotNull (parameters, "#B1");
			Assert.AreEqual (2, parameters.Length, "#B2");

			Assert.AreEqual (ParameterAttributes.None, parameters [0].Attributes, "#C1");
			Assert.IsNull (parameters [0].Name, "#C2");
			Assert.AreEqual (typeof (string), parameters [0].ParameterType, "#C3");
			Assert.AreEqual (0, parameters [0].Position, "#C4");

			Assert.AreEqual (ParameterAttributes.None, parameters [1].Attributes, "#D1");
			Assert.IsNull (parameters [1].Name, "#D2");
			Assert.AreEqual (typeof (int), parameters [1].ParameterType, "#D3");
			Assert.AreEqual (1, parameters [1].Position, "#D4");
		}

		[Test] // Invoke (Object [])
		public void Invoke1 ()
		{
			try {
				ci.Invoke (new object [0]);
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				// Operation is not valid due to the current
				// state of the object
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			tb.CreateType ();

			try {
				ci.Invoke (new object [0]);
				Assert.Fail ("#B1");
			} catch (InvalidOperationException ex) {
				// Operation is not valid due to the current
				// state of the object
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test] // Invoke (Object, Object [])
		public void Invoke2 ()
		{
			try {
				ci.Invoke (null, new object [0]);
				Assert.Fail ("#A1");
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			tb.CreateType ();

			try {
				ci.Invoke (null, new object [0]);
				Assert.Fail ("#B1");
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test] // Invoke (BindingFlags, Binder, Object [], CultureInfo)
		public void Invoke3 ()
		{
			try {
				ci.Invoke (BindingFlags.Default, null, new object [0],
					CultureInfo.InvariantCulture);
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				// Operation is not valid due to the current
				// state of the object
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			tb.CreateType ();

			try {
				ci.Invoke (BindingFlags.Default, null, new object [0],
					CultureInfo.InvariantCulture);
				Assert.Fail ("#B1");
			} catch (InvalidOperationException ex) {
				// Operation is not valid due to the current
				// state of the object
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test] // Invoke (Object, BindingFlags, Binder, Object [], CultureInfo)
		public void Invoke4 ()
		{
			try {
				ci.Invoke (null, BindingFlags.Default, null,
					new object [0], CultureInfo.InvariantCulture);
				Assert.Fail ("#A1");
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			tb.CreateType ();

			try {
				ci.Invoke (null, BindingFlags.Default, null,
					new object [0], CultureInfo.InvariantCulture);
				Assert.Fail ("#B1");
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test]
		public void IsDefined ()
		{
			try {
				ci.IsDefined (typeof (FlagsAttribute), false);
				Assert.Fail ("#A1");
			} catch (NotSupportedException ex) {
				// The invoked member is not supported in a dynamic module
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			tb.CreateType ();

			try {
				ci.IsDefined (typeof (FlagsAttribute), false);
				Assert.Fail ("#B1");
			} catch (NotSupportedException ex) {
				// The invoked member is not supported in a dynamic module
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test]
		public void IsGenericMethodDefinition ()
		{
			Assert.IsFalse (ci.IsGenericMethodDefinition, "#1");
			tb.CreateType ();
			Assert.IsFalse (ci.IsGenericMethodDefinition, "#2");
		}

		[Test]
		public void IsGenericMethod ()
		{
			Assert.IsFalse (ci.IsGenericMethod, "#1");
			tb.CreateType ();
			Assert.IsFalse (ci.IsGenericMethod, "#2");
		}

		[Test]
		public void MemberType ()
		{
			Assert.AreEqual (MemberTypes.Constructor, ci.MemberType, "#1");
			tb.CreateType ();
			Assert.AreEqual (MemberTypes.Constructor, ci.MemberType, "#2");
		}

		[Test]
		public void MethodHandle ()
		{
			try {
				RuntimeMethodHandle handle = ci.MethodHandle;
				Assert.Fail ("#A1:" + handle);
			} catch (NotSupportedException ex) {
				// The invoked member is not supported in a dynamic module
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			tb.CreateType ();

			try {
				RuntimeMethodHandle handle = ci.MethodHandle;
				Assert.Fail ("#B1:" + handle);
			} catch (NotSupportedException ex) {
				// The invoked member is not supported in a dynamic module
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test]
		public void Module ()
		{
			Assert.AreSame (module, ci.Module, "#1");
			tb.CreateType ();
			Assert.AreSame (module, ci.Module, "#2");
		}

		[Test]
		public void Name ()
		{
			Assert.AreEqual (".ctor", ci.Name, "#1");
			tb.CreateType ();
			Assert.AreEqual (".ctor", ci.Name, "#2");
		}

		[Test]
		public void ReflectedType ()
		{
			Assert.AreSame (typeBarOfInt32, ci.ReflectedType, "#1");
			tb.CreateType ();
			Assert.AreSame (typeBarOfInt32, ci.ReflectedType, "#2");
		}

		[Test]
		[Category ("NotDotNet")]
		// CompilerContext no longer supported
		[Category ("NotWorking")]
		public void MetadataTokenWorksUnderCompilerContext  ()
		{
			SetUp (AssemblyBuilderAccess.RunAndSave | (AssemblyBuilderAccess)0x800);
			int cb_token = cb.MetadataToken;
			int inst_token = ci.MetadataToken;
			Assert.AreEqual (cb_token, inst_token, "#1");
		}
	}
}

#endif
