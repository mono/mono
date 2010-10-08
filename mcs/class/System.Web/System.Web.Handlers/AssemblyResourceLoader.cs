//
// System.Web.Handlers.AssemblyResourceLoader
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
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

using System.Web.UI;
using System.Globalization;
using System.Reflection;
using System.IO;
using System.Resources;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Configuration;

namespace System.Web.Handlers {
#if SYSTEM_WEB_EXTENSIONS
	partial class ScriptResourceHandler
	{
		const string HandlerFileName = "ScriptResource.axd";
		static Assembly currAsm = typeof (ScriptResourceHandler).Assembly;
#else
	#if NET_2_0
	public sealed
	#else
	internal // since this is in the .config file, we need to support it, since we dont have versoned support.
	#endif
	class AssemblyResourceLoader : IHttpHandler {		
		const string HandlerFileName = "WebResource.axd";
		static Assembly currAsm = typeof (AssemblyResourceLoader).Assembly;
#endif
		const char QueryParamSeparator = '&';

		static readonly Hashtable _embeddedResources = Hashtable.Synchronized (new Hashtable ());

		static void InitEmbeddedResourcesUrls (Assembly assembly, Hashtable hashtable)
		{
			WebResourceAttribute [] attrs = (WebResourceAttribute []) assembly.GetCustomAttributes (typeof (WebResourceAttribute), false);
			for (int i = 0; i < attrs.Length; i++) {
				string resourceName = attrs [i].WebResource;
				if (resourceName != null && resourceName.Length > 0) {
#if SYSTEM_WEB_EXTENSIONS
					ResourceKey rkNoNotify = new ResourceKey (resourceName, false);
					ResourceKey rkNotify = new ResourceKey (resourceName, true);

					if (!hashtable.Contains (rkNoNotify))
						hashtable.Add (rkNoNotify, CreateResourceUrl (assembly, resourceName, false));
					if (!hashtable.Contains (rkNotify))
						hashtable.Add (rkNotify, CreateResourceUrl (assembly, resourceName, true));
#else
					if (!hashtable.Contains (resourceName))
						hashtable.Add (resourceName, CreateResourceUrl (assembly, resourceName, false));
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

		static string EncryptAssemblyResource (string asmName, string resName)
		{
			byte[] bytes = Encoding.UTF8.GetBytes (String.Concat (asmName, ";", resName));
			bytes = MachineKeySectionUtils.Encrypt (MachineKeySection.Config, bytes);
			return Convert.ToBase64String (bytes);
		}

		static void DecryptAssemblyResource (string val, out string asmName, out string resName)
		{
			byte[] bytes = Convert.FromBase64String (val);

			asmName = null;
			resName = null;			

			byte[] result = MachineKeySectionUtils.Decrypt (MachineKeySection.Config, bytes);
			bytes = null;
			// null will be returned if, for any reason, decryption fails
			if (result == null)
				return;

			string data = Encoding.UTF8.GetString (result);
			result = null;

			string[] parts = data.Split (';');
			if (parts.Length != 2)
				return;
			
			asmName = parts [0];
			resName = parts [1];
		}

		internal static string GetResourceUrl (Assembly assembly, string resourceName, bool notifyScriptLoaded)
		{
			Hashtable hashtable = (Hashtable)_embeddedResources [assembly];
			if (hashtable == null) {
				hashtable = new Hashtable ();
				InitEmbeddedResourcesUrls (assembly, hashtable);
				_embeddedResources [assembly] = hashtable;
			}
#if SYSTEM_WEB_EXTENSIONS
			string url = (string) hashtable [new ResourceKey (resourceName, notifyScriptLoaded)];
#else
			string url = (string) hashtable [resourceName];
#endif
			if (url == null)
				url = CreateResourceUrl (assembly, resourceName, notifyScriptLoaded);
			return url;
		}
		
		static string CreateResourceUrl (Assembly assembly, string resourceName, bool notifyScriptLoaded)
		{

			string aname = assembly == currAsm ? "s" : assembly.GetName ().FullName;
			string apath = assembly.Location;
			string atime = String.Empty;
			string extra = String.Empty;
#if SYSTEM_WEB_EXTENSIONS
			extra = String.Concat (QueryParamSeparator, "n=", notifyScriptLoaded ? "t" : "f");
#endif

#if TARGET_JVM
			atime = String.Format ("{0}t={1}", QueryParamSeparator, assembly.GetHashCode ());
#else
			if (apath != String.Empty)
				atime = String.Concat (QueryParamSeparator, "t=", File.GetLastWriteTimeUtc (apath).Ticks);
#endif
			string href = HandlerFileName + "?d=" + EncryptAssemblyResource (aname, resourceName) + atime + extra;

			HttpContext ctx = HttpContext.Current;
			if (ctx != null && ctx.Request != null) {
				string appPath = VirtualPathUtility.AppendTrailingSlash (ctx.Request.ApplicationPath);
				href = appPath + href;
			}
			
			return href;
		}

#if SYSTEM_WEB_EXTENSIONS
		protected virtual void ProcessRequest (HttpContext context)
#else
		[MonoTODO ("Substitution not implemented")]
		void System.Web.IHttpHandler.ProcessRequest (HttpContext context)
#endif
		{
			HttpRequest request = context.Request;
			HttpResponse response = context.Response;
			string resourceName;
			string asmName;
			Assembly assembly;

			DecryptAssemblyResource (request.QueryString ["d"], out asmName, out resourceName);
			if (resourceName == null)
				throw new HttpException (404, "No resource name given");

			if (asmName == null || asmName == "s")
				assembly = currAsm;
			else
				assembly = Assembly.Load (asmName);

			WebResourceAttribute wra = null;
			WebResourceAttribute [] attrs = (WebResourceAttribute []) assembly.GetCustomAttributes (typeof (WebResourceAttribute), false);
			for (int i = 0; i < attrs.Length; i++) {
				if (attrs [i].WebResource == resourceName) {
					wra = attrs [i];
					break;
				}
			}
#if SYSTEM_WEB_EXTENSIONS
			if (wra == null && resourceName.Length > 9 && resourceName.EndsWith (".debug.js", StringComparison.OrdinalIgnoreCase)) {
				resourceName = String.Concat (resourceName.Substring (0, resourceName.Length - 9), ".js");
				for (int i = 0; i < attrs.Length; i++) {
					if (attrs [i].WebResource == resourceName) {
						wra = attrs [i];
						break;
					}
				}
			}
#endif
			if (wra == null)
				throw new HttpException (404, String.Concat ("Resource ", resourceName, " not found"));
			
			string req_cache = request.Headers ["Cache-Control"];
			if (req_cache == "max-age=0") {
				long atime;
#if NET_2_0
				if (Int64.TryParse (request.QueryString ["t"], out atime)) {
#else
				atime = -1;
				try {
					atime = Int64.Parse (request.QueryString ["t"]);
				} catch {}
				if (atime > -1) {
#endif
					if (atime == File.GetLastWriteTimeUtc (assembly.Location).Ticks) {
						response.Clear ();
						response.StatusCode = 304;
						response.ContentType = null;
						response.CacheControl = "public"; // easier to set it to public as MS than remove it
						context.ApplicationInstance.CompleteRequest ();
						return;
					}
				}
			}
			string modif_since = request.Headers ["If-Modified-Since"];
			if (modif_since != null && modif_since != "") {
				try {
					DateTime modif;
#if NET_2_0
					if (DateTime.TryParseExact (modif_since, "r", null, 0, out modif))
#else
					modif = DateTime.MinValue;
					try {
						modif = DateTime.ParseExact (modif_since, "r", null, 0);
					} catch { }
					if (modif != DateTime.MinValue)
#endif
						if (File.GetLastWriteTimeUtc (assembly.Location) <= modif) {
							response.Clear ();
							response.StatusCode = 304;
							response.ContentType = null;
							response.CacheControl = "public"; // easier to set it to public as MS than remove it
							context.ApplicationInstance.CompleteRequest ();
							return;
						}
				} catch {}
			}

			response.ContentType = wra.ContentType;

			DateTime utcnow = DateTime.UtcNow;
			response.Headers.Add ("Last-Modified", utcnow.ToString ("r"));
			response.ExpiresAbsolute = utcnow.AddYears (1);
			response.CacheControl = "public";

			Stream s = assembly.GetManifestResourceStream (resourceName);
			if (s == null)
				throw new HttpException (404, String.Concat ("Resource ", resourceName, " not found"));

			if (wra.PerformSubstitution) {
				using (StreamReader r = new StreamReader (s)) {
					TextWriter w = response.Output;
					new PerformSubstitutionHelper (assembly).PerformSubstitution (r, w);
				}
#if NET_2_0
			} else if (response.OutputStream is HttpResponseStream) {
				UnmanagedMemoryStream st = (UnmanagedMemoryStream) s;
				HttpResponseStream hstream = (HttpResponseStream) response.OutputStream;
				unsafe {
					hstream.WritePtr (new IntPtr (st.PositionPointer), (int) st.Length);
				}
#endif
			} else {
				byte [] buf = new byte [1024];
				Stream output = response.OutputStream;
				int c;
				do {
					c = s.Read (buf, 0, 1024);
					output.Write (buf, 0, c);
				} while (c > 0);
			}
#if SYSTEM_WEB_EXTENSIONS
			TextWriter writer = response.Output;
			foreach (ScriptResourceAttribute sra in assembly.GetCustomAttributes (typeof (ScriptResourceAttribute), false)) {
				if (sra.ScriptName == resourceName) {
					string scriptResourceName = sra.ScriptResourceName;
					ResourceSet rset = null;
					try {
						rset = new ResourceManager (scriptResourceName, assembly).GetResourceSet (Threading.Thread.CurrentThread.CurrentUICulture, true, true);
					}
					catch (MissingManifestResourceException) {
#if TARGET_JVM // GetResourceSet does not throw  MissingManifestResourceException if ressource is not exists
					}
					if (rset == null) {
#endif
						if (scriptResourceName.EndsWith (".resources")) {
							scriptResourceName = scriptResourceName.Substring (0, scriptResourceName.Length - 10);
							rset = new ResourceManager (scriptResourceName, assembly).GetResourceSet (Threading.Thread.CurrentThread.CurrentUICulture, true, true);
						}
#if !TARGET_JVM
						else
							throw;
#endif
					}
					if (rset == null)
						break;
					writer.WriteLine ();
					string ns = sra.TypeName;
					int indx = ns.LastIndexOf ('.');
					if (indx > 0)
						writer.WriteLine ("Type.registerNamespace('" + ns.Substring (0, indx) + "')");
					writer.Write ("{0}={{", sra.TypeName);
					bool first = true;
					foreach (DictionaryEntry entry in rset) {
						string value = entry.Value as string;
						if (value != null) {
							if (first)
								first = false;
							else
								writer.Write (',');
							writer.WriteLine ();
							writer.Write ("{0}:{1}", GetScriptStringLiteral ((string) entry.Key), GetScriptStringLiteral (value));
						}
					}
					writer.WriteLine ();
					writer.WriteLine ("};");
					break;
				}
			}

			bool notifyScriptLoaded = request.QueryString ["n"] == "t";
			if (notifyScriptLoaded) {
				writer.WriteLine ();
				writer.WriteLine ("if(typeof(Sys)!=='undefined')Sys.Application.notifyScriptLoaded();");
			}
#endif
		}

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
	}
}

