//
// mcat.cs: Something similar to cat to exemplify using
//          Mono.GetOptions
//
// Author: Rafael Teixeira (rafaelteixeirabr@hotmail.com)
//
// (C) 2005 Rafael Teixeira
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
using System.IO;
using System.Text;

using Mono.GetOptions;

/* our source of inspiration

cat --help gives:

Usage: cat [OPTION] [FILE]...
Concatenate FILE(s), or standard input, to standard output.

  -A, --show-all           equivalent to -vET
  -b, --number-nonblank    number nonblank output lines
  -e                       equivalent to -vE
  -E, --show-ends          display $ at end of each line
  -n, --number             number all output lines
  -s, --squeeze-blank      never more than one single blank line
  -t                       equivalent to -vT
  -T, --show-tabs          display TAB characters as ^I
  -u                       (ignored)
  -v, --show-nonprinting   use ^ and M- notation, except for LFD and TAB
      --help     display this help and exit
      --version  output version information and exit

With no FILE, or when FILE is -, read standard input.

Report bugs to <bug-coreutils@gnu.org>.
*/

[assembly: System.Reflection.AssemblyTitle("mcat")]
[assembly: System.Reflection.AssemblyCopyright("(c)2005 Rafael Teixeira")]
[assembly: System.Reflection.AssemblyDescription("Simulated cat-like program")]
[assembly: System.Reflection.AssemblyVersion ("1.0.0.0")]

[assembly: Mono.About("Just a simulated cat to demonstrate Mono.GetOptions")]
[assembly: Mono.Author("Rafael Teixeira")]
[assembly: Mono.UsageComplement("[FILE]...\nConcatenate FILE(s), or standard input, to standard output.")]
[assembly: Mono.AdditionalInfo("With no FILE, or when FILE is -, read standard input.")]
[assembly: Mono.ReportBugsTo("rafaelteixeirabr@hotmail.com")]

public class CatLikeOptions : Options 
{	
	[Option("display TAB characters as ^I", 'T', "show-tabs")]
	public bool ShowTabs;

	[Option("display $ at end of each line", 'E', "show-ends")]
	public bool ShowLineEnds;
	
	[Option("use ^ and M- notation, except for LFD and TAB", 'v', "show-nonprinting")]
	public bool ShowNonPrinting;

	[Option("equivalent to -vE", 'e', null)]
	public bool ShowLineEndsAndNonPrinting { set { ShowLineEnds = ShowNonPrinting = value; } }
	
	[Option("equivalent to -vT", 't', null)]
	public bool ShowLineEndsAndTabs { set { ShowTabs = ShowNonPrinting = value; } }
	
	[Option("equivalent to -vET", 'A', "show-all")]
	public bool showAll { set { ShowTabs = ShowLineEnds = ShowNonPrinting = value; } }
	
	[Option("number nonblank output lines", 'b', "number-nonblank")]
	public bool NumberNonBlank;
	
	[Option("number all output lines", 'n', "number")]
	public bool NumberAllLines;
	
	[Option("never more than one single blank line", 's', "squeeze-blank")]
	public bool SqueezeBlankLines;
	
	[Option("(ignored)", 'u', null)]
	public bool Ignored;

	[Option("output version information and exit", "version")]
	public override WhatToDoNext DoAbout()
	{
		return base.DoAbout();
	}

	[Option("display this help and exit", "help")]
	public override WhatToDoNext DoHelp()
	{
		return base.DoHelp();
	}

	[KillOption]
	public override WhatToDoNext DoHelp2() { return WhatToDoNext.GoAhead; }

	[KillOption]
	public override WhatToDoNext DoUsage() { return WhatToDoNext.GoAhead; }

	public CatLikeOptions(string[] args) : base(args) {}
	
	protected override void InitializeOtherDefaults() 
	{
		ParsingMode = OptionsParsingMode.Both | OptionsParsingMode.GNU_DoubleDash;
		BreakSingleDashManyLettersIntoManyOptions = true; 
	}

}

public class Driver {

	public static int Main (string[] args)
	{
		CatLikeOptions options = new CatLikeOptions(args);
		
		Console.WriteLine(@"This is just a simulation of a cat-like program.

The command line options where processed by Mono.GetOptions and resulted as:

  ShowTabs = {0}
  ShowLineEnds = {1}
  ShowNonPrinting = {2}
  NumberNonBlank = {3}
  NumberAllLines = {4}
  SqueezeBlankLines = {5}
  
  RunningOnWindows = {6}

", 
			options.ShowTabs, options.ShowLineEnds, options.ShowNonPrinting,
			options.NumberNonBlank, options.NumberAllLines, options.SqueezeBlankLines, options.RunningOnWindows);
			
		if (options.GotNoArguments || options.FirstArgument == "-")
			Console.WriteLine("No arguments provided so cat would be copying stdin to stdout");
		else 
			Console.WriteLine("Would be copying these files to stdout: {0}", 
				String.Join(", ", options.RemainingArguments));
		Console.WriteLine("\nFollows help screen\n---------------------------------------------\n");
		options.DoHelp();
		return 0;
	}

}
