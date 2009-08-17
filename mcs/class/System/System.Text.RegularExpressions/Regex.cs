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
	public partial class Regex : ISerializable {

#if !TARGET_JVM
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
#endif
		
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
		static FactoryCache cache = new FactoryCache (15);
		public static int CacheSize {
			get { return cache.Capacity; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ("CacheSize");

				cache.Capacity = value;	
			}
		}
#else
		static FactoryCache cache = new FactoryCache (200);
#endif

		// private


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
#if !TARGET_JVM
		private void Init ()
		{
			this.machineFactory = cache.Lookup (this.pattern, this.roptions);

			if (this.machineFactory == null) {
				InitNewRegex();
			} else {
				this.group_count = this.machineFactory.GroupCount;
				this.mapping = this.machineFactory.Mapping;
				this._groupNumberToNameMap = this.machineFactory.NamesMapping;
			}
		}
#endif

		private void InitNewRegex () 
		{
			this.machineFactory = CreateMachineFactory (this.pattern, this.roptions);
			cache.Add (this.pattern, this.roptions, this.machineFactory);
			this.group_count = machineFactory.GroupCount;
			this.mapping = machineFactory.Mapping;
			this._groupNumberToNameMap = this.machineFactory.NamesMapping;
		}

#if !NET_2_1
		// The new rx engine has blocking bugs like
		// https://bugzilla.novell.com/show_bug.cgi?id=470827
		static readonly bool old_rx =
			Environment.GetEnvironmentVariable ("MONO_NEW_RX") == null;
#endif

		private static IMachineFactory CreateMachineFactory (string pattern, RegexOptions options) 
		{
			Parser psr = new Parser ();
			RegularExpression re = psr.ParseRegularExpression (pattern, options);

#if NET_2_1
			ICompiler cmp = new PatternCompiler ();
#else
			ICompiler cmp;
			if (!old_rx) {
				if ((options & RegexOptions.Compiled) != 0)
					cmp = new CILCompiler ();
				else
					cmp = new RxCompiler ();
			} else {
				cmp = new PatternCompiler ();
			}
#endif

			re.Compile (cmp, (options & RegexOptions.RightToLeft) != 0);

			IMachineFactory machineFactory = cmp.GetMachineFactory ();
			machineFactory.Mapping = psr.GetMapping ();
			machineFactory.NamesMapping = GetGroupNamesArray (machineFactory.GroupCount, machineFactory.Mapping);

			return machineFactory;
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

#if ONLY_1_1 && !TARGET_JVM
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
			string [] names = new string [1 + group_count];
			Array.Copy (_groupNumberToNameMap, names, 1 + group_count);
			return names;
		}

		public int[] GetGroupNumbers ()
		{
			int[] numbers = new int [1 + group_count];
			for (int i = 0; i <= group_count; ++i)
				numbers [i] = i;
			// FIXME: needs to handle arbitrarily numbered groups '(?<43>abc)'
			return numbers;
		}

		public string GroupNameFromNumber (int i)
		{
			if (i < 0 || i > group_count)
				return "";

			return _groupNumberToNameMap [i];
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

		public string Replace (string input, MatchEvaluator evaluator, int count, int startat)
		{
			if (input == null)
				throw new ArgumentNullException ("null");
			if (evaluator == null)
				throw new ArgumentNullException ("evaluator");

			BaseMachine m = (BaseMachine)CreateMachine ();

			if (RightToLeft)
				return m.RTLReplace (this, input, evaluator, count, startat);

			// NOTE: If this is a cause of a lot of allocations, we can convert it to
			//       use a ThreadStatic allocation mitigator
			Adapter a = new Adapter (evaluator);

			return m.LTRReplace (this, input, new BaseMachine.MatchAppendEvaluator (a.Evaluate),
								 count, startat);
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
			return CreateMachine ().Replace (this, input, replacement, count, startat);
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
			return CreateMachine ().Split (this, input, count, startat);
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
#if !NET_2_1
		protected bool UseOptionC ()
		{
			return ((roptions & RegexOptions.Compiled) != 0);
		}
#endif
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

		private static string [] GetGroupNamesArray (int groupCount, IDictionary mapping) 
		{
			string [] groupNumberToNameMap = new string [groupCount + 1];
			foreach (string name in mapping.Keys) {
				groupNumberToNameMap [(int) mapping [name]] = name;
			}
			return groupNumberToNameMap;
		}
		
		private IMachineFactory machineFactory;
		private IDictionary mapping;
		private int group_count;
		private bool refsInitialized;
		private string [] _groupNumberToNameMap;

		
		// protected members

		protected internal string pattern;
		protected internal RegexOptions roptions;
		
		// MS undocumented members
#if NET_2_1
		[MonoTODO]
		internal System.Collections.Generic.Dictionary<string, int> capnames;
		[MonoTODO]
		internal System.Collections.Generic.Dictionary<int, int> caps;
#else
		[MonoTODO]
		protected internal System.Collections.Hashtable capnames;
		[MonoTODO]
		protected internal System.Collections.Hashtable caps;

		[MonoTODO]
		protected internal RegexRunnerFactory factory;
#endif
		[MonoTODO]
		protected internal int capsize;
		[MonoTODO]
		protected internal string [] capslist;
	}
}
