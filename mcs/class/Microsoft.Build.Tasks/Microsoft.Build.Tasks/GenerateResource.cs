//
// GenerateResource.cs: Task that generates the resources.
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//   Paolo Molaro (lupus@ximian.com)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//   Lluis Sanchez Gual <lluis@novell.com>
//   Ankit Jain <jankit@novell.com>
//
// (C) 2005 Marek Sieradzki
// Copyright 2010 Novell, Inc (http://www.novell.com)
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
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.XBuild.Tasks.GenerateResourceInternal;
using Mono.XBuild.Utilities;

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

			bool result = true;
			List  <ITaskItem> temporaryFilesWritten = new List <ITaskItem> ();
			if (outputResources == null) {
				foreach (ITaskItem source in sources) {
					string sourceFile = source.ItemSpec;
					string outputFile = Path.ChangeExtension (sourceFile, "resources");

					if (IsResgenRequired (sourceFile, outputFile))
						result &= CompileResourceFile (sourceFile, outputFile);

					ITaskItem newItem = new TaskItem (source);
					source.ItemSpec = outputFile;

					temporaryFilesWritten.Add (newItem);
				}
			} else {
				if (sources.Length != outputResources.Length) {
					Log.LogError ("Sources count is different than OutputResources count.");
					return false;
				}

				for (int i = 0; i < sources.Length; i ++) {
					if (String.IsNullOrEmpty (outputResources [i].ItemSpec)) {
						Log.LogError ("Filename of output can not be empty.");
						result = false;
						continue;
					}

					if (IsResgenRequired (sources [i].ItemSpec, outputResources [i].ItemSpec))
						result &= CompileResourceFile (sources [i].ItemSpec, outputResources [i].ItemSpec);
					temporaryFilesWritten.Add (outputResources [i]);
				}
			}
			
			filesWritten = temporaryFilesWritten.ToArray ();

			return result;
		}
		
		// true if the resx file or any file referenced
		// by the resx is newer than the .resources file
		//
		// Code taken from monodevelop
		// main/src/core/MonoDevelop.Core/MonoDevelop.Projects.Formats.MD1/MD1DotNetProjectHandler.cs
		bool IsResgenRequired (string resx_filename, string resources_filename)
		{
			if (IsFileNewerThan (resx_filename, resources_filename)) {
				Log.LogMessage (MessageImportance.Low,
						"Resource file '{0}' is newer than the source file '{1}', skipping.",
						resources_filename, resx_filename);
				return true;
			}

			if (String.Compare (Path.GetExtension (resx_filename), ".resx", true) != 0)
				return true;

			// resx file, check for files referenced from there
			XmlTextReader xr = null;
			try {
				// look for
				// <data type="System.Resources.ResXFileRef, System.Windows.Forms" ..>
				//   <value>... filename;.. </value>
				// </data>
				xr = new XmlTextReader (resx_filename);
				string basepath = Path.GetDirectoryName (resx_filename);
				while (xr.Read ()) {
					if (xr.NodeType != XmlNodeType.Element ||
						String.Compare (xr.LocalName, "data") != 0)
						continue;

					string type = xr.GetAttribute ("type");
					if (String.IsNullOrEmpty (type))
						continue;

					if (String.Compare (type, "System.Resources.ResXFileRef, System.Windows.Forms") != 0)
						continue;

					xr.ReadToDescendant ("value");
					if (xr.NodeType != XmlNodeType.Element)
						continue;

					string value = xr.ReadElementContentAsString ();

					string [] parts = value.Split (';');
					if (parts.Length > 0) {
						string referenced_filename = MSBuildUtils.FromMSBuildPath (
								Path.Combine (basepath, parts [0]).Trim ());
						if (File.Exists (referenced_filename) &&
							IsFileNewerThan (referenced_filename, resources_filename))
							return true;
					}
				}
			} catch (XmlException) {
				// Ignore xml errors, let resgen handle it
				return true;
			} finally {
				if (xr != null)
					xr.Close ();
			}

			return false;
		}

		// true if first is newer than second
		static bool IsFileNewerThan (string first, string second)
		{
			FileInfo finfo_first = new FileInfo (first);
			FileInfo finfo_second = new FileInfo (second);
			return finfo_first.LastWriteTime > finfo_second.LastWriteTime;
		}

#if false
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
#endif
		
		private bool CompileResourceFile (string sname, string dname )
		{
			if (!File.Exists (sname)) {
				Log.LogError ("Resource file '{0}' not found.", sname);
				return false;
			}

			Resgen resgen = new Resgen ();
			resgen.BuildEngine = this.BuildEngine;
			resgen.UseSourcePath = true;

			resgen.SourceFile = sname;
			resgen.OutputFile = dname;

			return resgen.Execute ();
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

	class Resgen : ToolTaskExtension
	{
		public Resgen ()
		{
		}

		protected internal override void AddCommandLineCommands (
						 CommandLineBuilderExtension commandLine)
		{
			if (UseSourcePath)
				commandLine.AppendSwitch ("/useSourcePath");

			commandLine.AppendSwitch (String.Format ("/compile \"{0}{1}\"", SourceFile,
						OutputFile != null ? "," + OutputFile : ""));
		}

		public override bool Execute ()
		{
			if (String.IsNullOrEmpty (Environment.GetEnvironmentVariable ("MONO_IOMAP")))
				EnvironmentVariables = new string [] { "MONO_IOMAP=drive" };
			return base.Execute ();
		}

		protected override string GenerateFullPathToTool ()
		{
			return Path.Combine (ToolPath, ToolExe);
		}

		protected override MessageImportance StandardOutputLoggingImportance {
			get { return MessageImportance.Low; }
		}

		protected override string ToolName {
			get { return MSBuildUtils.RunningOnWindows ? "resgen2.bat" : "resgen2"; }
		}

		public string SourceFile { get; set; }
		public string OutputFile { get; set; }

		public bool UseSourcePath { get; set; }
	}
}

#endif
