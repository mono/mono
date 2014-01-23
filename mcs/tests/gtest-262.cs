using System;
using System.Reflection;
using System.Runtime.InteropServices;

public class Test {
	public enum ParamEnum {
		None = 0,
		Foo = 1,
		Bar = 2
	};
	
	public void f1 ([System.Runtime.InteropServices.DefaultParameterValue (null)] object x) {}
	public void f2 ([System.Runtime.InteropServices.DefaultParameterValue (null)] string x) {}
	public void f3 ([System.Runtime.InteropServices.DefaultParameterValue (null)] Test x) {}
	public void f4 ([System.Runtime.InteropServices.DefaultParameterValue (1)] int x) {}
	public void f5 ([System.Runtime.InteropServices.DefaultParameterValue ((short) 1)] short x) {}
	public void f6 ([DefaultParameterValue (ParamEnum.Foo)] ParamEnum n) {}

	public static void Main ()
	{
		string problems = "";
		Type t = typeof (Test);
		foreach (MethodInfo m in t.GetMethods (BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)) {
			ParameterInfo p = m.GetParameters () [0];
			Console.WriteLine (m.Name + " parameter attributes: " + p.Attributes);
			if ((p.Attributes & ParameterAttributes.HasDefault) == 0)
				problems = problems + " " + m.Name;
			if (problems.Length != 0)
				throw new Exception ("these functions don't have default values: " + problems);
		}
	}
}
