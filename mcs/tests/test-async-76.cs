// Compiler options: -r:$REF_DIR/Mono.Cecil.dll

using System;
using System.Threading.Tasks;
using Mono.Cecil;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace N
{
	class C
	{
	}

	interface I<T>
	{
		void Foo (T t);
	}

	class X : I<C>
	{
		async void I<C>.Foo (C c)
		{
			await Task.Delay (1);
		}

		public static int Main ()
		{
			var m = typeof (X).GetMethod ("N.I<N.C>.Foo", BindingFlags.NonPublic | BindingFlags.Instance);
			var attr = m.GetCustomAttribute<AsyncStateMachineAttribute> ();
			if (attr == null)
				return 1;

			var assembly = AssemblyDefinition.ReadAssembly (typeof (X).Assembly.Location);
			foreach (var t in assembly.MainModule.Types) {
				PrintType (t, 0);
			}

			return 0;
		}
 
		static void PrintType (TypeDefinition td, int indent)
		{
			if (td.IsNested && !string.IsNullOrEmpty (td.Namespace))
				throw new ApplicationException ("BROKEN NESTED TYPE:");
			Console.WriteLine ("{2} Namespace: {0} Name: {1}", td.Namespace, td.Name, new string (' ', indent * 4));
			foreach (var tn in td.NestedTypes)
				PrintType (tn, indent + 1);
		}
	}
}