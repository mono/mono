//
// System.CodeDom.Compiler CompilerResults Class implementation
//
// Author:
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2002 Ximian, Inc.
//

namespace System.CodeDom.Compiler
{
	using System.Reflection;
	using System.Collections.Specialized;

	public class CompilerResults
	{
		private Assembly compiledAssembly;
		private CompilerErrorCollection errors;
		private int nativeCompilerReturnValue;
		private StringCollection output;
		private string pathToAssembly;
		private TempFileCollection tempFiles;
		
		//
		// Constructors
		//
		public CompilerResults( TempFileCollection tempFiles )
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
				if ( errors == null )
					errors = new CompilerErrorCollection();
				return errors;
			}
		}

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
				if ( output == null )
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
