//
// PatternParser.cs
//
// Author:
//      Atsushi Enomoto <atsushi@ximian.com>
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2008-2010 Novell Inc. http://novell.com
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
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Util;
using System.Diagnostics;
using System.Globalization;

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

		static bool ParametersAreEqual (object a, object b)
		{
			if (a is string && b is string) {
				return String.Equals (a as string, b as string, StringComparison.OrdinalIgnoreCase);
			} else {
				// Parameter may be a boxed value type, need to use .Equals() for comparison
				return object.Equals (a, b);
			}
		}

		static bool ParameterIsNonEmpty (object param)
		{
			if (param is string)
				return !string.IsNullOrEmpty (param as string);

			return param != null;
		}

		bool IsParameterRequired (string parameterName, RouteValueDictionary defaultValues, out object defaultValue)
		{
			foreach (var token in tokens) {
				if (token == null)
					continue;

				if (string.Equals (token.Name, parameterName, StringComparison.OrdinalIgnoreCase)) {
					if (token.Type == PatternTokenType.CatchAll) {
						defaultValue = null;
						return false;
					}
				}
			}

			if (defaultValues == null)
				throw new ArgumentNullException ("defaultValues is null?!");

			return !defaultValues.TryGetValue (parameterName, out defaultValue);
		}

		static string EscapeReservedCharacters (Match m)
		{
			if (m == null)
				throw new ArgumentNullException("m");

			return Uri.HexEscape (m.Value[0]);
		}

		static string UriEncode (string str)
		{
			if (string.IsNullOrEmpty (str))
				return str;

			string escape = Uri.EscapeUriString (str);
			return Regex.Replace (escape, "([#?])", new MatchEvaluator (EscapeReservedCharacters));
		}

		bool MatchSegment (int segIndex, int argsCount, string[] argSegs, List <PatternToken> tokens, RouteValueDictionary ret)
		{
			string pathSegment = argSegs [segIndex];
			int pathSegmentLength = pathSegment != null ? pathSegment.Length : -1;
			int startIndex = pathSegmentLength - 1;
			PatternTokenType tokenType;
			int tokensCount = tokens.Count;
			PatternToken token;
			string tokenName;

			for (int tokenIndex = tokensCount - 1; tokenIndex > -1; tokenIndex--) {
				token = tokens [tokenIndex];
				if (startIndex < 0)
					return false;

				tokenType = token.Type;
				tokenName = token.Name;

				if (segIndex > segmentCount - 1 || tokenType == PatternTokenType.CatchAll) {
					var sb = new StringBuilder ();

					for (int j = segIndex; j < argsCount; j++) {
						if (j > segIndex)
							sb.Append ('/');
						sb.Append (argSegs [j]);
					}
						
					ret.Add (tokenName, sb.ToString ());
					break;
				}

				int scanIndex;
				if (token.Type == PatternTokenType.Literal) {
					int nameLen = tokenName.Length;
					if (startIndex + 1 < nameLen)
						return false;
					scanIndex = startIndex - nameLen + 1;
					if (String.Compare (pathSegment, scanIndex, tokenName, 0, nameLen, StringComparison.OrdinalIgnoreCase) != 0)
						return false;
					startIndex = scanIndex - 1;
					continue;
				}

				// Standard token
				int nextTokenIndex = tokenIndex - 1;
				if (nextTokenIndex < 0) {
					// First token
					ret.Add (tokenName, pathSegment.Substring (0, startIndex + 1));
					continue;
				}

				if (startIndex == 0)
					return false;
				
				var nextToken = tokens [nextTokenIndex];
				string nextTokenName = nextToken.Name;

				// Skip one char, since there can be no empty segments and if the
				// current token's value happens to be the same as preceeding
				// literal text, we'll save some time and complexity.
				scanIndex = startIndex - 1;
				int lastIndex = pathSegment.LastIndexOf (nextTokenName, scanIndex, StringComparison.OrdinalIgnoreCase);
				if (lastIndex == -1)
					return false;

				lastIndex += nextTokenName.Length - 1;
						
				string sectionValue = pathSegment.Substring (lastIndex + 1, startIndex - lastIndex);
				if (String.IsNullOrEmpty (sectionValue))
					return false;

				ret.Add (tokenName, sectionValue);
				startIndex = lastIndex;
			}
			
			return true;
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

				if (String.IsNullOrEmpty (argSegs [argsCount - 1]))
					argsCount--; // path ends with a trailinig '/'
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

				if (!MatchSegment (i, argsCount, argSegs, segment.Tokens, ret))
					return null;
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

					// if token is catch-all, we're done.
					if (tokens [0].Type == PatternTokenType.CatchAll)
						break;

					if (!defaults.ContainsKey (tokens [0].Name))
						return null;
				}
			} else if (!haveSegmentWithCatchAll && argsCount > segmentCount)
				return null;
			
			return AddDefaults (ret, defaults);
		}
		
		public string BuildUrl (Route route, RequestContext requestContext, RouteValueDictionary userValues, RouteValueDictionary constraints, out RouteValueDictionary usedValues)
		{
			usedValues = null;

			if (requestContext == null)
				return null;

			RouteData routeData = requestContext.RouteData;
			var currentValues = routeData.Values ?? new RouteValueDictionary ();
			var values = userValues ?? new RouteValueDictionary ();
			var defaultValues = (route != null ? route.Defaults : null) ?? new RouteValueDictionary ();

			// The set of values we should be using when generating the URL in this route
			var acceptedValues = new RouteValueDictionary ();

			// Keep track of which new values have been used
			HashSet<string> unusedNewValues = new HashSet<string> (values.Keys, StringComparer.OrdinalIgnoreCase);

			// This route building logic is based on System.Web.Http's Routing code (which is Apache Licensed by MS)
			// and which can be found at mono's external/aspnetwebstack/src/System.Web.Http/Routing/HttpParsedRoute.cs
			// Hopefully this will ensure a much higher compatiblity with MS.NET's System.Web.Routing logic. (pruiz)

			#region Step 1: Get the list of values we're going to use to match and generate this URL
			// Find out which entries in the URL are valid for the URL we want to generate.
			// If the URL had ordered parameters a="1", b="2", c="3" and the new values
			// specified that b="9", then we need to invalidate everything after it. The new
			// values should then be a="1", b="9", c=<no value>.
			foreach (var item in parameterNames) {
				var parameterName = item.Key;

				object newParameterValue;
				bool hasNewParameterValue = values.TryGetValue (parameterName, out newParameterValue);
				if (hasNewParameterValue) {
					unusedNewValues.Remove(parameterName);
				}

				object currentParameterValue;
				bool hasCurrentParameterValue = currentValues.TryGetValue (parameterName, out currentParameterValue);

				if (hasNewParameterValue && hasCurrentParameterValue) {
					if (!ParametersAreEqual (currentParameterValue, newParameterValue)) {
						// Stop copying current values when we find one that doesn't match
						break;
					}
				}

				// If the parameter is a match, add it to the list of values we will use for URL generation
				if (hasNewParameterValue) {
					if (ParameterIsNonEmpty (newParameterValue)) {
						acceptedValues.Add (parameterName, newParameterValue);
					}
				}
				else {
					if (hasCurrentParameterValue) {
						acceptedValues.Add (parameterName, currentParameterValue);
					}
				}
			}

			// Add all remaining new values to the list of values we will use for URL generation
			foreach (var newValue in values) {
				if (ParameterIsNonEmpty (newValue.Value) && !acceptedValues.ContainsKey (newValue.Key)) {
					acceptedValues.Add (newValue.Key, newValue.Value);
				}
			}

			// Add all current values that aren't in the URL at all
			foreach (var currentValue in currentValues) {
				if (!acceptedValues.ContainsKey (currentValue.Key) && !parameterNames.ContainsKey (currentValue.Key)) {
					acceptedValues.Add (currentValue.Key, currentValue.Value);
				}
			}

			// Add all remaining default values from the route to the list of values we will use for URL generation
			foreach (var item in parameterNames) {
				object defaultValue;
				if (!acceptedValues.ContainsKey (item.Key) && !IsParameterRequired (item.Key, defaultValues, out defaultValue)) {
					// Add the default value only if there isn't already a new value for it and
					// only if it actually has a default value, which we determine based on whether
					// the parameter value is required.
					acceptedValues.Add (item.Key, defaultValue);
				}
			}

			// All required parameters in this URL must have values from somewhere (i.e. the accepted values)
			foreach (var item in parameterNames) {
				object defaultValue;
				if (IsParameterRequired (item.Key, defaultValues, out defaultValue) && !acceptedValues.ContainsKey (item.Key)) {
					// If the route parameter value is required that means there's
					// no default value, so if there wasn't a new value for it
					// either, this route won't match.
					return null;
				}
			}

			// All other default values must match if they are explicitly defined in the new values
			var otherDefaultValues = new RouteValueDictionary (defaultValues);
			foreach (var item in parameterNames) {
				otherDefaultValues.Remove (item.Key);
			}

			foreach (var defaultValue in otherDefaultValues) {
				object value;
				if (values.TryGetValue (defaultValue.Key, out value)) {
					unusedNewValues.Remove (defaultValue.Key);
					if (!ParametersAreEqual (value, defaultValue.Value)) {
						// If there is a non-parameterized value in the route and there is a
						// new value for it and it doesn't match, this route won't match.
						return null;
					}
				}
			}
			#endregion

			#region Step 2: If the route is a match generate the appropriate URL

			var uri = new StringBuilder ();
			var pendingParts = new StringBuilder ();
			var pendingPartsAreAllSafe = false;
			bool blockAllUriAppends = false;
			var allSegments = new List<PatternSegment?> ();

			// Build a list of segments plus separators we can use as template.
			foreach (var segment in segments) {
				if (allSegments.Count > 0)
					allSegments.Add (null); // separator exposed as null.
				allSegments.Add (segment);
			}

			// Finally loop thru al segment-templates building the actual uri.
			foreach (var item in allSegments) {
				var segment = item.GetValueOrDefault ();

				// If segment is a separator..
				if (item == null) {
					if (pendingPartsAreAllSafe) {
						// Accept
						if (pendingParts.Length > 0) {
							if (blockAllUriAppends)
								return null;

							// Append any pending literals to the URL
							uri.Append (pendingParts.ToString ());
							pendingParts.Length = 0;
						}
					}
					pendingPartsAreAllSafe = false;

					// Guard against appending multiple separators for empty segments
					if (pendingParts.Length > 0 && pendingParts[pendingParts.Length - 1] == '/') {
						// Dev10 676725: Route should not be matched if that causes mismatched tokens
						// Dev11 86819: We will allow empty matches if all subsequent segments are null
						if (blockAllUriAppends)
							return null;

						// Append any pending literals to the URI (without the trailing slash) and prevent any future appends
						uri.Append(pendingParts.ToString (0, pendingParts.Length - 1));
						pendingParts.Length = 0;
					} else {
						pendingParts.Append ("/");
					}
#if false
				} else if (segment.AllLiteral) {
					// Spezial (optimized) case: all elements of segment are literals.
					pendingPartsAreAllSafe = true;
					foreach (var tk in segment.Tokens)
						pendingParts.Append (tk.Name);
#endif
				} else {
					// Segments are treated as all-or-none. We should never output a partial segment.
					// If we add any subsegment of this segment to the generated URL, we have to add
					// the complete match. For example, if the subsegment is "{p1}-{p2}.xml" and we
					// used a value for {p1}, we have to output the entire segment up to the next "/".
					// Otherwise we could end up with the partial segment "v1" instead of the entire
					// segment "v1-v2.xml".
					bool addedAnySubsegments = false;

					foreach (var token in segment.Tokens) {
						if (token.Type == PatternTokenType.Literal) {
							// If it's a literal we hold on to it until we are sure we need to add it
							pendingPartsAreAllSafe = true;
							pendingParts.Append (token.Name);
						} else {
							if (token.Type == PatternTokenType.Standard) {
								if (pendingPartsAreAllSafe) {
									// Accept
									if (pendingParts.Length > 0) {
										if (blockAllUriAppends)
											return null;

										// Append any pending literals to the URL
										uri.Append (pendingParts.ToString ());
										pendingParts.Length = 0;

										addedAnySubsegments = true;
									}
								}
								pendingPartsAreAllSafe = false;

								// If it's a parameter, get its value
								object acceptedParameterValue;
								bool hasAcceptedParameterValue = acceptedValues.TryGetValue (token.Name, out acceptedParameterValue);
								if (hasAcceptedParameterValue)
									unusedNewValues.Remove (token.Name);

								object defaultParameterValue;
								defaultValues.TryGetValue (token.Name, out defaultParameterValue);

								if (ParametersAreEqual (acceptedParameterValue, defaultParameterValue)) {
									// If the accepted value is the same as the default value, mark it as pending since
									// we won't necessarily add it to the URL we generate.
									pendingParts.Append (Convert.ToString (acceptedParameterValue, CultureInfo.InvariantCulture));
								} else {
									if (blockAllUriAppends)
										return null;

									// Add the new part to the URL as well as any pending parts
									if (pendingParts.Length > 0) {
										// Append any pending literals to the URL
										uri.Append (pendingParts.ToString ());
										pendingParts.Length = 0;
									}
									uri.Append (Convert.ToString (acceptedParameterValue, CultureInfo.InvariantCulture));

									addedAnySubsegments = true;
								}
							} else {
								Debug.Fail ("Invalid path subsegment type");
							}
						}
					}

					if (addedAnySubsegments) {
						// See comment above about why we add the pending parts
						if (pendingParts.Length > 0) {
							if (blockAllUriAppends)
								return null;

							// Append any pending literals to the URL
							uri.Append (pendingParts.ToString ());
							pendingParts.Length = 0;
						}
					}
				}
			}

			if (pendingPartsAreAllSafe) {
				// Accept
				if (pendingParts.Length > 0) {
					if (blockAllUriAppends)
						return null;

					// Append any pending literals to the URI
					uri.Append (pendingParts.ToString ());
				}
			}

			// Process constraints keys
			if (constraints != null) {
				// If there are any constraints, mark all the keys as being used so that we don't
				// generate query string items for custom constraints that don't appear as parameters
				// in the URI format.
				foreach (var constraintsItem in constraints) {
					unusedNewValues.Remove (constraintsItem.Key);
				}
			}

			// Encode the URI before we append the query string, otherwise we would double encode the query string
			var encodedUri = new StringBuilder ();
			encodedUri.Append (UriEncode (uri.ToString ()));
			uri = encodedUri;

			// Add remaining new values as query string parameters to the URI
			if (unusedNewValues.Count > 0) {
				// Generate the query string
				bool firstParam = true;
				foreach (string unusedNewValue in unusedNewValues) {
					object value;
					if (acceptedValues.TryGetValue (unusedNewValue, out value)) {
						uri.Append (firstParam ? '?' : '&');
						firstParam = false;
						uri.Append (Uri.EscapeDataString (unusedNewValue));
						uri.Append ('=');
						uri.Append (Uri.EscapeDataString (Convert.ToString (value, CultureInfo.InvariantCulture)));
					}
				}
			}

			#endregion

			usedValues = acceptedValues;
			return uri.ToString();
		}
	}
}

