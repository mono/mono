using System;
using System.Reflection.Emit;
using System.Reflection;

class MainClass
{
	public static int Main ()
	{
		var dynMethod = new DynamicMethod ("Metoda", MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard,
			                null, Type.EmptyTypes, typeof (MainClass), true);
		var generator = dynMethod.GetILGenerator ();

		generator.Emit (OpCodes.Ldc_I4_7);
		GenerateCodeCall (generator, (int a) => {
			Console.WriteLine (a);
		});

		generator.Emit (OpCodes.Ret);

		var deleg = (Action)dynMethod.CreateDelegate (typeof (Action));
		deleg ();
		return 0;
	}

	static void GenerateCodeCall<T1> (ILGenerator generator, Action<T1> a)
	{
		generator.Emit (OpCodes.Call, a.Method);
	}
}

