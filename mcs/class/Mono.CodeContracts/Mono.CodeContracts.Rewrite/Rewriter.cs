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
using Mono.CompilerServices.SymbolWriter;

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
		private bool usingMdb = false;
		private bool usingPdb = false;
		
		private void LoadSymbolReader (AssemblyDefinition assembly) {
			if (this.options.Assembly.IsStream && this.options.Assembly.Streams.Symbols == null) {
				this.warnings.Add ("-debug specified, but no symbol stream provided.");
			} else {
				try {
					foreach (ModuleDefinition module in assembly.Modules) {
						module.LoadSymbols ();
					}
					this.usingMdb = true;
				} catch {
				}
				if (!this.usingMdb && !this.usingPdb) {
					this.warnings.Add ("-debug specified, but no MDB or PDB symbol file found.");
				}
			}
		}
		
		private ISymbolWriter LoadSymbolWriter(AssemblyDefinition assembly, AssemblyRef output)
		{
			// TODO: Get symbol writing to work.
//			ISymbolWriterProvider symProv = null;
//			if (this.usingMdb) {
//				symProv = new Mono.Cecil.Mdb.MdbWriterProvider ();
//			} else if (this.usingPdb) {
//				symProv = new Mono.Cecil.Pdb.PdbWriterProvider ();
//			} else {
//				this.warnings.Add ("-writePDBFile specified, but no symbol file found, cannot write symbols.");
//			}
//			if (symProv != null) {
//				return output.IsFilename ?
//					symProv.GetSymbolWriter (assembly.MainModule, output.Filename) :
//					symProv.GetSymbolWriter (assembly.MainModule, output.Streams.Symbols);
//			}
			return null;
		}


		private RewriterResults RewriteImpl ()
		{
			if (!this.options.Rewrite) {
				return RewriterResults.Warning ("Not asked to rewrite");
			}

			if (!this.options.Assembly.IsSet) {
				return RewriterResults.Error ("No assembly given to rewrite");
			}
			AssemblyDefinition assembly = this.options.Assembly.IsFilename ?
				AssemblyFactory.GetAssembly (this.options.Assembly.Filename) :
				AssemblyFactory.GetAssembly (this.options.Assembly.Streams.Assembly);
			
			if (this.options.ForceAssemblyRename != null) {
				assembly.Name.Name = this.options.ForceAssemblyRename;
			} else if (this.options.OutputFile.IsSet && this.options.OutputFile.IsFilename) {
				assembly.Name.Name = Path.GetFileNameWithoutExtension(this.options.OutputFile.Filename);
			}

			if (options.Debug) {
				this.LoadSymbolReader (assembly);
			}

			var output = this.options.OutputFile.IsSet ? this.options.OutputFile : this.options.Assembly;
			ISymbolWriter symWriter = null;
			if (options.WritePdbFile) {
				if (!options.Debug) {
					return RewriterResults.Error ("Must specify -debug if using -writePDBFile.");
				}
				if (output.IsStream && output.Streams.Symbols==null){
					return RewriterResults.Error ("-writePDFFile specified, but no output symbol stream provided.");
				}
				symWriter = this.LoadSymbolWriter (assembly, output);
			}
			
			try {
				PerformRewrite rewriter = new PerformRewrite (symWriter, this.options);
				rewriter.Rewrite (assembly);

				if (output.IsFilename) {
					AssemblyFactory.SaveAssembly(assembly, output.Filename);
				} else {
					AssemblyFactory.SaveAssembly(assembly, output.Streams.Assembly);
				}
			} finally {
				if (symWriter != null) {
					symWriter.Dispose ();
				}
			}

			return new RewriterResults (warnings, errors);
		}
		
	}
}
