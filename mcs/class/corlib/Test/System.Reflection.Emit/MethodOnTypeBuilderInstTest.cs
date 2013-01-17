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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

using NUnit.Framework;

namespace MonoTests.System.Reflection.Emit
{
	[TestFixture]
	public class MethodOnTypeBuilderInstTest
	{
		private static string ASSEMBLY_NAME = "MonoTests.System.Reflection.Emit.MethodOnTypeBuilderInstTest";
		
		private AssemblyBuilder assembly;
		private ModuleBuilder module;
		private MethodBuilder mb_create;
		private MethodBuilder mb_edit;
		private Type typeBarOfT;
		private Type typeBarOfInt32;
		private MethodInfo method_create;
		private MethodInfo method_edit;
		private TypeBuilder typeBuilder;
		private GenericTypeParameterBuilder[] typeParams;
		

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

			TypeBuilder tb = typeBuilder = module.DefineType ("Bar");
			typeParams = tb.DefineGenericParameters ("T");

			ConstructorBuilder cb = tb.DefineConstructor (MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
			ILGenerator ig = cb.GetILGenerator ();
			ig.Emit (OpCodes.Ret);

			typeBarOfT = tb.MakeGenericType (typeParams [0]);

			mb_create = tb.DefineMethod ("create",
				MethodAttributes.Public | MethodAttributes.Static,
				typeBarOfT, Type.EmptyTypes);
			ig = mb_create.GetILGenerator ();
			ig.Emit (OpCodes.Newobj, TypeBuilder.GetConstructor (
				typeBarOfT, cb));
			ig.Emit (OpCodes.Ret);

			mb_edit = tb.DefineMethod ("edit",
				MethodAttributes.Public | MethodAttributes.Static,
				typeBarOfT, Type.EmptyTypes);
			ig = mb_edit.GetILGenerator ();
			ig.Emit (OpCodes.Newobj, TypeBuilder.GetConstructor (
				typeBarOfT, cb));
			ig.Emit (OpCodes.Ret);
			mb_edit.SetParameters (mb_edit.DefineGenericParameters ("X"));

			typeBarOfInt32 = tb.MakeGenericType (typeof (int));

			method_create = TypeBuilder.GetMethod (typeBarOfInt32, mb_create);
			method_edit = TypeBuilder.GetMethod (typeBarOfInt32, mb_edit);
		}

		[Test]
		public void Attributes ()
		{
			MethodAttributes attrs;

			attrs = method_create.Attributes;
			Assert.AreEqual (MethodAttributes.PrivateScope |
				MethodAttributes.Public | MethodAttributes.Static,
				attrs, "#1");
			attrs = method_edit.Attributes;
			Assert.AreEqual (MethodAttributes.PrivateScope |
				MethodAttributes.Public | MethodAttributes.Static,
				attrs, "#2");
		}

		[Test]
		public void CallingConvention ()
		{
			CallingConventions conv;

			conv = method_create.CallingConvention;
			Assert.AreEqual (CallingConventions.Standard, conv, "#1");
			conv = method_edit.CallingConvention;
			Assert.AreEqual (CallingConventions.Standard, conv, "#2");
		}

		[Test]
		[Category ("NotWorking")]
		public void ContainsGenericParameters ()
		{
			try {
				bool genparam = method_create.ContainsGenericParameters;
				Assert.Fail ("#A1:" + genparam);
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			try {
				bool genparam = method_edit.ContainsGenericParameters;
				Assert.Fail ("#B1:" + genparam);
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test]
		public void DeclaringType ()
		{
			Assert.AreSame (typeBarOfInt32, method_create.DeclaringType, "#1");
			Assert.AreSame (typeBarOfInt32, method_edit.DeclaringType, "#2");
		}

		[Test]
		[Category ("NotWorking")]
		public void GetBaseDefinition ()
		{
			try {
				method_create.GetBaseDefinition ();
				Assert.Fail ("#A1");
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			try {
				method_edit.GetBaseDefinition ();
				Assert.Fail ("#B1");
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test] // GetCustomAttributes (Boolean)
		[Category ("NotWorking")]
		public void GetCustomAttributes1 ()
		{
			try {
				method_create.GetCustomAttributes (false);
				Assert.Fail ("#A1");
			} catch (NotSupportedException ex) {
				// The invoked member is not supported in a dynamic module
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			try {
				method_edit.GetCustomAttributes (false);
				Assert.Fail ("#B1");
			} catch (NotSupportedException ex) {
				// The invoked member is not supported in a dynamic module
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test] // GetCustomAttributes (Type, Boolean)
		[Category ("NotWorking")]
		public void GetCustomAttributes2 ()
		{
			try {
				method_create.GetCustomAttributes (typeof (FlagsAttribute), false);
				Assert.Fail ("#A1");
			} catch (NotSupportedException ex) {
				// The invoked member is not supported in a dynamic module
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			try {
				method_edit.GetCustomAttributes (typeof (FlagsAttribute), false);
				Assert.Fail ("#B1");
			} catch (NotSupportedException ex) {
				// The invoked member is not supported in a dynamic module
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void GetGenericArguments ()
		{
			Type [] args;
			
			args = method_create.GetGenericArguments ();
			Assert.IsNull (args, "#A");

			args = method_edit.GetGenericArguments ();
			Assert.IsNotNull (args, "#B1");
			Assert.AreEqual (1, args.Length, "#B2");
			Assert.AreEqual ("X", args [0].Name, "#B3");
		}

		[Test]
		[Category ("NotWorking")]
		public void GetGenericMethodDefinition ()
		{
			MethodInfo method_def;

			method_def = method_create.GetGenericMethodDefinition ();
			Assert.IsNotNull (method_def, "#A1");
			Assert.AreSame (mb_create, method_def, "#A2");

			method_def = method_edit.GetGenericMethodDefinition ();
			Assert.IsNotNull (method_def, "#B1");
			Assert.AreSame (mb_edit, method_def, "#B2");
		}

		[Test]
		public void GetMethodImplementationFlags ()
		{
			MethodImplAttributes flags;
			
			flags = method_create.GetMethodImplementationFlags ();
			Assert.AreEqual (MethodImplAttributes.Managed, flags, "#1");
			flags = method_edit.GetMethodImplementationFlags ();
			Assert.AreEqual (MethodImplAttributes.Managed, flags, "#2");
		}

		[Test]
		[Category ("NotWorking")]
		public void GetParameters ()
		{
			try {
				method_create.GetParameters ();
				Assert.Fail ("#A1");
			} catch (NotSupportedException ex) {
				// Type has not been created
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			try {
				method_edit.GetParameters ();
				Assert.Fail ("#B1");
			} catch (NotSupportedException ex) {
				// Type has not been created
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void Invoke ()
		{
			try {
				method_create.Invoke (null, BindingFlags.Default, null,
					new object [0], CultureInfo.InvariantCulture);
				Assert.Fail ("#A1");
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			try {
				method_edit.Invoke (null, BindingFlags.Default, null,
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
		[Category ("NotWorking")]
		public void IsDefined ()
		{
			try {
				method_create.IsDefined (typeof (FlagsAttribute), false);
				Assert.Fail ("#A1");
			} catch (NotSupportedException ex) {
				// The invoked member is not supported in a dynamic module
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			try {
				method_edit.IsDefined (typeof (FlagsAttribute), false);
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
			Assert.IsFalse (method_create.IsGenericMethodDefinition, "#1");
			Assert.IsTrue (method_edit.IsGenericMethodDefinition, "#2");
		}

		[Test]
		public void IsGenericMethod ()
		{
			Assert.IsFalse (method_create.IsGenericMethod, "#1");
			Assert.IsTrue (method_edit.IsGenericMethod, "#2");
		}

		[Test]
		[Category ("NotWorking")]
		public void MakeGenericMethod ()
		{
			try {
				method_create.MakeGenericMethod (typeof (int));
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				// create is not a GenericMethodDefinition.
				// MakeGenericMethod may only be called on a
				// method for which MethodBase.IsGenericMethodDefinition
				// is true
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			MethodInfo genEdit = method_edit.MakeGenericMethod (typeof (int));
			Assert.IsFalse (genEdit.ContainsGenericParameters, "#B1");
			Assert.IsTrue (genEdit.IsGenericMethod, "#B2");
			Assert.IsFalse (genEdit.IsGenericMethodDefinition, "#B3");
		}

		[Test]
		public void MemberType ()
		{
			Assert.AreEqual (MemberTypes.Method, method_create.MemberType, "#1");
			Assert.AreEqual (MemberTypes.Method, method_edit.MemberType, "#2");
		}

		[Test]
		[Category ("NotWorking")]
		public void MethodHandle ()
		{
			try {
				RuntimeMethodHandle handle = method_create.MethodHandle;
				Assert.Fail ("#A1:" + handle);
			} catch (NotSupportedException ex) {
				// The invoked member is not supported in a dynamic module
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			try {
				RuntimeMethodHandle handle = method_edit.MethodHandle;
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
			Assert.AreSame (module, method_create.Module, "#1");
			Assert.AreSame (module, method_edit.Module, "#2");
		}

		[Test]
		public void Name ()
		{
			Assert.AreEqual ("create", method_create.Name, "#1");
			Assert.AreEqual ("edit", method_edit.Name, "#2");
		}

		[Test]
		public void ReflectedType ()
		{
			Assert.AreSame (typeBarOfInt32, method_create.ReflectedType, "#1");
			Assert.AreSame (typeBarOfInt32, method_edit.ReflectedType, "#2");
		}

		[Test]
		public void ReturnParameter ()
		{
			try {
				ParameterInfo ret = method_create.ReturnParameter;
				Assert.Fail ("#A1:" + (ret != null));
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			try {
				ParameterInfo ret = method_create.ReturnParameter;
				Assert.Fail ("#B1:" + (ret != null));
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void ReturnType ()
		{
			Type ret;
			
			ret = method_create.ReturnType;
			Assert.AreSame (typeBarOfT, ret, "#1");
			ret = method_edit.ReturnType;
			Assert.AreSame (typeBarOfT, ret, "#2");
		}

		[Test]
		[Category ("NotWorking")]
		public void ReturnTypeCustomAttributes ()
		{
			try {
				ICustomAttributeProvider attr_prov = method_create.ReturnTypeCustomAttributes;
				Assert.Fail ("#A1:" + (attr_prov != null));
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			try {
				ICustomAttributeProvider attr_prov = method_edit.ReturnTypeCustomAttributes;
				Assert.Fail ("#B1:" + (attr_prov != null));
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		public class GenericType<T> {
		}

		[Test]
		[Category ("NotDotNet")]
		// CompilerContext no longer supported
		[Category ("NotWorking")]
		public void MetadataTokenWorksUnderCompilerContext  ()
		{
			SetUp (AssemblyBuilderAccess.RunAndSave | (AssemblyBuilderAccess)0x800);
			int mb_token = mb_create.MetadataToken;
			int inst_token = method_create.MetadataToken;
			Assert.AreEqual (mb_token, inst_token, "#1");
		}
		
		[Test]
		[Category ("NotDotNet")] //bug #412965
		// CompilerContext no longer supported
		[Category ("NotWorking")]
		public void NullReturnTypeWorksUnderCompilerContext  ()
		{
			SetUp (AssemblyBuilderAccess.RunAndSave | (AssemblyBuilderAccess)0x800);

			Type oldGinst = typeBuilder.MakeGenericType (typeof (double));
			TypeBuilder.GetMethod (oldGinst, mb_create); //cause it to be inflated

			MethodBuilder method_0 = typeBuilder.DefineMethod ("_0", MethodAttributes.Public, typeParams [0], Type.EmptyTypes);
			method_0.SetReturnType (null);

			Type newGinst = typeBuilder.MakeGenericType (typeof (float));
			
			MethodInfo new_method_0 = TypeBuilder.GetMethod (newGinst, method_0);

			Assert.AreEqual (null, new_method_0.ReturnType, "O#1");
		}

		[Test]
		[Category ("NotDotNet")]
		// CompilerContext no longer supported
		[Category ("NotWorking")]
		public void ReturnTypeWorksUnderCompilerContext  ()
		{
			SetUp (AssemblyBuilderAccess.RunAndSave | (AssemblyBuilderAccess)0x800);

			Type oldGinst = typeBuilder.MakeGenericType (typeof (double));
			TypeBuilder.GetMethod (oldGinst, mb_create); //cause it to be inflated

			MethodBuilder method_0 = typeBuilder.DefineMethod ("_0", MethodAttributes.Public, typeParams [0], Type.EmptyTypes);
			MethodBuilder method_1 = typeBuilder.DefineMethod ("_1", MethodAttributes.Public, typeof (GenericType<>).MakeGenericType (typeParams [0]), Type.EmptyTypes);

			Type newGinst = typeBuilder.MakeGenericType (typeof (float));

			MethodInfo old_method_0 = TypeBuilder.GetMethod (oldGinst, method_0);
			MethodInfo new_method_0 = TypeBuilder.GetMethod (newGinst, method_0);

			MethodInfo old_method_1 = TypeBuilder.GetMethod (oldGinst, method_1);
			MethodInfo new_method_1 = TypeBuilder.GetMethod (newGinst, method_1);

			Assert.AreEqual (typeof (double), old_method_0.ReturnType, "O#1");
			Assert.AreEqual (typeof (float), new_method_0.ReturnType, "N#1");

			Assert.AreEqual (typeof (GenericType <double>), old_method_1.ReturnType, "O#1");
			Assert.AreEqual (typeof (GenericType <float>), new_method_1.ReturnType, "N#1");
		}

		[Test]
		[Category ("NotDotNet")]
		// CompilerContext no longer supported
		[Category ("NotWorking")]
		public void GetParametersWorksUnderCompilerContext  ()
		{
			SetUp (AssemblyBuilderAccess.RunAndSave | (AssemblyBuilderAccess)0x800);

			Type oldGinst = typeBuilder.MakeGenericType (typeof (double));
			TypeBuilder.GetMethod (oldGinst, mb_create); //cause it to be inflated

			MethodBuilder target_method = typeBuilder.DefineMethod ("_1", MethodAttributes.Public, typeof (void), 
			new Type[] {
				typeof (int),
				typeParams [0],
				typeParams [0].MakeArrayType (),
				typeParams [0].MakePointerType (),
				typeParams [0].MakeByRefType (),
				typeof (GenericType<>).MakeGenericType (typeParams [0]),
				typeof (GenericType<>).MakeGenericType (typeof (GenericType<>).MakeGenericType (typeParams [0]))});


			Type newGinst = typeBuilder.MakeGenericType (typeof (float));

			MethodInfo old_method = TypeBuilder.GetMethod (oldGinst, target_method);
			MethodInfo new_method = TypeBuilder.GetMethod (newGinst, target_method);
			ParameterInfo[] old_params = old_method.GetParameters ();
			ParameterInfo[] new_params = new_method.GetParameters ();

			Assert.AreEqual (typeof (int), old_params [0].ParameterType, "O#1");
			Assert.AreEqual (typeof (double), old_params [1].ParameterType, "O#2");
			Assert.AreEqual (typeof (double).MakeArrayType (), old_params [2].ParameterType, "O#3");

			Assert.AreEqual (typeof (double).MakePointerType (), old_params [3].ParameterType, "O#4");
			
			Assert.AreEqual (typeof (double).MakeByRefType (), old_params [4].ParameterType, "O#5");
			Assert.AreEqual (typeof (GenericType <double>), old_params [5].ParameterType, "O#6");
			Assert.AreEqual (typeof (GenericType <GenericType<double>>), old_params [6].ParameterType, "O#7");

			Assert.AreEqual (typeof (int), new_params [0].ParameterType, "N#1");
			Assert.AreEqual (typeof (float), new_params [1].ParameterType, "N#2");
			Assert.AreEqual (typeof (float).MakeArrayType (), new_params [2].ParameterType, "N#3");

			Assert.AreEqual (typeof (float).MakePointerType (), new_params [3].ParameterType, "N#4");

			Assert.AreEqual (typeof (float).MakeByRefType (), new_params [4].ParameterType, "N#5");
			Assert.AreEqual (typeof (GenericType <float>), new_params [5].ParameterType, "N#6");
			Assert.AreEqual (typeof (GenericType <GenericType<float>>), new_params [6].ParameterType, "N#7");
		}

		[Test]
		public void GenericMethodInstanceValues ()
		{
			var tb = module.DefineType ("foo.type");
			var gparam = tb.DefineGenericParameters ("T") [0];

			var mb0 = tb.DefineMethod ("str2", MethodAttributes.Public | MethodAttributes.Static, typeof (object), new Type[] { gparam, gparam.MakeArrayType () });

			var mb = tb.DefineMethod ("str", MethodAttributes.Public | MethodAttributes.Static, typeof (object), new Type [0]);
			var mparam = mb.DefineGenericParameters ("K") [0];
			mb.SetReturnType (mparam);
			mb.SetParameters (new Type[] { mparam, mparam.MakeArrayType () });

			var ginst = tb.MakeGenericType (typeof (double));
			var gmd = TypeBuilder.GetMethod (ginst, mb);
			var minst = gmd.MakeGenericMethod (typeof (int));

			var mmb = TypeBuilder.GetMethod (ginst, mb0);

			Assert.IsNull (mmb.GetGenericArguments (), "#1");
			Assert.AreEqual (1, gmd.GetGenericArguments ().Length, "#2");
			Assert.AreEqual (1, minst.GetGenericArguments ().Length, "#3");
			Assert.AreEqual (typeof (int), minst.GetGenericArguments () [0], "#4");

			try {
				var x = mmb.ContainsGenericParameters;
				Assert.Fail ("#5");
			} catch (NotSupportedException) { }

			Assert.IsTrue (gmd.IsGenericMethodDefinition, "#6");
			Assert.IsFalse (minst.IsGenericMethodDefinition, "#7");

			Assert.IsFalse (mmb.IsGenericMethod, "#8");
			Assert.IsTrue (gmd.IsGenericMethod, "#9");
			Assert.IsTrue (minst.IsGenericMethod, "#10");

			Assert.AreEqual (mb0, mmb.GetGenericMethodDefinition (), "#11");
			Assert.AreEqual (mb, gmd.GetGenericMethodDefinition (), "#12");
			Assert.AreEqual (gmd, minst.GetGenericMethodDefinition (), "#13");
		}

		[Test]
		[Category ("NotDotNet")]
		// CompilerContext no longer supported
		[Category ("NotWorking")]
		public void GenericMethodInstanceValuesUnderCompilerContext ()
		{
			SetUp (AssemblyBuilderAccess.RunAndSave | (AssemblyBuilderAccess)0x800);

			var tb = module.DefineType ("foo.type");
			var gparam = tb.DefineGenericParameters ("T") [0];

			var mb0 = tb.DefineMethod ("str2", MethodAttributes.Public | MethodAttributes.Static, typeof (object), new Type[] { gparam, gparam.MakeArrayType () });

			var mb = tb.DefineMethod ("str", MethodAttributes.Public | MethodAttributes.Static, typeof (object), new Type [0]);
			var mparam = mb.DefineGenericParameters ("K") [0];
			mb.SetReturnType (mparam);
			mb.SetParameters (new Type[] { mparam, mparam.MakeArrayType () });

			var ginst = tb.MakeGenericType (typeof (double));
			var gmd = TypeBuilder.GetMethod (ginst, mb);
			var minst = gmd.MakeGenericMethod (typeof (int));

			var mmb = TypeBuilder.GetMethod (ginst, mb0);

			Assert.AreEqual (mparam, gmd.ReturnType, "#1");
			Assert.AreEqual (typeof (int), minst.ReturnType, "#2");

			Assert.AreEqual (mparam, gmd.GetParameters ()[0].ParameterType, "#3");
			Assert.IsTrue (gmd.GetParameters ()[1].ParameterType.IsArray, "#4");
			Assert.AreEqual (typeof (int), minst.GetParameters ()[0].ParameterType, "#5");
			Assert.AreEqual (typeof (int[]), minst.GetParameters ()[1].ParameterType, "#6");
		}

		[Test]
		public void PropertiesOfANonGenericMethodOnGenericType ()
		{
			Type t = typeof (List<>);
			Type a = t.GetGenericArguments () [0];
			MethodInfo m = t.GetMethod ("IndexOf", new Type [] { a });
	
			var tb = module.DefineType ("foo.type");
			Type ttt = t.MakeGenericType (tb);
			MethodInfo mm = TypeBuilder.GetMethod (ttt, m);
			Assert.IsTrue (mm.GetGenericMethodDefinition ().ContainsGenericParameters, "#1");
			Assert.IsTrue (mm.ContainsGenericParameters, "#2");
		}

	}
}

#endif
