//
// ScriptResourceHandler.cs
//
// Authors:
//   Igor Zelmanovich <igorz@mainsoft.com>
//   Marek Habersack <grendel@twistedcode.net>
//
// (C) 2007 Mainsoft, Inc.  http://www.mainsoft.com
// (C) 2011 Novell, Inc.  http://novell.com
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
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.UI;
using System.Web.Util;

namespace System.Web.Handlers
{
	public partial class ScriptResourceHandler : IHttpHandler
	{		
		protected virtual bool IsReusable {
			get { return true; }
		}

		#region IHttpHandler Members

		bool IHttpHandler.IsReusable {
			get { return IsReusable; }
		}

		void IHttpHandler.ProcessRequest (HttpContext context) {
			ProcessRequest (context);
		}

		#endregion
#if NET_3_5
		void AppendResourceScriptContents (StringWriter sw, CompositeEntry entry)
		{
			if (entry.Assembly == null || entry.Attribute == null || String.IsNullOrEmpty (entry.NameOrPath))
				return;

			using (Stream s = entry.Assembly.GetManifestResourceStream (entry.NameOrPath)) {
				if (s == null)
					throw new HttpException (404, "Resource '" + entry.NameOrPath + "' not found");

				if (entry.Attribute.PerformSubstitution) {
					using (var r = new StreamReader (s)) {
						new PerformSubstitutionHelper (entry.Assembly).PerformSubstitution (r, sw);
					}
				} else {
					using (var r = new StreamReader (s)) {
						string line = r.ReadLine ();
						while (line != null) {
							sw.WriteLine (line);
							line = r.ReadLine ();
						}
					}
				}
			}
		}

		void AppendFileScriptContents (StringWriter sw, CompositeEntry entry)
		{
			// FIXME: should we limit the script size in any way?
			if (String.IsNullOrEmpty (entry.NameOrPath))
				return;

			string mappedPath;
			if (!HostingEnvironment.HaveCustomVPP) {
				// We'll take a shortcut here by bypassing the default VPP layers
				mappedPath = HostingEnvironment.MapPath (entry.NameOrPath);
				if (!File.Exists (mappedPath))
					return;
				sw.Write (File.ReadAllText (mappedPath));
				return;
			}

			VirtualPathProvider vpp = HostingEnvironment.VirtualPathProvider;
			if (!vpp.FileExists (entry.NameOrPath))
				return;
			VirtualFile file = vpp.GetFile (entry.NameOrPath);
			if (file == null)
				return;
			using (Stream s = file.Open ()) {
				using (var r = new StreamReader (s)) {
					string line = r.ReadLine ();
					while (line != null) {
						sw.WriteLine (line);
						line = r.ReadLine ();
					}
				}
			}
		}
		
		void AppendScriptContents (StringWriter sw, CompositeEntry entry)
		{
			if (entry.Assembly != null)
				AppendResourceScriptContents (sw, entry);
			else
				AppendFileScriptContents (sw, entry);
		}
		
		void SendCompositeScript (HttpContext context, HttpRequest request, bool notifyScriptLoaded, List <CompositeEntry> entries)
		{
			if (entries.Count == 0)
				throw new HttpException (404, "Resource not found");

			long atime;
			DateTime modifiedSince;
			bool hasCacheControl = HasCacheControl (request, request.QueryString, out atime);
			bool hasIfModifiedSince = HasIfModifiedSince (context.Request, out modifiedSince);
			
			if (hasCacheControl || hasIfModifiedSince) {
				bool notModified = true;
			
				foreach (CompositeEntry entry in entries) {
					if (entry == null)
						continue;
					if (notModified) {
						if ((hasCacheControl && entry.IsModifiedSince (atime)) || (hasIfModifiedSince && entry.IsModifiedSince (modifiedSince)))
							notModified = false;
					}
				}

				if (notModified) {
					RespondWithNotModified (context);
					return;
				}
			}
			
			StringBuilder contents = new StringBuilder ();
			using (var sw = new StringWriter (contents)) {
				foreach (CompositeEntry entry in entries) {
					if (entry == null)
						continue;
					AppendScriptContents (sw, entry);
				}
			}
			if (contents.Length == 0)
				throw new HttpException (404, "Resource not found");

			HttpResponse response = context.Response;
			DateTime utcnow = DateTime.UtcNow;

			response.ContentType = "text/javascript";
			response.Headers.Add ("Last-Modified", utcnow.ToString ("r"));
			response.ExpiresAbsolute = utcnow.AddYears (1);
			response.CacheControl = "public";

			response.Output.Write (contents.ToString ());
			if (notifyScriptLoaded)
				OutputScriptLoadedNotification (response.Output);
		}
#endif
		void OutputScriptLoadedNotification (TextWriter writer)
		{
			writer.WriteLine ();
			writer.WriteLine ("if(typeof(Sys)!=='undefined')Sys.Application.notifyScriptLoaded();");
		}
		
		protected virtual void ProcessRequest (HttpContext context)
		{
			HttpRequest request = context.Request;
			bool notifyScriptLoaded = request.QueryString ["n"] == "t";
#if NET_3_5
			List <CompositeEntry> compositeEntries = CompositeScriptReference.GetCompositeScriptEntries (request.RawUrl);
			if (compositeEntries != null) {
				SendCompositeScript (context, request, notifyScriptLoaded, compositeEntries);
				return;
			}
#endif
			EmbeddedResource res;
			Assembly assembly;			
			SendEmbeddedResource (context, out res, out assembly);

			HttpResponse response = context.Response;
			TextWriter writer = response.Output;
			foreach (ScriptResourceAttribute sra in assembly.GetCustomAttributes (typeof (ScriptResourceAttribute), false)) {
				if (String.Compare (sra.ScriptName, res.Name, StringComparison.Ordinal) == 0) {
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
						if (scriptResourceName.EndsWith (".resources", RuntimeHelpers.StringComparison)) {
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
					foreach (DictionaryEntry de in rset) {
						string value = de.Value as string;
						if (value != null) {
							if (first)
								first = false;
							else
								writer.Write (',');
							writer.WriteLine ();
							writer.Write ("{0}:{1}", GetScriptStringLiteral ((string) de.Key), GetScriptStringLiteral (value));
						}
					}
					writer.WriteLine ();
					writer.WriteLine ("};");
					break;
				}
			}
			
			if (notifyScriptLoaded)
				OutputScriptLoadedNotification (writer);
		}
#if NET_3_5
		static void CheckIfResourceIsCompositeScript (string resourceName, ref bool includeTimeStamp)
		{
			bool isCompositeScript = resourceName.StartsWith (CompositeScriptReference.COMPOSITE_SCRIPT_REFERENCE_PREFIX, StringComparison.Ordinal);
			if (!isCompositeScript)
				return;
			
			includeTimeStamp = false;
		}

		bool HandleCompositeScriptRequest (HttpContext context, HttpRequest request, string d)
		{
			return false;
		}
#endif
		// TODO: add value cache?
		static string GetScriptStringLiteral (string value)
		{
			if (String.IsNullOrEmpty (value))
				return "\"" + value + "\"";
			
			var sb = new StringBuilder ("\"");
			for (int i = 0; i < value.Length; i++) {
				char ch = value [i];
				switch (ch) {
					case '\'':
						sb.Append ("\\u0027");
						break;

					case '"':
						sb.Append ("\\\"");
						break;

					case '\\':
						sb.Append ("\\\\");
						break;

					case '\n':
						sb.Append ("\\n");
						break;

					case '\r':
						sb.Append ("\\r");
						break;

					default:
						sb.Append (ch);
						break;
				}
			}
			sb.Append ("\"");
			
			return sb.ToString ();
		}
	}
}
