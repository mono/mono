using System;
using System.Threading;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace WebAssembly.Net.Debugging
{
	class Program
	{
		static async Task Main (string[] args)
		{
			await PuppeteerHelper.CheckLatestRevisions ().ConfigureAwait (false);
			Console.WriteLine ();
		}
	}
}
