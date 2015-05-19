using System;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Reflection;

class CorrectEncodingOfNestedTypes
{
	static async Task<T> GetAsync<T>(string s) where T : new()
	{
		return await Task.Factory.StartNew(async () => {
				var response = await Task.FromResult (s);
				return (T)new object();
			}).Unwrap();
	}

	public static int Main ()
	{
		var t = typeof (CorrectEncodingOfNestedTypes).GetNestedTypes (BindingFlags.NonPublic) [0].GetNestedTypes (BindingFlags.NonPublic) [0];
		var m = t.GetMethods (BindingFlags.NonPublic | BindingFlags.Instance) [0];
		var ca = (AsyncStateMachineAttribute) m.GetCustomAttributes (true) [0];
		if (ca.StateMachineType.GetGenericArguments ().Length != 1)
			return 1;

		return 0;
	}
}