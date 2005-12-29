//
// assembly:	System
// namespace:	System.Text.RegularExpressions
// file:	regex.cs
//
// author:	Dan Lewis (dlewis@gmx.co.uk)
// 		(c) 2002

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
using System.Text;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;

using RegularExpression = System.Text.RegularExpressions.Syntax.RegularExpression;
using Parser = System.Text.RegularExpressions.Syntax.Parser;

using System.Diagnostics;


namespace System.Text.RegularExpressions {
	
	[Serializable]
	public class Regex : ISerializable {

#if NET_2_0
		private static int cache_size = 15;
#endif

		[MonoTODO]
		public static void CompileToAssembly (RegexCompilationInfo [] regexes, AssemblyName aname)
		{
			Regex.CompileToAssembly(regexes, aname, new CustomAttributeBuilder [] {}, null);
		}

		[MonoTODO]
		public static void CompileToAssembly (RegexCompilationInfo [] regexes, AssemblyName aname,
						      CustomAttributeBuilder [] attribs)
		{
			Regex.CompileToAssembly(regexes, aname, attribs, null);
		}

		[MonoTODO]
		public static void CompileToAssembly (RegexCompilationInfo [] regexes, AssemblyName aname,
						      CustomAttributeBuilder [] attribs, string resourceFile)
		{
			throw new NotImplementedException ();
			// TODO : Make use of attribs and resourceFile parameters
			/*
			AssemblyBuilder asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly (aname, AssemblyBuilderAccess.RunAndSave);
			ModuleBuilder modBuilder = asmBuilder.DefineDynamicModule("InnerRegexModule",aname.Name);
			Parser psr = new Parser ();	
			
			System.Console.WriteLine("CompileToAssembly");
			       
			for(int i=0; i < regexes.Length; i++)
				{
					System.Console.WriteLine("Compiling expression :" + regexes[i].Pattern);
					RegularExpression re = psr.ParseRegularExpression (regexes[i].Pattern, regexes[i].Options);
					
					// compile
										
					CILCompiler cmp = new CILCompiler (modBuilder, i);
					bool reverse = (regexes[i].Options & RegexOptions.RightToLeft) !=0;
					re.Compile (cmp, reverse);
					cmp.Close();
					
				}
		       

			// Define a runtime class with specified name and attributes.
			TypeBuilder builder = modBuilder.DefineType("ITest");
			builder.CreateType();
			asmBuilder.Save(aname.Name);
			*/
		}
		
		public static string Escape (string str)
		{
			return Parser.Escape (str);
		}

		public static string Unescape (string str)
		{
			return Parser.Unescape (str);
		}

		public static bool IsMatch (string input, string pattern)
		{
			return IsMatch (input, pattern, RegexOptions.None);
		}

		public static bool IsMatch (string input, string pattern, RegexOptions options)
		{
			Regex re = new Regex (pattern, options);
			return re.IsMatch (input);
		}

		public static Match Match (string input, string pattern)
		{
			return Regex.Match (input, pattern, RegexOptions.None);
		}

		public static Match Match (string input, string pattern, RegexOptions options)
		{
			Regex re = new Regex (pattern, options);
			return re.Match (input);
		}

		public static MatchCollection Matches (string input, string pattern)
		{
			return Matches (input, pattern, RegexOptions.None);
		}

		public static MatchCollection Matches (string input, string pattern, RegexOptions options)
		{
			Regex re = new Regex (pattern, options);
			return re.Matches (input);
		}

		public static string Replace (string input, string pattern, MatchEvaluator evaluator)
		{
			return Regex.Replace (input, pattern, evaluator, RegexOptions.None);
		}

		public static string Replace (string input, string pattern, MatchEvaluator evaluator,
					      RegexOptions options)
		{
			Regex re = new Regex (pattern, options);
			return re.Replace (input, evaluator);
		}

		public static string Replace (string input, string pattern, string replacement)
		{
			return Regex.Replace (input, pattern, replacement, RegexOptions.None);
		}

		public static string Replace (string input, string pattern, string replacement,
					      RegexOptions options)
		{
			Regex re = new Regex (pattern, options);
			return re.Replace (input, replacement);
		}

		public static string [] Split (string input, string pattern)
		{
			return Regex.Split (input, pattern, RegexOptions.None);
		}

		public static string [] Split (string input, string pattern, RegexOptions options)
		{
			Regex re = new Regex (pattern, options);
			return re.Split (input);
		}

#if NET_2_0
		[MonoTODO ("should be used somewhere ? FactoryCache ?")]
		public static int CacheSize {
			get { return cache_size; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ("CacheSize");
				cache_size = value;
			}
		}
#endif

		// private

		private static FactoryCache cache = new FactoryCache (200);	// TODO put some meaningful number here

		// constructors

		// This constructor is used by compiled regular expressions that are
		// classes derived from Regex class. No initialization required.
		protected Regex ()
		{
		}

		public Regex (string pattern) : this (pattern, RegexOptions.None)
		{
		}

		public Regex (string pattern, RegexOptions options)
		{
			this.pattern = pattern;
			this.roptions = options;
			Init ();
		}

		private void Init ()
		{
			this.machineFactory = cache.Lookup (this.pattern, this.roptions);

			if (this.machineFactory == null) {
				// parse and install group mapping

				Parser psr = new Parser ();
				RegularExpression re = psr.ParseRegularExpression (this.pattern, this.roptions);
				this.group_count = re.GroupCount;
				this.mapping = psr.GetMapping ();

				// compile
				
				ICompiler cmp;
				//if ((this.roptions & RegexOptions.Compiled) != 0)
				//	//throw new Exception ("Not implemented.");
				//	cmp = new CILCompiler ();
				//else
				cmp = new PatternCompiler ();

				re.Compile (cmp, RightToLeft);

				// install machine factory and add to pattern cache

				this.machineFactory = cmp.GetMachineFactory ();
				this.machineFactory.Mapping = mapping;
				cache.Add (this.pattern, this.roptions, this.machineFactory);
			} else {
				this.group_count = this.machineFactory.GroupCount;
				this.mapping = this.machineFactory.Mapping;
			}
		}

#if NET_2_0
		protected
#else
		private
#endif
		Regex (SerializationInfo info, StreamingContext context) :
			this (info.GetString ("pattern"), 
			      (RegexOptions) info.GetValue ("options", typeof (RegexOptions)))
		{
		}

#if NET_1_1
		// fixes public API signature
		~Regex ()
		{
		}
#endif
		// public instance properties
		
		public RegexOptions Options {
			get { return roptions; }
		}

		public bool RightToLeft {
			get { return (roptions & RegexOptions.RightToLeft) != 0; }
		}

		// public instance methods
		
		public string [] GetGroupNames ()
		{
			string [] names = new string [mapping.Count];
			mapping.Keys.CopyTo (names, 0);

			return names;
		}

		public int[] GetGroupNumbers ()
		{
			int[] numbers = new int [mapping.Count];
			mapping.Values.CopyTo (numbers, 0);

			return numbers;
		}

		public string GroupNameFromNumber (int i)
		{
			if (i > group_count)
				return "";
		
			foreach (string name in mapping.Keys) {
				if ((int) mapping [name] == i)
					return name;
			}

			return "";
		}

		public int GroupNumberFromName (string name)
		{
			if (mapping.Contains (name))
				return (int) mapping [name];

			return -1;
		}

		// match methods
		
		public bool IsMatch (string input)
		{
			return IsMatch (input, RightToLeft ? input.Length : 0);
		}

		public bool IsMatch (string input, int startat)
		{
			return Match (input, startat).Success;
		}

		public Match Match (string input)
		{
			return Match (input, RightToLeft ? input.Length : 0);
		}

		public Match Match (string input, int startat)
		{
			return CreateMachine ().Scan (this, input, startat, input.Length);
		}

		public Match Match (string input, int startat, int length)
		{
			return CreateMachine ().Scan (this, input, startat, startat + length);
		}

		public MatchCollection Matches (string input)
		{
			return Matches (input, RightToLeft ? input.Length : 0);
		}

		public MatchCollection Matches (string input, int startat)
		{
			Match m = Match (input, startat);
			return new MatchCollection (m);
		}

		// replace methods

		public string Replace (string input, MatchEvaluator evaluator)
		{
			return Replace (input, evaluator, Int32.MaxValue, RightToLeft ? input.Length : 0);
		}

		public string Replace (string input, MatchEvaluator evaluator, int count)
		{
			return Replace (input, evaluator, count, RightToLeft ? input.Length : 0);
		}

		class Adapter {
			MatchEvaluator ev;
			public Adapter (MatchEvaluator ev) { this.ev = ev; }
			public void Evaluate (Match m, StringBuilder sb) { sb.Append (ev (m)); }
		}

		delegate void MatchAppendEvaluator (Match match, StringBuilder sb);

		public string Replace (string input, MatchEvaluator evaluator, int count, int startat)
		{
			Adapter a = new Adapter (evaluator);
			return Replace (input, new MatchAppendEvaluator (a.Evaluate), count, startat);
		}

		string Replace (string input, MatchAppendEvaluator evaluator, int count, int startat)
		{
			StringBuilder result = new StringBuilder ();
			int ptr = startat;
			int counter = count;

			result.Append (input, 0, ptr);

			Match m = Match (input, startat);
			while (m.Success) {
				if (count != -1)
					if(counter -- <= 0)
						break;
				result.Append (input, ptr, m.Index - ptr);
				evaluator (m, result);

				ptr = m.Index + m.Length;
				m = m.NextMatch ();
			}
			
			if (ptr == 0)
				return input;
			
			result.Append (input, ptr, input.Length - ptr);

			return result.ToString ();
		}

		public string Replace (string input, string replacement)
		{
			return Replace (input, replacement, Int32.MaxValue, RightToLeft ? input.Length : 0);
		}

		public string Replace (string input, string replacement, int count)
		{
			return Replace (input, replacement, count, RightToLeft ? input.Length : 0);
		}

		public string Replace (string input, string replacement, int count, int startat)
		{
			ReplacementEvaluator ev = new ReplacementEvaluator (this, replacement);
			return Replace (input, new MatchAppendEvaluator (ev.EvaluateAppend), count, startat);
		}

		// split methods

		public string [] Split (string input)
		{
			return Split (input, Int32.MaxValue, RightToLeft ? input.Length : 0);
		}

		public string [] Split (string input, int count)
		{
			return Split (input, count, RightToLeft ? input.Length : 0);
		}

		public string [] Split (string input, int count, int startat)
		{
			ArrayList splits = new ArrayList ();
			if (count == 0)
				count = Int32.MaxValue;

			int ptr = startat;
			Match m = null;
			while (--count > 0) {
				if (m != null)
					m = m.NextMatch ();
				else
					m = Match (input, ptr);

				if (!m.Success)
					break;
			
				if (RightToLeft)
					splits.Add (input.Substring (m.Index + m.Length, ptr - m.Index - m.Length));
				else
					splits.Add (input.Substring (ptr, m.Index - ptr));
					
				int gcount = m.Groups.Count;
				for (int gindex = 1; gindex < gcount; gindex++) {
					Group grp = m.Groups [gindex];
					splits.Add (input.Substring (grp.Index, grp.Length));
				}

				if (RightToLeft)
					ptr = m.Index; 
				else
					ptr = m.Index + m.Length;
					
			}

			if (RightToLeft && ptr >= 0)
				splits.Add (input.Substring (0, ptr));
			if (!RightToLeft && ptr <= input.Length)
				splits.Add (input.Substring (ptr));

			return (string []) splits.ToArray (typeof (string));
		}

		// This method is called at the end of the constructor of compiled
		// regular expression classes to do internal initialization.
		protected void InitializeReferences ()
		{
			if (refsInitialized)
				throw new NotSupportedException ("This operation is only allowed once per object.");

			refsInitialized = true;

			// Compile pattern that results in performance loss as existing
			// CIL code is ignored but provides support for regular
			// expressions compiled to assemblies.
			Init ();
		}

		protected bool UseOptionC ()
		{
			return ((roptions & RegexOptions.Compiled) != 0);
		}

		protected bool UseOptionR ()
		{
			return ((roptions & RegexOptions.RightToLeft) != 0);
		}

		// object methods
		
		public override string ToString ()
		{
			return pattern;
		}

		// ISerializable interface
		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			info.AddValue ("pattern", this.ToString (), typeof (string));
			info.AddValue ("options", this.Options, typeof (RegexOptions));
		}

		// internal

		internal int GroupCount {
			get { return group_count; }
		}

		// private

		private IMachine CreateMachine ()
		{
			return machineFactory.NewInstance ();
		}

		private IMachineFactory machineFactory;
		private IDictionary mapping;
		private int group_count;
		private bool refsInitialized;

		
		// protected members

		protected internal string pattern;
		protected internal RegexOptions roptions;
		
		// MS undocumented members
		[MonoTODO]
		protected internal System.Collections.Hashtable capnames;
		[MonoTODO]
		protected internal System.Collections.Hashtable caps;
		[MonoTODO]
		protected internal int capsize;
		[MonoTODO]
		protected internal string [] capslist;
		[MonoTODO]
		protected internal RegexRunnerFactory factory;
	}
}
