using System;
using System.Threading.Tasks;

class EncodingVariableScopeInfoLiftedFieldName
{
	public static void Main ()
	{
	}

	async Task Test (int arg)
	{
		if (arg == 1) {
			{
			}
		}

		if (arg > 0)
		{
			var x = 1;
			await Task.Yield();
		} 
		else 
		{
			var x = 2;
			await Task.Yield();
		}
	}
}
