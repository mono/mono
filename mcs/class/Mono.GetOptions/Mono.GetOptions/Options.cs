//
// Options.cs
//
// Author: Rafael Teixeira (rafaelteixeirabr@hotmail.com)
//
// (C) 2002 Rafael Teixeira
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
using System.Collections;

namespace Mono.GetOptions
{

	public delegate void ErrorReporter (int num, string msg);

	[Obsolete ("This class is being obsoleted, we suggest that you use NDesk.Options library instead (ndesk.org/options")]
	public class Options
	{
		public OptionsParsingMode ParsingMode;
		public bool BreakSingleDashManyLettersIntoManyOptions;
		public bool EndOptionProcessingWithDoubleDash;
		public bool DontSplitOnCommas;
		public ErrorReporter ReportError;
		
		private OptionList optionParser;

		public Options() : this(null) {}
		
		public Options(string[] args) : this(args, OptionsParsingMode.Both, false, true, false, null) {}

		public Options(string[] args, 
					   OptionsParsingMode parsingMode, 
					   bool breakSingleDashManyLettersIntoManyOptions, 
					   bool endOptionProcessingWithDoubleDash,
					   bool dontSplitOnCommas) : 
			this(args, OptionsParsingMode.Both, false, true, false, null) {}
		
		public Options(string[] args, 
					   OptionsParsingMode parsingMode, 
					   bool breakSingleDashManyLettersIntoManyOptions, 
					   bool endOptionProcessingWithDoubleDash,
					   bool dontSplitOnCommas,
					   ErrorReporter reportError)
		{
			ParsingMode = parsingMode;
			BreakSingleDashManyLettersIntoManyOptions = breakSingleDashManyLettersIntoManyOptions;
			EndOptionProcessingWithDoubleDash = endOptionProcessingWithDoubleDash;
			DontSplitOnCommas = dontSplitOnCommas;
			if (reportError == null)
				ReportError = new ErrorReporter(DefaultErrorReporter);
			else
				ReportError = reportError;
			InitializeOtherDefaults();
			if (args != null)
				ProcessArgs(args);
		}
		
		protected virtual void InitializeOtherDefaults() { } // Only subclasses may need to implement something here

		public bool RunningOnWindows {
			get {
				// check for non-Unix platforms - see FAQ for more details
				// http://www.mono-project.com/FAQ:_Technical#How_to_detect_the_execution_platform_.3F
				int platform = (int) Environment.OSVersion.Platform;
				return ((platform != 4) && (platform != 128));
			}
		}

		#region non-option arguments
				
		private ArrayList arguments = new ArrayList();
		public string[] RemainingArguments;

		[ArgumentProcessor]
		public virtual void DefaultArgumentProcessor(string argument)
		{
			arguments.Add(argument);
		}
		
		public string FirstArgument  { get { return (arguments.Count > 0)?(string)arguments[0]:null; } }
		public string SecondArgument { get { return (arguments.Count > 1)?(string)arguments[1]:null; } }
		public string ThirdArgument  { get { return (arguments.Count > 2)?(string)arguments[2]:null; } }
		public string FourthArgument { get { return (arguments.Count > 3)?(string)arguments[3]:null; } }
		public string FifthArgument  { get { return (arguments.Count > 4)?(string)arguments[4]:null; } }
		
		public bool GotNoArguments { get { return arguments.Count == 0; }  }
		
		#endregion
		
		public void ProcessArgs(string[] args)
		{
			optionParser = new OptionList(this);
			optionParser.AdditionalBannerInfo = AdditionalBannerInfo;
			optionParser.ProcessArgs(args);
			RemainingArguments = (string[])arguments.ToArray(typeof(string));
		}

		private static void DefaultErrorReporter (int number, string message)
		{
			if (number > 0)
				Console.WriteLine("Error {0}: {1}", number, message);
			else
				Console.WriteLine("Error: {0}", message);				
		}
		
		public virtual string AdditionalBannerInfo { get { return null; } }
		
		public void ShowBanner()
		{
			optionParser.ShowBanner();
		}

		[Option("Show this help list", '?', "help")]
		public virtual WhatToDoNext DoHelp()
		{
			return optionParser.DoHelp();
		}

		[Option("Show an additional help list", "help2")]
		public virtual WhatToDoNext DoHelp2()
		{
			return optionParser.DoHelp2();
		}

		[Option("Display version and licensing information", 'V', "version")]
		public virtual WhatToDoNext DoAbout()
		{
			return optionParser.DoAbout();
		}

		[Option("Show usage syntax and exit", "usage")]
		public virtual WhatToDoNext DoUsage()
		{
			return optionParser.DoUsage();
		}

		private bool verboseParsingOfOptions = false;
		
		[Option("Show verbose parsing of options", '.', "verbosegetoptions", SecondLevelHelp = true)]
		public bool VerboseParsingOfOptions
		{
			set { verboseParsingOfOptions = value; }
			get { return verboseParsingOfOptions; }
		}

		private bool debuggingOfOptions = false;
		
		[Option("Show debugging info while processing options", '~', "debugoptions", SecondLevelHelp = true)]
		public bool DebuggingOfOptions
		{
			set { 
				debuggingOfOptions = value; 
				if (value) {
					Console.WriteLine("ParsingMode = {0}", ParsingMode);
					Console.WriteLine("BreakSingleDashManyLettersIntoManyOptions = {0}", BreakSingleDashManyLettersIntoManyOptions);
					Console.WriteLine("EndOptionProcessingWithDoubleDash = {0}", EndOptionProcessingWithDoubleDash);
					Console.WriteLine("DontSplitOnCommas = {0}", DontSplitOnCommas);
				}
			}
			get { return debuggingOfOptions; }
		}


	}
	
}
