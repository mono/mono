// Compiler options: -r:../class/lib/net_4_5/Mono.Cecil.dll

using System;
using System.Threading.Tasks;
using Mono.Cecil;

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

		public static void Main ()
		{
			var assembly = AssemblyDefinition.ReadAssembly (typeof (X).Assembly.Location);
			foreach (var t in assembly.MainModule.Types) {
				PrintType (t, 0);
			}
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