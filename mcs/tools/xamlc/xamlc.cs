//
// xamlc.cs
//
// Author:
//   Iain McCoy (iain@mccoy.id.au)
//
// (C) 2005 Iain McCoy
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
using System.IO;
using System.Xml;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Reflection;
using Mono.GetOptions;
using Mono.Windows.Serialization;

[assembly: AssemblyTitle ("xamlc.exe")]
[assembly: AssemblyVersion (Consts.MonoVersion)]
[assembly: AssemblyDescription ("Compiler from XAML to more conventional languages")]
[assembly: AssemblyCopyright ("(c) Iain McCoy")]

[assembly: Mono.UsageComplement ("")]

[assembly: Mono.About("Compiler to create normal clr-based high level language source code from XAML")]
[assembly: Mono.Author ("Iain McCoy")]

class XamlOptions : Options {
	[Option("Whether or not the class should be marked as partial", "p", "partial")]
	public bool Partial;
	
	[Option("the file to output to", "o", "output")]
	public string OutputFile;

	[Option("the language in which to write the output file", "l", "lang")]
	public string OutputLanguage;
}

class Driver {
	public static void process(XamlOptions options, string input) {
		if (!input.EndsWith(".xaml")) {
			Console.WriteLine("Input filenames must end in .xaml");
			return;
		}
		if (Environment.Version.Major < 2 && options.Partial) {
			Console.WriteLine("This runtime version does not support partial classes");
			return;
		}
		if (options.OutputFile == null) {
			options.OutputFile = input + ".out";
		}
		ICodeGenerator generator = getGenerator(options.OutputLanguage);
		XmlTextReader xr = new XmlTextReader(input);
		try {
			string result = ParserToCode.Parse(xr, generator, options.Partial);
			TextWriter tw = new StreamWriter(options.OutputFile);
			tw.Write(result);
			tw.Close();
		}
		catch (Exception ex) {
			Console.WriteLine("Line " + xr.LineNumber + ", Column " + xr.LinePosition);
			throw ex;
		}
	}

	private static ICodeGenerator getGenerator(string language)
	{
		if (language == null || language == "cs" || language == "c#") {
			return (new Microsoft.CSharp.CSharpCodeProvider()).CreateGenerator();
		} else if (language == "vb") {
			return (new Microsoft.VisualBasic.VBCodeProvider()).CreateGenerator();
		} else {
			Console.WriteLine("Unknown language: " + language);
			Environment.Exit(1);
			return null;
		}
	}
	
	public static void Main(string[] args) {
		XamlOptions options = new XamlOptions();
		options.ProcessArgs(args);
		if (options.RemainingArguments.Length != 1) {
			Console.WriteLine("Need exactly one input file");
			return;
		}
		process(options, options.RemainingArguments[0]);
	}
}
