//
// System.CodeDom.Compiler CompilerParameters Class implementation
//
// Author:
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2002 Ximian, Inc.
//

using System.Collections.Specialized;

namespace System.CodeDom.Compiler
{
	public class CompilerParameters
	{
		private CompilerOptions compilerOptions;
		private bool generateExecutables;
		private bool generateInMemory;
		private bool includeDebugInformation;
		private string mainClass;
		private string outputAssembly;
		private StringCollection referencedAssemblies;
		private TempFileCollection tempFiles;
		private bool treatWarningsAsErrors;
		private IntPtr userToken;
		private int warningLevel;
		private string win32Resources;

		//
		// Constructors
		//
		public CompilerParameters()
		{
		}
		
		public CompilerParameters( string[] assemblyNames )
		{
			referencedAssemblies=new StringCollection();
			referencedAssemblies.AddRange(assemblyNames);
		}

		public CompilerParameters( string[] assemblyNames, string output )
		{
			referencedAssemblies=new StringCollection();
			referencedAssemblies.AddRange(assemblyNames);
			outputAssembly=output;
		}

		public CompilerParameters( string[] assemblyNames, string output, bool includeDebugInfo )
		{
			referencedAssemblies=new StringCollection();
			referencedAssemblies.AddRange(assemblyNames);
			outputAssembly=output;
			includeDebugInformation=includeDebugInfo;
		}

		
		//
		// Properties
		//
		public CompilerOptions CompilerOptions {
			get {
				return compilerOptions;
			}
			set {
				compilerOptions = value;
			}
		}

		public bool GenerateExecutables {
			get {
				return generateExecutables;
			}
			set {
				generateExecutables = value;
			}
		}

		public bool GenerateInMemory {
			get {
				return generateInMemory;
			}
			set {
				generateInMemory = value;
			}
		}
		
		public bool IncludeDebugInformation {
			get {
				return includeDebugInformation;
			}
			set {
				includeDebugInformation = value;
			}
		}

		public string MainClass {
			get {
				return mainClass;
			}
			set {
				mainClass = value;
			}
		}

		public string OutputAssembly {
			get {
				return outputAssembly;
			}
			set {
				outputAssembly = value;
			}
		}

		public StringCollection ReferencedAssemblies {
			get {
				if (referencedAssemblies == null)
					referencedAssemblies = new StringCollection ();

				return referencedAssemblies;
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

		public bool TreatWarningsAsErrors {
			get {
				return treatWarningsAsErrors;
			}
			set {
				treatWarningsAsErrors = value;
			}
		}

		public IntPtr UserToken {
			get {
				return userToken;
			}
			set {
				userToken = value;
			}
		}

		public int WarningLevel {
			get {
				return warningLevel;
			}
			set {
				warningLevel = value;
			}
		}
		
		public string Win32Resources {
			get {
				return win32Resources;
			}
			set {
				win32Resources = value;
			}
		}
	}
};
