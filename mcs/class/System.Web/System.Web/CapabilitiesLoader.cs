// 
// System.Web.CapabilitiesLoader
//
// Loads data from browscap.ini file provided by Gary J. Keith from
// http://www.GaryKeith.com/browsers. Please don't abuse the
// site when updating browscap.ini file. Use the update-browscap.exe tool.
//
// Authors:
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

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Web.Util;

namespace System.Web
{
	sealed class BrowserData
	{
		static char [] wildchars = new char [] {'*', '?'};

		object this_lock = new object ();
		BrowserData parent;
		string text;
		string pattern;
#if TARGET_JVM
		java.util.regex.Pattern regex;
#else
		Regex regex;
#endif
		ListDictionary data;

		public BrowserData (string pattern)
		{
			int norx = pattern.IndexOfAny (wildchars);
			if (norx == -1) {
				text = pattern;
			} else {
				this.pattern = pattern.Substring (norx);
				text = pattern.Substring (0, norx);
				if (text.Length == 0)
					text = null;

				this.pattern = this.pattern.Replace (".", "\\.");
				this.pattern = this.pattern.Replace ("(", "\\(");
				this.pattern = this.pattern.Replace (")", "\\)");
				this.pattern = this.pattern.Replace ("[", "\\[");
				this.pattern = this.pattern.Replace ("]", "\\]");
				this.pattern = this.pattern.Replace ('?', '.');
				this.pattern = this.pattern.Replace ("*", ".*");
			}
		}

		public BrowserData Parent {
			get { return parent; }
			set { parent = value; }
		}

		public void Add (string key, string value)
		{
			if (data == null)
				data = new ListDictionary ();

			data.Add (key, value);
		}

		public Hashtable GetProperties (Hashtable tbl)
		{
			if (parent != null)
				parent.GetProperties (tbl);

			if (data ["browser"] != null) { // Last one (most derived) will win.
				tbl ["browser"] = data ["browser"];
			} else if (tbl ["browser"] == null) { // If none so far defined value set to *
				tbl ["browser"] = "*";
			}

			if (!tbl.ContainsKey ("browsers")) {
				tbl ["browsers"] = new ArrayList ();
			}

			((ArrayList) tbl ["browsers"]).Add (tbl["browser"]);

			foreach (string key in data.Keys)
				tbl [key.ToLower (Helpers.InvariantCulture).Trim ()] = data [key];
			
			return tbl;
		}
		
		public string GetParentName ()
		{
			return (string)(data.Contains("parent")? data ["parent"] : null);
		}
		
		public string GetAlternateBrowser ()
		{
			return (pattern == null) ? text : null;
		}

		public string GetBrowser ()
		{
			if (pattern == null)
				return text;

			return (string) data ["browser"];
		}
		
		public bool IsMatch (string expression)
		{
			if (expression == null || expression.Length == 0)
				return false;

			if (text != null) {
				if (text [0] != expression [0] ||
				    String.Compare (text, 1, expression, 1,
				    		    text.Length - 1, false,
						    Helpers.InvariantCulture) != 0) {
					return false;
				}
				expression = expression.Substring (text.Length);
			}
			
			if (pattern == null)
				return expression.Length == 0;

			lock (this_lock) {
				if (regex == null)
#if TARGET_JVM
					regex = java.util.regex.Pattern.compile (pattern);
#else
				regex = new Regex (pattern);
#endif
			}
#if TARGET_JVM
			return regex.matcher ((java.lang.CharSequence) (object) expression).matches ();
#else
			return regex.Match (expression).Success;
#endif
		}
	}
	
	sealed class CapabilitiesLoader : MarshalByRefObject
	{
		const int userAgentsCacheSize = 3000;
		static Hashtable defaultCaps;
		static readonly object lockobj = new object ();

#if TARGET_JVM
		static bool loaded {
			get {
				return alldata != null;
			}
			set {
				if (alldata == null)
					alldata = new ArrayList ();
			}
		}

 		const string alldataKey = "System.Web.CapabilitiesLoader.alldata";
		static ICollection alldata {
			get {
				return (ICollection) AppDomain.CurrentDomain.GetData (alldataKey);
			}
			set {
				AppDomain.CurrentDomain.SetData (alldataKey, value);
			}
		}

 		const string userAgentsCacheKey = "System.Web.CapabilitiesLoader.userAgentsCache";
		static Hashtable userAgentsCache {
			get {
				lock (typeof (CapabilitiesLoader)) {
					Hashtable agentsCache = (Hashtable) AppDomain.CurrentDomain.GetData (userAgentsCacheKey);
					if (agentsCache == null) {
						agentsCache = Hashtable.Synchronized (new Hashtable (userAgentsCacheSize + 10));
						AppDomain.CurrentDomain.SetData (userAgentsCacheKey, agentsCache);
					}

					return agentsCache;
				}
			}
		}
#else
		static volatile bool loaded;
		static ICollection alldata;
		static Hashtable userAgentsCache = Hashtable.Synchronized(new Hashtable(userAgentsCacheSize+10));
#endif

		CapabilitiesLoader () {}

		static CapabilitiesLoader ()
		{
			defaultCaps = new Hashtable (StringComparer.OrdinalIgnoreCase);
			defaultCaps.Add ("activexcontrols", "False");
			defaultCaps.Add ("alpha", "False");
			defaultCaps.Add ("aol", "False");
			defaultCaps.Add ("aolversion", "0");
			defaultCaps.Add ("authenticodeupdate", "");
			defaultCaps.Add ("backgroundsounds", "False");
			defaultCaps.Add ("beta", "False");
			defaultCaps.Add ("browser", "*");
			defaultCaps.Add ("browsers", new ArrayList ());
			defaultCaps.Add ("cdf", "False");
			defaultCaps.Add ("clrversion", "0");
			defaultCaps.Add ("cookies", "False");
			defaultCaps.Add ("crawler", "False");
			defaultCaps.Add ("css", "0");
			defaultCaps.Add ("cssversion", "0");
			defaultCaps.Add ("ecmascriptversion", "0.0");
			defaultCaps.Add ("frames", "False");
			defaultCaps.Add ("iframes", "False");
			defaultCaps.Add ("isbanned", "False");
			defaultCaps.Add ("ismobiledevice", "False");
			defaultCaps.Add ("issyndicationreader", "False");
			defaultCaps.Add ("javaapplets", "False");
			defaultCaps.Add ("javascript", "False");
			defaultCaps.Add ("majorver", "0");
			defaultCaps.Add ("minorver", "0");
			defaultCaps.Add ("msdomversion", "0.0");
			defaultCaps.Add ("netclr", "False");
			defaultCaps.Add ("platform", "unknown");
			defaultCaps.Add ("stripper", "False");
			defaultCaps.Add ("supportscss", "False");
			defaultCaps.Add ("tables", "False");
			defaultCaps.Add ("vbscript", "False");
			defaultCaps.Add ("version", "0");
			defaultCaps.Add ("w3cdomversion", "0.0");
			defaultCaps.Add ("wap", "False");
			defaultCaps.Add ("win16", "False");
			defaultCaps.Add ("win32", "False");
			defaultCaps.Add ("win64", "False");
			defaultCaps.Add ("adapters", new Hashtable ());
			defaultCaps.Add ("cancombineformsindeck", "False");
			defaultCaps.Add ("caninitiatevoicecall", "False");
			defaultCaps.Add ("canrenderafterinputorselectelement", "False");
			defaultCaps.Add ("canrenderemptyselects", "False");
			defaultCaps.Add ("canrenderinputandselectelementstogether", "False");
			defaultCaps.Add ("canrendermixedselects", "False");
			defaultCaps.Add ("canrenderoneventandprevelementstogether", "False");
			defaultCaps.Add ("canrenderpostbackcards", "False");
			defaultCaps.Add ("canrendersetvarzerowithmultiselectionlist", "False");
			defaultCaps.Add ("cansendmail", "False");
			defaultCaps.Add ("defaultsubmitbuttonlimit", "0");
			defaultCaps.Add ("gatewayminorversion", "0");
			defaultCaps.Add ("gatewaymajorversion", "0");
			defaultCaps.Add ("gatewayversion", "None");
			defaultCaps.Add ("hasbackbutton", "True");
			defaultCaps.Add ("hidesrightalignedmultiselectscrollbars", "False");
			defaultCaps.Add ("inputtype", "telephoneKeypad");
			defaultCaps.Add ("iscolor", "False");
			defaultCaps.Add ("jscriptversion", "0.0");
			defaultCaps.Add ("maximumhreflength", "0");
			defaultCaps.Add ("maximumrenderedpagesize", "2000");
			defaultCaps.Add ("maximumsoftkeylabellength", "5");
			defaultCaps.Add ("minorversionstring", "0.0");
			defaultCaps.Add ("mobiledevicemanufacturer", "Unknown");
			defaultCaps.Add ("mobiledevicemodel", "Unknown");
			defaultCaps.Add ("numberofsoftkeys", "0");
			defaultCaps.Add ("preferredimagemime", "image/gif");
			defaultCaps.Add ("preferredrenderingmime", "text/html");
			defaultCaps.Add ("preferredrenderingtype", "html32");
			defaultCaps.Add ("preferredrequestencoding", "");
			defaultCaps.Add ("preferredresponseencoding", "");
			defaultCaps.Add ("rendersbreakbeforewmlselectandinput", "False");
			defaultCaps.Add ("rendersbreaksafterhtmllists", "True");
			defaultCaps.Add ("rendersbreaksafterwmlanchor", "False");
			defaultCaps.Add ("rendersbreaksafterwmlinput", "False");
			defaultCaps.Add ("renderswmldoacceptsinline", "True");
			defaultCaps.Add ("renderswmlselectsasmenucards", "False");
			defaultCaps.Add ("requiredmetatagnamevalue", "");
			defaultCaps.Add ("requiresattributecolonsubstitution", "False");
			defaultCaps.Add ("requirescontenttypemetatag", "False");
			defaultCaps.Add ("requirescontrolstateinsession", "False");
			defaultCaps.Add ("requiresdbcscharacter", "False");
			defaultCaps.Add ("requireshtmladaptiveerrorreporting", "False");
			defaultCaps.Add ("requiresleadingpagebreak", "False");
			defaultCaps.Add ("requiresnobreakinformatting", "False");
			defaultCaps.Add ("requiresoutputoptimization", "False");
			defaultCaps.Add ("requiresphonenumbersasplaintext", "False");
			defaultCaps.Add ("requiresspecialviewstateencoding", "False");
			defaultCaps.Add ("requiresuniquefilepathsuffix", "False");
			defaultCaps.Add ("requiresuniquehtmlcheckboxnames", "False");
			defaultCaps.Add ("requiresuniquehtmlinputnames", "False");
			defaultCaps.Add ("requiresurlencodedpostfieldvalues", "False");
			defaultCaps.Add ("screenbitdepth", "1");
			defaultCaps.Add ("screencharactersheight", "6");
			defaultCaps.Add ("screencharacterswidth", "12");
			defaultCaps.Add ("screenpixelsheight", "72");
			defaultCaps.Add ("screenpixelswidth", "96");
			defaultCaps.Add ("supportsaccesskeyattribute", "False");
			defaultCaps.Add ("supportsbodycolor", "True");
			defaultCaps.Add ("supportsbold", "False");
			defaultCaps.Add ("supportscachecontrolmetatag", "True");
			defaultCaps.Add ("supportscallback", "False");
			defaultCaps.Add ("supportsdivalign", "True");
			defaultCaps.Add ("supportsdivnowrap", "False");
			defaultCaps.Add ("supportsemptystringincookievalue", "False");
			defaultCaps.Add ("supportsfontcolor", "True");
			defaultCaps.Add ("supportsfontname", "False");
			defaultCaps.Add ("supportsfontsize", "False");
			defaultCaps.Add ("supportsimagesubmit", "False");
			defaultCaps.Add ("supportsimodesymbols", "False");
			defaultCaps.Add ("supportsinputistyle", "False");
			defaultCaps.Add ("supportsinputmode", "False");
			defaultCaps.Add ("supportsitalic", "False");
			defaultCaps.Add ("supportsjphonemultimediaattributes", "False");
			defaultCaps.Add ("supportsjphonesymbols", "False");
			defaultCaps.Add ("supportsquerystringinformaction", "True");
			defaultCaps.Add ("supportsredirectwithcookie", "True");
			defaultCaps.Add ("supportsselectmultiple", "True");
			defaultCaps.Add ("supportsuncheck", "True");
			defaultCaps.Add ("supportsxmlhttp", "False");
			defaultCaps.Add ("type", "Unknown");
		}
		
		public static Hashtable GetCapabilities (string userAgent)
		{
			Init ();
			if (userAgent != null)
				userAgent = userAgent.Trim ();

			if (alldata == null || userAgent == null || userAgent.Length == 0)
				return defaultCaps;

			Hashtable userBrowserCaps = (Hashtable) (userAgentsCache.Contains(userAgent)? userAgentsCache [userAgent] : null);
			if (userBrowserCaps == null) {
				foreach (BrowserData bd in alldata) {
					if (bd.IsMatch (userAgent)) {
						Hashtable tbl;
						tbl = new Hashtable (defaultCaps, StringComparer.OrdinalIgnoreCase);
						userBrowserCaps = bd.GetProperties (tbl);
						break;
					}
				}

				if (userBrowserCaps == null)
					userBrowserCaps = defaultCaps;

				lock (lockobj) {
					if (userAgentsCache.Count >= userAgentsCacheSize)
						userAgentsCache.Clear ();
				}
				userAgentsCache [userAgent] = userBrowserCaps;
			}
			return userBrowserCaps;
		}

		static void Init ()
		{
			if (loaded)
				return;

			lock (lockobj) {
				if (loaded)
					return;
#if TARGET_J2EE
				string filepath = "browscap.ini";
#else
				string dir = HttpRuntime.MachineConfigurationDirectory;
				string filepath = Path.Combine (dir, "browscap.ini");
				if (!File.Exists (filepath)) {
					// try removing the trailing version directory
					dir = Path.GetDirectoryName (dir);
					filepath = Path.Combine (dir, "browscap.ini");
				}
#endif
				try {
					LoadFile (filepath);
				} catch (Exception) {}

				loaded = true;
			}
		}

#if TARGET_J2EE
		static TextReader GetJavaTextReader(string filename)
		{
			try
			{
				java.lang.ClassLoader cl = (java.lang.ClassLoader)
					AppDomain.CurrentDomain.GetData("GH_ContextClassLoader");
				if (cl == null)
					return null;

				string custom = String.Concat("browscap/", filename);
				
				java.io.InputStream inputStream = cl.getResourceAsStream(custom);
				if (inputStream == null)
					inputStream = cl.getResourceAsStream(filename);

				if (inputStream == null)
					return null;

				return new StreamReader (new System.Web.J2EE.J2EEUtils.InputStreamWrapper (inputStream));
			}
			catch (Exception e)
			{
				return null;
			}
		}
#endif

		static void LoadFile (string filename)
		{
#if TARGET_J2EE
			TextReader input = GetJavaTextReader(filename);
			if(input == null)
				return;
#else
			if (!File.Exists (filename))
				return;

			TextReader input = new StreamReader (File.OpenRead (filename));
#endif
			using (input) {
			string str;
			Hashtable allhash = new Hashtable (StringComparer.OrdinalIgnoreCase);
			int aux = 0;
			ArrayList browserData = new ArrayList ();
			while ((str = input.ReadLine ()) != null) {
				if (str.Length == 0 || str [0] == ';')
					continue;

				string userAgent = str.Substring (1, str.Length - 2);
				BrowserData data = new BrowserData (userAgent);
				ReadCapabilities (input, data);

				/* Ignore default browser and file version information */
				if (userAgent == "*" || userAgent == "GJK_Browscap_Version")
					continue;

				string key = data.GetBrowser ();
				if (key == null || allhash.ContainsKey (key)) {
					allhash.Add (aux++, data);
					browserData.Add (data);
				} else {
					allhash.Add (key, data);
					browserData.Add (data);
				}
			}			

			alldata = browserData;
			foreach (BrowserData data in alldata) {
				string pname = data.GetParentName ();
				if (pname == null)
					continue;

				data.Parent = (BrowserData) allhash [pname];
			}
			}
		}

		static char [] eq = new char []{'='};
		static void ReadCapabilities (TextReader input, BrowserData data)
		{
			string str, key;
			string [] keyvalue;
			
			while ((str = input.ReadLine ()) != null && str.Length != 0) {
				keyvalue = str.Split (eq, 2);
				key = keyvalue [0].ToLower (Helpers.InvariantCulture).Trim ();
				if (key.Length == 0)
					continue;
				data.Add (key, keyvalue [1]);
			}
		}
	}
}
