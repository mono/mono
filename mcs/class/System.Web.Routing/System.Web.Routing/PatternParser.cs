//
// PatternParser.cs
//
// Author:
//      Atsushi Enomoto <atsushi@ximian.com>
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2008-2009 Novell Inc. http://novell.com
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
using System.Collections.Generic;
using System.Security.Permissions;
using System.Text;
using System.Web;
using System.Web.Util;

namespace System.Web.Routing
{
	sealed class PatternParser
	{
		struct PatternSegment
		{
			public bool AllLiteral;
			public List <PatternToken> Tokens;
		}
		
		static readonly char[] placeholderDelimiters = { '{', '}' };
		
		PatternSegment[] segments;
		Dictionary <string, bool> parameterNames;
		PatternToken[] tokens;
		
		int segmentCount;
		bool haveSegmentWithCatchAll;
		
		public string Url {
			get;
			private set;
		}
		
		public PatternParser (string pattern)
		{
			this.Url = pattern;
			Parse ();
		}

		void Parse ()
		{
			string url = Url;
			parameterNames = new Dictionary <string, bool> (StringComparer.OrdinalIgnoreCase);
			
			if (!String.IsNullOrEmpty (url)) {
				if (url [0] == '~' || url [0] == '/')
					throw new ArgumentException ("Url must not start with '~' or '/'");
				if (url.IndexOf ('?') >= 0)
					throw new ArgumentException ("Url must not contain '?'");
			} else {
				segments = new PatternSegment [0];
				tokens = new PatternToken [0];
				return;
			}
			
			string[] parts = url.Split ('/');
			int partsCount = segmentCount = parts.Length;
			var allTokens = new List <PatternToken> ();
			PatternToken tmpToken;
			
			segments = new PatternSegment [partsCount];
			
			for (int i = 0; i < partsCount; i++) {
				if (haveSegmentWithCatchAll)
					throw new ArgumentException ("A catch-all parameter can only appear as the last segment of the route URL");
				
				int catchAlls = 0;
				string part = parts [i];
				int partLength = part.Length;
				var tokens = new List <PatternToken> ();

				if (partLength == 0 && i < partsCount - 1)
					throw new ArgumentException ("Consecutive URL segment separators '/' are not allowed");

				if (part.IndexOf ("{}") != -1)
					throw new ArgumentException ("Empty URL parameter name is not allowed");

				if (i > 0)
					allTokens.Add (null);
				
				if (part.IndexOfAny (placeholderDelimiters) == -1) {
					// no placeholders here, short-circuit it
					tmpToken = new PatternToken (PatternTokenType.Literal, part);
					tokens.Add (tmpToken);
					allTokens.Add (tmpToken);
					segments [i].AllLiteral = true;
					segments [i].Tokens = tokens;
					continue;
				}

				string tmp;
				int from = 0, start;
				bool allLiteral = true;
				while (from < partLength) {
					start = part.IndexOf ('{', from);
					if (start >= partLength - 2)
						throw new ArgumentException ("Unterminated URL parameter. It must contain matching '}'");

					if (start < 0) {
						if (part.IndexOf ('}', from) >= from)
							throw new ArgumentException ("Unmatched URL parameter closer '}'. A corresponding '{' must precede");
						tmp = part.Substring (from);
						tmpToken = new PatternToken (PatternTokenType.Literal, tmp);
						tokens.Add (tmpToken);
						allTokens.Add (tmpToken);
						from += tmp.Length;
						break;
					}

					if (from == 0 && start > 0) {
						tmpToken = new PatternToken (PatternTokenType.Literal, part.Substring (0, start));
						tokens.Add (tmpToken);
						allTokens.Add (tmpToken);
					}
					
					int end = part.IndexOf ('}', start + 1);
					int next = part.IndexOf ('{', start + 1);
					
					if (end < 0 || next >= 0 && next < end)
						throw new ArgumentException ("Unterminated URL parameter. It must contain matching '}'");
					if (next == end + 1)
						throw new ArgumentException ("Two consecutive URL parameters are not allowed. Split into a different segment by '/', or a literal string.");

					if (next == -1)
						next = partLength;
					
					string token = part.Substring (start + 1, end - start - 1);
					PatternTokenType type;
					if (token [0] == '*') {
						catchAlls++;
						haveSegmentWithCatchAll = true;
						type = PatternTokenType.CatchAll;
						token = token.Substring (1);
					} else
						type = PatternTokenType.Standard;

					if (!parameterNames.ContainsKey (token))
						parameterNames.Add (token, true);

					tmpToken = new PatternToken (type, token);
					tokens.Add (tmpToken);
					allTokens.Add (tmpToken);
					allLiteral = false;
					
					if (end < partLength - 1) {
						token = part.Substring (end + 1, next - end - 1);
						tmpToken = new PatternToken (PatternTokenType.Literal, token);
						tokens.Add (tmpToken);
						allTokens.Add (tmpToken);
						end += token.Length;
					}

					if (catchAlls > 1 || (catchAlls == 1 && tokens.Count > 1))
						throw new ArgumentException ("A path segment that contains more than one section, such as a literal section or a parameter, cannot contain a catch-all parameter.");
					from = end + 1;
				}
				
				segments [i].AllLiteral = allLiteral;
				segments [i].Tokens = tokens;
			}

			if (allTokens.Count > 0)
				this.tokens = allTokens.ToArray ();
			allTokens = null;
		}

		RouteValueDictionary AddDefaults (RouteValueDictionary dict, RouteValueDictionary defaults)
		{
			if (defaults != null && defaults.Count > 0) {
				string key;
				foreach (var def in defaults) {
					key = def.Key;
					if (dict.ContainsKey (key))
						continue;
					dict.Add (key, def.Value);
				}
			}

			return dict;
		}
		
		public RouteValueDictionary Match (string path, RouteValueDictionary defaults)
		{
			var ret = new RouteValueDictionary ();
			string url = Url;
			string [] argSegs;
			int argsCount;
			
			if (String.IsNullOrEmpty (path)) {
				argSegs = null;
				argsCount = 0;
			} else {
				// quick check
				if (String.Compare (url, path, StringComparison.Ordinal) == 0 && url.IndexOf ('{') < 0)
					return AddDefaults (ret, defaults);

				argSegs = path.Split ('/');
				argsCount = argSegs.Length;
			}
			
			bool haveDefaults = defaults != null && defaults.Count > 0;

			if (argsCount == 1 && String.IsNullOrEmpty (argSegs [0]))
				argsCount = 0;
			
			if (!haveDefaults && ((haveSegmentWithCatchAll && argsCount < segmentCount) || (!haveSegmentWithCatchAll && argsCount != segmentCount)))
				return null;

			int i = 0;

			foreach (PatternSegment segment in segments) {
				if (i >= argsCount)
					break;
				
				if (segment.AllLiteral) {
					if (String.Compare (argSegs [i], segment.Tokens [0].Name, StringComparison.OrdinalIgnoreCase) != 0)
						return null;
					i++;
					continue;
				}

				string pathSegment = argSegs [i];
				int pathSegmentLength = pathSegment != null ? pathSegment.Length : -1;
				int pathIndex = 0;
				PatternTokenType tokenType;
				List <PatternToken> tokens = segment.Tokens;
				int tokensCount = tokens.Count;
				
				// Process the path segments ignoring the defaults
				for (int tokenIndex = 0; tokenIndex < tokensCount; tokenIndex++) {
					var token = tokens [tokenIndex];
					if (pathIndex > pathSegmentLength - 1)
						return null;

					tokenType = token.Type;
					var tokenName = token.Name;

					// Catch-all
					if (i > segmentCount - 1 || tokenType == PatternTokenType.CatchAll) {
						if (tokenType != PatternTokenType.CatchAll)
							return null;

						StringBuilder sb = new StringBuilder ();
						for (int j = i; j < argsCount; j++) {
							if (j > i)
								sb.Append ('/');
							sb.Append (argSegs [j]);
						}
						
						ret.Add (tokenName, sb.ToString ());
						break;
					}

					// Literal sections
					if (token.Type == PatternTokenType.Literal) {
						int nameLen = tokenName.Length;
						if (pathSegmentLength < nameLen || String.Compare (pathSegment, pathIndex, tokenName, 0, nameLen, StringComparison.OrdinalIgnoreCase) != 0)
							return null;
						pathIndex += nameLen;
						continue;
					}

					int nextTokenIndex = tokenIndex + 1;
					if (nextTokenIndex >= tokensCount) {
						// Last token
						ret.Add (tokenName, pathSegment.Substring (pathIndex));
						continue;
					}

					// Next token is a literal - greedy matching. It seems .NET
					// uses a simple and naive algorithm here which finds the
					// last ocurrence of the next section literal and assigns
					// everything before that to this token. See the
					// GetRouteData28 test in RouteTest.cs
					var nextToken = tokens [nextTokenIndex];
					string nextTokenName = nextToken.Name;
					int lastIndex = pathSegment.LastIndexOf (nextTokenName, pathSegmentLength - 1, pathSegmentLength - pathIndex, StringComparison.OrdinalIgnoreCase);
					if (lastIndex == -1)
						return null;
					
					int copyLength = lastIndex - pathIndex;
					string sectionValue = pathSegment.Substring (pathIndex, copyLength);
					if (String.IsNullOrEmpty (sectionValue))
						return null;
					
					ret.Add (tokenName, sectionValue);
					pathIndex += copyLength;
				}
				i++;
			}

			// Check the remaining segments, if any, and see if they are required
			//
			// If a segment has more than one section (i.e. there's at least one
			// literal, then it cannot match defaults
			//
			// All of the remaining segments must have all defaults provided and they
			// must not be literals or the match will fail.
			if (i < segmentCount) {
				if (!haveDefaults)
					return null;
				
				for (;i < segmentCount; i++) {
					var segment = segments [i];
					if (segment.AllLiteral)
						return null;
					
					var tokens = segment.Tokens;
					if (tokens.Count != 1)
						return null;

					if (!defaults.ContainsKey (tokens [0].Name))
						return null;
				}
			}
			
			return AddDefaults (ret, defaults);
		}
		
		public bool BuildUrl (Route route, RequestContext requestContext, RouteValueDictionary userValues, out string value)
		{
			value = null;
			if (requestContext == null)
				return false;

			RouteData routeData = requestContext.RouteData;
			RouteValueDictionary defaultValues = route != null ? route.Defaults : null;
			RouteValueDictionary ambientValues = routeData.Values;

			if (defaultValues != null && defaultValues.Count == 0)
				defaultValues = null;
			if (ambientValues != null && ambientValues.Count == 0)
				ambientValues = null;
			if (userValues != null && userValues.Count == 0)
				userValues = null;

			// Check URL parameters
			// It is allowed to take ambient values for required parameters if:
			//
			//   - there are no default values provided
			//   - the default values dictionary contains at least one required
			//     parameter value
			//
			bool canTakeFromAmbient;
			if (defaultValues == null)
				canTakeFromAmbient = true;
			else {
				canTakeFromAmbient = false;
				foreach (KeyValuePair <string, bool> de in parameterNames) {
					if (defaultValues.ContainsKey (de.Key)) {
						canTakeFromAmbient = true;
						break;
					}
				}
			}
			
			bool allMustBeInUserValues = false;
			foreach (KeyValuePair <string, bool> de in parameterNames) {
				string parameterName = de.Key;
				// Is the parameter required?
				if (defaultValues == null || !defaultValues.ContainsKey (parameterName)) {
					// Yes, it is required (no value in defaults)
					// Has the user provided value for it?
					if (userValues == null || !userValues.ContainsKey (parameterName)) {
						if (allMustBeInUserValues)
							return false; // partial override => no match
						
						if (!canTakeFromAmbient || ambientValues == null || !ambientValues.ContainsKey (parameterName))
							return false; // no value provided => no match
					} else if (canTakeFromAmbient)
						allMustBeInUserValues = true;
				}
			}

			// Check for non-url parameters
			if (defaultValues != null) {
				foreach (var de in defaultValues) {
					string parameterName = de.Key;
					
					if (parameterNames.ContainsKey (parameterName))
						continue;

					object parameterValue = null;
					// Has the user specified value for this parameter and, if
					// yes, is it the same as the one in defaults?
					if (userValues != null && userValues.TryGetValue (parameterName, out parameterValue)) {
						object defaultValue = de.Value;
						if (defaultValue is string && parameterValue is string) {
							if (String.Compare ((string)defaultValue, (string)parameterValue, StringComparison.Ordinal) != 0)
								return false; // different value => no match
						} else if (defaultValue != parameterValue)
							return false; // different value => no match
					}
				}
			}

			// Check the constraints
			RouteValueDictionary constraints = route != null ? route.Constraints : null;
			if (constraints != null && constraints.Count > 0) {
				HttpContextBase context = requestContext.HttpContext;
				bool invalidConstraint;
				
				foreach (var de in constraints) {
					if (!Route.ProcessConstraintInternal (context, route, de.Value, de.Key, userValues, RouteDirection.UrlGeneration, out invalidConstraint) ||
					    invalidConstraint)
						return false; // constraint not met => no match
				}
			}

			// We're a match, generate the URL
			var ret = new StringBuilder ();
			bool canTrim = true;
			
			// Going in reverse order, so that we can trim without much ado
			int tokensCount = tokens.Length - 1;
			for (int i = tokensCount; i >= 0; i--) {
				PatternToken token = tokens [i];
				if (token == null) {
					if (i < tokensCount && ret.Length > 0 && ret [0] != '/')
						ret.Insert (0, '/');
					continue;
				}
				
				if (token.Type == PatternTokenType.Literal) {
					ret.Insert (0, token.Name);
					continue;
				}

				string parameterName = token.Name;
				object tokenValue;

#if SYSTEMCORE_DEP
				if (userValues.GetValue (parameterName, out tokenValue)) {
					if (!defaultValues.Has (parameterName, tokenValue)) {
						canTrim = false;
						if (tokenValue != null)
							ret.Insert (0, tokenValue.ToString ());
						continue;
					}

					if (!canTrim && tokenValue != null)
						ret.Insert (0, tokenValue.ToString ());
					continue;
				}

				if (defaultValues.GetValue (parameterName, out tokenValue)) {
					object ambientTokenValue;
					if (ambientValues.GetValue (parameterName, out ambientTokenValue))
						tokenValue = ambientTokenValue;
					
					if (!canTrim && tokenValue != null)
						ret.Insert (0, tokenValue.ToString ());
					continue;
				}

				canTrim = false;
				if (ambientValues.GetValue (parameterName, out tokenValue)) {
					if (tokenValue != null)
						ret.Insert (0, tokenValue.ToString ());
					continue;
				}
#endif
			}

			// All the values specified in userValues that aren't part of the original
			// URL, the constraints or defaults collections are treated as overflow
			// values - they are appended as query parameters to the URL
			if (userValues != null) {
				bool first = true;
				foreach (var de in userValues) {
					string parameterName = de.Key;

#if SYSTEMCORE_DEP
					if (parameterNames.ContainsKey (parameterName) || defaultValues.Has (parameterName) || constraints.Has (parameterName))
						continue;
#endif

					object parameterValue = de.Value;
					if (parameterValue == null)
						continue;

					var parameterValueAsString = parameterValue as string;
					if (parameterValueAsString != null && parameterValueAsString.Length == 0)
						continue;
					
					if (first) {
						ret.Append ('?');
						first = false;
					} else
						ret.Append ('&');

					
					ret.Append (Uri.EscapeDataString (parameterName));
					ret.Append ('=');
					if (parameterValue != null)
						ret.Append (Uri.EscapeDataString (de.Value.ToString ()));
				}
			}
			
			value = ret.ToString ();
			return true;
		}
	}
}

