// 
// System.Web.HttpBrowserCapabilities
//
// Authors:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2003 Novell, Inc. (http://www.novell.com)
//
using System;
using System.Web.Configuration;
using System.Web.UI;

namespace System.Web
{
	public class HttpBrowserCapabilities : HttpCapabilitiesBase
	{
		const int HaveActiveXControls = 1;
		const int HaveAOL = 2;
		const int HaveBackGroundSounds = 3;
		const int HaveBeta = 4;
		const int HaveBrowser = 5;
		const int HaveCDF = 6;
		const int HaveClrVersion = 7;
		const int HaveCookies = 8;
		const int HaveCrawler = 9;
		const int HaveEcmaScriptVersion = 10;
		const int HaveFrames = 11;
		const int HaveJavaApplets = 12;
		const int HaveJavaScript = 13;
		const int HaveMajorVersion = 14;
		const int HaveMinorVersion = 15;
		const int HaveMSDomVersion = 16;
		const int HavePlatform = 17;
		const int HaveTables = 18;
		const int HaveTagWriter = 19;
		const int HaveVBScript = 20;
		const int HaveVersion = 21;
		const int HaveW3CDomVersion = 22;
		const int HaveWin16 = 23;
		const int HaveWin32 = 24;

		int flags;
		bool activeXControls;
		bool aol;
		bool backgroundSounds;
		bool beta;
		string browser;
		bool cdf;
		Version clrVersion;
		bool cookies;
		bool crawler;
		Version ecmaScriptVersion;
		bool frames;
		bool javaApplets;
		bool javaScript;
		int majorVersion;
		double minorVersion;
		Version msDomVersion;
		string platform;
		bool tables;
		Type tagWriter;
		bool vbscript;
		string version;
		Version w3CDomVersion;
		bool win16;

		public HttpBrowserCapabilities () { }

		public bool ActiveXControls {
			get {
				if (!Get (HaveActiveXControls)) {
					Set (HaveActiveXControls);
					activeXControls = ReadBoolean ("activexcontrols", false);
				}

				return activeXControls;
			}
		}

		public bool AOL {
			get {
				if (!Get (HaveAOL)) {
					Set (HaveAOL);
					aol = ReadBoolean ("aol", false);
				}

				return aol;
			}
		}

		public bool BackgroundSounds {
			get {
				if (!Get (HaveBackGroundSounds)) {
					Set (HaveBackGroundSounds);
					backgroundSounds = ReadBoolean ("backgroundsounds", false);
				}

				return backgroundSounds;
			}
		}

		public bool Beta {
			get {
				if (!Get (HaveBeta)) {
					Set (HaveBeta);
					beta = ReadBoolean ("beta", false);
				}

				return beta;
			}
		}

		public string Browser {
			get {
				if (!Get (HaveBrowser)) {
					Set (HaveBrowser);
					browser = this ["browser"];
					if (browser == null)
						browser = "Unknown";
				}

				return browser;
			}
		}

		public bool CDF {
			get {
				if (!Get (HaveCDF)) {
					Set (HaveCDF);
					cdf = ReadBoolean ("cdf", false);
				}

				return cdf;
			}
		}

		public Version ClrVersion {
			get {
				if (!Get (HaveClrVersion)) {
					Set (HaveClrVersion);
					if (ReadBoolean ("netclr", false)) {
						clrVersion = new Version (1, 0);
					} else {
						clrVersion = new Version (0, 0);
					}
				}

				return clrVersion;
			}
		}

		public bool Cookies {
			get {
				if (!Get (HaveCookies)) {
					Set (HaveCookies);
					cookies = ReadBoolean ("cookies", false);
				}

				return cookies;
			}
		}

		public bool Crawler {
			get {
				if (!Get (HaveCrawler)) {
					Set (HaveCrawler);
					crawler = ReadBoolean ("crawler", false);
				}

				return crawler;
			}
		}

		[MonoTODO]
		public Version EcmaScriptVersion {
			get {
				return new Version (0, 0);
			}
		}

		public bool Frames {
			get {
				if (!Get (HaveFrames)) {
					Set (HaveFrames);
					frames = ReadBoolean ("frames", false);
				}

				return frames;
			}
		}

		public bool JavaApplets {
			get {
				if (!Get (HaveJavaApplets)) {
					Set (HaveJavaApplets);
					javaApplets = ReadBoolean ("javaapplets", false);
				}

				return javaApplets;
			}
		}

		public bool JavaScript {
			get {
				if (!Get (HaveJavaScript)) {
					Set (HaveJavaScript);
					javaScript = ReadBoolean ("javascript", false);
				}

				return javaScript;
			}
		}

		public int MajorVersion {
			get {
				if (!Get (HaveMajorVersion)) {
					Set (HaveMajorVersion);
					majorVersion = ReadInt32 ("majorver", 0);
				}

				return majorVersion;
			}
		}

		public double MinorVersion {
			get {
				if (!Get (HaveMinorVersion)) {
					Set (HaveMinorVersion);
					minorVersion = ReadDouble ("minorver", 0);
				}

				return minorVersion;
			}
		}

		[MonoTODO]
		public Version MSDomVersion {
			get {
				return new Version (0, 0);
			}
		}

		public string Platform {
			get {
				if (!Get (HavePlatform)) {
					Set (HavePlatform);
					platform = this ["platform"];
					if (platform == null)
						platform = "";
				}

				return platform;
			}
		}

		public bool Tables {
			get {
				if (!Get (HaveTables)) {
					Set (HaveTables);
					tables = ReadBoolean ("tables", false);
				}

				return tables;
			}
		}

		[MonoTODO]
		public Type TagWriter {
			get {
				return typeof (HtmlTextWriter);
			}
		}

		public string Type {
			get {
				return Browser + MajorVersion;
			}
		}

		public bool VBScript {
			get {
				if (!Get (HaveVBScript)) {
					Set (HaveVBScript);
					vbscript = ReadBoolean ("vbscript", false);
				}

				return vbscript;
			}
		}

		public string Version {
			get {
				if (!Get (HaveVersion)) {
					Set (HaveVersion);
					version = this ["version"];
					if (version == null)
						version = "";
				}

				return version;
			}
		}

		[MonoTODO]
		public Version W3CDomVersion {
			get {
				return new Version (0, 0);
			}
		}

		public bool Win16 {
			get {
				if (!Get (HaveWin16)) {
					Set (HaveWin16);
					win16 = ReadBoolean ("win16", false);
				}

				return win16;
			}
		}

		public bool Win32 {
			get {
				return !Win16;
			}
		}

#if NET_1_1
		[MonoTODO]
		public Version [] GetClrVersions ()
		{
			throw new NotImplementedException ();
		}
#endif

		bool ReadBoolean (string key, bool dflt)
		{
			string v = this [key];
			if (v == null)
				return dflt;

			return (v == "True");
		}

		int ReadInt32 (string key, int dflt)
		{
			string v = this [key];
			if (v == null)
				return dflt;

			try {
				return Int32.Parse (v);
			} catch {
				return dflt;
			}
		}

		double ReadDouble (string key, double dflt)
		{
			string v = this [key];
			if (v == null)
				return dflt;

			try {
				return Double.Parse (v);
			} catch {
				return dflt;
			}
		}

		bool Get (int idx)
		{
			return (flags & (1 << idx)) != 0;
		}

		void Set (int idx)
		{
			flags |= (1 << idx);
		}
	}
}

