//
// assembly:	System
// namespace:	System.Text.RegularExpressions
// file:	regex.cs
//
// author:	Dan Lewis (dlewis@gmx.co.uk)
// 		(c) 2002

using System;
using System.Text;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;

using RegularExpression = System.Text.RegularExpressions.Syntax.RegularExpression;
using Parser = System.Text.RegularExpressions.Syntax.Parser;

namespace System.Text.RegularExpressions {
	
	public delegate string MatchEvaluator (Match match);

	[Flags]
	public enum RegexOptions {
		None				= 0x000,
		IgnoreCase			= 0x001,
		Multiline			= 0x002,
		ExplicitCapture			= 0x004,
		Compiled			= 0x008,
		Singleline			= 0x010,
		IgnorePatternWhitespace		= 0x020,
		RightToLeft			= 0x040,
		ECMAScript			= 0x100
	}
	
	[Serializable]
	public class Regex : ISerializable {
		public static void CompileToAssembly
			(RegexCompilationInfo[] regexes, AssemblyName aname)
		{
			throw new Exception ("Not implemented.");
		}

		public static void CompileToAssembly
			(RegexCompilationInfo[] regexes, AssemblyName aname,
			 CustomAttributeBuilder[] attribs)
		{
			throw new Exception ("Not implemented.");
		}

		public static void CompileToAssembly
			(RegexCompilationInfo[] regexes, AssemblyName aname,
			 CustomAttributeBuilder[] attribs, string resourceFile)
		{
			throw new Exception ("Not implemented.");
		}
		
		public static string Escape (string str) {
			return Parser.Escape (str);
		}

		public static string Unescape (string str) {
			return Parser.Unescape (str);
		}

		public static bool IsMatch (string input, string pattern) {
			return IsMatch (input, pattern, RegexOptions.None);
		}

		public static bool IsMatch (string input, string pattern, RegexOptions options) {
			Regex re = new Regex (pattern, options);
			return re.IsMatch (input);
		}

		public static Match Match (string input, string pattern) {
			return Regex.Match (input, pattern, RegexOptions.None);
		}

		public static Match Match (string input, string pattern, RegexOptions options) {
			Regex re = new Regex (pattern, options);
			return re.Match (input);
		}

		public static MatchCollection Matches (string input, string pattern) {
			return Matches (input, pattern, RegexOptions.None);
		}

		public static MatchCollection Matches (string input, string pattern, RegexOptions options) {
			Regex re = new Regex (pattern, options);
			return re.Matches (input);
		}

		public static string Replace
			(string input, string pattern, MatchEvaluator evaluator)
		{
			return Regex.Replace (input, pattern, evaluator, RegexOptions.None);
		}

		public static string Replace
			(string input, string pattern, MatchEvaluator evaluator,
			 RegexOptions options)
		{
			Regex re = new Regex (pattern, options);
			return re.Replace (input, evaluator);
		}

		public static string Replace
			(string input, string pattern, string replacement)
		{
			return Regex.Replace (input, pattern, replacement, RegexOptions.None);
		}

		public static string Replace
			(string input, string pattern, string replacement,
			 RegexOptions options)
		{
			Regex re = new Regex (pattern, options);
			return re.Replace (input, replacement);
		}

		public static string[] Split (string input, string pattern) {
			return Regex.Split (input, pattern, RegexOptions.None);
		}

		public static string[] Split (string input, string pattern, RegexOptions options) {
			Regex re = new Regex (pattern, options);
			return re.Split (input);
		}

		// private

		private static FactoryCache cache = new FactoryCache (200);	// TODO put some meaningful number here

		// constructors

		protected Regex () {
			// XXX what's this constructor for?
		}

		public Regex (string pattern) : this (pattern, RegexOptions.None) {
		}

		public Regex (string pattern, RegexOptions options) {
			this.pattern = pattern;
			this.options = options;
		
			this.factory = cache.Lookup (pattern, options);

			if (this.factory == null) {
				// parse and install group mapping

				Parser psr = new Parser ();
				RegularExpression re = psr.ParseRegularExpression (pattern, options);
				this.group_count = re.GroupCount;
				this.mapping = psr.GetMapping ();

				// compile
				
				ICompiler cmp;
				//if ((options & RegexOptions.Compiled) != 0)
				//	throw new Exception ("Not implemented.");
					//cmp = new CILCompiler ();
				//else
					cmp = new PatternCompiler ();

				re.Compile (cmp, RightToLeft);

				// install machine factory and add to pattern cache

				this.factory = cmp.GetMachineFactory ();
				this.factory.Mapping = mapping;
				cache.Add (pattern, options, this.factory);
			} else {
				this.group_count = this.factory.GroupCount;
				this.mapping = this.factory.Mapping;
			}
		}

		protected Regex (SerializationInfo info, StreamingContext context) :
			this (info.GetString ("pattern"), 
			      (RegexOptions) info.GetValue ("options", typeof (RegexOptions))) {			
		}


		// public instance properties
		
		public RegexOptions Options {
			get { return options; }
		}

		public bool RightToLeft {
			get { return (options & RegexOptions.RightToLeft) != 0; }
		}

		// public instance methods
		
		public string[] GetGroupNames () {
			string[] names = new string[mapping.Count];
			mapping.Keys.CopyTo (names, 0);

			return names;
		}

		public int[] GetGroupNumbers () {
			int[] numbers = new int[mapping.Count];
			mapping.Values.CopyTo (numbers, 0);

			return numbers;
		}

		public string GroupNameFromNumber (int i) {
			if (i >= group_count)
				return "";
		
			foreach (string name in mapping.Keys) {
				if ((int)mapping[name] == i)
					return name;
			}

			return "";
		}

		public int GroupNumberFromName (string name) {
			if (mapping.Contains (name))
				return (int)mapping[name];

			return -1;
		}

		// match methods
		
		public bool IsMatch (string input) {
			return IsMatch (input, 0);
		}

		public bool IsMatch (string input, int startat) {
			return Match (input, startat).Success;
		}

		public Match Match (string input) {
			return Match (input, 0);
		}

		public Match Match (string input, int startat) {
			return CreateMachine ().Scan (this, input, startat, input.Length);
		}

		public Match Match (string input, int startat, int length) {
			return CreateMachine ().Scan (this, input, startat, startat + length);
		}

		public MatchCollection Matches (string input) {
			return Matches (input, 0);
		}

		public MatchCollection Matches (string input, int startat) {
			MatchCollection ms = new MatchCollection ();
			Match m = Match (input, startat);
			while (m.Success) {
				ms.Add (m);
				m = m.NextMatch ();
			}

			return ms;
		}

		// replace methods

		public string Replace (string input, MatchEvaluator evaluator) {
			return Replace (input, evaluator, Int32.MaxValue, 0);
		}

		public string Replace (string input, MatchEvaluator evaluator, int count) {
			return Replace (input, evaluator, count, 0);
		}

		public string Replace (string input, MatchEvaluator evaluator, int count, int startat)
		{
			StringBuilder result = new StringBuilder ();
			int ptr = startat;

			Match m = Match (input, startat);
			while (m.Success && count -- > 0) {
				result.Append (input.Substring (ptr, m.Index - ptr));
				result.Append (evaluator (m));

				ptr = m.Index + m.Length;
				m = m.NextMatch ();
			}
			result.Append (input.Substring (ptr));

			return result.ToString ();
		}

		public string Replace (string input, string replacement) {
			return Replace (input, replacement, Int32.MaxValue, 0);
		}

		public string Replace (string input, string replacement, int count) {
			return Replace (input, replacement, count, 0);
		}

		public string Replace (string input, string replacement, int count, int startat) {
			ReplacementEvaluator ev = new ReplacementEvaluator (this, replacement);
			return Replace (input, new MatchEvaluator (ev.Evaluate), count, startat);
		}

		// split methods

		public string[] Split (string input) {
			return Split (input, Int32.MaxValue, 0);
		}

		public string[] Split (string input, int count) {
			return Split (input, count, 0);
		}

		public string[] Split (string input, int count, int startat) {
			ArrayList splits = new ArrayList ();
			if (count == 0)
				count = Int32.MaxValue;

			int ptr = startat;
			while (--count > 0) {
				Match m = Match (input, ptr);
				if (!m.Success)
					break;
			
				splits.Add (input.Substring (ptr, m.Index - ptr));
				ptr = m.Index + m.Length;
			}

			if (ptr < input.Length) {
				splits.Add (input.Substring (ptr));
			}

			return (string []) splits.ToArray (typeof (string));
		}

		// object methods
		
		public override string ToString () {
			return pattern;
		}

		// ISerializable interface
		public virtual void GetObjectData (SerializationInfo info, StreamingContext context) {
			info.AddValue ("pattern", this.ToString (), typeof (string));
			info.AddValue ("options", this.Options, typeof (RegexOptions));
		}

		// internal

		internal int GroupCount {
			get { return group_count; }
		}

		// private

		private IMachine CreateMachine () {
			return factory.NewInstance ();
		}

		protected internal string pattern;
		private RegexOptions options;

		private IMachineFactory factory;
		private IDictionary mapping;
		private int group_count;
	}

	[Serializable]
	public class RegexCompilationInfo {
		public RegexCompilationInfo (string pattern, RegexOptions options, string name, string full_namespace, bool is_public) {
			this.pattern = pattern;
			this.options = options;
			this.name = name;
			this.full_namespace = full_namespace;
			this.is_public = is_public;
		}

		public bool IsPublic {
			get { return is_public; }
			set { is_public = value; }
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public string Namespace {
			get { return full_namespace; }
			set { full_namespace = value; }
		}

		public RegexOptions Options {
			get { return options; }
			set { options = value; }
		}

		public string Pattern {
			get { return pattern; }
			set { pattern = value; }
		}

		// private

		private string pattern, name, full_namespace;
		private RegexOptions options;
		private bool is_public;
	}
}
