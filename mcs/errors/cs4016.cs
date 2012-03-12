// CS4016: `C.GetValue()': The return expression type of async method must be `int' rather than `Task<int>'
// Line: 12

using System;
using System.Threading.Tasks;

class C
{
	public async Task<int> GetValue()
	{
		await Task.FromResult (0);
		return Task.FromResult (1);
	}
}
