// 
// System.Web.Configuration.HandlerItem
//
// Authors:
// 	Patrik Torstensson (ptorsten@hotmail.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
//   (c) 2002 Ximian, Inc. (http://www.ximian.com) 
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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace System.Web.Configuration
{
	class HandlerItem
	{
		private Type _type;
		private string _typeName;
		private string _path;
		private string _requestType;
		private Regex requestRegex;
		private Regex pathRegex;
		static Hashtable regexCache;
		object instance;
		bool validated;

		public HandlerItem (string requestType, string path, string type, bool validate)
		{
			_typeName = type;
			_path = path;
			_requestType = requestType.Replace (" ", "");
			requestRegex = GetRegex (_requestType);
			pathRegex = GetRegex (_path);
			if (validate)
				DoValidation ();
		}

		public object GetInstance ()
		{
			object obj = Interlocked.CompareExchange (ref instance, null, null);
			if (obj != null)
				return obj;

			DoValidation ();
			obj = HttpRuntime.CreateInternalObject (_type);
			IHttpHandler hnd = obj as IHttpHandler;
			if (hnd != null && hnd.IsReusable)
				Interlocked.CompareExchange (ref instance, hnd, null);

			return obj;
		}

		public Type Type
		{
			get {
				DoValidation ();
				return _type;
			}
		}

		public bool IsMatch (string type, string path)
		{
			return (MatchVerb (type) && MatchPath (path));
		}

		bool MatchVerb (string verb)
		{
			return requestRegex.IsMatch (verb);
		}

		bool MatchPath (string path)
		{
			if (pathRegex.IsMatch (path))
				return true;

			int slash = path.LastIndexOf ('/');
			if (slash != -1 && path.Length > slash + 1)
				return pathRegex.IsMatch (path.Substring (slash + 1));

			return false;
		}

		void DoValidation ()
		{
			if (validated)
				return;

			lock (this) {
				Type t = Type.GetType (_typeName, true);
				_type = t;
				validated = true;
			}

			if (typeof (IHttpHandler).IsAssignableFrom (_type))
				return;
			
			if (typeof (IHttpHandlerFactory).IsAssignableFrom (_type))
				return;

			throw new HttpException (HttpRuntime.FormatResourceString ("type_not_factory_or_handler"));
		}

		static string ToRegexPattern (string dosPattern)
		{
			string result = dosPattern.Replace (".", "\\.");
			result = result.Replace ("*", ".*");
			result = result.Replace ('?', '.');
			return result;
		}
			
		static Regex GetRegex (string verb)
		{
			EnsureCache ();
			if (regexCache.ContainsKey (verb))
				return (Regex) regexCache [verb];

			StringBuilder result = new StringBuilder ("\\A");
			string [] expressions = verb.Split (',');
			int end = expressions.Length;
			for (int i = 0; i < end; i++) {
				string regex = ToRegexPattern (expressions [i]);
				if (i + 1 < end) {
					result.AppendFormat ("{0}\\z|\\A", regex);
				} else {
					result.AppendFormat ("({0})\\z", regex);
				}
			}

			Regex r = new Regex (result.ToString ());
			regexCache [verb] = r;
			return r;
		}

		static void EnsureCache ()
		{
			if (regexCache == null)
				regexCache = new Hashtable ();
		}
	}
}

