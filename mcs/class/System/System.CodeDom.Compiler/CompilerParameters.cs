//
// System.CodeDom.Compiler.CompilerParameters.cs
//
// Authors:
//   Daniel Stodden (stodden@in.tum.de)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Ximian, Inc.
//

using System.Collections.Specialized;
using System.Security.Policy;
using System.Runtime.InteropServices;

namespace System.CodeDom.Compiler
{
	[ComVisible (false)]
	public class CompilerParameters
	{
		private string compilerOptions;
		#if (NET_1_1)
			private Evidence evidence;
		#endif
		private bool generateExecutable = false;
		private bool generateInMemory = false;
		private bool includeDebugInformation = false;
		private string mainClass;
		private string outputAssembly;
		private StringCollection referencedAssemblies;
		private TempFileCollection tempFiles;
		private bool treatWarningsAsErrors = false;
		private IntPtr userToken = IntPtr.Zero;
		private int warningLevel = -1;
		private string win32Resource;

		//
		// Constructors
		//
		public CompilerParameters()
		{
		}
		
		public CompilerParameters (string[] assemblyNames)
		{
			referencedAssemblies = new StringCollection();
			referencedAssemblies.AddRange (assemblyNames);
		}

		public CompilerParameters (string[] assemblyNames, string output)
		{
			referencedAssemblies = new StringCollection();
			referencedAssemblies.AddRange (assemblyNames);
			outputAssembly = output;
		}

		public CompilerParameters (string[] assemblyNames, string output, bool includeDebugInfo)
		{
			referencedAssemblies = new StringCollection();
			referencedAssemblies.AddRange (assemblyNames);
			outputAssembly = output;
			includeDebugInformation = includeDebugInfo;
		}

		//
		// Properties
		//
		public string CompilerOptions {
			get {
				return compilerOptions;
			}
			set {
				compilerOptions = value;
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

		public bool GenerateExecutable {
			get {
				return generateExecutable;
			}
			set {
				generateExecutable = value;
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
		
		public string Win32Resource {
			get {
				return win32Resource;
			}
			set {
				win32Resource = value;
			}
		}
	}
}
