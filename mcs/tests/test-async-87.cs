using System;
using System.Threading.Tasks;

class CA
{
	public CB CB { get; set; }
	public DeviceDetails DeviceDetails { get; set; }
}

class CB
{
	public CB (string arg)
	{
	}
}

class DeviceDetails
{
	public DeviceDetails (string arg)
	{
	}
}

class BB
{
	public Task<string> GetUser()
	{        
		return Task.FromResult ("aa");
	}

	public Task<string> GetDevice()
	{
		return Task.FromResult ("bb");
	}    
}

class X
{
	BB bb = new BB ();

	public async Task<CA> GetCAAsync()
	{
		return new CA
		{
			CB = new CB(await bb.GetUser()),
			DeviceDetails = new DeviceDetails(await bb.GetDevice())
		};
	}

	static void Main ()
	{
		var x = new X ();
		x.GetCAAsync ().Wait ();
	}
}