// 
// System.Web.Configuration.HandlerItem
//
// Authors:
// 	Patrik Torstensson (ptorsten@hotmail.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
//   (c) 2002 Ximian, Inc. (http://www.ximian.com) 
//
using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

namespace System.Web.Configuration
{
	public class HandlerItem
	{
		private Type _type;
		private string _typeName;
		private string _path;
		private string _requestType;
		private Regex requestRegex;
		private Regex pathRegex;
		static Hashtable regexCache;

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

		public object Create ()
		{
			if (_type == null)
				DoValidation ();

			return HttpRuntime.CreateInternalObject (_type);
		}

		public Type Type
		{
			get {
				if (_type == null)
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
			return pathRegex.IsMatch (path);
		}

		void DoValidation ()
		{
			_type = Type.GetType (_typeName, true);
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

