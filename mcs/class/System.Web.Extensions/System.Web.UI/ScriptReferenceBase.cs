//
// Author:
//   Marek Habersack <grendel@twistedcode.net>
//
// (C) 2009-2011 Novell, Inc.  http://novell.com/
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

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.ComponentModel;
using System.Reflection;
using System.Security.Permissions;
using System.Threading;
using System.Web.UI.WebControls;

namespace System.Web.UI
{
	[AspNetHostingPermissionAttribute(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public abstract class ScriptReferenceBase
	{
		static SplitOrderedList <ResourceCacheEntry, bool> resourceCache;
		string _path;
		
		public bool NotifyScriptLoaded {
			get; set;
		}

		public string Path {
			get { return _path != null ? _path : String.Empty; }
			set { _path = value; }
		}
		
		[TypeConverterAttribute(typeof(StringArrayConverter))]
		public string[] ResourceUICultures {
			get; set;
		}
		
		public ScriptMode ScriptMode {
			get; set;
		}

		internal static Assembly ThisAssembly {
			get; private set;
		}
		
		static ScriptReferenceBase ()
		{
			ThisAssembly = typeof (ScriptReferenceBase).Assembly;
			resourceCache = new SplitOrderedList <ResourceCacheEntry, bool> (EqualityComparer <ResourceCacheEntry>.Default);
		}
		
		protected ScriptReferenceBase ()
		{
			this.NotifyScriptLoaded = true;
			this.ScriptMode = ScriptMode.Auto;
		}
#if NET_4_0
		protected internal virtual bool IsAjaxFrameworkScript (ScriptManager scriptManager)
		{
			return false;
		}
		
		[Obsolete ("Use IsAjaxFrameworkScript(ScriptManager)")]
#endif
		protected internal abstract bool IsFromSystemWebExtensions ();
		protected internal abstract string GetUrl (ScriptManager scriptManager, bool zip);

		// This method is an example of particularily bad coding - .NET performs NO checks
		// on pathOrName!
		protected static string ReplaceExtension (string pathOrName)
		{
			// emulate .NET behavior
			if (pathOrName == null)
				throw new NullReferenceException ();
			
			// We should check the length, but since .NET doesn't do that, we won't
			// either. Ugh.
			return pathOrName.Substring (0, pathOrName.Length - 2) + "debug.js";
		}

		internal static string GetScriptName (string releaseName, bool isDebugMode, string [] supportedUICultures, Assembly assembly, out WebResourceAttribute wra)
		{
			if (assembly != null)
				VerifyAssemblyContainsResource (assembly, releaseName, out wra);
			else
				wra = null;
			
			if (!isDebugMode && (supportedUICultures == null || supportedUICultures.Length == 0))
				return releaseName;

			if (releaseName.Length < 3 || !releaseName.EndsWith (".js", StringComparison.OrdinalIgnoreCase))
				throw new InvalidOperationException (String.Format ("'{0}' is not a valid script path.  The path must end in '.js'.", releaseName));
			
			StringBuilder sb = new StringBuilder (releaseName);
			sb.Length -= 3;
			if (isDebugMode)
				sb.Append (".debug");
			string culture = Thread.CurrentThread.CurrentUICulture.Name;
			if (supportedUICultures != null && Array.IndexOf<string> (supportedUICultures, culture) >= 0)
				sb.AppendFormat (".{0}", culture);
			sb.Append (".js");

			string ret = sb.ToString ();
			WebResourceAttribute debugWra;
			if (!CheckIfAssemblyContainsResource (assembly, ret, out debugWra))
				return releaseName;
			wra = debugWra;
			
			return ret;
		}
		
		static void VerifyAssemblyContainsResource (Assembly assembly, string resourceName, out WebResourceAttribute wra)
		{
			var rce = new ResourceCacheEntry {
				Assembly = assembly,
				ResourceName = resourceName
			};

			WebResourceAttribute attr = null;
			if (!resourceCache.InsertOrGet ((uint)rce.GetHashCode (), rce, false, () => CheckIfAssemblyContainsResource (assembly, resourceName, out attr)))
				throw new InvalidOperationException (String.Format ("Assembly '{0}' does not contain a Web resource with name '{1}'.",
										    assembly.FullName, resourceName));
			wra = attr;
		}

		static bool CheckIfAssemblyContainsResource (Assembly assembly, string resourceName, out WebResourceAttribute wra)
		{
			foreach (WebResourceAttribute attr in assembly.GetCustomAttributes (typeof (WebResourceAttribute), false)) {
				if (String.Compare (resourceName, attr.WebResource, StringComparison.Ordinal) == 0) {
					using (Stream rs = assembly.GetManifestResourceStream (resourceName)) {
						if (rs == null)
							throw new InvalidOperationException (
								String.Format ("Assembly '{0}' contains a Web resource with name '{1}' but does not contain an embedded resource with name '{1}'.",
									       assembly.FullName, resourceName)
							);
					}
					wra = attr;
					return true;
				
				}
			}
			wra = null;
			return false;
		}

		sealed class ResourceCacheEntry
		{
			public Assembly Assembly;
			public string ResourceName;

			public override int GetHashCode ()
			{
				int ret = 0;
				if (Assembly != null)
					ret ^= Assembly.GetHashCode ();
				if (ResourceName != null)
					ret ^= ResourceName.GetHashCode ();
				return ret;
			}
		}
	}
}
