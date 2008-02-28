//
// System.Web.Configuration.HttpHandlerAction
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//	Daniel Nauck    (dna(at)mono-project(dot)de)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
// (C) 2008 Daniel Nauck
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

#if NET_2_0

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Util;

namespace System.Web.Configuration
{
	public sealed class HttpHandlerAction: ConfigurationElement
	{
		static ConfigurationPropertyCollection _properties;
		static ConfigurationProperty pathProp;
		static ConfigurationProperty typeProp;
		static ConfigurationProperty validateProp;
		static ConfigurationProperty verbProp;

		static HttpHandlerAction ()
		{
			pathProp = new ConfigurationProperty ("path", typeof (string), null,
							      TypeDescriptor.GetConverter (typeof (string)),
							      PropertyHelper.NonEmptyStringValidator,
							      ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
			typeProp = new ConfigurationProperty ("type", typeof (string), null,
							      TypeDescriptor.GetConverter (typeof (string)),
							      PropertyHelper.NonEmptyStringValidator,
							      ConfigurationPropertyOptions.IsRequired);
			validateProp = new ConfigurationProperty ("validate", typeof (bool), true);
			verbProp = new ConfigurationProperty ("verb", typeof (string), null,
							      TypeDescriptor.GetConverter (typeof (string)),
							      PropertyHelper.NonEmptyStringValidator,
							      ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);

			_properties = new ConfigurationPropertyCollection ();
			_properties.Add (pathProp);
			_properties.Add (typeProp);
			_properties.Add (validateProp);
			_properties.Add (verbProp);
		}

		internal HttpHandlerAction ()
		{ }

		public HttpHandlerAction (string path, string type, string verb)
			: this (path, type, verb, true)
		{ }

		public HttpHandlerAction (string path, string type, string verb, bool validate)
		{
			Path = path;
			Type = type;
			Verb = verb;
			Validate = validate;
		}

		[ConfigurationProperty ("path", Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		// LAMESPEC: MS lists no validator here but provides one in Properties.
		public string Path {
			get { return (string) base[pathProp]; }
			set { base[pathProp] = value; }
		}

		[ConfigurationProperty ("type", Options = ConfigurationPropertyOptions.IsRequired)]
		// LAMESPEC: MS lists no validator here but provides one in Properties.
		public string Type {
			get { return (string) base[typeProp]; }
			set { base[typeProp] = value; }
		}

		[ConfigurationProperty ("validate", DefaultValue = true)]
		public bool Validate {
			get { return (bool) base[validateProp]; }
			set { base[validateProp] = value; }
		}

		[ConfigurationProperty ("verb", Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		// LAMESPEC: MS lists no validator here but provides one in Properties.
		public string Verb {
			get { return (string) base[verbProp]; }
			set { base[verbProp] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return _properties; }
		}

#region CompatabilityCode
		object instance;
		Type type;

		string cached_verb = null;
		string[] cached_verbs;
		Dictionary<string, bool> cachedMatches = new Dictionary<string, bool> ();
		Object pathMatchesLock = new Object ();

		string[] SplitVerbs ()
		{
			if (Verb == "*")
				cached_verbs = null;
			else
				cached_verbs = Verb.Split (',');

			return cached_verbs;
		}

		internal string[] Verbs {
			get {
				if (cached_verb != Verb) {
					cached_verbs = SplitVerbs();
					cached_verb = Verb;
				}

				return cached_verbs;
			}
		}

		//
		// Loads the a type by name and verifies that it implements
		// IHttpHandler or IHttpHandlerFactory
		//
		internal static Type LoadType (string type_name)
		{
			Type t = null;
			
			t = HttpApplication.LoadType (type_name, false);

			if (t == null)
				throw new HttpException (String.Format ("Failed to load httpHandler type `{0}'", type_name));

			if (typeof (IHttpHandler).IsAssignableFrom (t) ||
			    typeof (IHttpHandlerFactory).IsAssignableFrom (t))
				return t;
			
			throw new HttpException (String.Format ("Type {0} does not implement IHttpHandler or IHttpHandlerFactory", type_name));
		}

		internal bool PathMatches (string pathToMatch)
		{
			if (cachedMatches.ContainsKey (pathToMatch))
				return cachedMatches [pathToMatch];

			lock (pathMatchesLock)
			{
				if (cachedMatches.ContainsKey (pathToMatch))
					return cachedMatches [pathToMatch];

				bool result = false;
				string[] handlerPaths = Path.Split (',');
				int slash = pathToMatch.LastIndexOf ('/');
				string origPathToMatch = pathToMatch;
				if (slash != -1)
					pathToMatch = pathToMatch.Substring (slash);

				foreach (string handlerPath in handlerPaths)
				{
					if (handlerPath == "*")
					{
						result = true;
						break;
					}

					string matchExact = null;
					string endsWith = null;
					Regex regEx = null;

					if (handlerPath.Length > 0)
					{
						if (handlerPath [0] == '*' && (handlerPath.IndexOf ('*', 1) == -1))
							endsWith = handlerPath.Substring (1);

						if (handlerPath.IndexOf ('*') == -1)
							if (handlerPath [0] != '/')
							{
								HttpContext ctx = HttpContext.Current;
								HttpRequest req = ctx != null ? ctx.Request : null;
								string vpath = req != null ? req.BaseVirtualDir : HttpRuntime.AppDomainAppVirtualPath;

								if (vpath == "/")
									vpath = String.Empty;

								matchExact = String.Concat (vpath, "/", handlerPath);
							}
					}

					if (matchExact != null)
					{
						result = matchExact.Length == origPathToMatch.Length && StrUtils.EndsWith (origPathToMatch, matchExact, true);
						if (result == true)
							break;
						else
							continue;
					}
					else if (endsWith != null)
					{
						result = StrUtils.EndsWith (pathToMatch, endsWith, true);
						if (result == true)
							break;
						else
							continue;
					}

					if (handlerPath != "*")
					{
						string expr = handlerPath.Replace (".", "\\.").Replace ("?", "\\?").Replace ("*", ".*");
						if (expr.Length > 0 && expr [0] == '/')
							expr = expr.Substring (1);

						expr += "\\z";
						regEx = new Regex (expr, RegexOptions.IgnoreCase);

						if (regEx.IsMatch (origPathToMatch))
						{
							result = true;
							break;
						}
					}
				}

				if (!cachedMatches.ContainsKey (origPathToMatch))
					cachedMatches.Add (origPathToMatch, result);

				return result;
			}
		}

		// Loads the handler, possibly delay-loaded.
		internal object GetHandlerInstance ()
		{
			IHttpHandler ihh = instance as IHttpHandler;
			
			if (instance == null || (ihh != null && !ihh.IsReusable)){
				if (type == null)
					type = LoadType (Type);

				instance = Activator.CreateInstance (type);
			} 
			
			return instance;
		}
#endregion

	}

}

#endif
