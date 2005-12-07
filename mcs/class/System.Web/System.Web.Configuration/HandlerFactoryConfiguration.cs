//
// System.Web.Configuration.HandlerFactoryConfiguration.cs
//  
//
// Authors:
//	Miguel de Icaza (miguel@novell.com)
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Web.Util;
using System.Text.RegularExpressions;

namespace System.Web.Configuration {

	class FileMatchingInfo {
		public string MatchExact;
		public string MatchExpr;

		// If set, we can fast-path the patch with string.EndsWith (FMI.EndsWith)
		public string EndsWith;
		public Regex RegExp;
		
		public FileMatchingInfo (string s)
		{
			MatchExpr = s;

			if (s[0] == '*' && (s.IndexOf ('*', 1) == -1))
				EndsWith = s.Substring (1);

			if (s.IndexOf ('*') == -1)
				MatchExact = "/" + s;

			if (MatchExpr != "*") {
				string expr = MatchExpr.Replace(".", "\\.").Replace("?", "\\?").Replace("*", ".*");
				if (expr.Length > 0 && expr [0] =='/')
					expr = expr.Substring (1);

				expr += "\\z";
				RegExp = new Regex (expr);
			}
		}
	}
	
	class HttpHandler {
		// If `null', we are the "*" match
		public string OriginalVerb;
		public string OriginalPath;
		
		public string [] Verbs;
		public FileMatchingInfo [] files;

		// To support lazy loading we keep the name around.
		public string TypeName;
		Type type;

		object instance;
		
		public HttpHandler (string verb, string path, string typename, Type t)
		{
			OriginalVerb = verb;
			OriginalPath = path;
			
			if (verb != "*")
				Verbs = verb.Split (',');
			string [] paths = path.Split (',');
			files = new FileMatchingInfo [paths.Length];

			int i = 0;
			foreach (string s in paths)
				files [i++] = new FileMatchingInfo (s);
			
			this.TypeName = typename;
			type = t;
		}

		//
		// Loads the a type by name and verifies that it implements
		// IHttpHandler or IHttpHandlerFactory
		//
		public static Type LoadType (string type_name)
		{
			Type t;
			
			try {
				t = Type.GetType (type_name, true);
			} catch (Exception e) {
				throw new HttpException (String.Format ("Failed to load httpHandler type `{0}'", type_name));
			}

			if (typeof (IHttpHandler).IsAssignableFrom (t) ||
			    typeof (IHttpHandlerFactory).IsAssignableFrom (t))
				return t;
			
			throw new HttpException (String.Format ("Type {0} does not implement IHttpHandler or IHttpHandlerFactory", type_name));
		}

		public bool PathMatches (string p)
		{
			int slash = p.LastIndexOf ('/');
			string orig = p;
			if (slash != -1)
				p = p.Substring (slash);

			for (int j = files.Length; j > 0; ){
				j--;
				FileMatchingInfo fm = files [j];

				if (fm.MatchExact != null)
					return fm.MatchExact.Length == p.Length && StrUtils.EndsWith (p, fm.MatchExact);
					
				if (fm.EndsWith != null)
					return StrUtils.EndsWith (p, fm.EndsWith);

				if (fm.MatchExpr == "*")
					return true;

				/* convert to regexp */
				return fm.RegExp.IsMatch (orig);
			}
			return false;
		}

		// Loads the handler, possibly delay-loaded.
		public object GetHandlerInstance ()
		{
			IHttpHandler ihh = instance as IHttpHandler;
			
			if (instance == null || (ihh != null && !ihh.IsReusable)){
				if (type == null)
					type = LoadType (TypeName);

				instance = Activator.CreateInstance (type);
			} 
			
			return instance;
		}
	}
	
	class HandlerFactoryConfiguration {
		ArrayList handlers;
		HandlerFactoryConfiguration parent;
		int parent_items;

		public HandlerFactoryConfiguration (HandlerFactoryConfiguration parent)
		{
			this.parent = parent;

			if (parent != null) {
				handlers = new ArrayList (parent.handlers);
				parent_items = handlers.Count;
			} else {
				handlers = new ArrayList ();
			}
		}

		public void Clear ()
		{
			handlers.Clear ();
		}

		public void Add (string verb, string path, string type_name, bool validate)
		{
			Type type;

			if (validate){
				type = HttpHandler.LoadType (type_name);
				if (type == null)
					throw new HttpException (String.Format ("Can not load {0}", type_name));
			} else
				type = null;
			
			handlers.Add (new HttpHandler (verb, path, type_name, type));
		}

		public bool Remove (string verb, string path)
		{
			for (int i = handlers.Count - 1; i >= 0; i--) {
				HttpHandler handler = (HttpHandler) handlers [i];

				if (verb == handler.OriginalVerb && path == handler.OriginalPath){
					handlers.RemoveAt (i);
					return true;
				}
			}

			return false;
		}

		public object LocateHandler (string verb, string filepath)
		{
			int start, end;
			int count = handlers.Count;
			for (int k = 0; k < 2; k++) {
				// First iteration searches for the mapping in the items added to this
				// instance. The second one searches through the parent items if any.
				start = (k == 0) ? parent_items : 0;
				end = (k == 0) ? count : parent_items;
				for (int i = start; i < end; i++) {
					HttpHandler handler = (HttpHandler) handlers [i];

					if (handler.Verbs == null){
						if (handler.PathMatches (filepath))
							return handler.GetHandlerInstance ();
						continue;
					}

					string [] verbs = handler.Verbs;
					for (int j = verbs.Length; j > 0; ){
						j--;
						if (verbs [j] != verb)
							continue;
						if (handler.PathMatches (filepath))
							return handler.GetHandlerInstance ();
					}
				}
			}

			return null;
		}
	}
}

