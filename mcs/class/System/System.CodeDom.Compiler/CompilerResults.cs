//
// System.CodeDom.Compiler.CompilerResults.cs
//
// Authors:
//   Daniel Stodden (stodden@in.tum.de)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Ximian, Inc.
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

using System.Security.Policy;

namespace System.CodeDom.Compiler
{
	using System.Reflection;
	using System.Collections.Specialized;

	public class CompilerResults
	{
		private Assembly compiledAssembly;
		private CompilerErrorCollection errors = new CompilerErrorCollection ();
		#if (NET_1_1)
			private Evidence evidence;
		#endif
		private int nativeCompilerReturnValue = 0;
		private StringCollection output = new StringCollection ();
		private string pathToAssembly;
		private TempFileCollection tempFiles;
		
		//
		// Constructors
		//
		public CompilerResults (TempFileCollection tempFiles)
		{
			this.tempFiles = tempFiles;
		}

		//
		// Properties
		//
		public Assembly CompiledAssembly {
			get {
				return compiledAssembly;
			}
			set {
				compiledAssembly = value;
			}
		}

		public CompilerErrorCollection Errors {
			get {
				if (errors == null)
					errors = new CompilerErrorCollection();
				return errors;
			}
		}

		#if (NET_1_1)
		public Evidence Evidence {
			get {
				return evidence;
			}
			set {
				evidence = value;
			}
		}
		#endif

		public int NativeCompilerReturnValue {
			get {
				return nativeCompilerReturnValue;
			}
			set {
				nativeCompilerReturnValue = value;
			}
		}

		public StringCollection Output {
			get {
				if (output == null)
					output = new StringCollection();
				return output;
			}
		}

		public string PathToAssembly {
			get {
				return pathToAssembly;
			}
			set {
				pathToAssembly = value;
			}
		}

		public TempFileCollection TempFiles {
			get {
				return tempFiles;
			}
			set {
				tempFiles = value;
			}
		}
	}
}
