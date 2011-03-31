// Compiler options: -warnaserror

namespace System
{
	class Int32
	{
	}

	class Program
	{
		static int Main ()
		{
			System.AppDomain.CurrentDomain.TypeResolve += new ResolveEventHandler (CurrentDomain_TypeResolve);
			System.Type intType = System.Type.GetType ("System.Int32");
			if (intType.AssemblyQualifiedName != "System.Int32, test-816, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null")
				return 1;

			return 0;
		}

		static System.Reflection.Assembly CurrentDomain_TypeResolve (object sender, ResolveEventArgs args)
		{
			throw new Exception ("Resolving " + args.Name);
		}
	}
}
