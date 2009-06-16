//
// PatternParser.cs
//
// Author:
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2009 Novell Inc. http://novell.com
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
#if SYSTEMCORE_DEP
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Web.Routing
{
	static class RouteValueDictionaryExtensions
	{
		public static bool Has (this RouteValueDictionary dict, string key)
		{
			if (dict == null)
				return false;
			
			return dict.ContainsKey (key);
		}

		public static bool Has (this RouteValueDictionary dict, string key, object value)
		{
			if (dict == null)
				return false;

			object entryValue;
			if (dict.TryGetValue (key, out entryValue)) {
				if (value is string) {
					if (!(entryValue is string))
						return false;
					
					string s1 = value as string;
					string s2 = entryValue as string;
					return String.Compare (s1, s2, StringComparison.OrdinalIgnoreCase) == 0;
				}
				
				return entryValue == null ? value == null : entryValue.Equals (value);
			}
			
			return false;
		}

		public static bool GetValue (this RouteValueDictionary dict, string key, out object value)
		{
			if (dict == null) {
				value = null;
				return false;
			}

			return dict.TryGetValue (key, out value);
		}

		[Conditional ("DEBUG")]
		public static void Dump (this RouteValueDictionary dict, string name, string indent)
		{
			if (indent == null)
				indent = String.Empty;
			
			if (dict == null) {
				Console.WriteLine (indent + "Dictionary '{0}' is null", name);
				return;
			}
			
			if (dict.Count == 0) {
				Console.WriteLine (indent + "Dictionary '{0}' is empty", name);
				return;
			}

			Console.WriteLine (indent + "Dictionary '{0}':", name);
			foreach (var de in dict)
				Console.WriteLine (indent + "\t'{0}' == {1}", de.Key, de.Value);
		}
	}
}
#endif