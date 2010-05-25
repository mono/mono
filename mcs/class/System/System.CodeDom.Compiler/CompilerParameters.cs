//
// System.CodeDom.Compiler.CompilerParameters.cs
//
// Authors:
//   Daniel Stodden (stodden@in.tum.de)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Ximian, Inc.
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Policy;

namespace System.CodeDom.Compiler {

#if NET_2_0
	[Serializable]
#else
	[ComVisible (false)]
#endif
	[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
	[PermissionSet (SecurityAction.InheritanceDemand, Unrestricted = true)]
	public class CompilerParameters {

		private string compilerOptions;
#if NET_1_1
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
#if NET_2_0
		private StringCollection embedded_resources;
		private StringCollection linked_resources;
#endif

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

#if NET_1_1
#if NET_4_0
		[Obsolete]
#endif
		public Evidence Evidence {
			get { return evidence; }
			[SecurityPermission (SecurityAction.Demand, ControlEvidence = true)]
			set { evidence = value; }
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
				if (tempFiles == null)
					tempFiles = new TempFileCollection ();
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
#if NET_2_0
		[ComVisible (false)]
		public StringCollection EmbeddedResources {
			get {
				if (embedded_resources == null)
					embedded_resources = new StringCollection ();
				return embedded_resources;
			}
		}

		[ComVisible (false)]
		public StringCollection LinkedResources {
			get {
				if (linked_resources == null)
					linked_resources = new StringCollection ();
				return linked_resources;
			}
		}
#endif
	}
}
