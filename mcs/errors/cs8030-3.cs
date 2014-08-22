// CS8030: Anonymous function or lambda expression converted to a void returning delegate cannot return a value
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
