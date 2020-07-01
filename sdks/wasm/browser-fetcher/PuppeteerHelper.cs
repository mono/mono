using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PuppeteerSharp;

namespace WebAssembly.Net.Debugging
{
	static class PuppeteerHelper
	{
		const int MAX_REVISIONS = 50;

		internal enum Channel
		{
			Stable,
			Beta,
			Dev,
			Canary,
			Latest
		}

		static int GetChromeRevision (string revisionString)
		{
			if (string.IsNullOrEmpty (revisionString))
				return BrowserFetcher.DefaultRevision;

			// You can find these on https://omahaproxy.appspot.com
			if (int.TryParse (revisionString, out var revision))
				return revision;

			// FIXME: is there a better option of detecting MacOS?
			var isMacOS = File.Exists ("/System/Applications/Utilities/Terminal.app");

			return revisionString switch
			{
				"default" => BrowserFetcher.DefaultRevision,
				"stable" => 756066,
				"beta" => 768962,
				"dev" => 782926,
				_ => throw new ArgumentException ($"Invalid chrome revision: '{revisionString}'.")
			};

			throw new ArgumentException ($"Invalid chrome revision: '{revisionString}'.");
		}

		public static async Task<string> ProvisionChrome (string chromeRevisionString)
		{
			var outputDir = Path.GetDirectoryName (Assembly.GetEntryAssembly ().Location);
			var browserDir = Path.Combine (Path.GetDirectoryName (outputDir), "local-browser");

			var chromeRevision = await GetLatestRevision (chromeRevisionString);
			Console.WriteLine ($"Trying to auto-provision Chrome: {chromeRevisionString} - {chromeRevision}");

			var options = new BrowserFetcherOptions { Path = browserDir };
			var browserFetcher = Puppeteer.CreateBrowserFetcher (options);
			var revisionInfo = await browserFetcher.DownloadAsync (chromeRevision);
			Console.WriteLine ($"Auto-provisioned Chrome revision {revisionInfo.Revision} into {browserDir}.");
			Console.WriteLine ($"Executable path: {revisionInfo.ExecutablePath}");
			return revisionInfo.ExecutablePath;
		}

		const string LatestMacUrl = "https://storage.googleapis.com/chromium-browser-snapshots/Mac/LAST_CHANGE";
		const string LatestLinuxUrl = "https://storage.googleapis.com/chromium-browser-snapshots/Linux_x64/LAST_CHANGE";

		static string UrlForPlatform (Platform platform) => platform switch
		{
			Platform.MacOS => LatestMacUrl,
			Platform.Linux => LatestLinuxUrl,
			_ => throw new ArgumentException ($"Invalid platform: '{platform}'", nameof (platform))
		};

		static string GetMacChannelUrl (Channel channel) => channel switch
		{
			Channel.Stable => "https://omahaproxy.appspot.com/all.json?channel=stable&os=mac",
			Channel.Beta => "https://omahaproxy.appspot.com/all.json?channel=beta&os=mac",
			Channel.Dev => "https://omahaproxy.appspot.com/all.json?channel=dev&os=mac",
			Channel.Canary => "https://omahaproxy.appspot.com/all.json?channel=canary&os=mac",
			_ => throw new ArgumentException ($"Invalid channel: '{channel}'.", nameof (channel))
		};

		static string GetLinuxChannelUrl (Channel channel) => channel switch
		{
			Channel.Stable => "https://omahaproxy.appspot.com/all.json?channel=stable&os=linux",
			Channel.Beta => "https://omahaproxy.appspot.com/all.json?channel=beta&os=linux",
			Channel.Dev => "https://omahaproxy.appspot.com/all.json?channel=dev&os=linux",
			Channel.Canary => throw new NotSupportedException ("Canary is not supported on Linux."),
			_ => throw new ArgumentException ($"Invalid channel: '{channel}'.", nameof (channel))
		};

		static string GetChannelUrl (Platform platform, Channel channel) => platform switch
		{
			Platform.MacOS => GetMacChannelUrl (channel),
			Platform.Linux => GetLinuxChannelUrl (channel),
			_ => throw new ArgumentException ($"Invalid platform: '{platform}'", nameof (platform))
		};

		static string FailedMessage (Platform platform, Channel channel) => $"Failed to find a working revision for the {channel} channel on {platform}.";

		static BrowserFetcher CreateFetcher (Platform platform) => new BrowserFetcher (
			new BrowserFetcherOptions { Platform = platform });

		public static async Task<int> GetLatestRevision (string channelOrRevision)
		{
			// FIXME: is there a better option of detecting that on netcore?
			var isMacOS = File.Exists ("/usr/lib/libc.dylib");
			var platform = isMacOS ? Platform.MacOS : Platform.Linux;

			Console.WriteLine ($"Platform check: {isMacOS}");

			int? revision;
			try {
				revision = await TryGetLatestRevision (platform, channelOrRevision).ConfigureAwait (false);
			} catch (Exception ex) {
				Console.Error.WriteLine ($"Failed to get latest revision: {ex}");
				revision = null;
			}

			if (revision != null)
				return revision.Value;

			Environment.FailFast ("Failed to get latest revision.");
			return -1;
		}

		public static async Task<int?> TryGetLatestRevision (Platform platform, string channelOrRevision)
		{
			if (int.TryParse (channelOrRevision, out var revision)) {
				Console.WriteLine ($"Using explicit revision: {revision}");
				var fetcher = CreateFetcher (platform);
				return await TryProbeRevision (fetcher, revision).ConfigureAwait (false);
			}

			var channel = Enum.Parse<Channel> (channelOrRevision, true);
			return await GetLatestRevision (platform, channel).ConfigureAwait (false);
		}

		public static async Task<int?> TryGetLatestRevision (Platform platform, Channel channel)
		{
			var fetcher = CreateFetcher (platform);

			int revision;
			if (channel == Channel.Latest) {
				var url = UrlForPlatform (platform);
				Console.WriteLine ($"Trying to get latest revision from OmahaProxy: {platform} {url}");
				revision = await GetOmahaProxyVersion (url).ConfigureAwait (false);
				Console.WriteLine ($"Got revision from OmahaProxy: {revision}");
			} else {
				var url = GetChannelUrl (platform, channel);
				Console.WriteLine ($"Trying to get {channel} revision from OmahaProxy: {platform} {url}");
				revision = await GetChannelRevision (url);
				Console.WriteLine ($"Got {channel} revision from OmahaProxy: {platform} {revision}");
			}

			var probed = await TryProbeRevision (fetcher, revision).ConfigureAwait (false);
			if (probed != null) {
				Console.WriteLine ($"Got working revision: {probed.Value}");
				return probed.Value;
			}

			Console.WriteLine (FailedMessage (platform, channel));
			return null;
		}

		public static async Task<int> GetLatestRevision (Platform platform, Channel channel)
		{
			var result = await TryGetLatestRevision (platform, channel).ConfigureAwait (false);
			return result ?? throw new NotSupportedException (FailedMessage (platform, channel));
		}

		static async Task<int?> TryProbeRevision (BrowserFetcher fetcher, int revision)
		{
			for (var current = revision; current >= revision - MAX_REVISIONS; current--) {
				Debug.WriteLine ($"Probing revision: {current}");
				if (await fetcher.CanDownloadAsync (current).ConfigureAwait (false)) {
					Debug.WriteLine ($"SUCCESS");
					return current;
				}
			}

			return null;
		}

		static async Task<int> ProbeRevision (BrowserFetcher fetcher, int revision)
		{
			var probed = await TryProbeRevision (fetcher, revision).ConfigureAwait (false);
			return probed ?? throw new NotSupportedException ($"Failed to find a working revision, starting at {revision}.");
		}

		static async Task<int> GetOmahaProxyVersion (string url)
		{
			var client = new HttpClient ();
			Debug.WriteLine ($"Checking OmahaProxy: {url}");
			var result = await client.GetStringAsync (url).ConfigureAwait (false);
			Debug.WriteLine ($"Got revision from OmahaProxy: {result}");
			return int.Parse (result);
		}

		static async Task<int> GetChannelRevision (string url)
		{
			var client = new HttpClient ();
			var result = await client.GetStringAsync (url).ConfigureAwait (false);
			var obj = JArray.Parse (result)[0];
			var versions = obj["versions"][0];
			Debug.WriteLine ($"Got version object from OmahaProxy: {versions}");
			return versions["branch_base_position"].ToObject<int> ();
		}

		static readonly Channel[] MacChannels = new[] { Channel.Stable, Channel.Beta, Channel.Dev, Channel.Canary, Channel.Latest };
		static readonly Channel[] LinuxChannels = new[] { Channel.Stable, Channel.Beta, Channel.Dev, Channel.Latest };

		public static async Task CheckLatestRevisions ()
		{
			var result = new Dictionary<(Platform, Channel), int> ();

			async Task CheckRevisions (Platform platform, Channel[] channels)
			{
				foreach (var channel in channels) {
					var revision = await TryGetLatestRevision (platform, channel).ConfigureAwait (false);
					if (revision != null) {
						Console.WriteLine ($"Got {platform} {channel} revision: {revision}");
						result[(platform, channel)] = revision.Value;
					} else {
						Console.WriteLine (FailedMessage (Platform.MacOS, channel));
					}
				}
			}

			await CheckRevisions (Platform.MacOS, MacChannels).ConfigureAwait (false);
			await CheckRevisions (Platform.Linux, LinuxChannels).ConfigureAwait (false);

			Console.WriteLine ();
			Console.WriteLine ("RESULT:");

			foreach (var entry in result) {
				Console.WriteLine ($"  {entry.Key.Item1} {entry.Key.Item2}: {entry.Value}");
			}
		}
	}
}
