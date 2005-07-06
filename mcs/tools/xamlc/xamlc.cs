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
using System.CodeDom;
using System.CodeDom.Compiler;
using Mono.GetOptions;
using Mono.Windows.Serialization;

class XamlOptions : Options {
	[Option("Whether or not the class should be marked as partial", "p", "partial")]
	public bool Partial;
	
	[Option("the file to output to", "o", "output")]
	public string OutputFile;
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
		ICodeGenerator generator = (new Microsoft.CSharp.CSharpCodeProvider()).CreateGenerator();
		TextWriter tw = new StreamWriter(options.OutputFile);
		CodeWriter cw = new CodeWriter(generator, tw, options.Partial);
		XamlParser r = new XamlParser(input, cw);
		r.Parse();
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
