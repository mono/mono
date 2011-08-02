//
// System.Web.Handlers.AssemblyResourceLoader
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Marek Habersack <grendel@twistedcode.net>
//
// (C) 2003 Ben Maurer
// (C) 2010 Novell, Inc (http://novell.com/)

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

using System.Web.UI;
using System.Globalization;
using System.Reflection;
using System.IO;
using System.Resources;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Configuration;
using System.Web.Util;

namespace System.Web.Handlers
{
#if SYSTEM_WEB_EXTENSIONS
	partial class ScriptResourceHandler
	{
		const string HandlerFileName = "ScriptResource.axd";
		static Assembly currAsm = typeof (ScriptResourceHandler).Assembly;
#else
	public sealed class AssemblyResourceLoader : IHttpHandler
	{
		const string HandlerFileName = "WebResource.axd";
		static Assembly currAsm = typeof (AssemblyResourceLoader).Assembly;
#endif
		const char QueryParamSeparator = '&';

		static readonly Dictionary <string, AssemblyEmbeddedResources> _embeddedResources = new Dictionary <string, AssemblyEmbeddedResources> (StringComparer.Ordinal);
		static readonly ReaderWriterLockSlim _embeddedResourcesLock = new ReaderWriterLockSlim ();
		static readonly ReaderWriterLockSlim _stringHashCacheLock = new ReaderWriterLockSlim ();
		static readonly Dictionary <string, string> stringHashCache = new Dictionary <string, string> (StringComparer.Ordinal);

		[ThreadStatic]
		static KeyedHashAlgorithm hashAlg;
		static bool canReuseHashAlg = true;

		static KeyedHashAlgorithm ReusableHashAlgorithm {
			get {
				if (!canReuseHashAlg)
					return null;

				if (hashAlg == null) {				
					MachineKeySection mks = MachineKeySection.Config;
					hashAlg = MachineKeySectionUtils.GetValidationAlgorithm (mks);
					if (!hashAlg.CanReuseTransform) {
						canReuseHashAlg = false;
						hashAlg = null;
					}
				}

				if (hashAlg != null)
					hashAlg.Initialize ();

				return hashAlg;
			}
		}
		
		static string GetStringHash (KeyedHashAlgorithm kha, string str)
		{
			if (String.IsNullOrEmpty (str))
				return String.Empty;

			string result;
			try {
				_stringHashCacheLock.EnterUpgradeableReadLock ();
				if (stringHashCache.TryGetValue (str, out result))
					return result;

				try {
					_stringHashCacheLock.EnterWriteLock ();
					if (stringHashCache.TryGetValue (str, out result))
						return result;
					
					result = Convert.ToBase64String (kha.ComputeHash (Encoding.UTF8.GetBytes (str)));
					stringHashCache.Add (str, result);
				} finally {
					_stringHashCacheLock.ExitWriteLock ();
				}
			} finally {
				_stringHashCacheLock.ExitUpgradeableReadLock ();
			}
			
			return result;
		}
		
		static void InitEmbeddedResourcesUrls (KeyedHashAlgorithm kha, Assembly assembly, string assemblyName, string assemblyHash, AssemblyEmbeddedResources entry)
		{
			WebResourceAttribute [] attrs = (WebResourceAttribute []) assembly.GetCustomAttributes (typeof (WebResourceAttribute), false);
			WebResourceAttribute attr;
			string apath = assembly.Location;
			for (int i = 0; i < attrs.Length; i++) {
				attr = attrs [i];
				string resourceName = attr.WebResource;
				if (!String.IsNullOrEmpty (resourceName)) {
					string resourceNameHash = GetStringHash (kha, resourceName);
#if SYSTEM_WEB_EXTENSIONS
					bool debug = resourceName.EndsWith (".debug.js", StringComparison.OrdinalIgnoreCase);
					string dbgTail = debug ? "d" : String.Empty;
					string rkNoNotify = resourceNameHash + "f" + dbgTail;
					string rkNotify = resourceNameHash + "t" + dbgTail;

					if (!entry.Resources.ContainsKey (rkNoNotify)) {
						var er = new EmbeddedResource () {
							Name = resourceName,
							Attribute = attr, 
							Url = CreateResourceUrl (kha, assemblyName, assemblyHash, apath, rkNoNotify, debug, false, true)
						};
						
						entry.Resources.Add (rkNoNotify, er);
					}
					
					if (!entry.Resources.ContainsKey (rkNotify)) {
						var er = new EmbeddedResource () {
							Name = resourceName,
							Attribute = attr, 
							Url = CreateResourceUrl (kha, assemblyName, assemblyHash, apath, rkNotify, debug, true, true)
						};
						
						entry.Resources.Add (rkNotify, er);
					}
#else
					if (!entry.Resources.ContainsKey (resourceNameHash)) {
						var er = new EmbeddedResource () {
							Name = resourceName,
							Attribute = attr, 
							Url = CreateResourceUrl (kha, assemblyName, assemblyHash, apath, resourceNameHash, false, false, true)
						};
						entry.Resources.Add (resourceNameHash, er);
					}
#endif
				}
			}
		}

#if !SYSTEM_WEB_EXTENSIONS
		internal static string GetResourceUrl (Type type, string resourceName)
		{
			return GetResourceUrl (type.Assembly, resourceName, false);
		}
#endif
		static EmbeddedResource DecryptAssemblyResource (string val, out AssemblyEmbeddedResources entry)
		{
			entry = null;
			
			string[] parts = val.Split ('_');
			if (parts.Length != 3)
				return null;

			string asmNameHash = parts [0];
			string resNameHash = parts [1];
			try {
				_embeddedResourcesLock.EnterReadLock ();
				if (!_embeddedResources.TryGetValue (asmNameHash, out entry) || entry == null)
					return null;
				
				EmbeddedResource res;
				if (!entry.Resources.TryGetValue (resNameHash, out res) || res == null) {
#if SYSTEM_WEB_EXTENSIONS
					bool debug = parts [2] == "t";
					if (!debug)
						return null;

					if (!entry.Resources.TryGetValue (resNameHash.Substring (0, resNameHash.Length - 1), out res))
						return null;
#else
					return null;
#endif
				}
				
				return res;
			} finally {
				_embeddedResourcesLock.ExitReadLock ();
			}
		}

		static void GetAssemblyNameAndHashes (KeyedHashAlgorithm kha, Assembly assembly, string resourceName, out string assemblyName, out string assemblyNameHash, out string resourceNameHash)
		{
			assemblyName = assembly == currAsm ? "s" : assembly.GetName ().FullName;
			assemblyNameHash = GetStringHash (kha, assemblyName);
			resourceNameHash = GetStringHash (kha, resourceName);
		}
		
		// MUST be called with the _embeddedResourcesLock taken in the upgradeable read lock mode
		static AssemblyEmbeddedResources GetAssemblyEmbeddedResource (KeyedHashAlgorithm kha, Assembly assembly, string assemblyNameHash, string assemblyName)
		{
			AssemblyEmbeddedResources entry;
			
			if (!_embeddedResources.TryGetValue (assemblyNameHash, out entry) || entry == null) {
				try {
					_embeddedResourcesLock.EnterWriteLock ();
					entry = new AssemblyEmbeddedResources () {
							AssemblyName = assemblyName
								};
					InitEmbeddedResourcesUrls (kha, assembly, assemblyName, assemblyNameHash, entry);
					_embeddedResources.Add (assemblyNameHash, entry);
				} finally {
					_embeddedResourcesLock.ExitWriteLock ();
				}
			}

			return entry;
		}
		
		internal static string GetResourceUrl (Assembly assembly, string resourceName, bool notifyScriptLoaded)
		{
			if (assembly == null)
				return String.Empty;

			KeyedHashAlgorithm kha = ReusableHashAlgorithm;
			if (kha != null) {
				return GetResourceUrl (kha, assembly, resourceName, notifyScriptLoaded);
			} else {
				MachineKeySection mks = MachineKeySection.Config;
				using (kha = MachineKeySectionUtils.GetValidationAlgorithm (mks)) {
					kha.Key = MachineKeySectionUtils.GetValidationKey (mks);
					return GetResourceUrl (kha, assembly, resourceName, notifyScriptLoaded);
				}
			}
		}

		static string GetResourceUrl (KeyedHashAlgorithm kha, Assembly assembly, string resourceName, bool notifyScriptLoaded)
		{
			string assemblyName;
			string assemblyNameHash;
			string resourceNameHash;

			GetAssemblyNameAndHashes (kha, assembly, resourceName, out assemblyName, out assemblyNameHash, out resourceNameHash);
			bool debug = false;
			string url;
			AssemblyEmbeddedResources entry;
			bool includeTimeStamp = true;

			try {
				_embeddedResourcesLock.EnterUpgradeableReadLock ();
				entry = GetAssemblyEmbeddedResource (kha, assembly, assemblyNameHash, assemblyName);
				string lookupKey;
#if SYSTEM_WEB_EXTENSIONS
				debug = resourceName.EndsWith (".debug.js", StringComparison.OrdinalIgnoreCase);
				string dbgTail = debug ? "d" : String.Empty;
				lookupKey = resourceNameHash + (notifyScriptLoaded ? "t" : "f") + dbgTail;
#if NET_3_5
				CheckIfResourceIsCompositeScript (resourceName, ref includeTimeStamp);
#endif
#else
				lookupKey = resourceNameHash;
#endif
				EmbeddedResource res;
				if (entry.Resources.TryGetValue (lookupKey, out res) && res != null)
					url = res.Url;
				else {
#if SYSTEM_WEB_EXTENSIONS
					if (debug) {
						resourceNameHash = GetStringHash (kha, resourceName.Substring (0, resourceName.Length - 9) + ".js");
						lookupKey = resourceNameHash + (notifyScriptLoaded ? "t" : "f");
					
						if (entry.Resources.TryGetValue (lookupKey, out res) && res != null)
							url = res.Url;
						else
							url = null;
					} else
#endif
						url = null;
				}
			} finally {
				_embeddedResourcesLock.ExitUpgradeableReadLock ();
			}

			if (url == null)
				url = CreateResourceUrl (kha, assemblyName, assemblyNameHash, assembly.Location, resourceNameHash, debug, notifyScriptLoaded, includeTimeStamp);
			
			return url;
		}
		
		static string CreateResourceUrl (KeyedHashAlgorithm kha, string assemblyName, string assemblyNameHash, string assemblyPath, string resourceNameHash, bool debug,
						 bool notifyScriptLoaded, bool includeTimeStamp)
		{
			string atime = String.Empty;
			string extra = String.Empty;
#if SYSTEM_WEB_EXTENSIONS
			extra = QueryParamSeparator + "n=" + (notifyScriptLoaded ? "t" : "f");
#endif

#if TARGET_JVM
			atime = QueryParamSeparator + "t=" + assemblyName.GetHashCode ();
#else
			if (includeTimeStamp) {
				if (!String.IsNullOrEmpty (assemblyPath) && File.Exists (assemblyPath))
					atime = QueryParamSeparator + "t=" + File.GetLastWriteTimeUtc (assemblyPath).Ticks;
				else
					atime = QueryParamSeparator + "t=" + DateTime.UtcNow.Ticks;
			}
#endif
			string d = assemblyNameHash + "_" + resourceNameHash +  (debug ? "_t" : "_f");
			string href = HandlerFileName + "?d=" + d + atime + extra;
			HttpContext ctx = HttpContext.Current;
			HttpRequest req = ctx != null ? ctx.Request : null;
			if (req != null) {
				string appPath = VirtualPathUtility.AppendTrailingSlash (req.ApplicationPath);
				href = appPath + href;
			}
			
			return href;
		}

		bool HasCacheControl (HttpRequest request, NameValueCollection queryString, out long atime)
		{
			if (String.Compare (request.Headers ["Cache-Control"], "max-age=0", StringComparison.Ordinal) != 0) {
				atime = 0;
				return false;
			}
			
			if (Int64.TryParse (request.QueryString ["t"], out atime))
				return true;

			return false;
		}

		bool HasIfModifiedSince (HttpRequest request, out DateTime modified)
		{
			string modif_since = request.Headers ["If-Modified-Since"];
			if (String.IsNullOrEmpty (modif_since)) {
				modified = DateTime.MinValue;
				return false;
			}

			try {
				if (DateTime.TryParseExact (modif_since, "r", null, 0, out modified))
					return true;
			} catch {
			}

			return false;
		}

		void RespondWithNotModified (HttpContext context)
		{
			HttpResponse response = context.Response;
			response.Clear ();
			response.StatusCode = 304;
			response.ContentType = null;
			response.CacheControl = "public"; // easier to set it to public as MS than remove it
			context.ApplicationInstance.CompleteRequest ();
		}
		
		void SendEmbeddedResource (HttpContext context, out EmbeddedResource res, out Assembly assembly)
		{
			HttpRequest request = context.Request;
			NameValueCollection queryString = request.QueryString;
			
			// val is URL-encoded, which means every + has been replaced with ' ', we
			// need to revert that or the base64 conversion will fail.
			string d = queryString ["d"];
			if (!String.IsNullOrEmpty (d))
				d = d.Replace (' ', '+');

			AssemblyEmbeddedResources entry;
			res = DecryptAssemblyResource (d, out entry);
			WebResourceAttribute wra = res != null ? res.Attribute : null;
			if (wra == null)
				throw new HttpException (404, "Resource not found");

			if (entry.AssemblyName == "s")
				assembly = currAsm;
			else
				assembly = Assembly.Load (entry.AssemblyName);
			
			long atime;
			if (HasCacheControl (request, queryString, out atime)) {
				if (atime == File.GetLastWriteTimeUtc (assembly.Location).Ticks) {
					RespondWithNotModified (context);
					return;
				}
			}

			DateTime modified;
			if (HasIfModifiedSince (request, out modified)) {
				if (File.GetLastWriteTimeUtc (assembly.Location) <= modified) {
					RespondWithNotModified (context);
					return;
				}
			}

			HttpResponse response = context.Response;
			response.ContentType = wra.ContentType;

			DateTime utcnow = DateTime.UtcNow;
			response.Headers.Add ("Last-Modified", utcnow.ToString ("r"));
			response.ExpiresAbsolute = utcnow.AddYears (1);
			response.CacheControl = "public";

			Stream s = assembly.GetManifestResourceStream (res.Name);
			if (s == null)
				throw new HttpException (404, "Resource " + res.Name + " not found");

			if (wra.PerformSubstitution) {
				using (StreamReader r = new StreamReader (s)) {
					TextWriter w = response.Output;
					new PerformSubstitutionHelper (assembly).PerformSubstitution (r, w);
				}
			} else if (response.OutputStream is HttpResponseStream) {
				UnmanagedMemoryStream st = (UnmanagedMemoryStream) s;
				HttpResponseStream hstream = (HttpResponseStream) response.OutputStream;
				unsafe {
					hstream.WritePtr (new IntPtr (st.PositionPointer), (int) st.Length);
				}
			} else {
				byte [] buf = new byte [1024];
				Stream output = response.OutputStream;
				int c;
				do {
					c = s.Read (buf, 0, 1024);
					output.Write (buf, 0, c);
				} while (c > 0);
			}
		}
		
#if !SYSTEM_WEB_EXTENSIONS
		void System.Web.IHttpHandler.ProcessRequest (HttpContext context)
		{
			EmbeddedResource res;
			Assembly assembly;
			
			SendEmbeddedResource (context, out res, out assembly);
		}
#endif
		sealed class PerformSubstitutionHelper
		{
			readonly Assembly _assembly;
			static readonly Regex _regex = new Regex (@"\<%=[ ]*WebResource[ ]*\([ ]*""([^""]+)""[ ]*\)[ ]*%\>");

			public PerformSubstitutionHelper (Assembly assembly) {
				_assembly = assembly;
			}

			public void PerformSubstitution (TextReader reader, TextWriter writer) {
				string line = reader.ReadLine ();
				while (line != null) {
					if (line.Length > 0 && _regex.IsMatch (line))
						line = _regex.Replace (line, new MatchEvaluator (PerformSubstitutionReplace));
					writer.WriteLine (line);
					line = reader.ReadLine ();
				}
			}

			string PerformSubstitutionReplace (Match m) {
				string resourceName = m.Groups [1].Value;
#if SYSTEM_WEB_EXTENSIONS
				return ScriptResourceHandler.GetResourceUrl (_assembly, resourceName, false);
#else
				return AssemblyResourceLoader.GetResourceUrl (_assembly, resourceName, false);
#endif
			}
		}
		
#if !SYSTEM_WEB_EXTENSIONS
		bool System.Web.IHttpHandler.IsReusable { get { return true; } }
#endif
		sealed class EmbeddedResource
		{
			public string Name;
			public string Url;
			public WebResourceAttribute Attribute;
		}
		
		sealed class AssemblyEmbeddedResources
		{
			public string AssemblyName = String.Empty;
			public Dictionary <string, EmbeddedResource> Resources = new Dictionary <string, EmbeddedResource> (StringComparer.Ordinal);
		}		
	}
}

