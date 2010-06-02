// 
// System.Web.HttpBrowserCapabilities
//
// Authors:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2003-2009 Novell, Inc. (http://www.novell.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Collections;
using System.Globalization;
using System.Web.Configuration;
using System.Web.UI;
using System.Security.Permissions;
using System.Web.Util;

namespace System.Web.Configuration
{
	public partial class HttpCapabilitiesBase
	{
		const int HaveActiveXControls = 1; // 1;
		const int HaveAdapters = 2;
		const int HaveAOL = 3; // 2;
		const int HaveBackGroundSounds = 4; // 3;
		const int HaveBeta = 5; // 4;
		const int HaveBrowser = 6; // 5;
		const int HaveBrowsers = 7;
		const int HaveCanCombineFormsInDeck = 8;
		const int HaveCanInitiateVoiceCall = 9;
		const int HaveCanRenderAfterInputOrSelectElement = 10;
		const int HaveCanRenderEmptySelects = 11;
		const int HaveCanRenderInputAndSelectElementsTogether = 12;
		const int HaveCanRenderMixedSelects = 13;
		const int HaveCanRenderOneventAndPrevElementsTogether = 14;
		const int HaveCanRenderPostBackCards = 15;
		const int HaveCanRenderSetvarZeroWithMultiSelectionList = 16;
		const int HaveCanSendMail = 17;
		const int HaveCDF = 18; // 6;
		//const int HaveClrVersion = 19; // 7;
		const int HaveCookies = 20; // 8;
		const int HaveCrawler = 21; // 9;
		const int HaveDefaultSubmitButtonLimit = 22;
		const int HaveEcmaScriptVersion = 23;
		const int HaveFrames = 24; // 11;
		const int HaveGatewayMajorVersion = 25;
		const int HaveGatewayMinorVersion = 26;
		const int HaveGatewayVersion = 27;
		const int HaveHasBackButton = 28;
		const int HaveHidesRightAlignedMultiselectScrollbars = 29;
		const int HaveHtmlTextWriter = 30;
		const int HaveId = 31;
		const int HaveInputType = 32;
		const int HaveIsColor = 33;
		const int HaveIsMobileDevice = 34;
		const int HaveJavaApplets = 35; // 12;
		const int HaveJavaScript = 36; // 13;
		const int HaveJScriptVersion = 37;
		const int HaveMajorVersion = 38; // 14;
		const int HaveMaximumHrefLength = 39;
		const int HaveMaximumRenderedPageSize = 40;
		const int HaveMaximumSoftkeyLabelLength = 41;
		const int HaveMinorVersion = 42; // 15;
		const int HaveMinorVersionString = 43;
		const int HaveMobileDeviceManufacturer = 44;
		const int HaveMobileDeviceModel = 45;
		const int HaveMSDomVersion = 46; // 16;
		const int HaveNumberOfSoftkeys = 47;
		const int HavePlatform = 48; // 17;
		const int HavePreferredImageMime = 49;
		const int HavePreferredRenderingMime = 50;
		const int HavePreferredRenderingType = 51;
		const int HavePreferredRequestEncoding = 52;
		const int HavePreferredResponseEncoding = 53;
		const int HaveRendersBreakBeforeWmlSelectAndInput = 54;
		const int HaveRendersBreaksAfterHtmlLists = 55;
		const int HaveRendersBreaksAfterWmlAnchor = 56;
		const int HaveRendersBreaksAfterWmlInput = 57;
		const int HaveRendersWmlDoAcceptsInline = 58;
		const int HaveRendersWmlSelectsAsMenuCards = 59;
		const int HaveRequiredMetaTagNameValue = 60;
		const int HaveRequiresAttributeColonSubstitution = 61;
		const int HaveRequiresContentTypeMetaTag = 62;
		const int HaveRequiresControlStateInSession = 63;
		const int HaveRequiresDBCSCharacter = 64;
		const int HaveRequiresHtmlAdaptiveErrorReporting = 65;
		const int HaveRequiresLeadingPageBreak = 66;
		const int HaveRequiresNoBreakInFormatting = 67;
		const int HaveRequiresOutputOptimization = 68;
		const int HaveRequiresPhoneNumbersAsPlainText = 69;
		const int HaveRequiresSpecialViewStateEncoding = 70;
		const int HaveRequiresUniqueFilePathSuffix = 71;
		const int HaveRequiresUniqueHtmlCheckboxNames = 72;
		const int HaveRequiresUniqueHtmlInputNames = 73;
		const int HaveRequiresUrlEncodedPostfieldValues = 74;
		const int HaveScreenBitDepth = 75;
		const int HaveScreenCharactersHeight = 76;
		const int HaveScreenCharactersWidth = 77;
		const int HaveScreenPixelsHeight = 78;
		const int HaveScreenPixelsWidth = 79;
		const int HaveSupportsAccesskeyAttribute = 80;
		const int HaveSupportsBodyColor = 81;
		const int HaveSupportsBold = 82;
		const int HaveSupportsCacheControlMetaTag = 83;
		const int HaveSupportsCallback = 84;
		const int HaveSupportsCss = 85;
		const int HaveSupportsDivAlign = 86;
		const int HaveSupportsDivNoWrap = 87;
		const int HaveSupportsEmptyStringInCookieValue = 88;
		const int HaveSupportsFontColor = 89;
		const int HaveSupportsFontName = 90;
		const int HaveSupportsFontSize = 91;
		const int HaveSupportsImageSubmit = 92;
		const int HaveSupportsIModeSymbols = 93;
		const int HaveSupportsInputIStyle = 94;
		const int HaveSupportsInputMode = 95;
		const int HaveSupportsItalic = 96;
		const int HaveSupportsJPhoneMultiMediaAttributes = 97;
		const int HaveSupportsJPhoneSymbols = 98;
		const int HaveSupportsQueryStringInFormAction = 99;
		const int HaveSupportsRedirectWithCookie = 100;
		const int HaveSupportsSelectMultiple = 101;
		const int HaveSupportsUncheck = 102;
		const int HaveSupportsXmlHttp = 103;
		const int HaveTables = 104; // 18;
		const int HaveTagWriter = 105; // 19;
		const int HaveType = 106;
		const int HaveUseOptimizedCacheKey = 107;
		const int HaveVBScript = 108; // 20;
		const int HaveVersion = 109; // 21;
		const int HaveW3CDomVersion = 110; // 22;
		const int HaveWin16 = 111; // 23;
		const int HaveWin32 = 112; // 24;
		const int LastHaveFlag = 113;

		BitArray flags = new BitArray (LastHaveFlag);
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
		bool win32;
		Version [] clrVersions;
		internal string useragent;

		public bool ActiveXControls {
			get {
				if (!Get (HaveActiveXControls)) {
					activeXControls = ReadBoolean ("activexcontrols");
					Set (HaveActiveXControls);
				}

				return activeXControls;
			}
		}

		public bool AOL {
			get {
				if (!Get (HaveAOL)) {
					aol = ReadBoolean ("aol");
					Set (HaveAOL);
				}

				return aol;
			}
		}

		public bool BackgroundSounds {
			get {
				if (!Get (HaveBackGroundSounds)) {
					backgroundSounds = ReadBoolean ("backgroundsounds");
					Set (HaveBackGroundSounds);
				}

				return backgroundSounds;
			}
		}

		public bool Beta {
			get {
				if (!Get (HaveBeta)) {
					beta = ReadBoolean ("beta");
					Set (HaveBeta);
				}

				return beta;
			}
		}

		public string Browser {
			get {
				if (!Get (HaveBrowser)) {
					browser = this ["browser"];
					if (browser == null || browser.Length == 0)
						browser = "Unknown";
					Set (HaveBrowser);
				}

				return browser;
			}
		}

		ArrayList browsers = null;
		public ArrayList Browsers {
			get {
				if (!Get (HaveBrowsers)) {
					browsers = ReadArrayList ("browsers");
					Set (HaveBrowsers);
				}

				return browsers;
			}
		}

		public bool IsBrowser (string browserName) 
		{
			foreach (string browser in Browsers) {
				if (0 == String.Compare (browser, "Unknown", StringComparison.OrdinalIgnoreCase)) {
					continue;
				}
				if (0 == String.Compare (browserName, browser, StringComparison.OrdinalIgnoreCase)) {
					return true;
				}
			}
			return false;
		}

		public bool CDF {
			get {
				if (!Get (HaveCDF)) {
					cdf = ReadBoolean ("cdf");
					Set (HaveCDF);
				}

				return cdf;
			}
		}

		public Version ClrVersion {
			get {
				if (clrVersion == null)
					InternalGetClrVersions ();

				return clrVersion;
			}
		}

		public bool Cookies {
			get {
				if (!Get (HaveCookies)) {
					cookies = ReadBoolean ("cookies");
					Set (HaveCookies);
				}

				return cookies;
			}
		}

		public bool Crawler {
			get {
				if (!Get (HaveCrawler)) {
					crawler = ReadBoolean ("crawler");
					Set (HaveCrawler);
				}

				return crawler;
			}
		}

		public Version EcmaScriptVersion {
			get {
				if (!Get (HaveEcmaScriptVersion)) {
					ecmaScriptVersion = ReadVersion ("ecmascriptversion");
					Set (HaveEcmaScriptVersion);
				}

				return ecmaScriptVersion;
			}
		}

		public bool Frames {
			get {
				if (!Get (HaveFrames)) {
					frames = ReadBoolean ("frames");
					Set (HaveFrames);
				}

				return frames;
			}
		}

		public bool JavaApplets {
			get {
				if (!Get (HaveJavaApplets)) {
					javaApplets = ReadBoolean ("javaapplets");
					Set (HaveJavaApplets);
				}

				return javaApplets;
			}
		}


		[Obsolete ("The recommended alternative is the EcmaScriptVersion property. A Major version value greater than or equal to 1 implies JavaScript support. http://go.microsoft.com/fwlink/?linkid=14202")]
		public bool JavaScript {
			get {
				if (!Get (HaveJavaScript)) {
					javaScript = ReadBoolean ("javascript");
					Set (HaveJavaScript);
				}

				return javaScript;
			}
		}

		public int MajorVersion {
			get {
				if (!Get (HaveMajorVersion)) {
					majorVersion = ReadInt32 ("majorver");
					Set (HaveMajorVersion);
				}

				return majorVersion;
			}
		}

		public double MinorVersion {
			get {
				if (!Get (HaveMinorVersion)) {
					minorVersion = ReadDouble ("minorver");
					Set (HaveMinorVersion);
				}

				return minorVersion;
			}
		}

		public Version MSDomVersion {
			get {
				if (!Get (HaveMSDomVersion)) {
					msDomVersion = ReadVersion ("msdomversion");
					Set (HaveMSDomVersion);
				}

				return msDomVersion;
			}
		}

		public string Platform {
			get {
				if (!Get (HavePlatform)) {
					platform = ReadString ("platform");
					Set (HavePlatform);
				}

				return platform;
			}
		}

		public bool Tables {
			get {
				if (!Get (HaveTables)) {
					tables = ReadBoolean ("tables");
					Set (HaveTables);
				}

				return tables;
			}
		}

		public Type TagWriter {
			get {
				if (!Get (HaveTagWriter)) {
					tagWriter = GetTagWriter ();
					Set (HaveTagWriter);
				}
				return tagWriter;
			}
		}
		
		internal virtual Type GetTagWriter ()
		{
				return typeof (HtmlTextWriter);			
		}

		public string Type {
			get {
				return Browser + MajorVersion;
			}
		}

		public bool VBScript {
			get {
				if (!Get (HaveVBScript)) {
					vbscript = ReadBoolean ("vbscript");
					Set (HaveVBScript);
				}

				return vbscript;
			}
		}

		public string Version {
			get {
				if (!Get (HaveVersion)) {
					version = ReadString ("version");
					Set (HaveVersion);
				}

				return version;
			}
		}

		public Version W3CDomVersion {
			get {
				if (!Get (HaveW3CDomVersion)) {
					w3CDomVersion = ReadVersion ("w3cdomversion");
					Set (HaveW3CDomVersion);
				}

				return w3CDomVersion;
			}
		}

		public bool Win16 {
			get {
				if (!Get (HaveWin16)) {
					win16 = ReadBoolean ("win16");
					Set (HaveWin16);
				}

				return win16;
			}
		}

		public bool Win32 {
			get {
				// This is the list of different windows platforms that browscap.ini has.
				// Win16 Win2000 Win2003 Win32 Win95 Win98 WinME WinNT WinVI WinXP
				if (!Get (HaveWin32)) {
					string platform = Platform;
					win32 = (platform != "Win16" && platform.StartsWith ("Win"));
					Set (HaveWin32);
				}
				return win32;
			}
		}

		public Version [] GetClrVersions ()
		{
			if (clrVersions == null)
				InternalGetClrVersions ();

			return clrVersions;
		}

		void InternalGetClrVersions ()
		{
			char [] anychars = new char [] { ';', ')' };
			string s = useragent;
			ArrayList list = null;
			int idx;
			while ((s != null) && (idx = s.IndexOf (".NET CLR ")) != -1) {
				int end = s.IndexOfAny (anychars, idx + 9);
				if (end == -1)
					break;

				string ver = s.Substring (idx + 9, end - idx - 9);
				Version v = null;
				try {
					v = new Version (ver);
					if (clrVersion == null || v > clrVersion)
						clrVersion = v;

					if (list == null)
						list = new ArrayList (4);

					list.Add (v);
				} catch { }
				s = s.Substring (idx + 9);
			}
			
			if (list == null || list.Count == 0) {
				clrVersion = new Version ();
				clrVersions = null;
			} else {
				list.Sort ();
				clrVersions = (Version []) list.ToArray (typeof (Version));
			}
		}

		bool ReadBoolean (string key)
		{
			string v = this [key];
			if (v == null) {
				throw CreateCapabilityNotFoundException (key);
			}

			return (String.Compare (v, "True", true, Helpers.InvariantCulture) == 0);
		}

		int ReadInt32 (string key)
		{
			string v = this [key];
			if (v == null) {
				throw CreateCapabilityNotFoundException (key);
			}

			try {
				return Int32.Parse (v);
			} catch {
				throw CreateCapabilityNotFoundException (key);
			}
		}

		double ReadDouble (string key)
		{
			string v = this [key];
			if (v == null) {
				throw CreateCapabilityNotFoundException (key);
			}

			try {
				return Double.Parse (v);
			} catch {
				throw CreateCapabilityNotFoundException (key);
			}
		}

		string ReadString (string key) 
		{
			string v = this [key];
			if (v == null) {
				throw CreateCapabilityNotFoundException (key);
			}

			return v;
		}

		Version ReadVersion (string key) 
		{
			string v = this [key];
			if (v == null) {
				throw CreateCapabilityNotFoundException (key);
			}

			try {
				return new Version (v);
			}
			catch {
				throw CreateCapabilityNotFoundException (key);
			}
		}

		ArrayList ReadArrayList (string key) 
		{
			ArrayList v = (ArrayList)this.capabilities [key];
			if (v == null) {
				throw CreateCapabilityNotFoundException (key);
			}

			return v;
		}

		Exception CreateCapabilityNotFoundException (string key) {
			return new ArgumentNullException (String.Format ("browscaps.ini does not contain a definition for capability {0} for userAgent {1}", key, Browser));
		}

		bool Get (int idx)
		{
			return flags.Get (idx);
		}

		void Set (int idx)
		{
			flags.Set (idx, true);
		}
	}
}
