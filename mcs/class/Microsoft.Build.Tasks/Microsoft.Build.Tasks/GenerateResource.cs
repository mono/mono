//
// GenerateResource.cs: Task that generates the resources.
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//   Paolo Molaro (lupus@ximian.com)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2005 Marek Sieradzki
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

#if NET_2_0

using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using System.Reflection;
using Microsoft.Build.Framework;
using Mono.XBuild.Tasks.GenerateResourceInternal;

namespace Microsoft.Build.Tasks {
	public sealed class GenerateResource : TaskExtension {
	
		ITaskItem[]	filesWritten;
		bool		neverLockTypeAssemblies;
		ITaskItem[]	outputResources;
		bool		publicClass;
		ITaskItem[]	references;
		ITaskItem[]	sources;
		ITaskItem	stateFile;
		string		stronglyTypedClassName;
		string		stronglyTypedFilename;
		string		stronglyTypedLanguage;
		string		stronglyTypedNamespace;
		bool		useSourcePath;
		
		public GenerateResource ()
		{
			useSourcePath = false;
		}

		public override bool Execute ()
		{
			if (sources.Length == 0)
				return true;

			List  <ITaskItem> temporaryFilesWritten = new List <ITaskItem> ();
			if (outputResources == null) {
				foreach (ITaskItem source in sources) {
					string sourceFile = source.ItemSpec;
					string outputFile = Path.ChangeExtension (sourceFile, "resources");
					CompileResourceFile (sourceFile, outputFile);
				}
			} else {
				if (sources.Length != outputResources.Length) {
					Log.LogErrorFromException (new Exception ("Sources count is different than OutputResources count."));
					return false;
				}

				for (int i = 0; i < sources.Length; i ++) {
					string sourceFile = sources [i].ItemSpec;
					string outputFile = outputResources [i].ItemSpec;

					if (outputFile == String.Empty) {
						Log.LogErrorFromException (new Exception ("Filename of output can not be empty."));
						return false;
					}
					if (CompileResourceFile (sourceFile, outputFile) == false) {
						Log.LogErrorFromException (new Exception ("Error during compiling resource file."));
						return false;
					}
					temporaryFilesWritten.Add (outputResources [i]);
				}
			}
			
			filesWritten = temporaryFilesWritten.ToArray ();
			
			return true;
		}
		
		private IResourceReader GetReader (Stream stream, string name)
		{
			string format = Path.GetExtension (name);
			switch (format.ToLower ()) {
			case ".po":
				return new PoResourceReader (stream);
			case ".txt":
			case ".text":
				return new TxtResourceReader (stream);
			case ".resources":
				return new ResourceReader (stream);
			case ".resx":
				ResXResourceReader reader = new ResXResourceReader (stream);

				// set correct basepath to resolve relative paths in file refs
				if (useSourcePath)
					reader.BasePath = Path.GetDirectoryName (Path.GetFullPath (name));

				return reader;
			default:
				throw new Exception ("Unknown format in file " + name);
			}
		}
		
		private IResourceWriter GetWriter (Stream stream, string name)
		{
			string format = Path.GetExtension (name);
			switch (format.ToLower ()) {
			case ".po":
				return new PoResourceWriter (stream);
			case ".txt":
			case ".text":
				return new TxtResourceWriter (stream);
			case ".resources":
				return new ResourceWriter (stream);
			case ".resx":
				return new System.Resources.ResXResourceWriter (stream);
			default:
				throw new Exception ("Unknown format in file " + name);
			}
		}
		
		private bool CompileResourceFile (string sname, string dname )
		{
			FileStream source, dest;
			IResourceReader reader;
			IResourceWriter writer;

			Log.LogMessage ("Compiling resource file '{0}' into '{1}'", sname, dname);
			try {
				source = new FileStream (sname, FileMode.Open, FileAccess.Read);

				reader = GetReader (source, sname);

				dest = new FileStream (dname, FileMode.Create, FileAccess.Write);
				writer = GetWriter (dest, dname);

				int rescount = 0;
				foreach (DictionaryEntry e in reader) {
					rescount++;
					object val = e.Value;
					if (val is string)
						writer.AddResource ((string)e.Key, (string)e.Value);
					else
						writer.AddResource ((string)e.Key, e.Value);
				}

				reader.Close ();
				writer.Close ();
			} catch (Exception e) {
				Log.LogErrorFromException (e);
				return false;
			}
			return true;
		}

		[Output]
		public ITaskItem[] FilesWritten {
			get {
				return filesWritten;
			}
		}

		[MonoTODO]
		public bool NeverLockTypeAssemblies {
			get {
				return neverLockTypeAssemblies;
			}
			set {
				neverLockTypeAssemblies = value;
			}
		}

		[Output]
		public ITaskItem[] OutputResources {
			get {
				return outputResources;
			}
			set {
				outputResources = value;
			}
		}
		
		public bool PublicClass {
			get { return publicClass; }
			set { publicClass = value; }
		}

		public ITaskItem[] References {
			get {
				return references;
			}
			set {
				references = value;
			}
		}

		[Required]
		public ITaskItem[] Sources {
			get {
				return sources;
			}
			set {
				sources = value;
			}
		}

		public ITaskItem StateFile {
			get {
				return stateFile;
			}
			set {
				stateFile = value;
			}
		}

		[Output]
		public string StronglyTypedClassName {
			get {
				return stronglyTypedClassName;
			}
			set {
				stronglyTypedClassName = value;
			}
		}

		[Output]
		public string StronglyTypedFileName {
			get {
				return stronglyTypedFilename;
			}
			set {
				stronglyTypedFilename = value;
			}
		}

		public string StronglyTypedLanguage {
			get {
				return stronglyTypedLanguage;
			}
			set {
				stronglyTypedLanguage = value;
			}
		}

		public string StronglyTypedNamespace {
			get {
				return stronglyTypedNamespace;
			}
			set {
				stronglyTypedNamespace = value;
			}
		}

		public bool UseSourcePath {
			get {
				return useSourcePath;
			}
			set {
				useSourcePath = value;
			}
		}
	}
}

#endif
