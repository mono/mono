//
// ProgramOptions.cs: Parser for Program Options
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//
// Permission is hereby granted, free of charge, to any           
// person obtaining a copy of this software and associated        
// documentation files (the "Software"), to deal in the           
// Software without restriction, including without limitation     
// the rights to use, copy, modify, merge, publish,               
// distribute, sublicense, and/or sell copies of the Software,    
// and to permit persons to whom the Software is furnished to     
// do so, subject to the following conditions:                    
//                                                                 
// The above copyright notice and this permission notice          
// shall be included in all copies or substantial portions        
// of the Software.                                               
//                                                                 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY      
// KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO         
// THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A               
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL      
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,      
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,  
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION       
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

/*
 * TODO: 
 *
 * Use a Dictionary object to store options
 *  - Removes O(n) searches that are currently used for option lookup
 *     - OR -
 *  - sort the options and use BinarySearch
 *
 * Option Handling:
 *  - Specify two sets of option groups:
 *    - Those that are "combinable"
 *    - Those that aren't
 *
 *  - "Combining" refers to the ability to specify multiple options "at once",
 *    e.g. `ls -ctr' is a combined form of `ls -c -t -r'.
 *
 *  - Specify a way to state how many '-'s prefix an option
 *
 *  - Specify the actual option prefix ('/' or '-' or...)
 *
 *  - Specify a way to set the delimeter between the option and the argument.
 *
 *  - Add a `count' field so that options can be specified multiple times
 *
 *  - Allow options to be specified multiple times, and collect all of the
 *    arguments for the option.
 *
 *  - These changes to option handling should permit:
 *    - GCC-style options
 *      - e.g. `-dumpspecs' is a valid option (note 1 '-', not 2A)
 *      - e.g. `-DMYSYMBOL' is the option `-D' with the value `MYSYMBOL'
 *      - e.g. `-DFOO -DBAR' makes arguments `FOO' and `BAR' available when
 *        searching for option `D'
 *    - CSC.EXE-style options
 *      - /out:file-name
 *      - /help
 *      - /t:exe or /target:exe (/t is a synonym for /target)
 *    - Multiple level options (forget which program I saw this in...)
 *      - -v is 1 level of verbosity
 *      - -vv is 2 levels of verbosity
 *      - ...
 *
 * Sanity Checking
 *  - Should make sure that there aren't any duplicate entries
 *
 * Pie-in-the-sky:
 *  - Provide a way to specify how many positions an argument takes up.
 *    - Currently, options with arguments can have 1 argv[] slot (for the
 *      argument) (for non-long-opt options).
 *    - It might be useful to allow arguments to have > 1
 *    - Not aware of any program that would actually make use of this.
 */

using System;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Mono.TypeReflector
{
	public class OptionException : Exception {

		public char ShortForm;

		public OptionException (string reason, char shortForm )
			: base (reason)
		{
			this.ShortForm = shortForm;
		}

		public OptionException (string reason)
			: base (reason)
		{
		}

		public OptionException (string reason, Exception inner)
			: base (reason, inner)
		{
		}
	}

	public class ProgramOptions {

		private class Option {
			public char   ShortForm = '\0';
			public string LongForm = null;
			public string Description = null;
			public bool   Found = false;
			public bool   HasArgument = false;
			public string ArgumentValue = null;
			public bool   AllowMultiple = false;
			public string ArgumentDescription = null;

			public Option (char shortForm, string longForm, string description, string argDesc, bool hasArgument)
			{
				ShortForm = shortForm;
				LongForm = longForm;
				Description = description;
				HasArgument = hasArgument;
				ArgumentDescription = argDesc;
			}

			public Option (char shortForm, string description)
			{
				ShortForm = shortForm;
				Description = description;
			}

			public Option (string longForm, string description)
			{
				LongForm = longForm;
				Description = description;
			}
		}

		// Option array
		private ArrayList options = new ArrayList ();

		// string array
		private ArrayList unmatched = new ArrayList ();

		public IList UnmatchedOptions {
			get {return unmatched;}
		}

		public ProgramOptions ()
		{
		}

		private void AddOption (Option opt)
		{
			options.Add (opt);
		}

		public void AddHelpOption ()
		{
			AddOption (new Option ('h', "help", "Display this help and exit.", null, false));
		}

		public void AddOption (char shortForm, string description)
		{
			AddOption (new Option (shortForm, description));
		}

		public void AddArgumentOption (char shortForm, string description, string argument)
		{
			AddOption (new Option (shortForm, null, description, argument, true));
		}

		public void AddOption (string longForm, string description)
		{
			AddOption (new Option (longForm, description));
		}

		public void AddArgumentOption (string longForm, string description, string argument)
		{
			AddOption (new Option ('\0', longForm, description, argument, true));
		}

		public void AddOption (char shortForm, string longForm, string description)
		{
			AddOption (new Option (shortForm, longForm, description, null, false));
		}

		public void AddArgumentOption (char shortForm, string longForm, string description, string argument)
		{
			AddOption (new Option (shortForm, longForm, description, argument, true));
		}

		public virtual void ParseOptions (string[] options)
		{
			int len = options.Length;
			bool handle = true;
			for (int cur = 0; cur != len; ++cur) {
				string option = options[cur];
				// necessary?
				if (option == null || option.Length == 0)
					continue;
				if (handle) {
					if (option.StartsWith ("-")) {
						// possible option
						if (option == "--")
							handle = false;
						else if (option.StartsWith ("--"))
							ParseLongOption (options, ref cur);
						else
							ParseShortOptions (options, ref cur);
					}
					else
						unmatched.Add (option);
				}
				else
					unmatched.Add (option);
			}
		}

		private void ParseLongOption (string[] args, ref int currentIndex)
		{
			// get rid of "--"
			string arg = args[currentIndex].Substring (2);
			bool found = false;
			foreach (Option o in options) {
				if (o.LongForm == null)
					continue;
				if (!arg.StartsWith(o.LongForm))
					continue;
				found = true;
				o.Found = true;
				if (o.HasArgument) {
					try {
						o.ArgumentValue = arg.Substring (arg.IndexOf('=')+1);
					} catch (Exception e) {
						throw new OptionException (
							"missing argument to option --" + o.LongForm,
							e);
					}
				}
			}

			if (!found)
				throw new OptionException (
					String.Format ("Unrecognized option `{0}'", args[currentIndex]));
		}

		private void ParseShortOptions (string[] args, ref int currentIndex)
		{
			string arg = args[currentIndex].Substring (1);
			int needsArg = 0;
			Option forArg = null;
			for (int i = 0; i != arg.Length; ++i) {
				bool found = false;
				foreach (Option o in options) {
					if (o.ShortForm != arg[i])
						continue;
					found = true;
					o.Found = true;
					if (o.HasArgument) {
						++needsArg;
						forArg = o;
					}
				}
				if (!found)
					throw new OptionException (
						String.Format("Unrecognized option `-{0}'", arg[i]));
			}

			if (needsArg > 1)
				throw new OptionException ("too many options requiring arguments specified");
			else if (needsArg == 1) {
				if (currentIndex == (args.Length - 1))
					throw new OptionException ("missing argument to option -" + forArg.ShortForm);
				++currentIndex;
				forArg.ArgumentValue = args[currentIndex];
			}
		}

		public virtual void Clear ()
		{
			foreach (Option o in options) {
				o.Found = false;
				o.ArgumentValue = null;
			}
		}

		private static readonly string[] OptionFormats = 
			{
			// 0: no short, no long, no arg
			"<invalid option format: 0: 0={0},1={1},2={2}>",
			// 1: short only
			"  -{0}",
			// 2: long only
			"      --{1}",
			// 3: long & short
			"  -{0}, --{1}",
			// 4: no short, no long, arg
			"<invalid option format: 4: 0={0},1={1},2={2}>",
			// 5: short w/ arg
			"  -{0} {2}",
			// 6: long w/ arg
			"      --{1}={2}",
			// 7: short & long w/ arg
			"  -{0}, --{1}={2}"
			};

		public virtual string OptionsHelp {
			get {
				StringBuilder sb = new StringBuilder ();
				foreach (Option o in options) {
					uint f_s =  Convert.ToUInt32 (o.ShortForm != '\0');
					uint f_l =  Convert.ToUInt32 (o.LongForm != null);
					uint f_h =  Convert.ToUInt32 (o.HasArgument);
					uint format = (f_s << 0) | (f_l << 1) | (f_h << 2);
					string opt = String.Format (OptionFormats[format], 
							o.ShortForm, 
							o.LongForm, 
							o.ArgumentDescription);
					string fmt = null;
					if (opt.Length < 30)
						fmt = "{0,-30}{1}";
					else
						fmt = "{0,-30}\n{2,-30}{1}";
					string d = new TextFormatter (30, 80, 2).Group (o.Description);
					sb.Append (String.Format (fmt, opt, d, ""));
					sb.Append ("\n");
				}
				return sb.ToString ();
			}
		}

		public static readonly string ProgramName = Environment.GetCommandLineArgs()[0];

		public bool FoundOption (char shortForm)
		{
			foreach (Option o in options) {
				if (o.ShortForm != shortForm)
					continue;
				return o.Found;
			}
			return false;
		}

		public bool FoundOption (string longForm)
		{
			foreach (Option o in options) {
				if (o.LongForm != longForm)
					continue;
				return o.Found;
			}
			return false;
		}

		public string FoundOptionValue (char shortForm)
		{
			foreach (Option o in options) {
				if (o.ShortForm != shortForm)
					continue;
				return o.ArgumentValue;
			}
			return null;
		}

		public string FoundOptionValue (string longForm)
		{
			foreach (Option o in options) {
				if (o.LongForm != longForm)
					continue;
				return o.ArgumentValue;
			}
			return null;
		}

		public bool FoundHelp {
			get {return FoundOption ('h');}
		}
	}
}

