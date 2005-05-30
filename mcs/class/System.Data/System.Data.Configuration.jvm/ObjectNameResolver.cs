// 
// System.Data.Configuration.ObjectNameResolver.cs
//
// Authors:
//	Konstantin Triger <kostat@mainsoft.com>
//	
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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

using System.Collections;
using System.Configuration;
using System.Xml;
using System.Text.RegularExpressions;

namespace System.Data.Configuration {
	sealed class ObjectNameResolver : IComparable {
		string _dbname;
		int _priority;
		Regex _regex;
		
		public ObjectNameResolver(string dbname, string match, int priority) {
			_dbname = dbname;
			_regex = new Regex(match, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
			
			_priority = priority;
		}

		public ObjectNameResolver(Regex regex) {
			_regex = regex;
			_dbname = null;
			_priority = int.MaxValue;
		}

		public string DbName {
			get {
				return _dbname;
			}
		}

		public static string GetCatalog(Match match) {
			return GetCapture(match, "CATALOG");
		}

		public static string GetSchema(Match match) {
			return GetCapture(match, "SCHEMA");
		}

		public static string GetName(Match match) {
			return GetCapture(match, "NAME");
		}

		public Match Match(string expression) {
			return _regex.Match(expression.Trim());
		}

		static string GetCapture(Match match, string captureName) {
			Group g = match.Groups[captureName];
			if (!g.Success)
				return String.Empty;

			return g.Value.Trim();
		}

		#region IComparable Members

		public int CompareTo(object obj) {
			// TODO:  Add ObjectNameResolver.CompareTo implementation
			return _priority.CompareTo(((ObjectNameResolver)obj)._priority);
		}

		#endregion

	}
}