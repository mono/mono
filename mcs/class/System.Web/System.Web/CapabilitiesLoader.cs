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
// (c) 2003 Novell, Inc. (http://www.novell.com)
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

namespace System.Web
{
	class BrowserData
	{
		static char [] wildchars = new char [] {'*', '?'};
		BrowserData parent;
		string text;
		string pattern;
		Regex regex;
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
				this.pattern = this.pattern.Replace ("?", ".");
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

			foreach (string key in data.Keys)
				tbl [key] = data [key];

			return tbl;
		}
		
		public string GetParentName ()
		{
			return (string) data ["parent"];
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
						    CultureInfo.InvariantCulture) != 0) {
					return false;
				}
				expression = expression.Substring (text.Length);
			}
			
			if (pattern == null)
				return expression.Length == 0;

			lock (this) {
				if (regex == null)
					regex = new Regex (pattern);
			}

			return regex.Match (expression).Success;
		}
	}
	
	class CapabilitiesLoader : MarshalByRefObject
	{
		static volatile bool loaded;
		static ICollection alldata;
		static Hashtable defaultCaps;
		static readonly object lockobj = new object ();
		private CapabilitiesLoader () {}

		static CapabilitiesLoader ()
		{
			defaultCaps = new Hashtable ();
			defaultCaps.Add ("frames", "True");
			defaultCaps.Add ("tables", "True");
		}
			
		public static Hashtable GetCapabilities (string userAgent)
		{
			Init ();
			if (userAgent != null)
				userAgent = userAgent.Trim ();

			if (alldata == null || userAgent == null || userAgent == "")
				return defaultCaps;

			foreach (BrowserData bd in alldata) {
				if (bd.IsMatch (userAgent))
					return bd.GetProperties (new Hashtable ());
			}
			
			return defaultCaps;
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
				string dir = Path.GetDirectoryName (WebConfigurationSettings.MachineConfigPath);
				string filepath = Path.Combine (dir, "browscap.ini");
				if (!File.Exists (filepath)) {
					// try removing the trailing version directory
					dir = Path.GetDirectoryName (dir);
					filepath = Path.Combine (dir, "browscap.ini");
				}
#endif
				try {
					LoadFile (filepath);
				} catch (Exception) { }

				loaded = true;
			}
		}

#if TARGET_J2EE
		private static TextReader GetJavaTextReader(string filename)
		{
			Stream s;
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

				s = (Stream)vmw.common.IOUtils.getStream(inputStream);
			}
			catch (Exception e)
			{
				return null;
			}
			return new StreamReader (s);
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
			string str;
			Hashtable allhash = new Hashtable ();
			int aux = 0;
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
				} else {
					allhash.Add (key, data);
				}
			}

			alldata = allhash.Values;
			foreach (BrowserData data in alldata) {
				if (data.Parent != null)
					continue;

				string pname = data.GetParentName ();
				if (pname != null)
					data.Parent = (BrowserData) allhash [pname];
			}
		}

		static char [] eq = new char []{'='};
		static void ReadCapabilities (TextReader input, BrowserData data)
		{
			string str;
			while ((str = input.ReadLine ()) != null && str.Length != 0) {
				string [] keyvalue = str.Split (eq, 2);
				data.Add (keyvalue [0], keyvalue [1]);
			}
		}
	}
}

