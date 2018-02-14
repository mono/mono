using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

class Program 
{ 
	static int Main (string[] args)
	{
		if (args == null || args.Length < 3)
		{
			Console.WriteLine ("Invalid argumenets.\n Usage: RemoteTestExecuter.exe {assembly_name} {type_name} {method_name} {exception_file} {method_args}");
			return -1;
		}
		
		string assemblyName = args[0];
		string typeName = args[1];
		string methodName = args[2];
		
		var type = Type.GetType ($"{typeName}, {assemblyName}");
		if (type == null)
			throw new Exception ($"Type {typeName} was not found in {assemblyName}");
		
		var method = type.GetMethod (methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
		if (method == null)
			throw new Exception ($"Method {methodName} was not found in {typeName}, {assemblyName}");

		var methodArgs = args.Length == 4 ? null : args.Skip(4).Cast<object>().ToArray();
		var instance = Activator.CreateInstance (type);
		int result = 0;

		if (method.ReturnType == typeof (int))
		{
			result = (int)method.Invoke (instance, methodArgs);
		}
		else if (method.ReturnType == typeof (Task<int>))
		{
			var task = (Task<int>)method.Invoke (instance, methodArgs);
			task.Wait(); //use C# 7.1 async Main?
			result = task.Result;
		}
		else
		{
			// FIXME: we're called for compiler-generated helper methods
			var attr = method.GetCustomAttribute<CompilerGeneratedAttribute> ();
			if (attr == null)
				attr = method.DeclaringType.GetCustomAttribute<CompilerGeneratedAttribute> ();
			if (attr != null)
				return -1;
			throw new Exception ($"ReturnType of `{method}` should be int or Task<int>. But was: {method.ReturnType.Name}");
		}

		return result;
	}
}