//
// System.CodeDom.Compiler.CompilerResults.cs
//
// Authors:
//   Daniel Stodden (stodden@in.tum.de)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Ximian, Inc.
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
