//
// System.Web.Compilation.AssemblyBuilder
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//      Marek Habersack (mhabersack@novell.com)
//
// (C) 2006-2008 Novell, Inc (http://www.novell.com)
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



using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Security.Cryptography;
using System.Reflection;
using System.Text;
using System.Web.Configuration;
using System.Web.Util;
using System.Web.Hosting;

namespace System.Web.Compilation
{
	class CompileUnitPartialType
	{
		public readonly CodeCompileUnit Unit;
		public readonly CodeNamespace ParentNamespace;
		public readonly CodeTypeDeclaration PartialType;

		string typeName;
		
		public string TypeName {
			get {
				if (typeName == null) {
					if (ParentNamespace == null || PartialType == null)
						return null;
					
					typeName = ParentNamespace.Name;
					if (String.IsNullOrEmpty (typeName))
						typeName = PartialType.Name;
					else
						typeName += "." + PartialType.Name;
				}

				return typeName;
			}
		}
		
		public CompileUnitPartialType (CodeCompileUnit unit, CodeNamespace parentNamespace, CodeTypeDeclaration type)
		{
			this.Unit = unit;
			this.ParentNamespace = parentNamespace;
			this.PartialType = type;
		}
	}
	
	public class AssemblyBuilder
	{
		struct CodeUnit
		{
			public readonly BuildProvider BuildProvider;
			public readonly CodeCompileUnit Unit;

			public CodeUnit (BuildProvider bp, CodeCompileUnit unit)
			{
				this.BuildProvider = bp;
				this.Unit = unit;
			}
		}

		interface ICodePragmaGenerator
		{
			int ReserveSpace (string filename);
			void DecorateFile (string path, string filename, MD5 checksum, Encoding enc);
		}

		class CSharpCodePragmaGenerator : ICodePragmaGenerator
		{
			// Copied from CSharpCodeGenerator.cs
			string QuoteSnippetString (string value)
			{
				// FIXME: this is weird, but works.
				string output = value.Replace ("\\", "\\\\");
				output = output.Replace ("\"", "\\\"");
				output = output.Replace ("\t", "\\t");
				output = output.Replace ("\r", "\\r");
				output = output.Replace ("\n", "\\n");
				
				return "\"" + output + "\"";
			}

			string ChecksumToHex (MD5 checksum)
			{
				var ret = new StringBuilder ();
				foreach (byte b in checksum.Hash)
					ret.Append (b.ToString ("X2"));

				return ret.ToString ();
			}

			const int pragmaChecksumStaticCount = 23;
			const int pragmaLineStaticCount = 8;
			const int md5ChecksumCount = 32;
			
			public int ReserveSpace (string filename) 
			{
				return pragmaChecksumStaticCount +
					pragmaLineStaticCount +
					md5ChecksumCount +
					(QuoteSnippetString (filename).Length * 2) +
					(Environment.NewLine.Length * 3) +
					BaseCompiler.HashMD5.ToString ("B").Length;
			}
			
			public void DecorateFile (string path, string filename, MD5 checksum, Encoding enc)
			{
				string newline = Environment.NewLine;
				var sb = new StringBuilder ();
				
				sb.AppendFormat ("#pragma checksum {0} \"{1}\" \"{2}\"{3}{3}",
						 QuoteSnippetString (filename),
						 BaseCompiler.HashMD5.ToString ("B"),
						 ChecksumToHex (checksum),
						 newline);
				sb.AppendFormat ("#line 1 {0}{1}", QuoteSnippetString (filename), newline);

				byte[] bytes = enc.GetBytes (sb.ToString ());
				using (FileStream fs = new FileStream (path, FileMode.Open, FileAccess.Write)) {
					fs.Seek (enc.GetPreamble ().Length, SeekOrigin.Begin);
					fs.Write (bytes, 0, bytes.Length);
					bytes = null;
				
					sb.Length = 0;
					sb.AppendFormat ("{0}#line default{0}#line hidden{0}", newline);
					bytes = Encoding.UTF8.GetBytes (sb.ToString ());
				
					fs.Seek (0, SeekOrigin.End);
					fs.Write (bytes, 0, bytes.Length);
				}
				
				sb = null;
				bytes = null;
			}
		}

		class VBCodePragmaGenerator : ICodePragmaGenerator
		{
			const int pragmaExternalSourceCount = 21;
			public int ReserveSpace (string filename)
			{
				return pragmaExternalSourceCount +
					filename.Length +
					(Environment.NewLine.Length);
			}
			
			public void DecorateFile (string path, string filename, MD5 checksum, Encoding enc)
			{
				string newline = Environment.NewLine;
				var sb = new StringBuilder ();

				sb.AppendFormat ("#ExternalSource(\"{0}\",1){1}", filename, newline);
				byte[] bytes = enc.GetBytes (sb.ToString ());
				using (FileStream fs = new FileStream (path, FileMode.Open, FileAccess.Write)) {
					fs.Seek (enc.GetPreamble ().Length, SeekOrigin.Begin);
					fs.Write (bytes, 0, bytes.Length);
					bytes = null;

					sb.Length = 0;
					sb.AppendFormat ("{0}#End ExternalSource{0}", newline);
					bytes = enc.GetBytes (sb.ToString ());
					fs.Seek (0, SeekOrigin.End);
					fs.Write (bytes, 0, bytes.Length);
				}
				sb = null;
				bytes = null;
			}
		}
		
		const string DEFAULT_ASSEMBLY_BASE_NAME = "App_Web_";
		const int COPY_BUFFER_SIZE = 8192;
		
		static bool KeepFiles = (Environment.GetEnvironmentVariable ("MONO_ASPNET_NODELETE") != null);
		
		CodeDomProvider provider;
		CompilerParameters parameters;

		Dictionary <string, bool> code_files;
		Dictionary <string, List <CompileUnitPartialType>> partial_types;
		Dictionary <string, BuildProvider> path_to_buildprovider;
		List <CodeUnit> units;
		List <string> source_files;
		List <Assembly> referenced_assemblies;
		Dictionary <string, string> resource_files;
		TempFileCollection temp_files;
		string outputFilesPrefix;
		string outputAssemblyPrefix;
		string outputAssemblyName;
		
		internal AssemblyBuilder (CodeDomProvider provider)
		: this (null, provider, DEFAULT_ASSEMBLY_BASE_NAME)
		{}

		internal AssemblyBuilder (CodeDomProvider provider, string assemblyBaseName)
		: this (null, provider, assemblyBaseName)
		{}

		internal AssemblyBuilder (VirtualPath virtualPath, CodeDomProvider provider)
		: this (virtualPath, provider, DEFAULT_ASSEMBLY_BASE_NAME)
		{}
		
		internal AssemblyBuilder (VirtualPath virtualPath, CodeDomProvider provider, string assemblyBaseName)
		{
			this.provider = provider;
			this.outputFilesPrefix = assemblyBaseName ?? DEFAULT_ASSEMBLY_BASE_NAME;
			
			units = new List <CodeUnit> ();

			CompilationSection section;
			section = (CompilationSection) WebConfigurationManager.GetWebApplicationSection ("system.web/compilation");
			string tempdir = section.TempDirectory;
			if (String.IsNullOrEmpty (tempdir))
				tempdir = AppDomain.CurrentDomain.SetupInformation.DynamicBase;

			if (!KeepFiles)
				KeepFiles = section.Debug;
			
			temp_files = new TempFileCollection (tempdir, KeepFiles);
		}

		internal string OutputFilesPrefix {
			get {
				if (outputFilesPrefix == null)
					outputFilesPrefix = DEFAULT_ASSEMBLY_BASE_NAME;

				return outputFilesPrefix;
			}

			set {
				if (String.IsNullOrEmpty (value))
					outputFilesPrefix = DEFAULT_ASSEMBLY_BASE_NAME;
				else
					outputFilesPrefix = value;
				outputAssemblyPrefix = null;
				outputAssemblyName = null;
			}
		}
		
		internal string OutputAssemblyPrefix {
			get {
				if (outputAssemblyPrefix == null) {
					string basePath = temp_files.BasePath;
					string baseName = Path.GetFileName (basePath);
					string baseDir = Path.GetDirectoryName (basePath);

					outputAssemblyPrefix = Path.Combine (baseDir, String.Concat (OutputFilesPrefix, baseName));
				}

				return outputAssemblyPrefix;
			}
		}

		internal string OutputAssemblyName {
			get {
				if (outputAssemblyName == null)
					outputAssemblyName = OutputAssemblyPrefix + ".dll";

				return outputAssemblyName;
			}
		}
		
		internal TempFileCollection TempFiles {
			get { return temp_files; }
		}

		internal CompilerParameters CompilerOptions {
			get { return parameters; }
			set { parameters = value; }
		}
		
		CodeUnit[] GetUnitsAsArray ()
		{
			CodeUnit[] result = new CodeUnit [units.Count];
			units.CopyTo (result, 0);
			return result;
		}
		
		internal Dictionary <string, List <CompileUnitPartialType>> PartialTypes {
			get {
				if (partial_types == null)
					partial_types = new Dictionary <string, List <CompileUnitPartialType>> ();
				return partial_types;
			}
		}
		
		Dictionary <string, bool> CodeFiles {
			get {
				if (code_files == null)
					code_files = new Dictionary <string, bool> ();
				return code_files;
			}
		}
		
		List <string> SourceFiles {
			get {
				if (source_files == null)
					source_files = new List <string> ();
				return source_files;
			}
		}

		Dictionary <string, string> ResourceFiles {
			get {
				if (resource_files == null)
					resource_files = new Dictionary <string, string> ();
				return resource_files;
			}
		}

		internal BuildProvider GetBuildProviderForPhysicalFilePath (string path)
		{
			if (String.IsNullOrEmpty (path) || path_to_buildprovider == null || path_to_buildprovider.Count == 0)
				return null;

			BuildProvider ret;
			if (path_to_buildprovider.TryGetValue (path, out ret))
				return ret;

			return null;
		}
		
		public void AddAssemblyReference (Assembly a)
		{
			if (a == null)
				throw new ArgumentNullException ("a");

			List <Assembly> assemblies = ReferencedAssemblies;
			
			if (assemblies.Contains (a))
				return;
			
			assemblies.Add (a);
		}

		internal void AddAssemblyReference (string assemblyLocation)
		{
			try {
				Assembly asm = Assembly.LoadFrom (assemblyLocation);
				if (asm == null)
					return;

				AddAssemblyReference (asm);
			} catch {
				// ignore, it will come up later
			}
		}

		internal void AddAssemblyReference (ICollection asmcoll)
		{
			if (asmcoll == null || asmcoll.Count == 0)
				return;

			Assembly asm;
			foreach (object o in asmcoll) {
				asm = o as Assembly;
				if (asm == null)
					continue;

				AddAssemblyReference (asm);
			}
		}
		
		internal void AddAssemblyReference (List <Assembly> asmlist)
		{
			if (asmlist == null)
				return;
			
			foreach (Assembly a in asmlist) {
				if (a == null)
					continue;

				AddAssemblyReference (a);
			}
		}
		
		internal void AddCodeCompileUnit (CodeCompileUnit compileUnit)
		{
			if (compileUnit == null)
				throw new ArgumentNullException ("compileUnit");
			units.Add (CheckForPartialTypes (new CodeUnit (null, compileUnit)));
		}
				
		public void AddCodeCompileUnit (BuildProvider buildProvider, CodeCompileUnit compileUnit)
		{
			if (buildProvider == null)
				throw new ArgumentNullException ("buildProvider");

			if (compileUnit == null)
				throw new ArgumentNullException ("compileUnit");

			units.Add (CheckForPartialTypes (new CodeUnit (buildProvider, compileUnit)));
		}

		void AddPathToBuilderMap (string path, BuildProvider bp)
		{
			if (path_to_buildprovider == null)
				path_to_buildprovider = new Dictionary <string, BuildProvider> ();

			if (path_to_buildprovider.ContainsKey (path))
				return;

			path_to_buildprovider.Add (path, bp);
		}
		
		public TextWriter CreateCodeFile (BuildProvider buildProvider)
		{
			if (buildProvider == null)
				throw new ArgumentNullException ("buildProvider");

			// Generate a file name with the correct source language extension
			string filename = GetTempFilePhysicalPath (provider.FileExtension);
			SourceFiles.Add (filename);
			AddPathToBuilderMap (filename, buildProvider);
			return new StreamWriter (File.OpenWrite (filename));
		}

		internal void AddCodeFile (string path)
		{
			AddCodeFile (path, null, false);
		}

		internal void AddCodeFile (string path, BuildProvider bp)
		{
			AddCodeFile (path, bp, false);
		}

		// The kludge of using ICodePragmaGenerator for C# and VB code files is bad, but
		// it's better than allowing for potential DoS while reading a file with arbitrary
		// size in memory for use with the CodeSnippetCompileUnit class.
		// Files with extensions other than .cs and .vb use CodeSnippetCompileUnit.
		internal void AddCodeFile (string path, BuildProvider bp, bool isVirtual)
		{
			if (String.IsNullOrEmpty (path))
				return;

			Dictionary <string, bool> codeFiles = CodeFiles;
			if (codeFiles.ContainsKey (path))
				return;
			
			codeFiles.Add (path, true);
			
			string extension = Path.GetExtension (path);
			if (extension == null || extension.Length == 0)
				return; // maybe better to throw an exception here?
			extension = extension.Substring (1);
			string filename = GetTempFilePhysicalPath (extension);
			ICodePragmaGenerator pragmaGenerator;
			
			switch (extension.ToLowerInvariant ()) {
				case "cs":
					pragmaGenerator = new CSharpCodePragmaGenerator ();
					break;

				case "vb":
					pragmaGenerator = new VBCodePragmaGenerator ();
					break;

				default:
					pragmaGenerator = null;
					break;
			}
			
			if (isVirtual) {
				VirtualFile vf = HostingEnvironment.VirtualPathProvider.GetFile (path);
				if (vf == null)
					throw new HttpException (404, "Virtual file '" + path + "' does not exist.");

				if (vf is DefaultVirtualFile)
					path = HostingEnvironment.MapPath (path);
				CopyFileWithChecksum (vf.Open (), filename, path, pragmaGenerator);
			} else
				CopyFileWithChecksum (path, filename, path, pragmaGenerator);

			if (pragmaGenerator != null) {
				if (bp != null)
					AddPathToBuilderMap (filename, bp);
			
				SourceFiles.Add (filename);
			}
		}

		void CopyFileWithChecksum (string input, string to, string from, ICodePragmaGenerator pragmaGenerator)
		{
			CopyFileWithChecksum (new FileStream (input, FileMode.Open, FileAccess.Read), to, from, pragmaGenerator);
		}
		
		void CopyFileWithChecksum (Stream input, string to, string from, ICodePragmaGenerator pragmaGenerator)
		{
			if (pragmaGenerator == null) {
				// This is BAD, BAD, BAD! CodeDOM API is really no good in this
				// instance.
				string filedata;
				using (StreamReader sr = new StreamReader (input, WebEncoding.FileEncoding)) {
					filedata = sr.ReadToEnd ();
				}

				var snippet = new CodeSnippetCompileUnit (filedata);
				snippet.LinePragma = new CodeLinePragma (from, 1);
				filedata = null;
				AddCodeCompileUnit (snippet);
				snippet = null;
				
				return;
			}
			
			MD5 checksum = MD5.Create ();
			using (FileStream fs = new FileStream (to, FileMode.Create, FileAccess.Write)) {
				using (StreamWriter sw = new StreamWriter (fs, Encoding.UTF8)) {
					using (StreamReader sr = new StreamReader (input, WebEncoding.FileEncoding)) {
						int count = pragmaGenerator.ReserveSpace (from);
						char[] src;
						
						if (count > COPY_BUFFER_SIZE)
							src = new char [count];
						else
							src = new char [COPY_BUFFER_SIZE];

						sw.Write (src, 0, count);
						do {
							count = sr.Read (src, 0, COPY_BUFFER_SIZE);
							if (count == 0) {
								UpdateChecksum (src, 0, checksum, true);
								break;
							}
						
							sw.Write (src, 0, count);
							UpdateChecksum (src, count, checksum, false);
						} while (true);
						src = null;
					}
				}
			}
			pragmaGenerator.DecorateFile (to, from, checksum, Encoding.UTF8);
		}

		void UpdateChecksum (char[] buf, int count, MD5 checksum, bool final)
		{
			byte[] input = Encoding.UTF8.GetBytes (buf, 0, count);

			if (final)
				checksum.TransformFinalBlock (input, 0, input.Length);
			else
				checksum.TransformBlock (input, 0, input.Length, input, 0);
			input = null;
		}
		
		public Stream CreateEmbeddedResource (BuildProvider buildProvider, string name)
		{
			if (buildProvider == null)
				throw new ArgumentNullException ("buildProvider");

			if (name == null || name == "")
				throw new ArgumentNullException ("name");

			string filename = GetTempFilePhysicalPath ("resource");
			Stream stream = File.OpenWrite (filename);
			ResourceFiles [name] = filename;
			return stream;
		}

		[MonoTODO ("Not implemented, does nothing")]
		public void GenerateTypeFactory (string typeName)
		{
			// Do nothing by now.
		}

		public string GetTempFilePhysicalPath (string extension)
		{
			if (extension == null)
				throw new ArgumentNullException ("extension");

			string newFileName = OutputAssemblyPrefix + "_" + temp_files.Count + "." + extension;
			temp_files.AddFile (newFileName, KeepFiles);

			return newFileName;
		}

		public CodeDomProvider CodeDomProvider {
			get { return provider; }
		}

		List <Assembly> ReferencedAssemblies {
			get {
				if (referenced_assemblies == null)
					referenced_assemblies = new List <Assembly> ();

				return referenced_assemblies;
			}
		}
		
		CodeUnit CheckForPartialTypes (CodeUnit codeUnit)
		{
			CodeTypeDeclarationCollection types;
			CompileUnitPartialType partialType;
			string partialTypeName;
			List <CompileUnitPartialType> tmp;
			Dictionary <string, List <CompileUnitPartialType>> partialTypes = PartialTypes;
			
			foreach (CodeNamespace ns in codeUnit.Unit.Namespaces) {
				if (ns == null)
					continue;
				types = ns.Types;
				if (types == null || types.Count == 0)
					continue;

				foreach (CodeTypeDeclaration type in types) {
					if (type == null)
						continue;

					if (type.IsPartial) {
						partialType = new CompileUnitPartialType (codeUnit.Unit, ns, type);
						partialTypeName = partialType.TypeName;
						
						if (!partialTypes.TryGetValue (partialTypeName, out tmp)) {
							tmp = new List <CompileUnitPartialType> (1);
							partialTypes.Add (partialTypeName, tmp);
						}
						tmp.Add (partialType);
					}
				}
			}
						
			return codeUnit;
		}
		
		void ProcessPartialTypes ()
		{
			Dictionary <string, List <CompileUnitPartialType>> partialTypes = PartialTypes;
			if (partialTypes.Count == 0)
				return;
			
			foreach (KeyValuePair <string, List <CompileUnitPartialType>> kvp in partialTypes)
				ProcessType (kvp.Value);
		}

		void ProcessType (List <CompileUnitPartialType> typeList)
		{
			CompileUnitPartialType[] types = new CompileUnitPartialType [typeList.Count];
			int counter = 0;
			
			foreach (CompileUnitPartialType type in typeList) {
				if (counter == 0) {
					types [0] = type;
					counter++;
					continue;
				}

				for (int i = 0; i < counter; i++)
					CompareTypes (types [i], type);
				types [counter++] = type;
			}
		}

		void CompareTypes (CompileUnitPartialType source, CompileUnitPartialType target)
		{
			CodeTypeDeclaration sourceType = source.PartialType;
			CodeTypeMemberCollection targetMembers = target.PartialType.Members;
			List <CodeTypeMember> membersToRemove = new List <CodeTypeMember> ();
			
			foreach (CodeTypeMember member in targetMembers) {
				if (TypeHasMember (sourceType, member))
					membersToRemove.Add (member);
			}

			foreach (CodeTypeMember member in membersToRemove)
				targetMembers.Remove (member);
		}

		bool TypeHasMember (CodeTypeDeclaration type, CodeTypeMember member)
		{
			if (type == null || member == null)
				return false;

			return (FindMemberByName (type, member.Name) != null);
		}

		CodeTypeMember FindMemberByName (CodeTypeDeclaration type, string name)
		{
			foreach (CodeTypeMember m in type.Members) {
				if (m == null || m.Name != name)
					continue;
				return m;
			}

			return null;
		}

		internal CompilerResults BuildAssembly ()
		{
			return BuildAssembly (null, CompilerOptions);
		}
		
		internal CompilerResults BuildAssembly (VirtualPath virtualPath)
		{
			return BuildAssembly (virtualPath, CompilerOptions);
		}
		
		internal CompilerResults BuildAssembly (CompilerParameters options)
		{
			return BuildAssembly (null, options);
		}
		
		internal CompilerResults BuildAssembly (VirtualPath virtualPath, CompilerParameters options)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			options.TempFiles = temp_files;
			if (options.OutputAssembly == null)
				options.OutputAssembly = OutputAssemblyName;

			ProcessPartialTypes ();
			
			CompilerResults results;
			CodeUnit [] units = GetUnitsAsArray ();

			// Since we may have some source files and some code
			// units, we generate code from all of them and then
			// compile the assembly from the set of temporary source
			// files. This also facilates possible debugging for the
			// end user, since they get the code beforehand.
			List <string> files = SourceFiles;
			Dictionary <string, string> resources = ResourceFiles;

			if (units.Length == 0 && files.Count == 0 && resources.Count == 0 && options.EmbeddedResources.Count == 0)
				return null;

			string compilerOptions = options.CompilerOptions;
			if (options.IncludeDebugInformation) {
				if (String.IsNullOrEmpty (compilerOptions))
					compilerOptions = "/d:DEBUG";
				else if (compilerOptions.IndexOf ("d:DEBUG", StringComparison.OrdinalIgnoreCase) == -1)
					compilerOptions += " /d:DEBUG";
				
				options.CompilerOptions = compilerOptions;
			}

			if (String.IsNullOrEmpty (compilerOptions))
				compilerOptions = "/noconfig";
			else if (compilerOptions.IndexOf ("noconfig", StringComparison.OrdinalIgnoreCase) == -1)
				compilerOptions += " /noconfig";
			options.CompilerOptions = compilerOptions;
			
			string filename;
			StreamWriter sw = null;
			
			foreach (CodeUnit unit in units) {
				filename = GetTempFilePhysicalPath (provider.FileExtension);
				try {
					sw = new StreamWriter (File.OpenWrite (filename), Encoding.UTF8);
					provider.GenerateCodeFromCompileUnit (unit.Unit, sw, null);
					files.Add (filename);
				} catch {
					throw;
				} finally {
					if (sw != null) {
						sw.Flush ();
						sw.Close ();
					}
				}

				if (unit.BuildProvider != null)
					AddPathToBuilderMap (filename, unit.BuildProvider);
			}

			foreach (KeyValuePair <string, string> de in resources)
				options.EmbeddedResources.Add (de.Value);

			AddAssemblyReference (BuildManager.GetReferencedAssemblies ());
			foreach (Assembly refasm in ReferencedAssemblies) {
				string path = new Uri (refasm.CodeBase).LocalPath;
				options.ReferencedAssemblies.Add (path);
			}
			
			results = provider.CompileAssemblyFromFile (options, files.ToArray ());

			if (results.NativeCompilerReturnValue != 0) {
				string fileText = null;
				try {
					using (StreamReader sr = File.OpenText (results.Errors [0].FileName)) {
						fileText = sr.ReadToEnd ();
					}
				} catch (Exception) {}
				
#if DEBUG
				Console.WriteLine ("********************************************************************");
				Console.WriteLine ("Compilation failed.");
				Console.WriteLine ("Output:");
				foreach (string s in results.Output)
					Console.WriteLine ("  " + s);
				Console.WriteLine ("\nErrors:");
				foreach (CompilerError err in results.Errors)
					Console.WriteLine (err);
				Console.WriteLine ("File name: {0}", results.Errors [0].FileName);
				Console.WriteLine ("File text:\n{0}\n", fileText);
				Console.WriteLine ("********************************************************************");
#endif
				
				throw new CompilationException (virtualPath != null ? virtualPath.Original : String.Empty, results, fileText);
			}
			
			Assembly assembly = results.CompiledAssembly;
			if (assembly == null) {
				if (!File.Exists (options.OutputAssembly)) {
					results.TempFiles.Delete ();
					throw new CompilationException (virtualPath != null ? virtualPath.Original : String.Empty, results.Errors,
						"No assembly returned after compilation!?");
				}

				try {
					results.CompiledAssembly = Assembly.LoadFrom (options.OutputAssembly);
				} catch (Exception ex) {
					results.TempFiles.Delete ();
					throw new HttpException ("Unable to load compiled assembly", ex);
				}
			}

			if (!KeepFiles)
				results.TempFiles.Delete ();
			return results;
		}
	}
}


