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