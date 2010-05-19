//
// System.Web.UI.BaseParser.cs
//
// Authors:
// 	Duncan Mak  (duncan@ximian.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc. (http://www.ximian.com)
// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
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
using System.IO;
using System.Security.Permissions;
using System.Web.Compilation;
using System.Web.Configuration;
using System.Globalization;
using System.Web.Util;

namespace System.Web.UI
{
	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class BaseParser
	{
		HttpContext context;
		string baseDir;
		string baseVDir;
		ILocation location;

		internal string MapPath (string path)
		{
			return MapPath (path, true);
		}

		internal string MapPath (string path, bool allowCrossAppMapping)
		{
			if (context == null)
				throw new HttpException ("context is null!!");

			return context.Request.MapPath (path, BaseVirtualDir, allowCrossAppMapping);
		}

		internal string PhysicalPath (string path)
		{
			if (Path.DirectorySeparatorChar != '/')
				path = path.Replace ('/', '\\');
				
			return Path.Combine (BaseDir, path);
		}

		internal bool GetBool (IDictionary hash, string key, bool deflt)
		{
			string val = hash [key] as string;
			if (val == null)
				return deflt;

			hash.Remove (key);

			bool result = false;
			if (String.Compare (val, "true", true, Helpers.InvariantCulture) == 0)
				result = true;
			else if (String.Compare (val, "false", true, Helpers.InvariantCulture) != 0)
				ThrowParseException ("Invalid value for " + key);

			return result;
		}

		internal static string GetString (IDictionary hash, string key, string deflt)
		{
			string val = hash [key] as string;
			if (val == null)
				return deflt;

			hash.Remove (key);
			return val;
		}

		internal static bool IsDirective (string value, char directiveChar)
		{
			if (value == null || value == String.Empty)
				return false;
			
			value = value.Trim ();
			if (!StrUtils.StartsWith (value, "<%") || !StrUtils.EndsWith (value, "%>"))
				return false;

			int dcIndex = value.IndexOf (directiveChar, 2);
			if (dcIndex == -1)
				return false;

			if (dcIndex == 2)
				return true;
			dcIndex--;
			
			while (dcIndex >= 2) {
				if (!Char.IsWhiteSpace (value [dcIndex]))
					return false;
				dcIndex--;
			}

			return true;
		}
		
		internal static bool IsDataBound (string value)
		{
			return IsDirective (value, '#');
		}

		internal static bool IsExpression (string value)
		{
			return IsDirective (value, '$');
		}
		
		internal void ThrowParseException (string message, params object[] parms)
		{
			if (parms == null)
				throw new ParseException (location, message);
			throw new ParseException (location, String.Format (message, parms));
		}
		
		internal void ThrowParseException (string message, Exception inner, params object[] parms)
		{
			if (parms == null || parms.Length == 0)
				throw new ParseException (location, message, inner);
			else
				throw new ParseException (location, String.Format (message, parms), inner);
		}

		internal void ThrowParseFileNotFound (string path, params object[] parms)
		{
			ThrowParseException ("The file '" + path + "' does not exist", parms);
		}
		
		internal ILocation Location {
			get { return location; }
			set { location = value; }
		}

		internal HttpContext Context {
			get { return context; }
			set { context = value; }
		}

		internal string BaseDir {
			get {
				if (baseDir == null)
					baseDir = MapPath (BaseVirtualDir, false);

				return baseDir;
			}
		}

		internal virtual string BaseVirtualDir {
			get {
				if (baseVDir == null)
					baseVDir = VirtualPathUtility.GetDirectory (context.Request.FilePath);
				
				return baseVDir;
			}

			set {
				if (VirtualPathUtility.IsRooted (value))
					baseVDir = VirtualPathUtility.ToAbsolute (value);
				else
					baseVDir = value; 
			}
		}

		internal TSection GetConfigSection <TSection> (string section) where TSection: global::System.Configuration.ConfigurationSection
		{
			VirtualPath vpath = VirtualPath;
			string vp = vpath != null ? vpath.Absolute : null;
			if (vp == null)
				return WebConfigurationManager.GetSection (section) as TSection;
			else
				return WebConfigurationManager.GetSection (section, vp) as TSection;
		}
		
		internal VirtualPath VirtualPath {
			get;
			set;
		}

		internal CompilationSection CompilationConfig {
			get { return GetConfigSection <CompilationSection> ("system.web/compilation"); }
		}
	}
}

