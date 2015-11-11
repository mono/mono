	//
	// MethodBuilderTestIL.cs - NUnit Test Cases for MethodBuilder.CreateMethodBody and MethodBuilder.SetMethodBody
	//
	// Marcos Henrich (marcos.henrich@xamarin.com)
	//
	// (C) Xamarin, Inc.

	using System;
	using System.Linq;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.Threading;

	using NUnit.Framework;
	using System.IO;

	namespace MonoTests.System.Reflection.Emit
	{
		public abstract class MethodBuilderTestIL
		{
			protected abstract void SetIL (MethodBuilder methodBuilder, MemoryStream ilStream);

			public static ModuleBuilder CreateModuleBuilder ()
			{
				AppDomain currentDom = Thread.GetDomain ();

				AssemblyBuilder assemblyBuilder = currentDom.DefineDynamicAssembly (
					new AssemblyName ("NewDynamicAssembly"),
					AssemblyBuilderAccess.Run);

				return assemblyBuilder.DefineDynamicModule ("NewModule");
			}

			public static object Invoke (Type type, MethodBuilder methodBuilder, params object[] parameters)
			{
				var method = type.GetMethods ().First (m => {
					if (m.Name != methodBuilder.Name)
						return false;
					var params1 = m.GetParameters ();
					var params2 = methodBuilder.GetParameters ();
					if (params1.Length != params2.Length)
						return false;
					for (var i = 0; i < params1.Length; i++)
						if (params1 [i].ParameterType.FullName != params2 [i].ParameterType.FullName)
							return false;

					return true;
				});

				object inst = Activator.CreateInstance (type, new object [0]);

				return method.Invoke (inst, parameters);
			}

			[Test]
			public void CallMethodRef ()
			{
				var expected = "value";

				var moduleBuilder = CreateModuleBuilder ();
				var typeBuilder = moduleBuilder.DefineType ("NewType");

				var methodBuilder1 = typeBuilder.DefineMethod ("NewMethod1",
					MethodAttributes.Public | MethodAttributes.Static,
					typeof (string),
					Type.EmptyTypes);

				var gen1 = methodBuilder1.GetILGenerator ();
				gen1.Emit (OpCodes.Ldstr, expected);
				gen1.Emit (OpCodes.Ret);

				var methodBuilder2 = typeBuilder.DefineMethod ("NewMethod2",
				MethodAttributes.Public | MethodAttributes.Static,
					typeof (string),
					Type.EmptyTypes);

				var ilStream = new MemoryStream ();
				var ilWriter = new BinaryWriter (ilStream);
				ilWriter.Write ((byte) 0x28); // call
				ilWriter.Write ((int)  moduleBuilder.GetMethodToken (methodBuilder1).Token);
				ilWriter.Write ((byte) 0x2A); // ret

				SetIL (methodBuilder2, ilStream);

				var type = typeBuilder.CreateType ();

				Assert.AreEqual (expected, Invoke (type, methodBuilder2));
			}

			[Test]
			public void CallMethodDef ()
			{
				var expected = "value";

				var moduleBuilder1 = CreateModuleBuilder ();
				var typeBuilder1 = moduleBuilder1.DefineType ("NewType1", TypeAttributes.Public);

				var methodBuilder1 = typeBuilder1.DefineMethod ("NewMethod1",
				 MethodAttributes.Public | MethodAttributes.Static,
					typeof (string),
					Type.EmptyTypes);

				var gen1 = methodBuilder1.GetILGenerator ();
				gen1.Emit (OpCodes.Ldstr, expected);
				gen1.Emit (OpCodes.Ret);

				typeBuilder1.CreateType ();

				var moduleBuilder2 = CreateModuleBuilder ();
				var typeBuilder2 = moduleBuilder2.DefineType ("NewType2");

				var methodBuilder2 = typeBuilder2.DefineMethod ("NewMethod2",
					MethodAttributes.Public | MethodAttributes.Static,
					typeof (string),
					Type.EmptyTypes);

				var ilStream = new MemoryStream ();
				var ilWriter = new BinaryWriter (ilStream);
				ilWriter.Write ((byte) 0x28); // call
				ilWriter.Write ((int) moduleBuilder2.GetMethodToken (methodBuilder1).Token);
				ilWriter.Write ((byte) 0x2A); // ret

				SetIL (methodBuilder2, ilStream);

				var type = typeBuilder2.CreateType ();

				Assert.AreEqual (expected, Invoke (type, methodBuilder2));
			}
		}

		/*
		 * Tests MethodBuilder.CreateMethodBody
		 */
		[TestFixture]
		public class MethodBuilderTestIL_CreateMethodBody : MethodBuilderTestIL
		{
			protected override void SetIL (MethodBuilder methodBuilder, MemoryStream ilStream)
			{
				methodBuilder.CreateMethodBody (ilStream.ToArray (), (int) ilStream.Length);
			}
		}

		/*
		 * Tests MethodBuilder.SetMethodBody
		 */
		[TestFixture]
		public class MethodBuilderTestIL_SetMethodBody : MethodBuilderTestIL
		{
			protected override void SetIL (MethodBuilder methodBuilder, MemoryStream ilStream)
			{
				methodBuilder.SetMethodBody (ilStream.ToArray (), 999, null, null, null);
			}
		}
	}
