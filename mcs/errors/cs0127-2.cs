// CS0127: `C.GetValue()': A return keyword must not be followed by any expression when method returns void
// Line: 11

using System;
using System.Threading.Tasks;

class C
{
	public async void GetValue()
	{
		return await Task.FromResult(100);
	}
}
