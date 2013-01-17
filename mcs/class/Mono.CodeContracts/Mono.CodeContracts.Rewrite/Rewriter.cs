//
// Rewriter.cs
//
// Authors:
//	Chris Bacon (chrisbacon76@gmail.com)
//
// Copyright (C) 2010 Chris Bacon
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;
using Mono.Cecil;
using System.IO;

namespace Mono.CodeContracts.Rewrite {
	public class Rewriter {

		public static RewriterResults Rewrite (RewriterOptions options)
		{
			Rewriter rewriter = new Rewriter(options);
			return rewriter.RewriteImpl();
		}
		
		private Rewriter(RewriterOptions options)
		{
			this.options = options;
		}
		
		private RewriterOptions options;
		private List<string> warnings = new List<string> ();
		private List<string> errors = new List<string> ();

		private RewriterResults RewriteImpl ()
		{
			if (!this.options.Rewrite) {
				return RewriterResults.Warning ("Not asked to rewrite");
			}

			if (!this.options.Assembly.IsSet) {
				return RewriterResults.Error ("No assembly given to rewrite");
			}

			var readerParameters = new ReaderParameters ();

			if (options.Debug)
				readerParameters.ReadSymbols = true;

			var assembly = this.options.Assembly.IsFilename ?
				AssemblyDefinition.ReadAssembly (options.Assembly.Filename, readerParameters) :
				AssemblyDefinition.ReadAssembly (options.Assembly.Streams.Assembly, readerParameters);
			
			if (this.options.ForceAssemblyRename != null) {
				assembly.Name.Name = this.options.ForceAssemblyRename;
			} else if (this.options.OutputFile.IsSet && this.options.OutputFile.IsFilename) {
				assembly.Name.Name = Path.GetFileNameWithoutExtension(this.options.OutputFile.Filename);
			}

			var output = this.options.OutputFile.IsSet ? this.options.OutputFile : this.options.Assembly;
			var writerParameters = new WriterParameters ();
			if (options.WritePdbFile) {
				if (!options.Debug) {
					return RewriterResults.Error ("Must specify -debug if using -writePDBFile.");
				}
				
				writerParameters.WriteSymbols = true;
			}
			
			PerformRewrite rewriter = new PerformRewrite (this.options);
			rewriter.Rewrite (assembly);

			if (output.IsFilename) {
				assembly.Write (output.Filename, writerParameters);
			} else {
				assembly.Write (output.Streams.Assembly, writerParameters);
			}
		
			return new RewriterResults (warnings, errors);
		}
		
	}
}
