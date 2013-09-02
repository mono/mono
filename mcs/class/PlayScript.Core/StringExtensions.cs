// Copyright 2013 Zynga Inc.
//	
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//		
//      Unless required by applicable law or agreed to in writing, software
//      distributed under the License is distributed on an "AS IS" BASIS,
//      WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//      See the License for the specific language governing permissions and
//      limitations under the License.

using System;
using System.Text;

namespace _root
{
	public static class StringExtensions
	{
		public static int get_length(this String s) {
			return s.Length;
		}

		public static String charAt (this String s, double index = 0) {
			return s[ (int)index ].ToString();
		}

		public static int charCodeAt(this String s, double index) {
			return s[(int)index];
		}

		public static String concat(this String s, params object[] args) {
			foreach (object arg in args)
			{
				s += arg.ToString();
			}
			return s;
		}

		private static char objectToChar(object o)
		{
			if (o is int) {
				return (char)(int)o;
			} else if (o is uint) {
				return (char)(uint)o;
			} else if (o is char) {
				return (char)o;
			} else {
				throw new NotImplementedException();
			}
		}

		public static String fromCharCode (params object[] charCodes)
		{
			if (charCodes.Length == 1)
			{
				return new String(objectToChar(charCodes[0]), 1);
			}
			else
			{
				var chars = new char[charCodes.Length];
				for (int i=0; i < charCodes.Length; i++) {
					chars[i] = objectToChar(charCodes[i]);
				}
				return new String(chars);
			}
		}

		public static int indexOf(this String s, String val, double startIndex = 0) {
			if (s == null) return -1;
			return s.IndexOf(val, (int)startIndex);
		}
						
		public static int lastIndexOf(this String s, String val, double startIndex = 0x7FFFFFFF) {
			throw new NotImplementedException();
		}
						
		public static int localeCompare(this String s, String other, params object[] values) {
			throw new NotImplementedException();
		}

		public static Array match(this String s, object pattern) {
			if (pattern is RegExp) {
				// pattern is a regexp
				var re = pattern as RegExp;
				return re.match(s);
			} else {
				// pattern is a String or other object
				throw new NotImplementedException();
			}
		}

		public static String replace (this String s, object pattern, object repl)
		{
			if (pattern is RegExp) {
				// pattern is a regexp
				var re = pattern as RegExp;
				return re.replace(s, repl.ToString());
			} else {
				// pattern is a String or other object
				return s.Replace(pattern.ToString (), repl.ToString());
			}
		}

		public static int search(this String s, object pattern) {
			if (pattern is RegExp) {
				// pattern is a regexp
				var re = pattern as RegExp;
				return re.search(s);
			} else {
				// pattern is a String or other object
				return s.IndexOf(pattern.ToString ());
			}
		}

		public static String slice(this String s) {
			throw new NotImplementedException();
		}

		public static String slice(this String s, int startIndex) {
			return s.Substring(startIndex);
		}

		public static String slice(this String s, int startIndex, int endIndex) {
			return s.Substring(startIndex, endIndex - startIndex);
		}

		public static Array split (this String s, object delimiter, int limit = 0x7fffffff)
		{
			if (limit != 0x7fffffff) {
				throw new NotImplementedException ();
			}

			if (delimiter is RegExp) {
				var re = delimiter as RegExp;
				return re.split(s);
			} else if (delimiter is String) {
				return new Array( s.Split(new String[] {(String)delimiter}, StringSplitOptions.None ));
			} else {
				throw new NotImplementedException ();
			}
		}

		public static String substr (this String s, double startIndex = 0, double len = 0x7fffffff) {
			if (len == 0x7fffffff) {
				return s.Substring((int)startIndex);
			} else {
				// TODO: should this throw or be silent if length exceeded?
				return s.Substring((int)startIndex, (int)len);
			}
		}

		public static String substring (this String s, double startIndex = 0, double endIndex = 0x7fffffff) {
			if (endIndex == 0x7fffffff) {
				return s.Substring((int)startIndex);
			} else {
				// TODO: should this throw or be silent if length exceeded?
				return s.Substring((int)startIndex, (int)endIndex - (int)startIndex);
			}
		}

		public static String toLocaleLowerCase(this String s) {
			throw new NotImplementedException();
		}

		public static String toLocaleUpperCase(this String s) {
			throw new NotImplementedException();
		}

		public static String toLowerCase(this String s) {
			return s.ToLowerInvariant();
		}

		public static String toUpperCase(this String s) {
			return s.ToUpperInvariant();
		}

		public static String valueOf(this String s) {
			return s;
		}

	}
}

