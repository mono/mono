using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace WebAssembly.Net.Debugging
{
	static class PuppeteerHelper
	{
		static int GetChromeRevision (string revisionString)
		{
			if (string.IsNullOrEmpty (revisionString))
				return BrowserFetcher.DefaultRevision;

			// You can find these on https://omahaproxy.appspot.com
			if (int.TryParse (revisionString, out var revision))
				return revision;

			// FIXME: is there a better option of detecting MacOS?
			var isMacOS = File.Exists ("/System/Applications/Utilities/Terminal.app");

			return revisionString switch {
				"default" => BrowserFetcher.DefaultRevision,
				"stable" => 756066,
				"beta" => 768962,
				"dev" => 781470,
				_ => throw new ArgumentException ($"Invalid chrome revision: '{revisionString}'.")
			};

			throw new ArgumentException ($"Invalid chrome revision: '{revisionString}'.");
		}

		public static async Task<string> ProvisionChrome (string chromeRevisionString)
		{
			var outputDir = Path.GetDirectoryName (Assembly.GetEntryAssembly ().Location);
			var browserDir = Path.Combine (Path.GetDirectoryName (outputDir), "local-browser");

			var chromeRevision = GetChromeRevision (chromeRevisionString);
			Console.WriteLine ($"Trying to auto-provision Chrome: {chromeRevisionString} - {chromeRevision}");

			var options = new BrowserFetcherOptions { Path = browserDir };
			var browserFetcher = Puppeteer.CreateBrowserFetcher (options);
			var revisionInfo = await browserFetcher.DownloadAsync (chromeRevision);
			Console.WriteLine ($"Auto-provisioned Chrome revision {revisionInfo.Revision} into {browserDir}.");
			Console.WriteLine ($"Executable path: {revisionInfo.ExecutablePath}");
			return revisionInfo.ExecutablePath;
		}

	}
}
