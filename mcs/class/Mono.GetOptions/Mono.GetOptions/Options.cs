
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

namespace Mono.GetOptions
{
	public class Options
	{
		public OptionsParsingMode ParsingMode;
		public bool BreakSingleDashManyLettersIntoManyOptions;
		public bool EndOptionProcessingWithDoubleDash;

		private OptionList optionParser;

		public string[] RemainingArguments;

		public Options()
		{
			ParsingMode = OptionsParsingMode.Both;
			BreakSingleDashManyLettersIntoManyOptions = false;
			EndOptionProcessingWithDoubleDash = true;
		}

		public void ProcessArgs(string[] args)
		{
			optionParser = new OptionList(this);
			RemainingArguments =  optionParser.ProcessArgs(args);
		}

		[Option("Show this help list", '?',"help")]
		public virtual WhatToDoNext DoHelp()
		{
			return optionParser.DoHelp();
		}

		[Option("Display version and licensing information", 'V', "version")]
		public virtual WhatToDoNext DoAbout()
		{
			return optionParser.DoAbout();
		}

		[Option("Show usage syntax and exit", ' ',"usage")]
		public virtual WhatToDoNext DoUsage()
		{
			return optionParser.DoUsage();
		}

		[Option("Show verbose parsing of options", ' ',"verbosegetoptions")]
		public bool VerboseParsingOfOptions
		{
			set { OptionDetails.Verbose = value;}
		}


	}
	
}
